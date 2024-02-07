using System.Reflection;

namespace MrKWatkins.DocGen.Tests;

public sealed class XmlDocIdTests : EqualityTestFixture
{
    [Test]
    public void Create()
    {
        var id = XmlDocId.Create(typeof(Dictionary<,>));
        id.ToString().Should().Be("T:System.Collections.Generic.Dictionary`2");
        id.Id.Should().Be("T:System.Collections.Generic.Dictionary`2");
    }

    [Test]
    public void Create_UnsupportedMemberInfoThrows() => FluentActions.Invoking(() => XmlDocId.Create(new InvalidMemberInfo())).Should().Throw<NotSupportedException>();

    [TestCaseSource(nameof(EqualityTestCases))]
    public void Equality(XmlDocId x, object? y, bool expected) => AssertEqual(x, y, expected);

    [Pure]
    public static IEnumerable<TestCaseData> EqualityTestCases()
    {
        var type = XmlDocId.Create(typeof(string));
        var property = XmlDocId.Create(typeof(string).GetProperty(nameof(string.Length))!);

        yield return new TestCaseData(type, null, false).SetName("Equality: null");
        yield return new TestCaseData(type, type, true).SetName("Equality: Type reference equals");
        yield return new TestCaseData(type, XmlDocId.Create(typeof(string)), true).SetName("Equality: Type value equals");
        yield return new TestCaseData(property, property, true).SetName("Equality: Property reference equals");
        yield return new TestCaseData(property, XmlDocId.Create(typeof(string).GetProperty(nameof(string.Length))!), true).SetName("Equality: Property value equals");
        yield return new TestCaseData(type, XmlDocId.Create(typeof(int)), false).SetName("Equality: Different types");
        yield return new TestCaseData(type, XmlDocId.Create(typeof(string).GetProperty(nameof(string.Length))!), false).SetName("Equality: Different kind");
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private static class TestCases
    {
#pragma warning disable CA1814
        public static void MultidimensionalArrayParameter(int[,] parameter)
        {
        }
#pragma warning restore CA1814
    }

    private sealed class InvalidMemberInfo : MemberInfo
    {
        public override object[] GetCustomAttributes(bool inherit) => throw new NotSupportedException();

        public override object[] GetCustomAttributes(Type attributeType, bool inherit) => throw new NotSupportedException();

        public override bool IsDefined(Type attributeType, bool inherit) => throw new NotSupportedException();

        public override Type DeclaringType => throw new NotSupportedException();

        public override MemberTypes MemberType => throw new NotSupportedException();

        public override string Name => "Invalid";

        public override Type ReflectedType => throw new NotSupportedException();
    }
}