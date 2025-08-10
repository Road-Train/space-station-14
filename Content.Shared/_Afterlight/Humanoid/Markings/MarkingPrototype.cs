using System.Numerics;
using Content.Shared.Inventory;
using Robust.Shared.Utility;

// ReSharper disable CheckNamespace

namespace Content.Shared.Humanoid.Markings;

public sealed partial class MarkingPrototype
{
    [DataField] public readonly Vector2 Offset;

    [DataField] public LocId[] Localization = Array.Empty<LocId>();

    [DataField] public SpriteSpecifier.Rsi[] BackSprites = Array.Empty<SpriteSpecifier.Rsi>();

    [DataField] public bool AllSpecies;

    [DataField] public SlotFlags HiddenBy;
}
