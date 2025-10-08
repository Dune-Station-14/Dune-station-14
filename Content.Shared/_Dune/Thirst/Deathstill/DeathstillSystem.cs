using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Buckle;
using Content.Shared.Buckle.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared._Dune.Thirst.Deathstill;

/// <summary>
/// Used to butcher some entities like monkeys.
/// </summary>
public sealed class DeathstillSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystemShared _examineSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly ThirstSystem _thirst = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] private readonly SharedBuckleSystem _buckle = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DeathstillComponent, StrapAttemptEvent>(OnStrapAttempt);
        SubscribeLocalEvent<DeathstillComponent, StrappedEvent>(OnEntStrapped);
        SubscribeLocalEvent<DeathstillComponent, UnstrappedEvent>(OnEntUnstrapped);
        SubscribeLocalEvent<DeathstillComponent, ExaminedEvent>(OnDeathstillExamined);

        SubscribeLocalEvent<DeathstillComponent, UnstrapAttemptEvent>(OnEntUnstrapAttempt);

        SubscribeLocalEvent<DeathstillVictimComponent, ExaminedEvent>(OnVictimExamined);
    }

    private bool CheckInsertValidity(EntityUid ent)
    {
        if (!TryComp<MobStateComponent>(ent, out var mobState))
            return false;

        return mobState.CurrentState == MobState.Dead;
    }

    private void OnStrapAttempt(Entity<DeathstillComponent> ent, ref StrapAttemptEvent args)
    {
        if (!CheckInsertValidity(args.Buckle))
        {
            args.Cancelled = true;
            return;
        }

        if (!TryComp<StrapComponent>(ent, out var strap))
            return;
        _buckle.TrySetIncapacitatedDelay((ent.Owner, strap), true);
    }

    private void OnEntStrapped(Entity<DeathstillComponent> ent, ref StrappedEvent args)
    {
        EnsureComp<DeathstillVictimComponent>(args.Buckle);

        _audioSystem.PlayPredicted(ent.Comp.ConnectSound, ent, args.User);

        ent.Comp.NextDamageTime = _gameTiming.CurTime + TimeSpan.FromSeconds(10); // 9 seconds connect sound, hardcode :(
    }

    private void OnEntUnstrapAttempt(Entity<DeathstillComponent> ent, ref UnstrapAttemptEvent args)
    {
        if (!TryComp<StrapComponent>(ent, out var strap))
            return;

        _buckle.SetUnbuckleDoAfter((ent, strap), true);
    }

    private void OnEntUnstrapped(Entity<DeathstillComponent> ent, ref UnstrappedEvent args)
    {
        _audioSystem.PlayPredicted(ent.Comp.DisconnectSound, ent, args.User);
    }

    private void OnDeathstillExamined(Entity<DeathstillComponent> ent, ref ExaminedEvent args)
    {
        if (!TryComp<StrapComponent>(ent, out var strap))
            return;

        var victim = strap.BuckledEntities.FirstOrNull();
        if (!victim.HasValue)
            return;

        args.PushMarkup(Loc.GetString("comp-deathstill-connected", ("victim", Identity.Entity(victim.Value, EntityManager))), -1);
        args.PushMessage(_examineSystem.GetExamineText(victim.Value, args.Examiner), -2);
    }

    private void OnVictimExamined(Entity<DeathstillVictimComponent> ent, ref ExaminedEvent args) =>
        args.PushMarkup(Loc.GetString("comp-kitchen-spike-victim-examine", ("target", Identity.Entity(ent, EntityManager))));

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<DeathstillComponent, StrapComponent, SolutionContainerManagerComponent>();
        while (query.MoveNext(out var uid, out var deathStill, out var strap, out var sol))
        {
            if (_gameTiming.CurTime < deathStill.NextDamageTime)
                continue;

            var entity = strap.BuckledEntities.FirstOrNull();
            if (!entity.HasValue || !TryComp<BodyComponent>(strap.BuckledEntities.FirstOrNull(), out var body))
                continue;

            if (!_solutionContainerSystem.ResolveSolution((uid, sol), deathStill.DeathstillSolutionName, ref deathStill.Solution, out _))
                continue;

            deathStill.NextDamageTime += deathStill.DamageInterval;

            var organs = _bodySystem.GetBodyOrganEntityComps<StomachComponent>((entity.Value, body));
            foreach (var ent in organs)
            {
                if (!TryComp<SolutionContainerManagerComponent>(ent, out var stomachSolutionsManager) ||
                    !TryComp<ThirstComponent>(entity, out var thirst))
                    continue;

                if (!_solutionContainerSystem.TryGetSolution((ent, stomachSolutionsManager), thirst.WaterSolutionName, out var stomach))
                    continue;
                var stomachSolutions = stomach.Value.Comp.Solution.Contents.ToArray();

                foreach (var solution in stomachSolutions)
                {
                    if (solution.Reagent.Prototype != "Water" || deathStill.Solution == null)
                        continue;

                    _thirst.ModifyThirst(entity.Value, thirst, -deathStill.ThirstDamage.Float());

                    var splitted = _solutionContainerSystem.SplitSolution(stomach.Value, deathStill.ThirstDamage / deathStill.DamageDivide); // 1 water unit = 4 thirst
                    _solutionContainerSystem.AddSolution(deathStill.Solution.Value, splitted);
                }

                if (_net.IsServer)
                    _audioSystem.PlayPvs(deathStill.SuckSound, uid); // i have to use fckin INetManager
            }
        }
    }
}
