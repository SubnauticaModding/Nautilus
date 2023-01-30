﻿namespace SMLHelper.Assets;

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
    /// <param name="displayName">The display name for this tech type.</param>
    /// <param name="description">The description for this tech type.</param>
    /// <returns>An instance of the constructed <see cref="PrefabInfo"/>.</returns>
    public static PrefabInfo WithTechType(string classId, string displayName, string description)
    {
        return new PrefabInfo
        (
            classId, 
            classId + "Prefab",
            EnumHandler.AddEntry<TechType>(classId).WithPdaInfo(displayName, description)
        );
    }

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