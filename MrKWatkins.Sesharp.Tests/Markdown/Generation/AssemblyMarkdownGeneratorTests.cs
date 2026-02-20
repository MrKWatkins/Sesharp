using MrKWatkins.Sesharp.Markdown.Generation;
using MrKWatkins.Sesharp.Model;
using MrKWatkins.Sesharp.Testing;
using MrKWatkins.Sesharp.XmlDocumentation;

namespace MrKWatkins.Sesharp.Tests.Markdown.Generation;

public sealed class AssemblyMarkdownGeneratorTests : TestFixture
{
    [Test]
    public async Task Generate()
    {
        var documentation = Documentation.Load(new RealFileSystem(), TestAssemblyXmlPath);
        var assemblyDetails = AssemblyParser.Parse(TestAssembly, documentation);

        var fileSystem = new TestFileSystem();
        AssemblyMarkdownGenerator.Generate(fileSystem, assemblyDetails, "/");

        var file = fileSystem.CreatedFiles["/MrKWatkins.Sesharp.TestAssembly.Properties/PropertyIndexer/index.md"];

        await Verify(file);
    }
}