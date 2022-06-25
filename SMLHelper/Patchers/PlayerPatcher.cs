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
        [HarmonyPatch(typeof(Player), nameof(Player.Awake))]
        internal static class Player_Awake_Patch
        {
            [HarmonyPostfix]
            internal static void Postfix(Player __instance)
            {
                 __instance.gameObject.EnsureComponent<FloatingOrigin>().ReferenceObject = __instance.transform;
            }
        }
        [HarmonyPatch(typeof(Player),nameof(Player.SetPosition),new Type[] {typeof(Vector3)})]
        internal static class Player_SetPosition_Patch
        {
            [HarmonyPrefix]
            internal static bool Prefix(ref Vector3 wsPos)
            {
                wsPos -= FloatingOrigin.CurrentOffset;
                return true;
            }
        }
        [HarmonyPatch(typeof(Player), nameof(Player.UpdateBiomeRichPresence))]
        internal static class Player_UpdateBiomeRichPresence_Patch
        {
            internal static bool isWarping;
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
