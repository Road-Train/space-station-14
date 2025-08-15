using Content.Shared.Humanoid;

namespace Content.Shared._Afterlight.Sprite;

// No struct inheritance in C# WOO
[DataRecord]
public readonly record struct ALSpriteLayerKey(HumanoidVisualLayers? Enum, string? String, int? Int);
