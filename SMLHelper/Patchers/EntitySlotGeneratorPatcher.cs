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
        internal static class ESG_Initialize_Patch
        {
            [HarmonyPrefix]
            internal static bool Prefix(ref UnityEngine.Bounds wsBounds)
            {
                wsBounds.size *= 1000;
                return true;
            }
        }
        [HarmonyPatch(typeof(EntitySlotGenerator.GeneratorRule), nameof(EntitySlotGenerator.GeneratorRule.ShouldSpawn))]
        internal static class GeneratorRule_ShouldSpawn_Patch
        {
            [HarmonyTranspiler]
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> list_)
            {
                var list = new List<CodeInstruction>(list_);
                for(var i = 0; i < list.Count;i++)
                {
                    if (list[i].Calls(typeof(EntitySlotGenerator.GeneratorRule).GetMethod(nameof(EntitySlotGenerator.GeneratorRule.Match),System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)))
                    {
                        list[i].operand = typeof(GeneratorRule_ShouldSpawn_Patch).GetMethod(nameof(GeneratorRule_ShouldSpawn_Patch.MatchReplace), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
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
        }
    }
}
