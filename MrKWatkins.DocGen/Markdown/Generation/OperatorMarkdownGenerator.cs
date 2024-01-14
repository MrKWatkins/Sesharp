using System.Reflection;
using MrKWatkins.DocGen.Markdown.Writing;
using MrKWatkins.DocGen.Model;

namespace MrKWatkins.DocGen.Markdown.Generation;

public sealed class OperatorMarkdownGenerator : MemberMarkdownGenerator<Operator, MethodInfo>
{
    public OperatorMarkdownGenerator(MemberLookup memberLookup, string outputDirectory)
        : base(memberLookup, outputDirectory)
    {
    }

    protected override void Generate(MarkdownWriter writer, Operator @operator)
    {
        writer.WriteMainHeading($"{@operator.Type.DisplayName}.{@operator.DisplayName} Operator");
    }
}