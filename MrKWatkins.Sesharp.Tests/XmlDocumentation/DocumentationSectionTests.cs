using System.Xml.Linq;
using MrKWatkins.Sesharp.XmlDocumentation;

namespace MrKWatkins.Sesharp.Tests.XmlDocumentation;

public sealed class DocumentationSectionTests
{
    [TestCaseSource(nameof(ParseTestCases))]
    public void Parse(XElement documentation, params DocumentationElement[] expected)
    {
        var section = DocumentationSection.Parse(documentation);
        section.Elements.Should().BeEquivalentTo(expected, c => c.WithStrictOrdering().RespectingRuntimeTypes());
    }

    [Pure]
    public static IEnumerable<TestCaseData> ParseTestCases()
    {
        yield return CreateTestCase(
            "Just text.",
            new TextElement("Just text."));

        yield return CreateTestCase(
            "<see cref=\"P:MrKWatkins.Ast.Message.Text\"/> Start with element.   ", // Also trailing whitespace.
            new See(XmlDocId.Parse("P:MrKWatkins.Ast.Message.Text"), null),
            new TextElement(" Start with element."));

        yield return CreateTestCase(
            "End with element. <see cref=\"P:MrKWatkins.Ast.Message.Text\"/>   ", // Also trailing whitespace.
            new TextElement("End with element. "),
            new See(XmlDocId.Parse("P:MrKWatkins.Ast.Message.Text"), null));

        yield return CreateTestCase($"No whitespace on{Environment.NewLine}line ends.",
            new TextElement("No whitespace on line ends."));

        yield return CreateTestCase($"   Whitespace on   {Environment.NewLine}   line ends.    ",
            new TextElement("Whitespace on line ends."));

        yield return CreateTestCase(
            """
            Whitespace around elements <see cref="P:MrKWatkins.Ast.Message.Level"/> <see cref="F:MrKWatkins.Ast.MessageLevel.Info"/>
               <see cref="P:MrKWatkins.Ast.Message.Text"/>.
            """,
            new TextElement("Whitespace around elements "),
            new See(XmlDocId.Parse("P:MrKWatkins.Ast.Message.Level"), null),
            new TextElement(" "),
            new See(XmlDocId.Parse("F:MrKWatkins.Ast.MessageLevel.Info"), null),
            new TextElement(" "),
            new See(XmlDocId.Parse("P:MrKWatkins.Ast.Message.Text"), null),
            new TextElement("."));

        yield return CreateTestCase(
            "<see langword=\"null\"/>",
            new CodeElement("null"));
    }

    [Pure]
    private static TestCaseData CreateTestCase(string summaryXml, params DocumentationElement[] expected)
    {
        var xml = $"<summary>{Environment.NewLine}{summaryXml}{Environment.NewLine}</summary>";
        var xElement = XElement.Parse(xml, LoadOptions.PreserveWhitespace);

        return new TestCaseData(xElement, expected);
    }
}