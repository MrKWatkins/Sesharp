using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using MrKWatkins.Sesharp.Markdown.Generation;
using MrKWatkins.Sesharp.Model;
using MrKWatkins.Sesharp.Testing;
using MrKWatkins.Sesharp.XmlDocumentation;

namespace MrKWatkins.Sesharp.IntegrationTests;

public sealed class VitePressIntegrationTests : TestFixture
{
    private readonly Dictionary<string, string> pages = new(StringComparer.Ordinal);
    private const string Base = "/sesharp-test/";
    private const string FixturePath = "API/MrKWatkins.Sesharp.TestAssembly.VitePress/";

    [OneTimeSetUp]
    public async Task BuildSite()
    {
        var directory = Directory.CreateTempSubdirectory("sesharp-vitepress-");
        try
        {
            var documentation = Documentation.Load(new RealFileSystem(), TestAssemblyXmlPath);
            var assembly = AssemblyParser.Parse(TestAssembly, documentation);
            AssemblyMarkdownGenerator.Generate(new RealFileSystem(), assembly, Path.Combine(directory.FullName, "API"), "https://github.com/example/library", sourceRef: "v1.2.3");
            await File.WriteAllTextAsync(Path.Combine(directory.FullName, "index.md"), "# API tests\n\n[API](API/index.md)\n");
            Directory.CreateDirectory(Path.Combine(directory.FullName, ".vitepress"));
            await File.WriteAllTextAsync(Path.Combine(directory.FullName, ".vitepress/config.mjs"),
                "export default { title: 'API tests', base: '/sesharp-test/', ignoreDeadLinks: false, themeConfig: { search: { provider: 'local' } } }");

            await using var image = new ImageFromDockerfileBuilder()
                .WithDockerfileDirectory(Path.Combine(AppContext.BaseDirectory, "VitePress"))
                .Build();
            await image.CreateAsync();

            await using var container = new ContainerBuilder(image)
                .WithResourceMapping(directory, "/app/docs")
                .WithCommand("sh", "-c", "node_modules/.bin/vitepress build docs && echo SESHARP_BUILD_COMPLETE")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("SESHARP_BUILD_COMPLETE",
                    options => options.WithMode(WaitStrategyMode.OneShot)))
                .Build();
            await container.StartAsync();

            foreach (var markdown in Directory.EnumerateFiles(directory.FullName, "*.md", SearchOption.AllDirectories))
            {
                var htmlPath = Path.ChangeExtension(Path.GetRelativePath(directory.FullName, markdown), ".html").Replace('\\', '/');
                var bytes = await container.ReadFileAsync($"/app/docs/.vitepress/dist/{htmlPath}");
                pages.Add(htmlPath, Encoding.UTF8.GetString(bytes));
            }
        }
        finally
        {
            directory.Delete(true);
        }
    }

    [Test]
    public void GenericHeading() => pages[FixturePath + "VitePressClass-T/index.html"].Should().Contain("VitePressClass&lt;T&gt; Class");

    [Test]
    public void LiteralText()
    {
        var html = pages[FixturePath + "VitePressClass-T/index.html"];
        HttpUtility.HtmlDecode(html).Should().Contain("Literal <tag> & {{literal}} {value} | text.");
        html.Should().Contain("<code>a|b</code>");
    }

    [Test]
    public void NestedTypes()
    {
        pages[FixturePath + "VitePressClass-T/Nested-T/index.html"].Should().Contain("Nested");
        pages[FixturePath + "AnotherClass/Nested/index.html"].Should().Contain("Nested");
    }

    [Test]
    public void SourceRef() => pages[FixturePath + "VitePressClass-T/index.html"].Should().Contain("/blob/v1.2.3/");

    [Test]
    public void InheritedFrameworkDocumentation() => pages["API/MrKWatkins.Sesharp.TestAssembly.InheritDoc/InheritDocBclStruct/Parse.html"].Should().Contain("Parses a string into a value.");

    [Test]
    public void EveryInternalAnchorExists()
    {
        foreach (var (path, html) in pages)
        {
            var source = new Uri($"https://docs.invalid{Base}{path}");
            foreach (Match match in Regex.Matches(html, "href=\"([^\"]+)\""))
            {
                var target = new Uri(source, HttpUtility.HtmlDecode(match.Groups[1].Value));
                if (target.Host != source.Host || target.Fragment.Length == 0)
                {
                    continue;
                }
                target.AbsolutePath.Should().StartWith(Base);
                var targetPath = Uri.UnescapeDataString(target.AbsolutePath[Base.Length..]);
                if (targetPath.EndsWith('/'))
                {
                    targetPath += "index.html";
                }
                else if (!Path.HasExtension(targetPath))
                {
                    targetPath += ".html";
                }
                pages.Should().ContainKey(targetPath);
                var targetHtml = pages[targetPath];
                var anchor = Uri.UnescapeDataString(target.Fragment[1..]);
                var ids = Regex.Matches(targetHtml, "id=\"([^\"]+)\"").Select(m => HttpUtility.HtmlDecode(m.Groups[1].Value));
                ids.Should().Contain(anchor);
            }
        }
    }
}