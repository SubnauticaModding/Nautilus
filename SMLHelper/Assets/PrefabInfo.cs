using System.Diagnostics;
using System.Reflection;
using SMLHelper.Utility;
using UnityEngine;

namespace SMLHelper.Assets;

using Handlers;

/// <summary>
/// Data class that represents information about a prefab.
/// </summary>
/// <param name="ClassID">The class identifier used for the <see cref="PrefabIdentifier"/> component whenever applicable.</param>
/// <param name="PrefabFileName">Name of the prefab file.</param>
/// <param name="TechType">The <see cref="TechType"/> of the corresponding item.</param>
public record struct PrefabInfo(string ClassID, string PrefabFileName, TechType TechType)
{
    /// <summary>
    /// Constructs a new <see cref="PrefabInfo"/> instance with automatically set <see cref="PrefabFileName"/> and <see cref="TechType"/>.
    /// </summary>
    /// <param name="classId">The class identifier used for the <see cref="PrefabIdentifier"/> component whenever applicable.</param>
    /// <param name="displayName">The display name of this Tech Type, can be anything. If null or empty, this will use the language line "{enumName}" instead.</param>
    /// <param name="description">The tooltip displayed when hovered in the PDA, can be anything. If null or empty, this will use the language line "Tooltip_{enumName}" instead.</param>
    /// <param name="language">The language for this entry. Defaults to English.</param>
    /// <param name="unlockAtStart">Whether this tech type should be unlocked on game start or not. Default to <see langword="true"/>.</param>
    /// <param name="techTypeOwner">The assembly that owns the created tech type. The name of this assembly will be shown in the PDA.</param>
    /// <returns>An instance of the constructed <see cref="PrefabInfo"/>.</returns>
    public static PrefabInfo WithTechType(string classId, string displayName, string description, string language = "English", bool unlockAtStart = true, Assembly techTypeOwner = null)
    {
        techTypeOwner ??= Assembly.GetCallingAssembly();
        techTypeOwner = techTypeOwner == Assembly.GetExecutingAssembly()
            ? ReflectionHelper.CallingAssemblyByStackTrace()
            : techTypeOwner;
        return new PrefabInfo
        (
            classId, 
            classId + "Prefab",
            EnumHandler.AddEntry<TechType>(classId, techTypeOwner).WithPdaInfo(displayName, description, language, unlockAtStart)
        );
    }

#if SUBNAUTICA
    /// <summary>
    /// Adds an icon for <see cref="TechType"/>.
    /// </summary>
    /// <param name="sprite"></param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public PrefabInfo WithIcon(Atlas.Sprite sprite)
    {
        ModSprite.Add(SpriteManager.Group.None, TechType.ToString(), sprite);
        return this;
    }
#endif
    
    /// <summary>
    /// Adds an icon for <see cref="TechType"/>.
    /// </summary>
    /// <param name="sprite"></param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public PrefabInfo WithIcon(Sprite sprite)
    {
        ModSprite.Add(SpriteManager.Group.None, TechType.ToString(), sprite);
        return this;
    }

    /// <summary>
    /// Sets the size of this tech type in the inventory.
    /// </summary>
    /// <param name="size">The 2x2 vector size</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public PrefabInfo WithSizeInInventory(Vector2int size)
    {
        CraftDataHandler.SetItemSize(TechType, size);
        return this;
    }

    /// <summary>
    /// Sets the prefab file name as prefab info.
    /// </summary>
    /// <param name="fileName">The prefab file name.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public PrefabInfo WithFileName(string fileName)
    {
        return this with {PrefabFileName = fileName};
    }
}