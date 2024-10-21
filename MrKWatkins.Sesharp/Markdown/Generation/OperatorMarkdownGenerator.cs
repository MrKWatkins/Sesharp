using System.Reflection;
using MrKWatkins.Sesharp.Markdown.Writing;
using MrKWatkins.Sesharp.Model;

namespace MrKWatkins.Sesharp.Markdown.Generation;

public sealed class OperatorMarkdownGenerator(MemberLookup memberLookup, string outputDirectory) : MemberMarkdownGenerator<Operator, MethodInfo>(memberLookup, outputDirectory)
{
    protected override void Generate(MarkdownWriter writer, Operator @operator)
    {
        writer.WriteMainHeading($"{@operator.Type.DisplayName}.{@operator.DisplayName} Operator");
    }
}