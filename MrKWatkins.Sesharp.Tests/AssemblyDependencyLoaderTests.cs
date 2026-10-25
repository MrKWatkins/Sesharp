using System.Reflection;
using System.Runtime.Loader;

namespace MrKWatkins.Sesharp.Tests;

public sealed class AssemblyDependencyLoaderTests
{
    [Test]
    public void LoadWithDependencies_UsesDocumentedAssemblyContext()
    {
        var hostAst = Assembly.Load("MrKWatkins.Ast");
        var context = new AssemblyLoadContext(nameof(LoadWithDependencies_UsesDocumentedAssemblyContext), isCollectible: true);
        try
        {
            var isolatedAst = context.LoadFromAssemblyPath(hostAst.Location);
            var documentAssembly = context.LoadFromAssemblyPath(typeof(MemberLookup).Assembly.Location);

            var result = AssemblyDependencyLoader.LoadWithDependencies(documentAssembly, documentAssembly.Location);

            result.Should().Contain(isolatedAst);
            result.Should().NotContain(hostAst);
            result.Should().Contain(typeof(object).Assembly);
        }
        finally
        {
            context.Unload();
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public void FindAssembly_UsesAdjacentAssembly(bool hasDepsFile)
    {
        var directory = Directory.CreateTempSubdirectory(nameof(AssemblyDependencyLoaderTests));
        try
        {
            var adjacent = Path.Combine(directory.FullName, "Example.Dependency.dll");
            File.WriteAllBytes(adjacent, []);
            var assemblyPath = Path.Combine(directory.FullName, "Example.dll");
            if (hasDepsFile)
            {
                File.WriteAllText(Path.ChangeExtension(assemblyPath, ".deps.json"), """
                    {
                      "targets": {
                        "net10.0": {
                          "Example.Package/9.0.0": {
                            "runtime": { "lib/net10.0/Example.Dependency.dll": {} }
                          }
                        }
                      },
                      "libraries": {
                        "Example.Package/9.0.0": { "type": "package", "path": "example.package/9.0.0" }
                      }
                    }
                    """);
            }
            var lookup = DepsJsonLookup.TryLoad(assemblyPath);

            var result = AssemblyDependencyLoader.FindAssembly(new AssemblyName("Example.Dependency"), lookup, directory.FullName);

            result.Should().Equal(adjacent);
        }
        finally
        {
            directory.Delete(true);
        }
    }
}