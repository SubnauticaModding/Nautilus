using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Nautilus.Options;

/// <summary>
/// Contains all the information about a toggle changed event.
/// </summary>
public class ToggleChangedEventArgs : ConfigOptionEventArgs<bool>
{
    /// <summary>
    /// Constructs a new <see cref="ToggleChangedEventArgs"/>.
    /// </summary>
    /// <param name="id">The ID of the <see cref="ModToggleOption"/> that was changed.</param>
    /// <param name="value">The new value for the <see cref="ModToggleOption"/>.</param>
    public ToggleChangedEventArgs(string id, bool value) : base(id, value) { }
}

/// <summary>
/// A mod option class for handling an option that can be either ON or OFF.
/// </summary>
public class ModToggleOption : ModOption<bool, ToggleChangedEventArgs>
{
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
        UnityEngine.UI.Toggle toggle = panel.AddToggleOption(tabIndex, Label, Value,
            new UnityAction<bool>((bool value) => {
                OnChange(Id, value);
                parentOptions.OnChange<bool, ToggleChangedEventArgs>(Id, value); 
            }), Tooltip);

        OptionGameObject = toggle.transform.parent.gameObject;

        base.AddToPanel(panel, tabIndex);
    }

    private ModToggleOption(string id, string label, bool value, string tooltip) : base(label, id, value)
    {
        Tooltip = tooltip;
    }

    /// <summary>
    /// Adds a new <see cref="ModToggleOption"/> to this instance.
    /// </summary>
    /// <param name="id">The internal ID for the toggle option.</param>
    /// <param name="label">The display text to use in the in-game menu.</param>
    /// <param name="value">The starting value.</param>
    /// <param name="tooltip">The tooltip to show when hovering over the option. defaults to no tooltip.</param>
    public static ModToggleOption Create(string id, string label, bool value, string tooltip = null)
    {
        return new ModToggleOption(id, label, value, tooltip);
    }

    internal class ToggleOptionAdjust: ModOptionAdjust
    {
        private const float spacing = 20f;

        public IEnumerator Start()
        {
            SetCaptionGameObject("Toggle/Caption");
            yield return null;

            Transform check = gameObject.transform.Find("Toggle/Background");

            if (CaptionWidth + spacing > check.localPosition.x)
            {
                check.localPosition = SetVec2x(check.localPosition, CaptionWidth + spacing);
            }

            Destroy(this);
        }
    }

    /// <summary>
    /// The Adjuster for this <see cref="OptionItem"/>.
    /// </summary>
    public override Type AdjusterComponent => typeof(ToggleOptionAdjust);
}