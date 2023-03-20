using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SMLHelper.Handlers;
using SMLHelper.Patchers;
using SMLHelper.Utility;
using Story;
using UnityEngine;

namespace SMLHelper.Assets.Gadgets;

/// <summary>
/// Represents a scanning gadget.
/// </summary>
public class ScanningGadget : Gadget
{
    private const string DefaultUnlockMessage = "NotficationBlueprintUnlocked";

    private bool _isBuildable;

    /// <summary>
    /// The blueprint that must first be scanned or picked up to unlocked this item.
    /// </summary>
    public required TechType RequiredForUnlock { get; set; }
    
    /// <summary>
    /// Multiple blueprints that must first be scanned or picked up to unlocked this item.
    /// </summary>
    public List<TechType> CompoundTechsForUnlock { get; set; }

    /// <summary>
    /// Amount of <see cref="RequiredForUnlock"/> that must be scanned to unlock this item.
    /// </summary>
    public int FragmentsToScan { get; set; } = 1;

    /// <summary>
    /// The main group in the PDA blueprints where this item appears.
    /// </summary>
    public TechGroup GroupForPda { get; set; }
    
    /// <summary>
    /// The category within the group in the PDA blueprints where this item appears.
    /// </summary>
    public TechCategory CategoryForPda { get; set; }

    /// <summary>
    /// Adds an encyclopedia entry for this item in the PDA.
    /// </summary>
    public PDAEncyclopedia.EntryData EncyclopediaEntryData { get; set; }
    
    /// <summary>
    /// Additional logic on how the Scanner tool will interact with this item.
    /// </summary>
    public PDAScanner.EntryData ScannerEntryData { get; set; }
    
    /// <summary>
    /// Additional logic on what will happen when this item is unlocked.
    /// </summary>
    public KnownTech.AnalysisTech AnalysisTech { get; set; }

    /// <summary>
    /// Constructs a scanning gadget.
    /// </summary>
    /// <param name="prefab"><inheritdoc cref="Gadget(ICustomPrefab)"/></param>
    public ScanningGadget(ICustomPrefab prefab) : base(prefab) { }

    /// <summary>
    /// Constructs a scanning gadget.
    /// </summary>
    /// <param name="prefab"><inheritdoc cref="Gadget(ICustomPrefab)"/></param>
    /// <param name="requiredForUnlock">The blueprint that must first be unlocked to unlock this item.</param>
    /// <param name="fragmentsToScan">The amount of <see cref="RequiredForUnlock"/> that must be scanned to unlock this item.</param>
    [SetsRequiredMembers]
    public ScanningGadget(ICustomPrefab prefab, TechType requiredForUnlock, int fragmentsToScan = 1) : base(prefab)
    {
        RequiredForUnlock = requiredForUnlock;
        FragmentsToScan = fragmentsToScan;
    }

    /// <summary>
    /// Adds multiple blueprints that must first be scanned or picked up to unlocked this item.
    /// </summary>
    /// <param name="compoundTechs">The compound blueprints.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public ScanningGadget WithCompoundTechsForUnlock(List<TechType> compoundTechs)
    {
        CompoundTechsForUnlock = compoundTechs;
        return this;
    }

    /// <summary>
    /// Adds this item into a blueprint category to appear in.
    /// </summary>
    /// <param name="group">The main group in the PDA blueprints where this item appears</param>
    /// <param name="category">The category within the group in the PDA blueprints where this item appears.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <remarks>If the specified <paramref name="group"/> is a tech group that is present in the <see cref="uGUI_BuilderMenu.groups"/> list, this item will automatically
    /// become buildable. To avoid this, or make this item a buildable manually, use the <see cref="SetBuildable"/> method.</remarks>
    public ScanningGadget WithPdaGroupCategory(TechGroup group, TechCategory category)
    {
        GroupForPda = group;
        CategoryForPda = category;
        if (uGUI_BuilderMenu.groups.Contains(group))
        {
            SetBuildable();
        }
        return this;
    }

    /// <summary>
    /// Classifies this item as buildable via the habitat builder.
    /// </summary>
    /// <param name="isBuildable">Should this item be buildable?</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public ScanningGadget SetBuildable(bool isBuildable = true)
    {
        _isBuildable = isBuildable;
        return this;
    }

    /// <summary>
    /// Adds an encyclopedia entry for this item in the PDA.
    /// </summary>
    /// <param name="path">The path this entry will appear in.</param>
    /// <param name="popupSprite">The sprite that will popup once this entry is unlocked.</param>
    /// <param name="encyImage">The entry image that will appear in the encyclopedia entry</param>
    /// <param name="encyAudio">The audio that can be played in the entry.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public ScanningGadget WithEncyclopediaEntry(string path, Sprite popupSprite, Texture2D encyImage = null, FMODAsset encyAudio = null)
    {
        EncyclopediaEntryData = new PDAEncyclopedia.EntryData
        {
            key = prefab.Info.TechType.AsString(),
            path = path,
            nodes = path.Split('/'),
            popup = popupSprite,
            sound = encyAudio
        };

        return this;
    }

    /// <summary>
    /// Adds additional info on how the Scanner tool should treat this item when scanning it.
    /// </summary>
    /// <param name="scanTime">The amount of seconds it takes to scan this item.</param>
    /// <param name="isFragment">Is this a fragment?</param>
    /// <param name="encyKey">The encyclopedia key to unlock once the scanning is completed.</param>
    /// <param name="destroyAfterScan">Should this object be destroyed after a successful scan?</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public ScanningGadget WithScannerEntry(float scanTime, bool isFragment = false, string encyKey = null, bool destroyAfterScan = false)
    {
        ScannerEntryData = new PDAScanner.EntryData
        {
            key = RequiredForUnlock,
            encyclopedia = encyKey,
            scanTime = scanTime,
            destroyAfterScan = destroyAfterScan,
            isFragment = isFragment,
            blueprint = prefab.Info.TechType,
            totalFragments = FragmentsToScan,
        };

        return this;
    }

    /// <summary>
    /// Adds additional info on what should happen when this item is unlocked.
    /// </summary>
    /// <param name="popupSprite">The sprite that should popup on unlock.</param>
    /// <param name="storyGoalsToTrigger">The story goals that should be triggered on unlock.</param>
    /// <param name="unlockSound">The sound that will be played on unlock.</param>
    /// <param name="unlockMessage">Message which should be shown on unlock.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public ScanningGadget WithAnalysisTech(
        Sprite popupSprite, 
#if SUBNAUTICA
        List<StoryGoal> storyGoalsToTrigger = null,
#endif
        FMODAsset unlockSound = null, 
        string unlockMessage = "NotficationBlueprintUnlocked"
        )
    {
        AnalysisTech ??= new KnownTech.AnalysisTech();
        AnalysisTech.unlockPopup = popupSprite;
#if SUBNAUTICA
        AnalysisTech.storyGoals = storyGoalsToTrigger ?? new();
#endif
        AnalysisTech.unlockSound = unlockSound;
        AnalysisTech.unlockMessage = unlockMessage == DefaultUnlockMessage
            ? unlockMessage
            : $"{prefab.Info.TechType.AsString()}_DiscoverMessage";

        return this;
    }

    /// <inheritdoc/>
    protected internal override void Build()
    {
        if (prefab.Info.TechType is TechType.None)
        {
            InternalLogger.Error($"Prefab '{prefab.Info}' does not contain a TechType. Skipping {nameof(ScanningGadget)} build.");
            return;
        }
        
        if (GroupForPda != TechGroup.Uncategorized)
        {
            List<TechCategory> categories = new();
            CraftData.GetBuilderCategories(GroupForPda, categories);
            if (categories.Contains(CategoryForPda))
            {
                CraftDataHandler.AddToGroup(GroupForPda, CategoryForPda, prefab.Info.TechType);
            }
            else
            {
                InternalLogger.Error($"Failed to add {prefab.Info.TechType.AsString()} to {GroupForPda}/{CategoryForPda} as it is not a registered combination.");
            }
            
            if (_isBuildable)
                CraftDataHandler.AddBuildable(prefab.Info.TechType);
        }

        if (EncyclopediaEntryData is { })
        {
            PDAHandler.AddEncyclopediaEntry(EncyclopediaEntryData);
        }

        if (AnalysisTech is { })
        {
            KnownTechHandler.SetAnalysisTechEntry(AnalysisTech);
        }

        if (CompoundTechsForUnlock is { Count: > 0 })
        {
            KnownTechHandler.RemoveAllCurrentAnalysisTechEntry(prefab.Info.TechType);
            KnownTechHandler.SetCompoundUnlock(prefab.Info.TechType, CompoundTechsForUnlock);
        }

        if (ScannerEntryData is { })
        {
            PDAHandler.AddCustomScannerEntry(ScannerEntryData);
        }

        if (CompoundTechsForUnlock is { Count: > 0 } || RequiredForUnlock is not TechType.None)
        {
            KnownTechPatcher.UnlockedAtStart.Remove(prefab.Info.TechType);
        }
    }
}