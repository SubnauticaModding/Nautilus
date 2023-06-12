using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using Nautilus.Examples.Upgrades;

namespace Nautilus.Examples;

[BepInPlugin("com.snmodding.nautilus.vehicle_example_upgrade", "Example Vehicle Upgrades", "1.0.0")]
public class Initializer : BaseUnityPlugin
{
    public static ModConfig Configs { get; } = Nautilus.Handlers.OptionsPanelHandler.RegisterModOptions<ModConfig>();

    private void Awake()
    {
        new SeaMothDepthUpgrade();
    }
}
