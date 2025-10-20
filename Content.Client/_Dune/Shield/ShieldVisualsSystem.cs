using System.Numerics;
using Content.Shared._Dune.Shield;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Client._Dune.Shield;

// times this system was redone from scratch: 5
// this time with the selection outline
public sealed class ShieldVisualsSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShieldVisualsComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ShieldVisualsComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<ShieldVisualsComponent, AfterAutoHandleStateEvent>(AfterHandleState);
    }

    private void OnInit(EntityUid uid, ShieldVisualsComponent component, ComponentInit args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        RecreatePostShader(uid, component, sprite);
    }

    private void OnRemove(EntityUid uid, ShieldVisualsComponent component, ComponentRemove args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        RemovePostShader(sprite);
    }

    private void AfterHandleState(EntityUid uid, ShieldVisualsComponent component, ref AfterAutoHandleStateEvent args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        RecreatePostShader(uid, component, sprite);
    }

    // deletes the shader
    private void RemovePostShader(SpriteComponent sprite)
    {
        sprite.PostShader = null;
    }

    // deletes the shader and applies it again
    private void RecreatePostShader(EntityUid uid, ShieldVisualsComponent component, SpriteComponent sprite)
    {
        sprite.PostShader = null;

        if (!_proto.TryIndex<ShaderPrototype>("ShieldOutline", out var shaderProto))
            return;

        var shaderInstance = shaderProto.Instance().Duplicate();
        sprite.PostShader = shaderInstance;

        var outlineColor = component.State switch
        {
            ShieldState.Active => component.ActiveColor,
            ShieldState.Weak => component.WeakColor,
            _ => component.OffColor
        };

        outlineColor = outlineColor.WithAlpha(outlineColor.A * component.CurrentOpacity);

        shaderInstance.SetParameter("outline_color", outlineColor);
        shaderInstance.SetParameter("outline_width", 6.0f);
        sprite.PostShader.SetParameter("light_boost", 2f);
        sprite.PostShader.SetParameter("light_gamma", 0.9f); // no im not unhardcoding this stuff, and you dont need to probably
        sprite.PostShader.SetParameter("light_whitepoint", 1f);
    }

    private void UpdateShaderParameters(EntityUid uid, ShieldVisualsComponent component, SpriteComponent sprite)
    {
        if (sprite.PostShader == null)
            return;

        if (component.State == ShieldState.Off)
        {
            sprite.PostShader.SetParameter("outline_width", 0f);
            return;
        }

        var outlineColor = component.State switch
        {
            ShieldState.Active => component.ActiveColor,
            ShieldState.Weak   => component.WeakColor,
            _ => component.OffColor
        };

        outlineColor = outlineColor.WithAlpha(outlineColor.A * component.CurrentOpacity);

        sprite.PostShader.SetParameter("outline_color", outlineColor);
        sprite.PostShader.SetParameter("outline_width", 6.0f);
        sprite.PostShader.SetParameter("light_boost", 2f);
        sprite.PostShader.SetParameter("light_gamma", 0.9f);
        sprite.PostShader.SetParameter("light_whitepoint", 1f);
    }



    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ShieldVisualsComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            comp.StateTimer += frameTime;

            if (!TryComp<SpriteComponent>(uid, out var sprite))
                continue;

            RecreatePostShader(uid, comp, sprite);
            UpdateShaderParameters(uid, comp, sprite);
        }
    }
}
