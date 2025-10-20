using Content.Shared._Dune.Shield.DamageBlock;
using Content.Shared.Clothing;
using Content.Shared.Clothing.Components;
using Content.Shared.Interaction.Components;

namespace Content.Shared._Dune.Shield;
public sealed class DuneShieldSystem : EntitySystem
{

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DuneToggleShieldActionEvent>(OnToggleShield);
        SubscribeLocalEvent<ClothingComponent, ClothingGotUnequippedEvent>(OnClothingUnequipped);
    }

    private void OnClothingUnequipped(EntityUid uid, ClothingComponent component, ClothingGotUnequippedEvent args)
    {
        var user = args.Wearer;
        if (TryComp<DamageBlockerComponent>(user, out var blocker))
        {
            RemComp<DamageBlockerComponent>(user);
            RemComp<ShieldVisualsComponent>(user);
        }
    }

    private void OnToggleShield(DuneToggleShieldActionEvent args)
    {
        if (args.Handled
            || args.Performer == null)
            return;

        var user = args.Performer;

        if (TryComp<DamageBlockerComponent>(user, out var b))
        {
            RemComp<DamageBlockerComponent>(user);
            RemComp<ShieldVisualsComponent>(user);
        }
        else
        {
            var visuals = EnsureComp<ShieldVisualsComponent>(user);
            var blocker = EnsureComp<DamageBlockerComponent>(user);
            /*visuals.ActiveColor = args.ActiveColor;
            visuals.WeakColor = args.WeakColor;*/

            Dirty(user, visuals);
            Dirty(user, blocker); // maybe there is a non-goida way of doing this, but i dont care lol
        }

        args.Handled = true;
    }
}
