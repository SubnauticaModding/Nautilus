using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using WorldStreaming;
namespace SMLHelper.V2.Patchers
{
    internal class BatchOctreesPatcher
    {
        [HarmonyPatch(typeof(BatchOctrees), nameof(BatchOctrees.LoadOctrees))]
        internal static class BatchOctrees_LoadOctrees_Patch
        {
            [HarmonyPrefix]
            internal static bool Prefix(BatchOctrees __instance)
            {
                var shouldContinue = false;
                BiomeThings.Biome containingBiome = null;
                for (var e = 0; e < BiomeThings.Variables.biomes.Count; e++)
                {
                    var biome = BiomeThings.Variables.biomes[e];
                    if (biome.batchIds.Contains(__instance.id))
                    {
                        shouldContinue = true;
                        containingBiome = biome;
                        break;
                    }
                }
                if(shouldContinue)
                {
                    var instantiatedgo = UnityEngine.GameObject.Instantiate(containingBiome.batchroots[__instance.id]);
                    LargeWorldStreamer.main.OnBatchObjectsLoaded(__instance.id, instantiatedgo);
                    return false;
                }
                return true;
            }
        }
    }
}
