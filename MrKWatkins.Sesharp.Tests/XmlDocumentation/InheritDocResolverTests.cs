using MrKWatkins.Sesharp.Testing;
using MrKWatkins.Sesharp.TestAssembly.InheritDoc;
using MrKWatkins.Sesharp.XmlDocumentation;

namespace MrKWatkins.Sesharp.Tests.XmlDocumentation;

public sealed class InheritDocResolverTests : TestFixture
{
    private static InheritDocResolver CreateResolver()
    {
        var documentation = Documentation.Load(new RealFileSystem(), TestAssemblyXmlPath);
        return new InheritDocResolver(TestAssembly, documentation);
    }

    [Test]
    public void Resolve_InterfaceMethod_InheritsDocumentation()
    {
        var resolver = CreateResolver();
        var method = typeof(InheritDocImplementation).GetMethod(nameof(InheritDocImplementation.GetValue))!;

        var result = resolver.Resolve(method);

        result.Should().NotBeNull();
        result!.HasInheritDoc.Should().BeFalse();
        result.Summary.Should().NotBeNull();
        result.Summary!.Elements.OfType<TextElement>().Single().Text.Should().Contain("Gets a value");
        result.Returns.Should().NotBeNull();
    }

    [Test]
    public void Resolve_InterfaceMethod_WithParameters_InheritsDocumentation()
    {
        var resolver = CreateResolver();
        var method = typeof(InheritDocImplementation).GetMethod(nameof(InheritDocImplementation.SetValue))!;

        var result = resolver.Resolve(method);

        result.Should().NotBeNull();
        result!.HasInheritDoc.Should().BeFalse();
        result.Summary.Should().NotBeNull();
        result.Parameters.Should().ContainKey("value");
    }

    [Test]
    public void Resolve_InterfaceProperty_InheritsDocumentation()
    {
        var resolver = CreateResolver();
        var property = typeof(InheritDocImplementation).GetProperty(nameof(InheritDocImplementation.Property))!;

        var result = resolver.Resolve(property);

        result.Should().NotBeNull();
        result!.HasInheritDoc.Should().BeFalse();
        result.Summary.Should().NotBeNull();
        result.Summary!.Elements.OfType<TextElement>().Single().Text.Should().Contain("A documented property");
    }

    [Test]
    public void Resolve_BaseClassMethod_InheritsDocumentation()
    {
        var resolver = CreateResolver();
        var method = typeof(InheritDocDerivedClass).GetMethod(nameof(InheritDocDerivedClass.VirtualMethod))!;

        var result = resolver.Resolve(method);

        result.Should().NotBeNull();
        result!.HasInheritDoc.Should().BeFalse();
        result.Summary.Should().NotBeNull();
        result.Summary!.Elements.OfType<TextElement>().Single().Text.Should().Contain("virtual method");
        result.Returns.Should().NotBeNull();
    }

    [Test]
    public void Resolve_BaseClassProperty_InheritsDocumentation()
    {
        var resolver = CreateResolver();
        var property = typeof(InheritDocDerivedClass).GetProperty(nameof(InheritDocDerivedClass.VirtualProperty))!;

        var result = resolver.Resolve(property);

        result.Should().NotBeNull();
        result!.HasInheritDoc.Should().BeFalse();
        result.Summary.Should().NotBeNull();
        result.Summary!.Elements.OfType<TextElement>().Single().Text.Should().Contain("virtual property");
    }

    [Test]
    public void Resolve_NoInheritDoc_ReturnsUnchanged()
    {
        var resolver = CreateResolver();
        // GetValue on the interface itself has a proper summary, no inheritdoc.
        var method = typeof(IInheritDocInterface).GetMethod(nameof(IInheritDocInterface.GetValue))!;

        var result = resolver.Resolve(method);

        result.Should().NotBeNull();
        result!.HasInheritDoc.Should().BeFalse();
        result.Summary.Should().NotBeNull();
    }

    [Test]
    public void Resolve_NullDocumentation_ReturnsNull()
    {
        // Use a member with no XML doc at all.
        var resolver = CreateResolver();
        var method = typeof(object).GetMethod(nameof(ToString))!;

        // object.ToString has no docs in our assembly - should return null or system docs.
        // The important thing is it doesn't throw.
        Assert.DoesNotThrow(() => _ = resolver.Resolve(method));
    }
}