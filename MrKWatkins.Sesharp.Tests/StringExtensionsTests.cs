namespace MrKWatkins.Sesharp.Tests;

public sealed class StringExtensionsTests
{
    [TestCase("blah", "Blah")]
    [TestCase("Blah", "Blah")]
    public void Capitalize(string value, string expected) => value.Capitalize().Should().Equal(expected);
}