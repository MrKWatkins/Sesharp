using System.Reflection;
using MrKWatkins.Sesharp.TestAssembly.Properties;

namespace MrKWatkins.Sesharp.Testing;

public abstract class TestFixture
{
    protected static Assembly TestAssembly => typeof(PropertyIndexer).Assembly;

    protected static string TestAssemblyXmlPath => TestAssembly.Location.Replace(".dll", ".xml", StringComparison.OrdinalIgnoreCase);
}