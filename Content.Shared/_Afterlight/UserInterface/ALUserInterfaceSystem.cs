using JetBrains.Annotations;

namespace Content.Shared._Afterlight.UserInterface;

public sealed class ALUserInterfaceSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    public void EnsureUI(Entity<UserInterfaceComponent?> ent, Enum key, string bui)
    {
        if (_ui.HasUi(ent, key))
            return;

        var data = new InterfaceData(bui);
        _ui.SetUi(ent.AsNullable(), key, data);
    }

    public void TryBui<T>(Entity<UserInterfaceComponent?> ent, [RequireStaticDelegate] Action<T> action) where T : BoundUserInterface
    {
        try
        {
            if (!Resolve(ent, ref ent.Comp, false))
                return;

            foreach (var bui in ent.Comp.ClientOpenInterfaces.Values)
            {
                if (bui is T dialogUi)
                    action(dialogUi);
            }
        }
        catch (Exception e)
        {
            Log.Error($"Error getting {nameof(T)}:\n{e}");
        }
    }
}
