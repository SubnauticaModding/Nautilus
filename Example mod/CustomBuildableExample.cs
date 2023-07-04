using BepInEx;
using Nautilus.Crafting;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Utility;
using UnityEngine;
using Nautilus.Assets.PrefabTemplates;
#if SUBNAUTICA
using Ingredient = CraftData.Ingredient;
#endif

namespace Nautilus.Examples;

[BepInPlugin("com.snmodding.nautilus.custombuildable", "Nautilus Custom Buildable Example Mod", PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.nautilus")]
public class CustomBuildableExample : BaseUnityPlugin
{
    private void Awake()
    {
        BuildableTallLocker.Register();
    }
}

public static class BuildableTallLocker
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("TallLocker", "Tall Locker", "A tall locker.")
        // set the icon to that of the vanilla locker:
        .WithIcon(SpriteManager.Get(TechType.Locker));

    public static void Register()
    {
        // create prefab:
        CustomPrefab prefab = new CustomPrefab(Info);

        // copy the model of a vanilla wreck piece (which looks like a taller locker):
        CloneTemplate lockerClone = new CloneTemplate(Info, "cd34fecd-794c-4a0c-8012-dd81b77f2840");

        // modify the cloned model:
        lockerClone.ModifyPrefab += obj =>
        {
            // allow it to be placced inside bases and submarines on the ground, and can be rotated:
            ConstructableFlags constructableFlags = ConstructableFlags.Inside | ConstructableFlags.Rotatable | ConstructableFlags.Ground | ConstructableFlags.Submarine;

            // find the object that holds the model:
            GameObject model = obj.transform.Find("submarine_locker_04").gameObject;

            // this line is only necessary for the tall locker so that the door is also part of the model:
            obj.transform.Find("submarine_locker_03_door_01").parent = model.transform;

            // add all components necessary for it to be built:
            PrefabUtils.AddConstructable(obj, Info.TechType, constructableFlags, model);

            // allow it to be opened as a storage container:
            PrefabUtils.AddStorageContainer(obj, "StorageRoot", "TallLocker", 3, 8, true);
        };

        // assign the created clone model to the prefab itself:
        prefab.SetGameObject(lockerClone);

        // assign it to the correct tab in the builder tool:
        prefab.SetPdaGroupCategory(TechGroup.InteriorModules, TechCategory.InteriorModule);

        // set recipe:
        prefab.SetRecipe(new RecipeData(new Ingredient(TechType.Titanium, 3), new Ingredient(TechType.Glass, 1)));

        // finally, register it into the game:
        prefab.Register();
    }
}