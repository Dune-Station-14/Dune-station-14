namespace Content.Shared._Dune.Particles;

[RegisterComponent]
public sealed partial class ParticleLifetimeComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public float TimeRemaining = 1f;
}
