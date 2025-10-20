using Content.Shared.Clothing                                                                                                   ;
using Content.Shared.Examine;
using Content.Shared.Nutrition.Components                                                                                       ; // i present to you:
using Content.Shared.Nutrition.EntitySystems                                                                                    ; // warcrime coding.
using Robust.Shared.Containers                                                                                                  ; // since we have no coders,
namespace Content.Shared._Dune.Thirst                                                                                           ; // we dont have any standarts
public sealed partial class ClothingThirstModifierSystem : EntitySystem                                                         { // so :trollface:
    [Dependency] private readonly SharedContainerSystem _container = default!                                                   ;
    [Dependency] private readonly ThirstSystem _thirstSystem = default!                                                         ;
    public override void Initialize()                                                                                           {
        base.Initialize()                                                                                                       ;
        SubscribeLocalEvent<ClothingThirstModifierComponent, ClothingGotEquippedEvent>(OnClothingEquipped)                      ;
        SubscribeLocalEvent<ClothingThirstModifierComponent, ClothingGotUnequippedEvent>(OnClothingUnequipped)                  ;
        SubscribeLocalEvent<ClothingThirstModifierComponent, ExaminedEvent>(OnExamined)                                         ;}
    private void OnClothingEquipped(EntityUid uid, ClothingThirstModifierComponent component, ClothingGotEquippedEvent args)    {
        if (!TryComp<ThirstComponent>(args.Wearer, out var thirstComp)) return                                  ;
        _thirstSystem.SetBaseDecayRate(args.Wearer, component.BaseDecayRateModifier, thirstComp)                                ;}
    private void OnClothingUnequipped(EntityUid uid, ClothingThirstModifierComponent component, ClothingGotUnequippedEvent args){
        if (!TryComp<ThirstComponent>(args.Wearer, out var thirstComp)) return                                  ;
        _thirstSystem.SetBaseDecayRate(args.Wearer, 0.1f, thirstComp)                                                   ;}
    private void OnExamined(EntityUid uid, ClothingThirstModifierComponent component, ExaminedEvent args)                       {
        var reductionPercent = (0.1f - component.BaseDecayRateModifier) / 0.1f * 100f                                      ;
        args.PushMarkup(Loc.GetString("stillsuit-examined-base", ("rate", reductionPercent.ToString("0"))))    ;}}
