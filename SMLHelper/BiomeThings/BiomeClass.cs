using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = QModManager.Utility.Logger;
using UWE;
using WorldStreaming;
namespace SMLHelper.V2.BiomeThings
{
    ///    <summary>
    /// A class for adding biomes.
    ///   </summary>
    public abstract class Biome
    {
        /// <summary>
        /// The sky of your biome.
        /// </summary>
        public abstract mset.Sky Sky { get; }
        /// <summary>
        /// The Waterscape Volume Settings of your biome.
        /// </summary>
        public abstract WaterscapeVolume.Settings WaterScapeSettings { get; }
        /// <summary>
        /// SpawnInfos for what you want to initially spawn into your biome without initializing a coordinated spawn class.
        /// </summary>
        public abstract List<SMLHelper.V2.Handlers.SpawnInfo> SpawnInfos { get; }
        
        /// <summary>
        /// The name of the added biome, must be a string.
        /// </summary>
        public abstract string BiomeName { get; }
        /// <summary>
        /// The Rich Presence Message of the biome, must be a string.
        /// </summary>
        public virtual string BiomeRichPresence { get; }
        /// <summary>
        /// The Ambient Light Settings of your biome.
        /// </summary>
        public abstract AmbientLightSettings amblightsettings { get; }
        /// <summary>
        /// The Fog Settings for the biome.
        /// </summary>
        public abstract FogSettings fogsettings { get; }
        /// <summary>
        /// Sunlight Settings for the biome.
        /// </summary>
        public abstract SunlightSettings sunsettings { get; }
        /// <summary>
        /// The batch Ids, of the biome, must be the same as the ones in your .optoctrees files
        /// </summary>
        public abstract List<Int3> batchIds { get; }
        internal Dictionary<Int3, GameObject> batchroots = new Dictionary<Int3, GameObject>();
        /// <summary>
        /// Call this method to Finialize setting values, and add the biome to the game.
        /// </summary>
        public void Patch()
        {
            if (!Directory.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "BiomeOctreeCache")))
                Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "BiomeOctreeCache"));
            /*
            var cols2_ = EditedBiomeMap(out var foo, out var bar);
            var cols1_ = Variables.finalbiomemap;
            var cols1 = cols1_.GetPixels();
            var cols2 = cols2_.GetPixels();
            if((cols2_.height * cols2_.width) > (cols1_.height * cols1_.width))
            {
                cols1_.height = cols2_.height;
                cols1_.width = cols2_.width;
            }
            for (int i = 0; i < (cols1_.width * cols1_.height); ++i)
            {
                int y = i / cols1_.width;
                int x = i - (y * cols1_.width);
                Color pixel = cols1_.GetPixel(x,y);
                Color pixel2 = cols2_.GetPixel(x, y);
                if (pixel.r != pixel2.r && pixel.g != pixel2.g && pixel.b != pixel2.b)
                {
                    throw new Exception("You cannot have two biomes in the same place.");
                } else if (pixel == cols2_.GetPixel(x, y))
                {
                    var random = new System.Random();
                    var color = new Color(random.NextByte(), random.NextByte(), random.NextByte());
                    while(Variables.usedrandomcolors.Contains(color))
                    {
                        color = new Color(random.NextByte(), random.NextByte(), random.NextByte());
                    }
                    cols2_.SetPixel(x, y, color);
                    Variables.usedrandomcolors.Add(color);
                    ColorOnBiomeMap = (Color32)color;
                }
                cols1_.SetPixel(x, y, cols2_.GetPixel(x, y));
            }
            if(Variables.finalbiomemap == null)
            {
                Variables.finalbiomemap = cols1_;
            }
            if (ColorOnBiomeMap == UnityEngine.Color.black)
            {
                var random = new System.Random();
                var color = new Color(random.NextByte(), random.NextByte(), random.NextByte());
                while (Variables.usedrandomcolors.Contains(color))
                {
                    color = new Color(random.NextByte(), random.NextByte(), random.NextByte());
                }
                Variables.usedrandomcolors.Add(color);
                ColorOnBiomeMap = (Color32)color;
            }
            */
            Variables.biomes.Add(this);
            
            QModManager.Utility.Logger.Log(QModManager.Utility.Logger.Level.Info, $"Patched biome {BiomeName}");
        }
        /// <summary>
        /// The BiomeMap you have edited.
        /// </summary>
        /// <returns>An Array of Color32</returns>
        public abstract Texture2D EditedBiomeMap(out int height, out int width);
        /// <summary>
        /// The gameobject with the collider for registering when biome is entered
        /// </summary>
        /// <returns>A gameobject with collider applied</returns>
        public abstract GameObject GetCollider();
        /// <summary>
        /// The Terrain for each batch of your biome, in format (BatchId,Terrain)
        /// </summary>
        public abstract Dictionary<Int3, GameObject> batchTerrains { get; }
    }

    internal static class Variables
    {
        internal static readonly List<Biome> biomes = new List<Biome>();
        internal static List<Int3> GetAllBiomeBatchIds()
        {
            var result = new List<Int3>();
            for(var i = 0;i < biomes.Count;i++)
            {
                var biome = biomes[i];
                for(var e = 0; i < biome.batchIds.Count;e++)
                {
                    result.Add(biome.batchIds[e]);
                }
            }
            return result;
        }
        internal static Texture2D finalbiomemap = SMLHelper.V2.Utility.ImageUtils.LoadTextureFromFile(Path.Combine(SNUtils.unmanagedDataDir,"Build18","biomeMap.png"));
        internal static int finalbiomemapheight => finalbiomemap.height;
        internal static int finalbiomemapwidth => finalbiomemap.width;
        internal static bool ignoreHeader = false;
        internal static List<Color> usedrandomcolors = new List<Color>();
    }
}
