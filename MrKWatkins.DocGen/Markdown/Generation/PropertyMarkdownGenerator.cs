using System.Reflection;
using MrKWatkins.DocGen.Markdown.Writing;
using MrKWatkins.DocGen.Model;

namespace MrKWatkins.DocGen.Markdown.Generation;

public sealed class PropertyMarkdownGenerator : MemberMarkdownGenerator<Property, PropertyInfo>
{
    public PropertyMarkdownGenerator(MemberLookup memberLookup, string outputDirectory)
        : base(memberLookup, outputDirectory)
    {
    }

    protected override void Generate(MarkdownWriter writer, Property property)
    {
        writer.WriteMainHeading($"{property.Type.DisplayName}.{property.MemberName} Property");

        writer.WriteSubHeading("Definition");

        WriteSection(writer, property.Documentation?.Summary);

        WriteSignature(writer, property);

        WriteParameters(writer, property, property.IndexParameters);

        WriteReturns(writer, property, property.MemberInfo.PropertyType, "Property Value");

        WriteRemarks(writer, property.Documentation);
    }

    private static void WriteSignature(MarkdownWriter writer, Property property)
    {
        using var code = writer.CodeBlock();

        var getter = property.Getter;
        var setter = property.Setter;

        code.Write(property.Visibility.ToKeyword());
        code.Write(" ");

        if (property.IsStatic)
        {
            code.Write("static ");
        }

        if (property.Virtuality.HasValue)
        {
            code.Write(property.Virtuality.Value.ToKeyword());
            code.Write(" ");
        }

        if (property.IsRequired)
        {
            code.Write("required ");
        }

        code.Write(property.MemberInfo.PropertyType.DisplayNameOrKeyword());
        code.Write(" ");

        code.Write(property.Name);

        WriteIndexerParameters(code, property);

        code.Write(" { ");
        WritePropertyAccessorSignature(code, property, getter, "get");
        WritePropertyAccessorSignature(code, property, setter, property.HasInitSetter ? "init" : "set");
        code.Write("}");
    }

    private static void WriteIndexerParameters(ITextWriter code, Property property)
    {
        var parameters = property.IndexParameters;
        if (parameters.Count == 0)
        {
            return;
        }

        code.Write("[");
        WriteSignatureParameters(code, parameters);
        code.Write("]");
    }

    private static void WritePropertyAccessorSignature(ITextWriter code, Property property, MethodInfo? accessor, string kind)
    {
        var accessorVisibility = accessor?.GetVisibility();
        if (accessorVisibility == null)
        {
            return;
        }

        if (accessorVisibility != property.Visibility)
        {
            code.Write(accessorVisibility.Value.ToKeyword());
            code.Write(" ");
        }

        code.Write(kind);
        code.Write("; ");
    }
}