using System.Reflection;
using MrKWatkins.DocGen.Markdown.Writing;
using MrKWatkins.DocGen.Model;

namespace MrKWatkins.DocGen.Markdown.Generation;

public sealed class OperatorMarkdownGenerator : MemberMarkdownGenerator<Operator, MethodBase>
{
    public OperatorMarkdownGenerator(MemberLookup memberLookup, string outputDirectory)
        : base(memberLookup, outputDirectory)
    {
    }

    protected override void Generate(Operator member, MarkdownWriter writer)
    {
    }
}