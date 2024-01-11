using System.Reflection;
using MrKWatkins.DocGen.Markdown.Writing;
using MrKWatkins.DocGen.Model;

namespace MrKWatkins.DocGen.Markdown.Generation;

public sealed class MethodMarkdownGenerator : MemberMarkdownGenerator<Method, MethodInfo>
{
    public MethodMarkdownGenerator(MemberLookup memberLookup, string outputDirectory)
        : base(memberLookup, outputDirectory)
    {
    }

    protected override void Generate(Method method, MarkdownWriter writer)
    {
        writer.WriteMainHeading($"{method.Type.DisplayName}.{method.DisplayName} Method");
    }

    protected override void Generate(MemberGroup<Method, MethodInfo> group, MarkdownWriter writer)
    {
        writer.WriteMainHeading($"{group.Type.DisplayName}.{group.DisplayName} Method");
    }
}