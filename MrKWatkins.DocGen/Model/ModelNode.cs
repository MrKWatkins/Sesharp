using MrKWatkins.Ast;

namespace MrKWatkins.DocGen.Model;

public abstract class ModelNode : Node<ModelNode>
{
    protected ModelNode(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public sealed override string ToString() => $"{GetType().Name} {Name}";
}