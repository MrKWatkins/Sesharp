
namespace MrKWatkins.DocGen.Model;

public abstract class OutputNode : ModelNode
{
    protected OutputNode(string name)
        : base(name)
    {
    }

    public virtual string DisplayName => Name;

    public abstract string FileName { get; }
}