namespace SMLHelper.V2.Options
{
    using HarmonyLib;
    using Interfaces;
    using Json;
    using QModManager.API;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
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
    internal class ConfigModOptions<T> : ModOptions where T : ConfigFile, new()
    {
        /// <summary>
        /// The <typeparamref name="T"/> <see cref="ConfigFile"/> instance related to this <see cref="ModOptions"/> menu.
        /// </summary>
        public T Config { get; }

        private Assembly assembly => Assembly.GetAssembly(typeof(T));
        private IQMod QMod { get; }

        /// <summary>
        /// Instantiates a new <see cref="ConfigModOptions{T}"/>, generating <see cref="ModOption"/>s by parsing the fields,
        /// properties and methods declared in the class.
        /// </summary>
        public ConfigModOptions() : base(null)
        {
            QMod = QModServices.Main.GetMod(assembly);

            // Instantiate and load the config
            Config = new T();
            Config.Load();

            LoadMetadata();
            BindEvents();
        }

        #region Serialization/Deserialization
        /// <summary>
        /// A simple struct containing metadata for each auto-generated <see cref="ModOption"/> in a serializable format.
        /// </summary>
        private class ModOptionMetadata
        {
            [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
            public ModOptionAttribute ModOptionAttribute;
            public MemberInfoMetadata MemberInfoMetadata;
            public IEnumerable<MemberInfoMetadata> OnChangeMetadata;
            public IEnumerable<MemberInfoMetadata> OnGameObjectCreatedMetadata;
        }

        /// <summary>
        /// Specifies the member type of the member, ie. field/property/method.
        /// </summary>
        private enum MemberType { Unknown, Field, Property, Method };

        /// <summary>
        /// A struct containing metadata for a <see cref="MemberInfo"/> in a serializable format.
        /// </summary>
        private class MemberInfoMetadata
        {
            public MemberType MemberType = MemberType.Unknown;
            public string Name;
            public Type ValueType;
            public Type[] MethodParameterTypes;
            public bool MethodValid = false;

            /// <summary>
            /// Uses the stored metadata to get the current value of the member.
            /// </summary>
            /// <typeparam name="TValue">The type of the value.</typeparam>
            /// <param name="config">The config to get the value from.</param>
            /// <returns>The value.</returns>
            public TValue GetValue<TValue>(T config)
            {
                switch (MemberType)
                {
                    case MemberType.Field:
                        return Traverse.Create(config).Field(Name).GetValue<TValue>();
                    case MemberType.Property:
                        return Traverse.Create(config).Property(Name).GetValue<TValue>();
                    default:
                        throw new InvalidOperationException($"Member must be a Field or Property but is {MemberType}: " +
                            $"{typeof(T).Name}.{Name}");
                }
            }

            /// <summary>
            /// Uses the stored metadata to get the current value of the member.
            /// </summary>
            /// <param name="config">The config to get the value from.</param>
            /// <returns>The value.</returns>
            public object GetValue(T config) => GetValue<object>(config);

            /// <summary>
            /// Uses the stored metadata to set the current value of the member.
            /// </summary>
            /// <typeparam name="TValue">The type of the value.</typeparam>
            /// <param name="config">The config the set the value in.</param>
            /// <param name="value">The value.</param>
            public void SetValue<TValue>(T config, TValue value)
            {
                switch (MemberType)
                {
                    case MemberType.Field:
                        Traverse.Create(config).Field(Name).SetValue(value);
                        break;
                    case MemberType.Property:
                        Traverse.Create(config).Property(Name).SetValue(value);
                        break;
                    default:
                        throw new InvalidOperationException($"Member must be a Field or Property but is {MemberType}: " +
                            $"{typeof(T).Name}.{Name}");
                }
            }

            /// <summary>
            /// Stores the <see cref="Type"/> of each parameter of a method to the
            /// <see cref="MethodParameterTypes"/> array.
            /// </summary>
            /// <param name="methodInfo"><see cref="MethodInfo"/> of the method to parse.</param>
            public void ParseMethodParameterTypes(MethodInfo methodInfo = null)
            {
                if (MemberType != MemberType.Method)
                    throw new InvalidOperationException($"Member must be a Method but is {MemberType}: {typeof(T).Name}.{Name}");

                if (methodInfo == null)
                {
                    methodInfo = AccessTools.Method(typeof(T), Name);

                    if (methodInfo == null)
                    {
                        // Method not found, log error and skip.
                        Logger.Error($"[ConfigModOptions] Could not find the specified method: {typeof(T)}.{Name}");
                        return;
                    }
                }

                MethodValid = true;
                MethodParameterTypes = methodInfo.GetParameters().Select(x => x.ParameterType).ToArray();
            }

            /// <summary>
            /// Use the stored metadata to invoke the method.
            /// </summary>
            /// <param name="config">The config in which the method.</param>
            /// <param name="arguments">An array of arguments to pass to the method.</param>
            public void InvokeMethod(T config, params object[] arguments)
            {
                if (MemberType != MemberType.Method)
                    throw new InvalidOperationException($"Member must be a Method but is {MemberType}: {typeof(T).Name}.{Name}");

                if (!MethodValid)
                {
                    // Method not found, log error and skip.
                    Logger.Error($"[ConfigModOptions] Could not find the specified method: {typeof(T)}.{Name}");
                    return;
                }

                Traverse.Create(config).Method(Name, MethodParameterTypes).GetValue(arguments);
            }
        }

        private struct ConfigModOptionsMetadata
        {
            /// <summary>
            /// Timestamp of the relevant mod assembly. Used to determine the validity of the metadata.
            /// </summary>
            public long Timestamp;

            /// <summary>
            /// The <see cref="MenuAttribute"/> relating to this <see cref="ModOptions"/> menu.
            /// </summary>
            public MenuAttribute MenuAttribute;

            /// <summary>
            /// A dictionary of <see cref="ModOptionMetadata"/>, indexed by <see cref="ModOption.Id"/>.
            /// </summary>
            public Dictionary<string, ModOptionMetadata> ModOptionsMetadata;
        }

        private ConfigModOptionsMetadata configModOptionsMetadata;

        private void LoadMetadata()
        {
            long timestamp = File.GetLastWriteTimeUtc(assembly.Location).Ticks;

            Stopwatch stopwatch = new Stopwatch();

            if (Logger.EnableDebugging)
                stopwatch.Start();

            configModOptionsMetadata = new ConfigModOptionsMetadata
            {
                MenuAttribute = GetMenuAttributeOrDefault(),
                ModOptionsMetadata = new Dictionary<string, ModOptionMetadata>(),
                Timestamp = timestamp
            };
            Name = configModOptionsMetadata.MenuAttribute.Name;

            ProcessMetadata();

            if (Logger.EnableDebugging)
            {
                stopwatch.Stop();
                Logger.Debug($"[{QMod.DisplayName}] ConfigModOptions metadata parsed via reflection in {stopwatch.ElapsedMilliseconds}ms.");
            }
        }
        #endregion

        #region Metadata Processing
        private MenuAttribute menuAttribute => configModOptionsMetadata.MenuAttribute;
        private Dictionary<string, ModOptionMetadata> modOptionsMetadata
            => configModOptionsMetadata.ModOptionsMetadata;

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

            Logger.Debug($"[ConfigModOptions] Found {modOptionsMetadata.Count()} options to add to the menu.");
        }

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
                return new MenuAttribute(QMod.DisplayName);
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
        {
            if (Attribute.IsDefined(memberInfo, typeof(IgnoreMemberAttribute)))
            {
                return false;
            }
            else if (!menuAttribute.IgnoreUnattributedMembers)
            {
                if (memberInfo is MethodInfo)
                {
                    if (Attribute.IsDefined(memberInfo, typeof(ButtonAttribute), true))
                        return true;

                    IEnumerable<MemberInfoMetadata> eventMetadatas
                        = modOptionsMetadata.Values.SelectMany(modOptionsMetadata =>
                        {
                            IEnumerable<MemberInfoMetadata> result = new List<MemberInfoMetadata>();

                            if (modOptionsMetadata.OnChangeMetadata != null)
                                result.Concat(modOptionsMetadata.OnChangeMetadata);

                            if (modOptionsMetadata.OnGameObjectCreatedMetadata != null)
                                result.Concat(modOptionsMetadata.OnGameObjectCreatedMetadata);

                            return result;
                        });
                    return eventMetadatas.Any(memberInfoMetadata => memberInfoMetadata.Name == memberInfo.Name);
                }

                return true;
            }
            else
            {
                return Attribute.IsDefined(memberInfo, typeof(ModOptionAttribute), true) ||
                    Attribute.IsDefined(memberInfo, typeof(ModOptionEventAttribute), true);
            }
        }

        /// <summary>
        /// Processes the given field or property and hands off to
        /// <see cref="AddModOptionMetadata{TAttribute}(MemberInfo, MemberType, Type)"/> to generate a <see cref="ModOptionMetadata"/>
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
        /// to generate a <see cref="ModOptionMetadata"/> and add it to the <see cref="modOptionsMetadata"/> dictionary.
        /// </summary>
        /// <param name="methodInfo">The <see cref="MethodInfo"/> of the method.</param>
        private void ProcessMethod(MethodInfo methodInfo)
        {
            AddModOptionMetadata<ButtonAttribute>(methodInfo, MemberType.Method);
        }

        /// <summary>
        /// Generates a <see cref="ModOptionMetadata"/> based on the member and its attributes, then adds it to the
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
                var modOptionAttribute = memberInfo.GetCustomAttributes(typeof(ModOptionAttribute), true)
                    .SingleOrDefault() as ModOptionAttribute ?? new TAttribute();

                // If there is no label specified, just use the member's name.
                if (string.IsNullOrEmpty(modOptionAttribute.Label))
                    modOptionAttribute.Label = memberInfo.Name;

                // ModOptionMetadata needed for all ModOptions
                var modOptionMetadata = new ModOptionMetadata
                {
                    ModOptionAttribute = modOptionAttribute,
                    MemberInfoMetadata = new MemberInfoMetadata
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
                Logger.Error($"[ConfigModOptions] {ex.Message}");
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
        private IEnumerable<MemberInfoMetadata> GetEventMetadata<TAttribute>(MemberInfo memberInfo)
            where TAttribute : ModOptionEventAttribute
        {
            var metadatas = new List<MemberInfoMetadata>();
            foreach (TAttribute attribute in memberInfo.GetCustomAttributes(typeof(TAttribute), true))
            {
                var methodMetadata = new MemberInfoMetadata
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
            ButtonClicked += ConfigModOptions_ButtonClicked;
            ChoiceChanged += ConfigModOptions_ChoiceChanged;
            KeybindChanged += ConfigModOptions_KeybindChanged;
            SliderChanged += ConfigModOptions_SliderChanged;
            ToggleChanged += ConfigModOptions_ToggleChanged;
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
        /// <see cref="InvokeOnChangeEvents{TSource}(ModOptionMetadata, object, TSource)"/> to invoke any methods specified with a
        /// <see cref="OnChangeAttribute"/>.
        /// </summary>
        /// <param name="sender">The sender of the original choice changed event.</param>
        /// <param name="e">The <see cref="ChoiceChangedEventArgs"/> for the choice changed event.</param>
        private void ConfigModOptions_ChoiceChanged(object sender, ChoiceChangedEventArgs e)
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
        /// <see cref="InvokeOnChangeEvents{TSource}(ModOptionMetadata, object, TSource)"/> to invoke any methods specified with a
        /// <see cref="OnChangeAttribute"/>.
        /// </summary>
        /// <param name="sender">The sender of the original keybind changed event.</param>
        /// <param name="e">The <see cref="KeybindChangedEventArgs"/> for the keybind changed event.</param>
        private void ConfigModOptions_KeybindChanged(object sender, KeybindChangedEventArgs e)
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
        /// <see cref="InvokeOnChangeEvents{TSource}(ModOptionMetadata, object, TSource)"/> to invoke any methods specified with a
        /// <see cref="OnChangeAttribute"/>.
        /// </summary>
        /// <param name="sender">The sender of the original slider changed event.</param>
        /// <param name="e">The <see cref="SliderChangedEventArgs"/> for the slider changed event.</param>
        private void ConfigModOptions_SliderChanged(object sender, SliderChangedEventArgs e)
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
        /// <see cref="InvokeOnChangeEvents{TSource}(ModOptionMetadata, object, TSource)"/> to invoke any methods specified with a
        /// <see cref="OnChangeAttribute"/>.
        /// </summary>
        /// <param name="sender">The sender of the original toggle changed event.</param>
        /// <param name="e">The <see cref="ToggleChangedEventArgs"/> for the toggle changed event.</param>
        private void ConfigModOptions_ToggleChanged(object sender, ToggleChangedEventArgs e)
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

        /// <summary>
        /// Invokes the relevant method(s) specified with <see cref="OnChangeAttribute"/>(s)
        /// and passes parameters when a value is changed.
        /// </summary>
        /// <typeparam name="TSource">The type of the event args.</typeparam>
        /// <param name="modOptionMetadata">The metadata for the mod option.</param>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event args from the OnChange event.</param>
        private void InvokeOnChangeEvents<TSource>(ModOptionMetadata modOptionMetadata, object sender, TSource e)
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
        private void InvokeEvent<TSource>(MemberInfoMetadata memberInfoMetadata, object sender, TSource e)
            where TSource : IModOptionEventArgs
        {
            if (!memberInfoMetadata.MethodValid)
            {
                // Method not found, log error and skip.
                Logger.Error($"[ConfigModOptions] Could not find the specified method: {typeof(T)}.{memberInfoMetadata.Name}");
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
        private void ConfigModOptions_GameObjectCreated(object sender, GameObjectCreatedEventArgs e)
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
            foreach (var entry in modOptionsMetadata
                .OrderBy(x => x.Value.ModOptionAttribute.Order)
                .ThenBy(x => x.Value.MemberInfoMetadata.Name))
            {
                var id = entry.Key;
                var modOptionMetadata = entry.Value;

                Logger.Debug($"[ConfigModOptions] {modOptionMetadata.MemberInfoMetadata.Name}: " +
                    $"{modOptionMetadata.ModOptionAttribute.GetType()}");
                Logger.Debug($"[ConfigModOptions] Label: {modOptionMetadata.ModOptionAttribute.Label}");

                var label = modOptionMetadata.ModOptionAttribute.Label;

                if (modOptionMetadata.ModOptionAttribute is ButtonAttribute)
                {
                    BuildModButtonOption(id, label);
                }
                else if (modOptionMetadata.ModOptionAttribute is ChoiceAttribute choiceAttribute)
                {
                    BuildModChoiceOption(id, label, modOptionMetadata.MemberInfoMetadata, choiceAttribute);
                }
                else if (modOptionMetadata.ModOptionAttribute is KeybindAttribute)
                {
                    BuildModKeybindOption(id, label, modOptionMetadata.MemberInfoMetadata);
                }
                else if (modOptionMetadata.ModOptionAttribute is SliderAttribute sliderAttribute)
                {
                    BuildModSliderOption(id, label, modOptionMetadata.MemberInfoMetadata, sliderAttribute);
                }
                else if (modOptionMetadata.ModOptionAttribute is ToggleAttribute)
                {
                    BuildModToggleOption(id, label, modOptionMetadata.MemberInfoMetadata);
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
            MemberInfoMetadata memberInfoMetadata, ChoiceAttribute choiceAttribute)
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
        private void BuildModKeybindOption(string id, string label, MemberInfoMetadata memberInfoMetadata)
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
            MemberInfoMetadata memberInfoMetadata, SliderAttribute sliderAttribute)
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
        private void BuildModToggleOption(string id, string label, MemberInfoMetadata memberInfoMetadata)
        {
            bool value = memberInfoMetadata.GetValue<bool>(Config);
            AddToggleOption(id, label, value);
        }
        #endregion
    }
}
