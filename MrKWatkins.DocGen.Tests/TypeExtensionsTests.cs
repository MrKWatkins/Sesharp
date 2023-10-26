namespace MrKWatkins.DocGen.Tests;

public class TypeExtensionsTests
{
    [TestCase(typeof(string), "String")]
    [TestCase(typeof(int), "Int32")]
    [TestCase(typeof(List<string>), "List<String>")]
    [TestCase(typeof(Dictionary<string, int>), "Dictionary<String, Int32>")]
    public void DisplayName(Type type, string expected) => type.DisplayName().Should().Be(expected);

    [TestCase(typeof(TestClass), false)]
    [TestCase(typeof(TestRecord), true)]
    public void IsRecord(Type type, bool expected) => type.IsRecord().Should().Be(expected);

#pragma warning disable CA1812
    private sealed class TestClass;

    private sealed record TestRecord;
#pragma warning restore CA1812
}