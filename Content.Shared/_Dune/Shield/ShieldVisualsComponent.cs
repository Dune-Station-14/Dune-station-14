using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._Dune.Shield;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(raiseAfterAutoHandleState: true)] // holy fucking shit
public sealed partial class ShieldVisualsComponent : Component
{
    [DataField("prototype", customTypeSerializer:typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string Prototype { get; private set; } = "DuneShieldVisual";

    // This field is now properly handled on the client side
    [ViewVariables, AutoNetworkedField]
    public ShieldState State = ShieldState.Off;

    [ViewVariables, AutoNetworkedField]
    public float StateTimer;

    [ViewVariables, AutoNetworkedField]
    public float CurrentOpacity = 0.4f;

    [ViewVariables, AutoNetworkedField]
    public bool WasActiveLastFrame = false;

    // Current color that will be used for the outline
    [ViewVariables, AutoNetworkedField]
    public Color CurrentColor = new Color(0, 0, 0, 0);

    [DataField, AutoNetworkedField]
    public Color ActiveColor = new Color(0, 100, 255, 85);  // #3627E355

    [DataField, AutoNetworkedField]
    public Color WeakColor = new Color(252, 3, 3, 85); // #fc030355

    [DataField, AutoNetworkedField]
    public Color OffColor = new Color(0, 0, 0, 0);
}

public enum ShieldState
{
    Active,
    Weak,
    Off
}

[Serializable, NetSerializable]
public enum ShieldVisualsKeys : byte
{
    State
}
