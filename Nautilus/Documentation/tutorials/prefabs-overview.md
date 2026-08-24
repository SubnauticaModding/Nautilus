# Prefab Basics

Understanding and creating prefabs is essential for any Subnautica mod that adds new content. This page will provide basic information on how prefabs are used in
Subnautica and why they are important.

## What are prefabs?

At the most basic level, prefabs are simply stored [GameObjects](https://docs.unity3d.com/Manual/class-GameObject.html) that can be instantiated into a scene. This is a Unity
Engine concept, and it applies for Subnautica modding as well.

In Subnautica, prefabs encompass essentially anything that exists in the world, aside from terrain, the player, and a few other exceptions. This includes anything from
creatures to base modules to fragments to unnamed rock formations and debris. They are registered and managed by the `UWE.PrefabDatabase` class. Nautilus allows you to indirectly
register your own custom prefabs into this system.

Subnautica prefabs consist of the following:
- **Class ID** (defined in the GameObject's `PrefabIdentifier` component)
- **TechType** (this is actually optional, and is defined in the GameObject's `TechTag` component)
- The actual **GameObject** that is spawned.

## Differences between Class ID and TechType

These are two distinct identification systems used by the game and should not be confused.

The Class ID is the only thing that is *required* for a prefab. A mod that adds new commands such as [DebugHelper](https://www.nexusmods.com/subnautica/mods/1560) is required
to actually spawn a prefab by its Class ID. All base-game Class IDs are 36-character-long GUIDs. When you create a prefab with Nautilus, the Class ID will use the TechType name by default. You can find a list of all Class IDs [here](https://github.com/SubnauticaModding/Nautilus/blob/master/Nautilus/Documentation/resources/SN1-PrefabPaths.json).

TechTypes are more accessible and readable. You may recognize them from the `spawn` command. Unlike Class IDs, which aren't readily available, TechTypes are all
listed under the `TechType` enum in the game's codebase. There are thousands of prefabs that cannot be spawned with the spawn command because they don't have
a TechType assigned. Having a TechType is also required for **crafting recipes**, **blueprints** and **inventory items**.

Most importantly: **there can be multiple prefabs with the same TechType, but every prefab has a different Class ID.** This is why you can have multiple
fragments with different models that unlock the same blueprint.

## Essential components

There are a few components that are required or heavily recommended for prefabs. The [PrefabUtils.AddBasicComponents](xref:Nautilus.Utility.PrefabUtils) method provided by
Nautilus handles most of this for you. You can also set up these components in the Unity Editor if using Thunderkit.

- PrefabIdentifier: The only component that is truly required for spawnable prefabs. Holds the Class ID of your prefab to be used when saving and loading regions.
- TechTag: Holds the TechType of your prefab. This is not required for prefabs, but it is generally used for convenience.
- LargeWorldEntity: Needed for a prefab to save and load properly as an individual world entity. You will also want to set its `cellLevel` field to change its loading distance.
- SkyApplier: Required for applying proper shading to a model.
- Pickupable: Needed for inventory items to be pickupable.
