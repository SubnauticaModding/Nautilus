namespace SMLHelper.API;

internal class ModPluginPack : CustomPack
{
    internal ModPluginPack(IModPluginPack pluginPack, bool ionCellSkin)
        : base(pluginPack, ionCellSkin, false)
    {
        _customBattery.Sprite = pluginPack.BatteryIcon;
        _customPowerCell.Sprite = pluginPack.PowerCellIcon;
    }
}