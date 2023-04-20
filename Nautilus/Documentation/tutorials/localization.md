# Localization

In Subnautica, localization is a key-value string dataset where the key represents a unique identifier that is the same on all languages, and the value represents the translation in a language.  

Nautilus provides a few different approaches to go about adding localization in the [LanguageHandler](xref:Nautilus.Handlers.LanguageHandler) class.

## Json Files Localization
The most common method of adding localization in game development is via json files where the json file name represents the language name (I.E: `English.json` for English). 

The json files contain a key-value pair where the key is the language key and the value is the translation.  


### Examples
The following examples demonstrate the usage of json-file-based localizations.

Json files:
```json
// English.json
{
  "TitaniumClone": "Titanium Clone",
  "Tooltip_TitaniumClone": "Titanium clone that makes me go yes."
}
```
```json
// Spanish.json
{
  "TitaniumClone": "Clon de Titanio",
  "Tooltip_TitaniumClone": "Clon de Titanio que me hace decir que sí"
}
```

To register json-file-based localizations, all you will have to call is one line of code:
```csharp
LanguageHandler.RegisterLocalizationFolder();
```

> [!NOTE]
> By default, Nautilus expects these json files to be located in the {modFolder}/Localization folder. 

The following example registers the `Translations` folder as the localization folder:
```csharp
LanguageHandler.RegisterLocalizationFolder("Translations");
```

## Dictionary Localization
Nautilus also offers to register string key-value dataset as localization. 

### Examples
The following examples demonstrate the usage of dictionary-based localization.
```csharp
Dictionary<string, string> _languageEntriesEng = new()
{
    { "TitaniumClone", "Titanium Clone" }, { "Tooltip_TitaniumClone", "Titanium clone that makes me go yes." }
};

Dictionary<string, string> _languageEntriesEsp = new()
{
    { "TitaniumClone", "Clon de Titanio" }, { "Tooltip_TitaniumClone", "Clon de Titanio que me hace decir que sí" }
};

// Register our English language entries to the English language
LanguageHandler.RegisterLocalization("English", _languageEntriesEng);

// Register our Spanish language entries to the Spanish language
LanguageHandler.RegisterLocalization("Spanish", _languageEntriesEsp);
```

---

## Singular Translation

Another approach that can be used is translating one key to any desired language.
Additionally, all Nautilus methods that interact with language keys also offer modders to choose the language to translate for.

### Examples
The following examples demonstrate the usage of singular translations.
```csharp
LanguageHandler.SetLanguageLine("TitaniumClone", "Titanium Clone", "English");
LanguageHandler.SetLanguageLine("TitaniumClone", "Clon de Titanio", "Spanish");

// Adds Spanish translation instead of English
PrefabInfo info = 
    PrefabInfo.WithTechType("TitaniumClone", 
                            "Clon de Titanio", 
                            "Clon de Titanio que me hace decir que sí", "Spanish");
```

## See also
- [LanguageHandler](xref:Nautilus.Handlers.LanguageHandler)