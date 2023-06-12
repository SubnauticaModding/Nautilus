namespace Nautilus.Examples;

// Disable the naming convention violation warning.
#pragma warning disable IDE1006

[Nautilus.Options.Attributes.Menu("Vehicle Upgrade Example mod")]
public class ModConfig : Nautilus.Json.ConfigFile
{
    [Nautilus.Options.Attributes.Slider(Format = "{0:F0}m", Label = "Seamoth Upgrade Max Depth", Min = 100f, Max = 10000f, Step = 10f,
        Tooltip = "This is the max depth of the seamtoh when the depth module is equipped. It is absolute.")]
    public float MaxDepth = 500.0f;
}