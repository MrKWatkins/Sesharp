namespace MrKWatkins.Sesharp.TestAssembly.VitePress;

/// <summary>A record used to verify constructor links.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public record OverloadedConstructorRecord
{
    public OverloadedConstructorRecord()
    {
    }

    public OverloadedConstructorRecord(int value) => Value = value;

    public int Value { get; init; }
}