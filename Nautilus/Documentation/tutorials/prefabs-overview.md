# Prefab Basics

Creating and utilizing prefabs is essential for any Subnautica mod that adds new content. This page will provide basic information on how prefabs are used in
Subnautica and why they are important.

## What are prefabs?

At the most basic levels, prefabs are simply [GameObjects](https://docs.unity3d.com/Manual/class-GameObject.html) that are instantiated into a scene. This is a Unity
Engine concept, and it applies for Subnautica modding as well.

In Subnautica, prefabs are essentially anything that exists in the world outside of terrain, the player, and a few other exceptions. This encompasses anything from
creatures to base modules to unnamed rock formations and debris props. They are registered and managed by the `UWE.PrefabDatabase` class. Nautilus allows you to indirectly
register your own custom prefabs into this system.

Subnautica prefabs consist of the following:
- **Class ID** (defined in the GameObject's `PrefabIdentifier` component)
- **TechType** (this is actually optional, and is defined in the GameObject's `TechTag` component)
- The actual **GameObject** that is spawned.

## Differences between Class ID and TechType

These are two distinct identification systems used by the game and should not be confused.

The Class ID is the only thing that is *required* for a prefab. A mod that adds new commands such as [DebugHelper](https://www.submodica.xyz/mods/sn1/248) is required
to actually spawn a prefab by its Class ID. Generally, Class IDs are 36 character strings, but when you create a prefab with Nautilus the Class ID will actually
match its TechType. You can find a list of all Class IDs [here](https://github.com/SubnauticaModding/Nautilus/blob/master/Nautilus/Documentation/resources/SN1-PrefabPaths.json).

TechTypes are more accessible and readable. You may recognize them from the `spawn` command. Unlike Class IDs, which live in an obfuscated file, TechTypes are all
listed under the `TechType` enum in the game's code. There are potentially thousands of prefabs that cannot be spawned with the spawn command because they don't have
a TechType assigned. These are also required for **crafting**, **blueprints** and **inventory items**.

The most important part? **There can be multiple prefabs with the same TechType, but every prefab has a different Class ID.** This is why you can have multiple
fragments with different models that unlock the same blueprint.

## Essential components

There are a few components that are required or heavily recommended for prefabs. The [PrefabUtils.AddBasicComponents](xref:Nautilus.Utility.PrefabUtils) method provided by
Nautilus handles most of this for you, besides Pickupable.

- PrefabIdentifier: The only one that is *truly* required for something to be spawned. Holds the Class ID of your prefab so it can be saved and loaded.
- TechTag: Holds the TechType of your prefab. This is not required for prefabs, but is needed for *many* cases. It's nice because it lets you use the spawn command.
- LargeWorldEntity: Heavily recommended. Generally needed for a prefab to save properly. You also will want to set its `cellLevel` field to change its loading distance.
- SkyApplier: Required for proper shading whenever you have a custom model.
- Pickupable: Not required unless you are making an inventory item.