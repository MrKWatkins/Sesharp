using System.Reflection;
using MrKWatkins.DocGen.Model;

namespace MrKWatkins.DocGen.Markdown.Generation;

public sealed class OperatorMarkdownGenerator : MemberMarkdownGenerator<MethodBase, Operator>
{
    public OperatorMarkdownGenerator(TypeLookup typeLookup, string parentDirectory)
        : base(typeLookup, parentDirectory)
    {
    }
}