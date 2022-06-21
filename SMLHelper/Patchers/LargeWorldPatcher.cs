using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UWE;
using SMLHelper.V2.BiomeThings;
using UnityEngine;

namespace SMLHelper.V2.Patchers
{
    internal class LargeWorldPatcher
    {
        [HarmonyPatch(typeof(LargeWorld),nameof(LargeWorld.InitializeBiomeMap))]
        internal static class LargeWorld_InitializeBiomeMap
        {
            [HarmonyPostfix]
            public static void Postfix(LargeWorld __instance)
            {
                    __instance.biomeMap = Variables.finalbiomemap.GetPixels32();
                    __instance.biomeMapHeight = Variables.finalbiomemapheight;
                    __instance.biomeMapWidth = Variables.finalbiomemapwidth;
                    __instance.biomeDownFactor = __instance.land.data.sizeX / __instance.biomeMapWidth;
            }
        }
        [HarmonyPatch(typeof(LargeWorld),nameof(LargeWorld.LoadBiomeMapLegend))]
        internal static class LargeWorld_LoadBiomeMapLegend_Patch
        {
            [HarmonyPostfix]
            internal static void Postfix(Dictionary<Int3,BiomeProperties> __result)
            {
                for (var i = 0; i < Variables.biomes.Count;i++)
                {
                    var biome = Variables.biomes[i];
                    var biomeproperties = new BiomeProperties();
                    biomeproperties.name = biome.BiomeName;
                    biomeproperties.bedrockType = biome.BedRockType;
                    biomeproperties.groundType = biome.GroundType;
                    biomeproperties.debugType = biome.DebugType;
                    __result.Add(Int3.FromRGB(biome.ColorOnBiomeMap), biomeproperties);
                }
            }
        }
    }
}
