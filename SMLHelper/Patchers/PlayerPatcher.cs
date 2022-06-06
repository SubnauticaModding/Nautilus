using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SMLHelper.V2.Patchers
{
    internal class PlayerPatcher
    {
        
        [HarmonyPatch(typeof(Player), nameof(Player.UpdateBiomeRichPresence))]
        public static class Player_UpdateBiomeRichPresence_Patch
        {
            [HarmonyPostfix]
            public static void Postfix(string biomeStr)
            {
                foreach (var biome in BiomeThings.Variables.biomes)
                {
                    if (biome.BiomeName == biomeStr)
                    {
                        if (!string.IsNullOrEmpty(biome.BiomeRichPresence))
                        {
                            PlatformUtils.main.GetServices().SetRichPresence(biome.BiomeRichPresence);
                        }
                        else
                        {
                            PlatformUtils.main.GetServices().SetRichPresence($"Exploring {biome.BiomeName}");
                        }
                        return;
                    }
                }
            }
        }
        
    }
}
