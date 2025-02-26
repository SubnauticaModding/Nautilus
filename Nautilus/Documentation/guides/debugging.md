# Setting up Debugging

This guide will show you how to enable advanced tools to make it easier to diagnose errors in your mods.

You should at least be somewhat familiar with modding for this tutorial. It is assumed that you have a development
environment set up, know basic C#, and have developed your first mod.

> [!NOTE]
> The contents of this guide are not specific to Subnautica and will work for any Unity game as long as a
> version of BepInEx that uses Unity Doorstop 4 is installed (v5.4.23.2+).


## Features and Limitations

Using debug information generated during build, you can:

- Get exact line numbers in exceptions that show up in the log file.
- Attach a debugger to the game.
- Use standard debugging features like breakpoints, executing code one line at a time, and watching what's happening in
your variables.

However, you cannot:

- Use the above features for code for which you do not have a .pdb file, i.e. any code you did not write yourself.
- In particular, you cannot use this to debug code that is part of Subnautica.


## Generating Debug Symbols

The first step is to make your IDE generate debugging information while it builds your mod. Every IDE can do this,
although the settings may be located in different places.

### JetBrains Rider

- In the file explorer window on the left, right-click on your project and choose `Properties...`.
- Navigate to the `Debug` configuration.
- Tick `Debug Symbols` and set the debug type to `Portable`.

Your IDE now generates a {projectname}.pdb file on every build. It is important that, whenever you build a new version
of your mod and drop it into your Subnautica plugins folder to test, you also remember to place the newest .pdb file
right next to your .dll.
Otherwise, the debug information will not line up with your actual code and the debugger will get confused.

> [!TIP]
> To ensure your files are always in sync, use MSBuild's post-build actions to automatically deploy both the .dll and
> the .pdb to your game folder.


## Enabling the Debugging Server

Since version 5.4.23.2, BepInEx ships with Unity Doorstop 4, which provides a built-in debugging server. Navigate to
your Subnautica install directory (the same place as `Subnautica.exe`), where you should find a file called `doorstop_config.ini`. If you do not have this
file, ensure you have the correct version of BepInEx installed.

In `doorstop_config.ini`, do the following:

- Set `debug_enabled` to `true`.
- Take note of the address in `debug_address` for later.
- Optionally, set `debug_suspend` to true. This will force the game to freeze until you attach a debugger. This is useful
if you need to debug code that executes very early on, before the game even loads into the main menu. If unsure, leave
this off.

With the debugging server enabled and the information from your .pdb files in the right place,
any exceptions that happen in your mod will now display the correct line numbers.


## Connecting a Debugger to the Debugging Server

### Using dnSpy

If you use dnSpy, you can use the `Debug > Start Debugging > Debug engine > Unity` option.

### Using Rider's built-in Debugger

- In the top right of your screen next to the green debug icon, click on the More Actions dropdown and `Attach to Unity Process...`.
- Click `Add Player Address Manually`.
- Give the connection a name and fill in the address you noted down earlier from the doorstop config. By default, the
hostname will be 127.0.0.1 and the port 10000.
- Save the connection.

From now on, you can choose the debugging configuration and press the green bug icon to start the debugger and connect
to Subnautica at any time. Note that you must start Subnautica *first* and then connect the debugger *second*. Start
setting breakpoints, explore your code in real time, and see what your variables are really doing.

Learn more about Rider's debugger and what it can do [in their docs](https://www.jetbrains.com/help/rider/Debugging_Code.html).