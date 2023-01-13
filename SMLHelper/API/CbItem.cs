namespace SMLHelper.API;

using System;
using System.Collections.Generic;
using Assets;
using SMLHelper.Utility;
using UnityEngine;
#if SUBNAUTICA
using Sprite = Atlas.Sprite;
#endif

/// <summary>
/// A class that holds all the necessary elements of a custom battery or power cell.
/// </summary>
public abstract class CbItem
{
    /// <summary>
    /// The full capacity of energy of the item.
    /// </summary>
    public int EnergyCapacity { get; set; } = -1;

    /// <summary>
    /// The internal ID for the custom item.
    /// </summary>
    public string ID { get; set; }

    /// <summary>
    /// The display name of the custom item shown in-game.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The flavor text for the custom item shown in-game when viewing it from a PDA screen.
    /// </summary>
    public string FlavorText { get; set; }

    /// <summary>
    /// The materials required to craft the item.<para/>
    /// If you want multiple copies of the same material, include multiple entries of that <see cref="TechType"/>.<para/>
    /// If this list is empty, a default recipe of a single <see cref="TechType.Titanium"/> will be applied instead.
    /// </summary>
    public List<TechType> CraftingMaterials { get; set; } = new List<TechType>();

    /// <summary>
    /// What item must be obtained, scanned, or built to unlock the battery and power cell.<para/>
    /// By default, the item will be unlocked at the start of the game.
    /// </summary>
    public TechType UnlocksWith { get; set; } = TechType.None;

    /// <summary>
    /// The custom sprite for the item's icon.<br/>
    /// This value is optional and will default to the standard icon for batteries or power cells.
    /// </summary>
    public Sprite CustomIcon { get; set; }

    /// <summary>
    /// The custom data that will make up your batteries model.<br/>
    /// This value is optional and will default to the standard model for batteries or power cells if left as null.
    /// </summary>
    public CBModelData CBModelData { get; set; }

    /// <summary>
    /// Override this optional value if you want to make changes to the your item's <see cref="GameObject"/> as it is being spawned from prefab.<br/>
    /// Use this if you want to add or modify components of your item.
    /// </summary>
    /// <param name="gameObject">The item's gameobject.</param>
    public Action<GameObject> EnhanceGameObject { get; set; }

    /// <summary>
    /// Override this to <c>false</c> if you do not want CustomBatteries to manage addeding of this item to the Fabricator crafting tree.<br/>
    /// This property is <c>true</c> by default.
    /// </summary>
    public bool AddToFabricator { get; set; } = true;

    private TechType _techType = TechType.None;

    /// <summary>
    /// After <see cref="CbBattery.Patch"/> method is invoked, this property will contain the <see cref="TechType"/> value for this item.
    /// </summary>
    public TechType TechType
    {
        get
        {
            if (_techType == TechType.None)
            {
                throw new InvalidOperationException("The Patch method must be called before you can access the TechType value.");
            }

            return _techType;
        }
    }            

    internal void Patch(ItemTypes itemType)
    {
        string name = this.GetType().Assembly.GetName().Name;
        InternalLogger.Info($"Received Custom {itemType} pack from '{name}'");

        // Check for required data
        string errors = string.Empty;

        if (this.EnergyCapacity <= 0)
            errors += "Missing required data 'EnergyCapacity" + Environment.NewLine;

        if (string.IsNullOrEmpty(this.ID))
            errors += "Missing required data 'ID'" + Environment.NewLine;

        if (string.IsNullOrEmpty(this.Name))
            errors += "Missing required data 'Name'" + Environment.NewLine;

        if (string.IsNullOrEmpty(this.FlavorText))
            errors += "Missing required data 'FlavorText'";

        if (!string.IsNullOrEmpty(errors))
        {
            string msg = "Unable to patch:" + Environment.NewLine + errors;
            InternalLogger.Error(msg);
            throw new InvalidOperationException(msg);
        }

        // Prepare
        var item = new CustomItem(this, itemType)
        {
            PluginPackName = name,
            FriendlyName = this.Name,
            Description = this.FlavorText,
            PowerCapacity = this.EnergyCapacity,
            RequiredForUnlock = this.UnlocksWith,
            Parts = this.CraftingMaterials
        };

        // Patch
        item.Patch();
            
        _techType = item.TechType;
    }
}