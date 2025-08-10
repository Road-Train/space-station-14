using System.Collections.Immutable;
using Content.Shared._Afterlight.Humanoid.Markings;
using Content.Shared._Afterlight.Inventory;
using Content.Shared._Afterlight.Movement;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Client.Utility;
using Robust.Shared.Graphics.RSI;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Client._Afterlight.Humanoid.Markings;

public sealed class ALMarkingSystem : SharedALMarkingSystem
{
    [Dependency] private readonly ALInventorySystem _alInventory = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    private readonly ImmutableArray<SlotFlags> _slotFlags = Enum.GetValues<SlotFlags>().ToImmutableArray();

    private EntityQuery<EyeComponent> _eyeQuery;
    private EntityQuery<SpriteComponent> _spriteQuery;
    private EntityQuery<TransformComponent> _transformQuery;

    public override void Initialize()
    {
        _eyeQuery = GetEntityQuery<EyeComponent>();
        _spriteQuery = GetEntityQuery<SpriteComponent>();
        _transformQuery = GetEntityQuery<TransformComponent>();

        SubscribeLocalEvent<ActorComponent, ALRelativeRotationChangedEvent>(OnActorRelativeRotationChanged);

        SubscribeLocalEvent<ALMarkingComponent, MoveEvent>(OnMarkingMove);
        SubscribeLocalEvent<ALMarkingComponent, DidEquipEvent>(OnMarkingDidEquip);
        SubscribeLocalEvent<ALMarkingComponent, DidUnequipEvent>(OnMarkingDidUnequip);
    }

    private void OnActorRelativeRotationChanged(Entity<ActorComponent> ent, ref ALRelativeRotationChangedEvent args)
    {
        if (ent.Comp.PlayerSession.UserId != _player.LocalUser)
            return;

        if (!_eyeQuery.TryComp(ent, out var eye))
            return;

        var eyeRotation = eye.Rotation;
        var query = EntityQueryEnumerator<ALMarkingComponent, SpriteComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var sprite, out var xform))
        {
            UpdateSprite((uid, comp, sprite, xform), eyeRotation);
        }
    }

    private void OnMarkingMove(Entity<ALMarkingComponent> ent, ref MoveEvent args)
    {
        if (!_eyeQuery.TryComp(_player.LocalEntity, out var eye))
            return;

        if (!_spriteQuery.TryComp(ent, out var sprite) ||
            !_transformQuery.TryComp(ent, out var transform))
        {
            return;
        }

        var eyeRotation = eye.Rotation;
        UpdateSprite((ent, ent, sprite, transform), eyeRotation);
    }

    private void OnMarkingDidEquip(Entity<ALMarkingComponent> ent, ref DidEquipEvent args)
    {
        MarkingEquippedChanged(ent, args.SlotFlags);
    }

    private void OnMarkingDidUnequip(Entity<ALMarkingComponent> ent, ref DidUnequipEvent args)
    {
        MarkingEquippedChanged(ent, args.SlotFlags);
    }

    private void UpdateSprite(Entity<ALMarkingComponent, SpriteComponent, TransformComponent> ent, Angle eyeRotation)
    {
        if (IsClientSide(ent))
            return;

        var worldRotation = _transform.GetWorldRotation(ent.Comp3);
        if (ent.Comp1.LastEyeRotation == eyeRotation &&
            ent.Comp1.LastWorldRotation == worldRotation)
        {
            return;
        }

        ent.Comp1.LastEyeRotation = eyeRotation;
        ent.Comp1.LastWorldRotation = worldRotation;

        var angle = (worldRotation + eyeRotation).Reduced().FlipPositive();
        Direction? overrideDirection = ent.Comp2.EnableDirectionOverride ? ent.Comp2.DirectionOverride : null;
        var entSprite = new Entity<SpriteComponent?>(ent, ent.Comp2);
        foreach (var (frontLayer, marking) in ent.Comp1.Layers)
        {
            UpdateMarkingVisibility(entSprite, frontLayer, angle, overrideDirection, marking);
        }
    }

    public void ApplyMarking(Entity<SpriteComponent?> entity,
        string layerId,
        MarkingPrototype markingPrototype,
        int j,
        bool visible,
        IReadOnlyList<Color>? colors,
        ref int targetLayer)
    {
        _sprite.LayerSetOffset(entity, layerId, markingPrototype.Offset);
        if (markingPrototype.BackSprites.TryGetValue(j, out var backRsi))
        {
            var markingComp = EnsureComp<ALMarkingComponent>(entity.Owner);
            var behindLayer = $"al_{layerId}_behind";
            var marking = markingComp.Layers.GetOrNew(layerId);
            marking.Back = behindLayer;
            marking.HiddenBy = markingPrototype.HiddenBy;

            markingComp.Layers[layerId] = marking;
            if (!_sprite.LayerMapTryGet(entity, behindLayer, out _, false))
            {
                var layer = _sprite.AddLayer(entity, backRsi, 0 + j);
                _sprite.LayerMapSet(entity, behindLayer, layer);
                _sprite.LayerSetSprite(entity, behindLayer, backRsi);
            }

            _sprite.LayerSetVisible(entity, behindLayer, visible);
            _sprite.LayerSetOffset(entity, behindLayer, markingPrototype.Offset);

            if (colors != null && j < colors.Count)
            {
                _sprite.LayerSetColor(entity, behindLayer, colors[j]);
            }
            else
            {
                _sprite.LayerSetColor(entity, behindLayer, Color.White);
            }

            // we do this so targetLayer doesn't get out of sync
            _sprite.LayerMapTryGet(entity, markingPrototype.BodyPart, out targetLayer, false);
        }
    }

    public override void MarkingsCleared(EntityUid ent)
    {
        RemCompDeferred<ALMarkingComponent>(ent);
    }

    private bool IsFacingFront(SpriteComponent.Layer markingLayer, Angle angle, Direction? overrideDirection)
    {
        var state = markingLayer.ActualState;
        var dir = state == null
            ? RsiDirection.South
            : SpriteComponent.Layer.GetDirection(state.RsiDirections, angle);
        if (overrideDirection != null && state != null)
            dir = overrideDirection.Value.Convert(state.RsiDirections);

        dir = dir.OffsetRsiDir(markingLayer.DirOffset);
        return dir != RsiDirection.North;
    }

    private void UpdateMarkingVisibility(Entity<SpriteComponent?> ent,
        string frontLayer,
        Angle angle,
        Direction? overrideDirection,
        ALMarking marking)
    {
        if (!_sprite.TryGetLayer(ent, frontLayer, out var markingLayer, false))
            return;

        var hiddenByEquipment = _alInventory.HasItemEquipped(ent.Owner, marking.HiddenBy);
        if (hiddenByEquipment)
        {
            _sprite.LayerSetVisible(ent, frontLayer, false);
            _sprite.LayerSetVisible(ent, marking.Back, false);
            return;
        }

        var frontFacing = IsFacingFront(markingLayer, angle, overrideDirection);
        if (frontFacing)
        {
            _sprite.LayerSetVisible(ent, frontLayer, true);
            _sprite.LayerSetVisible(ent, marking.Back, false);
        }
        else
        {
            _sprite.LayerSetVisible(ent, frontLayer, false);
            _sprite.LayerSetVisible(ent, marking.Back, true);
        }
    }

    private void MarkingEquippedChanged(Entity<ALMarkingComponent> ent, SlotFlags slotFlags)
    {
        if (!_spriteQuery.TryComp(ent, out var sprite))
            return;

        if (!_eyeQuery.TryComp(_player.LocalEntity, out var eye))
            return;

        var entSprite = new Entity<SpriteComponent?>(ent, sprite);
        var worldRotation = _transform.GetWorldRotation(ent);
        var angle = (worldRotation + eye.Rotation).Reduced().FlipPositive();
        Direction? overrideDirection = sprite.EnableDirectionOverride ? sprite.DirectionOverride : null;
        foreach (var (front, marking) in ent.Comp.Layers)
        {
            if ((marking.HiddenBy & slotFlags) == 0)
                continue;

            UpdateMarkingVisibility(entSprite, front, angle, overrideDirection, marking);
        }
    }
}
