using System.Reflection;
using MrKWatkins.Sesharp.Markdown;
using MrKWatkins.Sesharp.Markdown.Writing;
using MrKWatkins.Sesharp.Model;

namespace MrKWatkins.Sesharp.Markdown.Generation;

public sealed class OperatorMarkdownGenerator(IFileSystem fileSystem, MemberLookup memberLookup, string outputDirectory, IMarkdownOptions options) : MemberMarkdownGenerator<Operator, MethodInfo>(fileSystem, memberLookup, outputDirectory, options)
{
    protected override void Generate(MarkdownWriter writer, Operator @operator)
    {
        writer.WriteMainHeading($"{@operator.Type.DisplayName}.{@operator.DisplayName} Operator");
    }
}