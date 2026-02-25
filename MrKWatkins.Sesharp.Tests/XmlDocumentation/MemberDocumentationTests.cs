using System.Xml.Linq;
using MrKWatkins.Sesharp.XmlDocumentation;

namespace MrKWatkins.Sesharp.Tests.XmlDocumentation;

public sealed class MemberDocumentationTests
{
    [Test]
    public void Parse_NoInheritDoc()
    {
        var xml = XElement.Parse("""<member name="M:Foo.Bar"><summary>A summary.</summary></member>""", LoadOptions.PreserveWhitespace);

        var result = MemberDocumentation.Parse(xml);

        result.HasInheritDoc.Should().BeFalse();
        result.InheritDocCref.Should().BeNull();
        result.Summary.Should().NotBeNull();
    }

    [Test]
    public void Parse_InheritDoc_NoCref()
    {
        var xml = XElement.Parse("""<member name="M:Foo.Bar"><inheritdoc /></member>""", LoadOptions.PreserveWhitespace);

        var result = MemberDocumentation.Parse(xml);

        result.HasInheritDoc.Should().BeTrue();
        result.InheritDocCref.Should().BeNull();
        result.Summary.Should().BeNull();
    }

    [Test]
    public void Parse_InheritDoc_WithCref()
    {
        var xml = XElement.Parse("""<member name="M:Foo.Bar"><inheritdoc cref="M:Other.Method" /></member>""", LoadOptions.PreserveWhitespace);

        var result = MemberDocumentation.Parse(xml);

        result.HasInheritDoc.Should().BeTrue();
        result.InheritDocCref.Should().Equal(XmlDocId.Parse("M:Other.Method"));
    }

    [Test]
    public void Parse_InheritDoc_WithOtherContent()
    {
        var xml = XElement.Parse("""<member name="M:Foo.Bar"><inheritdoc /><remarks>Extra remarks.</remarks></member>""", LoadOptions.PreserveWhitespace);

        var result = MemberDocumentation.Parse(xml);

        result.HasInheritDoc.Should().BeTrue();
        result.InheritDocCref.Should().BeNull();
        result.Summary.Should().BeNull();
        result.Remarks.Should().NotBeNull();
    }

    [Test]
    public void MergeWithInherited_AllFieldsFromInherited()
    {
        var localXml = XElement.Parse("""<member name="M:Foo.Bar"><inheritdoc /></member>""", LoadOptions.PreserveWhitespace);
        var inheritedXml = XElement.Parse("""<member name="M:Base.Bar"><summary>Base summary.</summary><returns>A value.</returns></member>""", LoadOptions.PreserveWhitespace);

        var local = MemberDocumentation.Parse(localXml);
        var inherited = MemberDocumentation.Parse(inheritedXml);

        var merged = local.MergeWithInherited(inherited);

        merged.HasInheritDoc.Should().BeFalse();
        merged.InheritDocCref.Should().BeNull();
        merged.Summary.Should().NotBeNull();
        merged.Returns.Should().NotBeNull();
    }

    [Test]
    public void MergeWithInherited_LocalFieldsTakePriority()
    {
        var localXml = XElement.Parse("""<member name="M:Foo.Bar"><inheritdoc /><remarks>Local remarks.</remarks></member>""", LoadOptions.PreserveWhitespace);
        var inheritedXml = XElement.Parse("""<member name="M:Base.Bar"><summary>Base summary.</summary><remarks>Base remarks.</remarks></member>""", LoadOptions.PreserveWhitespace);

        var local = MemberDocumentation.Parse(localXml);
        var inherited = MemberDocumentation.Parse(inheritedXml);

        var merged = local.MergeWithInherited(inherited);

        // Summary comes from inherited, remarks from local.
        merged.Summary.Should().NotBeNull();
        merged.Remarks!.Elements.OfType<TextElement>().Single().Text.Should().Contain("Local remarks");
    }
}