using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SMLHelper.V2.Patchers
{
    internal class PAXTerrainControllerPatcher
    {
        [HarmonyPatch(typeof(PAXTerrainController),nameof(PAXTerrainController.LoadAsync))]
        internal static class PAXTerrainController_LoadAsync_Patch
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                var obj = new GameObject("WorldShifter");
                obj.EnsureComponent<WorldShifter>();
            }
        }
    }
}
