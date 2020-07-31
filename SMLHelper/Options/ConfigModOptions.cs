namespace SMLHelper.V2.Options
{
    using HarmonyLib;
    using QModManager.API;
    using SMLHelper.V2.Interfaces;
    using SMLHelper.V2.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;
    using UWE;
    using Logger = V2.Logger;
#if SUBNAUTICA
    using Text = UnityEngine.UI.Text;
#elif BELOWZERO
    using Text = TMPro.TextMeshProUGUI;
#endif

    /// <summary>
    /// An internal derivative of <see cref="ModOptions"/> for use in auto-generating a menu based on attributes
    /// declared in a <see cref="ConfigFile"/>.
    /// </summary>
    /// <typeparam name="TConfigFile">The type of the class derived from <see cref="ConfigFile"/> to use for
    /// loading to/saving from disk.</typeparam>
    internal class ConfigModOptions<TConfigFile> : ModOptions where TConfigFile : ConfigFile, new()
    {
        /// <summary>
        /// The <typeparamref name="TConfigFile"/> <see cref="ConfigFile"/> instance related to this <see cref="ModOptions"/> menu.
        /// </summary>
        public TConfigFile Config { get; }

        /// <summary>
        /// The <see cref="MenuAttribute"/> relating to this <see cref="ModOptions"/> menu.
        /// </summary>
        private readonly MenuAttribute menuAttribute;

        /// <summary>
        /// A simple struct containing metadata for each auto-generated <see cref="ModOption"/>.
        /// </summary>
        private struct ModOptionMetadata
        {
            public Type ModOptionType;
            public LabelAttribute LabelAttribute;
            public MemberInfo MemberInfo;
            public TooltipAttribute TooltipAttribute;
            public OnChangeAttribute[] OnChangeAttributes;
            public OnGameObjectCreatedAttribute[] OnGameObjectCreatedAttributes;
            public ButtonAttribute ButtonAttribute;
            public ChoiceAttribute ChoiceAttribute;
            public SliderAttribute SliderAttribute;
        }

        /// <summary>
        /// A dictionary of <see cref="ModOptionMetadata"/>, indexed by <see cref="ModOption.Id"/>.
        /// </summary>
        private readonly Dictionary<string, ModOptionMetadata> modOptionsMetadata
            = new Dictionary<string, ModOptionMetadata>();

        /// <summary>
        /// A list of attributes which when declared on a member of <see cref="Config"/> will be used to
        /// automatically generate a <see cref="ModOption"/>.
        /// </summary>
        private static readonly Type[] relevantAttributeTypes = new Type[] {
            typeof(LabelAttribute), typeof(ButtonAttribute), typeof(ChoiceAttribute), typeof(OnChangeAttribute),
            typeof(OnGameObjectCreatedAttribute), typeof(SliderAttribute), typeof(TooltipAttribute)
        };

        /// <summary>
        /// Instantiates a new <see cref="ConfigModOptions{T}"/>, generating <see cref="ModOption"/>s by parsing the fields,
        /// properties and methods declared in the class.
        /// </summary>
        public ConfigModOptions()
            : base((typeof(TConfigFile).GetCustomAttributes(typeof(MenuAttribute), true).SingleOrDefault() as MenuAttribute)?.Name
                  ?? QModServices.Main.GetMod(Assembly.GetCallingAssembly()).DisplayName)
        {
            // Instantiate and load the config
            Config = new TConfigFile();
            Config.Load();

            menuAttribute = typeof(TConfigFile).GetCustomAttributes(typeof(MenuAttribute), true).SingleOrDefault() as MenuAttribute
                ?? new MenuAttribute(Name);

            var bindingFlags = BindingFlags.Instance | BindingFlags.Public;
            foreach (PropertyInfo property in typeof(TConfigFile).GetProperties(bindingFlags)
                .Where(MemberIsDeclaredInConfigFileSubclass) // Only care about members declared in a subclass of ConfigFile
                .Where(MemberIsNotIgnored)) // Filter out explicitly ignored members
                ProcessFieldOrProperty(property, property.PropertyType);

            foreach (FieldInfo field in typeof(TConfigFile).GetFields(bindingFlags)
                .Where(MemberIsDeclaredInConfigFileSubclass) // Only care about members declared in a subclass of ConfigFile
                .Where(MemberIsNotIgnored)) // Filter out explicitly ignored members
                ProcessFieldOrProperty(field, field.FieldType);

            foreach (MethodInfo method in typeof(TConfigFile).GetMethods(bindingFlags | BindingFlags.NonPublic | BindingFlags.Static)
                .Where(MemberIsDeclaredInConfigFileSubclass) // Only care about members declared in a subclass of ConfigFile
                .Where(MemberIsNotIgnored)) // Filter out explicitly ignored members
                ProcessMethod(method);

            Logger.Debug($"[ConfigModOptions] Found {modOptionsMetadata.Count()} options to add to the menu.");

            // Conditionally add events
            if (modOptionsMetadata.Values.Any(x => x.ModOptionType == typeof(ModButtonOption)))
                ButtonClicked += ConfigModOptions_ButtonClicked;

            if (modOptionsMetadata.Values.Any(x => x.ModOptionType == typeof(ModChoiceOption)))
                ChoiceChanged += ConfigModOptions_ChoiceChanged;

            if (modOptionsMetadata.Values.Any(x => x.ModOptionType == typeof(ModKeybindOption)))
                KeybindChanged += ConfigModOptions_KeybindChanged;

            if (modOptionsMetadata.Values.Any(x => x.ModOptionType == typeof(ModSliderOption)))
                SliderChanged += ConfigModOptions_SliderChanged;

            if (modOptionsMetadata.Values.Any(x => x.ModOptionType == typeof(ModToggleOption)))
                ToggleChanged += ConfigModOptions_ToggleChanged;

            if (modOptionsMetadata.Values.Any(x => x.TooltipAttribute != null
                || (x.OnGameObjectCreatedAttributes != null && x.OnGameObjectCreatedAttributes.Any())))
                GameObjectCreated += ConfigModOptions_GameObjectCreated;
        }

        /// <summary>
        /// Checks whether a given <see cref="MemberInfo"/> is declared in any subclass of <see cref="ConfigFile"/>.
        /// </summary>
        /// <param name="memberInfo">The <see cref="MemberInfo"/> to check.</param>
        /// <returns>Whether the given <see cref="MemberInfo"/> is declared in any subclass of <see cref="ConfigFile"/>.</returns>
        private static bool MemberIsDeclaredInConfigFileSubclass(MemberInfo memberInfo)
            => memberInfo.DeclaringType.IsSubclassOf(typeof(ConfigFile));

        /// <summary>
        /// Checks whether a given <see cref="MemberInfo"/> should be ignored when generating the options menu, based on whether
        /// the member has a declared <see cref="IgnoreMemberAttribute"/>, or the <see cref="menuAttribute"/>'s
        /// <see cref="MenuAttribute.IgnoreUnattributedMembers"/> property.
        /// </summary>
        /// <param name="memberInfo">The <see cref="MemberInfo"/> to check.</param>
        /// <returns>Whether the given <see cref="MemberInfo"/> member should be ignored when generating the options menu.</returns>
        private bool MemberIsNotIgnored(MemberInfo memberInfo)
            => !memberInfo.GetCustomAttributes(typeof(IgnoreMemberAttribute), true).Any() &&
                (!menuAttribute.IgnoreUnattributedMembers || memberInfo.GetCustomAttributes(true)
                .Any(a => relevantAttributeTypes.Contains(a.GetType())));

        /// <summary>
        /// Processes the given field or property and hands off to <see cref="AddModOptionMetadata{TModOption}(MemberInfo, Type)"/>
        /// to generate a <see cref="ModOptionMetadata"/> and add it to the <see cref="modOptionsMetadata"/> dictionary.
        /// </summary>
        /// <param name="memberInfo">The <see cref="MemberInfo"/> of the member.</param>
        /// <param name="memberType">The underlying <see cref="Type"/> of the member.</param>
        private void ProcessFieldOrProperty(MemberInfo memberInfo, Type memberType)
        {
            if (memberType == typeof(bool))
            {   // Generate a ModToggleOption
                AddModOptionMetadata<ModToggleOption>(memberInfo, memberType);
            }
            else if (memberType == typeof(KeyCode))
            {   // Generate a ModKeybindOption
                AddModOptionMetadata<ModKeybindOption>(memberInfo, memberType);
            }
            else if (memberType.IsEnum || memberInfo.GetCustomAttributes(typeof(ChoiceAttribute), true).Any())
            {   // Generate a ModChoiceOption
                AddModOptionMetadata<ModChoiceOption>(memberInfo, memberType);
            }
            else if (new[] { typeof(float), typeof(double), typeof(int) }.Contains(memberType)
                || memberInfo.GetCustomAttributes(typeof(SliderAttribute), true).Any())
            {   // Generate a ModSliderOption
                AddModOptionMetadata<ModSliderOption>(memberInfo, memberType);
            }
        }

        /// <summary>
        /// Processes the given method and hands off to <see cref="AddModOptionMetadata{TModOption}(MemberInfo, Type)"/>
        /// to generate a <see cref="ModOptionMetadata"/> and add it to the <see cref="modOptionsMetadata"/> dictionary.
        /// </summary>
        /// <param name="methodInfo">The <see cref="MethodInfo"/> of the method.</param>
        private void ProcessMethod(MethodInfo methodInfo)
        {
            if (methodInfo.GetCustomAttributes(true).Any(attribute => attribute is LabelAttribute || attribute is ButtonAttribute))
            {   // Generate a ModButtonOption
                AddModOptionMetadata<ModButtonOption>(methodInfo, null);
            }
        }

        /// <summary>
        /// Generates a <see cref="ModOptionMetadata"/> based on the member and its attributes, then adds it to the
        /// <see cref="modOptionsMetadata"/> dictionary.
        /// </summary>
        /// <typeparam name="TModOption">The type of the <see cref="ModOption"/> to generate for this member.</typeparam>
        /// <param name="memberInfo">The <see cref="MemberInfo"/> of the member.</param>
        /// <param name="memberType">The underlying <see cref="Type"/> of the member.</param>
        private void AddModOptionMetadata<TModOption>(MemberInfo memberInfo, Type memberType) where TModOption : ModOption
        {
            try
            {
                var modOptionType = typeof(TModOption); // cache the ModOption type for comparisons

                // Get the label
                var labelAttribute = memberInfo.GetCustomAttributes(typeof(LabelAttribute), true)
                    .SingleOrDefault() as LabelAttribute;
                if (labelAttribute == null)
                    labelAttribute = new LabelAttribute();
                if (string.IsNullOrEmpty(labelAttribute.Label))
                    labelAttribute.Label = memberInfo.Name; // If there is no label specified, just use the member's name.

                // ModOptionMetadata needed for all ModOptions
                var modOptionMetadata = new ModOptionMetadata
                {
                    ModOptionType = modOptionType,
                    LabelAttribute = labelAttribute,
                    MemberInfo = memberInfo,
                    TooltipAttribute = memberInfo.GetCustomAttributes(typeof(TooltipAttribute), true)
                        .SingleOrDefault() as TooltipAttribute,
                    OnGameObjectCreatedAttributes = memberInfo.GetCustomAttributes(typeof(OnGameObjectCreatedAttribute), true)
                        .Select(o => o as OnGameObjectCreatedAttribute).ToArray()
                };

                if (modOptionType == typeof(ModButtonOption))
                {   // ModButtonOption specific metadata
                    modOptionMetadata.ButtonAttribute = memberInfo.GetCustomAttributes(typeof(ButtonAttribute), true)
                        .SingleOrDefault() as ButtonAttribute ?? new ButtonAttribute();
                    modOptionsMetadata.Add(labelAttribute.Id, modOptionMetadata);
                    return; // We don't need to process any further metadata for buttons
                }

                // Get the OnChange attributes
                modOptionMetadata.OnChangeAttributes = memberInfo.GetCustomAttributes(typeof(OnChangeAttribute), true)
                    .Select(o => o as OnChangeAttribute).ToArray();

                if (modOptionType == typeof(ModChoiceOption))
                {   // ModChoiceOption specific metadata
                    modOptionMetadata.ChoiceAttribute = memberInfo.GetCustomAttributes(typeof(ChoiceAttribute), true)
                        .SingleOrDefault() as ChoiceAttribute;
                }
                else if (modOptionType == typeof(ModSliderOption))
                {   // ModSliderOption specific metadata
                    modOptionMetadata.SliderAttribute = memberInfo.GetCustomAttributes(typeof(SliderAttribute), true)
                        .SingleOrDefault() as SliderAttribute
                        ?? new SliderAttribute();
                }

                modOptionsMetadata.Add(labelAttribute.Id, modOptionMetadata);
            }
            catch (Exception ex)
            {
                Logger.Error($"[ConfigModOptions] {ex.Message}");
            }
        }

        /// <summary>
        /// Invokes the method for a given <see cref="ButtonAttribute"/> and passes parameters when the button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the original button click event.</param>
        /// <param name="e">The <see cref="ButtonClickedEventArgs"/> for the click event.</param>
        private void ConfigModOptions_ButtonClicked(object sender, ButtonClickedEventArgs e)
        {
            if (modOptionsMetadata.TryGetValue(e.Id, out var modOptionMetadata)
                && modOptionMetadata.MemberInfo is MethodInfo methodInfo)
            {
                var parameters = methodInfo.GetParameters();
                var invokeParams = new object[parameters.Length];
                var senderFound = false;
                var eventArgsFound = false;

                for (var i = 0; i < parameters.Length; i++)
                {
                    if (senderFound && eventArgsFound)
                        break;

                    var param = parameters[i];
                    if (!senderFound && param.ParameterType == typeof(object))
                    {
                        senderFound = true;
                        invokeParams[i] = sender;
                        continue;
                    }

                    if (!eventArgsFound && param.ParameterType == typeof(ButtonClickedEventArgs))
                    {
                        eventArgsFound = true;
                        invokeParams[i] = e;
                    }
                }

                methodInfo.Invoke(Config, invokeParams);
            }
        }

        /// <summary>
        /// Sets the value in the <see cref="Config"/>, optionally saving the <see cref="Config"/> to disk if the
        /// <see cref="MenuAttribute.SaveEvents.ChangeValue"/> flag is set, before passing off to
        /// <see cref="InvokeOnChange{TEventArgs}(ModOptionMetadata, object, TEventArgs)"/> to invoke any methods specified with a
        /// <see cref="OnChangeAttribute"/>.
        /// </summary>
        /// <param name="sender">The sender of the original choice changed event.</param>
        /// <param name="e">The <see cref="ChoiceChangedEventArgs"/> for the choice changed event.</param>
        private void ConfigModOptions_ChoiceChanged(object sender, ChoiceChangedEventArgs e)
        {
            if (modOptionsMetadata.TryGetValue(e.Id, out var modOptionMetadata))
            {
                // Set the value in the Config
                switch (modOptionMetadata.MemberInfo)
                {
                    case PropertyInfo property when property.PropertyType.IsEnum && modOptionMetadata.ChoiceAttribute == null:
                        property.SetValue(Config, Enum.Parse(property.PropertyType, e.Value), null);
                        break;
                    case PropertyInfo property when property.PropertyType.IsEnum:
                        property.SetValue(Config,
                            Enum.Parse(property.PropertyType, Enum.GetNames(property.PropertyType)[e.Index]), null);
                        break;
                    case PropertyInfo property when property.PropertyType == typeof(string):
                        property.SetValue(Config, e.Value, null);
                        break;
                    case PropertyInfo property:
                        property.SetValue(Config, e.Index, null);
                        break;
                    case FieldInfo field when field.FieldType.IsEnum && modOptionMetadata.ChoiceAttribute == null:
                        field.SetValue(Config, Enum.Parse(field.FieldType, e.Value));
                        break;
                    case FieldInfo field when field.FieldType.IsEnum:
                        field.SetValue(Config, Enum.Parse(field.FieldType, Enum.GetNames(field.FieldType)[e.Index]));
                        break;
                    case FieldInfo field when field.FieldType == typeof(string):
                        field.SetValue(Config, e.Value);
                        break;
                    case FieldInfo field:
                        field.SetValue(Config, e.Index);
                        break;
                }

                // Optionally save the Config to disk
                if (menuAttribute.SaveOn.HasFlag(MenuAttribute.SaveEvents.ChangeValue))
                    Config.Save();

                // Invoke any OnChange methods specified
                InvokeOnChange(modOptionMetadata, sender, e);
            }
        }

        /// <summary>
        /// Sets the value in the <see cref="Config"/>, optionally saving the <see cref="Config"/> to disk if the
        /// <see cref="MenuAttribute.SaveEvents.ChangeValue"/> flag is set, before passing off to
        /// <see cref="InvokeOnChange{TEventArgs}(ModOptionMetadata, object, TEventArgs)"/> to invoke any methods specified with a
        /// <see cref="OnChangeAttribute"/>.
        /// </summary>
        /// <param name="sender">The sender of the original keybind changed event.</param>
        /// <param name="e">The <see cref="KeybindChangedEventArgs"/> for the keybind changed event.</param>
        private void ConfigModOptions_KeybindChanged(object sender, KeybindChangedEventArgs e)
        {
            if (modOptionsMetadata.TryGetValue(e.Id, out var modOptionMetadata))
            {
                // Set the value in the Config
                switch (modOptionMetadata.MemberInfo)
                {
                    case PropertyInfo property:
                        property.SetValue(Config, e.Key, null);
                        break;
                    case FieldInfo field:
                        field.SetValue(Config, e.Key);
                        break;
                }

                // Optionally save the Config to disk
                if (menuAttribute.SaveOn.HasFlag(MenuAttribute.SaveEvents.ChangeValue))
                    Config.Save();

                // Invoke any OnChange methods specified
                InvokeOnChange(modOptionMetadata, sender, e);
            }
        }

        /// <summary>
        /// Sets the value in the <see cref="Config"/>, optionally saving the <see cref="Config"/> to disk if the
        /// <see cref="MenuAttribute.SaveEvents.ChangeValue"/> flag is set, before passing off to
        /// <see cref="InvokeOnChange{TEventArgs}(ModOptionMetadata, object, TEventArgs)"/> to invoke any methods specified with a
        /// <see cref="OnChangeAttribute"/>.
        /// </summary>
        /// <param name="sender">The sender of the original slider changed event.</param>
        /// <param name="e">The <see cref="SliderChangedEventArgs"/> for the slider changed event.</param>
        private void ConfigModOptions_SliderChanged(object sender, SliderChangedEventArgs e)
        {
            if (modOptionsMetadata.TryGetValue(e.Id, out var modOptionMetadata))
            {
                // Set the value in the Config
                switch (modOptionMetadata.MemberInfo)
                {
                    case PropertyInfo property:
                        property.SetValue(Config, Convert.ChangeType(e.Value, property.PropertyType), null);
                        break;
                    case FieldInfo field:
                        field.SetValue(Config, Convert.ChangeType(e.Value, field.FieldType));
                        break;
                }

                // Optionally save the Config to disk
                if (menuAttribute.SaveOn.HasFlag(MenuAttribute.SaveEvents.ChangeValue))
                    Config.Save();

                // Invoke any OnChange methods specified
                InvokeOnChange(modOptionMetadata, sender, e);
            }
        }

        /// <summary>
        /// Sets the value in the <see cref="Config"/>, optionally saving the <see cref="Config"/> to disk if the
        /// <see cref="MenuAttribute.SaveEvents.ChangeValue"/> flag is set, before passing off to
        /// <see cref="InvokeOnChange{TEventArgs}(ModOptionMetadata, object, TEventArgs)"/> to invoke any methods specified with a
        /// <see cref="OnChangeAttribute"/>.
        /// </summary>
        /// <param name="sender">The sender of the original toggle changed event.</param>
        /// <param name="e">The <see cref="ToggleChangedEventArgs"/> for the toggle changed event.</param>
        private void ConfigModOptions_ToggleChanged(object sender, ToggleChangedEventArgs e)
        {
            if (modOptionsMetadata.TryGetValue(e.Id, out var modOptionMetadata))
            {
                // Set the value in the Config
                switch (modOptionMetadata.MemberInfo)
                {
                    case PropertyInfo property:
                        property.SetValue(Config, e.Value, null);
                        break;
                    case FieldInfo field:
                        field.SetValue(Config, e.Value);
                        break;
                }

                // Optionally save the Config to disk
                if (menuAttribute.SaveOn.HasFlag(MenuAttribute.SaveEvents.ChangeValue))
                    Config.Save();

                // Invoke any OnChange methods specified
                InvokeOnChange(modOptionMetadata, sender, e);
            }
        }

        /// <summary>
        /// Invokes the relevant method(s) specified with <see cref="OnChangeAttribute"/>(s)
        /// and passes parameters when a value is changed.
        /// </summary>
        /// <typeparam name="TEventArgs">The type of the original event args.</typeparam>
        /// <param name="modOptionMetadata"></param>
        /// <param name="sender">The sender of the original event.</param>
        /// <param name="e">The <typeparamref name="TEventArgs"/> for the original changed event.</param>
        private void InvokeOnChange<TEventArgs>(ModOptionMetadata modOptionMetadata, object sender, TEventArgs e)
            where TEventArgs : IModOptionEventArgs
        {
            if (modOptionMetadata.OnChangeAttributes == null)
                return; // Skip attempting to invoke events if there are no OnChangeAttributes set for the member.

            // cache types used in comparisons
            var objectType = typeof(object);
            var eventArgsType = typeof(TEventArgs);
            var genericModOptionEventArgsType = typeof(IModOptionEventArgs);

            foreach (var onChangeAttribute in modOptionMetadata.OnChangeAttributes)
            {
                var method = modOptionMetadata.MemberInfo.DeclaringType.GetMethod(onChangeAttribute.MethodName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

                if (method == null)
                {   // Method not found, log error and skip.
                    Logger.Error($"[ConfigModOptions] Could not find the specified OnChange method: " +
                        $"{onChangeAttribute.MethodName} in class {Config.GetType()}");
                    continue;
                }

                var parameters = method.GetParameters();
                var invokeParams = new object[parameters.Length];
                var senderFound = false;
                var eventArgsFound = false;
                var modOptionEventFound = false;
                for (var i = 0; i < parameters.Length; i++)
                {
                    if (senderFound && eventArgsFound && modOptionEventFound)
                        break;

                    var param = parameters[i];
                    if (!senderFound && param.ParameterType == objectType)
                    {
                        senderFound = true;
                        invokeParams[i] = sender;
                        continue;
                    }

                    if (!eventArgsFound && param.ParameterType == eventArgsType)
                    {
                        eventArgsFound = true;
                        invokeParams[i] = e;
                    }

                    if (!modOptionEventFound && param.ParameterType == genericModOptionEventArgsType)
                    {
                        modOptionEventFound = true;
                        invokeParams[i] = e;
                    }
                }

                method.Invoke(Config, invokeParams);
            }
        }

        /// <summary>
        /// Generates tooltips for each <see cref="ModOption"/> with a specified <see cref="TooltipAttribute"/>, before
        /// invoking any relevant method(s) specified with <see cref="OnGameObjectCreatedAttribute"/>(s) and passes
        /// parameters when a <see cref="GameObject"/> is created in the options menu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConfigModOptions_GameObjectCreated(object sender, GameObjectCreatedEventArgs e)
        {
            if (modOptionsMetadata.TryGetValue(e.Id, out var modOptionMetadata))
            {
                // Create a tooltip if there is a TooltipAttribute specified
                if (modOptionMetadata.TooltipAttribute != null)
                    e.GameObject.GetComponentInChildren<Text>().gameObject.AddComponent<ModOptionTooltip>().Tooltip
                        = modOptionMetadata.TooltipAttribute.Tooltip;

                if (modOptionMetadata.OnGameObjectCreatedAttributes == null)
                    return; // Skip attempting to invoke events if there are no OnGameObjectCreatedAttributes set for the member.

                // cache types used in comparisons
                var objectType = typeof(object);
                var gameObjectCreatedEventArgsType = typeof(GameObjectCreatedEventArgs);
                var genericModOptionEventArgsType = typeof(IModOptionEventArgs);

                foreach (var onGameObjectCreatedAttribute in modOptionMetadata.OnGameObjectCreatedAttributes)
                {
                    var method = modOptionMetadata.MemberInfo.DeclaringType.GetMethod(onGameObjectCreatedAttribute.MethodName,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

                    if (method == null)
                    {   // Method not found, log error and skip.
                        Logger.Error($"[ConfigModOptions] Could not find the specified OnGameObjectCreated method: " +
                            $"{onGameObjectCreatedAttribute.MethodName} in class {Config.GetType()}");
                        continue;
                    }

                    var parameters = method.GetParameters();
                    var invokeParams = new object[parameters.Length];
                    var senderFound = false;
                    var eventArgsFound = false;
                    var modOptionEventFound = false;
                    for (var i = 0; i < parameters.Length; i++)
                    {
                        if (senderFound && eventArgsFound && modOptionEventFound)
                            break;

                        var param = parameters[i];
                        if (!senderFound && param.ParameterType == objectType)
                        {
                            senderFound = true;
                            invokeParams[i] = sender;
                            continue;
                        }

                        if (!eventArgsFound && param.ParameterType == gameObjectCreatedEventArgsType)
                        {
                            eventArgsFound = true;
                            invokeParams[i] = e;
                        }

                        if (!modOptionEventFound && param.ParameterType == genericModOptionEventArgsType)
                        {
                            modOptionEventFound = true;
                            invokeParams[i] = e;
                        }
                    }

                    method.Invoke(Config, invokeParams);
                }
            }
        }

        /// <summary>
        /// Adds options to the menu based on the <see cref="modOptionsMetadata"/> dictionary, generated by the constructor.
        /// </summary>
        public override void BuildModOptions()
        {
            foreach (var entry in modOptionsMetadata.OrderBy(x => x.Value.LabelAttribute.Order).ThenBy(x => x.Value.MemberInfo.Name))
            {
                var id = entry.Key;
                var modOptionMetadata = entry.Value;

                Logger.Debug($"[ConfigModOptions] {modOptionMetadata.MemberInfo.Name}: {modOptionMetadata.ModOptionType}");
                Logger.Debug($"[ConfigModOptions] Label: {modOptionMetadata.LabelAttribute?.Label}");

                var label = modOptionMetadata.LabelAttribute.Label;
                if (modOptionMetadata.ModOptionType == typeof(ModButtonOption))
                    // Just add the button to the menu, easy
                    AddButtonOption(id, label);
                else if (modOptionMetadata.ModOptionType == typeof(ModChoiceOption))
                {   // Parse the metadata for the ModChoiceOption and add to menu
                    object value;
                    Type type;

                    // get value and type
                    switch (modOptionMetadata.MemberInfo)
                    {
                        case PropertyInfo property:
                            value = property.GetValue(Config, null);
                            type = property.PropertyType;
                            break;
                        case FieldInfo field:
                            value = field.GetValue(Config);
                            type = field.FieldType;
                            break;
                        default: continue;
                    }

                    if (type.IsEnum) // Add enum-based choice
                    {
                        if (modOptionMetadata.ChoiceAttribute is ChoiceAttribute choiceAttribute)
                            // Display custom strings 
                            AddChoiceOption(id, label, choiceAttribute.Options,
                                Array.IndexOf(Enum.GetValues(type), value));
                        else
                            // Display strings generated from the enum
                            AddChoiceOption(id, label, Enum.GetNames(type), value.ToString());
                    }
                    else if (type == typeof(string)) // Add string-based choice
                        AddChoiceOption(id, label, modOptionMetadata.ChoiceAttribute.Options, value);
                    else // Add index-based choice
                        AddChoiceOption(id, label, modOptionMetadata.ChoiceAttribute.Options, Convert.ToInt32(value));
                }
                else if (modOptionMetadata.ModOptionType == typeof(ModKeybindOption))
                {   // Parse the metadata for the ModKeybindOption and add to menu
                    object value;
                    switch (modOptionMetadata.MemberInfo)
                    {
                        case PropertyInfo property:
                            value = property.GetValue(Config, null);
                            break;
                        case FieldInfo field:
                            value = field.GetValue(Config);
                            break;
                        default: continue;
                    }

                    AddKeybindOption(id, label, GameInput.Device.Keyboard, (KeyCode)value);
                }
                else if (modOptionMetadata.ModOptionType == typeof(ModSliderOption))
                {   // Parse the metadata for the ModSliderOption and add to menu
                    object value;
                    Type type;

                    switch (modOptionMetadata.MemberInfo)
                    {
                        case PropertyInfo property:
                            value = property.GetValue(Config, null);
                            type = property.PropertyType;
                            break;
                        case FieldInfo field:
                            value = field.GetValue(Config);
                            type = field.FieldType;
                            break;
                        default: continue;
                    }

                    AddSliderOption(id, label, modOptionMetadata.SliderAttribute.Min, modOptionMetadata.SliderAttribute.Max,
                        Convert.ToSingle(value), modOptionMetadata.SliderAttribute.DefaultValue,
                        modOptionMetadata.SliderAttribute.Format, modOptionMetadata.SliderAttribute.Step);
                }
                else if (modOptionMetadata.ModOptionType == typeof(ModToggleOption))
                {   // Parse the metadata for the ModToggleOption and add to menu
                    object value;
                    switch (modOptionMetadata.MemberInfo)
                    {
                        case PropertyInfo property:
                            value = property.GetValue(Config, null);
                            break;
                        case FieldInfo field:
                            value = field.GetValue(Config);
                            break;
                        default: continue;
                    }

                    AddToggleOption(id, label, (bool)value);
                }
            }
        }
    }
}
