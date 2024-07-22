
namespace MrKWatkins.Sesharp.Model;

public abstract class OutputNode(string name) : ModelNode(name)
{
    public virtual string DisplayName => Name;

    public virtual string MenuName => DisplayName;

    public abstract string FileName { get; }

}