namespace SMLHelper.Options
{
    using System;
    using UnityEngine.Events;
    using UnityEngine.UI;

    /// <summary>
    /// Contains all the information about a button click event.
    /// </summary>
    public class ButtonClickedEventArgs : ModEventArgs<bool?>
    {
        /// <summary>
        /// The ID of the <see cref="ModButtonOption"/> that was clicked.
        /// </summary>
        public override string Id { get; }

        /// <summary>
        /// The new value for the <see cref="ModButtonOption"/>. Always null.
        /// </summary>
        public override bool? Value { get; }

        /// <summary>
        /// Constructs a new <see cref="ButtonClickedEventArgs"/>.
        /// </summary>
        /// <param name="id">The ID of the <see cref="ModButtonOption"/> that was clicked.</param>
        /// <param name="value">The value of the <see cref="ModButtonOption"/> that was clicked (null).</param>
        public ButtonClickedEventArgs(string id, bool? value = null)
        {
            this.Id = id;
            this.Value = value;
        }
    }

    /// <summary>
    /// A mod option class for handling a button that can be clicked.
    /// </summary>
    public class ModButtonOption : ModOption
    {
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
                parentOptions.OnChange<ButtonClickedEventArgs, bool?>(Id, null);
            }));

            // Add button to panel
            base.AddToPanel(panel, tabIndex);
        }

        internal ModButtonOption(string id, string label) : base(label, id, typeof(bool?), null) { }

        /// <summary>
        /// Creates a new <see cref="ModButtonOption"/> for handling a button that can be clicked.
        /// </summary>
        /// <param name="id">The internal ID of this option.</param>
        /// <param name="label">The display text to show on the in-game menus.</param>
        public static ModButtonOption Factory(string id, string label)
        {
            return new ModButtonOption(id, label);
        }

        internal override Type AdjusterComponent => null;
    }
}
