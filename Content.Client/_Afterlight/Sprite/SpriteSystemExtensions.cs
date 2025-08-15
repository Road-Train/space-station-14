using Robust.Client.GameObjects;

namespace Content.Client._Afterlight.Sprite;

public static class SpriteSystemExtensions
{
    public static void LayerSetVisibleNullable(this SpriteSystem system, Entity<SpriteComponent?> sprite, string? key, bool value)
    {
        if (key == null)
            return;

        system.LayerSetVisible(sprite, key, value);
    }

    public static void LayerSetVisibleNullable(this SpriteSystem system, Entity<SpriteComponent?> sprite, Enum? key, bool value)
    {
        if (key == null)
            return;

        system.LayerSetVisible(sprite, key, value);
    }
}
