using Robust.Shared.GameStates;

namespace Content.Shared._Dune.Thirst.Deathstill;

/// <summary>
/// Used to mark entity that was butchered on the spike.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DeathstillVictimComponent : Component;
