namespace SMLHelper.API;

#if SUBNAUTICA
using Sprite = Atlas.Sprite;
#elif BELOWZERO
    using UnityEngine;
#endif
/// <summary>
/// An interface that defines all the necessary elements of a CustomBatteries mod plugin pack.
/// </summary>
/// <seealso cref="IPluginPack" />
public interface IModPluginPack : IPluginPack
{
    /// <summary>
    /// Gets the battery icon sprite.
    /// </summary>
    /// <value>
    /// The battery icon.
    /// </value>
    Sprite BatteryIcon { get; }

    /// <summary>
    /// Gets the power cell icon sprite.
    /// </summary>
    /// <value>
    /// The power cell icon.
    /// </value>
    Sprite PowerCellIcon { get; }
}