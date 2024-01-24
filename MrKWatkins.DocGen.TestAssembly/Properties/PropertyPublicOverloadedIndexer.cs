namespace MrKWatkins.DocGen.TestAssembly.Properties;

[SuppressMessage("ReSharper", "UnusedParameter.Global")]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class PropertyPublicOverloadedIndexer
{
    public int this[int _] => 0;

    public int this[string _] => 0;
}