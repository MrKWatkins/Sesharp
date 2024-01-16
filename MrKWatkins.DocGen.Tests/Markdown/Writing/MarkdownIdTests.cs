using System.Data;
using System.Reflection;
using MrKWatkins.DocGen.Markdown.Writing;

namespace MrKWatkins.DocGen.Tests.Markdown.Writing;

public sealed class MarkdownIdTests
{
    [TestCaseSource(nameof(FromTextTestCases))]
    public void FromText(string title, string expectedId) => MarkdownId.FromText(title).Id.Should().Be(expectedId);

    [Pure]
    public static IEnumerable<TestCaseData> FromTextTestCases()
    {
        yield return new TestCaseData("Single", "single");
        yield return new TestCaseData("Two Words", "two-words");
    }

    [TestCaseSource(nameof(FromMemberTestCases))]
    public void FromMember(MemberInfo member, string expectedId) => MarkdownId.FromMember(member).Id.Should().Be(expectedId);

    [Pure]
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public static IEnumerable<TestCaseData> FromMemberTestCases()
    {
        yield return new TestCaseData(
            typeof(string).GetMethod(nameof(string.Compare), [typeof(string), typeof(string)]) ?? throw new InvalidOperationException("Method not found."),
            "system-string-compare(system-string-system-string)");

        yield return new TestCaseData(
            typeof(int).GetMethod(nameof(int.ToString), []) ?? throw new InvalidOperationException("Method not found."),
            "system-int32-tostring");

        yield return new TestCaseData(
            typeof(Activator).GetMethod(nameof(Activator.CreateInstance), []) ?? throw new InvalidOperationException("Method not found."),
            "system-activator-createinstance-1");

        yield return new TestCaseData(
            typeof(List<>).GetMethod(nameof(List<string>.IndexOf), [typeof(List<>).GetGenericArguments()[0], typeof(int)]) ?? throw new InvalidOperationException("Method not found."),
            "system-collections-generic-list-1-indexof(-0-system-int32)");

        yield return new TestCaseData(
            typeof(Enumerable).GetMethods().FirstOrDefault(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2) ?? throw new InvalidOperationException("Method not found."),
            "system-linq-enumerable-contains-1(system-collections-generic-ienumerable((-0))-0)");

        yield return new TestCaseData(
            typeof(Enumerable).GetMethods().FirstOrDefault(m => m.Name == nameof(Enumerable.SingleOrDefault) && m.GetParameters().Length == 3) ?? throw new InvalidOperationException("Method not found."),
            "system-linq-enumerable-singleordefault-1(system-collections-generic-ienumerable((-0))-system-func((-0-system-boolean))-0)");

        yield return new TestCaseData(
            typeof(Interlocked).GetMethod(nameof(Interlocked.CompareExchange), [typeof(int).MakeByRefType(), typeof(int), typeof(int)]) ?? throw new InvalidOperationException("Method not found."),
            "system-threading-interlocked-compareexchange(system-int32@-system-int32-system-int32)");

        yield return new TestCaseData(
            typeof(string).GetMethod(nameof(string.Concat), [typeof(string[])]) ?? throw new InvalidOperationException("Method not found."),
            "system-string-concat(system-string())");

        yield return new TestCaseData(
            typeof(string).GetConstructors()
                .FirstOrDefault(c =>
                    {
                        var parameters = c.GetParameters();
                        return parameters is [{ ParameterType.IsPointer: true }] && parameters[0].ParameterType.GetElementType()! == typeof(char);
                    }) ?? throw new InvalidOperationException("Constructor not found."),
            "system-string-ctor(system-char*)");

        yield return new TestCaseData(
            typeof(DataRow).GetProperty("Item", [typeof(int)])?? throw new InvalidOperationException("Indexer not found."),
            "system-data-datarow-item(system-int32)");
    }

    [Test]
    public void Addition()
    {
        var prefix = MarkdownId.FromText("prefix-id");
        var suffix = MarkdownId.FromText("suffix-id");

        var added = prefix + suffix;
        added.Id.Should().Be("prefix-id-suffix-id");
    }
}