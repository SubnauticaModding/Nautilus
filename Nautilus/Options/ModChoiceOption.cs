using System;
using System.Collections;
using System.Collections.Generic;
using Nautilus.Options.Utility;
using UnityEngine;
using UnityEngine.Events;

namespace Nautilus.Options;

/// <summary>
/// Contains all the information about a choice changed event.
/// </summary>
public class ChoiceChangedEventArgs<T> : ConfigOptionEventArgs<T>
{

    /// <summary>
    /// The new index for the <see cref="ModChoiceOption{T}"/>.
    /// </summary>
    public int Index { get; }


    /// <summary>
    /// Constructs a new <see cref="ChoiceChangedEventArgs{T}"/>.
    /// </summary>
    /// <param name="id">The ID of the <see cref="ModChoiceOption{T}"/> that was changed.</param>
    /// <param name="index">The new index for the <see cref="ModChoiceOption{T}"/>.</param>
    /// <param name="value">The value of the <see cref="ModChoiceOption{T}"/> as a string.</param>
    public ChoiceChangedEventArgs(string id, int index, T value) : base(id, value) 
    {
        Index = index;
    }
}

/// <summary>
/// A mod option class for handling an option that can select one item from a list of values.
/// </summary>
public class ModChoiceOption<T> : ModOption<T, ChoiceChangedEventArgs<T>>
{
    /// <summary>
    /// The actual <see cref="uGUI_Choice"/> when the menu is open.
    /// </summary>
    public uGUI_Choice Choice { get; private set; }

    /// <summary>
    /// The array of readable string options to choose between in the <see cref="ModChoiceOption{T}"/>.
    /// </summary>
    public T[] Options { get; }

    private string[] OptionStrings { get; }

    /// <summary>
    /// The currently selected index among the options array.
    /// </summary>
    public int Index { get; private set; }

    /// <summary>
    /// The tooltip to show when hovering over the option.
    /// </summary>
    public string Tooltip { get; }

    /// <summary>
    /// The base method for adding an object to the options panel
    /// </summary>
    /// <param name="panel">The panel to add the option to.</param>
    /// <param name="tabIndex">Where in the panel to add the option.</param>
    public override void AddToPanel(uGUI_TabbedControlsPanel panel, int tabIndex)
    {
        Choice = panel.AddChoiceOption(tabIndex, Label, OptionStrings, Index,
            new UnityAction<int>((int index) => {
                Index = index;
                OnChange(Id, Options[index]);
                parentOptions.OnChange<T, ChoiceChangedEventArgs<T>>(Id, Options[index]); 
            }), Tooltip);

        OptionGameObject = Choice.transform.parent.transform.parent.gameObject; // :(

        base.AddToPanel(panel, tabIndex);
    }

    private ModChoiceOption(string id, string label, T[] options, int index, string tooltip) : base(label, id, options[index])
    {
        Options = options;
        List<string> optionStrings = new List<string>();
        foreach(var option in options)
        {
            if (option is Color color)
                optionStrings.Add($"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{color}</color>");
            else
                optionStrings.Add(option.ToString());
        }
        OptionStrings = optionStrings.ToArray();
        Index = index;
        Tooltip = tooltip;
    }

    /// <summary>
    /// Adds a new <see cref="ModChoiceOption{T}"/> to this instance.
    /// </summary>
    /// <param name="id">The internal ID for the choice option.</param>
    /// <param name="label">The display text to use in the in-game menu.</param>
    /// <param name="options">The collection of available values.</param>
    /// <param name="index">The starting value.</param>
    /// <param name="tooltip">The tooltip to show when hovering over the option.</param>
    public static ModChoiceOption<T> Create(string id, string label, T[] options, int index, string tooltip = null)
    {
        if (Validator.ValidateChoiceOrDropdownOption<T>(id, label, options, index))
        {
            return new ModChoiceOption<T>(id, label, options, index, tooltip);
        }
        // Should never happen
        throw new ArgumentException("ModChoiceOption - could not create instance");
    }
    /// <summary>
    /// Adds a new <see cref="ModChoiceOption{T}"/> to this instance.
    /// </summary>
    /// <param name="id">The internal ID for the choice option.</param>
    /// <param name="label">The display text to use in the in-game menu.</param>
    /// <param name="options">The collection of available values.</param>
    /// <param name="value">The starting value.</param>
    /// <param name="tooltip">The tooltip to show when hovering over the option.</param>
    public static ModChoiceOption<T> Create(string id, string label, T[] options, T value, string tooltip = null)
    {
        int index = Array.IndexOf(options, value);
        if (index < 0)
        {
            index = 0;
        }

        return Create(id, label, options, index, tooltip);
    }

    /// <summary>
    /// The Adjuster for this <see cref="OptionItem"/>.
    /// </summary>
    public override Type AdjusterComponent => typeof(ChoiceOptionAdjust);
}
internal class ChoiceOptionAdjust: OptionItem.ModOptionAdjust
{
    private const float spacing = 10f;

    public IEnumerator Start()
    {
        SetCaptionGameObject("Choice/Caption");
        yield return null; // skip one frame

        RectTransform rect = gameObject.transform.Find("Choice/Background") as RectTransform;

        float widthAll = gameObject.GetComponent<RectTransform>().rect.width;
        float widthChoice = rect.rect.width;
        float widthText = CaptionWidth + spacing;

        if(widthText + widthChoice > widthAll)
        {
            rect.sizeDelta = SetVec2x(rect.sizeDelta, widthAll - widthText - widthChoice);
        }

        Destroy(this);
    }
}