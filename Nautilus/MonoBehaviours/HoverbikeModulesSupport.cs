#if BELOWZERO
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Handlers;
using Nautilus.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nautilus.MonoBehaviours;

/// <summary>
/// A custom monobehaviour made to support more modules types on hoverbike.
/// </summary>
public class HoverbikeModulesSupport : MonoBehaviour, IQuickSlots
{
    public event QuickSlots.OnBind onBind;
    public event QuickSlots.OnToggle onToggle;
    public event QuickSlots.OnSelect onSelect;
    public Hoverbike hoverbike;

    public static IDictionary<TechType, ICustomPrefab> CustomModules = new SelfCheckingDictionary<TechType, ICustomPrefab>("CustomModules");

    public float[] quickSlotCooldown;
    public float[] quickSlotTimeUsed;
    public float[] quickSlotCharge;
    public bool[]  quickSlotToggled;
    public int activeSlot;
    public Dictionary<string, int> slotIndices;
    public bool ignoreInput { get; private set; }
    private bool isInitialized = false;

    private void LazyInitialize()
    {
        if (isInitialized)
            return;
        isInitialized = true;
        slotIndices = new Dictionary<string, int>();
        int i = 0;
        foreach(string text in hoverbike.slotIDs)
        {
            slotIndices.Add(hoverbike.slotIDs[i], i);
            i++;
        }
        quickSlotCooldown = new float[hoverbike.slotIDs.Length];
        quickSlotTimeUsed = new float[hoverbike.slotIDs.Length];
        quickSlotCharge = new float[hoverbike.slotIDs.Length];
        quickSlotToggled = new bool[hoverbike.slotIDs.Length];
        hoverbike.modules.onEquip += OnEquip;
        hoverbike.modules.onUnequip += OnUnequip;
        onToggle += OnToggle;
    }

    private void Awake()
    {
        LazyInitialize();
    }

    public void ConsumeEnergy(float energy)
    {
        hoverbike.energyMixin.ConsumeEnergy(energy);
    }

    public bool ConsumeEnergy(TechType techType)
    {
        if (!TechData.GetEnergyCost(techType, out float energyCost))
            return false;
        ConsumeEnergy(energyCost);
        return true;
    }

    public bool QuickSlotHasCooldown(int slotID)
    {
        return quickSlotTimeUsed[slotID] + quickSlotCooldown[slotID] > Time.time;
    }

    protected void NotifySelectSlot(int slotID)
    {
        if (onSelect != null)
        {
            onSelect(slotID);
        }
    }

    public bool CanUseUpgrade(TechType techType)
    {
        bool canUseModule = TechData.GetSlotType(techType) == QuickSlotType.Passive || TechData.GetSlotType(techType) == QuickSlotType.None || !hoverbike.kinematicOverride;
        return canUseModule;
    }

    public void OnUpgradeModuleUse(TechType techType, int slotId)
    {
        quickSlotTimeUsed[slotId] = Time.time;
        if (!CustomModules.TryGetValue(techType, out ICustomPrefab modulePrefab))
            return;

        if (!modulePrefab.TryGetGadget(out UpgradeModuleGadget moduleGadget))
            return;

        var charge = quickSlotCharge[slotId];
        var chargeScalar = GetSlotCharge(slotId);
        moduleGadget.hoverbikeOnUsed?.Invoke(hoverbike, slotId, charge, chargeScalar);
        if(moduleGadget.Cooldown > 0f)
            quickSlotCooldown[slotId] = (float)moduleGadget.Cooldown;
    }

    private void OnToggle(int slotID, bool state)
    {
        TechType techType = GetSlotBinding(slotID);
        if (!CustomModules.TryGetValue(techType, out ICustomPrefab prefab))
            return;

        if (!prefab.TryGetGadget(out UpgradeModuleGadget moduleGadget))
            return;
        float energyCost;
        if (!TechData.GetEnergyCost(techType, out energyCost))
            energyCost = (float)moduleGadget.EnergyCost;
        moduleGadget.hoverbikeOnToggled.Invoke(hoverbike, slotID, energyCost, state);
    }

    private void OnEquip(string slot, InventoryItem item)
    {
        Pickupable item2 = item.item;
        TechType techType = (item2 != null) ? item2.GetTechType() : TechType.None;
        UpgradeModuleChanged(slot, techType, true);
    }

    private void OnUnequip(string slot, InventoryItem item)
    {
        Pickupable item2 = item.item;
        TechType techType = (item2 != null) ? item2.GetTechType() : TechType.None;
        UpgradeModuleChanged(slot, techType, false);
    }

    public void UpgradeModuleChanged(string slot, TechType techType, bool added)
    {
        int slotID;
        if (!slotIndices.TryGetValue(slot, out slotID))
            slotID = -1;

        OnUpgradeModuleChange(slotID, techType, added);
    }

    public void OnUpgradeModuleChange(int slotID, TechType techType, bool added)
    {
        if (!CustomModules.TryGetValue(techType, out ICustomPrefab prefab))
            return;

        if (!prefab.TryGetGadget(out UpgradeModuleGadget moduleGadget))
            return;

        if (moduleGadget.hoverbikeOnRemoved != null && !added)
            moduleGadget.hoverbikeOnRemoved.Invoke(hoverbike, slotID);

        if (moduleGadget.hoverbikeOnAdded != null && added)
            moduleGadget.hoverbikeOnAdded.Invoke(hoverbike, slotID);
    }

    public void Bind(int slotID, InventoryItem item)
    {
    }

    public void Unbind(int slotID)
    {
    }

    public void DeselectSlots()
    {
        activeSlot = -1;
        NotifySelectSlot(activeSlot);
    }

    public int GetActiveSlotID()
    {
        return activeSlot;
    }

    public TechType[] GetSlotBinding()
    {
        int num = hoverbike.slotIDs.Length;
        TechType[] array = new TechType[num];
        for (int i = 0; i < num; i++)
        {
            array[i] = hoverbike.modules.GetTechTypeInSlot(hoverbike.slotIDs[i]);
        }
        return array;
    }

    public TechType GetSlotBinding(int slotID)
    {
        if (slotID < 0 || slotID >= hoverbike.slotIDs.Length)
        {
            return TechType.None;
        }
        string slot = hoverbike.slotIDs[slotID];
        return hoverbike.modules.GetTechTypeInSlot(slot);
    }

    public int GetSlotByItem(InventoryItem item)
    {
        if (item != null)
        {
            int i = 0;
            while (i < hoverbike.slotIDs.Length)
            {
                if (hoverbike.modules.GetItemInSlot(hoverbike.slotIDs[i]) == item)
                {
                    return i;
                }
                i++;
            }
        }
        return -1;
    }

    public float GetSlotProgress(int slotID)
    {
        return GetQuickSlotCooldown(slotID);
    }

    public float GetSlotCharge(int slotID)
    {
        if (slotID < 0 || slotID >= hoverbike.slotIDs.Length)
        {
            return 1f;
        }
        TechType techType;
        QuickSlotType quickSlotType = GetQuickSlotType(slotID, out techType);
        if (quickSlotType == QuickSlotType.Chargeable || quickSlotType == QuickSlotType.SelectableChargeable)
        {
            float quickSlotMaxCharge = TechData.GetMaxCharge(techType);
            if (quickSlotMaxCharge > 0f)
            {
                return quickSlotCharge[slotID] / quickSlotMaxCharge;
            }
        }
        return 1f;
    }

    public int GetSlotCount()
    {
        return hoverbike.slotIDs.Length;
    }

    public InventoryItem GetSlotItem(int slotID)
    {
        if (slotID < 0 || slotID >= hoverbike.slotIDs.Length)
        {
            return null;
        }
        string slot = hoverbike.slotIDs[slotID];
        return hoverbike.modules.GetItemInSlot(slot);
    }

    public bool IsToggled(int slotID)
    {
        return slotID >= 0 && slotID < hoverbike.slotIDs.Length && (GetQuickSlotType(slotID) == QuickSlotType.Passive || quickSlotToggled[slotID]);
    }

    public void SlotKeyDown(int slotID)
    {
        if (slotID < 0 || slotID >= hoverbike.slotIDs.Length)
        {
            return;
        }
        TechType techTypeInSlot = hoverbike.modules.GetTechTypeInSlot(hoverbike.slotIDs[slotID]);
        QuickSlotType slotType = TechData.GetSlotType(techTypeInSlot);
        if (slotType == QuickSlotType.Selectable || slotType == QuickSlotType.SelectableChargeable)
        {
            if (activeSlot >= 0 && activeSlot < hoverbike.slotIDs.Length)
            {
                quickSlotCharge[activeSlot] = 0f;
            }
            activeSlot = slotID;
        }
        if (!QuickSlotHasCooldown(slotID) && slotType == QuickSlotType.Instant && CanUseUpgrade(techTypeInSlot) && ConsumeEnergy(techTypeInSlot))
        {
            OnUpgradeModuleUse(techTypeInSlot, slotID);
        }
        NotifySelectSlot(activeSlot);
    }

    protected float GetQuickSlotCooldown(int slotID)
    {
        float cooldown = quickSlotCooldown[slotID];
        if (cooldown <= 0f)
        {
            return 1f;
        }
        return Mathf.Clamp01((Time.time - quickSlotTimeUsed[slotID]) / cooldown);
    }

    public QuickSlotType GetQuickSlotType(int slotID, out TechType techType)
    {
        if (slotID >= 0 && slotID < hoverbike.slotIDs.Length)
        {
            techType = hoverbike.modules.GetTechTypeInSlot(hoverbike.slotIDs[slotID]);
            if (techType != TechType.None)
            {
                return TechData.GetSlotType(techType);
            }
        }
        techType = TechType.None;
        return QuickSlotType.None;
    }

    public QuickSlotType GetQuickSlotType(int slotID)
    {
        if (slotID >= 0 && slotID < hoverbike.slotIDs.Length)
        {
            TechType techType = hoverbike.modules.GetTechTypeInSlot(hoverbike.slotIDs[slotID]);
            if (techType != TechType.None)
            {
                return TechData.GetSlotType(techType);
            }
        }
        return QuickSlotType.None;
    }

    public QuickSlotType GetQuickSlotType(TechType techType)
    {
        return TechData.GetSlotType(techType);
    }

    public void ChargeModule(TechType techType, int slotID)
    {
        float slotCharge = quickSlotCharge[slotID];
        float maxCharge = TechData.GetMaxCharge(techType);
        float energyCost;
        TechData.GetEnergyCost(techType, out energyCost);
        float energyCostOnDeltaTime = energyCost * Time.deltaTime;
        float remainingCharge = maxCharge - slotCharge;
        bool energyCostGtEqRemainCharge = energyCostOnDeltaTime >= remainingCharge;
        float finalEnergyCost = energyCostGtEqRemainCharge ? Mathf.Max(0f, remainingCharge) : energyCostOnDeltaTime;
        float balance = Mathf.Min(hoverbike.energyMixin.energy, finalEnergyCost);
        ConsumeEnergy(balance);
        quickSlotCharge[slotID] = quickSlotCharge[slotID] + balance;
        if (quickSlotCharge[slotID] > 0f && (energyCostGtEqRemainCharge || balance == 0f))
        {
            OnUpgradeModuleUse(techType, slotID);
            quickSlotCharge[slotID] = 0f;
        }
    }

    public void SlotKeyHeld(int slotID)
    {
        if (ignoreInput)
        {
            return;
        }
        if (slotID < 0 || slotID >= hoverbike.slotIDs.Length)
        {
            return;
        }
        if (GetQuickSlotCooldown(slotID) != 1f)
        {
            return;
        }
        TechType techType;
        if (GetQuickSlotType(slotID, out techType) == QuickSlotType.Chargeable)
        {
            ChargeModule(techType, slotID);
        }
    }

    public void SlotKeyUp(int slotID)
    {
        if (this.ignoreInput)
        {
            return;
        }
        if (slotID < 0 || slotID >= hoverbike.slotIDs.Length)
        {
            return;
        }
        if (GetQuickSlotCooldown(slotID) != 1f)
        {
            return;
        }
        if (GetQuickSlotType(slotID, out TechType techType) == QuickSlotType.Chargeable)
        {
            OnUpgradeModuleUse(techType, slotID);
            quickSlotCharge[slotID] = 0f;
        }
    }

    public void SlotLeftDown()
    {
        if (ignoreInput)
        {
            return;
        }
        if (activeSlot < 0)
        {
            return;
        }
        if (GetQuickSlotCooldown(activeSlot) != 1f)
        {
            return;
        }
        if (GetQuickSlotType(activeSlot, out TechType techType) == QuickSlotType.Selectable && ConsumeEnergy(techType))
        {
            OnUpgradeModuleUse(techType, activeSlot);
        }
    }

    public void SlotLeftHeld()
    {
        if (ignoreInput)
        {
            return;
        }
        if (activeSlot < 0)
        {
            return;
        }
        if (GetQuickSlotCooldown(activeSlot) != 1f)
        {
            return;
        }
        if (GetQuickSlotType(activeSlot, out TechType techType) == QuickSlotType.SelectableChargeable)
        {
            ChargeModule(techType, activeSlot);
        }
    }

    public void SlotLeftUp()
    {
        if (ignoreInput)
        {
            return;
        }
        if (activeSlot < 0)
        {
            return;
        }
        if (GetQuickSlotCooldown(activeSlot) != 1f)
        {
            return;
        }
        TechType techType;
        if (GetQuickSlotType(activeSlot, out techType) == QuickSlotType.SelectableChargeable && quickSlotCharge[activeSlot] > 0f)
        {
            OnUpgradeModuleUse(techType, activeSlot);
            quickSlotCharge[activeSlot] = 0f;
        }
    }

    public void SlotNext()
    {
        if (ignoreInput)
        {
            return;
        }
        int activeSlotID = GetActiveSlotID();
        int slotCount = GetSlotCount();
        int slotID = (activeSlotID < 0) ? -1 : activeSlotID;
        for (int i = 0; i < slotCount; i++)
        {
            slotID++;
            if (slotID >= slotCount)
            {
                slotID = 0;
            }
            TechType slotBinding = GetSlotBinding(slotID);
            if (slotBinding != TechType.None)
            {
                QuickSlotType quickSlotType = GetQuickSlotType(slotBinding);
                if (quickSlotType == QuickSlotType.Selectable || quickSlotType == QuickSlotType.SelectableChargeable)
                {
                    SlotKeyDown(slotID);
                    return;
                }
            }
        }
    }

    public void SlotPrevious()
    {
        if (ignoreInput)
        {
            return;
        }
        int activeSlotID = GetActiveSlotID();
        int slotCount = GetSlotCount();
        int slotID = (activeSlotID < 0) ? slotCount : activeSlotID;
        for (int i = 0; i < slotCount; i++)
        {
            slotID--;
            if (slotID < 0)
            {
                slotID = slotCount - 1;
            }
            TechType slotBinding = GetSlotBinding(slotID);
            if (slotBinding != TechType.None)
            {
                QuickSlotType quickSlotType = GetQuickSlotType(slotBinding);
                if (quickSlotType == QuickSlotType.Selectable || quickSlotType == QuickSlotType.SelectableChargeable)
                {
                    SlotKeyDown(slotID);
                    return;
                }
            }
        }
    }

    public void SlotRightDown()
    {
    }

    public void SlotRightHeld()
    {
    }

    public void SlotRightUp()
    {
    }
}
#endif