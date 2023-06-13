# Loading and instantiating prefabs

In Subnautica, objects are created (obviously). Peepers spawn in the water around you. Titanium appears when outcrops are broken.
Your Seamoth is built in front of you as soon as you craft it. But how exactly is this done, and how can you recreate this in your own mods?

In reality there are many ways to do this. There are likely some that have yet to be discovered. Each method has its ups and downs,
so it's a good idea to be familiar with all of them.

## Asynchronous loading

---

As of the Living Large update, both SN1 and Below Zero use **asynchronous** prefab loading. Therefore, prefab loading now must always be handled within coroutines, because
the loading occurs across multiple frames. Please note that this is not a guide on coroutines. You can look that up on your own;
they have been extensively explained online.

Also take note of the `UWE.CoroutineHost.StartCoroutine(IEnumerator)` method. This amazing utility method provided by Subnautica lets you execute a coroutine
at any point in your mod, without needing your own MonoBehaviour to host it.

> [!NOTE]
> Resources.Load should no longer be used. The game does NOT use the Resources folder for managing prefabs. Instead, Unknown Worlds has begun using Unity's
[Addressable Asset System](https://docs.unity3d.com/Packages/com.unity.addressables@0.8/manual/index.html). Luckily, the game has many utilities that interact with
that for us.

## Methods of loading prefabs

--- 

### CraftData.GetPrefabForTechTypeAsync

```csharp
public static CoroutineTask<GameObject> GetPrefabForTechTypeAsync(TechType techType, bool verbose = true)
```

This is arguably the most simple way of loading prefabs. This method only takes a TechType and returns a coroutine task.

A coroutine task holds a reference to the prefab once it is complete. However, it will not be loaded instantly. You must write `yield return task` to await its
completion. Only then can you safely call its `GetResult()` method.

Example code that spawns a Peeper in front of the player:
```csharp
private static IEnumerator SpawnPeeper()
{
    // Fetch the prefab:
    CoroutineTask<GameObject> task = CraftData.GetPrefabForTechTypeAsync(TechType.Peeper);
    // Wait for the prefab task to complete:
    yield return task;
    // Get the prefab:
    GameObject prefab = task.GetResult();

    // Instantiate the prefab with a random rotation 2 meters in front of the player camera:
    Instantiate(prefab, MainCamera.camera.transform.position + (MainCamera.camera.transform.forward * 2), Random.rotation);
}
```

Bare minimum code with inferred typing:
```csharp
private static IEnumerator SpawnPeeper()
{
    var task = CraftData.GetPrefabForTechTypeAsync(TechType.Peeper);
    yield return task;
    var prefab = task.GetResult();
}
```

---

### UWE.PrefabDatabase.GetPrefabAsync

```csharp
public static IPrefabRequest GetPrefabAsync(string classId)
```

Instead of a TechType, this method requires a Class ID. While some prefabs have TechTypes, almost every prefab has a Class IDs. Of course, there are exceptions.
Certain visual effects and projectiles do not have Class IDs. Special "scene objects" such as the player, Aurora and Cyclops are also unable to be spawned in this way.

The downside of this method is that the Class IDs are not part of the game's assemblies like TechTypes are. This means you must find the Class ID yourself and the
compiler will not autocomplete them for you.

For your convenience, a list of all Class IDs can be found [here](https://github.com/SubnauticaModding/Nautilus/blob/master/Nautilus/Documentation/resources/SN1-PrefabPaths.json).
Do note that a list has not been made for Below Zero yet.

Example code that spawns a Peeper behind the player:
```csharp
using UWE;
// ...
private static IEnumerator SpawnPeeper()
{
    // Fetch the prefab (3fcd548b-781f-46ba-b076-7412608deeef is the Class ID of the Peeper):
    IPrefabRequest task = UWE.PrefabDatabase.GetPrefabAsync("3fcd548b-781f-46ba-b076-7412608deeef");
    // Wait for the prefab task to complete:
    yield return task;
    // Get the prefab:
    task.TryGetPrefab(out GameObject prefab);

    // Instantiate the prefab with a random rotation 2 meters behind the player camera:
    Instantiate(prefab, MainCamera.camera.transform.position - (MainCamera.camera.transform.forward * 2), Random.rotation);
}
```

Bare minimum code with inferred typing:
```csharp
private static IEnumerator SpawnPeeper()
{
    var task = UWE.PrefabDatabase.GetPrefabAsync("3fcd548b-781f-46ba-b076-7412608deeef");
    yield return task;
    task.TryGetPrefab(out var prefab);
}
```

---

### UWE.PrefabDatabase.GetPrefabForFilenameAsync

```csharp
public static IPrefabRequest GetPrefabForFilenameAsync(string filename)
```

This method is similar to `PrefabDatabase.GetPrefabAsync` but takes a file path as opposed to a Class ID. If you are familiar with `Resources.Load` from before the
Living Large update, the paths here are *very* similar to the ones used in that method.

The [Class ID list](https://github.com/SubnauticaModding/Nautilus/blob/master/Nautilus/Documentation/resources/SN1-PrefabPaths.json)
also contains the file path of every prefab on the right side. Make sure to include the `.prefab` extension and exclude the `Assets/AddressableResources/` prefix.

Example code that spawns a Peeper above the player:
```csharp
using UWE;
// ...
private static IEnumerator SpawnPeeper()
{
    // Fetch the prefab:
    IPrefabRequest task = UWE.PrefabDatabase.GetPrefabForFilenameAsync("WorldEntities/Creatures/Peeper.prefab");
    // Wait for the prefab task to complete:
    yield return task;
    // Get the prefab:
    task.TryGetPrefab(out GameObject prefab);

    // Instantiate the prefab with a random rotation 2 meters above the player camera:
    Instantiate(prefab, MainCamera.camera.transform.position + (MainCamera.camera.transform.up * 2), Random.rotation);
}
```

Bare minimum code with inferred typing:
```csharp
private static IEnumerator SpawnPeeper()
{
    var task = UWE.PrefabDatabase.GetPrefabForFilenameAsync("WorldEntities/Creatures/Peeper.prefab");
    yield return task;
    task.TryGetPrefab(out var prefab);
}
```

---

## When to use each method

For most purposes, `CraftData.GetPrefabForTechTypeAsync(TechType)` can be the only thing you use. TechTypes are convenient, and most functional prefabs have one. There
is no reason not to use this unless the prefab you need is lacking a TechType.

`PrefabDatabase.GetPrefabAsync` and `PrefabDatabase.GetPrefabForFilenameAsync` can be used interchangeably. The former generally takes up less space in terms of characters.
However the paths in the latter are definitely more readable than Class IDs. Whichever you want to use is up to you.