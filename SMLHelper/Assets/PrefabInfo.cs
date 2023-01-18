namespace SMLHelper.Assets;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using SMLHelper.Assets.Interfaces;
using SMLHelper.Crafting;
using SMLHelper.Handlers;
using SMLHelper.Patchers;
using SMLHelper.Utility;
using UnityEngine;

/// <summary>
/// The core of the new Prefab system. <br/>
/// Holds all the relavent information for a prefab that is being added to the game.
/// </summary>
public class PrefabInfo: IEquatable<PrefabInfo>
{

    #region Definitions
    private string displayName;
    private string description;
    private TechType techType;

    /// <summary>
    /// The Assembly of the mod that added this prefab.
    /// </summary>
    public Assembly Mod { get; private set; }

    /// <summary>
    /// The class identifier used for the <see cref="PrefabIdentifier" /> component whenever applicable.
    /// </summary>
    public string ClassID { get; }

    /// <summary>
    /// Name of the prefab file.
    /// </summary>
    public string PrefabFileName { get; }

    /// <summary>
    /// The display name of this prefab that will show up in tooltips.
    /// </summary>
    public string DisplayName
    {
        get => displayName;
        set
        {
            displayName = value;
            if(!string.IsNullOrWhiteSpace(displayName))
            {
                var name = TechType != TechType.None ? TechType.ToString() : ClassID;
                var modName = Mod.GetName().Name;
                LanguagePatcher.AddCustomLanguageLine(modName, name, displayName);
            }
        }
    }

    /// <summary>
    /// The Description of this prefab that will show up in tooltips.
    /// </summary>
    public string Description
    {
        get => description;
        set
        {
            description = value;
            if(!string.IsNullOrWhiteSpace(description))
            {
                var name = TechType != TechType.None ? TechType.ToString() : ClassID;
                var modName = Mod.GetName().Name;
                LanguagePatcher.AddCustomLanguageLine(modName, "Tooltip_" + name, description);
                var valueToString = TooltipFactory.techTypeTooltipStrings.valueToString;
                valueToString[TechType] = "Tooltip_" + name;
            }
        }
    }

    /// <summary>
    /// The <see cref="TechType"/> of the corresponding item.
    /// Used for <see cref="TechTag" />, and <see cref="Constructable" /> components whenever applicable.
    /// </summary>
    public TechType TechType 
    { 
        get => techType; 
        private set
        {
            bool updateLanguage = techType != value && value != TechType.None;
            techType = value;

            if(updateLanguage)
            {
                if(!string.IsNullOrWhiteSpace(displayName))
                    DisplayName = displayName;

                if(!string.IsNullOrWhiteSpace(description))
                    Description = description;
            }
        } 
    }

    /// <summary>
    /// The <see cref="IModPrefab"/> that is registered to this PrefabInfo if there is one.
    /// When this is set it will set <see cref="IModPrefabSync.GetGameObject"/>, and <see cref="IProcessPrefabOverride.ProcessPrefab"/> if set.
    /// </summary>
    public object ModPrefab { get; private set; }

    #endregion


    #region Creation

    private PrefabInfo(string classID, string prefabFileName = null, TechType techType = TechType.None)
    {
        if(string.IsNullOrWhiteSpace(classID))
            throw new ArgumentNullException("classID cannot be null or empty spaces.");

        this.ClassID = classID.Replace(" ", "");
        this.PrefabFileName = string.IsNullOrWhiteSpace(prefabFileName) ? classID + "Prefab" : prefabFileName.Replace(" ", "");
        this.TechType = techType;
        this.Mod = ReflectionHelper.CallingAssemblyByStackTrace();
    }

    /// <summary>
    /// Creates a PrefabInfo that can be registered into the game.
    /// </summary>
    /// <param name="classId">classID cannot be null or empty spaces.</param>
    /// <param name="prefabFileName">defaults to {classID}Prefab.</param>
    /// <param name="techType">Set the TechType if it already exists.</param>
    public static PrefabInfo Create(string classId, string prefabFileName, TechType techType)
    {
        return new PrefabInfo(classId, prefabFileName, techType);
    }

    /// <summary>
    /// Creates a PrefabInfo that can be registered into the game.
    /// </summary>
    /// <param name="classId">classID cannot be null or empty spaces.</param>
    /// <param name="prefabFileName">defaults to {classID}Prefab.</param>
    public static PrefabInfo Create(string classId, string prefabFileName)
    {
        return new PrefabInfo(classId, prefabFileName);
    }

    /// <summary>
    /// Creates a PrefabInfo that can be registered into the game.
    /// </summary>
    /// <param name="classId">classID cannot be null or empty spaces.</param>
    public static PrefabInfo Create(string classId)
    {
        return new PrefabInfo(classId);
    }

    #endregion


    #region Extensions
    /// <summary>
    /// Creates the TechType associated with this prefab.
    /// </summary>
    /// <param name="customTechTypeName">Optional parameter if you want the TechType to be different then the <see cref="ClassID"/></param>
    public PrefabInfo CreateTechType(string customTechTypeName = null)
    {

        string techTypeName = (customTechTypeName ?? ClassID).Replace(" ", "");
        if(string.IsNullOrWhiteSpace(techTypeName))
            techTypeName = ClassID;

        Mod = Mod ?? ReflectionHelper.CallingAssemblyByStackTrace();
        this.TechType = EnumHandler.AddEntry<TechType>(techTypeName, Mod);
        KnownTechPatcher.UnlockedAtStart.Add(TechType);
        return this;
    }

    /// <summary>
    /// Sets the display name and tooltip discription of this object.
    /// </summary>
    /// <param name="displayName"></param>
    /// <param name="description"></param>
    public PrefabInfo WithLanguageLines(string displayName, string description)
    {
        if(displayName is not null)
        {
            this.DisplayName = displayName;
        }

        if(description is not null)
        {
            this.Description = description;
        }

        return this;
    }

    /// <summary>
    /// Sets the TechType required to unlock this prefab. If <see cref="TechType.None"/> then it will be set to unlock at start.
    /// </summary>
    public PrefabInfo WithUnlock(TechType techType)
    {
        if(TechType == TechType.None)
        {
            InternalLogger.Error($"Must create TechType for {ClassID} before setting Unlock");
            return this;
        }

        KnownTechHandler.RemoveAllCurrentAnalysisTechEntry(TechType);
        KnownTechHandler.AddAnalysisTech(techType, new List<TechType>() { TechType });
        return this;
    }

    /// <summary>
    /// Sets the TechType required to unlock this prefab. If <see cref="TechType.None"/> then it will be set to unlock at start.
    /// </summary>
    public PrefabInfo WithCompoundUnlock(List<TechType> CompoundTechsForUnlock)
    {
        if(CompoundTechsForUnlock is null || CompoundTechsForUnlock.Count == 0)
        {
            InternalLogger.Error($"Cannot create CompoundTech Unlock without any techs!");
            return this;
        }

        if(TechType == TechType.None)
        {
            InternalLogger.Error($"Must create TechType for {ClassID} before setting Unlock");
            return this;
        }

        KnownTechHandler.RemoveAllCurrentAnalysisTechEntry(TechType);
        KnownTechHandler.SetCompoundUnlock(TechType, CompoundTechsForUnlock);
        return this;
    }

    /// <summary>
    /// Sets the size in inventory that this object will have.
    /// </summary>
    public PrefabInfo WithInventorySize(int x, int y)
    {
        if(TechType == TechType.None)
        {
            InternalLogger.Error($"Must create TechType for {ClassID} before setting Inventory Size!");
            return this;
        }

        CraftDataHandler.SetItemSize(TechType, x, y);
        return this;
    }

    /// <summary>
    /// Sets the size in inventory that this object will have.
    /// </summary>
    public PrefabInfo WithInventorySize(Vector2int size)
    {
        if(TechType == TechType.None)
        {
            InternalLogger.Error($"Must create TechType for {ClassID} before setting Inventory Size!");
            return this;
        }

        CraftDataHandler.SetItemSize(TechType, size);
        return this;
    }

#if SUBNAUTICA
    /// <summary>
    /// Registers a sprite to this prefabs TechType.
    /// </summary>
    /// <param name="sprite"></param>
    public PrefabInfo WithIcon(Atlas.Sprite sprite)
    {
        if(TechType == TechType.None)
        {
            InternalLogger.Error($"Must create TechType for {ClassID} before setting Icon");
            return this;
        }

        Mod = Mod ?? ReflectionHelper.CallingAssemblyByStackTrace();
        ModSprite.Add(SpriteManager.Group.None, TechType.ToString(), sprite);
        return this;
    }
#endif

    /// <summary>
    /// Registers a sprite to this prefabs TechType.
    /// </summary>
    /// <param name="sprite"></param>
    public PrefabInfo WithIcon(Sprite sprite)
    {
        if(TechType == TechType.None)
        {
            InternalLogger.Error($"Must create TechType for {ClassID} before setting Icon");
            return this;
        }
        Mod = Mod ?? ReflectionHelper.CallingAssemblyByStackTrace();
        ModSprite.Add(SpriteManager.Group.None, TechType.ToString(), sprite);
        return this;
    }

    /// <summary>
    /// Registers a ModPrefab into the game.
    /// </summary>
    public PrefabInfo RegisterPrefab(object prefab)
    {
        if(ModPrefab != null)
        {
            InternalLogger.Warn($"Prefab already registered to {ClassID}!");
            return this;
        }

        foreach(PrefabInfo registeredInfo in ModPrefabCache.Prefabs)
        {
            var techtype = registeredInfo.TechType == TechType && registeredInfo.TechType != TechType.None;
            var classid = registeredInfo.ClassID == ClassID;
            var filename = registeredInfo.PrefabFileName == PrefabFileName;
            if(techtype || classid || filename)
            {
                InternalLogger.Error($"Another IModPrefab is already registered with these values. {(techtype ? "TechType: " + registeredInfo.TechType : "")} {(classid ? "ClassId: " + registeredInfo.ClassID : "")} {(filename ? "PrefabFileName: " + registeredInfo.PrefabFileName : "")}");
                return this;
            }
        }

        HandleInterfaces(prefab);
        ModPrefabCache.Add(this);
        ModPrefab = prefab;
        return this;
    }

    private void HandleInterfaces(object customPrefab)
    {
        var techType = TechType;

        if(customPrefab is ICraftable craftable && techType != TechType.None && craftable.FabricatorType != CraftTree.Type.None)
        {
            InternalLogger.Debug($"{ClassID} is ICraftable, Registering Craft Node, Recipe and Craft Speed.");

            if(craftable.StepsToFabricatorTab == null || craftable.StepsToFabricatorTab.Length == 0)
                CraftTreeHandler.AddCraftingNode(craftable.FabricatorType, techType);
            else
                CraftTreeHandler.AddCraftingNode(craftable.FabricatorType, techType, craftable.StepsToFabricatorTab);
            if(craftable.CraftingTime >= 0f)
                CraftDataHandler.SetCraftingTime(techType, craftable.CraftingTime);

            if(craftable.RecipeData != null)
                CraftDataHandler.SetTechData(techType, craftable.RecipeData);
            else
                CraftDataHandler.SetTechData(techType, new RecipeData() { craftAmount = 1, Ingredients = new() { new(TechType.Titanium, 1) } });

        }

        if(customPrefab is ICustomBattery customBattery)
        {
            InternalLogger.Debug($"{ClassID} is ICustomBattery, Adding to the Battery Registry.");
            var batteryType = customBattery.BatteryType;
            if(batteryType == BatteryType.Battery)
                CustomBatteryHandler.RegisterCustomBattery(this, customBattery);

            if(batteryType == BatteryType.PowerCell)
                CustomBatteryHandler.RegisterCustomPowerCell(this, customBattery);
        }

        if(customPrefab is IPDAInfo info)
        {
            InternalLogger.Debug($"{ClassID} is IPDAInfo, {info.GroupForPDA}/{info.CategoryForPDA} ");

            if(info.GroupForPDA != TechGroup.Uncategorized)
            {
                CraftDataHandler.AddToGroup(info.GroupForPDA, info.CategoryForPDA, techType);
                if(customPrefab is IBuildable)
                {
                    InternalLogger.Debug($"{ClassID} is IBuildable. Adding to buildables list.");
                    CraftDataHandler.AddBuildable(techType);
                }
            }
            else
            {
                if(customPrefab is IBuildable)
                    InternalLogger.Error($"{ClassID} is IBuildable but GroupForPDA is Uncategorized!");
                InternalLogger.Error($"{ClassID} is IPDAInfo but GroupForPDA is Uncategorized!");
            }
        }

        if(customPrefab is IEquipable equipable)
        {
            InternalLogger.Debug($"{ClassID} is IEquipable. {equipable.EquipmentType}:{equipable.QuickSlotType}");
            CraftDataHandler.SetEquipmentType(techType, equipable.EquipmentType);
            CraftDataHandler.SetQuickSlotType(techType, equipable.QuickSlotType);
        }

        if(customPrefab is IDistributionSpawn distributionSpawn)
        {
            InternalLogger.Debug($"{ClassID} is IDistributionSpawn.");
            if(distributionSpawn.EntityInfo != null && distributionSpawn.BiomesToSpawnIn != null)
                LootDistributionHandler.AddLootDistributionData(this, distributionSpawn.BiomesToSpawnIn, distributionSpawn.EntityInfo);
            else
                InternalLogger.Error($"{ClassID} is IDistributionSpawn but one or both of the required data are null.");
        }

        if(customPrefab is IStaticSpawn staticSpawn)
        {
            InternalLogger.Debug($"{ClassID} is IStaticSpawn.");
            if(staticSpawn.CoordinatedSpawns!= null)
            {
                foreach((Vector3 position, Vector3 eulerAngles) in staticSpawn.CoordinatedSpawns)
                {
                    CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(new SpawnInfo(ClassID, position, eulerAngles));
                }
            }
            else
            {
                InternalLogger.Error($"{ClassID} is IStaticSpawn but CoordinatedSpawns is null.");
            }
        }

        if(customPrefab is IEncyclopedia encyclopedia)
        {
            InternalLogger.Debug($"{ClassID} is IEncyclopedia.");
            if(encyclopedia.EncyclopediaEntryData != null)
                PDAHandler.AddEncyclopediaEntry(encyclopedia.EncyclopediaEntryData);
            else
                InternalLogger.Error($"{ClassID} is IEncyclopedia but EncyclopediaEntryData is null.");
        }

        if(customPrefab is IScannable scannable)
        {
            InternalLogger.Debug($"{ClassID} is IScannable.");
            if(scannable.ScannerEntryData != null)
            {
                if(customPrefab is IEncyclopedia enc && enc.EncyclopediaEntryData != null)
                    scannable.ScannerEntryData.encyclopedia = enc.EncyclopediaEntryData.key;

                PDAHandler.AddCustomScannerEntry(scannable.ScannerEntryData);
            }
            else
            {
                InternalLogger.Error($"{ClassID} is IScannable but ScannerEntryData is null.");
            }
        }
    }

#endregion


#region Equatability
    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj is PrefabInfo prefabInfo && Equals(prefabInfo);
    }

    /// <inheritdoc/>
    public bool Equals(PrefabInfo other)
    {
        if(ReferenceEquals(null, other))
            return false;
        if(ReferenceEquals(this, other))
            return true;
        return TechType == other.TechType &&
               string.Equals(ClassID, other.ClassID, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(PrefabFileName, other.PrefabFileName, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (int)TechType;
            hashCode = (hashCode * 397) ^ (ClassID != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(ClassID) : 0);
            hashCode = (hashCode * 397) ^ (PrefabFileName != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(PrefabFileName) : 0);
            return hashCode;
        }
    }

    /// <summary>
    /// Indicates whether two <see cref="PrefabInfo"/> instances are equal.
    /// </summary>
    /// <param name="p1">The first instance to compare.</param>
    /// <param name="p2">The second instance to compare.</param>
    /// <returns><see langword="true"/> if the <see cref="PrefabInfo"/> instances are equal; otherwise, <see langword="false"/>.</returns>
    /// <seealso cref="operator!="/>
    /// <seealso cref="Equals(PrefabInfo)"/>
    public static bool operator ==(PrefabInfo p1, PrefabInfo p2) => p1!.Equals(p2);

    /// <summary>
    /// Indicates whether two <see cref="PrefabInfo"/> instances are not equal.
    /// </summary>
    /// <param name="p1">The first instance to compare.</param>
    /// <param name="p2">The second instance to compare.</param>
    /// <returns><see langword="true"/> if the <see cref="PrefabInfo"/> instances are not equal; otherwise, <see langword="false"/>.</returns>
    /// <seealso cref="operator=="/>
    /// <seealso cref="Equals(PrefabInfo)"/>
    public static bool operator !=(PrefabInfo p1, PrefabInfo p2) => !(p1 == p2);

#endregion


#region GameObjectProcessing

    internal IEnumerator GetGameObjectInternalAsync(IOut<GameObject> gameObject)
    {
        if(ModPrefabCache.CachedPrefabs.TryGetValue(ClassID, out var cache))
        {
            gameObject.Set(cache.Item2);
            yield break;
        }

        GameObject prefab = null;
        TaskResult<GameObject> taskResult = new();

        if(ModPrefab is ICustomFabricator customFabricator)
        {
            switch(customFabricator.FabricatorModel)
            {
                case FabricatorModel.Workbench:
                    yield return CraftData.GetPrefabForTechTypeAsync(TechType.Workbench, false, taskResult);
                    prefab = GameObject.Instantiate(taskResult.Get());
                    break;
#if SUBNAUTICA
                case FabricatorModel.MoonPool:
                    var request = UWE.PrefabDatabase.GetPrefabForFilenameAsync("Submarine/Build/CyclopsFabricator.prefab");
                    yield return request;
                    request.TryGetPrefab(out prefab);
                    prefab = GameObject.Instantiate(prefab);
                    break;
#endif
                case FabricatorModel.Fabricator:
                    yield return CraftData.GetPrefabForTechTypeAsync(TechType.Fabricator, false, taskResult);
                    prefab = GameObject.Instantiate(taskResult.Get());
                    break;
            };

            if(prefab != null)
            {
                ProcessPrefabInternal(prefab);
                gameObject.Set(prefab);
                yield break;
            }
        }

        if(ModPrefab is ICustomBattery customBattery)
        {
            TechType batteryModelType = TechType.None;

            switch(customBattery.BatteryModel)
            {
                case BatteryModel.Battery:
                    batteryModelType = TechType.Battery;
                    break;
                case BatteryModel.IonBattery:
                    batteryModelType = TechType.PrecursorIonBattery;
                    break;
                case BatteryModel.PowerCell:
                    batteryModelType = TechType.PowerCell;
                    break;
                case BatteryModel.IonPowerCell:
                    batteryModelType = TechType.PrecursorIonPowerCell;
                    break;
            }

            if(batteryModelType != TechType.None)
            {
                yield return CraftData.GetPrefabForTechTypeAsync(batteryModelType, false, taskResult);
                prefab = GameObject.Instantiate(taskResult.Get());
                if(prefab != null)
                {
                    ProcessPrefabInternal(prefab);
                    gameObject.Set(prefab);
                    yield break;
                }
            }
        }

        if(ModPrefab is IModPrefabSync sync && sync.GetGameObject != null)
        {
            try
            {
                prefab = sync.GetGameObject();
            }
            catch(Exception ex)
            {
                InternalLogger.Error($"GetGameObject for {ClassID} threw Exception: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }

            if(prefab != null)
            {
                ProcessPrefabInternal(prefab);
                gameObject.Set(prefab);
                yield break;
            }
        }

        if(ModPrefab is not IModPrefab modPrefab || modPrefab.GetGameObjectAsync == null)
            yield break;

        yield return modPrefab.GetGameObjectAsync(taskResult);

        prefab = taskResult.Get();
        if(prefab == null)
        {
            yield break;
        }

        ProcessPrefabInternal(prefab);
        gameObject.Set(prefab);
    }


    internal void ProcessPrefabInternal(GameObject prefab)
    {
        if(ModPrefab is IProcessPrefabOverride prefabOverride && prefabOverride.ProcessPrefab != null)
        {
            prefabOverride.ProcessPrefab(prefab);
            return;
        }

        ModPrefabCache.AddPrefab(prefab, false);

        prefab.name = this.ClassID;
        var tech = TechType;

        if(tech != TechType.None)
        {

            if(prefab.GetComponent<TechTag>() is { } tag)
            {
                tag.type = tech;
            }

            if(prefab.GetComponent<Constructable>() is { } cs)
            {
                cs.techType = tech;
            }
        }

        if(prefab.GetComponent<PrefabIdentifier>() is { } pid)
        {
            pid.ClassId = ClassID;
        }

        if(ModPrefab is ICustomBattery customBattery)
        {
            Battery battery = prefab.EnsureComponent<Battery>();
            battery._capacity = customBattery.PowerCapacity;
            battery.name = $"{ClassID}BatteryCell";

            // If "Enable batteries/powercells placement" feature from Decorations mod is ON.
#if SUBNAUTICA
            if(CustomBatteriesPatcher.PlaceBatteriesFeatureEnabled && CraftData.GetEquipmentType(TechType) != EquipmentType.Hand)
#elif BELOWZERO
                if (CustomBatteriesPatcher.PlaceBatteriesFeatureEnabled && TechData.GetEquipmentType(this.TechType) != EquipmentType.Hand)
#endif
            {
                CraftDataHandler.SetEquipmentType(TechType, EquipmentType.Hand); // Set equipment type to Hand.
                CraftDataHandler.SetQuickSlotType(TechType, QuickSlotType.Selectable); // We can select the item.
            }
        }

        if(ModPrefab is ICustomModelData CustomModelData)
        {
            CustomModelData[] modelDatas = CustomModelData.ModelDatas;
            if(modelDatas != null)
            {
                var renderers = prefab.GetComponentsInChildren<Renderer>(true) ?? Array.Empty<Renderer>();
                foreach(Renderer renderer in renderers)
                {
                    foreach(var modelData in modelDatas)
                    {
                        if(modelData.TargetName != renderer.name)
                            continue;

                        if(modelData.CustomTexture != null)
                            renderer.material.SetTexture(ShaderPropertyID._MainTex, modelData.CustomTexture);

                        if(modelData.CustomNormalMap != null)
                            renderer.material.SetTexture(ShaderPropertyID._BumpMap, modelData.CustomNormalMap);

                        if(modelData.CustomSpecMap != null)
                            renderer.material.SetTexture(ShaderPropertyID._SpecTex, modelData.CustomSpecMap);

                        if(modelData.CustomIllumMap != null)
                        {
                            renderer.material.SetTexture(ShaderPropertyID._Illum, modelData.CustomIllumMap);
                            renderer.material.SetFloat(ShaderPropertyID._GlowStrength, modelData.CustomIllumStrength);
                            renderer.material.SetFloat(ShaderPropertyID._GlowStrengthNight, modelData.CustomIllumStrength);
                        }
                    }
                }
            }
        }

        if(ModPrefab is ICustomFabricator customFabricator)
        {

            Constructable constructible = null;
            GhostCrafter crafter;
            switch(customFabricator.FabricatorModel)
            {
                case FabricatorModel.Fabricator:
                default:
                    crafter = prefab.GetComponent<Fabricator>();
                    break;
                case FabricatorModel.Workbench:
                    crafter = prefab.GetComponent<Workbench>();
                    break;
#if SUBNAUTICA
                case FabricatorModel.MoonPool:
                    crafter = prefab.GetComponent<Fabricator>();

                    PrefabIdentifier prefabId = prefab.EnsureComponent<PrefabIdentifier>();
                    prefabId.ClassId = this.ClassID;
                    if(this.DisplayName != null)
                        prefabId.name = this.DisplayName;

                    TechTag techTag = prefab.EnsureComponent<TechTag>();
                    techTag.type = this.TechType;

                    // Retrieve sub game objects
                    GameObject cyclopsFabLight = prefab.FindChild("fabricatorLight");
                    GameObject cyclopsFabModel = prefab.FindChild("submarine_fabricator_03");
                    // Translate CyclopsFabricator model and light
                    prefab.transform.localPosition = new Vector3(cyclopsFabModel.transform.localPosition.x, // Same X position
                                                                 cyclopsFabModel.transform.localPosition.y - 0.8f, // Push towards the wall slightly
                                                                 cyclopsFabModel.transform.localPosition.z); // Same Z position
                    prefab.transform.localPosition = new Vector3(cyclopsFabLight.transform.localPosition.x, // Same X position
                                                                 cyclopsFabLight.transform.localPosition.y - 0.8f, // Push towards the wall slightly
                                                                 cyclopsFabLight.transform.localPosition.z); // Same Z position
                    // Add constructable - This prefab normally isn't constructed.
                    prefab.EnsureComponent<Constructable>().model = cyclopsFabModel;
                    break;
#endif
                case FabricatorModel.Custom:
                    crafter = prefab.EnsureComponent<Fabricator>();
                    break;
            }

            crafter.craftTree = customFabricator.TreeTypeID;
            if(this.DisplayName != null)
                crafter.handOverText = $"Use {this.DisplayName}";

            constructible = prefab.GetComponent<Constructable>();

            if(constructible != null)
            {
                constructible.allowedInBase = true;
                constructible.allowedInSub = true;
                constructible.allowedOutside = false;
                constructible.allowedOnCeiling = false;
                constructible.allowedOnGround = customFabricator.FabricatorModel == FabricatorModel.Workbench;
                constructible.allowedOnWall = customFabricator.FabricatorModel != FabricatorModel.Workbench;
                constructible.allowedOnConstructables = false;
                constructible.controlModelState = true;
                constructible.rotationEnabled = customFabricator.FabricatorModel == FabricatorModel.Workbench;
                constructible.techType = this.TechType;
            }

            var active = prefab.activeSelf;

            prefab.SetActive(false);
            SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
            skyApplier.renderers = prefab.GetComponentsInChildren<Renderer>();
            skyApplier.anchorSky = Skies.Auto;

            if(active)
                prefab.SetActive(active);

            crafter.powerRelay = PowerSource.FindRelay(prefab.transform);
        }
    }
#endregion
}