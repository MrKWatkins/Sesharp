using MrKWatkins.Ast.Listening;
using MrKWatkins.Sesharp.XmlDocumentation;

namespace MrKWatkins.Sesharp.Model;

internal sealed class InheritDocListener : Listener<InheritDocResolver, ModelNode, DocumentableNode>
{
    protected override void ListenToNode(InheritDocResolver resolver, DocumentableNode node)
    {
        if (node.Documentation?.HasInheritDoc != true)
            return;

        var inherited = resolver.Resolve(node.MemberInfo);
        if (inherited != null)
            node.Documentation = node.Documentation.MergeWithInherited(inherited);
    }
}