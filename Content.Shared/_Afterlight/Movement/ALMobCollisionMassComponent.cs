using Robust.Shared.GameStates;

namespace Content.Shared._Afterlight.Movement;

// Taken from https://github.com/RMC-14/RMC-14
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(ALMovementSystem))]
public sealed partial class ALMobCollisionMassComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Mass = 150;
}
