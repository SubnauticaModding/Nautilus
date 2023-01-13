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

internal abstract class CbCore : ModPrefab
{
    protected abstract TechType PrefabType { get; } // Should only ever be Battery, IonBattery, PowerCell or IonPowerCell
    protected abstract EquipmentType ChargerType { get; } // Should only ever be BatteryCharger or PowerCellCharger

    public TechType RequiredForUnlock { get; set; } = TechType.None;
    public bool UnlocksAtStart => RequiredForUnlock == TechType.None;

    public virtual RecipeData GetBlueprintRecipe()
    {
        var partsList = new List<Ingredient>();

        CreateIngredients(Parts, partsList);

        if (partsList.Count == 0)
            partsList.Add(new Ingredient(TechType.Titanium, 1));

        var batteryBlueprint = new RecipeData
        {
            craftAmount = 1,
            Ingredients = partsList
        };

        return batteryBlueprint;
    }

    public float PowerCapacity { get; set; }

    public string FriendlyName { get; set; }

    public string Description { get; set; }

    public string IconFileName { get; set; }

    public string PluginPackName { get; set; }

    public string PluginFolder { get; set; }

    public Sprite Sprite { get; set; }

    public IList<TechType> Parts { get; set; }

    public bool IsPatched { get; private set; }

    public bool UsingIonCellSkins { get; }

    public CBModelData CustomModelData { get; set; }

    protected Action<GameObject> EnhanceGameObject { get; set; }

    public bool AddToFabricator { get; set; } = true;

    protected CbCore(string classId, bool ionCellSkins)
        : base(classId, $"{classId}PreFab", TechType.None)
    {
        UsingIonCellSkins = ionCellSkins;
    }

    protected CbCore(CbItem packItem)
        : base(packItem.ID, $"{packItem.ID}PreFab", TechType.None)
    {
        if (packItem.CBModelData != null)
        {
            CustomModelData = packItem.CBModelData;
        }

        UsingIonCellSkins = packItem.CBModelData?.UseIonModelsAsBase ?? false;

        Sprite = packItem.CustomIcon;

        EnhanceGameObject = packItem.EnhanceGameObject;

        AddToFabricator = packItem.AddToFabricator;
    }

    private GameObject ModifyPrefab(GameObject prefab)
    {
        var obj = GameObject.Instantiate(prefab);
        Battery battery = obj.GetComponent<Battery>();
        battery._capacity = PowerCapacity;
        battery.name = $"{ClassID}BatteryCell";

        // If "Enable batteries/powercells placement" feature from Decorations mod is ON.
#if SUBNAUTICA
        if (CbDatabase.PlaceBatteriesFeatureEnabled && CraftData.GetEquipmentType(TechType) != EquipmentType.Hand)
#elif BELOWZERO
            if (CbDatabase.PlaceBatteriesFeatureEnabled && TechData.GetEquipmentType(this.TechType) != EquipmentType.Hand)
#endif
        {
            CraftDataHandler.SetEquipmentType(TechType, EquipmentType.Hand); // Set equipment type to Hand.
            CraftDataHandler.SetQuickSlotType(TechType, QuickSlotType.Selectable); // We can select the item.
        }

        SkyApplier skyApplier = obj.EnsureComponent<SkyApplier>();
        skyApplier.renderers = obj.GetComponentsInChildren<Renderer>(true);
        skyApplier.anchorSky = Skies.Auto;

        if (CustomModelData != null)
        {
            foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>(true))
            {
                if (CustomModelData.CustomTexture != null)
                    renderer.material.SetTexture(ShaderPropertyID._MainTex, CustomModelData.CustomTexture);

                if (CustomModelData.CustomNormalMap != null)
                    renderer.material.SetTexture(ShaderPropertyID._BumpMap, CustomModelData.CustomNormalMap);

                if (CustomModelData.CustomSpecMap != null)
                    renderer.material.SetTexture(ShaderPropertyID._SpecTex, CustomModelData.CustomSpecMap);

                if (CustomModelData.CustomIllumMap != null)
                {
                    renderer.material.SetTexture(ShaderPropertyID._Illum, CustomModelData.CustomIllumMap);
                    renderer.material.SetFloat(ShaderPropertyID._GlowStrength, CustomModelData.CustomIllumStrength);
                    renderer.material.SetFloat(ShaderPropertyID._GlowStrengthNight, CustomModelData.CustomIllumStrength);
                }
            }
        }

        EnhanceGameObject?.Invoke(obj);

        return obj;
    }

    public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
    {
        CoroutineTask<GameObject> task = CraftData.GetPrefabForTechTypeAsync(PrefabType);
        yield return task;

        gameObject.Set(ModifyPrefab(task.GetResult()));
    }

    protected void CreateIngredients(IEnumerable<TechType> parts, List<Ingredient> partsList)
    {
        if (parts == null)
            return;

        foreach (TechType part in parts)
        {
            if (part == TechType.None)
            {
                InternalLogger.Warn($"Parts list for '{ClassID}' contained an unidentified TechType");
                continue;
            }

            Ingredient priorIngredient = partsList.Find(i => i.techType == part);

            if (priorIngredient != null)
#if SUBNAUTICA
                priorIngredient.amount++;
#elif BELOWZERO
                    priorIngredient._amount++;
#endif
            else
                partsList.Add(new Ingredient(part, 1));
        }
    }

    protected abstract void AddToList();

    protected abstract string[] StepsToFabricatorTab { get; }

    public void Patch()
    {
        if (IsPatched)
            return;

        TechType = EnumHandler.AddEntry<TechType>(ClassID).WithPdaInfo(FriendlyName, Description, UnlocksAtStart);

        ProcessBatterySkins();

        if (!UnlocksAtStart)
            KnownTechHandler.SetAnalysisTechEntry(RequiredForUnlock, new TechType[] { TechType });

        if (Sprite == null)
        {
            string imageFilePath = null;

            if (PluginFolder != null && IconFileName != null)
                imageFilePath = IOUtilities.Combine(CbDatabase.ExecutingFolder, PluginFolder, IconFileName);

            if (imageFilePath != null && File.Exists(imageFilePath))
                Sprite = ImageUtils.LoadSpriteFromFile(imageFilePath);
            else
            {
                InternalLogger.Warn($"Did not find a matching image file at {imageFilePath} or in {nameof(CbBattery.CustomIcon)}.{Environment.NewLine}Using default sprite instead.");
                Sprite = SpriteManager.Get(PrefabType);
            }
        }

        SpriteHandler.RegisterSprite(TechType, Sprite);

        CraftDataHandler.SetTechData(TechType, GetBlueprintRecipe());

        CraftDataHandler.AddToGroup(TechGroup.Resources, TechCategory.Electronics, TechType);

        CraftDataHandler.SetEquipmentType(TechType, ChargerType);

        if (AddToFabricator)
            CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, TechType, StepsToFabricatorTab);

        PrefabHandler.RegisterPrefab(this);

        AddToList();

        IsPatched = true;
    }

    private void ProcessBatterySkins()
    {
        if (CustomModelData != null)
        {
            if (ChargerType == EquipmentType.BatteryCharger && !CbDatabase.BatteryModels.ContainsKey(TechType))
            {
                CbDatabase.BatteryModels.Add(TechType, CustomModelData);
            }
            else if (ChargerType == EquipmentType.PowerCellCharger && !CbDatabase.PowerCellModels.ContainsKey(TechType))
            {
                CbDatabase.PowerCellModels.Add(TechType, CustomModelData);
            }
        }
        else
        {
            if (ChargerType == EquipmentType.BatteryCharger)
            {
                GameObject battery = CbDatabase.Battery();
                Material material = battery?.GetComponentInChildren<MeshRenderer>()?.material;

                Texture2D texture = material?.GetTexture(ShaderPropertyID._MainTex) as Texture2D;
                Texture2D bumpmap = material?.GetTexture(ShaderPropertyID._BumpMap) as Texture2D;
                Texture2D spec = material?.GetTexture(ShaderPropertyID._SpecTex) as Texture2D;
                Texture2D illum = material?.GetTexture(ShaderPropertyID._Illum) as Texture2D;
                float illumStrength = material?.GetFloat(ShaderPropertyID._GlowStrength) ?? 0;

                CbDatabase.BatteryModels.Add(TechType, CustomModelData);
            }
            else if (ChargerType == EquipmentType.PowerCellCharger)
            {
                GameObject battery = CbDatabase.PowerCell();
                Material material = battery?.GetComponentInChildren<MeshRenderer>()?.material;

                Texture2D texture = material?.GetTexture(ShaderPropertyID._MainTex) as Texture2D;
                Texture2D bumpmap = material?.GetTexture(ShaderPropertyID._BumpMap) as Texture2D;
                Texture2D spec = material?.GetTexture(ShaderPropertyID._SpecTex) as Texture2D;
                Texture2D illum = material?.GetTexture(ShaderPropertyID._Illum) as Texture2D;
                float illumStrength = material?.GetFloat(ShaderPropertyID._GlowStrength) ?? 0;

                CbDatabase.PowerCellModels.Add(TechType, CustomModelData);
            }
        }
    }
}