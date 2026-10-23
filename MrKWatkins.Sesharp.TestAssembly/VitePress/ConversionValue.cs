namespace MrKWatkins.Sesharp.TestAssembly.VitePress;

/// <summary>A value with conversions that share their input parameter type.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public readonly struct ConversionValue
{
    public static implicit operator int(ConversionValue value) => 0;

    public static implicit operator long(ConversionValue value) => 0L;

    public static explicit operator byte(ConversionValue value) => 0;

    public static explicit operator ushort(ConversionValue value) => 0;

    public static explicit operator checked byte(ConversionValue value) => 0;
}