using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using MrKWatkins.Sesharp.Markdown.Generation;
using MrKWatkins.Sesharp.Model;
using MrKWatkins.Sesharp.Testing;
using MrKWatkins.Sesharp.XmlDocumentation;

namespace MrKWatkins.Sesharp.IntegrationTests;

public sealed partial class MkDocsIntegrationTests : TestFixture
{
    [Test]
    public async Task Generated_Html_Has_No_Warnings_And_Correct_Navigation()
    {
        using var tempDir = new TempDirectory();

        // Generate markdown documentation from the test assembly.
        var documentation = Documentation.Load(new RealFileSystem(), TestAssemblyXmlPath);
        var assemblyDetails = AssemblyParser.Parse(TestAssembly, documentation);

        var docsDir = Path.Combine(tempDir.Path, "docs");
        var apiDir = Path.Combine(docsDir, "API");
        Directory.CreateDirectory(apiDir);

        AssemblyMarkdownGenerator.Generate(new RealFileSystem(), assemblyDetails, apiDir);

        // Create a minimal index.md so MkDocs has a landing page.
        await File.WriteAllTextAsync(Path.Combine(docsDir, "index.md"), "# Test\n");

        // Write the hooks.py file.
        await File.WriteAllTextAsync(Path.Combine(tempDir.Path, "hooks.py"), HooksPy);

        // Write the global_state.py file required by hooks.
        await File.WriteAllTextAsync(Path.Combine(tempDir.Path, "global_state.py"), GlobalStatePy);

        // Write a minimal mkdocs.yml.
        await File.WriteAllTextAsync(Path.Combine(tempDir.Path, "mkdocs.yml"), MkDocsYml);

        // Run mkdocs build.
        var (exitCode, stdout, stderr) = await RunMkDocsBuild(tempDir.Path);

        // Collect warnings from stderr.
        var warnings = stderr
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Where(line => line.Contains("WARNING", StringComparison.OrdinalIgnoreCase))
            .ToList();

        exitCode.Should().Equal(0);
        warnings.Should().BeEmpty();

        // Read the built index.html to verify navigation entries.
        var siteDir = Path.Combine(tempDir.Path, "site");
        var indexHtml = await File.ReadAllTextAsync(Path.Combine(siteDir, "index.html"));

        // Extract navigation entries from the HTML.
        var navEntries = ExtractNavEntries(indexHtml);

        await Verify(navEntries);
    }

    private static List<string> ExtractNavEntries(string html)
    {
        // MkDocs Material renders nav entries inside <span class="md-ellipsis">...</span>.
        var matches = NavEntryRegex().Matches(html);
        return matches
            .Select(m => m.Groups[1].Value.Trim())
            .Where(entry => !string.IsNullOrWhiteSpace(entry))
            .Distinct()
            .Order()
            .ToList();
    }

    [GeneratedRegex("""<span class="md-ellipsis">\s*([^<]+?)\s*</span>""")]
    private static partial Regex NavEntryRegex();

    private static async Task<(int ExitCode, string Stdout, string Stderr)> RunMkDocsBuild(string workingDirectory)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "mkdocs",
            Arguments = "build",
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start mkdocs.");

        var stdoutBuilder = new StringBuilder();
        var stderrBuilder = new StringBuilder();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null) stdoutBuilder.AppendLine(e.Data);
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null) stderrBuilder.AppendLine(e.Data);
        };

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        return (process.ExitCode, stdoutBuilder.ToString(), stderrBuilder.ToString());
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
