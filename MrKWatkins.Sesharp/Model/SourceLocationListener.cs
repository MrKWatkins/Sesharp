using MrKWatkins.Ast.Listening;
using MrKWatkins.Sesharp.SourceLink;

namespace MrKWatkins.Sesharp.Model;

internal sealed class SourceLocationListener : NodeListener<PortablePdbReader, ModelNode, DocumentableNode>
{
    protected override void ListenToNode(PortablePdbReader reader, DocumentableNode node) =>
        node.SourceLocation = reader.GetSourceLocation(node.MemberInfo);
}