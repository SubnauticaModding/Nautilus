namespace SMLHelper.API;

using Assets;
using SMLHelper.Utility;

/// <summary>
/// A container class that a holds the modded battery and power cell prefab objects.
/// </summary>
public abstract class CustomPack
{
    private readonly EasyBattery _customBattery;
    private readonly EasyPowerCell _customPowerCell;

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
    public EasyBattery CustomBattery => _customBattery;

    /// <summary>
    /// Gets the custom power cell.
    /// </summary>
    /// <value>
    /// The custom power cell.
    /// </value>
    public EasyPowerCell CustomPowerCell => _customPowerCell;

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

    /// <summary>
    /// Used to make custom pack systems like CustomBatteries mod reads batteries from text files.
    /// </summary>
    /// <param name="pluginPack"></param>
    /// <param name="ionCellSkins"></param>
    /// <param name="customSkin"></param>
    public CustomPack(IPluginPack pluginPack, bool ionCellSkins, bool customSkin)
    {
        this.OriginalPlugInPack = pluginPack;
        this.UsingIonCellSkins = ionCellSkins;
        this.UsingCustomTextures = customSkin;

        _customBattery = new EasyBattery(pluginPack.BatteryID, pluginPack.BatteryName, pluginPack.BatteryFlavorText, ionCellSkins)
        {
            PluginPackName = pluginPack.PluginPackName,

            PowerCapacity = pluginPack.BatteryCapacity,
            UnlocksWith = pluginPack.UnlocksWith,
            Parts = pluginPack.BatteryParts
        };

        _customPowerCell = new EasyPowerCell(pluginPack.PowerCellID, pluginPack.PowerCellName, pluginPack.PowerCellFlavorText, _customBattery)
        {
            PluginPackName = pluginPack.PluginPackName,

            PowerCapacity = pluginPack.BatteryCapacity * 2f, // Power Cell capacity is always 2x the battery capacity
            UnlocksWith = pluginPack.UnlocksWith,
            Parts = pluginPack.PowerCellAdditionalParts
        };
    }

    /// <summary>
    /// Patch the Battery and PowerCell into the game.
    /// </summary>
    public void Patch()
    {
        InternalLogger.Info($"Patching plugin pack '{this.OriginalPlugInPack.PluginPackName}'");
        // Batteries must always patch before Power Cells
        _customBattery.Patch();
        _customPowerCell.Patch();
    }
}