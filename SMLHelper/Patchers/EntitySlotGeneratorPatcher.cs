using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
namespace SMLHelper.V2.Patchers
{
    internal class EntitySlotGeneratorPatcher
    {
            [HarmonyPatch(typeof(EntitySlotGenerator), nameof(EntitySlotGenerator.Initialize))]
            [PatchUtils.Prefix]
            internal static bool ESG_Initialize_Prefix(ref UnityEngine.Bounds wsBounds)
            {
                wsBounds.size *= 1000;
                return true;
            }
        
            [HarmonyPatch(typeof(EntitySlotGenerator.GeneratorRule), nameof(EntitySlotGenerator.GeneratorRule.ShouldSpawn))]
            [PatchUtils.Transpiler]
            internal static IEnumerable<CodeInstruction> ESG_GR_ShouldSpawn_Transpiler(IEnumerable<CodeInstruction> list_)
            {
                var list = new List<CodeInstruction>(list_);
                for(var i = 0; i < list.Count;i++)
                {
                    if (list[i].Calls(typeof(EntitySlotGenerator.GeneratorRule).GetMethod(nameof(EntitySlotGenerator.GeneratorRule.Match),System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)))
                    {
                        list[i].operand = typeof(EntitySlotGeneratorPatcher).GetMethod(nameof(EntitySlotGeneratorPatcher.MatchReplace), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                    }
                }
                return list.AsEnumerable();
            }
            internal static bool MatchReplace(EntitySlotGenerator.GeneratorRule __instance,string voxelBiome,string voxelMaterial)
            {
                
                if(BiomeThings.Variables.biomes.Exists(biome => biome.BiomeName == voxelBiome))
                {
                    return true;
                }
               return __instance.Match(voxelBiome, voxelMaterial);
            }
        internal static void Patch(Harmony h)
        {
            PatchUtils.PatchClass(h);
            Logger.Log("Patched EntitySlotGenerator");
        }
    }
}
