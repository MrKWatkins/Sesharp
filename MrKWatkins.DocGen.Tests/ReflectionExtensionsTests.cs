using System.Data;
using System.Reflection;
using MrKWatkins.Ast.Position;

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

    [TestCase(0, null)]
    [TestCase(1, ParameterKind.In)]
    [TestCase(2, ParameterKind.Out)]
    [TestCase(3, ParameterKind.Ref)]
    [TestCase(4, ParameterKind.Params)]
    public void GetKind(int parameterIndex, ParameterKind? expected)
    {
        var methodInfo = typeof(TestParameterClass).GetMethod(nameof(TestParameterClass.TestMethod))
                         ?? throw new InvalidOperationException($"Method {nameof(TestParameterClass.TestMethod)} not found on type {nameof(TestParameterClass)}.");

        methodInfo.GetParameters()[parameterIndex].GetKind().Should().Be(expected);
    }

    [TestCase(typeof(string), nameof(string.Length), false)]
    [TestCase(typeof(List<string>), "Item", true)]
    public void IsIndexer(Type type, string propertyName, bool expected)
    {
        var property = type
            .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Could not find property.");

        property.IsIndexer().Should().Be(expected);
    }

    [TestCase(typeof(int), nameof(int.ToString), true)]
    [TestCase(typeof(object), nameof(int.ToString), false)]
    [TestCase(typeof(MemoryStream), nameof(MemoryStream.Flush), false)]
    [TestCase(typeof(SourceFilePosition<,>), "Combine", false)]
    public void HasPublicOrProtectedOverloads_MethodInfo(Type type, string methodName, bool expected)
    {
        var methods = type
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Where(m => m.Name == methodName)
            .ToList();

        if (methods.Count == 0)
        {
            throw new InvalidOperationException("Could not find method.");
        }

        methods[0].HasPublicOrProtectedOverloads().Should().Be(expected);
    }

    [TestCase(typeof(string), true)]
    [TestCase(typeof(object), false)]
    public void HasPublicOrProtectedOverloads_ConstructorInfo(Type type, bool expected)
    {
        var constructors = type
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .ToList();

        if (constructors.Count == 0)
        {
            throw new InvalidOperationException("Could not find constructor.");
        }

        constructors[0].HasPublicOrProtectedOverloads().Should().Be(expected);
    }

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

    public abstract class TestParameterClass
    {
        public abstract void TestMethod(string normal, in string @in, out string @out, ref string @ref, params string[] @params);
    }
#pragma warning restore CA1822
#pragma warning restore CA1812
}