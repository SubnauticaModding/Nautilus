namespace SMLHelper.Handlers
{
    using Assets;
    using SMLHelper.Assets.Interfaces;
    using SMLHelper.Crafting;
    using SMLHelper.Utility;

    /// <summary>
    /// A handler for registering Unity prefabs associated to a <see cref="TechType"/>.
    /// </summary>
    public static class PrefabHandler
    {

        /// <summary>
        /// Registers a ModPrefab into the game.
        /// </summary>
        public static void RegisterPrefab(this IModPrefab prefab)
        {
            foreach (PrefabInfo registeredInfo in ModPrefabCache.Prefabs)
            {
                var techtype = registeredInfo.TechType == prefab.PrefabInfo.TechType && registeredInfo.TechType != TechType.None;
                var classid = registeredInfo.ClassID == prefab.PrefabInfo.ClassID;
                var filename = registeredInfo.PrefabFileName == prefab.PrefabInfo.PrefabFileName;
                if(techtype || classid || filename)
                {
                    InternalLogger.Error($"Another ModPrefab is already registered with these values. {(techtype ? "TechType: " + registeredInfo.TechType : "")} {(classid ? "ClassId: " + registeredInfo.ClassID : "")} {(filename ? "PrefabFileName: " + registeredInfo.PrefabFileName : "")}");
                    return;
                }
            }

            HandleInterfaces(prefab);
            ModPrefabCache.Add(prefab.PrefabInfo);
        }

        private static void HandleInterfaces(IModPrefab customPrefab)
        {
            var techType = customPrefab.PrefabInfo.TechType;

            if(customPrefab is ICraftable craftable && techType != TechType.None && craftable.FabricatorType != CraftTree.Type.None)
            {
                InternalLogger.Debug($"{customPrefab.PrefabInfo.ClassID} is ICraftable, Registering Craft Node, Recipe and Craft Speed.");

                if(craftable.StepsToFabricatorTab == null || craftable.StepsToFabricatorTab.Length == 0)
                    CraftTreeHandler.AddCraftingNode(craftable.FabricatorType, techType);
                else
                    CraftTreeHandler.AddCraftingNode(craftable.FabricatorType, techType, craftable.StepsToFabricatorTab);
                if(craftable.CraftingTime >= 0f)
                    CraftDataHandler.SetCraftingTime(techType, craftable.CraftingTime);

                if(craftable.RecipeData != null)
                    CraftDataHandler.SetTechData(techType, craftable.RecipeData);
                else
                    CraftDataHandler.SetTechData(techType, new RecipeData() { craftAmount = 1, Ingredients = new() { new(TechType.Titanium) } });

            }

            if(customPrefab is ICustomBattery customBattery)
            {
                InternalLogger.Debug($"{customPrefab.PrefabInfo.ClassID} is ICustomBattery, Adding to the Battery Registry.");
                var batteryType = customBattery.BatteryType;
                if(batteryType == API.BatteryType.Battery || batteryType == API.BatteryType.Both)
                    CustomBatteryHandler.RegisterCustomBattery(techType);

                if(batteryType == API.BatteryType.PowerCell || batteryType == API.BatteryType.Both)
                    CustomBatteryHandler.RegisterCustomPowerCell(techType);
            }
        }

    }
}
