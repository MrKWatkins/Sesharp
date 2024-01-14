using System.Reflection;
using MrKWatkins.DocGen.Markdown.Writing;
using MrKWatkins.DocGen.Model;

namespace MrKWatkins.DocGen.Markdown.Generation;

public sealed class ConstructorMarkdownGenerator : MemberMarkdownGenerator<Constructor, ConstructorInfo>
{
    public ConstructorMarkdownGenerator(MemberLookup memberLookup, string outputDirectory)
        : base(memberLookup, outputDirectory)
    {
    }

    protected override void Generate(MarkdownWriter writer, MemberGroup<Constructor, ConstructorInfo> group)
    {
        writer.WriteMainHeading($"{group.Type.DisplayName} Constructors");
    }
}