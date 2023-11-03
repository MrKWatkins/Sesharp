using System.Reflection;
using MrKWatkins.DocGen.XmlDocumentation;

namespace MrKWatkins.DocGen.Markdown;

public static class TypeMarkdownGenerator
{
    public static void Generate(TypeLookup typeLookup, string namespaceDirectory, Model.Type type)
    {
        var filePath = Path.Combine(namespaceDirectory, type.FileName);
        using var writer = new MarkdownWriter(filePath);

        // TODO: Namespace.
        // TODO: Source code.

        writer.WriteMainHeading($"{type.DisplayName} {type.Kind.Capitalize()}");

        if (type.Documentation?.Summary != null)
        {
            WriteSection(typeLookup, writer, type.Documentation.Summary);
        }

        WriteSignature(writer, type);

        WriteTypeParameters(typeLookup, writer, type);

        WriteConstructors(typeLookup, writer, type);

        WriteFields(typeLookup, writer, type);

        WriteProperties(typeLookup, writer, type);

        WriteMethods(typeLookup, writer, type);

        WriteEvents(typeLookup, writer, type);
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

    // TODO: One method that is generic over the type of member. Exception for constructor due to skipping single parameterless constructor.
    private static void WriteConstructors(TypeLookup typeLookup, MarkdownWriter writer, Model.Type type)
    {
        var constructors = type.Constructors.ToList();
        if (constructors.Count == 0 ||
            // TODO: Add parameters to function.
            // TODO: Only skip if no documentation?
            (constructors.Count == 1 && constructors[0].MemberInfo.GetParameters().Length == 0))
        {
            return;
        }

        writer.WriteSubHeading("Constructors");
    }

    private static void WriteFields(TypeLookup typeLookup, MarkdownWriter writer, Model.Type type)
    {
        var fields = type.Fields.ToList();
        if (fields.Count == 0)
        {
            return;
        }

        writer.WriteSubHeading("Fields");
    }

    private static void WriteProperties(TypeLookup typeLookup, MarkdownWriter writer, Model.Type type)
    {
        var properties = type.Properties.ToList();
        if (properties.Count == 0)
        {
            return;
        }

        writer.WriteSubHeading("Properties");
    }

    private static void WriteMethods(TypeLookup typeLookup, MarkdownWriter writer, Model.Type type)
    {
        var methods = type.Methods.ToList();
        if (methods.Count == 0)
        {
            return;
        }

        writer.WriteSubHeading("Methods");
    }

    private static void WriteEvents(TypeLookup typeLookup, MarkdownWriter writer, Model.Type type)
    {
        var events = type.Events.ToList();
        if (events.Count == 0)
        {
            return;
        }

        writer.WriteSubHeading("Events");
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