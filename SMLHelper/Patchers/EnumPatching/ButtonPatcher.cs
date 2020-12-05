#if BELOWZERO
namespace SMLHelper.V2.Patchers.EnumPatching
{
    using System;
    using System.Collections.Generic;
    using SMLHelper.V2.Handlers;
    using Utility;
    using static GameInput;

    internal class ButtonPatcher
    {
        private const string ButtonEnumName = "Button";
        internal static readonly int startingIndex = 1000;
        internal static readonly List<int> bannedIndices = new List<int>();
        internal static readonly List<Tuple<Device, Button, string>> RegisteredButtons = new List<Tuple<Device, Button, string>>();

        internal static readonly EnumCacheManager<Button> cacheManager =
            new EnumCacheManager<Button>(
                enumTypeName: ButtonEnumName,
                startingIndex: startingIndex,
                bannedIDs: ExtBannedIdManager.GetBannedIdsFor(ButtonEnumName, bannedIndices));

        internal static Button EnsureButton(string name, string key, Device device)
        {
            if(!cacheManager.TryParse(name, out Button button))
            {
                EnumTypeCache cache = cacheManager.RequestCacheForTypeName(name);

                if (cache == null)
                {
                    cache = new EnumTypeCache()
                    {
                        Name = name,
                        Index = cacheManager.GetNextAvailableIndex()
                    };
                }

                if (cacheManager.IsIndexAvailable(cache.Index))
                    cache.Index = cacheManager.GetNextAvailableIndex();

                button = (Button)cache.Index;
                cacheManager.Add(button, cache.Index, cache.Name);
                RegisterNewButton(name, key, button, device);

                Logger.Log($"Successfully added Button: '{name}' to Index: '{cache.Index}'", LogLevel.Debug);
                return button;
            }
            else
            {
                return button;
            }
        }

        private static void RegisterNewButton(string name, string key, Button button, Device device)
        {
            var bind = new Tuple<Device, Button, string>(device, button, key);
            if (RegisteredButtons.Contains(bind))
                return;

            RegisteredButtons.Add(bind);
            LanguageHandler.SetLanguageLine($"Option{name}", name);
            Language.main.LoadLanguageFile(Language.main.GetCurrentLanguage());
            GameInput.AddKeyInput(name, KeyCodeUtils.StringToKeyCode(key), device);
            GameInput.instance.Initialize();
            for (int i = 0; i < GameInput.numDevices; i++)
            {
                GameInput.SetupDefaultBindings((GameInput.Device)i);
            }
            foreach(Tuple<Device, Button, string> binding in RegisteredButtons)
            {
                GameInput.SafeSetBinding(binding.Item1, binding.Item2, BindingSet.Primary, binding.Item3);
            }

        }

        internal static void Patch()
        {
            IngameMenuHandler.Main.RegisterOneTimeUseOnSaveEvent(() => cacheManager.SaveCache());

            Logger.Log($"Added {cacheManager.ModdedKeysCount} Buttons succesfully into the game.", LogLevel.Info);

            Logger.Log("ButtonPatcher is done.", LogLevel.Debug);
        }
    }
}
#endif