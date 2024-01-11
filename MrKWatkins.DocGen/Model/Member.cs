using System.Reflection;

namespace MrKWatkins.DocGen.Model;

public abstract class Member<TMemberInfo> : DocumentableNode<TMemberInfo>
    where TMemberInfo : MemberInfo
{
    protected Member(TMemberInfo methodInfo)
        : base(methodInfo)
    {
    }

    public Type Type => Ancestors.OfType<Type>().First();

    public virtual string MemberName => DisplayName;
}