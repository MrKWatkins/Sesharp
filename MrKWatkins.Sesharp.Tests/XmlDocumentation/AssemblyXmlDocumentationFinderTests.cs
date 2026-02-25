using System.Numerics;
using MrKWatkins.Sesharp.XmlDocumentation;

namespace MrKWatkins.Sesharp.Tests.XmlDocumentation;

public sealed class AssemblyXmlDocumentationFinderTests
{
    // System.Private.CoreLib doesn't have a matching XML in the reference packs — its
    // types' docs live in files like System.Runtime.xml. FindXmlPath correctly returns
    // null; the resolver falls back to FindReferencePackDirectory + full scan.
    [Test]
    public void FindXmlPath_SystemPrivateCoreLib_ReturnsNull()
    {
        var assembly = typeof(INumberBase<>).Assembly;

        var path = AssemblyXmlDocumentationFinder.FindXmlPath(assembly);

        path.Should().BeNull();
    }

    [Test]
    public void FindReferencePackDirectory_SystemPrivateCoreLib_ReturnsDirectory()
    {
        var assembly = typeof(INumberBase<>).Assembly;

        var dir = AssemblyXmlDocumentationFinder.FindReferencePackDirectory(assembly);

        dir.Should().NotBeNull();
        Directory.GetFiles(dir!, "*.xml").Should().NotBeEmpty();
    }
}