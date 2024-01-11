using System.Reflection;

namespace MrKWatkins.DocGen.Model;

public sealed class MethodGroup : MemberGroup<Method, MethodInfo>
{
    public MethodGroup(string name, [InstantHandle] IEnumerable<Method> methods)
        : base(name, methods)
    {
    }

    public override string GroupFileName => Name;
}