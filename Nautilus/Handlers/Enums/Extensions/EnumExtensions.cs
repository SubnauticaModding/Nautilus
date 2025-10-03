

using Nautilus.Handlers.Internals;
// ReSharper disable once CheckNamespace
using HarmonyLib;

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

    internal static void Register()
    {
        var onRegisterMethods = AccessTools.GetDeclaredMethods(typeof(EnumExtensions))
            .Where(m => m.GetCustomAttributes(false)
                .Any(att => att.GetType().IsGenericType && att.GetType().GetGenericTypeDefinition() == typeof(OnEnumRegisterAttribute<>))).ToArray();
        
        InternalLogger.Debug($"Found the following OnEnumRegister methods:{string.Join("\n", onRegisterMethods.Select(m => m.Name))}");

        foreach (var method in onRegisterMethods)
        {
            var attributes = method.GetCustomAttributes(false)
                .OfType<Attribute>()
                .Where(att => att.GetType().GetGenericTypeDefinition() == typeof(OnEnumRegisterAttribute<>));
            
            InternalLogger.Debug($"Adding enum register event for {method.Name}");

            foreach (var attribute in attributes)
            {
                var enumType = attribute.GetType().GetGenericArguments()[0];
                var delegateType = typeof(OnEnumRegistered<>).MakeGenericType(enumType);
                var @event = typeof(EnumHandler.Events<>).MakeGenericType(enumType).GetEvent(nameof(EnumHandler.Events<Enum>.OnEnumRegistered));
                @event.AddEventHandler(null, Delegate.CreateDelegate(delegateType, method));
                InternalLogger.Debug($"Added enum register event for {method.Name}<{enumType.Name}>");
            }
        }
    }
}