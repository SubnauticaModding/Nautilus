using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = QModManager.Utility.Logger;
namespace SMLHelper.V2.BiomeThings
{
    ///    <summary>
    /// A class for adding biomes.
    ///   </summary>
    public abstract class Biome
    {
        /// <summary>
        /// The name of the added biome, must be a string.
        /// </summary>
        public abstract string BiomeName { get; }
        /// <summary>
        /// The Rich Presence Message of the biome, must be a string.
        /// </summary>
        public virtual string BiomeRichPresence { get; }
        /// <summary>
        /// The RGB value of the biome on the provided biome map.
        /// </summary>
        public abstract Color32 ColorOnBiomeMap { get; }
        /// <summary>
        /// The bedrock type of the biome (I'm actually not sure what this does)
        /// </summary>
        public abstract int BedRockType { get; }
        /// <summary>
        /// The Ground Type of the biome (I'm actually not sure what this does)
        /// </summary>
        public abstract int GroundType { get; }
        /// <summary>
        /// The Debug Type of the biome (I'm actually not sure what this does)
        /// </summary>
        public abstract int DebugType { get; }
        /// <summary>
        /// Call this method to Finialize setting values, and add the biome to the game.
        /// </summary>
        public void Patch()
        {
            Variables.biomes.Add(this);
            var cols1 = Variables.finalbiomemap.GetPixels();
            var cols2 = EditedBiomeMap(out var dummy,out var dummy2).GetPixels();
            var dict1 = new Dictionary<Color,Vector2>();
            var dict2 = new Dictionary<Color,Vector2>();
            for (var i = 0; i < cols1.Length; i++)
            {
                var e = Variables.finalbiomemap.GetPixel(i,(i + 1));
                dict1.Add(e.linear,new Vector2(i,(i+1)));
            }
            for (var i = 0; i < cols1.Length; i++)
            {
                var e = EditedBiomeMap(out var dummy3,out var dummy4).GetPixel(i, (i + 1));
                dict2.Add(e.linear,new Vector2(i,(i+1)));
            }
            for (var i = 0; i < cols1.Length; ++i)
            {
                if (cols1[i].linear == new Color(117,134,142) || cols1[i].linear == new Color(175,225,126) || cols1[i].linear == new Color(202,70,101) || cols1[i].linear == new Color(184,119,93) || cols1[i].linear == new Color(55,79,168) || cols1[i].linear == new Color(158,144,213) || cols1[i].linear == new Color(70,146,187) || cols1[i].linear == new Color(168,255,252) || cols1[i].linear == new Color(142,99,6) || cols1[i].linear == new Color(208,208,208) || cols1[i].linear == new Color(126,90,90) || cols1[i].linear == new Color(0,0,0) || cols1[i].linear == new Color(62,100,77) || cols1[i].linear == new Color(246,229,169) || cols1[i].linear == new Color(131,59,110) || cols1[i].linear == new Color(42,49,51) || cols1[i].linear == new Color(123,160,188) || cols1[i].linear == new Color(225,82,225) || cols1[i].linear == new Color(90,21,0))
                {
                    continue;
                }
                if (cols1[i].linear != cols2[i].linear && dict1[cols1[i].linear] == dict2[cols2[i].linear])
                {
                    throw new Exception("Two different biomes in same position.");
                }
                
                cols1[i] += cols2[i];
            }
            Variables.finalbiomemap.SetPixels(cols1);
            Variables.finalbiomemap.Apply();
            QModManager.Utility.Logger.Log(QModManager.Utility.Logger.Level.Info, $"Patched biome {BiomeName}");
        }
        /// <summary>
        /// The BiomeMap you have edited.
        /// </summary>
        /// <returns>An Array of Color32</returns>
        public abstract Texture2D EditedBiomeMap(out int height, out int width);
    }

    internal static class Variables
    {
        internal static readonly List<Biome> biomes = new List<Biome>();
        internal static Texture2D finalbiomemap = SMLHelper.V2.Utility.ImageUtils.LoadTextureFromFile(Path.Combine(Directory.GetParent(Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName).FullName, @"\Subnautica_Data\StreamingAssets\SNUnmanagedData\Build18\biomeMap.png"));
        internal static int finalbiomemapheight => finalbiomemap.height;
        internal static int finalbiomemapwidth => finalbiomemap.width;
    }
}
