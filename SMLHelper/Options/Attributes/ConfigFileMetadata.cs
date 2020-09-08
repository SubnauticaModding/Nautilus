namespace SMLHelper.V2.Options.Attributes
{
    using Json;
    using QModManager.API;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;
    using Logger = Logger;

    internal class ConfigFileMetadata<T> where T : ConfigFile, new()
    {
        public T Config { get; } = new T();

        public IQMod QMod { get; } = QModServices.Main.GetMod(Assembly.GetAssembly(typeof(T)));

        /// <summary>
        /// The <see cref="MenuAttribute"/> relating to this <see cref="ModOptions"/> menu.
        /// </summary>
        public MenuAttribute MenuAttribute { get; private set; }

        /// <summary>
        /// A dictionary of <see cref="ModOptionAttributeMetadata{T}"/>, indexed by <see cref="ModOption.Id"/>.
        /// </summary>
        public Dictionary<string, ModOptionAttributeMetadata<T>> ModOptionAttributesMetadata { get; private set; }

        /// <summary>
        /// Process metadata for members of <typeparamref name="T"/>.
        /// </summary>
        public void ProcessMetadata()
        {
            Stopwatch stopwatch = new Stopwatch();

            if (Logger.EnableDebugging)
                stopwatch.Start();

            MenuAttribute = typeof(T).GetCustomAttribute<MenuAttribute>(true) ?? new MenuAttribute(QMod.DisplayName);
            ModOptionAttributesMetadata = new Dictionary<string, ModOptionAttributeMetadata<T>>();

            processMetadata();

            if (Logger.EnableDebugging)
            {
                stopwatch.Stop();
                Logger.Debug($"[{QMod.DisplayName}] [{typeof(T).Name}] OptionsMenuBuilder metadata parsed in {stopwatch.ElapsedMilliseconds}ms.");
            }
        }

        private void processMetadata()
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public;
            foreach (PropertyInfo property in typeof(T).GetProperties(bindingFlags)
                .Where(memberIsDeclaredInConfigFileSubclass) // Only care about members declared in a subclass of ConfigFile
                .Where(memberIsNotIgnored)) // Filter out explicitly ignored members
            {
                processFieldOrProperty(property, MemberType.Property, property.PropertyType);
            }

            foreach (FieldInfo field in typeof(T).GetFields(bindingFlags)
                .Where(memberIsDeclaredInConfigFileSubclass) // Only care about members declared in a subclass of ConfigFile
                .Where(memberIsNotIgnored)) // Filter out explicitly ignored members
            {
                processFieldOrProperty(field, MemberType.Field, field.FieldType);
            }

            foreach (MethodInfo method in typeof(T).GetMethods(bindingFlags | BindingFlags.Static)
                .Where(memberIsDeclaredInConfigFileSubclass) // Only care about members declared in a subclass of ConfigFile
                .Where(memberIsNotIgnored)) // Filter out explicitly ignored members
            {
                processMethod(method);
            }

            Logger.Debug($"[{QMod.DisplayName}] [{typeof(T).Name}] Found {ModOptionAttributesMetadata.Count()} options to add to the menu.");
        }

        /// <summary>
        /// Checks whether a given <see cref="MemberInfo"/> is declared in any subclass of <see cref="ConfigFile"/>.
        /// </summary>
        /// <param name="memberInfo">The <see cref="MemberInfo"/> to check.</param>
        /// <returns>Whether the given <see cref="MemberInfo"/> is declared in any subclass of <see cref="ConfigFile"/>.</returns>
        private static bool memberIsDeclaredInConfigFileSubclass(MemberInfo memberInfo)
            => memberInfo.DeclaringType.IsSubclassOf(typeof(ConfigFile));

        /// <summary>
        /// Checks whether a given <see cref="MemberInfo"/> should be ignored when generating the options menu, based on whether
        /// the member has a declared <see cref="IgnoreMemberAttribute"/>, or the <see cref="MenuAttribute"/>'s
        /// <see cref="MenuAttribute.MemberProcessing"/> property.
        /// </summary>
        /// <param name="memberInfo">The <see cref="MemberInfo"/> to check.</param>
        /// <returns>Whether the given <see cref="MemberInfo"/> member should be ignored when generating the options menu.</returns>
        private bool memberIsNotIgnored(MemberInfo memberInfo)
        {
            if (Attribute.IsDefined(memberInfo, typeof(IgnoreMemberAttribute)))
                return false;

            switch (MenuAttribute.MemberProcessing)
            {
                case MenuAttribute.Members.OptOut:
                    if (memberInfo is MethodInfo)
                    {
                        if (Attribute.IsDefined(memberInfo, typeof(ButtonAttribute), true))
                            return true;

                        IEnumerable<MemberInfoMetadata<T>> eventMetadatas
                            = ModOptionAttributesMetadata.Values.SelectMany(modOptionsMetadata =>
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

                case MenuAttribute.Members.OptIn:
                    return Attribute.IsDefined(memberInfo, typeof(ModOptionAttribute), true) ||
                        Attribute.IsDefined(memberInfo, typeof(ModOptionEventAttribute), true);

                default: throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Processes the given field or property and hands off to
        /// <see cref="addModOptionMetadata{TAttribute}(MemberInfo, MemberType, Type)"/> to generate a <see cref="ModOptionAttributeMetadata{T}"/>
        /// and add it to the <see cref="ModOptionAttributesMetadata"/> dictionary.
        /// </summary>
        /// <param name="memberInfo">The <see cref="MemberInfo"/> of the member.</param>
        /// <param name="memberType">The <see cref="MemberType"/> of the member.</param>
        /// <param name="underlyingType">The underlying <see cref="Type"/> of the member.</param>
        private void processFieldOrProperty(MemberInfo memberInfo, MemberType memberType, Type underlyingType)
        {
            if (underlyingType == typeof(bool))
            {
                addModOptionMetadata<ToggleAttribute>(memberInfo, memberType, underlyingType);
            }
            else if (underlyingType == typeof(KeyCode))
            {
                addModOptionMetadata<KeybindAttribute>(memberInfo, memberType, underlyingType);
            }
            else if (underlyingType.IsEnum || Attribute.IsDefined(memberInfo, typeof(ChoiceAttribute), true))
            {
                addModOptionMetadata<ChoiceAttribute>(memberInfo, memberType, underlyingType);
            }
            else if (underlyingType == typeof(float) ||
                    underlyingType == typeof(double) ||
                    underlyingType == typeof(int) ||
                    Attribute.IsDefined(memberInfo, typeof(SliderAttribute), true))
            {
                addModOptionMetadata<SliderAttribute>(memberInfo, memberType, underlyingType);
            }
        }

        /// <summary>
        /// Processes the given method and hands off to <see cref="addModOptionMetadata{TAttribute}(MemberInfo, MemberType, Type)"/>
        /// to generate a <see cref="ModOptionAttributeMetadata{T}"/> and add it to the <see cref="ModOptionAttributesMetadata"/> dictionary.
        /// </summary>
        /// <param name="methodInfo">The <see cref="MethodInfo"/> of the method.</param>
        private void processMethod(MethodInfo methodInfo)
        {
            addModOptionMetadata<ButtonAttribute>(methodInfo, MemberType.Method);
        }

        /// <summary>
        /// Generates a <see cref="ModOptionAttributeMetadata{T}"/> based on the member and its attributes, then adds it to the
        /// <see cref="ModOptionAttributesMetadata"/> dictionary.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the <see cref="ModOption"/> to generate for this member.</typeparam>
        /// <param name="memberInfo">The <see cref="MemberInfo"/> of the member.</param>
        /// <param name="memberType">The <see cref="MemberType"/> of the member.</param>
        /// <param name="underlyingType">The underlying <see cref="Type"/> of the member.</param>
        private void addModOptionMetadata<TAttribute>(MemberInfo memberInfo, MemberType memberType,
            Type underlyingType = null) where TAttribute : ModOptionAttribute, new()
        {
            try
            {
                // Get the ModOptionAttribute
                ModOptionAttribute modOptionAttribute = memberInfo.GetCustomAttribute<ModOptionAttribute>(true)
                    ?? new TAttribute();

                // If there is no label specified, just use the member's name.
                if (string.IsNullOrEmpty(modOptionAttribute.Label))
                    modOptionAttribute.Label = memberInfo.Name;

                // ModOptionMetadata needed for all ModOptions
                var modOptionMetadata = new ModOptionAttributeMetadata<T>
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

                ModOptionAttributesMetadata.Add(modOptionAttribute.Id, modOptionMetadata);
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

        public bool TryGetMetadata(string id, out ModOptionAttributeMetadata<T> modOptionAttributeMetadata)
        {
            return ModOptionAttributesMetadata.TryGetValue(id, out modOptionAttributeMetadata);
        }

        public IEnumerable<ModOptionAttributeMetadata<T>> GetValues()
        {
            return ModOptionAttributesMetadata.Values;
        }
    }
}
