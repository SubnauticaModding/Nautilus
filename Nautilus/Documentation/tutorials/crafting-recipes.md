# Editing Crafting Recipes

Recipes in Subnautica are combining one or multiple items to craft a new and more advanced item in various crafting stations.  

Nautilus offers the [RecipeData](xref:Nautilus.Crafting.RecipeData) class with sufficient data for recipes.  

Below is a table of all the parameters you may interact with in the RecipeData class.  

| Parameter Name | Type                 | Description                                                    |
|----------------|----------------------|----------------------------------------------------------------|
| craftAmount    | int                  | Amounts of copies of the item that is created for this recipe. |
| Ingredients    | List&lt;TechType&gt; | A list of ingredients required for this recipe.                |
| LinkedItems    | List&lt;TechType&gt; | Items that will also be created when this recipe is crafted.   |


To register or edit recipes, use the `Nautilus.Handlers.CraftDataHandler.SetRecipeData()` method.

## Examples
The following examples demonstrate the usage of the `SetRecipeData` method.
```csharp
// Set the Titanium Ingot's recipe to only two titaniums
RecipeData titaniumIngotRecipe = new RecipeData(new CraftData.Ingredient(TechType.Titanium, 2));

// register the recipe
CraftDataHandler.SetRecipeData(TechType.TitaniumIngot, titaniumIngotRecipe);


// Make the scrap metal recipe yield 10 titaniums instead of 5
RecipeData scrapMetalRecipe = new RecipeData
{
    // We don't want to get a new scrap metal in this recipe, so it should be 0.
    craftAmount = 0,
    
    // Require a scrap metal for the recipe
    Ingredients =
    {
	    new CraftData.Ingredient(TechType.ScrapMetal)
    },
    
    // Yield 10 titaniums when crafted
    LinkedItems = Enumerable.Repeat(TechType.Titanium, 10).ToList()
};

// register the recipe
CraftDataHandler.SetRecipeData(TechType.ScrapMetal, scrapMetalRecipe);
```

## See also
- [SetRecipeData()](xref:Nautilus.Handlers.CraftDataHandler.SetRecipeData(TechType,Nautilus.Crafting.RecipeData))
- [SetRecipeData()](xref:Nautilus.Handlers.CraftDataHandler.GetRecipeData(TechType))
- [RecipeData](xref:Nautilus.Crafting.RecipeData)