using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
namespace SMLHelper.V2.Patchers
{
    internal class CellManagerPatcher
    {
            [HarmonyPatch(typeof(CellManager), nameof(CellManager.InitializeBatchCells))]
            [PatchUtils.Prefix]
            internal static bool CellManager_IBC_Prefix(CellManager __instance, Int3 index, ref BatchCells __result)
            {
                if (BiomeThings.Variables.biomes.Exists(biome => biome.batchIds.Contains(index)))
                {
                    var batchcells = new BatchCells();
                    batchcells.Init(__instance, LargeWorldStreamer.main, index);
                    __instance.batch2cells[index] = batchcells;
                    __result = batchcells;
                    return false;
                }
                return true;
            }
      internal static void Patch(Harmony harmony)
        {
            PatchUtils.PatchClass(harmony);
            Logger.Log("Patched CellManager");
        }
    }
}
