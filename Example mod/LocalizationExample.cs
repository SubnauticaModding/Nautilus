using System.Collections.Generic;
using BepInEx;
using SMLHelper.Assets;
using SMLHelper.Assets.PrefabTemplates;
using SMLHelper.Handlers;

namespace SMLHelper.Examples;

[BepInPlugin("com.snmodding.smlhelper.localizaion", "SMLHelper Localization Example Mod", PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.smlhelper")]
public class LocalizationExample : BaseUnityPlugin
{
    /*
     * Here we have a dictionary that contains language keys and values for certain stuff.
     * The language key for the display name of TechTypes is "{enumName}".
     * Since our Titanium Clone's tech type is "TitaniumClone", this is what we will use for the display name.
     *
     * Similarly, for tooltips, the language key is "Tooltip_{enumName}", so for our Titanium Clone, it would be "Tooltip_TitaniumClone".
     */
    private Dictionary<string, string> _languageEntriesEng = new()
    {
        { "TitaniumClone", "Titanium Clone" }, { "Tooltip_TitaniumClone", "Titanium clone that makes me go yes." }
    };
    
    /*
     * Here we have a dictionary that translates the language entries above to Spanish.
     * Keep in mind that the language keys are the same for every language.
     */
    private Dictionary<string, string> _languageEntriesEsp = new()
    {
        { "TitaniumClone", "Clon de Titanio" }, { "Tooltip_TitaniumClone", "Clon de Titanio que me hace decir que sí" }
    };

    private void Awake()
    {
#if LOCALIZATION_FOLDER
        /*
         * Registers a folder as localization folder.
         * This folder must contain json files that are named after the language they translate.
         * For example, English translation must be named English.json and Spanish translation must be named Spanish.json.
         * SML expects this folder to be located in the mod folder at ModName/Localization by default.
         */
        LanguageHandler.RegisterLocalizationFolder();
#else
        // Register our English language entries to the English language
        LanguageHandler.RegisterLocalization("English", _languageEntriesEng);
        
        // Register our Spanish language entries to the Spanish language
        LanguageHandler.RegisterLocalization("Spanish", _languageEntriesEsp);
#endif

        /*
         * Create a CustomPrefab instance for our Titanium Clone. Must be set to null or empty if you registered language entries
         * for them earlier like we did.
         */ 
        var titaniumClone = new CustomPrefab("TitaniumClone", null, null, SpriteManager.Get(TechType.Titanium));
        
        // Set our prefab's game object to a clone of the Titanium prefab
        titaniumClone.SetGameObject(new CloneTemplate(titaniumClone.Info, TechType.Titanium));
        
        /*
         * Register our Titanium Clone to the game.
         * After this point, do not edit the prefab or modify gadgets as they will not be applied.
         */
        titaniumClone.Register();
    }
}