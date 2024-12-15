using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MrKWatkins.Sesharp.XmlDocumentation;

// TODO: More unit tests.
public sealed class DocumentationSection
{
    private static readonly Regex NewLineRegex = new(@"\r?\n", RegexOptions.Compiled);
    private static readonly Regex ReduceWhitespaceRegex = new(@"\s\s+", RegexOptions.Compiled);

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

            var docElement = node switch
            {
                XElement xElement => ParseElement(xElement),
                XText xText => ParseTextElement(xText, isFirst, isLast),
                _ => throw new NotSupportedException($"The element type {node.GetType().Name} is not supported.")
            };

            if (docElement != null)
            {
                yield return docElement;
            }
        }
    }

    [Pure]
    private static DocumentationElement ParseElement(XElement element) =>
        element.Name.LocalName switch
        {
            "c" => new CodeElement(element.Value),
            "code" => new CodeElement(element.Value),
            "paramref" => new ParamRef(element.Attribute("name")?.Value ?? throw new FormatException("<paramref> element does not have name attribute."), element.Value),
            "see" => new See(XmlDocId.Parse(element.Attribute("cref")?.Value ?? throw new FormatException("<see> element does not have cref attribute.")), element.Value),
            "typeparamref" => new TypeParamRef(element.Attribute("name")?.Value ?? throw new FormatException("<typeparamref> element does not have name attribute."), element.Value),
            _ => throw new NotSupportedException($"Elements of name {element.Name} are not supported.")
        };

    [Pure]
    private static TextElement? ParseTextElement(XText textElement, bool isFirst, bool isLast)
    {
        var text = NewLineRegex.Replace(textElement.Value, " ");
        text = ReduceWhitespaceRegex.Replace(text, " ");

        if (isFirst)
        {
            text = text.TrimStart();
        }
        if (isLast)
        {
            text = text.TrimEnd();
        }

        return text.Length > 0 ? new TextElement(text) : null;
    }
}