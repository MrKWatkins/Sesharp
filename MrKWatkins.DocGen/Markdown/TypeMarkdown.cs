using System.Reflection;
using MrKWatkins.DocGen.XmlDocumentation;

namespace MrKWatkins.DocGen.Markdown;

public static class TypeMarkdown
{
    public static void Generate(string namespaceDirectory, Model.Type type)
    {
        var filePath = Path.Combine(namespaceDirectory, type.FileName);
        using var writer = new MarkdownWriter(filePath + ".md");

        writer.WriteMainHeading($"{type.DisplayName} {type.Kind.Capitalize()}");

        if (type.Documentation?.Summary != null)
        {
            WriteSection(writer, type.Documentation.Summary);
        }

        WriteSignature(writer, type);
    }

    private static void WriteSection(MarkdownWriter writer, DocumentationSection section)
    {
        using var paragraph = writer.Paragraph();
        WriteSection(paragraph, section);
    }

    private static void WriteSection(IParagraphWriter writer, DocumentationSection section)
    {
        foreach (var element in section.Elements)
        {
            switch (element)
            {
                case CodeElement codeElement:
                    writer.WriteCode(codeElement.Code);
                    break;
                case ParamRef paramRef:
                    break;
                case See see:
                    writer.WriteLink(see.Text, see.Key);
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

            writer.Write(" ");
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