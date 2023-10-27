using System.Reflection;
using MrKWatkins.DocGen.XmlDocumentation;

namespace MrKWatkins.DocGen.Model;

public abstract class DocumentableNode : ModelNode
{
    protected DocumentableNode(MemberInfo memberInfo)
        : base(memberInfo.Name)
    {
    }

    public virtual string DisplayName => Name;

    public virtual string FileName => Name.Replace('`', '-');

    public abstract string DocumentationKey { get; }

    public MemberDocumentation? Documentation { get; internal set; }
}

public abstract class DocumentableNode<TMemberInfo> : DocumentableNode
    where TMemberInfo : MemberInfo
{
    protected DocumentableNode(TMemberInfo memberInfo)
        : base(memberInfo)
    {
        MemberInfo = memberInfo;
    }

    public TMemberInfo MemberInfo
    {
        get => Properties.GetOrThrow<TMemberInfo>(nameof(MemberInfo));
        private init => Properties.Set(nameof(MemberInfo), value);
    }
}