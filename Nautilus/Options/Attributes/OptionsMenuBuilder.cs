using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nautilus.Json;
using Nautilus.Utility;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Nautilus.Options.Attributes;

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
    /// Instantiates a new <see cref="OptionsMenuBuilder{T}"/>, generating <see cref="OptionItem"/>s by parsing the fields,
    /// properties and methods declared in the class.
    /// </summary>
    public OptionsMenuBuilder() : base(null)
    {
        BindEvents();

        ConfigFileMetadata.ProcessMetadata();

        Name = ConfigFileMetadata.MenuAttribute.Name;

        // Conditionally load the config
        if (ConfigFileMetadata.MenuAttribute.LoadOn.HasFlag(MenuAttribute.LoadEvents.MenuRegistered))
        {
            ConfigFileMetadata.Config.Load();
        }
    }

    private void BindEvents()
    {
        ConfigFileMetadata.BindEvents();

        OnChanged += EventHandler;
        GameObjectCreated += EventHandler;
    }

    private void EventHandler(object sender, EventArgs e)
    {
        if (!ConfigFileMetadata.Registered)
        {
            // if we haven't marked the options menu as being registered yet, its too soon to fire the events,
            // so run a coroutine that waits until the first frame where Registered == true
            // before routing the event
            UWE.CoroutineHost.StartCoroutine(DeferredEventHandlerRoutine(sender, e));
        }
        else
        {
            // otherwise, route the event immediately
            RouteEventHandler(sender, e);
        }
    }

    private IEnumerator DeferredEventHandlerRoutine(object sender, EventArgs e)
    {
        yield return new WaitUntil(() => ConfigFileMetadata.Registered);
        RouteEventHandler(sender, e);
    }

    private void RouteEventHandler(object sender, EventArgs e)
    {
        if (e is ButtonClickedEventArgs buttonClickedEventArgs)
            ConfigFileMetadata.HandleButtonClick(sender, buttonClickedEventArgs);
        else if (e.GetType().IsGenericType && e.GetType().GetGenericTypeDefinition() == typeof(ChoiceChangedEventArgs<>))
        {
            var genericParam = e.GetType().GetGenericArguments()[0];
            InternalLogger.Debug($"Generic param: {genericParam}");
            var genericType = typeof(ChoiceChangedEventArgs<>).MakeGenericType(genericParam);
            var typedEvent = Convert.ChangeType(e, genericType);
            var methodInfo = ConfigFileMetadata.GetType().GetMethod(nameof(ConfigFileMetadata.HandleChoiceChanged));
            var typedMethod = methodInfo.MakeGenericMethod(genericParam);
            typedMethod.Invoke(ConfigFileMetadata, new object[] { sender, typedEvent });
        }
        else if (e is ColorChangedEventArgs colorChangedEventArgs)
            ConfigFileMetadata.HandleColorChanged(sender, colorChangedEventArgs);
        else if (e is KeybindChangedEventArgs keybindChangedEventArgs)
            ConfigFileMetadata.HandleKeybindChanged(sender, keybindChangedEventArgs);
        else if (e is SliderChangedEventArgs sliderChangedEventArgs)
            ConfigFileMetadata.HandleSliderChanged(sender, sliderChangedEventArgs);
        else if (e is ToggleChangedEventArgs toggleChangedEventArgs)
            ConfigFileMetadata.HandleToggleChanged(sender, toggleChangedEventArgs);
        else if (e is GameObjectCreatedEventArgs gameObjectCreatedEventArgs)
            ConfigFileMetadata.HandleGameObjectCreated(sender, gameObjectCreatedEventArgs);
    }

    #region Build ModOptions Menu
    /// <summary>
    /// Adds options to the menu based on the <see cref="ConfigFileMetadata"/>.
    /// </summary>
    public override void BuildModOptions(uGUI_TabbedControlsPanel panel, int modsTabIndex, IReadOnlyCollection<OptionItem> options)
    {

        // Conditionally load the config
        if (ConfigFileMetadata.MenuAttribute.LoadOn.HasFlag(MenuAttribute.LoadEvents.MenuOpened))
        {
            ConfigFileMetadata.Config.Load();
            foreach(var option in new List<OptionItem>(Options))
            {
                RemoveItem(option.Id);
            }
        }
        if(Options.Count == 0)
        {
            foreach(KeyValuePair<string, ModOptionAttributeMetadata<T>> entry in ConfigFileMetadata.ModOptionAttributesMetadata
                        .OrderBy(x => x.Value.ModOptionAttribute.Order)
                        .ThenBy(x => x.Value.MemberInfoMetadata.Name))
            {
                string id = entry.Key;
                ModOptionAttributeMetadata<T> modOptionMetadata = entry.Value;

                string label = modOptionMetadata.ModOptionAttribute.Label;
                if(Language.main.TryGet(modOptionMetadata.ModOptionAttribute.LabelLanguageId, out string languageLabel))
                {
                    label = languageLabel;
                }

                InternalLogger.Debug($"[{ConfigFileMetadata.ModName}] [{typeof(T).Name}] {modOptionMetadata.MemberInfoMetadata.Name}: " +
                                     $"{modOptionMetadata.ModOptionAttribute.GetType().Name}");
                InternalLogger.Debug($"[{ConfigFileMetadata.ModName}] [{typeof(T).Name}] Label: {label}");


                switch(modOptionMetadata.ModOptionAttribute)
                {
                    case ButtonAttribute buttonAttribute:
                        BuildModButtonOption(id, label, modOptionMetadata.MemberInfoMetadata);
                        break;
                    case ChoiceAttribute choiceAttribute:
                        BuildModChoiceOption(id, label, modOptionMetadata.MemberInfoMetadata, choiceAttribute);
                        break;
                    case ColorPickerAttribute colorAttribute:
                        BuildModColorOption(id, label, modOptionMetadata.MemberInfoMetadata, colorAttribute);
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
        
        base.BuildModOptions(panel, modsTabIndex, options);
    }

    /// <summary>
    /// Adds a <see cref="ModButtonOption"/> to the <see cref="ModOptions"/> menu.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="label"></param>
    /// <param name="memberInfoMetadata">The metadata of the corresponding member.</param>
    private void BuildModButtonOption(string id, string label, MemberInfoMetadata<T> memberInfoMetadata)
    {
        if (memberInfoMetadata.MemberType != MemberType.Method)
        {
            InternalLogger.Warn($"Failed to add ModButtonOption with id {id} to {Name} as the attribute is not a Method.");
            return;
        }
        if(!AddItem(ModButtonOption.Create(id, label, memberInfoMetadata.GetMethodAsAction<ButtonClickedEventArgs>(ConfigFileMetadata.Config))))
            InternalLogger.Warn($"Failed to add ModButtonOption with id {id} to {Name} as an option with that id already exists.");

        InternalLogger.Debug($"Added ModButtonOption with id {id} to {Name}");
    }

    /// <summary>
    /// Adds a <see cref="ModChoiceOption{T}"/> to the <see cref="ModOptions"/> menu.
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
            try
            {
                // Enum-based choice where the values are parsed from the enum type
                var options = Enum.GetValues(memberInfoMetadata.ValueType);
                var value = memberInfoMetadata.GetValue(ConfigFileMetadata.Config);

                Type type = memberInfoMetadata.ValueType;
                Type arrayType = type.MakeArrayType();
                // make generic constructor for ModChoiceOption<T>
                var genericType = typeof(ModChoiceOption<>).MakeGenericType(memberInfoMetadata.ValueType);            // get the Create method
                var methodInfo = genericType.GetMethod("Create", new Type[] { typeof(string), typeof(string), arrayType, type, typeof(string) });
                OptionItem optionItem = (OptionItem) methodInfo.Invoke(null, new object[] { id, label, options, value, choiceAttribute.Tooltip });
                if (!AddItem(optionItem))
                    InternalLogger.Warn($"Failed to add ModChoiceOption with id {id} to {Name}");
            }
            catch (Exception e)
            {
                InternalLogger.Error($"Failed to add ModChoiceOption with id {id} to {Name} due to an error parsing the options: {e}");
            }
        }
        else if (memberInfoMetadata.ValueType.IsEnum)
        {
            try
            {
                // Enum-based choice where the values are defined as custom strings
                var options = Array.CreateInstance(memberInfoMetadata.ValueType, choiceAttribute.Options.Length);                
                for (int i = 0; i < choiceAttribute.Options.Length; i++)
                {
                    options.SetValue(Enum.Parse(memberInfoMetadata.ValueType, choiceAttribute.Options[i], true), i);
                }
                var option = memberInfoMetadata.GetValue(ConfigFileMetadata.Config);
                int index = Array.IndexOf(options, option);
                if (index < 0)
                    index = 0;
                Type type = memberInfoMetadata.ValueType;
                Type arrayType = type.MakeArrayType();
                // make generic constructor for ModChoiceOption<T>
                var genericType = typeof(ModChoiceOption<>).MakeGenericType(memberInfoMetadata.ValueType);            // get the Create method
                var methodInfo = genericType.GetMethod("Create", new Type[] { typeof(string), typeof(string), arrayType, typeof(int), typeof(string) });
                OptionItem optionItem = (OptionItem) methodInfo.Invoke(null, new object[] { id, label, options, index, choiceAttribute.Tooltip });
                if (!AddItem(optionItem))
                    InternalLogger.Warn($"Failed to add ModChoiceOption with id {id} to {Name}");
            }
            catch (Exception e)
            {
                InternalLogger.Error($"Failed to add ModChoiceOption with id {id} to {Name} due to an error parsing the options: {e}");
            }
        }
        else if (memberInfoMetadata.ValueType == typeof(string))
        {
            // string-based choice value
            string[] options = choiceAttribute.Options;
            string value = memberInfoMetadata.GetValue<string>(ConfigFileMetadata.Config);
            if(!AddItem(ModChoiceOption<string>.Create(id, label, options, value)))
                InternalLogger.Warn($"Failed to add ModChoiceOption with id {id} to {Name}");
        }
        else if (memberInfoMetadata.ValueType == typeof(int))
        {
            try
            {
                // index-based choice value
                int[] options = new int[choiceAttribute.Options.Length];
                for (int i = 0; i < choiceAttribute.Options.Length; i++)
                {
                    options[i] = int.Parse(choiceAttribute.Options[i]);
                }
                int index = memberInfoMetadata.GetValue<int>(ConfigFileMetadata.Config);
                if (!AddItem(ModChoiceOption<int>.Create(id, label, options, index)))
                    InternalLogger.Warn($"Failed to add ModChoiceOption with id {id} to {Name}");
            }
            catch (Exception e)
            {
                InternalLogger.Error($"Failed to add ModChoiceOption with id {id} to {Name} due to an error parsing the options: {e}");
            }
        }
    }

    /// <summary>
    /// Adds a <see cref="ModColorOption"/> to the <see cref="ModOptions"/> menu.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="label"></param>
    /// <param name="memberInfoMetadata">The metadata of the corresponding member.</param>
    /// <param name="colorAttribute">The defined or generated <see cref="ColorPickerAttribute"/> of the member.</param>
    private void BuildModColorOption(string id, string label, MemberInfoMetadata<T> memberInfoMetadata, ColorPickerAttribute colorAttribute)
    {
        Color value = memberInfoMetadata.GetValue<Color>(ConfigFileMetadata.Config);
        if (!AddItem(ModColorOption.Create(id, label, value, colorAttribute.Advanced)))
            InternalLogger.Warn($"Failed to add ModColorOption with id {id} to {Name}");
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
        if(!AddItem(ModKeybindOption.Create(id, label, GameInput.GetPrimaryDevice(), value)))
            InternalLogger.Warn($"Failed to add ModKeybindOption with id {id} to {Name}");
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
        {
            step = Mathf.CeilToInt(step);
        }

        if(!AddItem(ModSliderOption.Create(id, label, sliderAttribute.Min, sliderAttribute.Max,
               Convert.ToSingle(value), sliderAttribute.DefaultValue, sliderAttribute.Format, step, sliderAttribute.Tooltip)))
            InternalLogger.Warn($"Failed to add ModSliderOption with id {id} to {Name}");
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
        if(!AddItem(ModToggleOption.Create(id, label, value)))
            InternalLogger.Warn($"Failed to add ModToggleOption with id {id} to {Name}");
    }
    #endregion
}