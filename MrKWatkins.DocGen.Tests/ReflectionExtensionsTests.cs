namespace MrKWatkins.DocGen.Tests;

public class ReflectionExtensionsTests
{
    [TestCase(typeof(string), "String")]
    [TestCase(typeof(int), "Int32")]
    [TestCase(typeof(List<string>), "List<String>")]
    [TestCase(typeof(Dictionary<string, int>), "Dictionary<String, Int32>")]
    [TestCase(typeof(ITestContravariant<>), "ITestContravariant<in T>")]
    [TestCase(typeof(ITestCovariant<>), "ITestCovariant<out T>")]
    public void DisplayName(Type type, string expected) => type.DisplayName().Should().Be(expected);

    [TestCase(typeof(TestClass), false)]
    [TestCase(typeof(TestRecord), true)]
    public void IsRecord(Type type, bool expected) => type.IsRecord().Should().Be(expected);

    [TestCase(typeof(ReflectionExtensionsTests), typeof(ReflectionExtensionsTests))]
    [TestCase(typeof(NestedType), typeof(ReflectionExtensionsTests), typeof(NestedType))]
    [TestCase(typeof(NestedType.ReallyNestedType), typeof(ReflectionExtensionsTests), typeof(NestedType), typeof(NestedType.ReallyNestedType))]
    public void EnumerateTypeAndParentTypes(Type type, params Type[] expected) => type.EnumerateNestedTypes().Should().BeEquivalentTo(expected);

#pragma warning disable CA1812
    private sealed class TestClass;

    private sealed record TestRecord;

    // ReSharper disable once UnusedTypeParameter
    private interface ITestContravariant<in T>;

    // ReSharper disable once UnusedTypeParameter
    private interface ITestCovariant<out T>;

    public static class NestedType
    {
        public static class ReallyNestedType;
    }
#pragma warning restore CA1812
}