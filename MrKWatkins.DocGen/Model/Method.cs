using System.Reflection;

namespace MrKWatkins.DocGen.Model;

public sealed class Method : Function
{
    public Method(MethodInfo method)
        : base(method)
    {
    }

    public new MethodInfo MemberInfo => (MethodInfo)base.MemberInfo;
}