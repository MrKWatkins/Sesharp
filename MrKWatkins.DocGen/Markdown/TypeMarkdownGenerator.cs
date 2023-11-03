using System.Reflection;
using MrKWatkins.DocGen.XmlDocumentation;

namespace MrKWatkins.DocGen.Markdown;

public static class TypeMarkdownGenerator
{
    public static void Generate(TypeLookup typeLookup, string namespaceDirectory, Model.Type type)
    {
        var filePath = Path.Combine(namespaceDirectory, type.FileName);
        using var writer = new MarkdownWriter(filePath);

        writer.WriteMainHeading($"{type.DisplayName} {type.Kind.Capitalize()}");

        if (type.Documentation?.Summary != null)
        {
            WriteSection(typeLookup, writer, type.Documentation.Summary);
        }

        WriteSignature(writer, type);

        WriteTypeParameters(typeLookup, writer, type);
    }

    private static void WriteTypeParameters(TypeLookup typeLookup, MarkdownWriter writer, Model.Type type)
    {
        var typeParameters = type.TypeParameters.ToList();
        if (typeParameters.Count == 0)
        {
            return;
        }

        using var table = writer.Table("Parameter", "Description");

        foreach (var typeParameter in typeParameters)
        {
            table.NewRow();
            table.Write(typeParameter.Name);
            table.NewColumn();
            if (type.Documentation?.TypeParameters.TryGetValue(typeParameter.Name, out var summary) == true)
            {
                WriteSection(typeLookup, table, summary);
            }
        }
    }

    private static void WriteSection(TypeLookup typeLookup, MarkdownWriter writer, DocumentationSection section)
    {
        using var paragraph = writer.Paragraph();
        WriteSection(typeLookup, paragraph, section);
    }

    private static void WriteSection(TypeLookup typeLookup, IParagraphWriter writer, DocumentationSection section)
    {
        var isFirst = true;
        foreach (var element in section.Elements)
        {
            if (!isFirst)
            {
                writer.Write(" ");
                isFirst = false;
            }

            switch (element)
            {
                case CodeElement codeElement:
                    writer.WriteCode(codeElement.Code);
                    break;
                case ParamRef paramRef:
                    break;
                case See see:
                    WriteSee(typeLookup, writer, see);
                    break;
                case TypeParamRef typeParamRef:
                    break;
                case ReferenceElement referenceElement:
                    break;
                case TextElement textElement:
                    writer.Write(textElement.Text);
                    break;
                default:
                    throw new NotSupportedException($"The {nameof(DocumentationElement)} {element.GetType()} is not supported.");
            }
        }
    }

    private static void WriteSee(TypeLookup typeLookup, IParagraphWriter writer, See see)
    {
        var text = see.Text;
        if (text != null)
        {
            writer.WriteLink(text, see.Key);
            return;
        }

        var reference = MemberReference.Parse(typeLookup, see.Key);
        switch (reference.Location)
        {
            case TypeLocation.DocumentAssembly:
                writer.WriteLink(reference.Type.DisplayName(), reference.Type.DocumentationFileName());
                break;
            case TypeLocation.System:
                writer.WriteLink(reference.Type.DisplayName(), reference.Type.MicrosoftFileName());
                break;
            default:
                writer.Write(reference.Type.DisplayName());
                break;
        }
    }

    private static void WriteSignature(MarkdownWriter writer, Model.Type type)
    {
        using var code = writer.CodeBlock();
        code.Write("public ");
        if (type.MemberInfo.IsAbstract)
        {
            code.Write("abstract ");
        }
        if (type.MemberInfo.IsSealed)
        {
            code.Write("sealed ");
        }

        code.Write(type.Kind);
        code.Write(" ");
        code.Write(type.DisplayName);

        var hasBaseType = type.MemberInfo.IsClass && type.MemberInfo.BaseType != typeof(object);
        if (hasBaseType)
        {
            code.Write(" : ");
            code.Write(type.MemberInfo.BaseType!.DisplayName());
        }

        var separator = hasBaseType ? ", " : " : ";
        foreach (var @interface in type.MemberInfo.GetInterfaces())
        {
            code.Write(separator);
            code.Write(@interface.DisplayName());
            separator = ", ";
        }

        WriteGenericTypeConstraints(code, type.MemberInfo.GetGenericArguments());
    }

    private static void WriteGenericTypeConstraints(ITextWriter code, [InstantHandle] IEnumerable<Type> genericArguments)
    {
        foreach (var genericArgument in genericArguments)
        {
            code.WriteLine();
            code.Write("   where ");
            code.Write(genericArgument.Name);

            var separator = " : ";
            foreach (var constraint in genericArgument.GetGenericParameterConstraints())
            {
                code.Write(separator);
                code.Write(constraint.DisplayName());
                separator = ", ";
            }

            var attributes = genericArgument.GenericParameterAttributes;
            if (attributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
            {
                code.Write(separator);
                code.Write("new()");
            }
        }
    }
}