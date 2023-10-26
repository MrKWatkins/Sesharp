using System.Xml.Linq;

namespace MrKWatkins.DocGen.XmlDocumentation;

public sealed class DocumentationSection
{
    private DocumentationSection([InstantHandle] IEnumerable<DocumentationElement> elements)
    {
        Elements = elements.ToList();
    }

    public IReadOnlyList<DocumentationElement> Elements { get; }

    [Pure]
    public static DocumentationSection? Parse(XElement? element) =>
        element != null ? new DocumentationSection(ParseChildren(element)) : null;

    [Pure]
    private static IEnumerable<DocumentationElement> ParseChildren(XElement element) =>
        element.Nodes()
            .Select(node => node switch
            {
                XElement xElement => ParseElement(xElement),
                XText xText => new TextElement(xText.Value.Trim()),
                _ => throw new NotSupportedException($"The element type {node.GetType().Name} is not supported.")
            });

    [Pure]
    private static DocumentationElement ParseElement(XElement element)
    {
        switch (element.Name.LocalName)
        {
            case "c":
                return new CodeElement(element.Value.Trim());

            case "paramref":
                return new ParamRef(
                    element.Attribute("name")?.Value ?? throw new FormatException("<paramref> element does not have name attribute."),
                    element.Value.Trim());

            case "see":
                return new TypeParamRef(
                    element.Attribute("cref")?.Value ?? throw new FormatException("<see> element does not have cref attribute."),
                    element.Value.Trim());

            case "typeparamref":
                return new TypeParamRef(
                    element.Attribute("name")?.Value ?? throw new FormatException("<typeparamref> element does not have name attribute."),
                    element.Value.Trim());
        }
        throw new NotSupportedException($"Elements of name {element.Name} are not supported.");
    }
}