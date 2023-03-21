using BepInEx;
using SMLHelper.Assets;
using SMLHelper.Assets.Gadgets;
using SMLHelper.Assets.PrefabTemplates;
using SMLHelper.Crafting;

namespace SMLHelper.Examples;

[BepInPlugin("com.snmodding.smlhelper.customfabricator", "SMLHelper Custom Fabricator Example Mod", PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.smlhelper")]
public class CustomFabricatorExample : BaseUnityPlugin
{
    private void Awake()
    {
	    /*
	     * Here we create a very simple clone of the Nickel ore. We will be using this item for the recipe of our
	     * Custom fabricator later.
	     */
	    PrefabInfo stoneInfo = PrefabInfo.WithTechType("Stone", "Stone", "A good looking stone")
		    .WithIcon(SpriteManager.Get(TechType.Nickel));
	    CustomPrefab stone = new CustomPrefab(stoneInfo);
	    stone.SetGameObject(new CloneTemplate(stone.Info, TechType.Nickel));
	    stone.Register();
	    
	    /*
	     * To create a custom fabricator, as usual, we will be starting by initializing our CustomPrefab object.
	     */
        CustomPrefab customFab = new CustomPrefab(PrefabInfo.WithTechType("CustomFab", "Custom Fabricator", "My awesome custom fabricator!")
            .WithIcon(SpriteManager.Get(TechType.Fabricator)));
        
        /*
         * If we need our custom fabricator to use a new crafting tree (I.E; with new tabs and crafting nodes),
         * We will have to make our own CraftTree.Type. This is simply done by using the CustomPrefab.CreateFabricator method.
         * This method returns a FabricatorGadget, which we can use to customize our crafting tree. For example, to add a
         * New tab or crafting node.
         * This step is optional. If you wish to use an existing crafting tree, you may skip this step.
         */
        customFab.CreateFabricator(out CraftTree.Type treeType)
	        // Simply add the Battery to our fabricator.
            .AddCraftNode(TechType.Battery);
        
        /*
         * Here we construct our fabricator game object. The second parameter of the FabricatorTemplate constructor is
         * The CraftTree.Type our fabricator will use. If you wish to use an existing crafting tree, you may set this
         * To an existing CraftTree.Type.
         * In this example, our template will use the Workbench model.
         */
        FabricatorTemplate fabPrefab = new FabricatorTemplate(customFab.Info, treeType)
        {
	        FabricatorModel = FabricatorTemplate.Model.Workbench
        };
        customFab.SetGameObject(fabPrefab);

        /*
         * This is a string example of how a RecipeData may look like in a json file. TechTypes can be
         * Modded string values as well. The accepted values for tech types are: Vanilla names, modded names, and numbers.
         * In this example, the recipe represents:
         * - 1 titanium
         * - 1 nickel ore (7 is the integer number for nickel ore)
         * - The custom item we made earlier; Stone.
         */
        string recipeJson = """
        {
        	"craftAmount": 1,
        	"Ingredients": [
                {
        		"techType": "Titanium",
        		"amount": 1
        	    },
                {
        		"techType": 7,
        		"amount": 1
        	    },
                {
        		"techType": "Stone",
        		"amount": 1
        	    }
            ]
        }
        """;

        /*
         * This is what the aforementioned json object will look like as a RecipeData object.
         * You may use the CustomPrefab.SetRecipe() to set the recipe to a RecipeData object.
         */
        RecipeData recipe = new RecipeData
        {
	        craftAmount = 1,
	        Ingredients =
	        {
		        new CraftData.Ingredient(TechType.Titanium, 1),
		        new CraftData.Ingredient(TechType.Nickel, 1),
		        new CraftData.Ingredient(stone.Info.TechType, 1)
	        }
        };

#if JSONRECIPE
        /*
         * Set the recipe to the json object we made earlier.
         */
        customFab.SetRecipeFromJson(recipeJson);
#else
	    /*
	     * Set the recipe.
	     */
	    customFab.SetRecipe(recipe);
#endif

	    /*
	     * Make the modification station a requirement for our custom fabricator blueprint.
	     * Additionally we also add our custom fabricator to the Interior modules PDA group and category.
	     * Setting the tech group to a group that exists in the habitat builder will make our item buildable.
	     */
        customFab.SetUnlock(TechType.Workbench)
            .WithPdaGroupCategory(TechGroup.InteriorModules, TechCategory.InteriorModule);
        
        /*
         * Register our custom fabricator to the game.
         * After this point, do not edit the prefab or modify gadgets as they will not be applied.
         */
        customFab.Register();
    }
}