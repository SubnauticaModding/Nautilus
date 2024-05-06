using Nautilus.Utility;

namespace Nautilus.Assets;

/// <summary>
/// Represents extension methods for the <see cref="CustomPrefab"/> class.
/// </summary>
public static class CustomPrefabExtensions
{
    /// <summary>
    /// Removes the current prefab from the prefab cache and doesn't allow it to get cached later.
    /// </summary>
    /// <param name="customPrefab">The custom prefab to remove from the prefab cache.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static ICustomPrefab RemoveFromCache(this CustomPrefab customPrefab)
    {
        if (customPrefab.Info == default)
        {
            return customPrefab;
        }

        if (string.IsNullOrWhiteSpace(customPrefab.Info.ClassID))
        {
            InternalLogger.Error($"Couldn't remove prefab '{customPrefab.Info}' from cache because the class ID is null.");
            return customPrefab;
        }
        
        ModPrefabCache.RemovePrefabFromCache(customPrefab.Info.ClassID);

        return customPrefab;
    }
}