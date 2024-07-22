using MrKWatkins.Ast;

namespace MrKWatkins.Sesharp.Model;

public abstract class ModelNode(string name) : Node<ModelNode>
{
    public string Name { get; } = name;

    public sealed override string ToString() => $"{GetType().Name} {Name}";
}