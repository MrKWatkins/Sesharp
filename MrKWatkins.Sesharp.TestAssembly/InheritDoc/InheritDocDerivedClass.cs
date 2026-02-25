namespace MrKWatkins.Sesharp.TestAssembly.InheritDoc;

/// <summary>A derived class that inherits documentation from its base class.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class InheritDocDerivedClass : InheritDocBaseClass
{
    /// <inheritdoc />
    public override int VirtualMethod() => 1;

    /// <inheritdoc />
    public override int VirtualProperty => 1;
}