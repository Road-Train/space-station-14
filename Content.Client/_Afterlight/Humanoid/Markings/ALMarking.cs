using Content.Shared.Humanoid;
using Content.Shared.Inventory;

namespace Content.Client._Afterlight.Humanoid.Markings;

[DataRecord]
public partial record struct ALMarking(string Back, SlotFlags HiddenBy, HumanoidVisualLayers Layer);
