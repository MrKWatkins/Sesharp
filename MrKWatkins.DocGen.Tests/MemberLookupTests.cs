using System.Reflection;

namespace MrKWatkins.DocGen.Tests;

public sealed class MemberLookupTests
{
    private readonly MemberLookup lookup = new(typeof(MemberLookup).Assembly);

    [TestCaseSource(nameof(GetTestCases))]
    public void Get(XmlDocId id, MemberInfo expectedMember, MemberLocation expectedLocation) => lookup.Get(id).Should().Be((expectedMember, expectedLocation));

    [Pure]
    public static IEnumerable<TestCaseData> GetTestCases()
    {
        static TestCaseData Create(string id, MemberInfo expectedMember, MemberLocation memberLocation) =>
            new TestCaseData(XmlDocId.Parse(id), expectedMember, memberLocation).SetName(id);

        // Types.
        yield return Create(
            "T:System.String",
            typeof(string),
            MemberLocation.System);

        yield return Create(
            "T:System.Collections.Generic.IEnumerable`1",
            typeof(IEnumerable<>),
            MemberLocation.System);
    }
}