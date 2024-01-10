namespace MrKWatkins.DocGen.Model;

public sealed class Namespace : ModelNode
{
    public Namespace(string name)
        : base(name)
    {
    }

    public new AssemblyDetails Parent => (AssemblyDetails)base.Parent;

    public IEnumerable<Type> Types => Children.OfType<Type>();
}