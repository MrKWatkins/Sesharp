namespace MrKWatkins.Sesharp.TestAssembly.VitePress;

/// <summary>
/// Represents a primary constructor struct.
/// </summary>
/// <param name="value">The initial value.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public readonly struct PrimaryConstructorStruct(int value)
{
    public int Value { get; } = value;
}