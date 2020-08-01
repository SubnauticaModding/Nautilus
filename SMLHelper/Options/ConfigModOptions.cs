namespace SMLHelper.V2.Options
{
    using Interfaces;
    using Json;
    using Oculus.Newtonsoft.Json.Bson;
    using QModManager.API;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;
    using Logger = Logger;
#if SUBNAUTICA
    using Text = UnityEngine.UI.Text;
#elif BELOWZERO
    using Text = TMPro.TextMeshProUGUI;
#endif

    /// <summary>
    /// An internal derivative of <see cref="ModOptions"/> for use in auto-generating a menu based on attributes
    /// declared in a <see cref="ConfigFile"/>.
    /// </summary>
    /// <typeparam name="T">The type of the class derived from <see cref="ConfigFile"/> to use for
    /// loading to/saving from disk.</typeparam>
    internal class ConfigModOptions<T> : ModOptions where T : ConfigFile, new()
    {
        /// <summary>
        /// The <typeparamref name="T"/> <see cref="ConfigFile"/> instance related to this <see cref="ModOptions"/> menu.
        /// </summary>
        public T Config { get; }

        /// <summary>
        /// Instantiates a new <see cref="ConfigModOptions{T}"/>, generating <see cref="ModOption"/>s by parsing the fields,
        /// properties and methods declared in the class.
        /// </summary>
        public ConfigModOptions() : base(null)
        {
            // Instantiate and load the config
            Config = new T();
            Config.Load();

            menuAttribute = GetMenuAttributeOrDefault();
            Name = menuAttribute.Name;

            ProcessMembers();
            BindEvents();
        }

        #region Metadata Processing
        /// <summary>
        /// Process metadata for members of <typeparamref name="T"/>.
        /// </summary>
        private void ProcessMembers()
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public;
            foreach (PropertyInfo property in typeof(T).GetProperties(bindingFlags)
                .Where(MemberIsDeclaredInConfigFileSubclass) // Only care about members declared in a subclass of ConfigFile
                .Where(MemberIsNotIgnored)) // Filter out explicitly ignored members
            {
                ProcessFieldOrProperty(property, property.PropertyType);
            }

            foreach (FieldInfo field in typeof(T).GetFields(bindingFlags)
                .Where(MemberIsDeclaredInConfigFileSubclass) // Only care about members declared in a subclass of ConfigFile
                .Where(MemberIsNotIgnored)) // Filter out explicitly ignored members
            {
                ProcessFieldOrProperty(field, field.FieldType);
            }

            foreach (MethodInfo method in typeof(T).GetMethods(bindingFlags | BindingFlags.NonPublic | BindingFlags.Static)
                .Where(MemberIsDeclaredInConfigFileSubclass) // Only care about members declared in a subclass of ConfigFile
                .Where(MemberIsNotIgnored)) // Filter out explicitly ignored members
            {
                ProcessMethod(method);
            }

            Logger.Debug($"[ConfigModOptions] Found {modOptionsMetadata.Count()} options to add to the menu.");
        }

        /// <summary>
        /// The <see cref="MenuAttribute"/> relating to this <see cref="ModOptions"/> menu.
        /// </summary>
        private readonly MenuAttribute menuAttribute;

        /// <summary>
        /// Gets the <see cref="MenuAttribute"/> defined on the <typeparamref name="T"/>, or
        /// a default <see cref="MenuAttribute"/> with its name automatically parsed from the Display Name
        /// of the QMod that <typeparamref name="T"/> is defined in.
        /// </summary>
        /// <returns></returns>
        private MenuAttribute GetMenuAttributeOrDefault()
        {
            if (Attribute.IsDefined(typeof(T), typeof(MenuAttribute), true))
                return typeof(T).GetCustomAttributes(typeof(MenuAttribute), true).SingleOrDefault() as MenuAttribute;
            else
                return new MenuAttribute(QModServices.Main.GetMod(Assembly.GetAssembly(typeof(T))).DisplayName);
        }

        /// <summary>
        /// A simple struct containing metadata for each auto-generated <see cref="ModOption"/>.
        /// </summary>
        private struct ModOptionMetadata
        {
            public ModOptionAttribute ModOptionAttribute;
            public MemberInfo MemberInfo;
            public OnChangeAttribute[] OnChangeAttributes;
            public OnGameObjectCreatedAttribute[] OnGameObjectCreatedAttributes;
        }

        /// <summary>
        /// A dictionary of <see cref="ModOptionMetadata"/>, indexed by <see cref="ModOption.Id"/>.
        /// </summary>
        private readonly Dictionary<string, ModOptionMetadata> modOptionsMetadata
            = new Dictionary<string, ModOptionMetadata>();

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
        {
            if (Attribute.IsDefined(memberInfo, typeof(IgnoreMemberAttribute)))
            {
                return false;
            }
            else if (!menuAttribute.IgnoreUnattributedMembers)
            {
                return true;
            }
            else
            {
                return Attribute.IsDefined(memberInfo, typeof(ModOptionAttribute), true) ||
                    Attribute.IsDefined(memberInfo, typeof(ModOptionEventAttribute), true);
            }
        }

        /// <summary>
        /// Processes the given field or property and hands off to <see cref="AddModOptionMetadata{TAttribute}(MemberInfo)"/>
        /// to generate a <see cref="ModOptionMetadata"/> and add it to the <see cref="modOptionsMetadata"/> dictionary.
        /// </summary>
        /// <param name="memberInfo">The <see cref="MemberInfo"/> of the member.</param>
        /// <param name="memberType">The underlying <see cref="Type"/> of the member.</param>
        private void ProcessFieldOrProperty(MemberInfo memberInfo, Type memberType)
        {
            if (memberType == typeof(bool))
            {
                AddModOptionMetadata<ToggleAttribute>(memberInfo);
            }
            else if (memberType == typeof(KeyCode))
            {
                AddModOptionMetadata<KeybindAttribute>(memberInfo);
            }
            else if (memberType.IsEnum || Attribute.IsDefined(memberInfo, typeof(ChoiceAttribute), true))
            {
                AddModOptionMetadata<ChoiceAttribute>(memberInfo);
            }
            else if (memberType == typeof(float) ||
                    memberType == typeof(double) ||
                    memberType == typeof(int) ||
                    Attribute.IsDefined(memberInfo, typeof(SliderAttribute), true))
            {
                AddModOptionMetadata<SliderAttribute>(memberInfo);
            }
        }

        /// <summary>
        /// Processes the given method and hands off to <see cref="AddModOptionMetadata{TAttribute}(MemberInfo)"/>
        /// to generate a <see cref="ModOptionMetadata"/> and add it to the <see cref="modOptionsMetadata"/> dictionary.
        /// </summary>
        /// <param name="methodInfo">The <see cref="MethodInfo"/> of the method.</param>
        private void ProcessMethod(MethodInfo methodInfo)
        {
            AddModOptionMetadata<ButtonAttribute>(methodInfo);
        }

        /// <summary>
        /// Generates a <see cref="ModOptionMetadata"/> based on the member and its attributes, then adds it to the
        /// <see cref="modOptionsMetadata"/> dictionary.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the <see cref="ModOption"/> to generate for this member.</typeparam>
        /// <param name="memberInfo">The <see cref="MemberInfo"/> of the member.</param>
        private void AddModOptionMetadata<TAttribute>(MemberInfo memberInfo)
            where TAttribute : ModOptionAttribute, new()
        {
            try
            {
                // Get the ModOptionAttribute
                var modOptionAttribute = memberInfo.GetCustomAttributes(typeof(ModOptionAttribute), true)
                    .SingleOrDefault() as ModOptionAttribute ?? new TAttribute();

                // If there is no label specified, just use the member's name.
                if (string.IsNullOrEmpty(modOptionAttribute.Label))
                    modOptionAttribute.Label = memberInfo.Name;

                // ModOptionMetadata needed for all ModOptions
                var modOptionMetadata = new ModOptionMetadata
                {
                    ModOptionAttribute = modOptionAttribute,
                    MemberInfo = memberInfo,
                    OnGameObjectCreatedAttributes = memberInfo.GetCustomAttributes(typeof(OnGameObjectCreatedAttribute), true)
                        .Select(o => o as OnGameObjectCreatedAttribute).ToArray()
                };

                if (typeof(TAttribute) != typeof(ButtonAttribute))
                {
                    // Get the OnChange attributes
                    modOptionMetadata.OnChangeAttributes = memberInfo.GetCustomAttributes(typeof(OnChangeAttribute), true)
                        .Select(o => o as OnChangeAttribute).ToArray();
                }

                modOptionsMetadata.Add(modOptionAttribute.Id, modOptionMetadata);
            }
            catch (Exception ex)
            {
                Logger.Error($"[ConfigModOptions] {ex.Message}");
            }
        }
        #endregion

        #region Click, Change and GameObjectCreated Events
        /// <summary>
        /// Conditionally binds events
        /// </summary>
        private void BindEvents()
        {
            if (modOptionsMetadata.Values.Any(x => x.ModOptionAttribute is ButtonAttribute))
                ButtonClicked += ConfigModOptions_ButtonClicked;

            if (modOptionsMetadata.Values.Any(x => x.ModOptionAttribute is ChoiceAttribute))
                ChoiceChanged += ConfigModOptions_ChoiceChanged;

            if (modOptionsMetadata.Values.Any(x => x.ModOptionAttribute is KeybindAttribute))
                KeybindChanged += ConfigModOptions_KeybindChanged;

            if (modOptionsMetadata.Values.Any(x => x.ModOptionAttribute is SliderAttribute))
                SliderChanged += ConfigModOptions_SliderChanged;

            if (modOptionsMetadata.Values.Any(x => x.ModOptionAttribute is ToggleAttribute))
                ToggleChanged += ConfigModOptions_ToggleChanged;

            if (modOptionsMetadata.Values.Any(x => x.ModOptionAttribute.Tooltip != null
                || (x.OnGameObjectCreatedAttributes != null && x.OnGameObjectCreatedAttributes.Any())))
                GameObjectCreated += ConfigModOptions_GameObjectCreated;
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

                    if (senderFound && eventArgsFound)
                    {
                        break;
                    }
                }

                methodInfo.Invoke(Config, invokeParams);
            }
        }

        /// <summary>
        /// Sets the value in the <see cref="Config"/>, optionally saving the <see cref="Config"/> to disk if the
        /// <see cref="MenuAttribute.SaveEvents.ChangeValue"/> flag is set, before passing off to
        /// <see cref="InvokeOnChange{TSource}(ModOptionMetadata, object, TSource)"/> to invoke any methods specified with a
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
                    case PropertyInfo property when property.PropertyType.IsEnum && modOptionMetadata.ModOptionAttribute is ChoiceAttribute:
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
                    case FieldInfo field when field.FieldType.IsEnum && modOptionMetadata.ModOptionAttribute is ChoiceAttribute:
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
        /// <see cref="InvokeOnChange{TSource}(ModOptionMetadata, object, TSource)"/> to invoke any methods specified with a
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
        /// <see cref="InvokeOnChange{TSource}(ModOptionMetadata, object, TSource)"/> to invoke any methods specified with a
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
        /// <see cref="InvokeOnChange{TSource}(ModOptionMetadata, object, TSource)"/> to invoke any methods specified with a
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
        /// <typeparam name="TSource">The type of the original event args.</typeparam>
        /// <param name="modOptionMetadata"></param>
        /// <param name="sender">The sender of the original event.</param>
        /// <param name="e">The <typeparamref name="TSource"/> for the original changed event.</param>
        private void InvokeOnChange<TSource>(ModOptionMetadata modOptionMetadata, object sender, TSource e)
            where TSource : IModOptionEventArgs
        {
            if (modOptionMetadata.OnChangeAttributes == null)
                return; // Skip attempting to invoke events if there are no OnChangeAttributes set for the member.

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
                    var param = parameters[i];
                    if (!senderFound && param.ParameterType == typeof(object))
                    {
                        senderFound = true;
                        invokeParams[i] = sender;
                        continue;
                    }

                    if (!eventArgsFound && param.ParameterType == typeof(TSource))
                    {
                        eventArgsFound = true;
                        invokeParams[i] = e;
                    }

                    if (!modOptionEventFound && param.ParameterType == typeof(IModOptionEventArgs))
                    {
                        modOptionEventFound = true;
                        invokeParams[i] = e;
                    }

                    if (senderFound && eventArgsFound && modOptionEventFound)
                        break;
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
                if (modOptionMetadata.ModOptionAttribute.Tooltip is string tooltip)
                {
                    e.GameObject.GetComponentInChildren<Text>().gameObject.AddComponent<ModOptionTooltip>().Tooltip = tooltip;
                }

                if (modOptionMetadata.OnGameObjectCreatedAttributes == null)
                    return; // Skip attempting to invoke events if there are no OnGameObjectCreatedAttributes set for the member.

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
                        var param = parameters[i];
                        if (!senderFound && param.ParameterType == typeof(object))
                        {
                            senderFound = true;
                            invokeParams[i] = sender;
                            continue;
                        }

                        if (!eventArgsFound && param.ParameterType == typeof(GameObjectCreatedEventArgs))
                        {
                            eventArgsFound = true;
                            invokeParams[i] = e;
                        }

                        if (!modOptionEventFound && param.ParameterType == typeof(IModOptionEventArgs))
                        {
                            modOptionEventFound = true;
                            invokeParams[i] = e;
                        }

                        if (senderFound && eventArgsFound && modOptionEventFound)
                        {
                            break;
                        }
                    }

                    method.Invoke(Config, invokeParams);
                }
            }
        }
        #endregion

        #region Build ModOptions Menu
        /// <summary>
        /// Adds options to the menu based on the <see cref="modOptionsMetadata"/> dictionary, generated by the constructor.
        /// </summary>
        public override void BuildModOptions()
        {
            foreach (var entry in modOptionsMetadata
                .OrderBy(x => x.Value.ModOptionAttribute.Order)
                .ThenBy(x => x.Value.MemberInfo.Name))
            {
                var id = entry.Key;
                var modOptionMetadata = entry.Value;

                Logger.Debug($"[ConfigModOptions] {modOptionMetadata.MemberInfo.Name}: " +
                    $"{modOptionMetadata.ModOptionAttribute.GetType()}");
                Logger.Debug($"[ConfigModOptions] Label: {modOptionMetadata.ModOptionAttribute.Label}");

                var label = modOptionMetadata.ModOptionAttribute.Label;

                if (modOptionMetadata.ModOptionAttribute is ButtonAttribute)
                {
                    BuildModButtonOption(id, label);
                }
                else if (modOptionMetadata.ModOptionAttribute is ChoiceAttribute choiceAttribute)
                {
                    BuildModChoiceOption(id, label, modOptionMetadata.MemberInfo, choiceAttribute);
                }
                else if (modOptionMetadata.ModOptionAttribute is KeybindAttribute)
                {
                    BuildModKeybindOption(id, label, modOptionMetadata.MemberInfo);
                }
                else if (modOptionMetadata.ModOptionAttribute is SliderAttribute sliderAttribute)
                {
                    BuildModSliderOption(id, label, modOptionMetadata.MemberInfo, sliderAttribute);
                }
                else if (modOptionMetadata.ModOptionAttribute is ToggleAttribute)
                {
                    BuildModToggleOption(id, label, modOptionMetadata.MemberInfo);
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
        /// <param name="memberInfo">The <see cref="MemberInfo"/> of the corresponding member.</param>
        /// <param name="choiceAttribute">The defined or generated <see cref="ChoiceAttribute"/> of the member.</param>
        private void BuildModChoiceOption(string id, string label, MemberInfo memberInfo, ChoiceAttribute choiceAttribute)
        {
            // Parse the metadata for the ModChoiceOption and add to menu
            object value;
            Type type;

            // get value and type
            switch (memberInfo)
            {
                case PropertyInfo property:
                    value = property.GetValue(Config, null);
                    type = property.PropertyType;
                    break;
                case FieldInfo field:
                    value = field.GetValue(Config);
                    type = field.FieldType;
                    break;
                default: return;
            }

            if (type.IsEnum) // Add enum-based choice
            {
                if (choiceAttribute.Options != null && choiceAttribute.Options.Any())
                {   // Display custom strings 
                    AddChoiceOption(id, label, choiceAttribute.Options,
                        Array.IndexOf(Enum.GetValues(type), value));
                }
                else
                {   // Display strings generated from the enum
                    AddChoiceOption(id, label, Enum.GetNames(type), value.ToString());
                }
            }
            else if (type == typeof(string))
            {   // Add string-based choice
                AddChoiceOption(id, label, choiceAttribute.Options, value);
            }
            else
            {   // Add index-based choice
                AddChoiceOption(id, label, choiceAttribute.Options, Convert.ToInt32(value));
            }
        }

        /// <summary>
        /// Adds a <see cref="ModKeybindOption"/> to the <see cref="ModOptions"/> menu.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="label"></param>
        /// <param name="memberInfo">The <see cref="MemberInfo"/> of the corresponding member.</param>
        private void BuildModKeybindOption(string id, string label, MemberInfo memberInfo)
        {
            // Parse the metadata for the ModKeybindOption and add to menu
            object value;
            switch (memberInfo)
            {
                case PropertyInfo property:
                    value = property.GetValue(Config, null);
                    break;
                case FieldInfo field:
                    value = field.GetValue(Config);
                    break;
                default: return;
            }

            AddKeybindOption(id, label, GameInput.Device.Keyboard, (KeyCode)value);
        }

        /// <summary>
        /// Adds a <see cref="ModSliderOption"/> to the <see cref="ModOptions"/> menu.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="label"></param>
        /// <param name="memberInfo">The <see cref="MemberInfo"/> of the corresponding member.</param>
        /// <param name="sliderAttribute">The defined or generated <see cref="SliderAttribute"/> of the member.</param>
        private void BuildModSliderOption(string id, string label, MemberInfo memberInfo, SliderAttribute sliderAttribute)
        {
            // Parse the metadata for the ModSliderOption and add to menu
            object value;
            Type type;

            switch (memberInfo)
            {
                case PropertyInfo property:
                    value = property.GetValue(Config, null);
                    type = property.PropertyType;
                    break;
                case FieldInfo field:
                    value = field.GetValue(Config);
                    type = field.FieldType;
                    break;
                default: return;
            }

            AddSliderOption(id, label, sliderAttribute.Min, sliderAttribute.Max,
                Convert.ToSingle(value), sliderAttribute.DefaultValue,
                sliderAttribute.Format, sliderAttribute.Step);
        }

        /// <summary>
        /// Adds a <see cref="ModToggleOption"/> to the <see cref="ModOptions"/> menu.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="label"></param>
        /// <param name="memberInfo">The <see cref="MemberInfo"/> of the corresponding member.</param>
        private void BuildModToggleOption(string id, string label, MemberInfo memberInfo)
        {
            // Parse the metadata for the ModToggleOption and add to menu
            object value;
            switch (memberInfo)
            {
                case PropertyInfo property:
                    value = property.GetValue(Config, null);
                    break;
                case FieldInfo field:
                    value = field.GetValue(Config);
                    break;
                default: return;
            }

            AddToggleOption(id, label, (bool)value);
        }
        #endregion
    }
}
