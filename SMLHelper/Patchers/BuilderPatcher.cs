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
        [HarmonyPatch(typeof(Builder),nameof(Builder.UpdateAllowed))]
        internal static class Builder_UpdateAllowed_Patch
        {
            [HarmonyPostfix]
            internal static void Postfix(ref bool __result)
            {
                if(BiomeThings.Variables.biomes.Exists(biome => biome.BiomeName == Player.main.GetBiomeString() && Player.main.IsInSub() is false))
                __result = true;
            }
        }
    }
}
