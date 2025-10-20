using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using System.Linq;
using System.Numerics;

namespace Content.Shared._Dune.Particles;

// todo figure out how to make entities spawn multiple particles with one component
[RegisterComponent, NetworkedComponent]
public sealed partial class ParticleCreatorComponent : Component
{
    /// <summary>
    /// radius of particle spawn
    /// </summary>
    [DataField]
    public float Radius = 0.5f;

    /// <summary>
    /// offset from the entity that has the comp
    /// </summary>
    [DataField]
    public Vector2 Offset = Vector2.Zero;

    /// <summary>
    /// speed of the particle
    /// </summary>
    [DataField]
    public float Speed = 1.0f;

    /// <summary>
    /// deletes the particle after X time
    /// </summary>
    [DataField]
    public float Lifetime = 1.0f;

    /// <summary>
    /// proto of the entity that will be used as a particle
    /// </summary>
    [DataField("particleProto", required: true)]
    public string ParticlePrototype = "Crowbar"!; // so that you could give the component to an entity and edit it in-game

    /// <summary>
    /// amount of particles spawning
    /// </summary>
    [DataField]
    public int Amount = 5;

    /// <summary>
    /// spawn particles in X seconds
    /// </summary>
    [DataField]
    public float SpawnInterval = 1.0f;

    /// <summary>
    /// If disabled, stops particles from spawning if they are spawning on grids.
    /// it was supposed to be used for planets but there is no way to differ planets from grids that are on them ;-;
    /// </summary>
    [DataField]
    public bool WorkOnGrids = true;
}
