# Mod loading during the loading screen

The more complex your project, the more loading and/or setup it is likely to have to do. It is common for mod authors
to do all their initial loading in their plugin's `Awake()` method. On its own, this is not a problem. However, when
a user installs many mods, all of which do all their loading at the very beginning, it can look like the game has
crashed on a black screen before the main menu even loads. Imagine if, instead, all this loading happened at a time
when the user knows that loading is *going* to happen, and can even see the current progress. Imagine if mods could
do all their loading as part of the game's actual loading screen.

Enter the [WaitScreenTask](xref:Nautilus.Handlers.WaitScreenHandler.WaitScreenTask).

WaitScreenTasks can be used for many different things, but consider using them if:
- Your mod needs to load files (e.g. asset bundles, JSON files)
- Your mod needs to do a considerable amount of computing before the game loads
- Your mod requires something done before the game loads that it cannot function without (i.e. you are worried about 
  race conditions)
- You want to make a quick adjustment moments before the game has finished loading

## Understanding WaitScreenTasks

You should know:

### 1. The game must wait

Nautilus inserts a few extra stages into the loading screen. During each of these stages, any registered WaitScreenTasks
are completed one by one, each only proceeding when the previous one has finished. **The game cannot exit the loading
screen until every single task is done.**

### 2. Registration order

The tasks are always completed in the order they were registered. This allows you to set up chains of tasks that depend
on the result of the previous one. For example, you could create one task that loads an asset bundle, and then a second
task that uses said bundle to do something else, or register a [SaveDataCache](xref:Nautilus.Json.SaveDataCache) and
then a second task which requires access to save data ([see below](#example-using-a-savedatacache)).

### 3. Three Loading Stages

During the loading screen, there are three stages you can register a task for: `Early`, `Main`, and `Late`. They differ
only in their execution timing.

The `Early` stage executes almost immediately after the player finishes pressing the button in the main menu and the 
loading screen appears. Crucially, the game is still technically in the main menu. Any GameObjects created during this 
stage will be destroyed during the scene transition to the main game.
[SaveDataCaches](xref:Nautilus.Json.SaveDataCache) are loaded here.

The `Main` stage executes after the scene has switched to the in-game Main scene. This is the ideal time to initialise
custom GameObjects or perform any other work that doesn't require special timing. Note that even though the main scene
has loaded, it is prevented from performing all its usual setup until this mod loading stage is finished. As a result,
commonly used singletons like `Player.main` or `EscapePod.main` do not exist and none of the game's objects have had
a chance to `Awake()` yet.

The `Late` stage executes just before the loading screen would usually end. All vanilla GameObjects have run
`Awake()` and finished their setup. Any tasks that run here can make last-minute changes to the game without worrying
about the player gaining control before everything is ready.

### 4. `SaveDataCache` uses WaitScreenTask

[SaveDataCaches](xref:Nautilus.Json.SaveDataCache) also use the `WaitScreenTask` system. Registering a cache with
the [SaveDataHandler](xref:Nautilus.Handlers.SaveDataHandler) implicitly registers a task during the `Early` mod
loading stage which will load the cache. Any tasks registered after the cache can always rely on save data being ready
for use.

## Simple example

This code demonstrates how to register a simple task to make a last-second change to the game. The example increases
the life pod's inventory size just before the game is ready to play.

```csharp
private void Awake()
{
    ... // Other plugin setup code
    
    // First, register your task, for example during your plugin Awake().
    // We choose the Late stage for this task because we need access to the life pod.
    WaitScreenHandler.RegisterLateLoadTask("ExampleMod", ExpandPodInventory);
}

// This function will be called by Nautilus as part of the task during the loading screen.
private void ExpandPodInventory(WaitScreenHandler.WaitScreenTask task)
{
    // Change the life pod's inventory to be 10x10.
    EscapePod.main.storageContainer.Resize(10, 10);
}
```

### Example using a SaveDataCache

This example demonstrates how to use save data from a [SaveDataCache](xref:Nautilus.Json.SaveDataCache) in a task.

```csharp
public class ExampleSaveData : SaveDataCache
{
    public bool ExampleThing;
}

public class ExampleMod : BaseUnityPlugin
{
    private void Awake()
    {
        ... // Other plugin setup code
        
        // First, register your save data.
        var saveData = SaveDataHandler.RegisterSaveDataCache<ExampleSaveData>();
        
        // Despite choosing the Early load stage for this task, we can always be sure that the save data is ready
        // because we registered the save data first.
        // Our function expects to be given ExampleSaveData, but Nautilus will always try to give it a WaitScreenTask.
        // Here, we use a lambda expression to discard the task and pass the save data instead.
        WaitScreenHandler.RegisterEarlyLoadTask("ExampleMod", _ => DoSaveDependentThing(saveData));
    }
    
    private void DoSaveDependentThing(ExampleSaveData saveData)
    {
        // Do something that is different from save to save.
        if (saveData.ExampleThing)
        {
            ...
        }
    }
}
```

If the above example confuses you, you may want to read up on [C# lambda expressions](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/lambda-expressions).

## Reporting Status

By default, while a task is executing Nautilus will simply show the task number and your mod's name. You can add
more context for the player in one of two ways: Either by adding a decription while you register your
task or by editing the status text during task execution.

### Adding a description

The description will persist through the entire duration of the task.

```csharp
private void Awake()
{
    ...
    
    // This is the same escape pod inventory example as above, but this time we add more context.
    WaitScreenHandler.RegisterLateLoadTask("ExampleMod", ExpandPodInventory, "Expanding Inventory!");
}

// Other code
...
```

### Updating status during a task

You can set the status text directly through the [WaitScreenTask](xref:Nautilus.Handlers.WaitScreenHandler.WaitScreenTask) argument you are given by Nautilus. Note that
this only makes sense for longer tasks that take a long time to complete. Your task also *must* be an async task,
otherwise the loading screen never gets to run the code that shows any changes you make to the status.

```csharp
private void Awake()
{
    ...
        
    WaitScreenHandler.RegisterAsyncLoadTask("ExampleMod", PerformSetupAsync);
}

private IEnumerator PerformSetupAsync(WaitScreenHandler.WaitScreenTask task)
{
    // Set the new status.
    task.Status = "Loading important assets...";
    // The loading screen will show the new status the next time you yield.
    yield return CoroutineThatLoadsAssetBundles();
    
    // Set the status again to show the task has progressed.
    task.Status = "Modifying game prefabs...";
    // This advances the game by one frame and lets the loading screen update.
    yield return null;
    // Run other code
    ...
}
```

You can also mix and match the two by adding a description during registration and then editing the status text later.

## File loading with WaitScreenTasks

WaitScreenTasks force the loading screen to wait until they're complete, which makes them ideal for loading files.
This way, you do not have to worry about whether the game is already way ahead of you by the time your mod has finally
finished prep work. Nautilus ensures that it isn't.

### Loading AssetBundles

A good way to go about loading asset bundles is to use an async task to simply wait until the bundle is loaded.
This way, you avoid loading the bundle again on subsequent loads.

```csharp
private static AssetBundle _assetBundle;

private void Awake()
{
    ...
        
    WaitScreenHandler.RegisterAsyncLoadTask("ExampleMod", LoadAssetsAsync);
}

private IEnumerator LoadAssetsAsync(WaitScreenHandler.WaitScreenTask task)
{
    task.Status = "Loading asset bundle...";
    // By caching the asset bundle we avoid loading it again later and speed up subsequent loads.
    if (_assetBundle == null)
    {
        // Load an asset bundle from the mod folder.
        request = AssetBundle.LoadFromFileAsync("exampleAssetBundle");
        // Wait until the bundle has finished loading.
        yield return request;
        // Cache the bundle for later.
        _assetBundle = request.assetBundle;
    }
    
    task.Status = "Loading data from bundle...";
    // Load a GameObject for a custom plant from the asset bundle.
    var objRequest = _assetBundle.LoadAssetAsync<GameObject>("CustomPlant1");
    // Wait until the plant has finished loading.
    yield return objRequest;
    var gameObject = objRequest.asset as GameObject;
    // Do something with the GameObject
    ...
}
```

### Loading non-bundle files

You may need to load files that are not asset bundles for your mod to work properly. For example, mods which
edit a lot of recipes often prefer to load their recipe changes from a JSON file rather than hardcoding it all.

This can get a little more complicated because we need to convert from [C# Async](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/)
to Unity [Coroutines](https://docs.unity3d.com/2019.4/Documentation/Manual/Coroutines.html).

```csharp
private void Awake()
{
    ...
        
    // We use an async WaitScreenTask for file loading.
    WaitScreenHandler.RegisterAsyncLoadTask("ExampleMod", LoadFileAsync);
}

// This method is a coroutine wrapped around the method that *actually* loads the file.
private IEnumerator LoadFileAsync(WaitScreenHandler.WaitScreenTask task)
{
    task.Status = "Loading very important file...";
    
    // Start loading the file.
    Task<string> loadOperation = LoadImportantFileAsync();
    // Wait until the file has finished loading.
    yield return new WaitUntil(() => loadOperation.IsCompleted);
    // Get the contents of the file.
    string contents = loadOperation.Result;
    
    // Do something with the file contents.
    ...
}

// This method is what actually loads the file. Unfortunately, Unity has poor async support
// so it cannot be used directly.
private async Task<string> LoadImportantFileAsync()
{
    // Always open files with a 'using' statement so they don't stay opened indefinitely.
    using StreamReader reader = new StreamReader(File.OpenRead("recipeChanges.json"));
    // Read everything and return it when done.
    string fileContents = await reader.ReadToEndAsync();
    return fileContents;
}
```

### Loading multiple files in one WaitScreenTask

If you have multiple non-bundle files that each need loading you could follow the steps above for each one
of these files individually, which would load one file, wait until it's done, then load the next, etc.

Alternatively, you can start loading every file at once and simply wait until all of them have finished.

```csharp
// Define the filenames of each file that needs to be loaded.
private string[] _fileNames = new[] { "recipeChanges.json", "fragmentChanges.json", "databankChanges.json" };

private void Awake()
{
    ...
        
    // We use an async WaitScreenTask for file loading.
    WaitScreenHandler.RegisterAsyncLoadTask("ExampleMod", LoadFilesAsync);
}

// This method is a coroutine wrapped around the method that *actually* loads the file.
private IEnumerator LoadFilesAsync(WaitScreenHandler.WaitScreenTask task)
{
    task.Status = "Loading very important files...";
    
    // Keep a list of all active async Tasks.
    var fileTasks = new List<Task>();
    
    // Start loading each file and keep track of its Task.
    foreach (string fileName of _fileNames)
    {
        fileTasks.Add(LoadImportantFileAsync(fileName));
    }
    
    // Wait until all files have finished loading.
    yield return new WaitUntil(fileTasks.TrueForAll(fileTask => fileTask.IsCompleted));
    
    // Do something with the file contents.
    ...
}

// This method is what actually loads the file. Unfortunately, Unity has poor async support
// so it cannot be used directly.
private async Task<string> LoadImportantFileAsync(string filePath)
{
    // This time, we pass in the file path as an argument for reusability.
    using StreamReader reader = new StreamReader(File.OpenRead(filePath));
    // Read everything and return it when done.
    string fileContents = await reader.ReadToEndAsync();
    return fileContents;
}
```
