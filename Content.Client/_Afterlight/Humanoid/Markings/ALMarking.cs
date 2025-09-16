using Content.Shared._Afterlight.Sprite;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;

namespace Content.Client._Afterlight.Humanoid.Markings;

[DataRecord]
public partial record struct ALMarking(
    string? Back,
    string? Side,
    SlotFlags HiddenBy,
    HumanoidVisualLayers Layer
);
