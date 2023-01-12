namespace SMLHelper.Handlers
{
    using Assets;

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
            foreach (ModPrefab modPrefab in ModPrefabCache.Prefabs)
            {
                if (modPrefab.TechType == prefab.TechType || modPrefab.ClassID == prefab.ClassID || modPrefab.PrefabFileName == prefab.PrefabFileName)
                {
                    return;
                }
            }

            ModPrefabCache.Add(prefab);
        }
    }
}
