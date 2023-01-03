namespace SMLHelper.Options
{
    using System;
    using UnityEngine.Events;
    using UnityEngine.UI;

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
        private readonly Action<ButtonClickedEventArgs> Callback;

        internal override void AddToPanel(uGUI_TabbedControlsPanel panel, int tabIndex)
        {
            // Add button to GUI
            Button componentInChildren = panel.AddItem(tabIndex, panel.buttonPrefab, Label).GetComponentInChildren<Button>();

            // Store a reference to parent object to simplify further modifications
            OptionGameObject = componentInChildren.transform.parent.gameObject;

            // Setup "on click" event
            componentInChildren.onClick.AddListener(new UnityAction(() =>
            {
                // Apply "deselected" style to button right after it is clicked
                componentInChildren.OnDeselect(null);
                // Propagate button click event to parent
                Callback(new ButtonClickedEventArgs(Id));
            }));

            // Add button to panel
            base.AddToPanel(panel, tabIndex);
        }

        private ModButtonOption(string id, string label, Action<ButtonClickedEventArgs> callback) : base(label, id)
        {
            Callback = callback;
        }

        /// <summary>
        /// Creates a new <see cref="ModButtonOption"/> for handling a button that can be clicked.
        /// </summary>
        /// <param name="id">The internal ID of this option.</param>
        /// <param name="label">The display text to show on the in-game menus.</param>
        public static ModButtonOption Factory(string id, string label, Action<ButtonClickedEventArgs> callback)
        {
            return new ModButtonOption(id, label, callback);
        }

        internal override Type AdjusterComponent => null;
    }
}
