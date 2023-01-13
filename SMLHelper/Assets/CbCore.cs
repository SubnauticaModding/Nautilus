namespace SMLHelper.Assets;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using API;
using Crafting;
using Handlers;
using Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif

/// <summary>
/// 
/// </summary>
public abstract class CbCore: Equipable
{
    private TechType prefabType = TechType.Battery;

    private List<TechType> AllowedPrefabTypes { get; } = new() { TechType.Battery, TechType.PrecursorIonBattery, TechType.PowerCell, TechType.PrecursorIonPowerCell };

    /// <summary>
    /// Can only ever be Battery, IonBattery, PowerCell or IonPowerCell
    /// </summary>
    public TechType PrefabType { 
        get => prefabType; 
        set { 

            if(AllowedPrefabTypes.Contains(value))
                prefabType = value; 
        } 
    }

    /// <summary>
    /// Sets the Charger type based on the PrefabType.
    /// If the PrefabType is a Battery or PrecursorIonBattery it will use BatteryCharger else it will use PowerCellCharger
    /// </summary>
    public sealed override EquipmentType EquipmentType => PrefabType == TechType.Battery || PrefabType == TechType.PrecursorIonBattery ? EquipmentType.BatteryCharger : EquipmentType.PowerCellCharger;

    /// <summary>
    /// Creates the RecipeData based on what is in the <see cref="Parts"/> list.
    /// If nothing is found then it will set the recipe to cost 1 Titanium.
    /// </summary>
    /// <returns></returns>
    protected override RecipeData GetBlueprintRecipe()
    {
        var partsList = new List<Ingredient>();
        CreateIngredients(Parts, partsList);

        if(partsList.Count == 0)
            partsList.Add(new Ingredient(TechType.Titanium, 1));

        var batteryBlueprint = new RecipeData
        {
            craftAmount = 1,
            Ingredients = partsList
        };

        return batteryBlueprint;
    }

    public float PowerCapacity { get; set; } = 100;

    public string PluginPackName { get; set; }

    public string PluginFolder { get; set; }

    public Sprite Sprite { get; set; }

    public IList<TechType> Parts { get; set; }

    public bool UsingIonCellSkins { get; }

    public CBModelData CustomModelData { get; set; }

    /// <summary>
    /// 
    /// </summary>
    protected Action<GameObject> EnhanceGameObject { get; set; }

    /// <summary>
    /// If true the battery will be added to the Fabricator.
    /// </summary>
    public bool AddToFabricator { get; set; } = true;

    /// <summary>
    /// Sets the default to the normal Fabricator for the Custom Bateries and PowerCells
    /// </summary>
    public override CraftTree.Type FabricatorType => AddToFabricator? CraftTree.Type.Fabricator : CraftTree.Type.None;


    /// <summary>
    /// Sets the default to Resources for the Custom Bateries and PowerCells
    /// </summary>
    public override TechGroup GroupForPDA => TechGroup.Resources;

    /// <summary>
    /// Sets the default to Electronics for the Custom Bateries and PowerCells
    /// </summary>
    public override TechCategory CategoryForPDA => TechCategory.Electronics;

    /// <summary>
    /// Initializes a new instance of the <see cref="CbCore"/> class which can be either a Batery or a PowerCell.
    /// </summary>
    /// <param name="classId">The main internal identifier for this item. Your item's <see cref="TechType" /> will be created using this name.</param>
    /// <param name="displayName">The name displayed in-game for this item whether in the open world or in the inventory.</param>
    /// <param name="description">The description for this item; Typically seen in the PDA, inventory, or crafting screens.</param>
    /// <param name="ionCellSkins">If the item is to be based off of the ion batteries or regular ones</param>
    protected CbCore(string classId, string displayName, string description, bool ionCellSkins = false)
        : base(classId, displayName, description)
    {
        UsingIonCellSkins = ionCellSkins;
        CorePatchEvents += PatchBatteryData;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CbCore"/> using the details from a <see cref="CbItem"/>.
    /// </summary>
    protected CbCore(CbItem packItem)
        : base(packItem.ID, packItem.Name, packItem.FlavorText)
    {
        if(packItem.CBModelData != null)
        {
            CustomModelData = packItem.CBModelData;
        }

        UsingIonCellSkins = packItem.CBModelData?.UseIonModelsAsBase ?? false;

        Sprite = packItem.CustomIcon;

        EnhanceGameObject = packItem.EnhanceGameObject;

        AddToFabricator = packItem.AddToFabricator;
        CorePatchEvents += PatchBatteryData;
    }

    /// <summary>
    /// Does some postprocessing on the batery prefabs to set any custom model details and then calls any <see cref="EnhanceGameObject"/> actions that have been registered.
    /// WARNING! if you override this you will have to do all this yourself or call base.ProcessPrefab(GameObject prefab) so that this can do its work!.
    /// </summary>
    /// <param name="prefab"></param>
    protected override void ProcessPrefab(GameObject prefab)
    {
        prefab.SetActive(false);
        Battery battery = prefab.GetComponent<Battery>();
        battery._capacity = PowerCapacity;
        battery.name = $"{ClassID}BatteryCell";

        // If "Enable batteries/powercells placement" feature from Decorations mod is ON.
#if SUBNAUTICA
        if(CbDatabase.PlaceBatteriesFeatureEnabled && CraftData.GetEquipmentType(TechType) != EquipmentType.Hand)
#elif BELOWZERO
            if (CbDatabase.PlaceBatteriesFeatureEnabled && TechData.GetEquipmentType(this.TechType) != EquipmentType.Hand)
#endif
        {
            CraftDataHandler.SetEquipmentType(TechType, EquipmentType.Hand); // Set equipment type to Hand.
            CraftDataHandler.SetQuickSlotType(TechType, QuickSlotType.Selectable); // We can select the item.
        }

        SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
        skyApplier.renderers = prefab.GetComponentsInChildren<Renderer>(true);
        skyApplier.anchorSky = Skies.Auto;

        if(CustomModelData != null)
        {
            foreach(Renderer renderer in prefab.GetComponentsInChildren<Renderer>(true))
            {
                if(CustomModelData.CustomTexture != null)
                    renderer.material.SetTexture(ShaderPropertyID._MainTex, CustomModelData.CustomTexture);

                if(CustomModelData.CustomNormalMap != null)
                    renderer.material.SetTexture(ShaderPropertyID._BumpMap, CustomModelData.CustomNormalMap);

                if(CustomModelData.CustomSpecMap != null)
                    renderer.material.SetTexture(ShaderPropertyID._SpecTex, CustomModelData.CustomSpecMap);

                if(CustomModelData.CustomIllumMap != null)
                {
                    renderer.material.SetTexture(ShaderPropertyID._Illum, CustomModelData.CustomIllumMap);
                    renderer.material.SetFloat(ShaderPropertyID._GlowStrength, CustomModelData.CustomIllumStrength);
                    renderer.material.SetFloat(ShaderPropertyID._GlowStrengthNight, CustomModelData.CustomIllumStrength);
                }
            }
        }

        base.ProcessPrefab(prefab);
        EnhanceGameObject?.Invoke(prefab);
    }


    public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
    {
        TaskResult<GameObject> task = new TaskResult<GameObject>();
        yield return CraftData.InstantiateFromPrefabAsync(prefabType, task);
        gameObject.Set(task.Get());
    }

    internal void CreateIngredients(IEnumerable<TechType> parts, List<Ingredient> partsList)
    {
        if(parts == null)
            return;

        foreach(TechType part in parts)
        {
            if(part == TechType.None)
            {
                InternalLogger.Warn($"Parts list for '{ClassID}' contained an unidentified TechType");
                continue;
            }

            Ingredient priorIngredient = partsList.Find(i => i.techType == part);

            if(priorIngredient != null)
#if SUBNAUTICA
                priorIngredient.amount++;
#elif BELOWZERO
                priorIngredient._amount++;
#endif
            else
                partsList.Add(new Ingredient(part, 1));
        }
    }

    private void PatchBatteryData()
    {
        if(EquipmentType == EquipmentType.BatteryCharger)
        {
            CbDatabase.BatteryItems.Add(this);
            CbDatabase.BatteryModels[TechType] = CustomModelData;
        }
        else if(EquipmentType == EquipmentType.PowerCellCharger)
        {
            CbDatabase.PowerCellModels[TechType] = CustomModelData;
            CbDatabase.PowerCellItems.Add(this);
        }

        CbDatabase.TrackItems.Add(TechType);
    }

    /// <summary>
    /// Looks for 
    /// </summary>
    /// <returns></returns>
    protected override Sprite GetItemSprite()
    {
        if(Sprite == null)
        {
            string imageFilePath = null;

            if(PluginFolder != null && IconFileName != null)
                imageFilePath = IOUtilities.Combine(PluginFolder, IconFileName);

            try
            {
                if(imageFilePath != null)
                {
                    if(!File.Exists(imageFilePath))
                    {
                        InternalLogger.Debug($"Failed to find icon for {ClassID} at {imageFilePath}, Using default sprite for {prefabType} instead.");
                        Sprite = SpriteManager.Get(PrefabType);
                    }
                    else
                    {
                        Sprite = ImageUtils.LoadSpriteFromFile(imageFilePath);
                    }
                }
                else
                {
                    Sprite = SpriteManager.Get(PrefabType);
                }
            }
            catch(Exception ex)
            {
                InternalLogger.Error($"Failed to load image file at {imageFilePath}! Using default sprite instead.\n {ex.Message}\n {ex.StackTrace}");
                Sprite = SpriteManager.Get(prefabType);
            }
        }

        return Sprite;
    }
}