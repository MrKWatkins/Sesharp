using MrKWatkins.Ast.Listening;
using MrKWatkins.Sesharp.XmlDocumentation;

namespace MrKWatkins.Sesharp.Model;

public sealed class DocumentationListener : Listener<Documentation, ModelNode, DocumentableNode>
{
    protected override void ListenToNode(Documentation context, DocumentableNode node)
    {
        node.Documentation = context.GetMemberDocumentationOrNull(node.XmlDocId);
    }
}