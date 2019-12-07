namespace SMLHelper.V2.Patchers
{
    using System; 
    using Harmony;
    using System.Collections.Generic;
    using UnityEngine;
    using UWE;
    using Logger = V2.Logger;

    internal class LootDistributionPatcher
    {
        internal static SelfCheckingDictionary<string, LootDistributionData.SrcData> CustomSrcData = new SelfCheckingDictionary<string, LootDistributionData.SrcData>("CustomSrcData");

        internal static void Patch(HarmonyInstance harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(LootDistributionData), "Initialize"),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(LootDistributionPatcher), "InitializePostfix")));

            Logger.Log("LootDistributionPatcher is done.", LogLevel.Debug);
        }

        private static void InitializePostfix(LootDistributionData __instance)
        {
            foreach(var entry in CustomSrcData)
            {
                if(__instance.srcDistribution.ContainsKey(entry.Key))
                {
                    LootDistributionData.SrcData srcData = __instance.srcDistribution[entry.Key];
                    LootDistributionData.SrcData customSrcData = entry.Value;
                    string classId = entry.Key;
                
                    foreach(var customBiomeDist in customSrcData.distribution)
                    {
                        bool foundBiome = false;

                        for(int i = 0; i < srcData.distribution.Count; i++)
                        {
                            LootDistributionData.BiomeData biomeDist = srcData.distribution[i];

                            if(customBiomeDist.biome == biomeDist.biome)
                            {
                                biomeDist.count = customBiomeDist.count;
                                biomeDist.probability = customBiomeDist.probability;

                                foundBiome = true;
                            }
                        }

                        if (!foundBiome)
                        {
                            srcData.distribution.Add(customBiomeDist);
                        }

                        if (__instance.dstDistribution.TryGetValue(customBiomeDist.biome, out LootDistributionData.DstData dstData))
                        {
                            bool foundPrefab = false;

                            for (int j = 0; j < dstData.prefabs.Count; j++)
                            {
                                LootDistributionData.PrefabData prefabData = dstData.prefabs[j];

                                if (prefabData.classId == classId)
                                {
                                    prefabData.count = customBiomeDist.count;
                                    prefabData.probability = customBiomeDist.probability;

                                    foundPrefab = true;
                                }
                            }

                            if(!foundPrefab)
                            {
                                dstData.prefabs.Add(new LootDistributionData.PrefabData()
                                {
                                    classId = classId,
                                    count = customBiomeDist.count,
                                    probability = customBiomeDist.probability
                                });
                            }
                        }
                    }
                }
                else
                {
                    __instance.srcDistribution.Add(entry.Key, entry.Value);

                    string classId = entry.Key;
                    LootDistributionData.SrcData data = entry.Value;
                    List<LootDistributionData.BiomeData> distribution = data.distribution;

                    if (distribution != null)
                    {
                        for(int i = 0; i < distribution.Count; i++)
                        {
                            LootDistributionData.BiomeData biomeData = distribution[i];
                            BiomeType biome = biomeData.biome;
                            int count = biomeData.count;
                            float probability = biomeData.probability;
                            LootDistributionData.DstData dstData;

                            if (!__instance.dstDistribution.TryGetValue(biome, out dstData))
                            {
                                dstData = new LootDistributionData.DstData();
                                dstData.prefabs = new List<LootDistributionData.PrefabData>();
                                __instance.dstDistribution.Add(biome, dstData);
                            }

                            LootDistributionData.PrefabData prefabData = new LootDistributionData.PrefabData();
                            prefabData.classId = classId;
                            prefabData.count = count;
                            prefabData.probability = probability;
                            dstData.prefabs.Add(prefabData);
                        }
                    }
                }
            }
        }
    }
}
