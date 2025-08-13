using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Nautilus.Options;

/// <summary>
/// Contains all the information about a button click event.
/// </summary>
public class ButtonClickedEventArgs : OptionEventArgs
{
    /// <summary>
    /// Constructs a new <see cref="ButtonClickedEventArgs"/>.
    /// </summary>
    /// <param name="id">The ID of the <see cref="ModButtonOption"/> that was clicked.</param>
    public ButtonClickedEventArgs(string id) : base(id) { }
}

/// <summary>
/// A mod option class for handling a button that can be clicked.
/// </summary>
public class ModButtonOption : OptionItem
{
    /// <summary>
    /// The event that is called whenever an option is changed.
    /// </summary>
    public event Action<ButtonClickedEventArgs> OnPressed;

    /// <summary>
    /// The tooltip to show when hovering over the option.
    /// </summary>
    public string Tooltip { get; }

    /// <summary>
    /// Gets the Invocation List for the OnPressed event or returns null if none present.
    /// </summary>
    public IEnumerable<Action<ButtonClickedEventArgs>> GetDelegates()
    {
        return OnPressed?.GetInvocationList().Cast<Action<ButtonClickedEventArgs>>();
    }

    /// <summary>
    /// The base method for adding an object to the options panel
    /// </summary>
    /// <param name="panel">The panel to add the option to.</param>
    /// <param name="tabIndex">Where in the panel to add the option.</param>
    public override void AddToPanel(uGUI_TabbedControlsPanel panel, int tabIndex)
    {
        // Add button to GUI
#if SUBNAUTICA
        Button componentInChildren = panel.AddItem(tabIndex, panel.controls.prefabButton).GetComponentInChildren<Button>();
        uGUI_Controls.AssignLabel(componentInChildren.gameObject, Label);
#else
        Button componentInChildren = panel.AddItem(tabIndex, panel.buttonPrefab, Label).GetComponentInChildren<Button>();
#endif
        

        // Store a reference to parent object to simplify further modifications
        OptionGameObject = componentInChildren.transform.parent.gameObject;

        // Setup "on click" event
        componentInChildren.onClick.AddListener(new UnityAction(() =>
        {
            // Apply "deselected" style to button right after it is clicked
            componentInChildren.OnDeselect(null);
            OnPressed?.Invoke(new ButtonClickedEventArgs(Id));
        }));

        // Add tooltip
        MenuTooltip tooltip = OptionGameObject.EnsureComponent<MenuTooltip>();
        tooltip.key = Tooltip;

        // Add button to panel
        base.AddToPanel(panel, tabIndex);
    }

    private ModButtonOption(string id, string label, Action<ButtonClickedEventArgs> onPressed, string tooltip) : base(label, id)
    {
        if (onPressed != null)
        {
            OnPressed += onPressed;
        }
        Tooltip = tooltip;
    }

    /// <summary>
    /// Creates a new <see cref="ModButtonOption"/> for handling a button that can be clicked.
    /// </summary>
    /// <param name="id">The internal ID of this option.</param>
    /// <param name="label">The display text to show on the in-game menus.</param>
    /// <param name="onPressed"> Action to trigger when button is pressed. Can leave as Null and then add events using the OnPressed += method;</param>
    /// <param name="tooltip">The tooltip to show when hovering over the option.</param>
    public static ModButtonOption Create(string id, string label, Action<ButtonClickedEventArgs> onPressed = null, string tooltip = null)
    {
        return new ModButtonOption(id, label, onPressed, tooltip);
    }

    /// <summary>
    /// The Adjuster for this <see cref="OptionItem"/>.
    /// </summary>
    public override Type AdjusterComponent => null;
}