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
namespace SMLHelper.V2.Patchers
{
    internal class LargeWorldPatcher
    {
        [HarmonyPatch(typeof(LargeWorld),nameof(LargeWorld.InitializeBiomeMap))]
        internal static class LargeWorld_InitializeBiomeMap
        {
            [HarmonyPrefix]
            public static bool Prefix(LargeWorld __instance)
            {
                __instance.biomeMap = Variables.finalbiomemap.GetPixels32();
                __instance.biomeMapHeight = Variables.finalbiomemapheight;
                __instance.biomeMapWidth = Variables.finalbiomemapwidth;
                __instance.biomeDownFactor = __instance.land.data.sizeX / __instance.biomeMapWidth;
                var biomemaplegend = new Dictionary<Int3, BiomeProperties>();
                foreach(var biome in SMLHelper.V2.BiomeThings.Variables.biomes)
                {
                    var biomeproperties = new BiomeProperties();
                    biomeproperties.name = biome.BiomeName;
                    biomeproperties.bedrockType = biome.BedRockType;
                    biomeproperties.groundType = biome.GroundType;
                    biomeproperties.debugType = biome.DebugType;
                    biomemaplegend.Add(Int3.FromRGB(biome.ColorOnBiomeMap), biomeproperties);
                }
                var tex = SMLHelper.V2.Utility.ImageUtils.LoadTextureFromFile(Path.Combine(Directory.GetParent(Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName).FullName, @"\Subnautica_Data\StreamingAssets\SNUnmanagedData\Build18\legendColors.png"));

                var pixels32 = tex.GetPixels32();
                List<BiomeProperties> biomePropertiesList = CSVUtils.Load<BiomeProperties>(Path.Combine(Directory.GetParent(Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName).FullName, @"\Subnautica_Data\StreamingAssets\SNUnmanagedData\Build18\biomes.csv"));
                Dictionary<Int3, BiomeProperties> dictionary = biomemaplegend;
                for (int index = 0; index < biomePropertiesList.Count; ++index)
                { 
                    Int3 key = Int3.FromRGB(pixels32[biomePropertiesList.Count - index - 1]);
                    dictionary[key] = biomePropertiesList[index];
                }
                __instance.biomeMapLegend = biomemaplegend;
                return true;
            }
        }
    }
}
