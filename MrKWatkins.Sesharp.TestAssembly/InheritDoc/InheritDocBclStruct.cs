namespace MrKWatkins.Sesharp.TestAssembly.InheritDoc;

/// <summary>A struct that implements a BCL interface using inheritdoc for the static members.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public readonly struct InheritDocBclStruct : IParsable<InheritDocBclStruct>
{
    /// <inheritdoc />
    public static InheritDocBclStruct Parse(string s, IFormatProvider? provider) => default;

    /// <inheritdoc />
    public static bool TryParse(string? s, IFormatProvider? provider, out InheritDocBclStruct result)
    {
        result = default;
        return false;
    }
}