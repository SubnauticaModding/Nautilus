namespace SMLHelper.Assets;

using System.Collections;
using System.IO;
using System.Reflection;
using Handlers;
using UnityEngine;

/// <summary>
/// An Vehicle Upgrade Module that can be crafted into the game world from a fabricator.
/// </summary>
/// <seealso cref="PdaItem" />
/// <seealso cref="Spawnable" />
/// <seealso cref="Craftable" />
public abstract class VehicleUpgradeModule : Equipable
{
    /// <summary>
    /// Initializes a new <see cref="VehicleUpgradeModule"/>, the basic class for any Vehicle Upgrade Module that can be crafted at a fabricator.
    /// </summary>
    /// <param name="classId"></param>
    /// <param name="friendlyName"></param>
    /// <param name="description"></param>
    protected VehicleUpgradeModule(string classId, string friendlyName, string description)
        : base(classId, friendlyName, description)
    {
        OnFinishedPatching += PostPatch;
    }

    /// <summary>
    /// Override with the main group in the PDA blueprints where this item appears.
    /// </summary>
    public sealed override TechGroup GroupForPDA => TechGroup.VehicleUpgrades;
        
    /// <summary>
    /// Override with the category within the group in the PDA blueprints where this item appears.
    /// </summary>
    public sealed override TechCategory CategoryForPDA => TechCategory.VehicleUpgrades;
        
    /// <summary>
    /// Default prefab instantiate using the prefabs <see cref="TechType"/> to choose it. 
    /// </summary>
    protected virtual TechType PrefabTemplate { get; } = TechType.ExoHullModule1;
        
    /// <summary>
    /// Override to set the <see cref="TechType"/> that must first be scanned or picked up to unlock the blueprint for this item.
    /// If not overriden, it this item will be unlocked from the start of the game.
    /// </summary>
    public override TechType RequiredForUnlock => TechType.BaseUpgradeConsole;
        
    /// <summary>
    /// Override with the vanilla fabricator that crafts this item.<para/>
    /// Leave this as <see cref="CraftTree.Type.None"/> if you are manually adding this item to a custom fabricator.
    /// </summary>
    public override CraftTree.Type FabricatorType => CraftTree.Type.SeamothUpgrades;
        
    /// <summary>
    /// Override with the tab node steps to take to get to the tab you want the item's blueprint to appear in.
    /// If not overriden, the item will appear at the craft tree's root.
    /// </summary>
    public override string[] StepsToFabricatorTab => new[] { "CommonModules" };
        
    /// <summary>
    /// Override with the folder where your mod's icons and other assets are stored.
    /// By default, this will point to the same folder where your mod DLL is.
    /// </summary>
    /// <example>"MyModAssembly/Assets"</example>
    public override string AssetsFolder => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Assets");
        

    /// <summary>
    /// Gets the type of equipment slot this item can fit into.
    /// </summary>
    /// <value>
    /// The type of the equipment slot compatible with this item.
    /// </value>
    public override EquipmentType EquipmentType => EquipmentType.VehicleModule;
        

    /// <summary>
    /// Gets the type of <see cref="QuickSlotType"/> this module uses for activation.
    /// </summary>
    /// <value>
    /// The type of <see cref="QuickSlotType"/> this module uses for activation.
    /// </value>
    public override QuickSlotType QuickSlotType => QuickSlotType.Passive;

    /// <summary>
    /// Gets the prefab game object. Set up your prefab components here.<para/>
    /// A default implementation is already provided which creates the new item by modifying a clone of the item defined in <see cref="PrefabTemplate"/>.
    /// </summary>
    /// <returns>
    /// The game object to be instantiated into a new in-game entity.
    /// </returns>
    public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
    {
        CoroutineTask<GameObject> task = CraftData.GetPrefabForTechTypeAsync(PrefabTemplate);
        yield return task;
        GameObject prefab = task.GetResult();
        GameObject obj = GameObject.Instantiate(prefab);

        gameObject.Set(obj);
    }

    /// <summary>
    /// Method you can override to alter the Newly created object.
    /// </summary>
    public virtual GameObject ModifyGameObject(GameObject obj)
    {
        return obj;
    }

    private void PostPatch()
    {
        CraftDataHandler.SetEquipmentType(TechType, EquipmentType);
        CraftDataHandler.SetQuickSlotType(TechType, QuickSlotType);
    }
}