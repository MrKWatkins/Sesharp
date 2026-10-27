namespace MrKWatkins.Sesharp.TestAssembly.Fields;

public static class EnumConstants
{
    public enum Choice { None = 0, First = 1, Second = 2 }
    [SuppressMessage("Design", "CA1028:Enum Storage should be Int32", Justification = "Exercises unsigned high-bit constant rendering.")]
    [Flags]
    public enum Bits : ulong { None = 0, First = 1, Second = 2, High = 1UL << 63 }

    public const Choice Named = Choice.First;
    public const Choice Unnamed = (Choice)4;
    public const Choice Negative = (Choice)(-1);
    public const Bits Zero = Bits.None;
    public const Bits Combined = Bits.First | Bits.Second;
    public const Bits High = Bits.High;
    public const Bits HighCombined = Bits.High | Bits.First;
    public const Bits UnnamedBits = (Bits)8;
}