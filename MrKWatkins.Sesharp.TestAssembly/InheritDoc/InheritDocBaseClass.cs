namespace MrKWatkins.Sesharp.TestAssembly.InheritDoc;

/// <summary>A base class with documented virtual members.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public abstract class InheritDocBaseClass
{
    /// <summary>A virtual method on the base class.</summary>
    /// <returns>A value.</returns>
    public virtual int VirtualMethod() => 0;

    /// <summary>A virtual property on the base class.</summary>
    public virtual int VirtualProperty => 0;
}