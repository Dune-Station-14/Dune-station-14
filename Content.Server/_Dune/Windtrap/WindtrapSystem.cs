using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Light.Components;
using Content.Shared.Light.EntitySystems;
using Content.Shared.Maps;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Server._Dune.Windtrap;

public sealed class WindtrapSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] private readonly SharedRoofSystem _roof = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly TurfSystem _turf = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WindtrapComponent, ComponentStartup>(OnComponentStartup);
    }

    private void OnComponentStartup(EntityUid uid, WindtrapComponent component, ComponentStartup args) =>
        component.NextGainTime += _gameTiming.CurTime + component.GainInterval;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<WindtrapComponent, SolutionContainerManagerComponent>();
        while (query.MoveNext(out var uid, out var windtrap, out var sol))
        {
            var xform = Transform(uid);
            var gridUid = xform.GridUid;
            if (gridUid == null ||
                !TryComp<MapGridComponent>(gridUid, out var grid))
                continue;

            var tilePos = _mapSystem.TileIndicesFor(gridUid.Value, grid, xform.Coordinates);
            var tileRef = _turf.GetTileRef(xform.Coordinates);

            if (tileRef == null)
                continue;
            var tileDef = _turf.GetContentTileDefinition(tileRef.Value);

            if (!tileDef.Weather)
                continue;

            if (TryComp<RoofComponent>(gridUid.Value, out var roofComp) &&
                _roof.IsRooved((gridUid.Value, grid, roofComp), tilePos))
                continue;

            if (_gameTiming.CurTime < windtrap.NextGainTime ||
                !_solutionContainerSystem.ResolveSolution((uid, sol), windtrap.WindtrapSolutionName, ref windtrap.Solution, out _))
                continue;

            windtrap.NextGainTime += windtrap.GainInterval;
            _solutionContainerSystem.TryAddReagent(windtrap.Solution.Value, "Water", windtrap.WaterGainPerSecond);
        }
    }
}
