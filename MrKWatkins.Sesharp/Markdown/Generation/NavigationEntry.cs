using MrKWatkins.Sesharp.Model;

namespace MrKWatkins.Sesharp.Markdown.Generation;

internal sealed record NavigationEntry(string Id, string Path, string Title, string Kind, string? Parent)
{
    public static NavigationEntry FromNode(OutputNode node)
    {
        var id = node is DocumentableNode documentable ? documentable.XmlDocId.Id : $"G:{node.FileName}";
        var parent = node is Model.Type type
            ? type.MemberInfo.DeclaringType is { } declaringType
                ? XmlDocId.Create(declaringType).Id
                : $"N:{type.Namespace.Name}"
            : node.Ancestors.OfType<Model.Type>().First().XmlDocId.Id;
        return new NavigationEntry(id, node.FileName.Replace('\\', '/'), node.MenuName, node is Model.Type ? "type" : "member", parent);
    }
}