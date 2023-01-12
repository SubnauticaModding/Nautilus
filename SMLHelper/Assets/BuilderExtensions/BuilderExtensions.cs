namespace SMLHelper.Assets
{
    using Handlers;
    using Utility;
    using UnityEngine;

    /// <summary>
    /// Extensions for the ModPrefab class to set things up without having to use Inheritance based prefabs.
    /// </summary>
    public static partial class BuilderExtensions
    {
        /// <summary>
        /// Registers the ModPrefab into the game and returns a ModPrefabBuilder if sucessfull or null if it fails.
        /// </summary>
        /// <param name="modPrefab">The mod prefab to register.</param>
        /// <seealso cref="ModPrefab"/>
        public static ModPrefabBuilder RegisterPrefab(this ModPrefab modPrefab)
        {
            foreach(ModPrefab prefab in ModPrefabCache.Prefabs)
            {
                var techtype = modPrefab.TechType == prefab.TechType && modPrefab.TechType != TechType.None;
                var classid = modPrefab.ClassID == prefab.ClassID;
                var filename = modPrefab.PrefabFileName == prefab.PrefabFileName;
                if(techtype || classid || filename)
                {
                    InternalLogger.Error($"Another ModPrefab is already registered with these values. {(techtype? "TechType: "+modPrefab.TechType: "")} {(classid? "ClassId: "+modPrefab.ClassID:"")} {(filename? "PrefabFileName: " + modPrefab.PrefabFileName:"")}");
                    return null;
                }
            }

            ModPrefabCache.Add(modPrefab);
            return ModPrefabBuilder.Create(modPrefab);
        }

        /// <summary>
        /// Registers the ModPrefab into the game.
        /// </summary>
        /// <param name="modPrefabBuilder">The ModPrefabBuilder with an unregisterd prefab.</param>
        /// <seealso cref="ModPrefab"/>
        public static ModPrefabBuilder RegisterPrefab(this ModPrefabBuilder modPrefabBuilder)
        {
            ModPrefab modPrefab = modPrefabBuilder.ModPrefab;
            foreach(ModPrefab prefab in ModPrefabCache.Prefabs)
            {
                var techtype = modPrefab.TechType == prefab.TechType && modPrefab.TechType != TechType.None;
                var classid = modPrefab.ClassID == prefab.ClassID;
                var filename = modPrefab.PrefabFileName == prefab.PrefabFileName;
                if(techtype || classid || filename)
                {
                    InternalLogger.Error($"Another ModPrefab is already registered with these values. {(techtype ? "TechType: " + modPrefab.TechType : "")} {(classid ? "ClassId: " + modPrefab.ClassID : "")} {(filename ? "PrefabFileName: " + modPrefab.PrefabFileName : "")}");
                    return modPrefabBuilder;
                }
            }

            ModPrefabCache.Add(modPrefab);
            return modPrefabBuilder;
        }

        /// <summary>
        /// Generates a Techtype using the ModPrefabs <see cref="ModPrefab.ClassID"/> with optional ability to set PDA info for the created TechType.
        /// </summary>
        /// <param name="modPrefabBuilder">The prefab to handle</param>
        /// <param name="displayName"></param>
        /// <param name="toolTip"></param>
        /// <param name="unlockAtStart"></param>
        /// <returns>The original Modprefab so these can be called in sequence.</returns>
        public static ModPrefabBuilder SetTechType(this ModPrefabBuilder modPrefabBuilder, string displayName = null, string toolTip = null, bool unlockAtStart = true)
        {
            ModPrefab modPrefab = modPrefabBuilder.ModPrefab;
            if(modPrefab.TechType != TechType.None)
            {
                InternalLogger.Error($"Cannot set TechType for {modPrefab.ClassID} as it already has a TechType of {modPrefab.TechType}.");
                return modPrefabBuilder;
            }    

            EnumBuilder<TechType> builder = EnumHandler.AddEntry<TechType>(modPrefab.ClassID);
            if(builder == null)
                return modPrefabBuilder;

            if(displayName != null && toolTip != null)
            {
                builder.WithPdaInfo(displayName, toolTip, unlockAtStart);
            }
            modPrefab.TechType = builder.Value;
            return modPrefabBuilder;
        }

#if SUBNAUTICA
        /// <summary>
        /// Registers the Sprite for this <see cref="ModPrefab"/> using its <see cref="ModPrefab.TechType"/> and the provided <see cref="Atlas.Sprite"/>. 
        /// </summary>
        /// <param name="modPrefabBuilder">The prefab to handle</param>
        /// <param name="sprite">The Sprite you want to register to this ModPrefab's TechType</param>
        /// <returns>The original Modprefab so these can be called in sequence.</returns>
        public static ModPrefabBuilder SetSprite(this ModPrefabBuilder modPrefabBuilder, Atlas.Sprite sprite)
        {
            ModPrefab modPrefab = modPrefabBuilder.ModPrefab;
            if(modPrefab.TechType == TechType.None)
            {
                InternalLogger.Error($"Cannot set Recipe for {modPrefab.ClassID} as it does not have a TechType.");
                return modPrefabBuilder;
            }

            SpriteHandler.RegisterSprite(modPrefab.TechType, sprite);
            return modPrefabBuilder;
        }
#endif

        /// <summary>
        /// Registers the Sprite for this <see cref="ModPrefab"/> using its <see cref="ModPrefab.TechType"/> and the provided <see cref="Sprite"/>. 
        /// </summary>
        /// <param name="modPrefabBuilder">The prefab to handle</param>
        /// <param name="sprite">The Sprite you want to register to this ModPrefab's TechType</param>
        /// <returns>The original Modprefab so these can be called in sequence.</returns>
        public static ModPrefabBuilder SetSprite(this ModPrefabBuilder modPrefabBuilder, Sprite sprite)
        {
            ModPrefab modPrefab = modPrefabBuilder.ModPrefab;
            if(modPrefab.TechType == TechType.None)
            {
                InternalLogger.Error($"Cannot set Recipe for {modPrefab.ClassID} as it does not have a TechType.");
                return modPrefabBuilder;
            }

            SpriteHandler.RegisterSprite(modPrefab.TechType, sprite);
            return modPrefabBuilder;
        }
    }
}
