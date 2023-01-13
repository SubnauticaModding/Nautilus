namespace SMLHelper.API;

using Assets;
using SMLHelper.Utility;

/// <summary>
/// A container class that a holds the modded battery and power cell prefab objects.
/// </summary>
public abstract class CustomPack
{
    internal readonly CustomBattery _customBattery;
    internal readonly CustomPowerCell _customPowerCell;

    /// <summary>
    /// Gets the original plugin pack.
    /// </summary>
    /// <value>
    /// The original plugin pack.
    /// </value>
    public IPluginPack OriginalPlugInPack { get; }

    /// <summary>
    /// Gets the custom battery.
    /// </summary>
    /// <value>
    /// The custom battery.
    /// </value>
    public ModPrefab CustomBattery => _customBattery;

    /// <summary>
    /// Gets the custom power cell.
    /// </summary>
    /// <value>
    /// The custom power cell.
    /// </value>
    public ModPrefab CustomPowerCell => _customPowerCell;

    /// <summary>
    /// Gets a value indicating whether the <see cref="CustomBattery"/> and <see cref="CustomPowerCell"/> have been patched.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this pack is patched; otherwise, <c>false</c>.
    /// </value>
    public bool IsPatched => _customBattery.IsPatched && _customPowerCell.IsPatched;

    /// <summary>
    /// Gets a value indicating whether the ion cell textures are being used.
    /// </summary>
    /// <value><c>True</c> if using the ion battery and ion power cell skins; Otherwise <c>false</c>.</value>
    public bool UsingIonCellSkins { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether custom textures are being used.
    /// </summary>
    /// <value><c>True</c> if using mod provided custom textures; Otherwise <c>false</c>.</value>
    public bool UsingCustomTextures { get; protected set; }

    internal CustomPack(IPluginPack pluginPack, bool ionCellSkins, bool customSkin)
    {
        this.OriginalPlugInPack = pluginPack;
        this.UsingIonCellSkins = ionCellSkins;
        this.UsingCustomTextures = customSkin;

        _customBattery = new CustomBattery(pluginPack.BatteryID, ionCellSkins)
        {
            PluginPackName = pluginPack.PluginPackName,
            FriendlyName = pluginPack.BatteryName,
            Description = pluginPack.BatteryFlavorText,

            PowerCapacity = pluginPack.BatteryCapacity,
            RequiredForUnlock = pluginPack.UnlocksWith,
            Parts = pluginPack.BatteryParts
        };

        _customPowerCell = new CustomPowerCell(pluginPack.PowerCellID, ionCellSkins, _customBattery)
        {
            PluginPackName = pluginPack.PluginPackName,
            FriendlyName = pluginPack.PowerCellName,
            Description = pluginPack.PowerCellFlavorText,

            PowerCapacity = pluginPack.BatteryCapacity * 2f, // Power Cell capacity is always 2x the battery capacity
            RequiredForUnlock = pluginPack.UnlocksWith,
            Parts = pluginPack.PowerCellAdditionalParts
        };
    }

    internal void Patch()
    {
        InternalLogger.Info($"Patching plugin pack '{this.OriginalPlugInPack.PluginPackName}'");
        // Batteries must always patch before Power Cells
        _customBattery.Patch();
        _customPowerCell.Patch();
    }
}