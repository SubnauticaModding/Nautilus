namespace SMLHelper.Examples;

using System.Collections;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using SMLHelper.API;
using SMLHelper.Assets;
using SMLHelper.Assets.Interfaces;
using SMLHelper.Assets.PrefabTemplates;
using SMLHelper.Crafting;
using SMLHelper.DependencyInjection;
using SMLHelper.Handlers;
using UnityEngine;
using static CraftData;

public class NuclearBattery: IModPrefab, ICraftable, ICustomBattery
{
    public PrefabInfo PrefabInfo { get; }

    public RecipeData RecipeData { get; } = new RecipeData()
    {
        craftAmount = 1,
        Ingredients = new() { new Ingredient(TechType.ReactorRod), new Ingredient(TechType.Lead, 2) },
        LinkedItems = new() { TechType.DepletedReactorRod }
    };

    public CraftTree.Type FabricatorType { get; } = CraftTree.Type.Fabricator;

    public string[] StepsToFabricatorTab { get; } = CustomBatteryHandler.BatteryCraftPath;

    public float CraftingTime { get; } = 1;

    public BatteryType BatteryType { get; } = BatteryType.Battery;

    public NuclearBattery()
    {
        PrefabInfo = PrefabInfo.Create("NuclearBattery", GetGameObjectAsync)
            .CreateTechType().WithPdaInfo("Nuclear Battery", "Nuclear Battery that makes me go yes")
        .WithIcon(SpriteManager.Get(TechType.PrecursorIonBattery));
    }

    [InjectionSetup]
    private void Setup(ManualLogSource logger)
    {
        logger.LogDebug($"{nameof(NuclearBattery)} Patched.");
    }

    public IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
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