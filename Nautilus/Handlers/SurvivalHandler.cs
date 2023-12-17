using System;
using System.Collections.Generic;
using Nautilus.Patchers;

namespace Nautilus.Handlers;

/// <summary>
/// Handler class that relates to the <see cref="Survival"/> component. Allows the defining of oxygen or health gains when consuming specific items.
/// </summary>
public static class SurvivalHandler
{
    /// <summary>
    /// <para>makes the item gives oxygen on use.</para>
    /// </summary>
    /// <param name="techType">the TechType that you want to make it give oxygen on use</param>
    /// <param name="oxygenGiven">the oxygen amount the item gives</param>
    /// <param name="isEdible">set it to <see langword="true" /> if the item is edible and has the <see cref="Eatable"/> component attached to it. 
    /// </param>
    public static void GiveOxygenOnConsume(TechType techType, float oxygenGiven, bool isEdible)
    {
        if (!isEdible)
        {
            SurvivalPatcher.InventoryUseables.Add(techType); // add it to the HashSet of useables if its not edible
        }

        if (!SurvivalPatcher.CustomSurvivalInventoryAction.TryGetValue(techType, out List<Action> actions))
            actions = new List<Action>();

        // add an action to the list
        actions.Add(() => 
        {
            var oxygenManager = Player.main.GetComponent<OxygenManager>();
            if (oxygenGiven > 0f)
                oxygenManager.AddOxygen(oxygenGiven);
            else
                oxygenManager.RemoveOxygen(-oxygenGiven);
        });

        SurvivalPatcher.CustomSurvivalInventoryAction[techType] = actions;
    }

    /// <summary>
    /// <para>makes the item Heal the player on consume.</para>
    /// </summary>
    /// <param name="techType">the TechType that you want it to heal back</param>
    /// <param name="healthBack">amount to heal the player</param>
    /// <param name="isEdible">set it to <see langword="true" /> if the item is edible and has the <see cref="Eatable"/> component attached to it. 
    /// </param>
    public static void GiveHealthOnConsume(TechType techType, float healthBack, bool isEdible)
    {
        if (!isEdible)
        {
            SurvivalPatcher.InventoryUseables.Add(techType); // add it to the HashSet of useables if its not edible
        }

        if (!SurvivalPatcher.CustomSurvivalInventoryAction.TryGetValue(techType, out List<Action> actions))
            actions = new List<Action>();

        actions.Add(() => {
            var liveMixin = Player.main.GetComponent<LiveMixin>();
            if (healthBack > 0)
                liveMixin.AddHealth(healthBack); 
            else
                liveMixin.TakeDamage(-healthBack, default, DamageType.Poison);
        }); 

        SurvivalPatcher.CustomSurvivalInventoryAction[techType] = actions;
    }

    /// <summary>
    /// <para>runs a custom action on consume.</para>
    /// </summary>
    /// <param name="techType">the TechType that you want it to heal back</param>
    /// <param name="customAction"> the Action to perform.</param>
    /// <param name="isEdible">set it to <see langword="true" /> if the item is edible and has the <see cref="Eatable"/> component attached to it. 
    /// </param>
    public static void RunActionOnConsume(TechType techType, Action customAction, bool isEdible)
    {
        if (techType == TechType.None)
            throw new ArgumentNullException(nameof(techType), "TechType cannot be None.");
        if (customAction == null)
            throw new ArgumentNullException(nameof(customAction), "Action cannot be null.");

        if (!isEdible)
        {
            SurvivalPatcher.InventoryUseables.Add(techType); // add it to the HashSet of useables if its not edible
        }

        if (!SurvivalPatcher.CustomSurvivalInventoryAction.TryGetValue(techType, out List<Action> actions))
            actions = new List<Action>();

        actions.Add(customAction);
        SurvivalPatcher.CustomSurvivalInventoryAction[techType] = actions;
    }
}