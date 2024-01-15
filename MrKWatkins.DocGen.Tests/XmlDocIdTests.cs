using System.Collections;
using System.Reflection;
using Timer = System.Timers.Timer;

namespace MrKWatkins.DocGen.Tests;

public sealed class XmlDocIdTests : EqualityTestFixture
{
    [TestCaseSource(nameof(CreateTestCases))]
    public void Create(MemberInfo member, string expectedId)
    {
        var id = XmlDocId.Create(member);
        id.ToString().Should().Be(expectedId);
        id.Id.Should().Be(expectedId);
    }

    [Test]
    public void Create_UnsupportedMemberInfoThrows() => FluentActions.Invoking(() => XmlDocId.Create(new InvalidMemberInfo())).Should().Throw<NotSupportedException>();

    [Pure]
    public static IEnumerable<TestCaseData> CreateTestCases()
    {
        static TestCaseData Create(MemberInfo member, string expectedId) => new TestCaseData(member, expectedId).SetName($"{nameof(XmlDocId.Parse)}: {expectedId}");

        // Types.
        yield return Create(
            typeof(string),
            "T:System.String");

        yield return Create(
            typeof(IEnumerable<string>),
            "T:System.Collections.Generic.IEnumerable`1");

        yield return Create(
            typeof(Dictionary<int, string>),
            "T:System.Collections.Generic.Dictionary`2");

        yield return Create(
            typeof(Dictionary<int, string>.KeyCollection),
            "T:System.Collections.Generic.Dictionary`2.KeyCollection");

        // Fields.
        yield return Create(
            typeof(int).GetField(nameof(int.MaxValue))!,
            "F:System.Int32.MaxValue");

        // Properties.
        yield return Create(
            typeof(Exception).GetProperty(nameof(Exception.InnerException))!,
            "P:System.Exception.InnerException");

        yield return Create(
            typeof(string).GetProperty("Chars")!,
            "P:System.String.Chars(System.Int32)");

        yield return Create(
            typeof(ArrayList).GetProperty("Item")!,
            "P:System.Collections.ArrayList.Item(System.Int32)");

        yield return Create(
            typeof(Dictionary<,>).GetProperty("System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<TKey,TValue>>.IsReadOnly",
                BindingFlags.Instance | BindingFlags.NonPublic)!,
            "P:System.Collections.Generic.Dictionary`2.System#Collections#Generic#ICollection{System#Collections#Generic#KeyValuePair{TKey@TValue}}#IsReadOnly");

        // Methods.
        yield return Create(
            typeof(object).GetMethod(nameof(ToString))!,
            "M:System.Object.ToString");

        yield return Create(
            typeof(HashSet<>).GetMethod(nameof(HashSet<int>.CopyTo),
            [
                typeof(HashSet<>).GetGenericArguments()[0].MakeArrayType()
            ])!,
            "M:System.Collections.Generic.HashSet`1.CopyTo(`0[])");

        yield return Create(
            typeof(Dictionary<,>).GetMethod(nameof(Dictionary<int, int>.TryGetValue))!,
            "M:System.Collections.Generic.Dictionary`2.TryGetValue(`0,`1@)");

        yield return Create(
            typeof(Dictionary<,>).GetMethod("System.Collections.IDictionary.GetEnumerator", BindingFlags.Instance | BindingFlags.NonPublic)!,
            "M:System.Collections.Generic.Dictionary`2.System#Collections#IDictionary#GetEnumerator");

        yield return Create(
            typeof(CollectionExtensions).GetMethods().Single(m => m.Name == nameof(CollectionExtensions.GetValueOrDefault) && m.GetParameters().Length == 2),
            "M:System.Collections.Generic.CollectionExtensions.GetValueOrDefault``2(System.Collections.Generic.IReadOnlyDictionary{``0,``1},``0)");

        yield return Create(
            typeof(TestCases).GetMethod(nameof(TestCases.MultidimensionalArrayParameter))!,
            "M:MrKWatkins.DocGen.Tests.XmlDocIdTests.TestCases.MultidimensionalArrayParameter(System.Int32[,])");

        // Constructors.
        yield return Create(
            typeof(Dictionary<int, string>).GetConstructor(Type.EmptyTypes)!,
            "M:System.Collections.Generic.Dictionary`2.#ctor");

        yield return Create(
            typeof(Dictionary<int, string>).GetConstructor([typeof(int)])!,
            "M:System.Collections.Generic.Dictionary`2.#ctor(System.Int32)");

        yield return Create(
            typeof(Dictionary<int, string>).GetConstructor([typeof(IEqualityComparer<int>)])!,
            "M:System.Collections.Generic.Dictionary`2.#ctor(System.Collections.Generic.IEqualityComparer{System.Int32})");

        yield return Create(
            typeof(Dictionary<,>).GetConstructor(
            [
                typeof(IEqualityComparer<>).MakeGenericType(typeof(Dictionary<,>).GetGenericArguments()[0])
            ])!,
            "M:System.Collections.Generic.Dictionary`2.#ctor(System.Collections.Generic.IEqualityComparer{`0})");

        yield return Create(
            typeof(Dictionary<,>).GetConstructor(
            [
                typeof(IEnumerable<>).MakeGenericType(typeof(KeyValuePair<,>).MakeGenericType(typeof(Dictionary<,>).GetGenericArguments()))
            ])!,
            "M:System.Collections.Generic.Dictionary`2.#ctor(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{`0,`1}})");

        yield return Create(
            typeof(string).GetConstructor([typeof(char).MakePointerType()])!,
            "M:System.String.#ctor(System.Char*)");

        // Events.
        yield return Create(
            typeof(Timer).GetEvent(nameof(Timer.Elapsed))!,
            "E:System.Timers.Timer.Elapsed");

        // Operators.
        yield return Create(
            typeof(decimal).GetMethod("op_Implicit", [typeof(byte)])!,
            "M:System.Decimal.op_Implicit(System.Byte)~System.Decimal");

        yield return Create(
            typeof(decimal).GetMethod("op_Explicit", [typeof(double)])!,
            "M:System.Decimal.op_Explicit(System.Double)~System.Decimal");

        yield return Create(
            typeof(Half).GetMethods().Single(m => m.Name == "op_CheckedExplicit" && m.ReturnType == typeof(byte)),
            "M:System.Half.op_CheckedExplicit(System.Half)~System.Byte");
    }

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