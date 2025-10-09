using Content.Shared.Chemistry.Components;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Server._Dune.Windtrap;

[RegisterComponent, AutoGenerateComponentState]
public sealed partial class WindtrapComponent : Component
{
    /// <summary>
    /// The solution inside of windtrap.
    /// </summary>
    [ViewVariables]
    public Entity<SolutionComponent>? Solution;

    /// <summary>
    /// What solution should windtrap contain (not reagent)?
    /// </summary>
    [DataField]
    public string WindtrapSolutionName = "windtrap";

    /// <summary>
    /// Water gain per sec.
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 WaterGainPerSecond = 1;

    /// <summary>
    /// The next time when the water will be gained.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan NextGainTime = TimeSpan.Zero;

    /// <summary>
    /// How often should windtrap get water?
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan GainInterval = TimeSpan.FromSeconds(1);
}
