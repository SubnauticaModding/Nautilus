namespace SMLHelper.Handlers
{
    using Assets;
    using SMLHelper.Utility;

    /// <summary>
    /// A handler for registering Unity prefabs associated to a <see cref="TechType"/>.
    /// </summary>
    public static class PrefabHandler
    {
        /// <summary>
        /// Registers a ModPrefab into the game.
        /// </summary>
        /// <param name="prefab">The mod prefab to register. Create a child class inheriting off this one and configure as needed.</param>
        /// <seealso cref="ModPrefab"/>
        public static void RegisterPrefab(ModPrefabRoot prefab)
        {

            foreach (ModPrefabRoot modPrefab in ModPrefabCache.Prefabs)
            {
                var techtype = modPrefab.PrefabInfo.TechType == prefab.PrefabInfo.TechType && modPrefab.PrefabInfo.TechType != TechType.None;
                var classid = modPrefab.PrefabInfo.ClassID == prefab.PrefabInfo.ClassID;
                var filename = modPrefab.PrefabInfo.PrefabPath == prefab.PrefabInfo.PrefabPath;
                if(techtype || classid || filename)
                {
                    InternalLogger.Error($"Another ModPrefab is already registered with these values. {(techtype ? "TechType: " + modPrefab.PrefabInfo.TechType : "")} {(classid ? "ClassId: " + modPrefab.PrefabInfo.ClassID : "")} {(filename ? "PrefabFileName: " + modPrefab.PrefabInfo.PrefabPath : "")}");
                    return;
                }
            }

            ModPrefabCache.Add(prefab);
        }
    }
}
