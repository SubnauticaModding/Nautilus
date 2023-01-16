namespace SMLHelper.Assets;

using System;
using System.Collections;
using System.Reflection;
using SMLHelper.API;
using SMLHelper.Handlers;
using SMLHelper.Patchers;
using SMLHelper.Utility;
using UnityEngine;

public class PrefabInfo: IEquatable<PrefabInfo>
{
    private ModPrefab modPrefab;

    /// <summary>
    /// The Assembly of the mod that added this prefab.
    /// </summary>
    public Assembly Mod { get; protected set; }

    /// <summary>
    /// The class identifier used for the <see cref="PrefabIdentifier" /> component whenever applicable.
    /// </summary>
    public string ClassID { get; set; }
    /// <summary>
    /// Name of the prefab file.
    /// </summary>
    public string PrefabFileName { get; set; }

    /// <summary>
    /// The <see cref="TechType"/> of the corresponding item.
    /// Used for <see cref="TechTag" />, and <see cref="Constructable" /> components whenever applicable.
    /// </summary>
    public TechType TechType { get; set; }

    /// <summary>
    /// Optional function you can pass in for the game to get your Prefab Object Synchronously.
    /// </summary>
    public Func<GameObject> GetGameObject { get; set; }

    /// <summary>
    /// Required Function you need to set for the game to get your Prefab Object Synchronously.
    /// </summary>
    public Func<IOut<GameObject>, IEnumerator> GetGameObjectAsync { get; private set; }

    /// <summary>
    /// Caches the prefab, then sets its TechType and ClassID to a default set of values applicable to most mods.<br/>
    /// FOR ADVANCED MODDING ONLY. Do not override unless you know exactly what you are doing.
    /// </summary>
    public Action<GameObject> ProcessPrefab { get; set; }

    /// <summary>
    /// The <see cref="Assets.ModPrefab"/> that is registered to this PrefabInfo if there is one.
    /// When this is set it will set the <see cref="GetGameObject"/>, <see cref="GetGameObjectAsync"/> and <see cref="ProcessPrefab"/> 
    /// to the <see cref="Assets.ModPrefab"/>'s methods if they have not already been set.
    /// </summary>
    public ModPrefab ModPrefab { 
        get => modPrefab; 
        private set
        {
            modPrefab = value;

            if(modPrefab != null)
            {
                if(GetGameObject == null)
                    GetGameObject = modPrefab.GetGameObject;
                if(GetGameObjectAsync == null)
                    GetGameObjectAsync = modPrefab.GetGameObjectAsync;
                if(ProcessPrefab == null)
                    ProcessPrefab = modPrefab.ProcessPrefab;
            }
        } 
    }

    internal PrefabInfo(ModPrefab modPrefab)
    {
        if(string.IsNullOrWhiteSpace(modPrefab.ClassID))
            throw new ArgumentNullException("classID cannot be null or empty spaces.");

        this.ClassID = modPrefab.ClassID.Replace(" ", "");
        this.PrefabFileName = string.IsNullOrWhiteSpace(modPrefab.PrefabFileName) ? ClassID + "Prefab" : modPrefab.PrefabFileName.Trim();
        this.TechType = modPrefab.TechType;
        this.Mod = modPrefab.Mod;
        this.ModPrefab = modPrefab;
    }

    private PrefabInfo(string classID, string prefabFileName = null, TechType techType = TechType.None)
    {
        if(string.IsNullOrWhiteSpace(classID))
            throw new ArgumentNullException("classID cannot be null or empty spaces.");

        this.ClassID = classID.Replace(" ", "");
        this.PrefabFileName = string.IsNullOrWhiteSpace(prefabFileName) ? classID + "Prefab" : prefabFileName.Replace(" ", "");
        this.TechType= techType;
    }

    /// <summary>
    /// Creates a PrefabInfo that can be registered into the game.
    /// </summary>
    /// <param name="classId">classID cannot be null or empty spaces.</param>
    /// <param name="getGameObjectAsync">Required Method for the game to get the actual prefab object.</param>
    /// <param name="prefabFileName">defaults to {classID}Prefab.</param>
    /// <param name="techType">Set the TechType if it already exists.</param>
    public static PrefabInfo Create(string classId, Func<IOut<GameObject>, IEnumerator> getGameObjectAsync, string prefabFileName, TechType techType)
    {
        return new PrefabInfo(classId, prefabFileName, techType) { GetGameObjectAsync = getGameObjectAsync };
    }

    /// <summary>
    /// Creates a PrefabInfo that can be registered into the game.
    /// </summary>
    /// <param name="classId">classID cannot be null or empty spaces.</param>
    /// <param name="getGameObjectAsync">Required Method for the game to get the actual prefab object.</param>
    /// <param name="prefabFileName">defaults to {classID}Prefab.</param>
    public static PrefabInfo Create(string classId, Func<IOut<GameObject>, IEnumerator> getGameObjectAsync, string prefabFileName = null)
    {
        return new PrefabInfo(classId, prefabFileName) { GetGameObjectAsync = getGameObjectAsync };
    }

    /// <summary>
    /// Creates a PrefabInfo that can be registered into the game.
    /// </summary>
    /// <param name="classId">classID cannot be null or empty spaces.</param>
    /// <param name="getGameObjectAsync">Required Method for the game to get the actual prefab object.</param>
    public static PrefabInfo Create(string classId, Func<IOut<GameObject>, IEnumerator> getGameObjectAsync)
    {
        return new PrefabInfo(classId) { GetGameObjectAsync = getGameObjectAsync };
    }

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
        if(modPrefab != null)
            modPrefab.TechType = this.TechType;
        return this;
    }

    /// <summary>
    /// Sets the general PDA data of this object.
    /// </summary>
    /// <param name="displayName"></param>
    /// <param name="description"></param>
    /// <param name="unlockAtStart"></param>
    public PrefabInfo WithPdaInfo(string displayName, string description, bool unlockAtStart = true)
    {
        if(TechType == TechType.None)
        {
            return this;
        }

        var name = TechType.ToString();
        var modName = Mod.GetName().Name;
        if(displayName is not null)
            LanguagePatcher.AddCustomLanguageLine(modName, name, displayName);

        if(description is not null)
        {
            LanguagePatcher.AddCustomLanguageLine(modName, "Tooltip_" + name, description);
            var valueToString = TooltipFactory.techTypeTooltipStrings.valueToString;
            valueToString[TechType] = "Tooltip_" + name;
        }

        if(unlockAtStart)
            KnownTechPatcher.UnlockedAtStart.Add(TechType);

        return this;
    }

    /// <summary>
    /// Registers a sprite to this prefabs TechType.
    /// </summary>
    /// <param name="sprite"></param>
    public PrefabInfo WithIcon(Atlas.Sprite sprite)
    {
        Mod = Mod ?? ReflectionHelper.CallingAssemblyByStackTrace();
        ModSprite.Add(SpriteManager.Group.None, TechType.ToString(), sprite);
        return this;
    }

    /// <summary>
    /// Registers a sprite to this prefabs TechType.
    /// </summary>
    /// <param name="sprite"></param>
    public PrefabInfo WithIcon(Sprite sprite)
    {
        Mod = Mod ?? ReflectionHelper.CallingAssemblyByStackTrace();
        ModSprite.Add(SpriteManager.Group.None, TechType.ToString(), sprite);
        return this;
    }

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

    internal GameObject GetGameObjectInternal()
    {
        if(GetGameObject == null)
            return null;

        GameObject go = GetGameObject();
        if(go == null)
        {
            return null;
        }
        
        ProcessPrefabInternal(go);
        return go;
    }

    internal IEnumerator GetGameObjectInternalAsync(IOut<GameObject> gameObject)
    {
        if(GetGameObjectAsync == null)
            yield break;

        TaskResult<GameObject> taskResult = new();
        yield return GetGameObjectAsync(taskResult);

        GameObject go = taskResult.Get();
        if(go == null)
        {
            yield break;
        }

        ProcessPrefabInternal(go);
        gameObject.Set(go);
    }


    internal void ProcessPrefabInternal(GameObject go)
    {
        if(ProcessPrefab != null)
        {
            ProcessPrefab(go);
            return;
        }

        if(go.activeInHierarchy) // inactive prefabs don't need to be removed by cache
        {
            ModPrefabCache.AddPrefab(go);
        }

        go.name = this.ClassID;
        var tech = TechType;

        if(tech != TechType.None)
        {
            if(CbDatabase.TrackItems.Contains(tech))
            {
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
            }

            if(go.GetComponent<TechTag>() is { } tag)
            {
                tag.type = tech;
            }

            if(go.GetComponent<Constructable>() is { } cs)
            {
                cs.techType = tech;
            }
        }

        if(go.GetComponent<PrefabIdentifier>() is { } pid)
        {
            pid.ClassId = ClassID;
        }
    }


    #endregion

}