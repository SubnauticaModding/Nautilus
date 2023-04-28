# Adding Spawns

Most of the time in Subnautica, the game uses two different ways to spawn objects; static spawns that always have a fixed position in the world to spawn in,
and loot distribution which is biome-based and sudo-random.  

Nautilus offers modders to add spawns to either systems. You can add spawns for your own custom item, you can also edit vanilla spawns,
or straight up remove them.

## Coordinated Spawns
Coordinated spawns is Nautilus' version of the aforementioned static spawns. With this system, you are allowed to specify exact world position and rotation spawns for an item.  
You may register one or more coordinated spawn(s) for any item by providing either their class ID, or their tech type.

### Examples
The following examples demonstrate the usage of [CoordinatedSpawnsHandler](xref:Nautilus.Handlers.CoordinatedSpawnsHandler) methods.

```csharp
private void Awake()
{
    // Adds a Reaper Leviathan to the lava lakes
    SpawnInfo reaperInfo = new SpawnInfo(TechType.ReaperLeviathan, new Vector3(280f, -1400f, 47f)); // Lava Lakes
    CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(reaperInfo);
	
	// Adds multiple spawn infos at once
	
	// Sand Shark's class ID
	string sandSharkId = "5e5f00b4-1531-45c0-8aca-84cbd3b580a4";
		
	var spawnInfos = new List<SpawnInfo>() 
	{
		new SpawnInfo(TechType.Seamoth, Vector3.zero),
		new SpawnInfo(sandSharkId, new Vector3(10, -4, 5), Vector3.up * 90f) // rotate its Y axis 90 degrees
	}
	CoordinatedSpawnsHandler.RegisterCoordinatedSpawns(spawnInfos);
	
	// Spawns a batch of titaniums around 10, -3, 15 world position
	var randomPositions = RandomPositions(new Vector3(10f, -3f, 15f));
	CoordinatedSpawnsHandler.RegisterCoordinatedSpawnsForOneTechType(
		TechType.Titanium, randomPositions);
}

private List<Vector3> RandomPositions(Vector3 centerPosition)
{
    var result = new List<Vector3>();
    for (int i = 0; i < 5; i++)
    {
        result.Add(centerPosition + (Random.insideUnitSphere * i));
    }
    return result;
}
```  
  
For custom prefabs, it is advised to use the [ICustomPrefab.SetSpawns(SpawnLocation[])](xref:Nautilus.Assets.Gadgets.GadgetExtensions#Nautilus_Assets_Gadgets_GadgetExtensions_SetSpawns_Nautilus_Assets_ICustomPrefab_Nautilus_Assets_SpawnLocation___) method instead of directly interacting with the [CoordinatedSpawnsHandler](xref:Nautilus.Handlers.CoordinatedSpawnsHandler) class.  

The example below demonstrates the usage of the `SetSpawns` method.
```csharp
var blueReaper = new CustomPrefab("BlueReaper", "Blue Reaper Leviathan", null);
           
// Creates a clone of the Reaper Leviathan prefab and colors it blue, then set the new prefab as our Blue Reaper's game object.          
var blueReaperPrefab = new CloneTemplate(blueReaper.Info, TechType.ReaperLeviathan)
{
    ModifyPrefab = prefab => prefab.GetComponentsInChildren<Renderer>().ForEach(r => r.materials.ForEach(m => m.color = Color.blue))
};                                               
blueReaper.SetPrefab(blueReaperPrefab);

// Adds a spawn for our Blue Reaper Leviathan in the lava lakes.
blueReaper.SetSpawns(new SpawnLocation(280f, -1400, 47f));

// Register the Blue Reaper Leviathan to the game.
blueReaper.Register();
```  

## Loot Distribution
Loot distribution system is by far the most widely used spawning system in the game. Unlike static spawns, Nautilus does not have it's own version of this system, so
we will be registering distributions into the game's system.  
Loot distribution only allows adding or editing distributions using a class ID and prefab file name. You normally will also need to provide a biome type, probability, and count for each loot you want to add.  

Below is a table of all the parameters you may interact with in the loot distribution system.  

| Parameter Name    | Type                           | Description                                                                                                                                      |
|-------------------|--------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------|
| classId           | string                         | The classId of the entity to add loot for.                                                                                                       |
| prefabFileName    | string                         | The internal file path of the entity to add.                                                                                                     |
| biomeDistribution | LootDistributionData.BiomeData | The biome data in which this entity might spawn in.                                                                                              |
| probability       | float                          | The chance of this entity spawning. This value can only be between 0-1 inclusive. 0 being no chance of spawning, while 1 is guaranteed to spawn. |
| count             | float                          | Multiplies 1 with this value. This multiplication is accounted everytime this entity has the highest chance to spawn, not a global count.        |
| srcData           | LootDistributionData.SrcData   | A class that combines the prefab file name and biome distribution to one data type.                                                              |
| entityInfo        | WorldEntityInfo                | Contains information on how to spawn this entity. E.G: The size it should spawn in as, and how far it can stay before unloading.                 |

> [!WARNING]
> An Entity Info for each class ID to spawn via loot distribution is required. If an entity info does not have one, the loot distribution will ignore it.

> [!NOTE]
> Usually, vanilla prefabs do have a world entity info assigned to them. While you can, you don't have to register a new one for those that already have one.

### Examples
The following examples demonstrate the usage of [LootDistributionHandler](xref:Nautilus.Handlers.LootDistributionHandler) methods.

```csharp
// Drillable Sulphur's class ID
string drillableSulphurClassId = "697beac5-e39a-4809-854d-9163da9f997e";

var biomes = new LootDistribution.BiomeData[]
{
 // Lost river's bones field ground
 new LootDistributionData.BiomeData { biome = BiomeType.BonesField_Ground, count = 1, probability = 0.07f },
 
 // Inactive Lava Zone floor, near the lava
 new LootDistributionData.BiomeData { biome = BiomeType.InactiveLavaZone_Chamber_Floor_Far, count = 1, probability = 0.05f }
};

// Add spawn for the drillable sulphur
LootDistributionHandler.AddLootDistributionData(drillableSulphurClassId, biomes);

string rockgrubClassId = CraftData.GetClassIdForTechType(TechType.Rockgrub);

// Prevents the rockgrub from spawning in the Bulb zone caves.
LootDistributionHandler.EditLootDistributionData(rockgrubClassId, BiomeType.KooshZone_CaveWall, 0f, 0);
```  
  
For custom prefabs, it is advised to use the [ICustomPrefab.SetSpawns(LootDistributionData.BiomeData[])](xref:Nautilus.Assets.Gadgets.GadgetExtensions#Nautilus_Assets_Gadgets_GadgetExtensions_SetSpawns_Nautilus_Assets_ICustomPrefab_Nautilus_Assets_SpawnLocation___) method instead of directly interacting with the [LootDistributionHandler](xref:Nautilus.Handlers.LootDistributionHandler) class.

The example below demonstrates the usage of the `SetSpawns` method.
```csharp
// Set the vanilla titanium icon for our item
CustomPrefab titaniumClone = new CustomPrefab("TitaniumClone", "Titanium Clone", "Titanium clone that makes me go yes.", SpriteManager.Get(TechType.Titanium));

// Creates a clone of the Titanium prefab and colors it red, then set the new prefab as our Titanium Clone's game object.  
PrefabTemplate cloneTemplate = new CloneTemplate(titaniumClone.Info, TechType.Titanium)
{
    // Callback to change all material colors of this clone to red.
    ModifyPrefab = prefab => prefab.GetComponentsInChildren<Renderer>().ForEach(r => r.materials.ForEach(m => m.color = Color.red))
};
titaniumClone.SetPrefab(cloneTemplate);

titaniumClone.SetSpawns(
        // Adds a chance for our titanium clone to spawn in Safe shallows grass, x4 each time.
        new BiomeData { biome = BiomeType.SafeShallows_Grass, count = 4, probability = 0.1f },
        // Adds a chance for our titanium clone to spawn in Safe shallows caves, once each time.
        new BiomeData { biome = BiomeType.SafeShallows_CaveFloor, count = 1, probability = 0.4f });

// Register the Titanium Clone to the game.
titaniumClone.Register();
```

## See also
   - [CoordinatedSpawnsHandler](xref:Nautilus.Handlers.CoordinatedSpawnsHandler)
   - [LootDistributionHandler](xref:Nautilus.Handlers.LootDistributionHandler)