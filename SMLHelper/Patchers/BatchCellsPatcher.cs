using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using System.Reflection;
namespace SMLHelper.V2.Patchers
{
    internal class BatchCellsPatcher
    {
            [HarmonyPatch(typeof(BatchCells), nameof(BatchCells.InitCellsTiers))]
            [PatchUtils.Prefix]
            internal static bool BatchCells_ICT_Prefix(BatchCells __instance)
            { 
                __instance.cellsTier0 = new Array3<EntityCell>(50);
                __instance.cellsTier1 = new Array3<EntityCell>(50);
                __instance.cellsTier2 = new Array3<EntityCell>(50);
                __instance.cellsTier3 = new Array3<EntityCell>(50);
                return false;
            }
        internal static void Patch(Harmony harmony)
        {
            PatchUtils.PatchClass(harmony);
            Logger.Info("Patched BatchCells");
        }
    }
}
