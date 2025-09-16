using Content.Shared._Afterlight.Sprite;
using Robust.Shared.Utility;

namespace Content.Shared._Afterlight.Humanoid.Markings;

// You might be wondering why Rsi here isn't just an IncludeDataField
// Presenting https://github.com/space-wizards/RobustToolbox/issues/6141
[DataRecord]
public readonly record struct ALMarkingLayer(
    ALSpriteLayerKey? Map,
    int Offset,
    SpriteSpecifier.Rsi? Rsi
);
