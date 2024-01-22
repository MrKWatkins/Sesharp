using System.Globalization;
using System.Reflection;
using MrKWatkins.DocGen.Markdown.Writing;
using MrKWatkins.DocGen.Model;

namespace MrKWatkins.DocGen.Markdown.Generation;

public sealed class FieldMarkdownGenerator : MemberMarkdownGenerator<Field, FieldInfo>
{
    public FieldMarkdownGenerator(MemberLookup memberLookup, string outputDirectory)
        : base(memberLookup, outputDirectory)
    {
    }

    protected override bool ShouldGenerate(Field field) => !field.Type.MemberInfo.IsEnum;

    protected override void Generate(MarkdownWriter writer, Field field)
    {
        if (field.Type.MemberInfo.IsEnum)
        {
            throw new InvalidOperationException("Field pages are not supported for enums.");
        }

        writer.WriteMainHeading($"{field.Type.DisplayName}.{field.DisplayName} Field");

        WriteSection(writer, field.Documentation?.Summary);

        WriteSignature(writer, field);

        WriteReturns(writer, field, field.MemberInfo.FieldType);

        WriteRemarks(writer, field.Documentation);
    }

    private static void WriteSignature(MarkdownWriter writer, Field field)
    {
        using var code = writer.CodeBlock();

        code.Write(field.Visibility.ToKeyword());
        code.Write(" ");

        if (field.IsConst)
        {
            code.Write("const ");
        }
        else
        {
            if (field.IsStatic)
            {
                code.Write("static ");
            }
            if (field.IsReadOnly)
            {
                code.Write("readonly ");
            }
        }

        code.Write(field.MemberInfo.FieldType.DisplayNameOrKeyword());
        code.Write(" ");
        code.Write(field.Name);

        if (field.IsConst)
        {
            var value = field.MemberInfo.GetValue(null);
            if (value != null)
            {
                WriteLiteralValue(code, value);
            }
        }

        code.Write(";");
    }

    private static void WriteLiteralValue(ITextWriter code, object literal)
    {
        code.Write(" = ");
        switch (literal)
        {
            case int value:
                code.Write(value.ToString(DateTimeFormatInfo.InvariantInfo));
                break;

            case string value:
                code.Write("\"");
                code.Write(value);
                code.Write("\"");
                break;

            default:
                throw new NotSupportedException($"Literals of type {literal.GetType().DisplayName()} are not supported.");
        }
    }
}