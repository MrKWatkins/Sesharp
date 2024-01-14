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

    [TestCase(typeof(TestSubClass), nameof(TestSubClass.TestAbstractMethod), false)]
    [TestCase(typeof(TestSubClass), nameof(TestSubClass.TestVirtualMethod), false)]
    [TestCase(typeof(TestSubClass), nameof(TestSubClass.TestMethod), true)]
    [TestCase(typeof(TestSubSubClass), nameof(TestSubSubClass.TestVirtualMethod), true)]
    [TestCase(typeof(TestSubSubClass), nameof(TestSubSubClass.TestMethod), true)]
    public void IsNew(Type type, string method, bool expected)
    {
        var methodInfo = type.GetMethod(method) ?? throw new InvalidOperationException($"Method {method} not found on type {type}.");

        methodInfo.IsNew().Should().Be(expected);
    }

    [TestCase(typeof(TestAbstractClass), nameof(TestAbstractClass.TestAbstractMethod), Virtuality.Abstract)]
    [TestCase(typeof(TestAbstractClass), nameof(TestAbstractClass.TestVirtualMethod), Virtuality.Virtual)]
    [TestCase(typeof(TestAbstractClass), nameof(TestAbstractClass.TestMethod), null)]
    [TestCase(typeof(TestSubClass), nameof(TestSubClass.TestAbstractMethod), Virtuality.SealedOverride)]
    [TestCase(typeof(TestSubClass), nameof(TestSubClass.TestVirtualMethod), Virtuality.Override)]
    [TestCase(typeof(TestSubClass), nameof(TestSubClass.TestMethod), Virtuality.New)]
    [TestCase(typeof(TestSubSubClass), nameof(TestSubSubClass.TestVirtualMethod), Virtuality.NewAbstract)]
    [TestCase(typeof(TestSubSubClass), nameof(TestSubSubClass.TestMethod), Virtuality.NewVirtual)]
    public void GetVirtuality(Type type, string method, Virtuality? expected)
    {
        var methodInfo = type.GetMethod(method) ?? throw new InvalidOperationException($"Method {method} not found on type {type}.");

        methodInfo.GetVirtuality().Should().Be(expected);
    }

#pragma warning disable CA1812
#pragma warning disable CA1822
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

    public abstract class TestAbstractClass
    {
        public abstract void TestAbstractMethod();

        public virtual void TestVirtualMethod()
        {
        }

        public void TestMethod()
        {
        }
    }

    public class TestSubClass : TestAbstractClass
    {
        public sealed override void TestAbstractMethod()
        {
        }

        public override void TestVirtualMethod()
        {
        }

        public new void TestMethod()
        {
        }
    }

    public abstract class TestSubSubClass : TestAbstractClass
    {
        public new abstract void TestVirtualMethod();

        public new virtual void TestMethod()
        {
        }
    }
#pragma warning restore CA1822
#pragma warning restore CA1812
}