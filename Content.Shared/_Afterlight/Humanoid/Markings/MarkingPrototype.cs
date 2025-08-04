using System.Numerics;
using Robust.Shared.Prototypes;

// ReSharper disable CheckNamespace

namespace Content.Shared.Humanoid.Markings;

public sealed partial class MarkingPrototype : IInheritingPrototype
{
    [DataField] public readonly Vector2 Offset;
}
