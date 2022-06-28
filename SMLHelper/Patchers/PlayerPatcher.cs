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
            [PatchUtils.Postfix]
            internal static void Player_Awake_Postfix(Player __instance)
            {
                __instance.gameObject.EnsureComponent<FloatingOrigin>().ReferenceObject = __instance.transform;
            }
            [HarmonyPatch(typeof(Player), nameof(Player.SetPosition), new Type[] { typeof(Vector3) })]
            [PatchUtils.Prefix]
            internal static bool Player_SetPosition_Prefix(ref Vector3 wsPos)
            {
                wsPos -= FloatingOrigin.CurrentOffset;
                return true;
            }
            [HarmonyPatch(typeof(Player), nameof(Player.UpdateBiomeRichPresence))]
            [PatchUtils.Postfix]
            public static void Player_UBRP_Postfix(string biomeStr)
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
        internal static void Patch(Harmony h)
        {
            PatchUtils.PatchClass(h);
            Logger.Log("Patched Player");
        }
        
    }
}
