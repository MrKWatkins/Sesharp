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

    protected override void Generate(Field field, MarkdownWriter writer)
    {
        writer.WriteMainHeading($"{field.Type.DisplayName}.{field.DisplayName} Field");
    }
}