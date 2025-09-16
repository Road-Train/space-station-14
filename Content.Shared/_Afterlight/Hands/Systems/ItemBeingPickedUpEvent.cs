using Content.Shared.Item;

[ByRefEvent]
public record struct ItemBeingPickedUpEvent(EntityUid User, EntityUid Item)
{
    public bool Cancelled = false;
}
