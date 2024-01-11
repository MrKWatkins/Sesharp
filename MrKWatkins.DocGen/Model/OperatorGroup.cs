using System.Reflection;

namespace MrKWatkins.DocGen.Model;

public sealed class OperatorGroup : MemberGroup<Operator, MethodInfo>
{
    public OperatorGroup(string name, [InstantHandle] IEnumerable<Operator> operators)
        : base(name, operators)
    {
    }

    public override string GroupFileName => Name;
}