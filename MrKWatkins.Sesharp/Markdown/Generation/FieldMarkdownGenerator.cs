using System.Globalization;
using System.Reflection;
using MrKWatkins.Sesharp.Markdown.Writing;
using MrKWatkins.Sesharp.Model;
using MrKWatkins.Reflection;

namespace MrKWatkins.Sesharp.Markdown.Generation;

public sealed class FieldMarkdownGenerator(IFileSystem fileSystem, MemberLookup memberLookup, string outputDirectory) : MemberMarkdownGenerator<Field, FieldInfo>(fileSystem, memberLookup, outputDirectory)
{
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
        WriteSourceLink(writer, field);

        WriteReturns(writer, field, field.MemberInfo.FieldType);

        WriteRemarks(writer, field.Documentation);
    }

    private static void WriteSignature(MarkdownWriter writer, Field field)
    {
        using var code = writer.CodeBlock();

        code.Write(field.Accessibility.ToCSharpKeywords());
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

        WriteTypeOrKeyword(code, field.MemberInfo.FieldType);
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
            case byte value:
                code.Write(value.ToString(DateTimeFormatInfo.InvariantInfo));
                break;

            case sbyte value:
                code.Write(value.ToString(DateTimeFormatInfo.InvariantInfo));
                break;

            case short value:
                code.Write(value.ToString(DateTimeFormatInfo.InvariantInfo));
                break;

            case ushort value:
                code.Write(value.ToString(DateTimeFormatInfo.InvariantInfo));
                break;

            case int value:
                code.Write(value.ToString(DateTimeFormatInfo.InvariantInfo));
                break;

            case uint value:
                code.Write(value.ToString(DateTimeFormatInfo.InvariantInfo));
                break;

            case long value:
                code.Write(value.ToString(DateTimeFormatInfo.InvariantInfo));
                break;

            case ulong value:
                code.Write(value.ToString(DateTimeFormatInfo.InvariantInfo));
                break;

            case double value:
                code.Write(double.IsNaN(value) ? "double.NaN" :
                    double.IsPositiveInfinity(value) ? "double.PositiveInfinity" :
                    double.IsNegativeInfinity(value) ? "double.NegativeInfinity" :
                    value == 0 && double.IsNegative(value) ? "-0.0" : value.ToString("R", CultureInfo.InvariantCulture));
                break;

            case float value:
                code.Write(float.IsNaN(value) ? "float.NaN" :
                    float.IsPositiveInfinity(value) ? "float.PositiveInfinity" :
                    float.IsNegativeInfinity(value) ? "float.NegativeInfinity" :
                    value.ToString("R", CultureInfo.InvariantCulture) + "f");
                break;

            case string value:
                code.Write("\"");
                code.Write(value);
                code.Write("\"");
                break;

            case Enum value:
                var enumType = value.GetType();
                var typeName = enumType.ToDisplayName();
                var name = Enum.GetName(enumType, value);
                if (name != null)
                {
                    code.Write(typeName + "." + name);
                }
                else
                {
                    var formatted = value.ToString(enumType.IsDefined(typeof(FlagsAttribute)) ? "F" : "D");
                    code.Write(char.IsDigit(formatted[0]) || formatted[0] == '-'
                        ? "(" + typeName + ")" + formatted
                        : string.Join(" | ", formatted.Split(", ").Select(flag => typeName + "." + flag)));
                }
                break;

            default:
                throw new NotSupportedException($"Literals of type {literal.GetType().ToDisplayName()} are not supported.");
        }
    }

}