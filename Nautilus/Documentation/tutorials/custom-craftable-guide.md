# Adding a custom item using Asset Bundles

This step-by-step tutorial covers the basics of setting up a Unity project, building assets, loading them, and making an item with your custom model. This guide does not utilize Thunderkit, which is an extremely powerful tool that delegates a lot of the work we do here into the Unity editor.

By the end of this page you will have created a custom material with a unique model that can be unlocked & crafted in the Fabricator.

IMPORTANT! see [this page](asset-bundle-creation.md) first for a general explanation on asset bundles

## Prerequisites
- An existing mod project that uses Nautilus.
- An installation of the correct Unity Engine version (2019.4.36).
- A Unity project with the Asset Bundle Browser installed

## 1. Tutorial preparation
For this specific tutorial, which involves adding a 3D model into the game, I will be using a basic screw model. A zip file containing the model & textures can be downloaded [here]. Make sure you have some sort of model that you are ready to use.

IMPORTANT! Please familiarize yourself with [the rules on importing assets](…), especially if you are unfamiliar with the engine.
## 2. Unity project
Either create or open an existing Unity 2019.4.36 project. This project does not have to be empty and can contain assets for multiple mods if desired.

## 3. Importing assets into Unity
In the Unity project, create a new folder for your item and give it the same name as your item.

Now, drag the model and any other related assets (textures, etc.) into this newly created folder. Unity does have built-in drag and drop behavior, which I recommend utilizing.

## Including the asset in your bundle
This step is crucial for getting Asset Bundles to work. Your asset will not be exported unless you follow these steps to get it to work.

Now that the asset bundle has been chosen, you can just use the drop down menu to find it.