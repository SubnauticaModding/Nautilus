using System.Reflection;
using BepInEx;
using HarmonyLib;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Extensions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UWE;

namespace Nautilus.Examples;

[BepInPlugin("com.snmodding.nautilus.creatureegg", "Nautilus Creature Egg Example Mod", Nautilus.PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.nautilus")]
public class CreatureEggExample : BaseUnityPlugin
{
    /// <summary>
    /// This patch is required because the reaper leviathan doesn't have a WaterParkCreature component and it's required for creatures in the ACU to work properly.
    /// </summary>
    [HarmonyPatch(typeof(LiveMixin), nameof(LiveMixin.Awake))]
    private static class Patcher
    {
        [HarmonyPostfix]
        private static void AwakePostfix(Creature __instance)
        {
# if SUBNAUTICA
            if (!__instance.TryGetComponent(out ReaperLeviathan _))
#else
            if (!__instance.TryGetComponent(out Chelicerate _))
#endif
            {
                return;
            }

#if SUBNAUTICA
            if (!PrefabDatabase.TryGetPrefabFilename(CraftData.GetClassIdForTechType(TechType.ReaperLeviathan), out var filename))
#else
            if (!PrefabDatabase.TryGetPrefabFilename(CraftData.GetClassIdForTechType(TechType.Chelicerate), out var filename))
#endif
            {
                return;
            }

            var wpc = __instance.gameObject.EnsureComponent<WaterParkCreature>();
            wpc.data = ScriptableObject.CreateInstance<WaterParkCreatureData>();
            wpc.data.eggOrChildPrefab = new AssetReferenceGameObject(filename).ForceValid();
            wpc.data.canBreed = true;
            // Initial size is when the creature just hatched
            wpc.data.initialSize = 0.04f;

            // Max size is the maximum size this creature can reach inside the ACU
            wpc.data.maxSize = 0.05f;

            // Outside size is when you drop the creature outside of the ACU
            wpc.data.outsideSize = 0.07f;

            // How long will it take for this creature to reach the maximum size
            wpc.data.daysToGrow = 6;

            wpc.data.isPickupableOutside = false;
        }
    }
    private void Awake()
    {
#if SUBNAUTICA
        CustomPrefab customEgg = new CustomPrefab("ReaperEgg", "Reaper Leviathan Egg", "Reaper Leviathan Egg that makes me go yes.");
#else
        CustomPrefab customEgg = new CustomPrefab("ChelicerateEgg", "Chelicerate Egg", "Chelicerate Egg that makes me go yes.");
#endif
        customEgg.Info.WithSizeInInventory(new Vector2int(3, 3));

        /*
         * Here we make the creature egg immune to the brine acid. Please note that a creature egg doesn't require an egg gadget to work.
         * The egg gadget simply has methods that add additional item functionality to the egg, but the egg will still work like an
         * egg without this gadget.
         * In this example, we've used the egg gadget simply to make it acid immune.
         */
        EggGadget eggGadget = customEgg.CreateCreatureEgg();
        eggGadget.SetAcidImmune(true);

        /*
         * Here we set the required ACU stacks to two. This means that you cannot drop this egg in an ACU unless you have 2 ACUs stacks on top of each other.
         */
        eggGadget.WithRequiredAcuSize(2);

        /*
         * Here we create an egg template instance that copies the Crash Egg model.
         * In the object initializer, we're setting the hatching creature to ReaperLeviathan and also made the egg take 3 days to hatch.
         */
#if SUBNAUTICA
        EggTemplate egg = new EggTemplate(customEgg.Info, TechType.CrashEgg)
#else
        EggTemplate egg = new EggTemplate(customEgg.Info, TechType.RockPuncherEgg)
#endif
        {
#if SUBNAUTICA
            HatchingCreature = TechType.ReaperLeviathan,
#else
            HatchingCreature = TechType.Chelicerate,
#endif
            HatchingTime = 3
        };

        /*
         * Here we make this egg have an unidentified egg tech type before hatching. Once it hatches, it will receive the main egg tech type.
         */
        egg.SetUndiscoveredTechType();

        /*
         * Set the game object of our custom prefab to the egg template we setup.
         */
        customEgg.SetGameObject(egg);

        /*
         * Register our custom fabricator to the game.
         * After this point, do not edit the prefab or modify gadgets as they will not be applied.
         */
        customEgg.Register();

        Harmony.CreateAndPatchAll(typeof(CreatureEggExample), PluginInfo.PLUGIN_GUID);
    }
}