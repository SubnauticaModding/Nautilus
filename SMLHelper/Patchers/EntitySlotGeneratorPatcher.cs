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
            internal static bool Prefix(ref UnityEngine.Bounds bounds)
            {
                bounds.size *= 1000;
                return true;
            }
        }
        [HarmonyPatch(typeof(EntitySlotGenerator.GeneratorRule), nameof(EntitySlotGenerator.GeneratorRule.ShouldSpawn))]
        internal static class GeneratorRule_ShouldSpawn_Patch
        {
            private static EntitySlotGenerator.GeneratorRule instance;
            internal static bool Prefix(EntitySlotGenerator.GeneratorRule __instance)
            {
                instance = __instance;
                return true;
            }
            [HarmonyTranspiler]
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> list_)
            {
                var list = new List<CodeInstruction>(list_);
                for(var i = 0; i < list.Count;i++)
                {
                    if (list[i].Calls(typeof(EntitySlotGenerator.GeneratorRule).GetMethod(nameof(EntitySlotGenerator.GeneratorRule.Match))))
                    {
                        list[i].operand = typeof(GeneratorRule_ShouldSpawn_Patch).GetMethod(nameof(GeneratorRule_ShouldSpawn_Patch.MatchReplace));
                    }
                }
                return list.AsEnumerable();
            }
            internal static bool MatchReplace(string voxelBiome,string voxelMaterial)
            {
                for(var i = 0;i < BiomeThings.Variables.biomes.Count;i++)
                {
                    var biome = BiomeThings.Variables.biomes[i];
                    if(biome.BiomeName == voxelBiome)
                    {
                        return true;
                    }
                }
               return instance.Match(voxelBiome, voxelMaterial);
            }
        }
    }
}
