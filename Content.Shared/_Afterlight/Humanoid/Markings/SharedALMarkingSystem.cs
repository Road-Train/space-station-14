using Content.Shared.Humanoid.Markings;

namespace Content.Shared._Afterlight.Humanoid.Markings;

public abstract class SharedALMarkingSystem : EntitySystem
{
    public virtual void MarkingsCleared(EntityUid ent)
    {
    }

    public virtual void MarkingRemoved(EntityUid ent, string id)
    {
    }

    public void MarkingsRemoved(EntityUid ent, IReadOnlyList<Marking> markings)
    {
        foreach (var marking in markings)
        {
            MarkingRemoved(ent, marking.MarkingId);
        }
    }
}
