using System.Reflection;

namespace MrKWatkins.Sesharp.Model;

public abstract class MemberGroup<TMember, TMemberInfo> : OutputNode
    where TMember : DocumentableNode<TMemberInfo>
    where TMemberInfo : MemberInfo
{
    protected MemberGroup(string name, [InstantHandle] IEnumerable<TMember> members)
        : base(name)
    {
        Children.Add(members);
    }

    public abstract string GroupFileName { get; }

    public override string FileName => $"{Type.MemberInfo.BuildBaseFilename()}.{GroupFileName}.md";

    public new Type Parent => (Type)base.Parent;

    public Type Type => Parent;

    public IReadOnlyList<TMember> Members => Children.OfType<TMember>().ToList();
}