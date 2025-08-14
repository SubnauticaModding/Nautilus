using Nautilus.Assets;

// ReSharper disable once CheckNamespace
namespace Nautilus.Handlers;

public static partial class EnumExtensions
{
    /// <summary>
    /// Adds an icon for this instance.
    /// </summary>
    /// <param name="builder">The current custom enum object instance.</param>
    /// <param name="sprite">The icon to add for this instance.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static EnumBuilder<PingType> WithIcon(this EnumBuilder<PingType> builder, UnityEngine.Sprite sprite)
    {
        PingType pingType = builder;
        var name = pingType.ToString();

        ModSprite.Add(SpriteManager.Group.Pings, pingType.ToString(), sprite);

        if(PingManager.sCachedPingTypeStrings.valueToString.ContainsKey(pingType) == false)
            PingManager.sCachedPingTypeStrings.valueToString.Add(pingType, name);

        if(PingManager.sCachedPingTypeTranslationStrings.valueToString.ContainsKey(pingType) == false)
            PingManager.sCachedPingTypeTranslationStrings.valueToString.Add(pingType, name);

        return builder;
    }
}