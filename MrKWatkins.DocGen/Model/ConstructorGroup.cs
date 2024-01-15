using System.Reflection;

namespace MrKWatkins.DocGen.Model;

public sealed class ConstructorGroup : MemberGroup<Constructor, ConstructorInfo>
{
    public ConstructorGroup([InstantHandle] IEnumerable<Constructor> constructors)
        : base("Constructors", constructors)
    {
    }

    public override string GroupFileName => "-ctor";
}