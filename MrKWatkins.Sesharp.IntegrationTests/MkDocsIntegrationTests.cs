using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using MrKWatkins.Sesharp.Markdown.Generation;
using MrKWatkins.Sesharp.Model;
using MrKWatkins.Sesharp.Testing;
using MrKWatkins.Sesharp.XmlDocumentation;

namespace MrKWatkins.Sesharp.IntegrationTests;

public sealed class MkDocsIntegrationTests : TestFixture
{
    private static string? siteDirectory;
    private static TempDirectory? tempDirectory;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        tempDirectory = new TempDirectory();
        var tempPath = tempDirectory.Path;

        // Generate Markdown documentation from the test assembly.
        var documentation = Documentation.Load(new RealFileSystem(), TestAssemblyXmlPath);
        var assemblyDetails = AssemblyParser.Parse(TestAssembly, documentation);

        var docsDir = Path.Combine(tempPath, "docs");
        var apiDir = Path.Combine(docsDir, "API");
        Directory.CreateDirectory(apiDir);

        AssemblyMarkdownGenerator.Generate(new RealFileSystem(), assemblyDetails, apiDir);

        await File.WriteAllTextAsync(Path.Combine(docsDir, "index.md"), "# Test\n");
        await File.WriteAllTextAsync(Path.Combine(tempPath, "hooks.py"), HooksPy);
        await File.WriteAllTextAsync(Path.Combine(tempPath, "global_state.py"), GlobalStatePy);
        await File.WriteAllTextAsync(Path.Combine(tempPath, "mkdocs.yml"), MkDocsYml);

        // Build the Docker image from the Dockerfile in this project.
        await using var image = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(CommonDirectoryPath.GetCallerFileDirectory(), string.Empty)
            .WithDeleteIfExists(false)
            .Build();

        await image.CreateAsync();

        // Run MKDocs build inside the container with the temp dir bind-mounted.
        await using var container = new ContainerBuilder(image)
            .WithBindMount(tempPath, "/docs", AccessMode.ReadWrite)
            .WithWorkingDirectory("/docs")
            .WithCommand("build", "--strict")
            .Build();

        await container.StartAsync();

        var exitCode = await container.GetExitCodeAsync();
        if (exitCode != 0)
        {
            var (stdout, stderr) = await container.GetLogsAsync();
            throw new InvalidOperationException($"mkdocs build failed (exit code {exitCode}):\n{stderr}\n{stdout}");
        }

        siteDirectory = Path.Combine(tempPath, "site");
    }

    [OneTimeTearDown]
    public void OneTimeTearDown() => tempDirectory?.Dispose();

    [Test]
    public Task MkDocsClass_Page() =>
        VerifyPage("API/MrKWatkins.Sesharp.TestAssembly.MkDocs/MkDocsClass/index.html");

    [Test]
    public Task MkDocsClass_Constructors_Page() =>
        VerifyPage("API/MrKWatkins.Sesharp.TestAssembly.MkDocs/MkDocsClass/-ctor/index.html");

    [Test]
    public Task MkDocsClass_ToStruct_Page() =>
        VerifyPage("API/MrKWatkins.Sesharp.TestAssembly.MkDocs/MkDocsClass/ToStruct/index.html");

    [Test]
    public Task MkDocsClass_GetValue_Page() =>
        VerifyPage("API/MrKWatkins.Sesharp.TestAssembly.MkDocs/MkDocsClass/GetValue/index.html");

    [Test]
    public Task MkDocsGenericClass_Page() =>
        VerifyPage("API/MrKWatkins.Sesharp.TestAssembly.MkDocs/MkDocsGenericClass-T/index.html");

    [Test]
    public Task MkDocsGenericClass_Process_Page() =>
        VerifyPage("API/MrKWatkins.Sesharp.TestAssembly.MkDocs/MkDocsGenericClass-T/Process/index.html");

    [Test]
    public Task IMkDocsInterface_Page() =>
        VerifyPage("API/MrKWatkins.Sesharp.TestAssembly.MkDocs/IMkDocsInterface/index.html");

    [Test]
    public Task MkDocsStruct_Page() =>
        VerifyPage("API/MrKWatkins.Sesharp.TestAssembly.MkDocs/MkDocsStruct/index.html");

    private static async Task VerifyPage(string relativePath)
    {
        var html = await File.ReadAllTextAsync(Path.Combine(siteDirectory!, relativePath));
        await Verify(html).UseMethodName(TestContext.CurrentContext.Test.MethodName!);
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"sesharp-integration-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            try
            {
                Directory.Delete(Path, true);
            }
            catch
            {
                // Best effort cleanup.
            }
        }
    }

    private const string MkDocsYml =
        """
        site_name: Test
        theme:
          name: material
          features:
            - navigation.sections
            - navigation.indexes
        plugins:
          - search
          - mkdocs-simple-hooks:
              hooks:
                on_nav: "hooks:on_nav"
                on_page_context: "hooks:on_page_context"
        markdown_extensions:
          - attr_list
        """;

    private const string HooksPy =
        """
        import html
        import logging
        import os
        import yaml
        from global_state import global_state
        logger = logging.getLogger("mkdocs")
        def on_nav(nav, config, files):
            _fix_nav_titles(nav.items, config)
            global_state["nav_tree"] = nav.items
            logger.log(logging.INFO, "Captured navigation tree.")

        def on_page_context(context, page, config, nav):
            if page.title and ('<' in page.title or '>' in page.title):
                page.title = html.escape(page.title)

        def _fix_nav_titles(items, config):
            docs_dir = config['docs_dir']
            for item in items:
                if item.is_section:
                    for child in item.children:
                        if child.is_page and child.file.name == 'index':
                            title = _read_front_matter_title(os.path.join(docs_dir, child.file.src_path))
                            if title:
                                item.title = html.escape(title)
                            break
                    if item.children:
                        _fix_nav_titles(item.children, config)
                elif item.is_page:
                    title = _read_front_matter_title(os.path.join(docs_dir, item.file.src_path))
                    if title:
                        item.title = html.escape(title)

        def _read_front_matter_title(path):
            try:
                with open(path, 'r') as f:
                    content = f.read()
                if content.startswith('---'):
                    end = content.index('---', 3)
                    front_matter = yaml.safe_load(content[3:end])
                    if isinstance(front_matter, dict):
                        return front_matter.get('title')
            except Exception:
                pass
            return None
        """;

    private const string GlobalStatePy =
        """
        global_state = {
            "nav_tree": None
        }
        """;
}
