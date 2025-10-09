using Content.Shared.Chemistry.Components;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Dune.Thirst.Deathstill;


[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class DeathstillComponent : Component
{
    /// <summary>
    ///     The solution inside of deathstill this transfers reagents from the body.
    /// </summary>
    [ViewVariables]
    public Entity<SolutionComponent>? Solution;

    /// <summary>
    ///     What solution should deathstill contain (not reagent)?
    /// </summary>
    [DataField]
    public string DeathstillSolutionName = "deathstill";

    /// <summary>
    /// Sound to play when the victim is connected.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier ConnectSound = new SoundPathSpecifier("/Audio/_Dune/Items/Deathstill/connect.ogg");

    /// <summary>
    /// Sound to play when the victim is disconnected.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier DisconnectSound = new SoundPathSpecifier("/Audio/_Dune/Items/Deathstill/disconnect.ogg");

    /// <summary>
    /// Sound to play when the victim is sucked.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier SuckSound = new SoundPathSpecifier("/Audio/_Dune/Items/Deathstill/suck.ogg");

    /// <summary>
    /// Thirst damage that the victim will receive over time.
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 ThirstDamage = 1;

    /// <summary>
    /// The next time when the damage will be applied to the victim.
    /// </summary>
    [AutoPausedField, AutoNetworkedField]
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextDamageTime = TimeSpan.Zero;

    /// <summary>
    /// How often the damage should be applied to the victim.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan DamageInterval = TimeSpan.FromSeconds(1);

    /// <summary>
    /// How many times should the thirst damage be divided?
    /// </summary>
    [DataField, AutoNetworkedField]
    public int DamageDivide = 4;
}
