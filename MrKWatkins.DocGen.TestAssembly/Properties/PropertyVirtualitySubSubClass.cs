namespace MrKWatkins.DocGen.TestAssembly.Properties;

[SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public abstract class PropertyVirtualitySubSubClass : PropertyVirtualitySubClass
{
    public new int New { get; }

    public new int NewSubSubClass { get; }
}