using System.Reflection;

namespace MrKWatkins.Sesharp.Model;

public sealed class OperatorGroup(string name, [InstantHandle] IEnumerable<Operator> operators) : MemberGroup<Operator, MethodInfo>(name, operators)
{
    public override string GroupFileName => Name;
}