namespace SMLHelper.Examples;

using System.Collections;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using SMLHelper.API;
using SMLHelper.Assets;
using SMLHelper.Assets.PrefabTemplates;
using SMLHelper.DependencyInjection;
using SMLHelper.Handlers;
using UnityEngine;

public class NuclearBattery : CustomPrefab
{
    public override PrefabInfo PrefabInfo { get; protected set; } = PrefabInfo.WithTechType("NuclearBattery", "Nuclear Battery", "Nuclear Battery that makes me go yes")
        .WithIcon(SpriteManager.Get(TechType.PrecursorIonBattery));

    [InjectionSetup]
    private void Setup(ManualLogSource logger)
    {
        if (Chainloader.PluginInfos.ContainsKey("DecorationsMod"))
        {
            logger.LogDebug("Found Decorations mod. Adding compatibility patch");
            CraftDataHandler.SetEquipmentType(PrefabInfo.TechType, EquipmentType.Hand);
            CraftDataHandler.SetQuickSlotType(PrefabInfo.TechType, QuickSlotType.Selectable);
        }
        
        logger.LogDebug($"{nameof(NuclearBattery)} Patched.");
    }

    public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
    {
        var battery = new EnergySourceTemplate(69420)
        {
            ModelData = new CBModelData
            {
                UseIonModelsAsBase = true
            }
        };
            
        var task = new TaskResult<GameObject>();
        yield return battery.GetPrefabAsync(task);
        
        gameObject.Set(task.Get());
    }
}