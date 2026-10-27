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
            links.Count.Should().BeGreaterThanOrEqualTo(2);
            foreach (Match link in links)
            {
                text.Should().Contain(prefix + link.Groups[1].Value + suffix);
            }
        }
    }

    [TestCase(MarkdownDialect.VitePress)]
    [TestCase(MarkdownDialect.MkDocs)]
    public void Generate_RecordConstructorLinks(MarkdownDialect dialect)
    {
        var files = GenerateFiles(dialect);
        foreach (var name in new[] { "DefaultConstructorRecord", "PrimaryConstructorRecord" })
        {
            var type = files.CreatedFiles[$"/MrKWatkins.Sesharp.TestAssembly.VitePress/{name}/index.md"].Text;

            type.Should().Contain("](-ctor.md)");
            type.Should().NotContain("](-ctor.md#");
        }
    }

    [Test]
    public void Generate_VitePressText()
    {
        var files = GenerateFiles();
        var type = files.CreatedFiles["/MrKWatkins.Sesharp.TestAssembly.VitePress/VitePressClass-T/index.md"].Text;
        type.Should().Contain("# VitePressClass&lt;T&gt; Class");
        type.Should().Contain("&lt;tag&gt; &amp; &#123;&#123;literal&#125;&#125; &#123;value&#125; &#124; text");
        type.Should().Contain("`a\\|b`");
        type.Should().Contain("editLink: false");
        var methods = files.CreatedFiles["/MrKWatkins.Sesharp.TestAssembly.VitePress/VitePressClass-T/Read.md"].Text;
        methods.Should().StartWith("---\neditLink: false\n---\n");
        methods.Should().Contain("string? Read(string? value)");
    }

    [Test]
    public void Generate_IndexerOverloads()
    {
        var files = GenerateFiles();
        const string prefix = "/MrKWatkins.Sesharp.TestAssembly.Properties/PropertyPublicOverloadedIndexer/";
        var text = files.CreatedFiles[prefix + "Item.md"].Text;
        text.Should().Contain("Item[Int32]").And.Contain("Item[String]");
        var links = Regex.Matches(text, @"\]\(#([^)]+)\)");
        links.Count.Should().Equal(2);
        foreach (Match link in links)
        {
            text.Should().Contain("{#" + link.Groups[1].Value + "}");
            files.CreatedFiles[prefix + "index.md"].Text.Should().Contain("Item.md#" + link.Groups[1].Value);
        }
    }

    [TestCase(MarkdownDialect.VitePress)]
    [TestCase(MarkdownDialect.MkDocs)]
    public void Generate_OperatorDisplayNames(MarkdownDialect dialect)
    {
        var files = GenerateFiles(dialect);
        const string prefix = "/MrKWatkins.Sesharp.TestAssembly.VitePress/";
        var type = files.CreatedFiles[prefix + "VitePressValue/index.md"].Text;
        type.Should().Contain("[`BitwiseOr(VitePressValue, VitePressValue)`](op_BitwiseOr.md#");
        type.Should().Contain("[`BitwiseOr(VitePressValue, Int32)`](op_BitwiseOr.md#");
        type.Should().NotContain("[`op_");

        var overloads = files.CreatedFiles[prefix + "VitePressValue/op_BitwiseOr.md"].Text;
        overloads.Should().Contain("# VitePressValue.BitwiseOr Operator");
        overloads.Should().Contain("[`BitwiseOr(VitePressValue, Int32)`](#");
        overloads.Should().Contain("## BitwiseOr(VitePressValue, Int32)");
        overloads.Should().NotContain("[`op_");

        var conversions = files.CreatedFiles[prefix + "ConversionValue/index.md"].Text;
        conversions.Should().Contain("[`CheckedExplicit(ConversionValue)`](op_CheckedExplicit.md)");
        conversions.Should().NotContain("[`op_");

        using var json = JsonDocument.Parse(files.CreatedFiles["/navigation.json"].Text);
        var entries = json.RootElement.GetProperty("entries").EnumerateArray().ToArray();
        var group = entries.Single(e => e.GetProperty("path").GetString() == prefix[1..] + "VitePressValue/op_BitwiseOr.md");
        group.GetProperty("title").GetString().Should().Equal("BitwiseOr");
        group.GetProperty("id").GetString().Should().Equal("G:" + prefix[1..] + "VitePressValue/op_BitwiseOr.md");
    }

    [TestCase(MarkdownDialect.VitePress)]
    [TestCase(MarkdownDialect.MkDocs)]
    public void Generate_ReservedIndexMember(MarkdownDialect dialect)
    {
        var files = GenerateFiles(dialect);
        const string prefix = "/MrKWatkins.Sesharp.TestAssembly.VitePress/VitePressClass-T/";
        files.CreatedFiles[prefix + "index.md"].Text.Should().Contain("](member-Index.md)");
        files.CreatedFiles[prefix + "member-Index.md"].Text.Should().Contain(".Index Property");
    }

    [Test]
    public void Generate_Navigation()
    {
        var files = GenerateFiles();
        using var json = JsonDocument.Parse(files.CreatedFiles["/navigation.json"].Text);
        json.RootElement.GetProperty("schemaVersion").GetInt32().Should().Equal(1);
        var entries = json.RootElement.GetProperty("entries").EnumerateArray().ToArray();
        entries.Select(e => e.GetProperty("kind").GetString()).ToHashSet().Should().SetEquals("namespace", "type", "member");
        var generic = entries.Single(e => e.GetProperty("path").GetString() == "MrKWatkins.Sesharp.TestAssembly.VitePress/VitePressClass-T/index.md");
        generic.GetProperty("title").GetString().Should().Equal("VitePressClass<T>");
        generic.GetProperty("parent").GetString().Should().Equal("N:MrKWatkins.Sesharp.TestAssembly.VitePress");
        foreach (var entry in entries)
        {
            files.CreatedFiles.Should().ContainKey("/" + entry.GetProperty("path").GetString());
            if (entry.GetProperty("parent").GetString() is { } parent)
            {
                entries.Select(e => e.GetProperty("id").GetString()).Should().Contain(parent);
            }
        }
        entries.Select(e => e.GetProperty("id").GetString()).Distinct().Count().Should().Equal(entries.Length);
        entries.Select(e => e.GetProperty("path").GetString()).Distinct(StringComparer.OrdinalIgnoreCase).Count().Should().Equal(entries.Length);
    }

    [Test]
    public void Generate_NestedTypes()
    {
        var files = GenerateFiles();
        const string prefix = "/MrKWatkins.Sesharp.TestAssembly.VitePress/";
        files.CreatedFiles.Should().ContainKey(prefix + "VitePressClass-T/Nested-T/index.md");
        files.CreatedFiles.Should().ContainKey(prefix + "AnotherClass/Nested/index.md");
        using var json = JsonDocument.Parse(files.CreatedFiles["/navigation.json"].Text);
        var entries = json.RootElement.GetProperty("entries").EnumerateArray().ToArray();
        var nested = entries.Single(e => e.GetProperty("path").GetString() == prefix[1..] + "VitePressClass-T/Nested-T/index.md");
        nested.GetProperty("parent").GetString().Should().Equal("T:MrKWatkins.Sesharp.TestAssembly.VitePress.VitePressClass`1");
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
        files.CreatedFiles["/" + type.FileName].Text.Should().Contain("https://github.com/example/library/blob/release%2F1.2.3/src/My%20File.cs#L42");
        string.Join('\n', files.CreatedFiles.Values.Select(f => f.Text)).Should().NotContain("/blob/main/");
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
        first.CreatedFiles["/navigation.json"].Text.Should().Equal(second.CreatedFiles["/navigation.json"].Text);
        first.CreatedFiles["/System/index.md"].Text.Should().Contain("String/index.md").And.Contain("Uri/index.md");
        using var json = JsonDocument.Parse(first.CreatedFiles["/navigation.json"].Text);
        json.RootElement.GetProperty("entries").EnumerateArray().Count(e => e.GetProperty("kind").GetString() == "namespace").Should().Equal(1);
    }

    [TestCase("Fraction", "double Fraction = 0.125;")]
    [TestCase("RoundTrip", "double RoundTrip = 0.12345678901234568;")]
    [TestCase("NegativeZero", "double NegativeZero = -0.0;")]
    [TestCase("NotANumber", "double NotANumber = double.NaN;")]
    [TestCase("PositiveInfinity", "double PositiveInfinity = double.PositiveInfinity;")]
    [TestCase("NegativeInfinity", "double NegativeInfinity = double.NegativeInfinity;")]
    [TestCase("SingleFraction", "float SingleFraction = 0.125f;")]
    [TestCase("SingleNegativeZero", "float SingleNegativeZero = -0f;")]
    [TestCase("SingleNotANumber", "float SingleNotANumber = float.NaN;")]
    [TestCase("SinglePositiveInfinity", "float SinglePositiveInfinity = float.PositiveInfinity;")]
    [TestCase("SingleNegativeInfinity", "float SingleNegativeInfinity = float.NegativeInfinity;")]
    [SetCulture("fr-FR")]
    public void Generate_FloatingPointConstants(string name, string signature)
    {
        var files = GenerateFiles();
        files.CreatedFiles[$"/MrKWatkins.Sesharp.TestAssembly.Fields/FloatingPointConstants/{name}.md"].Text.Should().Contain("public const " + signature);
    }

    [TestCase("Named", "EnumConstants.Choice Named = EnumConstants.Choice.First;")]
    [TestCase("Unnamed", "EnumConstants.Choice Unnamed = (EnumConstants.Choice)4;")]
    [TestCase("Negative", "EnumConstants.Choice Negative = (EnumConstants.Choice)-1;")]
    [TestCase("Zero", "EnumConstants.Bits Zero = EnumConstants.Bits.None;")]
    [TestCase("Combined", "EnumConstants.Bits Combined = EnumConstants.Bits.First | EnumConstants.Bits.Second;")]
    [TestCase("High", "EnumConstants.Bits High = EnumConstants.Bits.High;")]
    [TestCase("HighCombined", "EnumConstants.Bits HighCombined = EnumConstants.Bits.First | EnumConstants.Bits.High;")]
    [TestCase("UnnamedBits", "EnumConstants.Bits UnnamedBits = (EnumConstants.Bits)8;")]
    public void Generate_EnumConstants(string name, string signature)
    {
        var files = GenerateFiles();
        var text = files.CreatedFiles[$"/MrKWatkins.Sesharp.TestAssembly.Fields/EnumConstants/{name}.md"].Text;
        Regex.Match(text, @"public const [^\r\n]+").Value.Should().Equal("public const " + signature);
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