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
}
