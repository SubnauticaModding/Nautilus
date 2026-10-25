# Nautilus: Subnautica Modding API

<!---------BANNER START--------->
![banner](https://user-images.githubusercontent.com/71298690/233505405-e89fbc70-31c9-45a2-bb31-64e1f498d4a7.png)
<!---------BADGES END--------->



<!---- Badges are from: https://shields.io/ ---->
<!---------BADGES START--------->
[![NuGet](https://img.shields.io/nuget/vpre/Subnautica.Nautilus)](https://www.nuget.org/packages/Subnautica.Nautilus)
[![Discord](https://img.shields.io/discord/324207629784186882?logo=discord&logoColor=white)](https://discord.gg/UpWuWwq)
[![GitHub contributors](https://img.shields.io/github/contributors/SubnauticaModding/Nautilus)](https://github.com/SubnauticaModding/Nautilus/graphs/contributors)
[![License](https://img.shields.io/github/license/SubnauticaModding/Nautilus)](https://github.com/SubnauticaModding/Nautilus/blob/master/LICENSE.md)
<!---------BADGES END--------->


<br>


<!---- To get the links for section names that have emojis: go to 'Preview' > hover over the section text > hit the link icon that shows, and use that url ---->
<!---------TABLE OF CONTENTS START--------->  
## Contents
- [About](#-about)
- [Installation](#-installation)
- [Resources / links](#-links)
- [Contributors](#-contributors)
- [Contributing](#-contributing)
<!---------TABLE OF CONTENTS END--------->


<br>


<!---------ABOUT SECTION START--------->
## About 

Nautilus is the __core modding library__ for Subnautica and Subnautica: Below Zero. It offers many simple helpers and utilities for interacting with the game, allowing for easy implementation of content.

Some notable features and techniques that Nautilus allows for are:
- Adding new items, decorations, base modules, tools, and other prefabs
- Regstering custom biomes, spawning objects in the world
- In-game mod configuration in the Subnautica style
- Adding sounds and replacing existing audio
- Registering new story events: voice logs, PDA entries, story triggers, etc.
- Setting up materials and shaders as accurately as possible
- Registration utilities to help with making larger mods scaleable

This project is a continuation of [SMLHelper](https://github.com/SubnauticaModding/Nautilus/tree/sml-2) with an improved codebase and maintainability. Nautilus offers all the features SML used to offer with a more robust implementation. Additionally, Nautilus took a different route for managing handlers to address many bugs and timing issues that were persistent in SML.  

We hope to keep improving the modding experience in Subnautica to allow developers to create mods more easily and eliminate the implementation concerns in the mod-making process. For more information on Nautilus and its capabilities, please refer to our [documentation](https://subnauticamodding.github.io/Nautilus).

### ⚠️ Nautilus will only receive continued support for the latest version of Subnautica
- #### If you are using the latest version of Subnautica (post [2025 patch](https://unknownworlds.com/en/news/subnautica-2025-patch)) or Below Zero, use [the latest release of Nautilus](https://github.com/SubnauticaModding/Nautilus/releases)
- #### If you are using the `march_2023` Steam branch, use [Nautilus pre.41](https://github.com/SubnauticaModding/Nautilus/releases/tag/1.0.0-pre.41)
- #### If you are using the `legacy` Steam branch, use [SMLHelper](https://www.nexusmods.com/subnautica/mods/113)

<!---------ABOUT SECTION END--------->


<br>


<!---------INSTALLATION SECTION START--------->
## Installation
1. Download the [Subnautica BepInEx Pack](https://www.nexusmods.com/subnautica/mods/1108). 
2. Extract/unzip the BepInEx x64 zip file to your game folder
   <!---- FYI these dropdowns have a really weird spacing issue, the line breaks are mandatory to keep formatting! ---->
   - <details><summary>Where is my Subnautica/game folder? (Click to expand)</summary>

      - Steam: &ensp; &ensp; &ensp; &ensp; <code>C:\Program Files (x86)\Steam\steamapps\common\Subnautica</code> <br>
      - Epic Games: &ensp; <code>C:\Program Files\Epic Games\Subnautica</code> <br>
      - Xbox PC: &ensp; &ensp; &ensp; <code>C:\XboxGames\Subnautica\Content</code> <br>
      ###### Note: the above paths are the default locations, yours may vary
    </details>
    
    - <details><summary>How do I extract the zip file? (Click to expand)</summary>

      - Extracting/unzipping a zip file is as simple as right clicking it, and selecting the `Extract here` prompt. We highly recommend the use of a zipping tool besides the Windows default one, such as [WinRAR](https://www.rarlab.com/download.htm), or [7-Zip](https://7-zip.org/download.html)
    </details>
3. Download Nautilus from [Nexus Mods](https://www.nexusmods.com/subnautica/mods/1262) or [GitHub Releases](https://github.com/SubnauticaModding/Nautilus/releases)
4. Extract/unzip the Nautilus zip file to your `Subnautica\BepInEx` folder
5. Confirm your `Subnautica\BepInEx\plugins` now looks like this:
   ###### If it doesn't look like this, try figuring out where your Nautilus folder has gone, and move it into the plugins folder
   ![Screenshot of plugins folder](https://i.imgur.com/HD6QD8g.png)
7. Launch the game, and enjoy! *(a good way to confirm Nautilus has loaded is to check the game Options for a `Mods` tab)*

#### If you have any trouble installing the mod, please join our [Discord server](https://discord.gg/UpWuWwq), and explain whats going on in a help and support channel.
<!---------INSTALLATION SECTION END--------->


<br>


<!---------LINKS SECTION START--------->
## Links
[Nautilus Documentation](https://subnauticamodding.github.io/Nautilus) ([source](https://github.com/SubnauticaModding/Nautilus/tree/docs/Nautilus/Documentation))

[Development Setup Guide](https://subnauticamodding.github.io/Nautilus/guides/dev-setup.html)

[Updating to Nautilus from SMLHelper](https://subnauticamodding.github.io/Nautilus/guides/sml2-to-nautilus.html)

[Subnautica Modding Discord Server](https://discord.gg/UpWuWwq)
<!---------LINKS SECTION END--------->


<br>


<!---------CONTRIBUTORS SECTION START--------->
## Contributors
To everyone who has taken the time to contribute to Nautilus and/or SMLHelper, we cannot thank you enough!

[Table of contributors](https://github.com/SubnauticaModding/Nautilus/blob/master/AUTHORS.md) ❤️

<!---------CONTRIBUTORS SECTION END--------->


<br>


<!---------CONTRIBUTING SECTION START--------->
## Contributing
We welcome all kinds of contributions; simply ensure you have read through our [Contribution Guidelines](CONTRIBUTING.md) before submitting a pull request.

If you are not capable of writing code, but have feature ideas or bug reports, feel free to [submit an issue](https://github.com/SubnauticaModding/Nautilus/issues/new) instead.

<!---------CONTRIBUTING SECTION END--------->



<!--                                      -->
