using System.Reflection;
using BepInEx;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Utility.AttributeRegistrationUtils;

namespace Nautilus.Examples;
[BepInPlugin("com.snmodding.nautilus.registerevent", "Nautilus RegisterEvent Example Mod", Nautilus.PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.nautilus")]
public class RegisterEventExamples  : BaseUnityPlugin
{
    // Register Events allow you to define Prefabs with less code
    [RegisterEvent("MyCoolItem")]
    private static void RegisterMyCoolItem(PrefabInfo info)
    {
        CustomPrefab myPrefab = new CustomPrefab(info);
        myPrefab.Register();
        /*
         This is the bare minimum for a Custom Prefab
         Later when we have Nautilus search for and execute this method,
         It will pass in a PrefabInfo already assigned a TechType of the iD of the register.
         So for this example myPrefab will have the TechType 'MyCoolItem'
         */
        /*
         IMPORTANT: if your Registry's iD is the exact same from another mod,
         your registry may fail to execute. Ensure you give it a unique name that won't cause,
         any conflicts with other mods using the system. In most cases, if your TechType is
         unique, you won't have to worry about this.
         */
    }
    
    // Register Events can depend on 1 (or multiple) registries to be loaded before it.
    // Registers can also supply you modded TechType's with ease
    [RegisterEvent("MyEpicItem", "MyCoolItem")]
    private static void RegisterMyEpicItem(PrefabInfo info, TechType myCoolItem)
    {
        CustomPrefab myPrefab = new CustomPrefab(info);
        myPrefab
            .SetRecipe(new RecipeData()
            {
                craftAmount = 1,
                Ingredients =
                {
                    /*
                     We can set the crafting here to use a modded TechType without having to
                     touch EnumHandler
                     */
                    new Ingredient(myCoolItem, 1)
                }
            })
            .WithFabricatorType(CraftTree.Type.Fabricator)
            .WithStepsToFabricatorTab("Personal", "Equipment");
        // Although this can be crafted you won't be able to pick it up due to missing components
        myPrefab.Register();
        /*
         In this example 2 things are showcased, MyCoolItem will have its RegisterMyCoolItem()
         method executed first,and then MyEpicItem will execute.
        */
        /*
         This method has A TechType myCoolItem in the method Header. The name of the TechType 
         parameter "myCoolItem" is used internally to create a modded enum value for you, so
         there is no need to use the EnumHandler or store static instances of your TechTypes.
         NOTE: a *dependency* does not always have to a modded TechType and vise versa. The
         dependency is usually the same name as the modded TechType but this is not required
         */
        /*
         NOTE: You can also depend on registries from other mods to load first. Simply put
         whatever their registry uses as an ID into your dependency list and Nautilus will
         handle the rest. This is also why your IDs have to be unique, as conflicts with other
         mods can occur.
         */
    }
    
    // Now, to have Nautilus execute your registries, you must use the RegistryEventUtils
    private void Awake()
    {
        // Here we ask Nautilus to search our assembly and execute our registries
        AttributeRegistrationUtils.ExecuteAssemblyAttributeRegistries(Assembly.GetExecutingAssembly());
        
        /*
         If you need more debug info, enable Nautilus's Debug toggle in settings.
         RegistryEventUtils will log every time a registry is executed.
        */
    }
    
    // Bonus Info Below

    /*Register Events are not limited to prefabs however
     they can execute anything and are useful in managing load order of your mod*/
    [RegisterEvent("MyCoolSounds", "SomeRegistryIWantToLoadFirst")]
    private static void RegisterMySounds()
    {
        // code for loading sounds that load after something else or whatever
    }
    
    // Register Events can also load assets from an asset bundle (provided to Nautilus in a later step)
    /* Though this is outside the scope of a basic tutorial, the code below should get you started */
    
    /*[RegisterEvent("MyEpicItem")]
    private static void RegisterItemWithAsset(PrefabInfo info, [AssetLoad("myBundleAssetIcon")] Sprite icon)
    {
        info.WithIcon(icon);
        CustomPrefab myPrefab = new CustomPrefab(info);
        myPrefab.Register();
        // Commented out as no asset bundle is present, will throw an exception if the asset fails to load
    }*/
}