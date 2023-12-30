using MrKWatkins.Ast.Listening;
using MrKWatkins.DocGen.XmlDocumentation;

namespace MrKWatkins.DocGen.Model;

public sealed class DocumentationListener : ListenerWithContext<Documentation, ModelNode, DocumentableNode>
{
    protected override void ListenToNode(Documentation context, DocumentableNode node)
    {
        node.Documentation = context.GetMemberDocumentationOrNull(node.XmlDocId);
    }
}