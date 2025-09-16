using Content.Shared.Item;
using Content.Shared.Starlight.Restrict;

namespace Content.Shared.Humanoid;

public sealed class ALHumanoidAppearanceSystem : EntitySystem
{
    [Dependency] private readonly SharedItemSystem _item = default!;

    public void AddPickupData(EntityUid uid)
    {
        TryComp<HumanoidAppearanceComponent>(uid, out var humanoid);
        if (humanoid != null)
        {
            EnsureComp<MultiHandedItemComponent>(uid);
            EnsureComp<RestrictNestingItemComponent>(uid);

            var item = EnsureComp<ItemComponent>(uid);

            // Default values
            var width = 6;
            var height = 4;
            _item.SetSize(uid, "Huge", item);
            _item.SetDirectPickup(uid, false, item);
            _item.SetShape(uid, new List<Box2i> { Box2i.FromDimensions(0, 0, height, width) }, item);
        }
    }
}
