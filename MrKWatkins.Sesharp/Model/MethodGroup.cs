using System.Reflection;

namespace MrKWatkins.Sesharp.Model;

public sealed class MethodGroup(string name, [InstantHandle] IEnumerable<Method> methods) : MemberGroup<Method, MethodInfo>(name, methods)
{
    public override string GroupFileName => Name;
}