using System.Numerics;
using Content.Shared._Afterlight.Humanoid.Markings;
using Content.Shared.Inventory;
using Robust.Shared.Utility;

// ReSharper disable CheckNamespace

namespace Content.Shared.Humanoid.Markings;

public sealed partial class MarkingPrototype
{
    [DataField] public readonly Vector2 Offset;

    [DataField] public LocId[] Localization = Array.Empty<LocId>();

    [DataField] public bool AllSpecies;

    [DataField] public SlotFlags HiddenBy;

    [DataField] public ALMarkingLayer[] BackSprites = Array.Empty<ALMarkingLayer>();

    [DataField] public ALMarkingLayer[] SideSprites = Array.Empty<ALMarkingLayer>();
}
