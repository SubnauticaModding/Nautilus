namespace SMLHelper.API;

internal class ModPluginPack : CustomPack
{
    internal ModPluginPack(IModPluginPack pluginPack, bool ionCellSkin)
        : base(pluginPack, ionCellSkin, false)
    {
        CustomBattery.Sprite = pluginPack.BatteryIcon;
        CustomPowerCell.Sprite = pluginPack.PowerCellIcon;
    }
}