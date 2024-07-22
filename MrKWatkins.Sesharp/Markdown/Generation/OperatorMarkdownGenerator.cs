using System.Reflection;
using MrKWatkins.Sesharp.Markdown.Writing;
using MrKWatkins.Sesharp.Model;

namespace MrKWatkins.Sesharp.Markdown.Generation;

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