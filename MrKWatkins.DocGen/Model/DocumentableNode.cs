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

    public MemberInfo MemberInfo { get; }

    public XmlDocId XmlDocId { get; }

    public override string FileName => MemberInfo.DocumentationFileName();

    public MemberDocumentation? Documentation { get; internal set; }
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