using System.Reflection;

namespace MrKWatkins.DocGen.Model;

public sealed class Operator : Function
{
    public Operator(MethodInfo methodInfo)
        : base(methodInfo)
    {
    }

    public override string DisplayName => Name[3..];

    public new MethodInfo MemberInfo => (MethodInfo)base.MemberInfo;
}