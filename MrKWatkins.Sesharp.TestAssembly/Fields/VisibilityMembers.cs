namespace MrKWatkins.Sesharp.TestAssembly.Fields;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class VisibilityMembers
{
    public int Value { get; set; }

    private int Hidden() => Value;

    protected int ForDerived() => Value;

    [UsedImplicitly]
    private static class HiddenType;
}
