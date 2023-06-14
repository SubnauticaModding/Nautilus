// ReSharper disable once CheckNamespace

namespace Nautilus.Handlers;

using System;
using System.Linq;
using Utility;

/// <summary>
/// Extensions to interact more with custom enum objects.
/// </summary>
public static partial class EnumExtensions
{
    /// <summary>
    /// Checks if an enum value is defined by default or added using Nautilus.
    /// </summary>
    /// <param name="enumValue">The enum value to look for.</param>
    /// <typeparam name="TEnum">The type of the enum.</typeparam>
    /// <returns><see langword="true"/> if the specified enum value is defined by default, otherwise; <see langword="false"/>.</returns>
    public static bool IsDefinedByDefault<TEnum>(this TEnum enumValue) where TEnum : Enum
    {
        // We have not added any values to this enum so it must be vanilla or someone not using Nautilus.
        if (!EnumCacheProvider.TryGetManager(typeof(TEnum), out var manager))
        {
            return true;
        }
        
        if (manager.ModdedKeys.Contains(enumValue))
        {
            return false;
        }

        return true;
    }

}