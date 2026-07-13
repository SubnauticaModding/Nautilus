using System;
using System.Collections.Generic;
using Nautilus.Patchers;

namespace Nautilus.Handlers;

/// <summary>
/// Handler class that relates to the <see cref="Survival"/> component. Allows the defining of oxygen or health gains when consuming specific items.
/// </summary>
public static class SurvivalHandler
{
    [Obsolete]
    private static void GiveOxygenOnConsume(TechType techType, float oxygenGiven, bool isEdible) => GiveOxygenOnConsume(techType, oxygenGiven);
    
    /// <summary>
    /// <para>makes the item gives oxygen on use.</para>
    /// </summary>
    /// <param name="techType">the TechType that you want to make it give oxygen on use</param>
    /// <param name="oxygenGiven">the oxygen amount the item gives</param>
    public static void GiveOxygenOnConsume(TechType techType, float oxygenGiven)
    {
        if (!SurvivalPatcher.CustomSurvivalInventoryAction.TryGetValue(techType, out List<Action> actions))
        {
            actions = new List<Action>();
            SurvivalPatcher.CustomSurvivalInventoryAction[techType] = actions;
        }

        // add an action to the list
        actions.Add(() => 
        {
            var oxygenManager = Player.main.GetComponent<OxygenManager>();
            if (oxygenGiven > 0f)
                oxygenManager.AddOxygen(oxygenGiven);
            else
                oxygenManager.RemoveOxygen(-oxygenGiven);
        });
    }

    [Obsolete]
    private static void GiveHealthOnConsume(TechType techType, float healthBack, bool isEdible) => GiveHealthOnConsume(techType, healthBack);
    
    /// <summary>
    /// <para>makes the item Heal the player on consume.</para>
    /// </summary>
    /// <param name="techType">the TechType that you want it to heal back</param>
    /// <param name="healthBack">amount to heal the player</param>
    public static void GiveHealthOnConsume(TechType techType, float healthBack)
    {
        if (!SurvivalPatcher.CustomSurvivalInventoryAction.TryGetValue(techType, out List<Action> actions))
        {
            actions = new List<Action>();
            SurvivalPatcher.CustomSurvivalInventoryAction[techType] = actions;
        }
        
        actions.Add(() => {
            var liveMixin = Player.main.GetComponent<LiveMixin>();
            if (healthBack > 0)
                liveMixin.AddHealth(healthBack); 
            else
                liveMixin.TakeDamage(-healthBack, default, DamageType.Poison);
        }); 
    }

    [Obsolete]
    private static void RunActionOnConsume(TechType techType, Action customAction, bool isEdible) => RunActionOnConsume(techType, customAction);
    
    /// <summary>
    /// <para>runs a custom action on consume.</para>
    /// </summary>
    /// <param name="techType">the TechType that you want it to heal back</param>
    /// <param name="customAction"> the Action to perform.</param>
    public static void RunActionOnConsume(TechType techType, Action customAction)
    {
        if (techType == TechType.None)
            throw new ArgumentNullException(nameof(techType), "TechType cannot be None.");
        if (customAction == null)
            throw new ArgumentNullException(nameof(customAction), "Action cannot be null.");

        if (!SurvivalPatcher.CustomSurvivalInventoryAction.TryGetValue(techType, out List<Action> actions))
        {
            actions = new List<Action>();
            SurvivalPatcher.CustomSurvivalInventoryAction[techType] = actions;
        }

        actions.Add(customAction);
    }
}