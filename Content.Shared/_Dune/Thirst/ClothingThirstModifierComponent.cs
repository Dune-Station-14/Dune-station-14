namespace Content.Shared._Dune.Thirst;

[RegisterComponent]
public sealed partial class ClothingThirstModifierComponent : Component
{
    [DataField("decay")]
    public float BaseDecayRateModifier = 0.06f;
}
