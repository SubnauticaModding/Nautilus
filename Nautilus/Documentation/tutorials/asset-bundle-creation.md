# Creating and Using Asset Bundles

This page provides a general overview on Asset Bundles and suggested practices when using them. For a more specific tutorial on adding items with custom assets, please see [this page](…) instead.

## Prerequisites
- An installation of the correct Unity Engine version (2019.4.36).

## 1. Setting up Unity
Through Unity Hub, create a new Unity Project with a version of 2019.4.36. If you already have a project with the correct version you may use that, but make sure you stay organized, because otherwise you’ll regret it later (I know through personal experience).

## Asset Bundle Browser
In your Unity project, it is required that the Asset Bundle Browser (or a similar tool for packaging bundles) is installed.

At the top of the editor window, navigate to Window/Package Manager. From here you want to search for the “AssetBundleBrowser” and install it. This process should be quick. If installed correctly you can open it through “…”.

## Rules on importing assets
FBX and OBJ are the recommended formats for models, but a list of all supported file types can be found [here](https://docs.unity3d.com/2020.1/Documentation/Manual/3D-formats.html).
“Unity supports most common image file types, such as BMP, TIF, TGA, JPG, and PSD. If you save your layered Photoshop (.psd) files into your Assets folder, Unity imports them as flattened images” ([Unity documentation](https://docs.unity3d.com/2019.2/Documentation/Manual/AssetTypes.html )). However you will likely only use PNG and/or TGA for most purposes.

Some images need to be imported as Sprites. You can do this by (…). Most icons and UI elements (besides databank entries) are required to be sprites.

## Organizing your assets
While not required, I would recommend organizing all of your assets into folders. Do note that the folder structure does not affect any of the code of your mod, so where you put your assets is entirely up to you. In the project window, create a folder in the Assets folder and name it after your mod.

## Loading the assets in your mod
Actually getting a reference to the asset in your mod file is very simple, however you must first load the asset bundle.

For an asset bundle to be loaded and distributed probably, it must be placed in the mod’s folder. It is STRONGLY RECOMMENDED that you create an Assets folder and place your bundle in there. This guide will assume you have done that.

There are different ways to load an asset bundle but AssetBundleLoadingUtils.LoadFromAssetsFolder() is the most simple. Simply pass in the name of your asset bundle’s file.