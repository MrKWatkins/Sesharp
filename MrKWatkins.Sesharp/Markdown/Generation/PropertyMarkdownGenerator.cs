using System.Reflection;
using MrKWatkins.Sesharp.Markdown.Writing;
using MrKWatkins.Sesharp.Model;
using MrKWatkins.Reflection;

namespace MrKWatkins.Sesharp.Markdown.Generation;

public sealed class PropertyMarkdownGenerator(IFileSystem fileSystem, MemberLookup memberLookup, string outputDirectory) : MemberMarkdownGenerator<Property, PropertyInfo>(fileSystem, memberLookup, outputDirectory)
{
    protected override void Generate(MarkdownWriter writer, Property property)
    {
        writer.WriteMainHeading($"{property.Type.DisplayName}.{property.MemberName} Property");

        writer.WriteSubHeading("Definition");

        WriteProperty(writer, property);
    }

    protected override void Generate(MarkdownWriter writer, MemberGroup<Property, PropertyInfo> group)
    {
        writer.WriteMainHeading($"{group.Type.DisplayName}.{group.DisplayName} Property");
        WriteMemberTable(writer, group.Members, "Overloads");
        foreach (var property in group.Members)
        {
            var id = MarkdownId.FromMember(property.MemberInfo);
            writer.WriteSubHeading(property.MemberName, id);
            // The scope only resets the heading suffix; it does not own the writer.
#pragma warning disable CA2000
            using var scope = writer.WithIdSuffix(id);
#pragma warning restore CA2000
            WriteProperty(writer, property);
        }
    }

    private void WriteProperty(MarkdownWriter writer, Property property)
    {
        WriteSection(writer, property.Documentation?.Summary);

        WriteSignature(writer, property);
        WriteSourceLink(writer, property);

        WriteParameters(writer, property, property.IndexParameters);

        WriteReturns(writer, property, property.MemberInfo.PropertyType, "Property Value");

        WriteRemarks(writer, property.Documentation);
    }

    private static void WriteSignature(MarkdownWriter writer, Property property)
    {
        using var code = writer.CodeBlock();

        var getter = property.Getter;
        var setter = property.Setter;

        WriteAccessibility(code, property.Accessibility);
        code.Write(" ");

        if (property.IsStatic)
        {
            code.Write("static ");
        }

        if (property.Virtuality != Virtuality.Normal)
        {
            code.Write(property.Virtuality.ToCSharpKeywords());
            code.Write(" ");
        }

        if (property.IsRequired)
        {
            code.Write("required ");
        }

        WriteTypeOrKeyword(code, property.MemberInfo.PropertyType);
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
        var accessorVisibility = accessor?.GetAccessibility();
        if (accessorVisibility == null)
        {
            return;
        }

        if (accessorVisibility != property.Accessibility)
        {
            code.Write(accessorVisibility.Value.ToCSharpKeywords());
            code.Write(" ");
        }

        code.Write(kind);
        code.Write("; ");
    }
}