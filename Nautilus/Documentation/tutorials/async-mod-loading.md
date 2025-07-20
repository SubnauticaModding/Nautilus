# Mod loading during the loading screen

The more complex your project, the more loading and/or setup it is likely to have to do. It is common for mod authors
to do all their initial loading in their plugin's `Awake()` method. On its own, this is not a problem. However, when
a user installs many mods, all of which do all their loading at the very beginning, it can look like the game has
crashed on a black screen before the main menu even loads. Imagine if, instead, all this loading happened at a time
where the user knows that loading is *going* to happen, and can even see the current progress. Imagine if mods could
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
then a second task which requires access to save data ([see below](#4-savedatacache-uses-waitscreentask)).

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
    WaitScreenHandler.RegisterLateLoadTask("ExampleMod", ExpandPodInventory)
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
        WaitScreenHandler.RegisterEarlyLoadTask("ExampleMod", _ => DoSaveDependentThing(saveData))
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



## File loading with WaitScreenTasks

show async usage