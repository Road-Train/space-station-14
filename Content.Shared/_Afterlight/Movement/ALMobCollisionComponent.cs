using Content.Shared.Physics;
using Robust.Shared.GameStates;
using Robust.Shared.Physics.Collision.Shapes;

namespace Content.Shared._Afterlight.Movement;

// Taken from https://github.com/RMC-14/RMC-14
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(ALMovementSystem))]
public sealed partial class ALMobCollisionComponent : Component
{
    [DataField, AutoNetworkedField]
    public string FixtureId = "al_mob_collision";

    [DataField, AutoNetworkedField]
    public IPhysShape FixtureShape = new PhysShapeCircle(0.4f);

    [DataField, AutoNetworkedField]
    public CollisionGroup FixtureLayer = CollisionGroup.MobCollision;
}
