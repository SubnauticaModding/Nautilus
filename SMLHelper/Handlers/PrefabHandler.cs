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
        public static void RegisterPrefab(ModPrefab prefab)
        {
            RegisterPrefab(prefab.PrefabInfo);
        }

        /// <summary>
        /// Registers a ModPrefab into the game.
        /// </summary>
        /// <param name="prefabInfo">The prefabInfo to register.</param>
        /// <seealso cref="ModPrefab"/>
        public static void RegisterPrefab(PrefabInfo prefabInfo)
        {

            foreach (PrefabInfo registeredInfo in ModPrefabCache.Prefabs)
            {
                var techtype = registeredInfo.TechType == prefabInfo.TechType && registeredInfo.TechType != TechType.None;
                var classid = registeredInfo.ClassID == prefabInfo.ClassID;
                var filename = registeredInfo.PrefabFileName == prefabInfo.PrefabFileName;
                if(techtype || classid || filename)
                {
                    InternalLogger.Error($"Another ModPrefab is already registered with these values. {(techtype ? "TechType: " + registeredInfo.TechType : "")} {(classid ? "ClassId: " + registeredInfo.ClassID : "")} {(filename ? "PrefabFileName: " + registeredInfo.PrefabFileName : "")}");
                    return;
                }
            }

            ModPrefabCache.Add(prefabInfo);
        }
    }
}
