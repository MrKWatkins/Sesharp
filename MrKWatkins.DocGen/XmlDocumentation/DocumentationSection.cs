using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MrKWatkins.DocGen.XmlDocumentation;

public sealed class DocumentationSection
{
    private static readonly Regex SummaryTrimRegex = new(@"\r?\n\s*");

    private DocumentationSection([InstantHandle] IEnumerable<DocumentationElement> elements)
    {
        Elements = elements.ToList();
    }

    public IReadOnlyList<DocumentationElement> Elements { get; }

    [Pure]
    public static DocumentationSection Parse(XElement element) => new(ParseChildren(element));

    [Pure]
    [return: NotNullIfNotNull(nameof(element))]
    public static DocumentationSection? ParseOrNull(XElement? element) =>
        element != null ? new DocumentationSection(ParseChildren(element)) : null;

    [Pure]
    private static IEnumerable<DocumentationElement> ParseChildren(XElement element)
    {
        var nodes = element.Nodes().ToList();
        foreach (var node in nodes)
        {
            var isFirst = node == nodes[0];
            var isLast = node == nodes[^1];
            yield return node switch
            {
                XElement xElement => ParseElement(xElement),
                XText xText => ParseTextElement(xText, isFirst, isLast),
                _ => throw new NotSupportedException($"The element type {node.GetType().Name} is not supported.")
            };
        }
    }

    [Pure]
    private static DocumentationElement ParseElement(XElement element)
    {
        switch (element.Name.LocalName)
        {
            case "c":
                return new CodeElement(element.Value);

            case "paramref":
                return new ParamRef(
                    element.Attribute("name")?.Value ?? throw new FormatException("<paramref> element does not have name attribute."),
                    element.Value);

            case "see":
                return new See(
                    element.Attribute("cref")?.Value ?? throw new FormatException("<see> element does not have cref attribute."),
                    element.Value);

            case "typeparamref":
                return new TypeParamRef(
                    element.Attribute("name")?.Value ?? throw new FormatException("<typeparamref> element does not have name attribute."),
                    element.Value);
        }
        throw new NotSupportedException($"Elements of name {element.Name} are not supported.");
    }

    [Pure]
    private static TextElement ParseTextElement(XText textElement, bool isFirst, bool isLast)
    {
        var text = SummaryTrimRegex.Replace(textElement.Value, "");
        if (isFirst)
        {
            text = text.TrimStart();
        }
        else if (isLast)
        {
            text = text.TrimEnd();
        }
        return new TextElement(text);
    }
}