using MrKWatkins.Sesharp.TestAssembly.VitePress;

namespace MrKWatkins.Sesharp.Tests;

public sealed class LinkExtensionsTests
{
    [TestCase(typeof(DefaultConstructorRecord))]
    [TestCase(typeof(PrimaryConstructorRecord))]
    public void DocumentationLink_RecordConstructor(Type type)
    {
        var constructor = type.GetConstructors().Single();

        constructor.DocumentationLink().Should().Equal($"{type.Namespace}/{type.Name}/-ctor.md");
    }

    [TestCase(0, "ctor")]
    [TestCase(1, "ctor_system-int32_")]
    public void DocumentationLink_RecordConstructorOverloads(int parameterCount, string suffix)
    {
        var constructor = typeof(OverloadedConstructorRecord).GetConstructors()
            .Single(candidate => candidate.GetParameters().Length == parameterCount);

        constructor.DocumentationLink().Should().Equal(
            "MrKWatkins.Sesharp.TestAssembly.VitePress/OverloadedConstructorRecord/-ctor.md#mrkwatkins-sesharp-testassembly-vitepress-overloadedconstructorrecord-" + suffix);
    }

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