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

    protected override void Generate(Property member, MarkdownWriter writer)
    {
    }
}