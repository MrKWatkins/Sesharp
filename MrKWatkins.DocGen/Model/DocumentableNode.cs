using System.Reflection;

namespace MrKWatkins.DocGen.Model;

public abstract class DocumentableNode<TMemberInfo> : ModelNode
    where TMemberInfo : MemberInfo
{
    protected DocumentableNode(TMemberInfo memberInfo)
        : base(memberInfo.Name)
    {
        MemberInfo = memberInfo;
    }

    public TMemberInfo MemberInfo
    {
        get => Properties.GetOrThrow<TMemberInfo>(nameof(MemberInfo));
        private init => Properties.Set(nameof(MemberInfo), value);
    }

    public virtual string DisplayName => Name;

    public virtual string FileName => Name.Replace('`', '-');

    public abstract string DocumentationKey { get; }
}