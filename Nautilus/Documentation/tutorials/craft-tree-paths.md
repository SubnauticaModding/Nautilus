# SN1 Crafting Tree Paths

List of default crafting tree paths for use in the `CraftingGadget.WithStepsToFabricatorTab(...)` method, alongside any of the methods in the [CraftTreeHandler](xref:Nautilus.Handlers.CraftTreeHandler) class.

Remember that paths divided by slashes must always be separate strings when used in Nautilus functions. For example:

```csharp
// EX1: Add the prefab to the Advanced Materials tab.
craftingGadget.WithStepsToFabricatorTab("Resources", "AdvancedMaterials");

// EX2: Add the prefab to the root of the fabricator.
craftingGadget.WithStepsToFabricatorTab();

// EX3: Add the prefab to the Vehicles tab of the mobile vehicle bay.
craftingGadget.WithFabricatorType(CraftTree.Type.Constructor)
    .WithStepsToFabricatorTab("Vehicles");
```

## Fabricator (Fabricator):
Resources
  - BasicMaterials (Basic Materials)
    - [Craft nodes]
  - AdvancedMaterials (Advanced Materials)
    - [Craft nodes]
  - Electronics
    - [Craft nodes]

Survival
  - Water
    - [Craft nodes]
  - CookedFood (Cooked food)
    - [Craft nodes]
  - CuredFood (Cured food)
    - [Craft nodes]

Personal
  - Equipment
    - [Craft nodes]
  - Tools
    - [Craft nodes]

Machines
  - [Craft nodes]

#### Simplified list of paths
* `Resources/BasicMaterials`
* `Resources/AdvancedMaterials`
* `Resources/Electronics`
* `Survival/Water`
* `Survival/CookedFood`
* `Survival/CuredFood`
* `Personal/Equipment`
* `Personal/Tools`
* `Machines`

---

## Workbench (Modification station):
Not applicable; all crafting nodes are located in the root.

---

## Constructor (Mobile vehicle bay):
  - Vehicles
    - [Craft nodes]
  - Rocket
    - [Craft nodes]

#### Simplified list of paths
* `Vehicles`
* `Rocket`

---

## CyclopsFabricator:
Not applicable; all crafting nodes are located in the root.

---

## Centrifuge:
Not applicable; all crafting nodes are located in the root.

---

## MapRoom (Scanner Room Fabricator):
Not applicable; all crafting nodes are located in the root.

---

## SeamothUpgrades (Vehicle Upgrade Console):
CommonModules (Common Modules)
  - [Craft nodes]

SeamothModules (Seamoth Modules)
  - [Craft nodes]

ExosuitModules (Prawn Suit Modules)
  - [Craft nodes]

Torpedoes (Torpedoes)
  - [Craft nodes]

#### Simplified list of paths
* `CommonModules`
* `SeamothModules`
* `ExosuitModules`
* `Torpedoes`