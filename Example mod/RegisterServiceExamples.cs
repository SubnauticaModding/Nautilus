using System.Reflection;
using BepInEx;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Utility.AttributeRegistration;
using Nautilus.Utility.AttributeRegistration.RegistryRequirements;

namespace Nautilus.Examples;
[BepInPlugin(ModGuid, "Nautilus RegisterEvent Example Mod", Nautilus.PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.nautilus")]
public class RegisterServiceExamples : BaseUnityPlugin
{
    private const string ModGuid = "com.snmodding.nautilus.registerevent";
    
    // Register attributes allow you to define Prefabs (and more) with less code.
    // At their bare bones, they let you execute code blocks without the need to store a reference somewhere to execute it later
    // For this tutorial Prefabs are used with this registration system, but more can be used.
    [Register("MyCoolItem")]
    private static void RegisterMyCoolItem(PrefabInfo info)
    {
        CustomPrefab myPrefab = new CustomPrefab(info);
        myPrefab.Register();
        /*
         This is the minimum for a Custom Prefab
         Later when we have Nautilus search for and execute this method,
         It will pass in a PrefabInfo already assigned a TechType of the registry ID.
         So for this example myPrefab will have the TechType 'MyCoolItem'
         */
    }
    
    // Register Events can depend on 1 (or multiple) registries to be loaded before it.
    // Registers can also supply you modded TechType's with ease
    [Register("MyEpicItem", "MyCoolItem")]
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
        // (Although this can be crafted you won't be able to pick it up due to missing components)
        myPrefab.Register();
        /*
         In this example 2 things are showcased, MyCoolItem will have its RegisterMyCoolItem()
         method executed first,and then MyEpicItem will execute.
        */
        /*
         This method has A TechType myCoolItem in the method Header. The name of the TechType 
         parameter "myCoolItem" is used internally to create a modded enum value for you, so
         there is no need to use the EnumHandler or store static instances of your TechTypes.
         NOTE: a *dependency* does not always have to a modded TechType and vice versa. The
         dependency is usually the same name as the modded TechType, but this is not required
         */
    }
    
    // For cross mod dependencies, the syntax "modGUID::RegistryID" is used. Nautilus will execute your
    // registry once the other mod's registry is loaded, which could be during their load phase
    [RequireGuid("com.theplaguespreads.theredplague")]
    // require GUID is an example of conditional registrations. [RequireGuid] is a built-in implementor,
    // only executing when all defined Mod Guid's are present within the BepInEx Chainloader.
    // This functionality can be extended however by implementing IRegistryRequirement on an attribute.
    [Register("MyModdeditem", "com.theplaguespreads.theredplague::SomePrefab")]
    private static void RegisterModdedItem(PrefabInfo info, TechType someTRPPrefab)
    {
        // some code :3
        // This will not ever execute though as "SomePrefab" is not a valid prefab within TRP
    }

    // Now, to have Nautilus execute your registries, you must use the RegistryEventUtils
    private void Awake()
    {
        RegisterAttributeService service = new RegisterAttributeService(this);
        service.AddBasicInjectors();// This adds several injectors built into Nautilus. 
        // Custom injectors can be defined here and added as well, but this is not needed for most use cases
        service.ExecuteAssemblyRegisterAttributes(Assembly.GetExecutingAssembly());
        /* 
         If you need more debug info, enable Nautilus's Debug toggle in settings.
         RegistryEventUtils will log every time a registry is executed.
        */
    }
    
    // Bonus Info Below

    // Register Events are not limited to prefabs however
    // they can execute anything and are useful in managing load order of your mod
    [Register("MyCoolSounds", "SomeRegistryIWantToLoadFirst")]
    private static void RegisterMySounds()
    {
        // code for loading sounds that load after something else or whatever
    }
    
    // Register Events can also load assets from an asset bundle (provided to Nautilus in a later step)
    /* Though this is outside the scope of a basic tutorial, the code below should get you started,*/
    
    /*[RegisterEvent("MyEpicItem")]
    private static void RegisterItemWithAsset(PrefabInfo info, [AssetLoad("myBundleAssetIcon")] Sprite icon)
    {
        info.WithIcon(icon);
        CustomPrefab myPrefab = new CustomPrefab(info);
        myPrefab.Register();
        // Commented out as no asset bundle is present, will throw an exception if the asset fails to load
    }*/
}