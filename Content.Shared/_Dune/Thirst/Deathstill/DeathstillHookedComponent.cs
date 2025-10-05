using Robust.Shared.GameStates;

namespace Content.Shared._Dune.Thirst.Deathstill;

/// <summary>
/// Used to mark entities that are currently hooked on the spike.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DeathstillHookedComponent : Component;
