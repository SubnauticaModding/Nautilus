namespace SMLHelper.V2.Options.Attributes
{
    using Interfaces;
    using Json;
    using QModManager.API;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;
    using Logger = Logger;
#if SUBNAUTICA
    using Text = UnityEngine.UI.Text;
#elif BELOWZERO
    using Text = TMPro.TextMeshProUGUI;
#endif
#if SUBNAUTICA_STABLE
    using Oculus.Newtonsoft.Json;
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
        /// <summary>
        /// The <typeparamref name="T"/> <see cref="ConfigFile"/> instance related to this <see cref="ModOptions"/> menu.
        /// </summary>
        public T Config { get; }

        private Assembly assembly => Assembly.GetAssembly(typeof(T));
        private IQMod QMod { get; }

        /// <summary>
        /// Instantiates a new <see cref="OptionsMenuBuilder{T}"/>, generating <see cref="ModOption"/>s by parsing the fields,
        /// properties and methods declared in the class.
        /// </summary>
        public OptionsMenuBuilder() : base(null)
        {
            QMod = QModServices.Main.GetMod(assembly);

            Config = new T();

            BindEvents();

            LoadMetadata();

            // Conditionally load the config
            if (menuAttribute.LoadOn.HasFlag(MenuAttribute.LoadEvents.MenuRegistered))
                Config.Load();
        }

        #region Metadata Processing
        /// <summary>
        /// The <see cref="MenuAttribute"/> relating to this <see cref="ModOptions"/> menu.
        /// </summary>
        private MenuAttribute menuAttribute;

        /// <summary>
        /// A dictionary of <see cref="ModOptionMetadata{T}"/>, indexed by <see cref="ModOption.Id"/>.
        /// </summary>
        public Dictionary<string, ModOptionMetadata<T>> modOptionsMetadata;

        private void LoadMetadata()
        {
            Stopwatch stopwatch = new Stopwatch();

            if (Logger.EnableDebugging)
                stopwatch.Start();

            menuAttribute = GetMenuAttributeOrDefault();
            modOptionsMetadata = new Dictionary<string, ModOptionMetadata<T>>();

            Name = menuAttribute.Name;

            ProcessMetadata();

            if (Logger.EnableDebugging)
            {
                stopwatch.Stop();
                Logger.Debug($"[{QMod.DisplayName}] OptionsMenuBuilder metadata parsed via reflection in {stopwatch.ElapsedMilliseconds}ms.");
            }
        }

        /// <summary>
        /// Process metadata for members of <typeparamref name="T"/>.
        /// </summary>
        private void ProcessMetadata()
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public;
            foreach (PropertyInfo property in typeof(T).GetProperties(bindingFlags)
                .Where(MemberIsDeclaredInConfigFileSubclass) // Only care about members declared in a subclass of ConfigFile
                .Where(MemberIsNotIgnored)) // Filter out explicitly ignored members
            {
                ProcessFieldOrProperty(property, MemberType.Property, property.PropertyType);
            }

            foreach (FieldInfo field in typeof(T).GetFields(bindingFlags)
                .Where(MemberIsDeclaredInConfigFileSubclass) // Only care about members declared in a subclass of ConfigFile
                .Where(MemberIsNotIgnored)) // Filter out explicitly ignored members
            {
                ProcessFieldOrProperty(field, MemberType.Field, field.FieldType);
            }

            foreach (MethodInfo method in typeof(T).GetMethods(bindingFlags | BindingFlags.Static)
                .Where(MemberIsDeclaredInConfigFileSubclass) // Only care about members declared in a subclass of ConfigFile
                .Where(MemberIsNotIgnored)) // Filter out explicitly ignored members
            {
                ProcessMethod(method);
            }

            Logger.Debug($"[OptionsMenuBuilder] Found {modOptionsMetadata.Count()} options to add to the menu.");
        }

        /// <summary>
        /// Gets the <see cref="Attributes.MenuAttribute"/> defined on the <typeparamref name="T"/>, or
        /// a default <see cref="Attributes.MenuAttribute"/> with its name automatically parsed from the Display Name
        /// of the QMod that <typeparamref name="T"/> is defined in.
        /// </summary>
        /// <returns></returns>
        private MenuAttribute GetMenuAttributeOrDefault()
        {
            return typeof(T).GetCustomAttribute<MenuAttribute>(true) ?? new MenuAttribute(QMod.DisplayName);
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
        /// <see cref="MenuAttribute.MemberProcessing"/> property.
        /// </summary>
        /// <param name="memberInfo">The <see cref="MemberInfo"/> to check.</param>
        /// <returns>Whether the given <see cref="MemberInfo"/> member should be ignored when generating the options menu.</returns>
        private bool MemberIsNotIgnored(MemberInfo memberInfo)
        {
            if (Attribute.IsDefined(memberInfo, typeof(IgnoreMemberAttribute)))
                return false;

            switch (menuAttribute.MemberProcessing)
            {
                case MenuAttribute.Members.OptOut:
                    if (memberInfo is MethodInfo)
                    {
                        if (Attribute.IsDefined(memberInfo, typeof(ButtonAttribute), true))
                            return true;

                        IEnumerable<MemberInfoMetadata<T>> eventMetadatas
                            = modOptionsMetadata.Values.SelectMany(modOptionsMetadata =>
                            {
                                IEnumerable<MemberInfoMetadata<T>> result = new List<MemberInfoMetadata<T>>();

                                if (modOptionsMetadata.OnChangeMetadata != null)
                                    result.Concat(modOptionsMetadata.OnChangeMetadata);

                                if (modOptionsMetadata.OnGameObjectCreatedMetadata != null)
                                    result.Concat(modOptionsMetadata.OnGameObjectCreatedMetadata);

                                return result;
                            });
                        return eventMetadatas.Any(memberInfoMetadata => memberInfoMetadata.Name == memberInfo.Name);
                    }
                    return true;

                default:
                case MenuAttribute.Members.OptIn:
                    return Attribute.IsDefined(memberInfo, typeof(ModOptionAttribute), true) ||
                        Attribute.IsDefined(memberInfo, typeof(ModOptionEventAttribute), true);
            }
        }

        /// <summary>
        /// Processes the given field or property and hands off to
        /// <see cref="AddModOptionMetadata{TAttribute}(MemberInfo, MemberType, Type)"/> to generate a <see cref="ModOptionMetadata{T}"/>
        /// and add it to the <see cref="modOptionsMetadata"/> dictionary.
        /// </summary>
        /// <param name="memberInfo">The <see cref="MemberInfo"/> of the member.</param>
        /// <param name="memberType">The <see cref="MemberType"/> of the member.</param>
        /// <param name="underlyingType">The underlying <see cref="Type"/> of the member.</param>
        private void ProcessFieldOrProperty(MemberInfo memberInfo, MemberType memberType, Type underlyingType)
        {
            if (underlyingType == typeof(bool))
            {
                AddModOptionMetadata<ToggleAttribute>(memberInfo, memberType, underlyingType);
            }
            else if (underlyingType == typeof(KeyCode))
            {
                AddModOptionMetadata<KeybindAttribute>(memberInfo, memberType, underlyingType);
            }
            else if (underlyingType.IsEnum || Attribute.IsDefined(memberInfo, typeof(ChoiceAttribute), true))
            {
                AddModOptionMetadata<ChoiceAttribute>(memberInfo, memberType, underlyingType);
            }
            else if (underlyingType == typeof(float) ||
                    underlyingType == typeof(double) ||
                    underlyingType == typeof(int) ||
                    Attribute.IsDefined(memberInfo, typeof(SliderAttribute), true))
            {
                AddModOptionMetadata<SliderAttribute>(memberInfo, memberType, underlyingType);
            }
        }

        /// <summary>
        /// Processes the given method and hands off to <see cref="AddModOptionMetadata{TAttribute}(MemberInfo, MemberType, Type)"/>
        /// to generate a <see cref="ModOptionMetadata{T}"/> and add it to the <see cref="modOptionsMetadata"/> dictionary.
        /// </summary>
        /// <param name="methodInfo">The <see cref="MethodInfo"/> of the method.</param>
        private void ProcessMethod(MethodInfo methodInfo)
        {
            AddModOptionMetadata<ButtonAttribute>(methodInfo, MemberType.Method);
        }

        /// <summary>
        /// Generates a <see cref="ModOptionMetadata{T}"/> based on the member and its attributes, then adds it to the
        /// <see cref="modOptionsMetadata"/> dictionary.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the <see cref="ModOption"/> to generate for this member.</typeparam>
        /// <param name="memberInfo">The <see cref="MemberInfo"/> of the member.</param>
        /// <param name="memberType">The <see cref="MemberType"/> of the member.</param>
        /// <param name="underlyingType">The underlying <see cref="Type"/> of the member.</param>
        private void AddModOptionMetadata<TAttribute>(MemberInfo memberInfo, MemberType memberType,
            Type underlyingType = null) where TAttribute : ModOptionAttribute, new()
        {
            try
            {
                // Get the ModOptionAttribute
                var modOptionAttribute = memberInfo.GetCustomAttribute<ModOptionAttribute>(true)
                    ?? new TAttribute();

                // If there is no label specified, just use the member's name.
                if (string.IsNullOrEmpty(modOptionAttribute.Label))
                    modOptionAttribute.Label = memberInfo.Name;

                // ModOptionMetadata needed for all ModOptions
                var modOptionMetadata = new ModOptionMetadata<T>
                {
                    ModOptionAttribute = modOptionAttribute,
                    MemberInfoMetadata = new MemberInfoMetadata<T>
                    {
                        MemberType = memberType,
                        Name = memberInfo.Name,
                        ValueType = underlyingType
                    },
                    OnGameObjectCreatedMetadata = GetEventMetadata<OnGameObjectCreatedAttribute>(memberInfo)
                };

                if (memberType == MemberType.Method)
                    modOptionMetadata.MemberInfoMetadata.ParseMethodParameterTypes(memberInfo as MethodInfo);

                if (typeof(TAttribute) != typeof(ButtonAttribute))
                    modOptionMetadata.OnChangeMetadata = GetEventMetadata<OnChangeAttribute>(memberInfo);

                modOptionsMetadata.Add(modOptionAttribute.Id, modOptionMetadata);
            }
            catch (Exception ex)
            {
                Logger.Error($"[OptionsMenuBuilder] {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the metadata of every <typeparamref name="TAttribute"/> defined for a member.
        /// </summary>
        /// <typeparam name="TAttribute">
        /// The type of <see cref="ModOptionEventAttribute"/> attribute defined on the member to gather metadata for.
        /// </typeparam>
        /// <param name="memberInfo">The member to gather attribute metadata for.</param>
        /// <returns></returns>
        private IEnumerable<MemberInfoMetadata<T>> GetEventMetadata<TAttribute>(MemberInfo memberInfo)
            where TAttribute : ModOptionEventAttribute
        {
            var metadatas = new List<MemberInfoMetadata<T>>();
            foreach (TAttribute attribute in memberInfo.GetCustomAttributes<TAttribute>(true))
            {
                var methodMetadata = new MemberInfoMetadata<T>
                {
                    MemberType = MemberType.Method,
                    Name = attribute.MethodName
                };
                methodMetadata.ParseMethodParameterTypes();
                metadatas.Add(methodMetadata);
            }
            return metadatas;
        }
        #endregion

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
            Config.OnStartedLoading += OptionsMenuBuilder_Config_OnStartedLoading;
            Config.OnFinishedLoading += OptionsMenuBuilder_Config_OnFinishedLoading;

            GameObjectCreated += OptionsMenuBuilder_GameObjectCreated;
        }

        /// <summary>
        /// Invokes the method for a given <see cref="ButtonAttribute"/> and passes parameters when the button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the original button click event.</param>
        /// <param name="e">The <see cref="ButtonClickedEventArgs"/> for the click event.</param>
        private void OptionsMenuBuilder_ButtonClicked(object sender, ButtonClickedEventArgs e)
        {
            if (modOptionsMetadata.TryGetValue(e.Id, out var modOptionMetadata)
                && modOptionMetadata.MemberInfoMetadata.MethodParameterTypes is Type[] parameterTypes)
            {
                var parameters = new object[parameterTypes.Length];
                var senderFound = false;
                var eventArgsFound = false;

                for (var i = 0; i < parameters.Length; i++)
                {
                    var type = parameterTypes[i];
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

                modOptionMetadata.MemberInfoMetadata.InvokeMethod(Config, parameters);
            }
        }

        /// <summary>
        /// Sets the value in the <see cref="Config"/>, optionally saving the <see cref="Config"/> to disk if the
        /// <see cref="MenuAttribute.SaveEvents.ChangeValue"/> flag is set, before passing off to
        /// <see cref="InvokeOnChangeEvents{TSource}(ModOptionMetadata{T}, object, TSource)"/> to invoke any methods specified with a
        /// <see cref="OnChangeAttribute"/>.
        /// </summary>
        /// <param name="sender">The sender of the original choice changed event.</param>
        /// <param name="e">The <see cref="ChoiceChangedEventArgs"/> for the choice changed event.</param>
        private void OptionsMenuBuilder_ChoiceChanged(object sender, ChoiceChangedEventArgs e)
        {
            if (modOptionsMetadata.TryGetValue(e.Id, out var modOptionMetadata))
            {
                // Set the value in the Config
                var memberInfoMetadata = modOptionMetadata.MemberInfoMetadata;
                var choiceAttribute = modOptionMetadata.ModOptionAttribute as ChoiceAttribute;

                if (memberInfoMetadata.ValueType.IsEnum && (choiceAttribute.Options == null || !choiceAttribute.Options.Any()))
                {
                    // Enum-based choice where the values are parsed from the enum type
                    var value = Enum.Parse(memberInfoMetadata.ValueType, e.Value);
                    memberInfoMetadata.SetValue(Config, value);
                }
                else if (memberInfoMetadata.ValueType.IsEnum)
                {
                    // Enum-based choice where the values are defined as custom strings
                    var value = Enum.Parse(memberInfoMetadata.ValueType, Enum.GetNames(memberInfoMetadata.ValueType)[e.Index]);
                    memberInfoMetadata.SetValue(Config, value);
                }
                else if (memberInfoMetadata.ValueType == typeof(string))
                {
                    // string-based choice value
                    var value = e.Value;
                    memberInfoMetadata.SetValue(Config, value);
                }
                else if (memberInfoMetadata.ValueType == typeof(int))
                {
                    // index-based choice value
                    var value = e.Index;
                    memberInfoMetadata.SetValue(Config, value);
                }

                // Optionally save the Config to disk
                if (menuAttribute.SaveOn.HasFlag(MenuAttribute.SaveEvents.ChangeValue))
                    Config.Save();

                // Invoke any OnChange methods specified
                InvokeOnChangeEvents(modOptionMetadata, sender, e);
            }
        }

        /// <summary>
        /// Sets the value in the <see cref="Config"/>, optionally saving the <see cref="Config"/> to disk if the
        /// <see cref="MenuAttribute.SaveEvents.ChangeValue"/> flag is set, before passing off to
        /// <see cref="InvokeOnChangeEvents{TSource}(ModOptionMetadata{T}, object, TSource)"/> to invoke any methods specified with a
        /// <see cref="OnChangeAttribute"/>.
        /// </summary>
        /// <param name="sender">The sender of the original keybind changed event.</param>
        /// <param name="e">The <see cref="KeybindChangedEventArgs"/> for the keybind changed event.</param>
        private void OptionsMenuBuilder_KeybindChanged(object sender, KeybindChangedEventArgs e)
        {
            if (modOptionsMetadata.TryGetValue(e.Id, out var modOptionMetadata))
            {
                // Set the value in the Config
                modOptionMetadata.MemberInfoMetadata.SetValue(Config, e.Key);

                // Optionally save the Config to disk
                if (menuAttribute.SaveOn.HasFlag(MenuAttribute.SaveEvents.ChangeValue))
                    Config.Save();

                // Invoke any OnChange methods specified
                InvokeOnChangeEvents(modOptionMetadata, sender, e);
            }
        }

        /// <summary>
        /// Sets the value in the <see cref="Config"/>, optionally saving the <see cref="Config"/> to disk if the
        /// <see cref="MenuAttribute.SaveEvents.ChangeValue"/> flag is set, before passing off to
        /// <see cref="InvokeOnChangeEvents{TSource}(ModOptionMetadata{T}, object, TSource)"/> to invoke any methods specified with a
        /// <see cref="OnChangeAttribute"/>.
        /// </summary>
        /// <param name="sender">The sender of the original slider changed event.</param>
        /// <param name="e">The <see cref="SliderChangedEventArgs"/> for the slider changed event.</param>
        private void OptionsMenuBuilder_SliderChanged(object sender, SliderChangedEventArgs e)
        {
            if (modOptionsMetadata.TryGetValue(e.Id, out var modOptionMetadata))
            {
                // Set the value in the Config
                var memberInfoMetadata = modOptionMetadata.MemberInfoMetadata;
                var value = Convert.ChangeType(e.Value, memberInfoMetadata.ValueType);
                memberInfoMetadata.SetValue(Config, value);

                // Optionally save the Config to disk
                if (menuAttribute.SaveOn.HasFlag(MenuAttribute.SaveEvents.ChangeValue))
                    Config.Save();

                // Invoke any OnChange methods specified
                InvokeOnChangeEvents(modOptionMetadata, sender, e);
            }
        }

        /// <summary>
        /// Sets the value in the <see cref="Config"/>, optionally saving the <see cref="Config"/> to disk if the
        /// <see cref="MenuAttribute.SaveEvents.ChangeValue"/> flag is set, before passing off to
        /// <see cref="InvokeOnChangeEvents{TSource}(ModOptionMetadata{T}, object, TSource)"/> to invoke any methods specified with a
        /// <see cref="OnChangeAttribute"/>.
        /// </summary>
        /// <param name="sender">The sender of the original toggle changed event.</param>
        /// <param name="e">The <see cref="ToggleChangedEventArgs"/> for the toggle changed event.</param>
        private void OptionsMenuBuilder_ToggleChanged(object sender, ToggleChangedEventArgs e)
        {
            if (modOptionsMetadata.TryGetValue(e.Id, out var modOptionMetadata))
            {
                // Set the value in the Config
                modOptionMetadata.MemberInfoMetadata.SetValue(Config, e.Value);

                // Optionally save the Config to disk
                if (menuAttribute.SaveOn.HasFlag(MenuAttribute.SaveEvents.ChangeValue))
                    Config.Save();

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

            foreach (var modOptionMetadata in modOptionsMetadata.Values)
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
        private void InvokeOnChangeEvents(ModOptionMetadata<T> modOptionMetadata, object sender)
        {
            var id = modOptionMetadata.ModOptionAttribute.Id;
            var memberInfoMetadata = modOptionMetadata.MemberInfoMetadata;

            switch (modOptionMetadata.ModOptionAttribute)
            {
                case ChoiceAttribute choiceAttribute when (memberInfoMetadata.ValueType.IsEnum && (choiceAttribute.Options == null || !choiceAttribute.Options.Any())):
                    // Enum-based choice where the values are parsed from the enum type
                    {
                        string[] options = Enum.GetNames(memberInfoMetadata.ValueType);
                        string value = memberInfoMetadata.GetValue(Config).ToString();
                        var eventArgs = new ChoiceChangedEventArgs(id, Array.IndexOf(options, value), value);
                        InvokeOnChangeEvents(modOptionMetadata, sender, eventArgs);
                    }
                    break;
                case ChoiceAttribute _ when memberInfoMetadata.ValueType.IsEnum:
                    // Enum-based choice where the values are defined as custom strings
                    {
                        string value = memberInfoMetadata.GetValue(Config).ToString();
                        int index = Math.Max(Array.IndexOf(Enum.GetValues(memberInfoMetadata.ValueType), value), 0);
                        var eventArgs = new ChoiceChangedEventArgs(id, index, value);
                        InvokeOnChangeEvents(modOptionMetadata, sender, eventArgs);
                    }
                    break;
                case ChoiceAttribute choiceAttribute when memberInfoMetadata.ValueType == typeof(string):
                    // string-based choice value
                    {
                        string[] options = choiceAttribute.Options;
                        string value = memberInfoMetadata.GetValue<string>(Config);
                        var eventArgs = new ChoiceChangedEventArgs(id, Array.IndexOf(options, value), value);
                        InvokeOnChangeEvents(modOptionMetadata, sender, eventArgs);
                    }
                    break;
                case ChoiceAttribute choiceAttribute when memberInfoMetadata.ValueType == typeof(int):
                    // index-based choice value
                    {
                        string[] options = choiceAttribute.Options;
                        int index = memberInfoMetadata.GetValue<int>(Config);
                        var eventArgs = new ChoiceChangedEventArgs(id, index, options[index]);
                        InvokeOnChangeEvents(modOptionMetadata, sender, eventArgs);
                    }
                    break;

                case KeybindAttribute _:
                    {
                        var eventArgs = new KeybindChangedEventArgs(id, memberInfoMetadata.GetValue<KeyCode>(Config));
                        InvokeOnChangeEvents(modOptionMetadata, sender, eventArgs);
                    }
                    break;

                case SliderAttribute _:
                    {
                        var eventArgs = new SliderChangedEventArgs(id, Convert.ToSingle(memberInfoMetadata.GetValue(Config)));
                        InvokeOnChangeEvents(modOptionMetadata, sender, eventArgs);
                    }
                    break;

                case ToggleAttribute _:
                    {
                        var eventArgs = new ToggleChangedEventArgs(id, memberInfoMetadata.GetValue<bool>(Config));
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
        private void InvokeOnChangeEvents<TSource>(ModOptionMetadata<T> modOptionMetadata, object sender, TSource e)
            where TSource : IModOptionEventArgs
        {
            if (modOptionMetadata.OnChangeMetadata == null)
                return; // Skip attempting to invoke events if there are no OnChangeAttributes set for the member.

            foreach (var onChangeMetadata in modOptionMetadata.OnChangeMetadata)
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

                memberInfoMetadata.InvokeMethod(Config, parameters);
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
            if (modOptionsMetadata.TryGetValue(e.Id, out var modOptionMetadata))
            {
                // Create a tooltip if there is a TooltipAttribute specified
                if (modOptionMetadata.ModOptionAttribute.Tooltip is string tooltip)
                {
                    e.GameObject.GetComponentInChildren<Text>().gameObject.AddComponent<ModOptionTooltip>().Tooltip = tooltip;
                }

                if (modOptionMetadata.OnGameObjectCreatedMetadata == null)
                    return; // Skip attempting to invoke events if there are no OnGameObjectCreatedAttributes set for the member.

                foreach (var onGameObjectCreatedMetadata in modOptionMetadata.OnGameObjectCreatedMetadata)
                {
                    InvokeEvent(onGameObjectCreatedMetadata, sender, e);
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
            // Conditionally load the config
            if (menuAttribute.LoadOn.HasFlag(MenuAttribute.LoadEvents.MenuOpened))
                Config.Load();

            foreach (var entry in modOptionsMetadata
                .OrderBy(x => x.Value.ModOptionAttribute.Order)
                .ThenBy(x => x.Value.MemberInfoMetadata.Name))
            {
                var id = entry.Key;
                var modOptionMetadata = entry.Value;

                Logger.Debug($"[OptionsMenuBuilder] {modOptionMetadata.MemberInfoMetadata.Name}: " +
                    $"{modOptionMetadata.ModOptionAttribute.GetType()}");
                Logger.Debug($"[OptionsMenuBuilder] Label: {modOptionMetadata.ModOptionAttribute.Label}");

                var label = modOptionMetadata.ModOptionAttribute.Label;

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
                string value = memberInfoMetadata.GetValue(Config).ToString();
                AddChoiceOption(id, label, options, value);
            }
            else if (memberInfoMetadata.ValueType.IsEnum)
            {
                // Enum-based choice where the values are defined as custom strings
                string[] options = choiceAttribute.Options;
                string value = memberInfoMetadata.GetValue(Config).ToString();
                int index = Math.Max(Array.IndexOf(Enum.GetValues(memberInfoMetadata.ValueType), value), 0);
                AddChoiceOption(id, label, options, index);
            }
            else if (memberInfoMetadata.ValueType == typeof(string))
            {
                // string-based choice value
                string[] options = choiceAttribute.Options;
                string value = memberInfoMetadata.GetValue<string>(Config);
                AddChoiceOption(id, label, options, value);
            }
            else if (memberInfoMetadata.ValueType == typeof(int))
            {
                // index-based choice value
                string[] options = choiceAttribute.Options;
                int index = memberInfoMetadata.GetValue<int>(Config);
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
            var value = memberInfoMetadata.GetValue<KeyCode>(Config);
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
            float value = Convert.ToSingle(memberInfoMetadata.GetValue(Config));

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
            bool value = memberInfoMetadata.GetValue<bool>(Config);
            AddToggleOption(id, label, value);
        }
        #endregion
    }
}
