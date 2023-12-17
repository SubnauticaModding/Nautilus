using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nautilus.Handlers;
using Nautilus.Patchers;
using Nautilus.Utility;
using Story;
using UnityEngine;

namespace Nautilus.Assets.Gadgets;

/// <summary>
/// Represents a scanning gadget.
/// </summary>
public class ScanningGadget : Gadget
{
    /// <summary>
    /// Classifies this item as buildable via the habitat builder.
    /// </summary>
    public bool IsBuildable { get; private set; }
    
    /// <summary>
    /// Marks this item as hard locked.
    /// </summary>
    public bool IsHardLocked { get; private set; }

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
    public TechGroup GroupForPda { get; set; } = TechGroup.Uncategorized;
    
    /// <summary>
    /// The category within the group in the PDA blueprints where this item appears.
    /// </summary>
    public TechCategory CategoryForPda { get; set; }

    /// <summary>
    /// Defines the insertion position for the new blueprint in relation to the <see cref="PdaSortTarget"/> in the PDA.
    /// </summary>
    public enum SortPosition
    {
        /// <summary>
        /// Use this to insert the new blueprint before the <see cref="PdaSortTarget"/> or at the beginning if not found.
        /// </summary>
        InsertBefore,

        /// <summary>
        /// Use this to append the new blueprint after the <see cref="PdaSortTarget"/> or at the end if not found.
        /// </summary>
        AppendAfter
    }

    /// <summary>
    /// Whether the blueprint is inserted before or appended after the <see cref="PdaSortTarget"/> in the PDA.
    /// </summary>
    public SortPosition PdaSortPosition { get; set; }

    /// <summary>
    /// It will be added/inserted next to this item or at the end/beginning if not found.
    /// </summary>
    public TechType PdaSortTarget { get; set; }

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
    /// <param name="group">The main group in the PDA blueprints where this item appears.</param>
    /// <param name="category">The category within the group in the PDA blueprints where this item appears.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <remarks>If the specified <paramref name="group"/> is a tech group that is present in the <see cref="uGUI_BuilderMenu.groups"/> list, this item will automatically
    /// become buildable. To avoid this, or make this item a buildable manually, use the <see cref="SetBuildable"/> method.</remarks>
    public ScanningGadget WithPdaGroupCategory(TechGroup group, TechCategory category)
    {
        GroupForPda = group;
        CategoryForPda = category;
        PdaSortPosition = SortPosition.AppendAfter;
        PdaSortTarget = TechType.None;
        if (uGUI_BuilderMenu.groups.Contains(group))
        {
            SetBuildable();
        }
        return this;
    }

    /// <summary>
    /// Adds this item into a blueprint category to appear in.
    /// </summary>
    /// <param name="group">The main group in the PDA blueprints where this item appears.</param>
    /// <param name="category">The category within the group in the PDA blueprints where this item appears.</param>
    /// <param name="target">It will be added after this target item or at the end if not found.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <remarks>If the specified <paramref name="group"/> is a tech group that is present in the <see cref="uGUI_BuilderMenu.groups"/> list, this item will automatically
    /// become buildable. To avoid this, or make this item a buildable manually, use the <see cref="SetBuildable"/> method.</remarks>
    public ScanningGadget WithPdaGroupCategoryAfter(TechGroup group, TechCategory category, TechType target = TechType.None)
    {
        GroupForPda = group;
        CategoryForPda = category;
        PdaSortPosition = SortPosition.AppendAfter;
        PdaSortTarget = target;
        if (uGUI_BuilderMenu.groups.Contains(group))
        {
            SetBuildable();
        }
        return this;
    }

    /// <summary>
    /// Adds this item into a blueprint category to appear in.
    /// </summary>
    /// <param name="group">The main group in the PDA blueprints where this item appears.</param>
    /// <param name="category">The category within the group in the PDA blueprints where this item appears.</param>
    /// <param name="target">It will be inserted before this target item or at the beginning if not found.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <remarks>If the specified <paramref name="group"/> is a tech group that is present in the <see cref="uGUI_BuilderMenu.groups"/> list, this item will automatically
    /// become buildable. To avoid this, or make this item a buildable manually, use the <see cref="SetBuildable"/> method.</remarks>
    public ScanningGadget WithPdaGroupCategoryBefore(TechGroup group, TechCategory category, TechType target = TechType.None)
    {
        GroupForPda = group;
        CategoryForPda = category;
        PdaSortPosition = SortPosition.InsertBefore;
        PdaSortTarget = target;
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
        IsBuildable = isBuildable;
        return this;
    }

    /// <summary>
    /// Makes this item hard locked. Hard locked items are not unlocked by default even in creative and can't be unlocked using the `unlockall` command.
    /// </summary>
    /// <param name="isHardLocked">Should this item be hard locked?</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public ScanningGadget SetHardLocked(bool isHardLocked = true)
    {
        IsHardLocked = isHardLocked;

        return this;
    }

    /// <summary>
    /// <para>Adds an encyclopedia entry for this item in the PDA. This method does not ask for display text, for that you must use the <see cref="LanguageHandler"/>.</para>
    /// <para>The encyclopedia entry's key will be set as the TechType string.</para>
    /// <para>The language keys for this ency are as as follows: "Ency_{TechType}" (title) and "EncyDesc_{TechType}" (description), i.e. "Ency_Peeper".</para>
    /// </summary>
    /// <param name="path">The path this entry will appear in.</param>
    /// <param name="popupSprite">The sprite that will pop up on the side of the screen once this entry is unlocked.</param>
    /// <param name="encyImage">The entry image that will appear in the encyclopedia entry</param>
    /// <param name="unlockSound">The audio that is played when this sound is unlocked. Typical values are <see cref="PDAHandler.UnlockBasic"/> and <see cref="PDAHandler.UnlockBasic"/>. If unassigned, will have a default value of <see cref="PDAHandler.UnlockBasic"/>.</param>
    /// <param name="encyAudio">The audio that can be played in the entry.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public ScanningGadget WithEncyclopediaEntry(string path, Sprite popupSprite, Texture2D encyImage = null, FMODAsset unlockSound = null, FMODAsset encyAudio = null)
    {
        EncyclopediaEntryData = new PDAEncyclopedia.EntryData
        {
            key = prefab.Info.TechType.AsString(),
            path = path,
            nodes = path.Split('/'),
            popup = popupSprite,
            image = encyImage,
            sound = unlockSound ?? PDAHandler.UnlockBasic,
            audio = encyAudio
        };

        return this;
    }
    
    /// <summary>
    /// Adds additional info on how the Scanner tool should treat this item when scanning it.
    /// </summary>
    /// <param name="blueprint">The blueprint that gets unlocked once this item is scanned.</param>
    /// <param name="scanTime">The amount of seconds it takes to scan this item.</param>
    /// <param name="isFragment">Is this a fragment?</param>
    /// <param name="encyKey">The encyclopedia key to unlock once the scanning is completed.</param>
    /// <param name="destroyAfterScan">Should this object be destroyed after a successful scan?</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public ScanningGadget WithScannerEntry(TechType blueprint, float scanTime, bool isFragment = false, string encyKey = null, bool destroyAfterScan = false)
    {
        ScannerEntryData = new PDAScanner.EntryData
        {
            key = prefab.Info.TechType,
            encyclopedia = encyKey,
            scanTime = scanTime,
            destroyAfterScan = destroyAfterScan,
            isFragment = isFragment,
            blueprint = blueprint,
            totalFragments = FragmentsToScan,
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
    /// <remarks>This overload overrides the PDAScanner entry data for the <see cref="RequiredForUnlock"/>'s entry.</remarks>
    [Obsolete("Deprecated; Use WithScannerEntry(TechType, float, bool, string, bool) overload instead.")]
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
    /// <param name="unlockSound">The sound that will be played on unlock.</param>
    /// <param name="unlockMessage">Message which should be shown on unlock.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public ScanningGadget WithAnalysisTech(
        Sprite popupSprite, 
        FMODAsset unlockSound = null, 
        string unlockMessage = null
    )
    {
        return WithAnalysisTech(new KnownTech.AnalysisTech
        {
            unlockPopup = popupSprite,
            unlockSound = unlockSound,
            unlockMessage = unlockMessage
        });
    }

#if SUBNAUTICA
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
        List<StoryGoal> storyGoalsToTrigger,
        FMODAsset unlockSound = null, 
        string unlockMessage = null
        )
    {
        return WithAnalysisTech(new KnownTech.AnalysisTech
        {
            unlockPopup = popupSprite,
            storyGoals = storyGoalsToTrigger,
            unlockSound = unlockSound,
            unlockMessage = unlockMessage
        });
    }
#endif

    private ScanningGadget WithAnalysisTech(KnownTech.AnalysisTech analysisTech)
    {
        AnalysisTech = analysisTech;
        AnalysisTech.techType = prefab.Info.TechType;
        AnalysisTech.unlockTechTypes = RequiredForUnlock != TechType.None
            ? new() { prefab.Info.TechType }
            : new();
        AnalysisTech.unlockPopup = analysisTech.unlockPopup;
#if SUBNAUTICA
        AnalysisTech.storyGoals = analysisTech.storyGoals ?? new();
#endif
        AnalysisTech.unlockSound = analysisTech.unlockSound;
        AnalysisTech.unlockMessage = analysisTech.unlockMessage ?? KnownTechHandler.DefaultUnlockData.BlueprintUnlockMessage;

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
                CraftDataHandler.AddToGroup(GroupForPda, CategoryForPda, prefab.Info.TechType, PdaSortTarget, PdaSortPosition == SortPosition.AppendAfter);
            }
            else
            {
                InternalLogger.Error($"Failed to add {prefab.Info.TechType.AsString()} to {GroupForPda}/{CategoryForPda} as it is not a registered combination.");
            }
            
            if (IsBuildable)
                CraftDataHandler.AddBuildable(prefab.Info.TechType);
        }

        if (EncyclopediaEntryData is { })
        {
            PDAHandler.AddEncyclopediaEntry(EncyclopediaEntryData);
        }

        if (CompoundTechsForUnlock is { Count: > 0 })
        {
            KnownTechHandler.RemoveAllCurrentAnalysisTechEntry(prefab.Info.TechType);
            KnownTechHandler.SetCompoundUnlock(prefab.Info.TechType, CompoundTechsForUnlock);
        }

        if (AnalysisTech is { })
        {
            KnownTechHandler.SetAnalysisTechEntry(AnalysisTech);
        }

        if (ScannerEntryData is { })
        {
            PDAHandler.AddCustomScannerEntry(ScannerEntryData);
        }

        if (RequiredForUnlock != TechType.None)
        {
            KnownTechHandler.SetAnalysisTechEntry(RequiredForUnlock, new TechType[] { prefab.Info.TechType });
        }

        if (CompoundTechsForUnlock is { Count: > 0 } || RequiredForUnlock != TechType.None)
        {
            KnownTechPatcher.UnlockedAtStart.Remove(prefab.Info.TechType);
        }

        if (!KnownTechPatcher.UnlockedAtStart.Contains(prefab.Info.TechType) && 
            (CompoundTechsForUnlock is null || CompoundTechsForUnlock.Count <= 0) && RequiredForUnlock == TechType.None && ScannerEntryData is null)
        {
            KnownTechPatcher.LockedWithNoUnlocks.Add(prefab.Info.TechType);
        }

        if (IsHardLocked)
        {
            KnownTechHandler.SetHardLocked(prefab.Info.TechType);
        }
    }
}