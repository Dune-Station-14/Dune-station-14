using System.Numerics;

namespace Content.Shared._Dune.Particles;

[RegisterComponent]
public sealed partial class ParticleVelocityComponent : Component
{
    public Vector2 Velocity;
}
