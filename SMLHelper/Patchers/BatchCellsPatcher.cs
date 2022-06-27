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
        [HarmonyPatch(typeof(BatchCells),nameof(BatchCells.InitCellsTiers))]
        internal static class BatchCells_InitCellsTiers_Patch
        {
            [HarmonyPrefix]
            internal static bool Prefix(BatchCells __instance)
            {
                __instance.cellsTier0 = new Array3<EntityCell>(50);
                __instance.cellsTier1 = new Array3<EntityCell>(50);
                __instance.cellsTier2 = new Array3<EntityCell>(50);
                __instance.cellsTier3 = new Array3<EntityCell>(50);
                return false;
            }
        }
    }
}
