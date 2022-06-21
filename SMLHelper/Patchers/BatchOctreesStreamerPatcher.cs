using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using System.Reflection;
using WorldStreaming;
namespace SMLHelper.V2.Patchers
{
    internal class BatchOctreesStreamerPatcher
    {
        [HarmonyPatch(typeof(BatchOctreesStreamer),nameof(BatchOctreesStreamer.GetPath))]
        internal static class BatchOctreesStreamer_GetPath_Patch
        {
            [HarmonyPostfix]
            internal static void Postfix(Int3 batchId,ref string __result)
            {
                for(int i = 0; i < BiomeThings.Variables.biomes.Count;i++)
                {
                    var biome = BiomeThings.Variables.biomes[i];
                    if (biome.batchIds.Contains(batchId))
                    {
                        var formatted = string.Format("compiled-batch-{0}-{1}-{2}.optoctrees", batchId.x, batchId.y, batchId.z);
                        __result = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "BiomeOctreeCache", formatted);
                    }
                }
            }
        }
    }
}
