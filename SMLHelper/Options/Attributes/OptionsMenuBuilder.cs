namespace SMLHelper.V2.Options.Attributes
{
    using Interfaces;
    using Json;
    using System;
    using System.Linq;
    using UnityEngine;
    using Logger = Logger;
#if SUBNAUTICA
    using Text = UnityEngine.UI.Text;
#elif BELOWZERO
    using Text = TMPro.TextMeshProUGUI;
#endif
#if SUBNAUTICA_STABLE
    using Oculus.Newtonsoft.Json;
    using System.Collections.Generic;
#else
    using Newtonsoft.Json;
#endif

    /// <summary>
    /// An internal derivative of <see cref="ModOptions"/> for use in auto-generating a menu based on attributes
    /// declared in a <see cref="ConfigFile"/>.
    /// </summary>
    /// <typeparam name="T">The type of the class derived from <see cref="ConfigFile"/> to use for
    /// loading to/saving from disk.</typeparam>
    internal class OptionsMenuBuilder<T> : ModOptions where T : ConfigFile, new()
    {
        public ConfigFileMetadata<T> ConfigFileMetadata { get; } = new ConfigFileMetadata<T>();

        /// <summary>
        /// Instantiates a new <see cref="OptionsMenuBuilder{T}"/>, generating <see cref="ModOption"/>s by parsing the fields,
        /// properties and methods declared in the class.
        /// </summary>
        public OptionsMenuBuilder() : base(null)
        {
            BindEvents();

            ConfigFileMetadata.ProcessMetadata();

            Name = ConfigFileMetadata.MenuAttribute.Name;

            // Conditionally load the config
            if (ConfigFileMetadata.MenuAttribute.LoadOn.HasFlag(MenuAttribute.LoadEvents.MenuRegistered))
                ConfigFileMetadata.Config.Load();
        }

        #region Click, Change and GameObjectCreated Events
        /// <summary>
        /// Conditionally binds events, ie. OnButtonClicked, OnKeybindChanged, etc.
        /// </summary>
        private void BindEvents()
        {
            ButtonClicked += OptionsMenuBuilder_ButtonClicked;
            ChoiceChanged += OptionsMenuBuilder_ChoiceChanged;
            KeybindChanged += OptionsMenuBuilder_KeybindChanged;
            SliderChanged += OptionsMenuBuilder_SliderChanged;
            ToggleChanged += OptionsMenuBuilder_ToggleChanged;
            ConfigFileMetadata.Config.OnStartedLoading += OptionsMenuBuilder_Config_OnStartedLoading;
            ConfigFileMetadata.Config.OnFinishedLoading += OptionsMenuBuilder_Config_OnFinishedLoading;

            GameObjectCreated += OptionsMenuBuilder_GameObjectCreated;
        }

        /// <summary>
        /// Invokes the method for a given <see cref="ButtonAttribute"/> and passes parameters when the button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the original button click event.</param>
        /// <param name="e">The <see cref="ButtonClickedEventArgs"/> for the click event.</param>
        private void OptionsMenuBuilder_ButtonClicked(object sender, ButtonClickedEventArgs e)
        {
            if (ConfigFileMetadata.TryGetMetadata(e.Id, out ModOptionAttributeMetadata<T> modOptionMetadata)
                && modOptionMetadata.MemberInfoMetadata.MethodParameterTypes is Type[] parameterTypes)
            {
                var parameters = new object[parameterTypes.Length];
                var senderFound = false;
                var eventArgsFound = false;

                for (var i = 0; i < parameters.Length; i++)
                {
                    Type type = parameterTypes[i];
                    if (!senderFound && type == typeof(object))
                    {
                        senderFound = true;
                        parameters[i] = sender;
                    }
                    else if (!eventArgsFound && type == typeof(ButtonClickedEventArgs))
                    {
                        eventArgsFound = true;
                        parameters[i] = e;
                    }

                    if (senderFound && eventArgsFound)
                        break;
                }

                modOptionMetadata.MemberInfoMetadata.InvokeMethod(ConfigFileMetadata.Config, parameters);
            }
        }

        /// <summary>
        /// Sets the value in the <see cref="ConfigFileMetadata{T}.Config"/>, optionally saving the
        /// <see cref="ConfigFileMetadata{T}.Config"/> to disk if the <see cref="MenuAttribute.SaveEvents.ChangeValue"/>
        /// flag is set, before passing off to <see cref="InvokeOnChangeEvents{TSource}(ModOptionAttributeMetadata{T}, object, TSource)"/>
        /// to invoke any methods specified with an <see cref="OnChangeAttribute"/>.
        /// </summary>
        /// <param name="sender">The sender of the original choice changed event.</param>
        /// <param name="e">The <see cref="ChoiceChangedEventArgs"/> for the choice changed event.</param>
        private void OptionsMenuBuilder_ChoiceChanged(object sender, ChoiceChangedEventArgs e)
        {
            if (ConfigFileMetadata.TryGetMetadata(e.Id, out ModOptionAttributeMetadata<T> modOptionMetadata))
            {
                // Set the value in the Config
                MemberInfoMetadata<T> memberInfoMetadata = modOptionMetadata.MemberInfoMetadata;
                ChoiceAttribute choiceAttribute = modOptionMetadata.ModOptionAttribute as ChoiceAttribute;

                if (memberInfoMetadata.ValueType.IsEnum && (choiceAttribute.Options == null || !choiceAttribute.Options.Any()))
                {
                    // Enum-based choice where the values are parsed from the enum type
                    object value = Enum.Parse(memberInfoMetadata.ValueType, e.Value);
                    memberInfoMetadata.SetValue(ConfigFileMetadata.Config, value);
                }
                else if (memberInfoMetadata.ValueType.IsEnum)
                {
                    // Enum-based choice where the values are defined as custom strings
                    object value = Enum.Parse(memberInfoMetadata.ValueType, Enum.GetNames(memberInfoMetadata.ValueType)[e.Index]);
                    memberInfoMetadata.SetValue(ConfigFileMetadata.Config, value);
                }
                else if (memberInfoMetadata.ValueType == typeof(string))
                {
                    // string-based choice value
                    string value = e.Value;
                    memberInfoMetadata.SetValue(ConfigFileMetadata.Config, value);
                }
                else if (memberInfoMetadata.ValueType == typeof(int))
                {
                    // index-based choice value
                    int value = e.Index;
                    memberInfoMetadata.SetValue(ConfigFileMetadata.Config, value);
                }

                // Optionally save the Config to disk
                if (ConfigFileMetadata.MenuAttribute.SaveOn.HasFlag(MenuAttribute.SaveEvents.ChangeValue))
                    ConfigFileMetadata.Config.Save();

                // Invoke any OnChange methods specified
                InvokeOnChangeEvents(modOptionMetadata, sender, e);
            }
        }

        /// <summary>
        /// Sets the value in the <see cref="ConfigFileMetadata{T}.Config"/>, optionally saving the
        /// <see cref="ConfigFileMetadata{T}.Config"/> to disk if the <see cref="MenuAttribute.SaveEvents.ChangeValue"/>
        /// flag is set, before passing off to <see cref="InvokeOnChangeEvents{TSource}(ModOptionAttributeMetadata{T}, object, TSource)"/>
        /// to invoke any methods specified with an <see cref="OnChangeAttribute"/>.
        /// </summary>
        /// <param name="sender">The sender of the original keybind changed event.</param>
        /// <param name="e">The <see cref="KeybindChangedEventArgs"/> for the keybind changed event.</param>
        private void OptionsMenuBuilder_KeybindChanged(object sender, KeybindChangedEventArgs e)
        {
            if (ConfigFileMetadata.TryGetMetadata(e.Id, out ModOptionAttributeMetadata<T> modOptionMetadata))
            {
                // Set the value in the Config
                modOptionMetadata.MemberInfoMetadata.SetValue(ConfigFileMetadata.Config, e.Key);

                // Optionally save the Config to disk
                if (ConfigFileMetadata.MenuAttribute.SaveOn.HasFlag(MenuAttribute.SaveEvents.ChangeValue))
                    ConfigFileMetadata.Config.Save();

                // Invoke any OnChange methods specified
                InvokeOnChangeEvents(modOptionMetadata, sender, e);
            }
        }

        /// <summary>
        /// Sets the value in the <see cref="ConfigFileMetadata{T}.Config"/>, optionally saving the
        /// <see cref="ConfigFileMetadata{T}.Config"/> to disk if the <see cref="MenuAttribute.SaveEvents.ChangeValue"/>
        /// flag is set, before passing off to <see cref="InvokeOnChangeEvents{TSource}(ModOptionAttributeMetadata{T}, object, TSource)"/>
        /// to invoke any methods specified with an <see cref="OnChangeAttribute"/>.
        /// </summary>
        /// <param name="sender">The sender of the original slider changed event.</param>
        /// <param name="e">The <see cref="SliderChangedEventArgs"/> for the slider changed event.</param>
        private void OptionsMenuBuilder_SliderChanged(object sender, SliderChangedEventArgs e)
        {
            if (ConfigFileMetadata.TryGetMetadata(e.Id, out ModOptionAttributeMetadata<T> modOptionMetadata))
            {
                // Set the value in the Config
                MemberInfoMetadata<T> memberInfoMetadata = modOptionMetadata.MemberInfoMetadata;
                object value = Convert.ChangeType(e.Value, memberInfoMetadata.ValueType);
                memberInfoMetadata.SetValue(ConfigFileMetadata.Config, value);

                // Optionally save the Config to disk
                if (ConfigFileMetadata.MenuAttribute.SaveOn.HasFlag(MenuAttribute.SaveEvents.ChangeValue))
                    ConfigFileMetadata.Config.Save();

                // Invoke any OnChange methods specified
                InvokeOnChangeEvents(modOptionMetadata, sender, e);
            }
        }

        /// <summary>
        /// Sets the value in the <see cref="ConfigFileMetadata{T}.Config"/>, optionally saving the
        /// <see cref="ConfigFileMetadata{T}.Config"/> to disk if the <see cref="MenuAttribute.SaveEvents.ChangeValue"/>
        /// flag is set, before passing off to <see cref="InvokeOnChangeEvents{TSource}(ModOptionAttributeMetadata{T}, object, TSource)"/>
        /// to invoke any methods specified with an <see cref="OnChangeAttribute"/>.
        /// </summary>
        /// <param name="sender">The sender of the original toggle changed event.</param>
        /// <param name="e">The <see cref="ToggleChangedEventArgs"/> for the toggle changed event.</param>
        private void OptionsMenuBuilder_ToggleChanged(object sender, ToggleChangedEventArgs e)
        {
            if (ConfigFileMetadata.TryGetMetadata(e.Id, out ModOptionAttributeMetadata<T> modOptionMetadata))
            {
                // Set the value in the Config
                modOptionMetadata.MemberInfoMetadata.SetValue(ConfigFileMetadata.Config, e.Value);

                // Optionally save the Config to disk
                if (ConfigFileMetadata.MenuAttribute.SaveOn.HasFlag(MenuAttribute.SaveEvents.ChangeValue))
                    ConfigFileMetadata.Config.Save();

                // Invoke any OnChange methods specified
                InvokeOnChangeEvents(modOptionMetadata, sender, e);
            }
        }

        private string jsonConfig;
        private void OptionsMenuBuilder_Config_OnStartedLoading(object sender, ConfigFileEventArgs e)
        {
            jsonConfig = JsonConvert.SerializeObject(e.Instance as T);
        }

        private void OptionsMenuBuilder_Config_OnFinishedLoading(object sender, ConfigFileEventArgs e)
        {
            T oldConfig = JsonConvert.DeserializeObject<T>(jsonConfig);
            T currentConfig = e.Instance as T;

            foreach (ModOptionAttributeMetadata<T> modOptionMetadata in ConfigFileMetadata.GetValues())
            {
                if (modOptionMetadata.MemberInfoMetadata.MemberType != MemberType.Field &&
                    modOptionMetadata.MemberInfoMetadata.MemberType != MemberType.Property)
                {
                    continue;
                }

                if (!modOptionMetadata.MemberInfoMetadata.GetValue(oldConfig)
                    .Equals(modOptionMetadata.MemberInfoMetadata.GetValue(currentConfig)))
                {
                    InvokeOnChangeEvents(modOptionMetadata, sender);
                }
            }
        }

        /// <summary>
        /// Invokes the relevant method(s) specified with the <see cref="OnChangeAttribute"/>(s)
        /// and passes parameters when a value is changed when loaded from disk.
        /// </summary>
        /// <param name="modOptionMetadata">The metadata for the mod option.</param>
        /// <param name="sender">The sender of the event.</param>
        private void InvokeOnChangeEvents(ModOptionAttributeMetadata<T> modOptionMetadata, object sender)
        {
            string id = modOptionMetadata.ModOptionAttribute.Id;
            MemberInfoMetadata<T> memberInfoMetadata = modOptionMetadata.MemberInfoMetadata;

            switch (modOptionMetadata.ModOptionAttribute)
            {
                case ChoiceAttribute choiceAttribute when (memberInfoMetadata.ValueType.IsEnum && (choiceAttribute.Options == null || !choiceAttribute.Options.Any())):
                    // Enum-based choice where the values are parsed from the enum type
                    {
                        string[] options = Enum.GetNames(memberInfoMetadata.ValueType);
                        string value = memberInfoMetadata.GetValue(ConfigFileMetadata.Config).ToString();
                        var eventArgs = new ChoiceChangedEventArgs(id, Array.IndexOf(options, value), value);
                        InvokeOnChangeEvents(modOptionMetadata, sender, eventArgs);
                    }
                    break;
                case ChoiceAttribute _ when memberInfoMetadata.ValueType.IsEnum:
                    // Enum-based choice where the values are defined as custom strings
                    {
                        string value = memberInfoMetadata.GetValue(ConfigFileMetadata.Config).ToString();
                        int index = Math.Max(Array.IndexOf(Enum.GetValues(memberInfoMetadata.ValueType), value), 0);
                        var eventArgs = new ChoiceChangedEventArgs(id, index, value);
                        InvokeOnChangeEvents(modOptionMetadata, sender, eventArgs);
                    }
                    break;
                case ChoiceAttribute choiceAttribute when memberInfoMetadata.ValueType == typeof(string):
                    // string-based choice value
                    {
                        string[] options = choiceAttribute.Options;
                        string value = memberInfoMetadata.GetValue<string>(ConfigFileMetadata.Config);
                        var eventArgs = new ChoiceChangedEventArgs(id, Array.IndexOf(options, value), value);
                        InvokeOnChangeEvents(modOptionMetadata, sender, eventArgs);
                    }
                    break;
                case ChoiceAttribute choiceAttribute when memberInfoMetadata.ValueType == typeof(int):
                    // index-based choice value
                    {
                        string[] options = choiceAttribute.Options;
                        int index = memberInfoMetadata.GetValue<int>(ConfigFileMetadata.Config);
                        var eventArgs = new ChoiceChangedEventArgs(id, index, options[index]);
                        InvokeOnChangeEvents(modOptionMetadata, sender, eventArgs);
                    }
                    break;

                case KeybindAttribute _:
                    {
                        var eventArgs = new KeybindChangedEventArgs(id, memberInfoMetadata.GetValue<KeyCode>(ConfigFileMetadata.Config));
                        InvokeOnChangeEvents(modOptionMetadata, sender, eventArgs);
                    }
                    break;

                case SliderAttribute _:
                    {
                        var eventArgs = new SliderChangedEventArgs(id, Convert.ToSingle(memberInfoMetadata.GetValue(ConfigFileMetadata.Config)));
                        InvokeOnChangeEvents(modOptionMetadata, sender, eventArgs);
                    }
                    break;

                case ToggleAttribute _:
                    {
                        var eventArgs = new ToggleChangedEventArgs(id, memberInfoMetadata.GetValue<bool>(ConfigFileMetadata.Config));
                        InvokeOnChangeEvents(modOptionMetadata, sender, eventArgs);
                    }
                    break;
            }
        }

        /// <summary>
        /// Invokes the relevant method(s) specified with <see cref="OnChangeAttribute"/>(s)
        /// and passes parameters when a value is changed.
        /// </summary>
        /// <typeparam name="TSource">The type of the event args.</typeparam>
        /// <param name="modOptionMetadata">The metadata for the mod option.</param>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event args from the OnChange event.</param>
        private void InvokeOnChangeEvents<TSource>(ModOptionAttributeMetadata<T> modOptionMetadata, object sender, TSource e)
            where TSource : IModOptionEventArgs
        {
            if (modOptionMetadata.OnChangeMetadata == null)
                return; // Skip attempting to invoke events if there are no OnChangeAttributes set for the member.

            foreach (MemberInfoMetadata<T> onChangeMetadata in modOptionMetadata.OnChangeMetadata)
            {
                InvokeEvent(onChangeMetadata, sender, e);
            }
        }

        /// <summary>
        /// Invoke the relevant method specified by a <see cref="ModOptionEventAttribute"/>
        /// and passes relevant parameters.
        /// </summary>
        /// <typeparam name="TSource">The type of the event args.</typeparam>
        /// <param name="memberInfoMetadata">The metadata for the method.</param>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event args from the event.</param>
        private void InvokeEvent<TSource>(MemberInfoMetadata<T> memberInfoMetadata, object sender, TSource e)
            where TSource : IModOptionEventArgs
        {
            if (!memberInfoMetadata.MethodValid)
            {
                // Method not found, log error and skip.
                Logger.Error($"[OptionsMenuBuilder] Could not find the specified method: {typeof(T)}.{memberInfoMetadata.Name}");
                return;
            }

            if (memberInfoMetadata.MethodParameterTypes is Type[] parameterTypes)
            {
                var parameters = new object[parameterTypes.Length];
                var senderFound = false;
                var eventArgsFound = false;
                var modOptionEventFound = false;

                for (var i = 0; i < parameterTypes.Length; i++)
                {
                    if (!senderFound && parameterTypes[i] == typeof(object))
                    {
                        senderFound = true;
                        parameters[i] = sender;
                    }
                    else if (!eventArgsFound && parameterTypes[i] == typeof(TSource))
                    {
                        eventArgsFound = true;
                        parameters[i] = e;
                    }
                    else if (!modOptionEventFound && parameterTypes[i] == typeof(IModOptionEventArgs))
                    {
                        modOptionEventFound = true;
                        parameters[i] = e;
                    }

                    if (senderFound && eventArgsFound && modOptionEventFound)
                        break;
                }

                memberInfoMetadata.InvokeMethod(ConfigFileMetadata.Config, parameters);
            }
        }

        /// <summary>
        /// Generates tooltips for each <see cref="ModOption"/> with a specified <see cref="TooltipAttribute"/>, before
        /// invoking any relevant method(s) specified with <see cref="OnGameObjectCreatedAttribute"/>(s) and passes
        /// parameters when a <see cref="GameObject"/> is created in the options menu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OptionsMenuBuilder_GameObjectCreated(object sender, GameObjectCreatedEventArgs e)
        {
            if (ConfigFileMetadata.TryGetMetadata(e.Id, out ModOptionAttributeMetadata<T> modOptionMetadata))
            {
                // Create a tooltip if there is a TooltipAttribute specified
                if (modOptionMetadata.ModOptionAttribute.Tooltip is string tooltip)
                {
                    e.GameObject.GetComponentInChildren<Text>().gameObject.AddComponent<ModOptionTooltip>().Tooltip = tooltip;
                }

                if (modOptionMetadata.OnGameObjectCreatedMetadata == null)
                    return; // Skip attempting to invoke events if there are no OnGameObjectCreatedAttributes set for the member.

                foreach (MemberInfoMetadata<T> onGameObjectCreatedMetadata in modOptionMetadata.OnGameObjectCreatedMetadata)
                {
                    InvokeEvent(onGameObjectCreatedMetadata, sender, e);
                }
            }
        }
        #endregion

        #region Build ModOptions Menu
        /// <summary>
        /// Adds options to the menu based on the <see cref="ConfigFileMetadata"/>.
        /// </summary>
        public override void BuildModOptions()
        {
            // Conditionally load the config
            if (ConfigFileMetadata.MenuAttribute.LoadOn.HasFlag(MenuAttribute.LoadEvents.MenuOpened))
                ConfigFileMetadata.Config.Load();

            foreach (KeyValuePair<string, ModOptionAttributeMetadata<T>> entry in ConfigFileMetadata.ModOptionAttributesMetadata
                .OrderBy(x => x.Value.ModOptionAttribute.Order)
                .ThenBy(x => x.Value.MemberInfoMetadata.Name))
            {
                string id = entry.Key;
                ModOptionAttributeMetadata<T> modOptionMetadata = entry.Value;

                Logger.Debug($"[{ConfigFileMetadata.QMod.DisplayName}] [{typeof(T).Name}] {modOptionMetadata.MemberInfoMetadata.Name}: " +
                    $"{modOptionMetadata.ModOptionAttribute.GetType()}");
                Logger.Debug($"[{ConfigFileMetadata.QMod.DisplayName}] [{typeof(T).Name}] Label: {modOptionMetadata.ModOptionAttribute.Label}");

                string label = modOptionMetadata.ModOptionAttribute.Label;

                switch (modOptionMetadata.ModOptionAttribute)
                {
                    case ButtonAttribute _:
                        BuildModButtonOption(id, label);
                        break;
                    case ChoiceAttribute choiceAttribute:
                        BuildModChoiceOption(id, label, modOptionMetadata.MemberInfoMetadata, choiceAttribute);
                        break;
                    case KeybindAttribute _:
                        BuildModKeybindOption(id, label, modOptionMetadata.MemberInfoMetadata);
                        break;
                    case SliderAttribute sliderAttribute:
                        BuildModSliderOption(id, label, modOptionMetadata.MemberInfoMetadata, sliderAttribute);
                        break;
                    case ToggleAttribute _:
                        BuildModToggleOption(id, label, modOptionMetadata.MemberInfoMetadata);
                        break;
                }
            }
        }

        /// <summary>
        /// Adds a <see cref="ModButtonOption"/> to the <see cref="ModOptions"/> menu.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="label"></param>
        private void BuildModButtonOption(string id, string label)
        {
            AddButtonOption(id, label);
        }

        /// <summary>
        /// Adds a <see cref="ModChoiceOption"/> to the <see cref="ModOptions"/> menu.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="label"></param>
        /// <param name="memberInfoMetadata">The metadata of the corresponding member.</param>
        /// <param name="choiceAttribute">The defined or generated <see cref="ChoiceAttribute"/> of the member.</param>
        private void BuildModChoiceOption(string id, string label,
            MemberInfoMetadata<T> memberInfoMetadata, ChoiceAttribute choiceAttribute)
        {
            if (memberInfoMetadata.ValueType.IsEnum && (choiceAttribute.Options == null || !choiceAttribute.Options.Any()))
            {
                // Enum-based choice where the values are parsed from the enum type
                string[] options = Enum.GetNames(memberInfoMetadata.ValueType);
                string value = memberInfoMetadata.GetValue(ConfigFileMetadata.Config).ToString();
                AddChoiceOption(id, label, options, value);
            }
            else if (memberInfoMetadata.ValueType.IsEnum)
            {
                // Enum-based choice where the values are defined as custom strings
                string[] options = choiceAttribute.Options;
                string value = memberInfoMetadata.GetValue(ConfigFileMetadata.Config).ToString();
                int index = Math.Max(Array.IndexOf(Enum.GetValues(memberInfoMetadata.ValueType), value), 0);
                AddChoiceOption(id, label, options, index);
            }
            else if (memberInfoMetadata.ValueType == typeof(string))
            {
                // string-based choice value
                string[] options = choiceAttribute.Options;
                string value = memberInfoMetadata.GetValue<string>(ConfigFileMetadata.Config);
                AddChoiceOption(id, label, options, value);
            }
            else if (memberInfoMetadata.ValueType == typeof(int))
            {
                // index-based choice value
                string[] options = choiceAttribute.Options;
                int index = memberInfoMetadata.GetValue<int>(ConfigFileMetadata.Config);
                AddChoiceOption(id, label, options, index);
            }
        }

        /// <summary>
        /// Adds a <see cref="ModKeybindOption"/> to the <see cref="ModOptions"/> menu.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="label"></param>
        /// <param name="memberInfoMetadata">The metadata of the corresponding member.</param>
        private void BuildModKeybindOption(string id, string label, MemberInfoMetadata<T> memberInfoMetadata)
        {
            KeyCode value = memberInfoMetadata.GetValue<KeyCode>(ConfigFileMetadata.Config);
            AddKeybindOption(id, label, GameInput.Device.Keyboard, value);
        }

        /// <summary>
        /// Adds a <see cref="ModSliderOption"/> to the <see cref="ModOptions"/> menu.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="label"></param>
        /// <param name="memberInfoMetadata">The metadata of the corresponding member.</param>
        /// <param name="sliderAttribute">The defined or generated <see cref="SliderAttribute"/> of the member.</param>
        private void BuildModSliderOption(string id, string label,
            MemberInfoMetadata<T> memberInfoMetadata, SliderAttribute sliderAttribute)
        {
            float value = Convert.ToSingle(memberInfoMetadata.GetValue(ConfigFileMetadata.Config));

            float step = sliderAttribute.Step;
            if (memberInfoMetadata.ValueType == typeof(int))
                step = Mathf.CeilToInt(step);

            AddSliderOption(id, label, sliderAttribute.Min, sliderAttribute.Max,
                Convert.ToSingle(value), sliderAttribute.DefaultValue,
                sliderAttribute.Format, step);
        }

        /// <summary>
        /// Adds a <see cref="ModToggleOption"/> to the <see cref="ModOptions"/> menu.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="label"></param>
        /// <param name="memberInfoMetadata">The metadata of the corresponding member.</param>
        private void BuildModToggleOption(string id, string label, MemberInfoMetadata<T> memberInfoMetadata)
        {
            bool value = memberInfoMetadata.GetValue<bool>(ConfigFileMetadata.Config);
            AddToggleOption(id, label, value);
        }
        #endregion
    }
}
