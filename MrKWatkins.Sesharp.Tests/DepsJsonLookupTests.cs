namespace MrKWatkins.Sesharp.Tests;

public sealed class DepsJsonLookupTests
{
    [Test]
    public void TryLoad_ReturnNullIfNoDepsFile()
    {
        var result = DepsJsonLookup.TryLoad("/nonexistent/path/to/assembly.dll");
        result.Should().BeNull();
    }

    [Test]
    public void TryLoad_ParsesDepsJson()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"sesharp-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var assemblyPath = Path.Combine(tempDir, "Test.dll");
            var depsPath = Path.Combine(tempDir, "Test.deps.json");

            // Write a minimal deps.json with a package whose assembly version differs from package version.
            File.WriteAllText(depsPath, """
                {
                  "runtimeTarget": { "name": ".NETCoreApp,Version=v10.0" },
                  "targets": {
                    ".NETCoreApp,Version=v10.0": {
                      "SomePackage/2.0.0": {
                        "runtime": {
                          "lib/net10.0/SomeAssembly.dll": {
                            "assemblyVersion": "1.0.0.0",
                            "fileVersion": "2.0.0.0"
                          }
                        }
                      }
                    }
                  },
                  "libraries": {
                    "SomePackage/2.0.0": {
                      "type": "package",
                      "path": "somepackage/2.0.0"
                    }
                  }
                }
                """);

            var lookup = DepsJsonLookup.TryLoad(assemblyPath);
            lookup.Should().NotBeNull();

            var nugetCache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
            var expectedPath = Path.Combine(nugetCache, "somepackage", "2.0.0", "lib", "net10.0", "SomeAssembly.dll");

            lookup!.Resolve("SomeAssembly").Should().Equal(expectedPath);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public void Resolve_ReturnsNullForUnknownAssembly()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"sesharp-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var assemblyPath = Path.Combine(tempDir, "Test.dll");
            var depsPath = Path.Combine(tempDir, "Test.deps.json");

            File.WriteAllText(depsPath, """
                {
                  "runtimeTarget": { "name": ".NETCoreApp,Version=v10.0" },
                  "targets": {
                    ".NETCoreApp,Version=v10.0": {}
                  },
                  "libraries": {}
                }
                """);

            var lookup = DepsJsonLookup.TryLoad(assemblyPath);
            lookup.Should().NotBeNull();
            lookup!.Resolve("NonExistent").Should().BeNull();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public void Resolve_IsCaseInsensitive()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"sesharp-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var assemblyPath = Path.Combine(tempDir, "Test.dll");
            var depsPath = Path.Combine(tempDir, "Test.deps.json");

            File.WriteAllText(depsPath, """
                {
                  "runtimeTarget": { "name": ".NETCoreApp,Version=v10.0" },
                  "targets": {
                    ".NETCoreApp,Version=v10.0": {
                      "MyPackage/1.0.0": {
                        "runtime": {
                          "lib/net10.0/MyAssembly.dll": {}
                        }
                      }
                    }
                  },
                  "libraries": {
                    "MyPackage/1.0.0": {
                      "type": "package",
                      "path": "mypackage/1.0.0"
                    }
                  }
                }
                """);

            var lookup = DepsJsonLookup.TryLoad(assemblyPath);
            lookup.Should().NotBeNull();
            lookup!.Resolve("myassembly").Should().NotBeNull();
            lookup.Resolve("MYASSEMBLY").Should().NotBeNull();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public void TryLoad_SkipsNonPackageLibraries()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"sesharp-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var assemblyPath = Path.Combine(tempDir, "Test.dll");
            var depsPath = Path.Combine(tempDir, "Test.deps.json");

            File.WriteAllText(depsPath, """
                {
                  "runtimeTarget": { "name": ".NETCoreApp,Version=v10.0" },
                  "targets": {
                    ".NETCoreApp,Version=v10.0": {
                      "MyProject/1.0.0": {
                        "runtime": {
                          "MyProject.dll": {}
                        }
                      }
                    }
                  },
                  "libraries": {
                    "MyProject/1.0.0": {
                      "type": "project",
                      "path": "myproject/1.0.0"
                    }
                  }
                }
                """);

            var lookup = DepsJsonLookup.TryLoad(assemblyPath);
            lookup.Should().NotBeNull();
            lookup!.Resolve("MyProject").Should().BeNull();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
