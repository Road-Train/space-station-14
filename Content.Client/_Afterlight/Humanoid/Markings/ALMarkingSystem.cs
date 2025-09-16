using Content.Client._Afterlight.Sprite;
using Content.Shared._Afterlight.Humanoid.Markings;
using Content.Shared._Afterlight.Inventory;
using Content.Shared._Afterlight.Movement;
using Content.Shared.Clothing.EntitySystems;
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

    private EntityQuery<EyeComponent> _eyeQuery;
    private EntityQuery<SpriteComponent> _spriteQuery;
    private EntityQuery<TransformComponent> _transformQuery;

    public override void Initialize()
    {
        _eyeQuery = GetEntityQuery<EyeComponent>();
        _spriteQuery = GetEntityQuery<SpriteComponent>();
        _transformQuery = GetEntityQuery<TransformComponent>();

        SubscribeLocalEvent<ActorComponent, ALRelativeRotationChangedEvent>(OnActorRelativeRotationChanged);

        SubscribeLocalEvent<ALMarkingComponent, ComponentStartup>(OnMarkingMapInit, after: [typeof(ClothingSystem), typeof(InventorySystem)]);
        SubscribeLocalEvent<ALMarkingComponent, MoveEvent>(OnMarkingMove);
        SubscribeLocalEvent<ALMarkingComponent, DidEquipEvent>(OnMarkingDidEquip);
        SubscribeLocalEvent<ALMarkingComponent, DidUnequipEvent>(OnMarkingDidUnequip);
        SubscribeLocalEvent<ALMarkingComponent, ALMarkingsSpriteUpdatedEvent>(OnMarkingsSpriteUpdated);
    }

    private void OnActorRelativeRotationChanged(Entity<ActorComponent> ent, ref ALRelativeRotationChangedEvent args)
    {
        if (ent.Comp.PlayerSession.UserId != _player.LocalUser)
            return;

        var eyeRotation = GetLocalEyeRotation();
        var query = EntityQueryEnumerator<ALMarkingComponent, SpriteComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var sprite, out var xform))
        {
            UpdateSprite((uid, comp, sprite, xform), eyeRotation);
        }
    }

    private void OnMarkingMapInit(Entity<ALMarkingComponent> ent, ref ComponentStartup args)
    {
        UpdateEntity(ent, false);
    }

    private void OnMarkingMove(Entity<ALMarkingComponent> ent, ref MoveEvent args)
    {
        UpdateEntity(ent);
    }

    private void OnMarkingDidEquip(Entity<ALMarkingComponent> ent, ref DidEquipEvent args)
    {
        MarkingEquippedChanged(ent, args.SlotFlags);
    }

    private void OnMarkingDidUnequip(Entity<ALMarkingComponent> ent, ref DidUnequipEvent args)
    {
        MarkingEquippedChanged(ent, args.SlotFlags);
    }

    private void OnMarkingsSpriteUpdated(Entity<ALMarkingComponent> ent, ref ALMarkingsSpriteUpdatedEvent args)
    {
        UpdateEntity(ent, false);
    }

    private void UpdateEntity(Entity<ALMarkingComponent> ent, bool cached = true)
    {
        if (!_spriteQuery.TryComp(ent, out var sprite) ||
            !_transformQuery.TryComp(ent, out var transform))
        {
            return;
        }

        var eyeRotation = GetLocalEyeRotation();
        UpdateSprite((ent, ent, sprite, transform), eyeRotation, cached);
    }

    private void UpdateSprite(Entity<ALMarkingComponent, SpriteComponent, TransformComponent> ent, Angle eyeRotation, bool cached = true)
    {
        if (IsClientSide(ent))
            return;

        var worldRotation = _transform.GetWorldRotation(ent.Comp3);
        if (cached &&
            ent.Comp1.LastEyeRotation == eyeRotation &&
            ent.Comp1.LastWorldRotation == worldRotation)
        {
            return;
        }

        ent.Comp1.LastEyeRotation = eyeRotation;
        ent.Comp1.LastWorldRotation = worldRotation;

        var angle = GetTotalAngle(worldRotation, eyeRotation);
        Direction? overrideDirection = ent.Comp2.EnableDirectionOverride ? ent.Comp2.DirectionOverride : null;
        var entSprite = new Entity<SpriteComponent?>(ent, ent.Comp2);
        foreach (var (frontLayer, marking) in ent.Comp1.Layers)
        {
            UpdateMarkingVisibility(entSprite, frontLayer, angle, overrideDirection, marking);
        }
    }

    private void TryCreateLayer(Entity<SpriteComponent?> entity,
        string layerId,
        MarkingPrototype markingPrototype,
        bool visible,
        ALMarkingLayer? markingLayerNullable,
        Color? color,
        int offset,
        SpriteSpecifier markingSprite)
    {
        if (markingLayerNullable is not { } markingLayer)
            return;

        if (_sprite.TryGetLayer(entity, layerId, out _, false))
            return;

        int layerIndex;
        // One day SpriteComponent will be in shared and have sensible layering
        if (markingLayer.Map == null)
            layerIndex = 0;
        else if (markingLayer.Map?.Enum is { } enumValue)
            _sprite.LayerMapTryGet(entity, enumValue, out layerIndex, false);
        else if (markingLayer.Map?.String is { } stringValue)
            _sprite.LayerMapTryGet(entity, stringValue, out layerIndex, false);
        else if (markingLayer.Map?.Int is { } intValue)
            layerIndex = intValue;
        else
            return;

        layerIndex += markingLayer.Offset + offset;
        var rsi = markingLayer.Rsi ?? markingSprite;
        if (!_sprite.LayerMapTryGet(entity, layerId, out _, false))
        {
            var layer = _sprite.AddLayer(entity, rsi, layerIndex);
            _sprite.LayerMapSet(entity, layerId, layer);
            _sprite.LayerSetSprite(entity, layerId, rsi);
        }

        _sprite.LayerSetVisible(entity, layerId, visible);
        _sprite.LayerSetOffset(entity, layerId, markingPrototype.Offset);
        _sprite.LayerSetColor(entity, layerId, color ?? Color.White);
    }

    public void ApplyMarking(Entity<SpriteComponent?> entity,
        string layerId,
        MarkingPrototype markingPrototype,
        int offset,
        bool visible,
        IReadOnlyList<Color>? colors,
        ref int targetLayer,
        SpriteSpecifier markingSprite)
    {
        _sprite.LayerSetOffset(entity, layerId, markingPrototype.Offset);
        if (markingPrototype.HiddenBy == SlotFlags.NONE &&
            markingPrototype.BackSprites.Length == 0 &&
            markingPrototype.SideSprites.Length == 0)
        {
            return;
        }

        var markingComp = EnsureComp<ALMarkingComponent>(entity.Owner);
        var marking = markingComp.Layers.GetOrNew(layerId);
        marking.HiddenBy = markingPrototype.HiddenBy;
        marking.Layer = markingPrototype.BodyPart;
        Color? color = colors == null || offset >= colors.Count ? null : colors[offset];
        if (markingPrototype.BackSprites.TryGetValue(offset, out var backSprite))
        {
            var behindLayer = $"al_{layerId}_behind";
            TryCreateLayer(entity, behindLayer, markingPrototype, visible, backSprite, color, offset, markingSprite);
            marking.Back = behindLayer;
        }

        if (markingPrototype.SideSprites.TryGetValue(offset, out var sideSprite))
        {
            var sideLayer = $"al_{layerId}_side";
            TryCreateLayer(entity, sideLayer, markingPrototype, visible, sideSprite, color, offset, markingSprite);
            marking.Side = sideLayer;
        }

        markingComp.Layers[layerId] = marking;

        // we do this so targetLayer doesn't get out of sync in the calling method
        _sprite.LayerMapTryGet(entity, markingPrototype.BodyPart, out targetLayer, false);
    }

    public override void MarkingsCleared(EntityUid ent)
    {
        RemCompDeferred<ALMarkingComponent>(ent);
    }

    private RsiDirection GetFacingDirection(Entity<SpriteComponent?> ent, string frontLayer, Angle angle, Direction? overrideDirection)
    {
        _sprite.TryGetLayer(ent, frontLayer, out var markingLayer, false);
        var state = markingLayer?.ActualState;
        var dir = state == null
            ? RsiDirection.South
            : SpriteComponent.Layer.GetDirection(state.RsiDirections, angle);
        if (overrideDirection != null && state != null)
            dir = overrideDirection.Value.Convert(state.RsiDirections);

        if (markingLayer != null)
            dir = dir.OffsetRsiDir(markingLayer.DirOffset);

        return dir;
    }

    private void UpdateMarkingVisibility(Entity<SpriteComponent?> ent,
        string frontLayerId,
        Angle angle,
        Direction? overrideDirection,
        ALMarking marking)
    {
        if (IsHiddenByEquipment(ent.Owner, marking))
        {
            _sprite.TryLayerSetVisible(ent, frontLayerId, false);
            _sprite.TryLayerSetVisible(ent, marking.Side, false);
            _sprite.TryLayerSetVisible(ent, marking.Back, false);
            return;
        }

        var facingDirection = GetFacingDirection(ent, frontLayerId, angle, overrideDirection);
        var sideFacing = facingDirection is RsiDirection.West or RsiDirection.East && marking.Side != null;
        var frontFacing = facingDirection != RsiDirection.North || marking.Back == null;
        if (sideFacing)
        {
            _sprite.TryLayerSetVisible(ent, frontLayerId, false);
            _sprite.TryLayerSetVisible(ent, marking.Side, true);
            _sprite.TryLayerSetVisible(ent, marking.Back, false);
        }
        else if (frontFacing)
        {
            _sprite.TryLayerSetVisible(ent, frontLayerId, true);
            _sprite.TryLayerSetVisible(ent, marking.Side, false);
            _sprite.TryLayerSetVisible(ent, marking.Back, false);
        }
        else
        {
            _sprite.TryLayerSetVisible(ent, frontLayerId, false);
            _sprite.TryLayerSetVisible(ent, marking.Side, false);
            _sprite.TryLayerSetVisible(ent, marking.Back, true);
        }
    }

    private void MarkingEquippedChanged(Entity<ALMarkingComponent> ent, SlotFlags slotFlags)
    {
        if (!_spriteQuery.TryComp(ent, out var sprite))
            return;

        var entSprite = new Entity<SpriteComponent?>(ent, sprite);
        var worldRotation = _transform.GetWorldRotation(ent);
        var eyeRotation = GetLocalEyeRotation();
        var angle = GetTotalAngle(worldRotation, eyeRotation);
        Direction? overrideDirection = sprite.EnableDirectionOverride ? sprite.DirectionOverride : null;
        foreach (var (front, marking) in ent.Comp.Layers)
        {
            if ((marking.HiddenBy & slotFlags) == 0)
                continue;

            UpdateMarkingVisibility(entSprite, front, angle, overrideDirection, marking);
        }
    }

    private Angle GetLocalEyeRotation()
    {
        return _player.LocalEntity == null
            ? Angle.Zero
            : _eyeQuery.CompOrNull(_player.LocalEntity.Value)?.Rotation ?? Angle.Zero;
    }

    private Angle GetTotalAngle(Angle worldRotation, Angle eyeRotation)
    {
        return (worldRotation + eyeRotation).Reduced().FlipPositive();
    }

    private bool IsHiddenByEquipment(Entity<InventoryComponent?> inventory, ALMarking marking)
    {
        return marking.HiddenBy != SlotFlags.NONE &&
               _alInventory.HasItemEquipped(inventory, marking.HiddenBy);
    }
}
