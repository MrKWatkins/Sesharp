using System.Text.Json;
using System.Text.RegularExpressions;
using MrKWatkins.Sesharp.Markdown;
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
        AssemblyMarkdownGenerator.Generate(fileSystem, assemblyDetails, "/", dialect: MarkdownDialect.MkDocs);

        var file = fileSystem.CreatedFiles["/MrKWatkins.Sesharp.TestAssembly.Properties/PropertyIndexer/index.md"];

        await Verify(file);
    }

    [TestCase(MarkdownDialect.VitePress, "{#", "}")]
    [TestCase(MarkdownDialect.MkDocs, "{: #", " }")]
    public void Generate_OverloadAnchors(MarkdownDialect dialect, string prefix, string suffix)
    {
        var files = GenerateFiles(dialect);
        foreach (var name in new[] { "VitePressClass-T/Read.md", "VitePressClass-T/-ctor.md", "VitePressValue/op_BitwiseOr.md" })
        {
            var text = files.CreatedFiles[$"/MrKWatkins.Sesharp.TestAssembly.VitePress/{name}"].Text;
            var links = Regex.Matches(text, @"\]\(#([^)]+)\)");
            Assert.That(links.Count, Is.GreaterThanOrEqualTo(2), name);
            foreach (Match link in links)
            {
                Assert.That(text, Does.Contain(prefix + link.Groups[1].Value + suffix), name);
            }
        }
    }

    [Test]
    public void Generate_VitePressText()
    {
        var files = GenerateFiles();
        var type = files.CreatedFiles["/MrKWatkins.Sesharp.TestAssembly.VitePress/VitePressClass-T/index.md"].Text;
        Assert.That(type, Does.Contain("# VitePressClass&lt;T&gt; Class"));
        Assert.That(type, Does.Contain("&lt;tag&gt; &amp; &#123;&#123;literal&#125;&#125; &#123;value&#125; &#124; text"));
        Assert.That(type, Does.Contain("`a\\|b`"));
        Assert.That(type, Does.Contain("editLink: false"));
        var methods = files.CreatedFiles["/MrKWatkins.Sesharp.TestAssembly.VitePress/VitePressClass-T/Read.md"].Text;
        Assert.That(methods, Does.StartWith("---\neditLink: false\n---\n"));
        Assert.That(methods, Does.Contain("string? Read(string? value)"));
    }

    [Test]
    public void Generate_IndexerOverloads()
    {
        var files = GenerateFiles();
        const string prefix = "/MrKWatkins.Sesharp.TestAssembly.Properties/PropertyPublicOverloadedIndexer/";
        var text = files.CreatedFiles[prefix + "Item.md"].Text;
        Assert.That(text, Does.Contain("Item[Int32]").And.Contain("Item[String]"));
        var links = Regex.Matches(text, @"\]\(#([^)]+)\)");
        Assert.That(links.Count, Is.EqualTo(2));
        foreach (Match link in links)
        {
            Assert.That(text, Does.Contain("{#" + link.Groups[1].Value + "}"));
            Assert.That(files.CreatedFiles[prefix + "index.md"].Text, Does.Contain("Item.md#" + link.Groups[1].Value));
        }
    }

    [Test]
    public void Generate_Navigation()
    {
        var files = GenerateFiles();
        using var json = JsonDocument.Parse(files.CreatedFiles["/navigation.json"].Text);
        Assert.That(json.RootElement.GetProperty("schemaVersion").GetInt32(), Is.EqualTo(1));
        var entries = json.RootElement.GetProperty("entries").EnumerateArray().ToArray();
        Assert.That(entries.Select(e => e.GetProperty("kind").GetString()).Distinct(), Is.EquivalentTo(new[] { "namespace", "type", "member" }));
        var generic = entries.Single(e => e.GetProperty("path").GetString() == "MrKWatkins.Sesharp.TestAssembly.VitePress/VitePressClass-T/index.md");
        Assert.That(generic.GetProperty("title").GetString(), Is.EqualTo("VitePressClass<T>"));
        Assert.That(generic.GetProperty("parent").GetString(), Is.EqualTo("N:MrKWatkins.Sesharp.TestAssembly.VitePress"));
        foreach (var entry in entries)
        {
            Assert.That(files.CreatedFiles.ContainsKey("/" + entry.GetProperty("path").GetString()), Is.True, entry.ToString());
            if (entry.GetProperty("parent").GetString() is { } parent)
            {
                Assert.That(entries.Any(e => e.GetProperty("id").GetString() == parent), Is.True, parent);
            }
        }
        Assert.That(entries.Select(e => e.GetProperty("id").GetString()).Distinct().Count(), Is.EqualTo(entries.Length));
        Assert.That(entries.Select(e => e.GetProperty("path").GetString()).Distinct().Count(), Is.EqualTo(entries.Length));
    }

    [Test]
    public void Generate_NestedTypes()
    {
        var files = GenerateFiles();
        const string prefix = "/MrKWatkins.Sesharp.TestAssembly.VitePress/";
        Assert.That(files.CreatedFiles.ContainsKey(prefix + "VitePressClass-T/Nested-T/index.md"), Is.True);
        Assert.That(files.CreatedFiles.ContainsKey(prefix + "AnotherClass/Nested/index.md"), Is.True);
        using var json = JsonDocument.Parse(files.CreatedFiles["/navigation.json"].Text);
        var entries = json.RootElement.GetProperty("entries").EnumerateArray().ToArray();
        var nested = entries.Single(e => e.GetProperty("path").GetString() == prefix[1..] + "VitePressClass-T/Nested-T/index.md");
        Assert.That(nested.GetProperty("parent").GetString(), Is.EqualTo("T:MrKWatkins.Sesharp.TestAssembly.VitePress.VitePressClass`1"));
    }

    [Test]
    public void Generate_SourceRef()
    {
        var documentation = Documentation.Load(new RealFileSystem(), TestAssemblyXmlPath);
        var assembly = AssemblyParser.Parse(TestAssembly, documentation);
        var type = assembly.Namespaces.SelectMany(n => n.Types).First(t => t.Name == "AnotherClass");
        type.SourceLocation = new SourceLocation("src/My File.cs", 42);
        var files = new TestFileSystem();
        AssemblyMarkdownGenerator.Generate(files, assembly, "/", "https://github.com/example/library/", sourceRef: "release/1.2.3");
        Assert.That(files.CreatedFiles["/" + type.FileName].Text,
            Does.Contain("https://github.com/example/library/blob/release%2F1.2.3/src/My%20File.cs#L42"));
        Assert.That(string.Join('\n', files.CreatedFiles.Values.Select(f => f.Text)), Does.Not.Contain("/blob/main/"));
    }

    [Test]
    public void Generate_MultipleAssemblies()
    {
        static AssemblyDetails Create(System.Type type)
        {
            var assembly = new AssemblyDetails(type.Assembly);
            var space = new Namespace("System");
            space.Children.Add(new Model.Type(type));
            assembly.Children.Add(space);
            return assembly;
        }
        var assemblies = new[] { Create(typeof(string)), Create(typeof(Uri)) };
        var first = new TestFileSystem();
        var second = new TestFileSystem();
        AssemblyMarkdownGenerator.Generate(first, assemblies, "/");
        AssemblyMarkdownGenerator.Generate(second, assemblies.Reverse().ToArray(), "/");
        Assert.That(first.CreatedFiles["/navigation.json"].Text, Is.EqualTo(second.CreatedFiles["/navigation.json"].Text));
        Assert.That(first.CreatedFiles["/System/index.md"].Text, Does.Contain("String/index.md").And.Contain("Uri/index.md"));
        using var json = JsonDocument.Parse(first.CreatedFiles["/navigation.json"].Text);
        Assert.That(json.RootElement.GetProperty("entries").EnumerateArray().Count(e => e.GetProperty("kind").GetString() == "namespace"), Is.EqualTo(1));
    }

    private static TestFileSystem GenerateFiles(MarkdownDialect dialect = MarkdownDialect.VitePress)
    {
        var documentation = Documentation.Load(new RealFileSystem(), TestAssemblyXmlPath);
        var assembly = AssemblyParser.Parse(TestAssembly, documentation);
        var files = new TestFileSystem();
        AssemblyMarkdownGenerator.Generate(files, assembly, "/", dialect: dialect);
        return files;
    }
}