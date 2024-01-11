using System.Reflection;
using MrKWatkins.DocGen.XmlDocumentation;

namespace MrKWatkins.DocGen.Model;

public abstract class DocumentableNode : OutputNode
{
    protected DocumentableNode(MemberInfo memberInfo)
        : base(memberInfo.Name)
    {
        MemberInfo = memberInfo;
        XmlDocId = XmlDocId.Create(memberInfo);
    }

    public MemberInfo MemberInfo
    {
        get => Properties.GetOrThrow<MemberInfo>(nameof(MemberInfo));
        private init => Properties.Set(nameof(MemberInfo), value);
    }

    public XmlDocId XmlDocId
    {
        get => Properties.GetOrThrow<XmlDocId>(nameof(XmlDocId));
        private init => Properties.Set(nameof(XmlDocId), value);
    }

    public override string FileName => MemberInfo.DocumentationFileName();

    public MemberDocumentation? Documentation
    {
        get => Properties.GetOrDefault<MemberDocumentation>(nameof(Documentation));
        internal set
        {
            if (value != null)
            {
                Properties.Set(nameof(Documentation), value);
            }
        }
    }
}

public abstract class DocumentableNode<TMemberInfo> : DocumentableNode
    where TMemberInfo : MemberInfo
{
    protected DocumentableNode(TMemberInfo memberInfo)
        : base(memberInfo)
    {
    }

    public new TMemberInfo MemberInfo => (TMemberInfo)base.MemberInfo;
}