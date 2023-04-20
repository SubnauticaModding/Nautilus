## What are equipment types?
`EquipmentType` is an enum that handles special items. The possible values for this enum are listed below.

```csharp
public enum EquipmentType
{
    None, // Normal item
    Hand, // The item can be equipped in the Hand slot
    Head, // The item can be equipped in the Head slot
    Body, // The item can be equipped in the Body slot
    Gloves, // The item can be equipped in the Gloves slot
    Foots, // The item can be equipped in the Feet slot
    Tank, // The item can be equipped in the Oxygen Tank slot
    Chip, // The item can be equipped in the Chip slots
    CyclopsModule, // The item can be equipped in the Cyclops as an upgrade module
    VehicleModule, // The item can be equipped both in the Seamoth and in the Prawn Suit as an upgrade module
    NuclearReactor, // The item can be used in a Nuclear Reactor
    BatteryCharger, // When batteries are thrown in it, they get charged (for buildables)
    PowerCellCharger, // When power cells are thrown in it, they get charged (for buildables)
    SeamothModule, // The item can be equipped in the Seamoth as an upgrade module
    ExosuitModule, // The item can be equipped in the Prawn Suit as an upgrade module
    ExosuitArm, // The item can be equipped in the Prawn Suit as an arm
    DecoySlot // (Need actual name) Possibly for the decoy tube thing in the cyclops
}
```

## How can I create a custom background type?
Since equipment types are simply just enums, we can use the enum handler to create a new instance.
```csharp
private void Awake()
{
    var myCustomEquipmentType = EnumHandler.AddEntry<EquipmentType>("CustomEquipmentType");
}
```

And that's it. Now you can use the new `CraftData.BackgroundType` instance anywhere you want.

## How can edit an item's equipment type?
To edit an item's equipment type, you need to call the `CraftDataHandler.SetEquipmentType` method sitting in the `Nautilus.Handlers` namespace

### Examples
The following example demonstrates the usage of `SetEquipmentType` that enables the player to wear titanium on their head.

```csharp
CraftDataHandler.SetEquipmentType(TechType.Titanium, EquipmentType.Head);
```

Similarly, if we wanted to set the titanium's equipment type to our custom equipment type from earlier, it would look like the following:
```csharp
CraftDataHandler.SetBackgroundType(TechType.Titanium, myCustomEquipmentType);
```

If you're setting the equipment type for a custom prefab, we recommend using the `ICustomPrefab.SetEquipment` method instead.
```csharp
var customPrefab = new CustomPrefab("CustomItem", ".", ".");
customPrefab.SetEquipment(EquipmentType.Head);
// rest of the custom prefab configuration is omitted for brevity.
```

> [!WARNING] 
> It is dangerous to edit equipment types for items that already have one, because they can break.  
> For instance, modifying the equipment type for the Radiation Helmet will disable the player from wearing it.

