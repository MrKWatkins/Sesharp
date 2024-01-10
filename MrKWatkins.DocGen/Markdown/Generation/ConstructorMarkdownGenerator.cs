using System.Reflection;
using MrKWatkins.DocGen.Markdown.Writing;
using MrKWatkins.DocGen.Model;

namespace MrKWatkins.DocGen.Markdown.Generation;

public sealed class ConstructorMarkdownGenerator : MemberMarkdownGenerator<Constructor, MethodBase>
{
    public ConstructorMarkdownGenerator(MemberLookup memberLookup, string outputDirectory)
        : base(memberLookup, outputDirectory)
    {
    }

    public override void Generate(IReadOnlyList<Constructor> nodes)
    {
        if (nodes.Count == 0)
        {
            return;
        }

        var filePath = Path.Combine(OutputDirectory, nodes.First().FileName);
        using var writer = new MarkdownWriter(filePath);
    }

    protected override void Generate(Constructor member, MarkdownWriter writer) => throw new NotSupportedException("Cannot write a page for a single constructor, just one page for all constructors.");
}