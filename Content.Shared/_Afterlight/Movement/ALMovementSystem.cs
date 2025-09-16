using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._Afterlight.Movement;

// Taken from https://github.com/RMC-14/RMC-14
public sealed class ALMovementSystem : EntitySystem
{
    [Dependency] private readonly FixtureSystem _fixture = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ALMobCollisionComponent, MapInitEvent>(OnMobCollisionMapInit);
        SubscribeLocalEvent<ALMobCollisionComponent, MobStateChangedEvent>(OnMobCollisionMobStateChanged);
    }

    private void OnMobCollisionMapInit(Entity<ALMobCollisionComponent> ent, ref MapInitEvent args)
    {
        if (TryComp(ent, out MobCollisionComponent? collision))
        {
            collision.FixtureId = ent.Comp.FixtureId;
            DirtyField(ent, collision, nameof(MobCollisionComponent.FixtureId));
        }

        if (_mobState.IsDead(ent))
            return;

        if (!TryComp<PhysicsComponent>(ent, out var body))
            return;

        CreateMobCollisionFixture((ent, ent, body));
    }

    private void CreateMobCollisionFixture(Entity<ALMobCollisionComponent, PhysicsComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp2, false))
            return;

        _fixture.TryCreateFixture(
            ent,
            ent.Comp1.FixtureShape,
            ent.Comp1.FixtureId,
            hard: false,
            collisionLayer: (int) ent.Comp1.FixtureLayer,
            collisionMask: (int) ent.Comp1.FixtureLayer,
            body: ent
        );
    }

    private void OnMobCollisionMobStateChanged(Entity<ALMobCollisionComponent> ent, ref MobStateChangedEvent args)
    {
        switch (args.NewMobState)
        {
            case MobState.Alive:
                CreateMobCollisionFixture(ent);
                break;
            case MobState.Dead:
                _fixture.DestroyFixture(ent, ent.Comp.FixtureId);
                break;
        }
    }
}
