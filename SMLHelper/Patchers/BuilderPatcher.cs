using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
namespace SMLHelper.V2.Patchers
{
    internal class BuilderPatcher
    {
            [HarmonyPatch(typeof(Builder), nameof(Builder.UpdateAllowed))]
            [PatchUtils.Postfix]
            internal static void Builder_UpdateAllowed_Postfix(ref bool __result)
            {
                if(BiomeThings.Variables.biomes.Exists(biome => biome.BiomeName == Player.main.GetBiomeString() && Player.main.IsInSub() is false))
                __result = true;
            }
        internal static void Patch(Harmony h)
        {
            PatchUtils.PatchClass(h);
            Logger.Log("Patched Builder");
        }
    }
}
