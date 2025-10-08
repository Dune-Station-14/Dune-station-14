using Content.Shared.Cuffs.Components;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Buckle;

[Serializable, NetSerializable]
public sealed partial class UnbuckleDoAfterEvent : DoAfterEvent
{
    public bool IncapacitatedDelay;
    public UnbuckleDoAfterEvent(bool incapacitatedDelay)
    {
        IncapacitatedDelay = incapacitatedDelay;
    }

    public override DoAfterEvent Clone() => this;
}
