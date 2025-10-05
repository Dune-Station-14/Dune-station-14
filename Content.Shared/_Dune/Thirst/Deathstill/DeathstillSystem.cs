using Content.Shared.Administration.Logs;
using Content.Shared.Body.Components;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Systems;
using Content.Shared.Buckle;
using Content.Shared.Buckle.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.Destructible;
using Content.Shared.DoAfter;
using Content.Shared.DragDrop;
using Content.Shared.Examine;
using Content.Shared.Fluids;
using Content.Shared.Fluids.Components;
using Content.Shared.Hands;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory.Events;
using Content.Shared.Item;
using Content.Shared.Kitchen.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Events;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared._Dune.Thirst.Deathstill;

/// <summary>
/// Used to butcher some entities like monkeys.
/// </summary>
public sealed class DeathstillSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly ExamineSystemShared _examineSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly MetaDataSystem _metaDataSystem = default!;
    [Dependency] private readonly ISharedAdminLogManager _logger = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ThirstSystem _thirst = default!;
    [Dependency] private readonly SharedPuddleSystem _puddle = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DeathstillComponent, ExaminedEvent>(OnSpikeExamined);
        SubscribeLocalEvent<DeathstillComponent, GetVerbsEvent<Verb>>(OnGetVerbs);
        SubscribeLocalEvent<DeathstillComponent, AfterInteractEvent>(OnInteract);

        SubscribeLocalEvent<DeathstillVictimComponent, ExaminedEvent>(OnVictimExamined);
        SubscribeLocalEvent<DeathstillComponent, DeathstillDoAfterEvent>(OnDeathstillDoAfter);
    }

    private void OnDeathstillDoAfter(Entity<DeathstillComponent> ent, ref DeathstillDoAfterEvent args)
    {
        var (uid, comp) = ent;
        if (args.Cancelled || args.Handled)
            return;

        args.Handled = true;

        if (!TryComp<ThirstComponent>(args.Target, out var thirst) || thirst.Solution == null)
            return;

        if (!TryComp<BodyComponent>(args.Target, out var body))
            return;

        if (!TryComp<SolutionContainerManagerComponent>(uid, out var sol))
            return;

        var organs = _bodySystem.GetBodyOrganEntityComps<StomachComponent>((args.Target.Value, body));

        if (!_solutionContainerSystem.ResolveSolution((uid, sol), comp.WaterSolutionName, ref comp.Solution, out _))
            return;

        bool hasExtractedWater = false;

        foreach (var organ in organs)
        {
            if (!TryComp<SolutionContainerManagerComponent>(organ, out var stomachSolutionsManager) ||
                !TryComp<ThirstComponent>(args.Target, out var thirstComp))
                continue;

            if (!_solutionContainerSystem.TryGetSolution((organ, stomachSolutionsManager),
                    thirstComp.WaterSolutionName,
                    out var stomach))
                continue;

            var stomachSolutions = stomach.Value.Comp.Solution.Contents.ToArray();

            foreach (var solutions in stomachSolutions)
            {
                if (solutions.Reagent.Prototype != "Water")
                    continue;

                if (stomach.Value.Comp.Solution.Volume <= 0)
                    break;

                _thirst.ModifyThirst(args.Target.Value, thirstComp, -comp.TimeDamage.Float());

                var splitted = _solutionContainerSystem.SplitSolution(stomach.Value, comp.TimeDamage);
                _solutionContainerSystem.AddSolution(comp.Solution.Value, splitted);

                hasExtractedWater = true;
            }
        }

        if (hasExtractedWater && _net.IsServer)
            _audioSystem.PlayPvs(comp.SuckSound, uid);

        // РЕКУРСИВНЫЙ ЗАПУСК: проверяем, осталось ли вещество
        if (TryComp<ThirstComponent>(args.Target, out var finalCheck) &&
            finalCheck.Solution != null &&
            finalCheck.Solution.Value.Comp.Solution.Volume > 0)
        {
            // Запускаем новый DoAfter
            StartDeathstillDoAfter(uid, comp, args.User, args.Target.Value);
        }
    }

    private void OnInteract(EntityUid uid, DeathstillComponent comp, AfterInteractEvent args)
    {
        if (!TryComp<ThirstComponent>(args.Target, out var th) || th.Solution == null)
            return;
        if (!TryComp<MobStateComponent>(args.Target, out var mob))
            return;
        if (mob.CurrentState != MobState.Dead)
            return;

        // Запускаем DoAfter только если есть вещество
        if (th.Solution.Value.Comp.Solution.Volume > 0)
        {
            StartDeathstillDoAfter(uid, comp, args.User, args.Target.Value);
        }
    }

    private void StartDeathstillDoAfter(EntityUid uid, DeathstillComponent comp, EntityUid user, EntityUid target)
    {
        var doAfterArgs = new DoAfterArgs(EntityManager,
            user,
            comp.HookDelay,
            new DeathstillDoAfterEvent(),
            uid,
            target,
            uid)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            AttemptFrequency = AttemptFrequency.EveryTick
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }


    private void OnSpikeExamined(Entity<DeathstillComponent> ent, ref ExaminedEvent args)
    {
        if (!TryComp<StrapComponent>(ent, out var strap))
            return;
        var victim = strap.BuckledEntities.FirstOrNull();

        if (!victim.HasValue)
            return;

        // Show it at the end of the examine so it looks good.
        args.PushMarkup(Loc.GetString("comp-kitchen-spike-hooked",
                ("victim", Identity.Entity(victim.Value, EntityManager))),
            -1);
        args.PushMessage(_examineSystem.GetExamineText(victim.Value, args.Examiner), -2);
    }

    private void OnGetVerbs(Entity<DeathstillComponent> ent, ref GetVerbsEvent<Verb> args)
    {
    }

    private void OnVictimExamined(Entity<DeathstillVictimComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("comp-kitchen-spike-victim-examine", ("target", Identity.Entity(ent, EntityManager))));
    }

    private static void OnAttempt(EntityUid uid, DeathstillHookedComponent component, CancellableEntityEventArgs args)
    {
        args.Cancel();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<DeathstillComponent, StrapComponent, SolutionContainerManagerComponent>();
        while (query.MoveNext(out var uid, out var deathStill, out var strap, out var sol))
        {
            if (_gameTiming.CurTime < deathStill.NextDamage)
                continue;

            var entity = strap.BuckledEntities.FirstOrNull();
            if (!entity.HasValue)
                continue;
            if (!TryComp<BodyComponent>(strap.BuckledEntities.FirstOrNull(), out var body))
                continue;

            var organs = _bodySystem.GetBodyOrganEntityComps<StomachComponent>((entity.Value, body));

            if (!_solutionContainerSystem.ResolveSolution((uid, sol), deathStill.WaterSolutionName, ref deathStill.Solution, out _))
                continue;

            deathStill.NextDamage += deathStill.DamageInterval;

            foreach (var ent in organs)
            {
                if (!TryComp<SolutionContainerManagerComponent>(ent, out var stomachSolutionsManager) ||
                    !TryComp<ThirstComponent>(entity, out var thirst))
                    continue;

                if (!_solutionContainerSystem.TryGetSolution((ent, stomachSolutionsManager), thirst.WaterSolutionName, out var stomach))
                    continue;
                var stomachSolutions = stomach.Value.Comp.Solution.Contents.ToArray();

                foreach (var solutions in stomachSolutions)
                {
                    if (solutions.Reagent.Prototype != "Water")
                        continue;
                    if (deathStill.Solution == null)
                        continue;

                    _thirst.ModifyThirst(entity.Value, thirst, -deathStill.TimeDamage.Float());

                    var splitted = _solutionContainerSystem.SplitSolution(stomach.Value, deathStill.TimeDamage);
                    _solutionContainerSystem.AddSolution(deathStill.Solution.Value, splitted);

                }
            }

            if (_net.IsServer)
                _audioSystem.PlayPvs(deathStill.SuckSound, uid); // i have to use fucking INetManager
        }
    }

    /// <summary>
    /// A helper method to show predicted popups that can be targeted towards yourself or somebody else.
    /// </summary>
    private void ShowPopups(string selfLocMessageSelf,
        string selfLocMessageOthers,
        string locMessageSelf,
        string locMessageOthers,
        EntityUid user,
        EntityUid victim,
        EntityUid hook)
    {
        string messageSelf, messageOthers;

        var victimIdentity = Identity.Entity(victim, EntityManager);

        if (user == victim)
        {
            messageSelf = Loc.GetString(selfLocMessageSelf, ("hook", hook));
            messageOthers = Loc.GetString(selfLocMessageOthers, ("victim", victimIdentity), ("hook", hook));
        }
        else
        {
            messageSelf = Loc.GetString(locMessageSelf, ("victim", victimIdentity), ("hook", hook));
            messageOthers = Loc.GetString(locMessageOthers,
                ("user", Identity.Entity(user, EntityManager)),
                ("victim", victimIdentity),
                ("hook", hook));
        }

        _popupSystem.PopupPredicted(messageSelf, messageOthers, hook, user, PopupType.MediumCaution);
    }

}

[Serializable, NetSerializable]
public sealed partial class DeathstillDoAfterEvent : SimpleDoAfterEvent;
