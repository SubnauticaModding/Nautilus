using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Nautilus.MonoBehaviours;
internal class HoverbikeComplement : MonoBehaviour, IQuickSlots
{
    public event QuickSlots.OnBind onBind;
    public event QuickSlots.OnToggle onToggle;
    public event QuickSlots.OnSelect onSelect;
    public Hoverbike hoverbike;

    public float[] quickSlotCooldown;
    public float[] quickSlotTimeUsed;
    public float[] quickSlotCharge;
    public int activeSlot;

    public void ConsumeEnergy(float energy)
    {
        hoverbike.energyMixin.ConsumeEnergy(energy);
    }

    public void ConsumeEnergy(TechType techType)
    {
        float energyCost = 0f;
        if (TechData.GetEnergyCost(techType, out energyCost))
            return;
        ConsumeEnergy(energyCost);
    }

    public bool QuickSlotHasCooldown(int slotID)
    {
        return this.quickSlotTimeUsed[slotID] + this.quickSlotCooldown[slotID] > Time.time;
    }

    protected void NotifySelectSlot(int slotID)
    {
        if (this.onSelect != null)
        {
            this.onSelect(slotID);
        }
    }

    public bool CanUseUpgrade(TechType techType)
    {
        bool canUseModule = TechData.GetSlotType(techType) == QuickSlotType.Passive || TechData.GetSlotType(techType) == QuickSlotType.None || !hoverbike.kinematicOverride;
        return canUseModule;
    }

    public void OnUpgradeModuleUse(TechType techType, int slotId)
    {

    }

    public void Bind(int slotID, InventoryItem item)
    {
        throw new NotImplementedException();
    }

    public void DeselectSlots()
    {
        throw new NotImplementedException();
    }

    public int GetActiveSlotID()
    {
        throw new NotImplementedException();
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
            int num = hoverbike.slotIDs.Length;
            while (i < num)
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
        return 1f;
    }

    public float GetSlotCharge(int slotID)
    {
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
        return false;
    }

    public void SlotKeyDown(int slotID)
    {
        if (slotID < 0 || slotID >= SeaTruckUpgrades.slotIDs.Length)
        {
            return;
        }
        TechType techTypeInSlot = hoverbike.modules.GetTechTypeInSlot(hoverbike.slotIDs[slotID]);
        QuickSlotType slotType = TechData.GetSlotType(techTypeInSlot);
        if (slotType == QuickSlotType.Selectable || slotType == QuickSlotType.SelectableChargeable)
        {
            if (this.activeSlot >= 0 && this.activeSlot < SeaTruckUpgrades.slotIDs.Length)
            {
                this.quickSlotCharge[this.activeSlot] = 0f;
            }
            this.activeSlot = slotID;
        }
        if (!this.QuickSlotHasCooldown(slotID) && slotType == QuickSlotType.Instant && this.CanUseUpgrade(techTypeInSlot) && this.ConsumeEnergy(techTypeInSlot))
        {
            this.OnUpgradeModuleUse(techTypeInSlot, slotID);
        }
        this.NotifySelectSlot(this.activeSlot);
    }

    public void SlotKeyHeld(int slotID)
    {
        throw new NotImplementedException();
    }

    public void SlotKeyUp(int slotID)
    {
        throw new NotImplementedException();
    }

    public void SlotLeftDown()
    {
        throw new NotImplementedException();
    }

    public void SlotLeftHeld()
    {
        throw new NotImplementedException();
    }

    public void SlotLeftUp()
    {
        throw new NotImplementedException();
    }

    public void SlotNext()
    {
        throw new NotImplementedException();
    }

    public void SlotPrevious()
    {
        throw new NotImplementedException();
    }

    public void SlotRightDown()
    {
        throw new NotImplementedException();
    }

    public void SlotRightHeld()
    {
        throw new NotImplementedException();
    }

    public void SlotRightUp()
    {
        throw new NotImplementedException();
    }

    public void Unbind(int slotID)
    {
        throw new NotImplementedException();
    }
}
