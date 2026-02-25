using System.Reflection;
using MrKWatkins.Sesharp.XmlDocumentation;

namespace MrKWatkins.Sesharp.Model;

public abstract class DocumentableNode(MemberInfo memberInfo) : OutputNode(memberInfo.Name)
{
    public MemberInfo MemberInfo { get; } = memberInfo;

    public XmlDocId XmlDocId { get; } = XmlDocId.Create(memberInfo);

    public override string FileName => MemberInfo.DocumentationFileName();

    public MemberDocumentation? Documentation { get; internal set; }

    public SourceLocation? SourceLocation { get; internal set; }
}

public abstract class DocumentableNode<TMemberInfo>(TMemberInfo memberInfo) : DocumentableNode(memberInfo)
    where TMemberInfo : MemberInfo
{
    public new TMemberInfo MemberInfo => (TMemberInfo)base.MemberInfo;
}