using System.Reflection;
using MrKWatkins.DocGen.Markdown.Writing;
using MrKWatkins.DocGen.Model;
using MrKWatkins.DocGen.XmlDocumentation;
using MrKWatkins.Reflection;
using MrKWatkins.Reflection.Formatting;

namespace MrKWatkins.DocGen.Markdown.Generation;

public abstract class MarkdownGenerator
{
    private static readonly IReflectionFormatter MemberLinkFormatter =
        new CachedReflectionFormatter(new DisplayNameFormatter(new DisplayNameFormatterOptions { PrefixMembersWithType = false }));

    protected MarkdownGenerator(MemberLookup memberLookup, string outputDirectory)
    {
        MemberLookup = memberLookup;
        OutputDirectory = outputDirectory;
    }

    public abstract void Generate(OutputNode node);

    protected MemberLookup MemberLookup { get; }

    protected string OutputDirectory { get; }

    [MustUseReturnValue]
    protected MarkdownWriter CreateWriter(OutputNode node)
    {
        var filePath = Path.Combine(OutputDirectory, node.FileName);
        return new MarkdownWriter(filePath);
    }

    protected void WriteTypeParameters(MarkdownWriter writer, DocumentableNode member, IReadOnlyList<TypeParameter> typeParameters)
    {
        if (typeParameters.Count == 0)
        {
            return;
        }

        writer.WriteSubSubHeading("Type Parameters");

        using var table = writer.Table("Name", "Description");

        foreach (var typeParameter in typeParameters)
        {
            table.NewRow();
            table.Write(typeParameter.Name);
            table.NewColumn();
            if (member.Documentation?.TypeParameters.TryGetValue(typeParameter.Name, out var summary) == true)
            {
                WriteSection(table, summary);
            }
        }
    }

    protected static void WriteSignatureParameters(ITextWriter code, IReadOnlyList<Parameter> parameters)
    {
        foreach (var parameter in parameters)
        {
            if (parameter != parameters[0])
            {
                code.Write(", ");
            }

            if (parameter.Kind.HasValue)
            {
                code.Write(parameter.Kind.Value.ToKeyword());
                code.Write(" ");
            }

            code.Write(parameter.Type.DisplayNameOrKeyword());
            code.Write(" ");

            if (CSharp.Keywords.Contains(parameter.Name))
            {
                code.Write("@");
            }
            code.Write(parameter.Name);

            if (parameter.HasDefaultValue)
            {
                code.Write(" = ");
                WriteSignatureValue(code, parameter.DefaultValue);
            }
        }
    }

    private static void WriteSignatureValue(ITextWriter code, object? defaultValue)
    {
        if (defaultValue == null)
        {
            code.Write("null");
            return;
        }

        if (defaultValue is bool defaultBool)
        {
            code.Write(defaultBool ? "true" : "false");
            return;
        }

        var type = defaultValue.GetType();
        if (type.IsEnum)
        {
            code.Write(type.ToDisplayName());
            code.Write(".");
            code.Write(type.ToString());
            return;
        }

        code.Write(defaultValue.ToString() ?? "");
    }

    protected static void WriteSignatureTypeParameters(ITextWriter code, [InstantHandle] IReadOnlyList<System.Type> genericArguments)
    {
        if (genericArguments.Count == 0)
        {
            return;
        }

        code.Write("<");
        foreach (var genericArgument in genericArguments)
        {
            if (genericArgument != genericArguments[0])
            {
                code.Write(", ");
            }

            code.Write(genericArgument.Name);
        }
        code.Write(">");
    }

    protected static void WriteSignatureTypeConstraints(ITextWriter code, IReadOnlyList<System.Type> genericArguments)
    {
        foreach (var genericArgument in genericArguments)
        {
            var constraints = genericArgument.GetGenericParameterConstraints();
            var attributes = genericArgument.GenericParameterAttributes;

            if (constraints.Length == 0 && attributes == GenericParameterAttributes.None)
            {
                continue;
            }

            var hasStructConstraint = constraints.Any(c => c == typeof(ValueType));

            code.WriteLine();
            code.Write("   where ");
            code.Write(genericArgument.Name);

            var separator = " : ";

            void WriteConstraint(string constraint)
            {
                code.Write(separator);
                code.Write(constraint);
                separator = ", ";
            }

            if (attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
            {
                WriteConstraint("class");
            }
            else if (hasStructConstraint)
            {
                WriteConstraint("struct");
            }
            else if (attributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
            {
                WriteConstraint("notnull");
            }

            foreach (var constraint in constraints.Where(c => c != typeof(ValueType)))
            {
                WriteConstraint(constraint.ToDisplayName());
            }

            if (!hasStructConstraint && attributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
            {
                WriteConstraint("new()");
            }
        }
    }

    protected static void WriteAccessibility(ITextWriter code, Accessibility accessibility)
    {
        switch (accessibility)
        {
            case Accessibility.Private:
            case Accessibility.PrivateProtected:
            case Accessibility.Internal:
                throw new InvalidOperationException($"Should not generate documentation for {accessibility} members.");

            case Accessibility.Protected:
            case Accessibility.ProtectedInternal:
                code.Write("protected");
                break;
            case Accessibility.Public:
                code.Write("public");
                break;
        }
    }

    protected void WriteRemarks(MarkdownWriter writer, MemberDocumentation? documentation)
    {
        if (documentation?.Remarks == null)
        {
            return;
        }

        writer.WriteSubHeading("Remarks");

        using var paragraph = writer.Paragraph();
        WriteSection(paragraph, documentation.Remarks);
    }

    protected void WriteSection(MarkdownWriter writer, DocumentationSection? section)
    {
        if (section == null)
        {
            return;
        }

        using var paragraph = writer.Paragraph();
        WriteSection(paragraph, section);
    }

    protected void WriteSection(IParagraphWriter writer, DocumentationSection? section)
    {
        if (section == null)
        {
            return;
        }

        foreach (var element in section.Elements)
        {
            switch (element)
            {
                case CodeElement codeElement:
                    writer.WriteCode(codeElement.Code);
                    break;
                case ParamRef paramRef:
                    writer.WriteCode(paramRef.Name);
                    break;
                case See see:
                    WriteSee(writer, see);
                    break;
                case TypeParamRef typeParamRef:
                    writer.WriteCode(typeParamRef.Name);
                    break;
                case TextElement textElement:
                    writer.Write(textElement.Text);
                    break;
                default:
                    throw new NotSupportedException($"The {nameof(DocumentationElement)} {element.GetType()} is not supported.");
            }
        }
    }

    private void WriteSee(IParagraphWriter writer, See see)
    {
        var (member, location) = MemberLookup.Get(see.Id);
        WriteMemberLink(writer, member, location, see.Text);
    }

    protected void WriteMemberLink(IParagraphWriter writer, MemberInfo member, string? text = null) =>
        WriteMemberLink(writer, member, MemberLookup.GetLocation(member), text);

    protected static void WriteMemberLink(IParagraphWriter writer, MemberInfo member, MemberLocation location, string? text = null)
    {
        text ??= MemberLinkFormatter.Format(member);

        if (member is System.Type type && (type.IsArray || type.IsByRef))
        {
            WriteMemberLink(writer, type.GetElementType()!, location, text);
            return;
        }

        if (member is System.Type { IsGenericParameter: true })
        {
            writer.Write(text);
            return;
        }

        switch (location)
        {
            case MemberLocation.DocumentAssembly:
                writer.WriteLink(text, member.DocumentationLink());
                break;
            case MemberLocation.System:
                writer.WriteLink(text, member.MicrosoftLink());
                break;
            default:
                writer.Write(text);
                break;
        }
    }
}