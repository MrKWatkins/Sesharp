namespace MrKWatkins.Sesharp.Tests;

public sealed class LinkExtensionsTests
{
    [TestCase("index", "member-index")]
    [TestCase("Index", "member-Index")]
    [TestCase("INDEX", "member-INDEX")]
    [TestCase("IndexOf", "IndexOf")]
    public void BuildMemberBaseFilename(string name, string expected) =>
        LinkExtensions.BuildMemberBaseFilename(name).Should().Equal(expected);

    [Test]
    public void DocumentationLink_SameType()
    {
        var type = typeof(string);
        type.DocumentationLink(type.DocumentationFileName()).Should().Equal("index.md");
    }

    [Test]
    public void DocumentationLink_SameMethod()
    {
        var method = typeof(object).GetMethod(nameof(GetHashCode))!;
        method.DocumentationLink(method.DocumentationFileName()).Should().Equal("GetHashCode.md");
    }

    [Test]
    public void DocumentationLink_SameEnumField()
    {
        var field = typeof(DayOfWeek).GetField(nameof(DayOfWeek.Monday))!;
        field.DocumentationLink(field.DocumentationFileName()).Should().Equal("#fields");
    }
}