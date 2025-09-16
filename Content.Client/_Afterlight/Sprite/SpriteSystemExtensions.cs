using Robust.Client.GameObjects;

namespace Content.Client._Afterlight.Sprite;

public static class SpriteSystemExtensions
{
    public static void TryLayerSetVisible(this SpriteSystem system, Entity<SpriteComponent?> sprite, string? key, bool value)
    {
        if (key == null)
            return;

        if (!system.TryGetLayer(sprite, key, out var layer, false))
            return;

        system.LayerSetVisible(layer, value);
    }

    public static void TryLayerSetVisible(this SpriteSystem system, Entity<SpriteComponent?> sprite, Enum? key, bool value)
    {
        if (key == null)
            return;

        if (!system.TryGetLayer(sprite, key, out var layer, false))
            return;

        system.LayerSetVisible(layer, value);
    }
}
