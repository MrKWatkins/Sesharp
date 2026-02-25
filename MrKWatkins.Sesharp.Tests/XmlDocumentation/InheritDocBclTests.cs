using System.Numerics;
using System.Reflection;
using MrKWatkins.Sesharp.Testing;
using MrKWatkins.Sesharp.TestAssembly.InheritDoc;
using MrKWatkins.Sesharp.XmlDocumentation;

namespace MrKWatkins.Sesharp.Tests.XmlDocumentation;

public sealed class InheritDocBclTests : TestFixture
{
    [Test]
    public void SystemRuntimeXml_LoadsSuccessfully()
    {
        var coreLibAssembly = typeof(INumberBase<>).Assembly;
        var refPackDir = AssemblyXmlDocumentationFinder.FindReferencePackDirectory(coreLibAssembly);
        refPackDir.Should().NotBeNull();

        var systemRuntimeXml = Path.Combine(refPackDir!, "System.Runtime.xml");
        File.Exists(systemRuntimeXml).Should().BeTrue();

        // Should not throw after the ParseElement fix.
        var docs = Documentation.Load(systemRuntimeXml);
        docs.Should().NotBeNull();

        var id = XmlDocId.Parse("M:System.Numerics.INumberBase`1.Abs(`0)");
        var memberDoc = docs.GetMemberDocumentationOrNull(id);
        Console.WriteLine($"INumberBase.Abs docs: {memberDoc?.Summary}");
        memberDoc.Should().NotBeNull();
        memberDoc!.Summary.Should().NotBeNull();
    }

    [Test]
    public void Resolver_INumberBaseAbs_FindsBclDocs()
    {
        var documentation = Documentation.Load(new RealFileSystem(), TestAssemblyXmlPath);
        var resolver = new InheritDocResolver(TestAssembly, documentation);

        // INumberBase<T>.Abs on the open generic type (simulates what ToOpenMember returns).
        var method = typeof(INumberBase<>).GetMethod("Abs", BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);
        method.Should().NotBeNull();

        var id = XmlDocId.Create(method!);
        Console.WriteLine($"XmlDocId for INumberBase<T>.Abs: {id}");

        var result = resolver.Resolve(method!);
        Console.WriteLine($"Summary: {result?.Summary?.Elements.FirstOrDefault()}");

        result.Should().NotBeNull();
        result!.Summary.Should().NotBeNull();
    }

    [Test]
    public void Resolver_BclStaticAbstractInterface_InheritsDocumentation()
    {
        // InheritDocBclStruct.Parse has <inheritdoc /> and implements IParsable<T>.Parse (static abstract).
        // This simulates what UInt24.Abs does with INumberBase<T>.Abs.
        var documentation = Documentation.Load(new RealFileSystem(), TestAssemblyXmlPath);
        var resolver = new InheritDocResolver(TestAssembly, documentation);

        var method = typeof(InheritDocBclStruct).GetMethod("Parse", BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);
        method.Should().NotBeNull();

        var id = XmlDocId.Create(method!);
        Console.WriteLine($"XmlDocId for InheritDocBclStruct.Parse: {id}");

        var memberDoc = documentation.GetMemberDocumentationOrNull(id);
        Console.WriteLine($"HasInheritDoc: {memberDoc?.HasInheritDoc}");
        memberDoc.Should().NotBeNull();
        memberDoc!.HasInheritDoc.Should().BeTrue();

        var result = resolver.Resolve(method!);
        Console.WriteLine($"Summary: {result?.Summary?.Elements.FirstOrDefault()}");

        result.Should().NotBeNull();
        result!.Summary.Should().NotBeNull();
    }
}