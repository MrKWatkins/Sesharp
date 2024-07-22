using System.Reflection;

namespace MrKWatkins.Sesharp.Model;

public abstract class Member<TMemberInfo>(TMemberInfo methodInfo) : DocumentableNode<TMemberInfo>(methodInfo)
    where TMemberInfo : MemberInfo
{
    public Type Type => Ancestors.OfType<Type>().First();

    public virtual string MemberName => DisplayName;
}