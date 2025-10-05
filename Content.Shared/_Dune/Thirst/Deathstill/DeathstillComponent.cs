using Content.Shared.Chemistry.Components;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.Nutrition.Components;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Dune.Thirst.Deathstill;

/// <summary>
/// Used to mark entity that should act as a spike.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class DeathstillComponent : Component
{
    /// <summary>
    /// The state of the injector. Determines it's attack behavior. Containers must have the
    /// right SolutionCaps to support injection/drawing. For InjectOnly injectors this should
    /// only ever be set to Inject
    /// </summary>
    [AutoNetworkedField]
    [DataField]
    public InjectorToggleMode ToggleState = InjectorToggleMode.Draw;

    /// <summary>
    ///     The solution inside of this stomach this transfers reagents to the body.
    /// </summary>
    [ViewVariables]
    public Entity<SolutionComponent>? Solution;

    /// <summary>
    ///     What solution should this stomach push reagents into, on the body?
    /// </summary>
    [DataField]
    public string WaterSolutionName = "deathstill";

    /// <summary>
    /// Sound to play when the victim is hooked or unhooked.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier ConnectSound = new SoundPathSpecifier("/Audio/_Dune/Items/Deathstill/connect.ogg");

    /// <summary>
    /// Sound to play when the victim is hooked or unhooked.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier DisconnectSound = new SoundPathSpecifier("/Audio/_Dune/Items/Deathstill/disconnect.ogg");

    /// <summary>
    /// Sound to play when the victim is butchered.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier SuckSound = new SoundPathSpecifier("/Audio/_Dune/Items/Deathstill/suck.ogg");

    /// <summary>
    /// Damage that the victim will receive over time.
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 TimeDamage = 1;

    /// <summary>
    /// The next time when the damage will be applied to the victim.
    /// </summary>
    [AutoPausedField, AutoNetworkedField]
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextDamage = TimeSpan.Zero;

    /// <summary>
    /// How often the damage should be applied to the victim.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan DamageInterval = TimeSpan.FromSeconds(1,5);

    /// <summary>
    /// Time that it will take to put the victim on the spike.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan HookDelay = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Time that it will take to put the victim off the spike.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan UnhookDelay = TimeSpan.FromSeconds(1);
}
