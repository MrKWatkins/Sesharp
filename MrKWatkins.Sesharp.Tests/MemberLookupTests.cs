using System.Reflection;
using MrKWatkins.Sesharp.TestAssembly.Fields;

namespace MrKWatkins.Sesharp.Tests;

public sealed class MemberLookupTests
{
    private readonly MemberLookup lookup = new(typeof(MemberLookup).Assembly);

    [TestCaseSource(nameof(GetTestCases))]
    public void Get(XmlDocId id, MemberInfo expectedMember, MemberLocation expectedLocation) => lookup.Get(id).Should().Equal((expectedMember, expectedLocation));

    [Test]
    public void Get_HiddenType()
    {
        var type = typeof(VisibilityMembers).GetNestedType("HiddenType", BindingFlags.NonPublic)!;
        var local = new MemberLookup(type.Assembly);
        local.Get(XmlDocId.Create(type)).Should().Equal((type, MemberLocation.Other));
    }

    [TestCase("Hidden", MemberLocation.Other)]
    [TestCase("ForDerived", MemberLocation.DocumentAssembly)]
    public void Get_MemberVisibility(string name, MemberLocation expected)
    {
        var local = new MemberLookup(typeof(VisibilityMembers).Assembly);
        var member = typeof(VisibilityMembers).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!;
        local.Get(XmlDocId.Create(member)).Should().Equal((member, expected));
    }

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