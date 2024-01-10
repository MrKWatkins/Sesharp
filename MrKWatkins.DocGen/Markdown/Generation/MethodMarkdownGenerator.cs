using System.Reflection;
using MrKWatkins.DocGen.Markdown.Writing;
using MrKWatkins.DocGen.Model;

namespace MrKWatkins.DocGen.Markdown.Generation;

public sealed class MethodMarkdownGenerator : MemberMarkdownGenerator<Method, MethodBase>
{
    public MethodMarkdownGenerator(MemberLookup memberLookup, string outputDirectory)
        : base(memberLookup, outputDirectory)
    {
    }

    protected override void Generate(Method member, MarkdownWriter writer)
    {
    }
}