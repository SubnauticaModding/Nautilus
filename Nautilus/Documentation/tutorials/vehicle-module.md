# Using the Vehicle Upgrade Module Gadget

Vehicle modules are often hard to mod, and when multiple mods patch a function of a vehicle there can be severe conflicts that could break all the mods adding new modules.  
The Vehicle Upgrade Module Gadget was made to make your life of modder as easy as possible when it comes to vehicle modules.  

This brand new gadget was made to be compatible with both Below Zero and Subnautica 1, some functions differ between Nautilus for SN1 and Nautilus for BZ.

## Possibilities of custom vehicle upgrade modules

- Set the new crush depth of the vehicle when the upgrade is equipped. You can choose whether the depth is absolute or added to the default crush depth of the vehicle.
- Set the max charge of the module if the module slot type is set to `QuickSlotType.SelectableChargeable` or `QuickSlotType.Chargeable`. The way to measure the charge is still blurry.
- Set the energy cost of the module if the module slot type is set to `QuickSlotType.Selectable`, `QuickSlotType.Instant`, `QuickSlotType.SelectableChargeable` or `QuickSlotType.Chargeable`. Charge does not affect the energy consumption.
- Set the cooldown of the module if the module slot type is set to `QuickSlotType.Selectable`, `QuickSlotType.Instant`, `QuickSlotType.SelectableChargeable` or `QuickSlotType.Chargeable`. Charge does not affect the cooldown. The unit of the cooldown is seconds, and stored as a `double` on Subnautica 1 and as a `float` on Below Zero.
- Add delegates when some actions are done, and they all give many parameters so you can make actions on the vehicle. Here are the events supported by Nautilus, and preventing conflicts between mods:
  - When the module is added to the vehicle
  - When the module is removed from the vehicle
  - When the module is used (Any active `QuickSlotType` triggers this except `QuickSlotType.Toggleable`)
  - When the module is toggled. One of the parameters given into the delegate is a boolean which is the state of the module.

## What is the Upgrade Module Gadget

The Upgrade Module Gadget is an extension for the `ICustomPrefab`. It is basically patching many functions of the vehicles using prefix, postfixes and transpilers for a better compatibility and more safety.  
Basically, all the modules are stored locally by Nautilus in order to run actions, set the modules settings in game, such as the crush depth if provided, the cooldown if provided, etc...

# Create a module with the gadget

Here are some examples of uses of the gadget, used in many ways.  
You are not limited, you can do everything you want with those.

## Passive module setting a new crush depth

Let's create a custom module setting the depth to, let's say, 1700m for the SeaMoth.  
So, first, let's make our prefab info.

```csharp
var prefabInfo = PrefabInfo.WithTechType("SeamothDepthUpgrade", "Seamoth Depth Module MK.4", "Dive down to 1700 meters!!! Let's meet the Sea Dragon!")
    .WithIcon(SpriteManager.Get(TechType.HullReinforcementModule3));
CustomPrefab prefab = new CustomPrefab(prefabInfo);
```


Then, we're making the Custom Prefab based on Reinforced Hull prefab.

```csharp
var clone = new CloneTemplate(prefabInfo, TechType.HullReinforcementModule3);
prefab.SetGameObject(clone);
```


Now, let's quickly set the basic settings.

```csharp
prefab.SetRecipe(new Crafting.RecipeData()
{
    craftAmount = 1,
    Ingredients = new List<CraftData.Ingredient>()
    {
        new CraftData.Ingredient(TechType.HullReinforcementModule3),
        new CraftData.Ingredient(TechType.AluminumOxyde, 2),
        new CraftData.Ingredient(TechType.AdvancedWiringKit, 3)
    }
})
    .WithFabricatorType(CraftTree.Type.SeamothUpgrades)
    .WithStepsToFabricator("SeamothModules")
    .WithCraftingTime(5f);
```


And finally, let's make the part that interest us: Adding the Upgrade Module Gadget.

```csharp
// This first function defines the equipment type and the quick slot type.
prefab.SetVehicleUpgradeModule(EquipmentType.SeamothModule, QuickSlotType.Passive)
    .WithDepthUpgrade(1700f, true)  // 1700 is the depth in meters. The boolean after defines if we want the value to be absolute or "relative", added to the default depth. 
                                    // If it is on false, it will set the new crush depth to 1900 meters because the default depth of the Seamoth is 200 meters.
                                    // Otherwise, it will set the new crush depth on 1700 meters.
    .WithOnModuleAdded((Vehicle vehicleInstance, int slotId) =>
    {
        Subtitles.Add("Well done! The module is working! The new depth is 1700 meters.");
    })
    .WithOnModuleRemoved((Vehicle vehicleInstance, int slotId) => {
        Subtitles.Add("Ah... You removed the upgrade. Take care of your hull.");
    });
```


> [!NOTE]
> You can do basically everything you want in these delegates, such as trigger story events, destroy the vehicle, add components to the vehicle instance, etc...

## Chargeable self-destruct module

Admitting we've already done the prefab info and the custom prefab, let's directly work only with the upgrade module gadget.

```csharp
var maxCharge = 50f;
var cooldown = 10f;
var energyCost = 6.9f;
prefab.SetVehicleUpgradeModule(EquipmentType.VehicleModule, QuickSlotType.SelectableChargeable)
    .WithMaxCharge(maxCharge)
    .WithCooldown(cooldown)
    .WithEnergyCost(energyCost)
    .WithOnModuleAdded((Vehicle inst, int slotId) =>
    {
        Subtitles.Add("Self-destruct module installed. The module needs to be charged fully to detonate.");
    })
    .WithOnModuleRemoved((Vehicle inst, int slotId) =>
    {
        Subtitles.Add("Self-destruct module uninstalled.");
    })
    .WithOnModuleUsed((Vehicle inst, int slotID, float charge, float chargeScalar) =>
    {
        if (charge < maxCharge)
        {
            Subtitles.Add("Self-destruction sequence disengaged.")
            return;
        }
        else
        {
            Subtitles.Add("Self-destruction sequence engaged.")
            UWE.CoroutineHost.StartCoroutine(EngageSelfDestruct(inst, countdown));
        }
    });

static IEnumerator EngageSelfDestruct(Vehicle instance, float countdown)
{
    var startTime = Time.time
    while(Time.time < (startTime + countdown))
    {
        yield return null;
    }
    instance.liveMixin.Kill(DamageType.Explosive);
}
```


## Instant selectable message sender

Yet another example...

```csharp
prefab.SetVehicleUpgradeModule(EquipmentType.VehicleModule, QuickSlotType.Selectable)
    .WithOnModuleUsed((Vehicle inst, int slotID, float _charge, float _chargeScalar) => // charge and chargeScalar are always 0f here.
    {
        Subtitles.Add("Hello world!");
    });
```


And you can do a lot more.
