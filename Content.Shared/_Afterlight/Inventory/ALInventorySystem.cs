using Content.Shared.Inventory;

namespace Content.Shared._Afterlight.Inventory;

public sealed class ALInventorySystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;

    public bool HasItemEquipped(Entity<InventoryComponent?> ent, SlotFlags slotId)
    {
        var slots = _inventory.GetSlotEnumerator(ent, slotId);
        while (slots.MoveNext(out var slot))
        {
            if (slot.ContainedEntity != null)
                return true;
        }

        return false;
    }
}
