using System.Reflection;
using MrKWatkins.DocGen.Model;

namespace MrKWatkins.DocGen.Markdown.Generation;

public sealed class OperatorMarkdownGenerator : MemberMarkdownGenerator<MethodBase, Operator>
{
    public OperatorMarkdownGenerator(MemberLookup memberLookup, string parentDirectory)
        : base(memberLookup, parentDirectory)
    {
    }
}