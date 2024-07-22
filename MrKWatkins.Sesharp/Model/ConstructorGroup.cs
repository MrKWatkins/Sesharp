using System.Reflection;

namespace MrKWatkins.Sesharp.Model;

public sealed class ConstructorGroup([InstantHandle] IEnumerable<Constructor> constructors) : MemberGroup<Constructor, ConstructorInfo>("Constructors", constructors)
{
    public override string GroupFileName => "-ctor";
}