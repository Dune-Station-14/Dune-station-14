using Robust.Shared.GameStates;

namespace Content.Shared._Dune.Shield.DamageBlock;

[RegisterComponent, NetworkedComponent]
public sealed partial class DamageBlockerComponent : Component
{
    [DataField]
    public float MaxHealth = 25f;

    [DataField]
    public float RegenRate = 1f;

    // if the health drops below zero, it would take 10 seconds for shield to start working again
    [DataField]
    public float RechargeTime = 20f;

    // below this, if the entity has ShieldVisualsComponent, will trigger the weak state
    [DataField]
    public float WeakThreshold = 10f;

    [ViewVariables]
    public float CurrentHealth;

    [ViewVariables]
    public bool IsBroken = false;

    [ViewVariables]
    public float RechargeTimer;
}
