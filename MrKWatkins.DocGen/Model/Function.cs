using System.Reflection;

namespace MrKWatkins.DocGen.Model;

public abstract class Function<TMemberInfo> : Member<TMemberInfo>
    where TMemberInfo : MemberInfo
{
    protected Function(TMemberInfo memberInfo)
        : base(memberInfo)
    {
    }
}