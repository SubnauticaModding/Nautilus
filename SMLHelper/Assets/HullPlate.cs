namespace SMLHelper.V2.Assets
{
    using System;
    using SMLHelper.V2.Crafting;
    using UnityEngine;

    /// <summary>
    /// A hull plate item that can be built into the game world.
    /// </summary>
    /// <seealso cref="PdaItem" />
    /// <seealso cref="Spawnable"/>
    /// <seealso cref="Buildable"/>
    public abstract class HullPlate : Buildable
    {
        /// <summary>
        /// Gets the <see cref="Texture2D"/> that the hull plate should have
        /// </summary>
        /// <returns></returns>
        public abstract Texture2D GetTexture();

        /// <summary>
        /// Initializes a new <see cref="HullPlate"/>, the basic class for any hull plate.
        /// </summary>
        /// <param name="classId">The main internal identifier for this item. Your item's <see cref="TechType" /> will be created using this name.</param>
        /// <param name="friendlyName">The name displayed in-game for this item whether in the open world or in the inventory.</param>
        /// <param name="description">The description for this item; Typically seen in the PDA, inventory, or crafting screens.</param>
        protected HullPlate(string classId, string friendlyName, string description)
            : base(classId, friendlyName, description)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public sealed override TechGroup GroupForPDA => TechGroup.Miscellaneous;

        /// <summary>
        /// 
        /// </summary>
        public sealed override TechCategory CategoryForPDA => TechCategory.MiscHullplates;

#if SUBNAUTICA
        /// <summary>
        /// This provides the <see cref="TechData"/> instance used to designate how this item is crafted or constructed.
        /// </summary>
        protected override TechData GetBlueprintRecipe()
        {
            return new TechData(new Ingredient(TechType.Titanium, 1), new Ingredient(TechType.Glass, 1));
        }
#elif BELOWZERO
        /// <summary>
        /// This provides the <see cref="RecipeData"/> instance used to designate how this item is crafted or constructed.
        /// </summary>
        protected override RecipeData GetBlueprintRecipe()
        {
            return new RecipeData(new Ingredient(TechType.Titanium, 1), new Ingredient(TechType.Glass, 1));
        }
#endif

        /// <summary>
        /// Gets the prefab game object.
        /// You are not required to override this method if you are using <see cref="HullPlate"/>
        /// </summary>
        /// <returns>The game object to be instantiated into a new in-game entity.</returns>
        public override GameObject GetGameObject()
        {
            GameObject obj = GameObject.Instantiate(CraftData.GetPrefabForTechType(TechType.DioramaHullPlate));

            MeshRenderer meshRenderer = obj.FindChild("Icon").GetComponent<MeshRenderer>();
            meshRenderer.material.mainTexture = this.GetTexture();

            return obj;
        }
    }
}
