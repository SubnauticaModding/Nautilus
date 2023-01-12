namespace SMLHelper.Assets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// A shell class to use with a TON of Handler extensions.
    /// </summary>
    public sealed class ModPrefabBuilder
    {
        private ModPrefabBuilder(ModPrefab modPrefab)
        {
            ModPrefab = modPrefab;
        }

        /// <summary>
        /// The ModPrefab that the Extension methods will perform their work on.
        /// </summary>
        public ModPrefab ModPrefab { get; }

        /// <summary>
        /// Left public so people do not have to register before they want to.
        /// </summary>
        /// <remarks>If you are using this you will still need to register your prefab using the extension for the game to be able to find your prefab.</remarks>
        /// <param name="modPrefab"></param>
        public static ModPrefabBuilder Create(ModPrefab modPrefab)
        {
            return new ModPrefabBuilder(modPrefab);
        }
    }
}
