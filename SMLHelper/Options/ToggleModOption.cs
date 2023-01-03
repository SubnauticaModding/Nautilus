namespace SMLHelper.Options
{
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Events;

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
    public class ModToggleOption : ModOption<bool>
    {
        internal override void AddToPanel(uGUI_TabbedControlsPanel panel, int tabIndex)
        {
            UnityEngine.UI.Toggle toggle = panel.AddToggleOption(tabIndex, Label, Value,
                new UnityAction<bool>((bool value) => parentOptions.OnChange<ToggleChangedEventArgs, bool>(Id, value)));

            OptionGameObject = toggle.transform.parent.gameObject;

            base.AddToPanel(panel, tabIndex);
        }

        /// <summary>
        /// Instantiates a new <see cref="ModToggleOption"/> for handling an option that can be either ON or OFF.
        /// </summary>
        /// <param name="id">The internal ID of this option.</param>
        /// <param name="label">The display text to show on the in-game menus.</param>
        /// <param name="value">The starting value.</param>
        private ModToggleOption(string id, string label, bool value) : base(label, id, value)
        { }

        /// <summary>
        /// Adds a new <see cref="ModToggleOption"/> to this instance.
        /// </summary>
        /// <param name="id">The internal ID for the toggle option.</param>
        /// <param name="label">The display text to use in the in-game menu.</param>
        /// <param name="value">The starting value.</param>
        public static ModToggleOption Factory(string id, string label, bool value)
        {
            return new ModToggleOption(id, label, value);
        }

        private class ToggleOptionAdjust: ModOptionAdjust
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
        internal override Type AdjusterComponent => typeof(ToggleOptionAdjust);
    }
}
