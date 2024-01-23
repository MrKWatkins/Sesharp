using System.Data;
using System.Reflection;
using System.Text.RegularExpressions;
using MrKWatkins.DocGen.TestAssembly.Properties;

namespace MrKWatkins.DocGen.Tests;

public sealed class PropertyInfoExtensionsTests
{
    [TestCase(nameof(PropertyModifiers.InitSetter), true)]
    [TestCase(nameof(PropertyModifiers.Normal), false)]
    public void HasInitSetter(string name, bool expected) => GetProperty<PropertyModifiers>(name).HasInitSetter().Should().Be(expected);

    [TestCase(typeof(string), nameof(string.Length), false)]
    [TestCase(typeof(List<string>), "Item", false)]
    [TestCase(typeof(DataRow), "Item", true)]
    public void HasPublicOrProtectedOverloads_PropertyInfo(Type type, string propertyName, bool expected)
    {
        var properties = type
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Where(p => p.Name == propertyName)
            .ToList();

        if (properties.Count == 0)
        {
            throw new InvalidOperationException("Could not find property.");
        }

        properties[0].HasPublicOrProtectedOverloads().Should().Be(expected);
    }

    [TestCase(nameof(PropertyVirtuality.Abstract), true)]
    [TestCase(nameof(PropertyVirtuality.Normal), false)]
    [TestCase(nameof(PropertyVirtuality.Virtual), false)]
    public void IsAbstract(string name, bool expected) => GetProperty<PropertyVirtuality>(name).IsAbstract().Should().Be(expected);

    [TestCase(nameof(PropertyVirtuality.Abstract), true)]
    [TestCase(nameof(PropertyVirtuality.Normal), false)]
    [TestCase(nameof(PropertyVirtuality.Virtual), true)]
    public void IsAbstractOrVirtual(string name, bool expected) => GetProperty<PropertyVirtuality>(name).IsAbstractOrVirtual().Should().Be(expected);

    [TestCase(typeof(string), nameof(string.Length), false)]
    [TestCase(typeof(List<string>), "Item", true)]
    public void IsIndexer(Type type, string propertyName, bool expected)
    {
        var property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                       ?? throw new InvalidOperationException("Could not find property.");

        property.IsIndexer().Should().Be(expected);
    }

    [TestCaseSource(nameof(PropertyVisibilityTestCases))]
    public void IsProtected(PropertyInfo property, bool _, bool expectedProtected) =>
        property.IsProtected().Should().Be(expectedProtected);

    [TestCaseSource(nameof(PropertyVisibilityTestCases))]
    public void IsPublic(PropertyInfo property, bool expectedPublic, bool _) =>
        property.IsPublic().Should().Be(expectedPublic);

    [TestCaseSource(nameof(PropertyVisibilityTestCases))]
    public void IsPublicOrProtected(PropertyInfo property, bool expectedPublic, bool expectedProtected) =>
        property.IsPublicOrProtected().Should().Be(expectedPublic || expectedProtected);

    [TestCase(nameof(PropertyModifiers.Required), true)]
    [TestCase(nameof(PropertyModifiers.Normal), false)]
    public void IsRequired(string name, bool expected) => GetProperty<PropertyModifiers>(name).IsRequired().Should().Be(expected);

    [Pure]
    public static IEnumerable<TestCaseData> PropertyVisibilityTestCases() =>
        typeof(PropertyVisibility)
            .GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Select(property =>
            {
                var isPublic = property.Name.Contains("Public");
                var isProtected = !isPublic && Regex.IsMatch(property.Name, "(?<!Private)Protected");
                return new TestCaseData(property, isPublic, isProtected);
            });

    [Pure]
    private static PropertyInfo GetProperty<T>(string name) =>
        typeof(T).GetProperty(name) ?? throw new InvalidOperationException($"Could not find property {name} on {typeof(T).DisplayName()}.");
}