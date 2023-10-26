using System.Reflection;

namespace MrKWatkins.DocGen.Model;

public abstract class Member<TMemberInfo> : DocumentableNode<TMemberInfo>
    where TMemberInfo : MemberInfo
{
    protected Member(TMemberInfo memberInfo)
        : base(memberInfo)
    {
    }

    public new Type Parent => (Type)base.Parent;
}