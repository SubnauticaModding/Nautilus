using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using System.Reflection.Emit;
namespace SMLHelper.V2.Patchers
{
    internal class WaterBiomeManagerPatcher
    {
        [HarmonyPatch(typeof(WaterBiomeManager), nameof(WaterBiomeManager.Start))]
        internal static class WBM_Start_Patch
        {
            [HarmonyPrefix]
            internal static bool Prefix(WaterBiomeManager __instance)
            {
                for (var i = 0; i < BiomeThings.Variables.biomes.Count; i++)
                {
                    var biome = BiomeThings.Variables.biomes[i];
                    var settings = new WaterBiomeManager.BiomeSettings()
                    {
                        name = biome.BiomeName,
                        settings = biome.WaterScapeSettings,
                        skyPrefab = new UnityEngine.GameObject("Useless Junk.")
                    };
                    __instance.biomeSettings.Add(settings);
                }
                return true;
            }
            [HarmonyTranspiler]
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {

                return new CodeMatcher(instructions)
                .MatchForward(false,

                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Ldloc_0),
                new CodeMatch(OpCodes.Callvirt),
                new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Callvirt, typeof(MarmoSkies).GetMethod(nameof(MarmoSkies.GetSky), new Type[] { typeof(UnityEngine.GameObject) }))
                )
                .RemoveInstruction()
                .RemoveInstruction()
                .RemoveInstruction()
                .RemoveInstruction()
                .SetOpcodeAndAdvance(OpCodes.Ldloc_0)
                .SetAndAdvance(OpCodes.Call, AccessTools.Method(typeof(WBM_Start_Patch), nameof(WBM_Start_Patch.GetSkyReplace)))
                .InstructionEnumeration();
            }



            
                internal static mset.Sky GetSkyReplace(MarmoSkies __instance, Int32 index)
            {
                var name = WaterBiomeManager.main.biomeSettings[index].name;
                QModManager.Utility.Logger.Log(QModManager.Utility.Logger.Level.Info, name);
                if (BiomeThings.Variables.biomes.Exists(biome => biome.BiomeName.ToLower() == name.ToLower()))
                {
                    var biome = BiomeThings.Variables.biomes.First(biome => biome.BiomeName.ToLower() == name.ToLower());
                    UnityEngine.Object.Destroy(WaterBiomeManager.main.biomeSettings[index].skyPrefab);
                    var go = new UnityEngine.GameObject($"Sky for {biome.BiomeName}");
                    go.transform.SetParent(__instance.transform);
                    var component = go.EnsureComponent<mset.Sky>();
                    var fields = component.GetType().GetFields();
                    for(var i = 0; i < fields.Count(); i++)
                    {
                            fields[i].SetValue(component, fields[i].GetValue(biome.Sky));

                    }
                    return component;
                }
                if (WaterBiomeManager.main.biomeSettings[index].skyPrefab is not null)
                {
                    return __instance.GetSky(WaterBiomeManager.main.biomeSettings[index].skyPrefab);
                }
                return null;
            }
        }
    }
}
