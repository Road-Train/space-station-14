namespace Content.Client._Afterlight.Humanoid.Markings;

[RegisterComponent]
public sealed partial class ALMarkingComponent : Component
{
    [DataField]
    public Dictionary<string, ALMarking> Layers = new();

    [DataField]
    public Angle? LastEyeRotation;

    [DataField]
    public Angle? LastWorldRotation;
}
