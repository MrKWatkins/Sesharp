namespace MrKWatkins.DocGen.Model;

public sealed class Namespace : ModelNode
{
    public Namespace(string name)
        : base(name)
    {
    }

    public new Assembly Parent => (Assembly)base.Parent;

    public IEnumerable<Type> Types => Children.OfType<Type>();
}