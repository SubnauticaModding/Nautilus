namespace SMLHelper.V2.Options
{
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.Events;

    using Object = UnityEngine.Object;

    /// <summary>
    /// Contains all the information about a slider changed event.
    /// </summary>
    public class SliderChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The ID of the <see cref="ModSliderOption"/> that was changed.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The new value for the <see cref="ModSliderOption"/>.
        /// </summary>
        public float Value { get; }

        /// <summary>
        /// The new value for the <see cref="ModSliderOption"/> parsed as an <see cref="int"/>
        /// </summary>
        public int IntegerValue { get; }

        /// <summary>
        /// Constructs a new <see cref="SliderChangedEventArgs"/>.
        /// </summary>
        /// <param name="id">The ID of the <see cref="ModSliderOption"/> that was changed.</param>
        /// <param name="value">The new value for the <see cref="ModSliderOption"/>.</param>
        public SliderChangedEventArgs(string id, float value)
        {
            this.Id = id;
            this.Value = value;
            this.IntegerValue = Mathf.RoundToInt(value);
        }
    }

    public abstract partial class ModOptions
    {
        /// <summary>
        /// The event that is called whenever a slider is changed. Subscribe to this in the constructor.
        /// </summary>
        protected event EventHandler<SliderChangedEventArgs> SliderChanged;

        /// <summary>
        /// Notifies a slider change to all subsribed event handlers.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        internal void OnSliderChange(string id, float value)
        {
            SliderChanged(this, new SliderChangedEventArgs(id, value));
        }

        /// <summary>
        /// Adds a new <see cref="ModSliderOption"/> to this instance.
        /// </summary>
        /// <param name="id">The internal ID for the slider option.</param>
        /// <param name="label">The display text to use in the in-game menu.</param>
        /// <param name="minValue">The minimum value for the range.</param>
        /// <param name="maxValue">The maximum value for the range.</param>
        /// <param name="value">The starting value.</param>
        protected void AddSliderOption(string id, string label, float minValue, float maxValue, float value)
        {
            AddSliderOption(id, label, minValue, maxValue, value, null);
        }

        /// <summary>
        /// Adds a new <see cref="ModSliderOption"/> to this instance.
        /// </summary>
        /// <param name="id">The internal ID for the slider option.</param>
        /// <param name="label">The display text to use in the in-game menu.</param>
        /// <param name="minValue">The minimum value for the range.</param>
        /// <param name="maxValue">The maximum value for the range.</param>
        /// <param name="value">The starting value.</param>
        /// <param name="valueFormat"> format for value, e.g. "{0:F2}" or "{0:F0} %"
        /// (more on this <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings">here</see>)</param>
        protected void AddSliderOption(string id, string label, float minValue, float maxValue, float value, string valueFormat)
        {
            AddOption(new ModSliderOption(id, label, minValue, maxValue, value, valueFormat));
        }
    }

    /// <summary>
    /// A mod option class for handling an option that can have any floating point value between a minimum and maximum.
    /// </summary>
    public class ModSliderOption : ModOption
    {
        /// <summary>
        /// The minimum value of the <see cref="ModSliderOption"/>.
        /// </summary>
        public float MinValue { get; }

        /// <summary>
        /// The maximum value of the <see cref="ModSliderOption"/>.
        /// </summary>
        public float MaxValue { get; }

        /// <summary>
        /// The current value of the <see cref="ModSliderOption"/>.
        /// </summary>
        public float Value { get; }

        /// <summary> Format for value field (<see cref="ModOptions.AddSliderOption(string, string, float, float, float, string)"/>) </summary>
        public string ValueFormat { get; }

        internal override void AddToPanel(uGUI_TabbedControlsPanel panel, int tabIndex)
        {
            panel.AddSliderOption(tabIndex, Label, Value, MinValue, MaxValue, Value,
                new UnityAction<float>((float value) => parentOptions.OnSliderChange(Id, value)));

            // AddSliderOption for some reason doesn't return created GameObject, so we need this little hack
            Transform options = panel.tabs[tabIndex].container.transform;
            OptionGameObject = options.GetChild(options.childCount - 1).gameObject; // last added game object

            // if we using custom value format, we need to replace vanilla uGUI_SliderWithLabel with our component
            if (ValueFormat != null)
            {
                GameObject sliderObject = OptionGameObject.transform.Find("Slider").gameObject;
                uGUI_SliderWithLabel sliderLabel = sliderObject.GetComponent<uGUI_SliderWithLabel>();
                sliderObject.AddComponent<SliderLabel>().Init(sliderLabel.label, sliderLabel.slider, this);
                Object.Destroy(sliderLabel);
            }

            base.AddToPanel(panel, tabIndex);
        }

        /// <summary>
        /// Instantiates a new <see cref="ModSliderOption"/> for handling an option that can have any floating point value between a minimum and maximum.
        /// </summary>
        /// <param name="id">The internal ID of this option.</param>
        /// <param name="label">The display text to show on the in-game menus.</param>
        /// <param name="minValue">The minimum value for the range.</param>
        /// <param name="maxValue">The maximum value for the range.</param>
        /// <param name="value">The starting value.</param>
        /// <param name="valueFormat">Format for value field (<see cref="ModOptions.AddSliderOption(string, string, float, float, float, string)"/>) </param>
        internal ModSliderOption(string id, string label, float minValue, float maxValue, float value, string valueFormat = null) : base(label, id)
        {
            this.MinValue = minValue;
            this.MaxValue = maxValue;
            this.Value = value;
            this.ValueFormat = valueFormat;
        }

        // component for showing slider value with custom format
        private class SliderLabel: MonoBehaviour
        {
            private Text label;
            private Slider slider;

            private string valueFormat = null;
            private ModSliderOption sliderOption;

            public float ValueWidth { get; private set; } = -1f; // width for value text field

            public void Init(Text _label, Slider _slider, ModSliderOption _sliderOption)
            {
                label = _label;
                slider = _slider;
                sliderOption = _sliderOption;

                valueFormat = sliderOption.ValueFormat;
            }

            public IEnumerator Start()
            {
                slider.onValueChanged.AddListener(new UnityAction<float>(OnValueChanged));
                UpdateLabel();

                // we need to know necessary width for value text field based on min/max values and value format
                GameObject tempLabel = Instantiate(label.gameObject);
                tempLabel.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

                // we'll add formatted min value to the label and skip one frame for updating ContentSizeFitter
                tempLabel.GetComponent<Text>().text = string.Format(valueFormat, sliderOption.MinValue);
                yield return null;
                float widthForMin = tempLabel.GetComponent<RectTransform>().rect.width;

                // same for max value
                tempLabel.GetComponent<Text>().text = string.Format(valueFormat, sliderOption.MaxValue);
                yield return null;
                float widthForMax = tempLabel.GetComponent<RectTransform>().rect.width;

                Destroy(tempLabel);
                ValueWidth = Math.Max(widthForMin, widthForMax);
            }

            private void OnValueChanged(float value) => UpdateLabel();

            private void UpdateLabel() => label.text = string.Format(valueFormat, slider.value);
        }


        private class SliderOptionAdjust: ModOptionAdjust
        {
            private const float spacing_MainMenu = 30f;
            private const float spacing_GameMenu = 10f;
            private const float valueSpacing = 15f; // used in game menu

            public IEnumerator Start()
            {
                SetCaptionGameObject("Slider/Caption", isMainMenu? 488f: 364.7f); // need to use custom width for slider's captions
                yield return null; // skip one frame

                // for some reason sliders don't update their handle positions sometimes
                uGUI_SnappingSlider slider = gameObject.GetComponentInChildren<uGUI_SnappingSlider>();
                Harmony.AccessTools.Method(typeof(Slider), "UpdateVisuals")?.Invoke(slider, null);

                float sliderValueWidth = 0f;

                if (gameObject.GetComponentInChildren<SliderLabel>() is SliderLabel sliderLabel)
                {
                    // wait while SliderLabel calculating ValueWidth (one or two frames)
                    while (sliderLabel.ValueWidth == -1f)
                        yield return null;

                    sliderValueWidth = sliderLabel.ValueWidth + (isMainMenu? 0f: valueSpacing);
                }

                // changing width for slider value label (we don't change width for default format!)
                float widthDelta = 0f;
                RectTransform sliderValueRect = gameObject.transform.Find("Slider/Value") as RectTransform;

                if (sliderValueWidth > sliderValueRect.rect.width)
                {
                    widthDelta = sliderValueWidth - sliderValueRect.rect.width;
                    sliderValueRect.sizeDelta = SetVec2x(sliderValueRect.sizeDelta, sliderValueWidth);
                }
                else
                    sliderValueWidth = sliderValueRect.rect.width;

                RectTransform rect = gameObject.transform.Find("Slider/Background") as RectTransform;

                if (widthDelta != 0f)
                    rect.localPosition = SetVec2x(rect.localPosition, rect.localPosition.x - widthDelta);

                // changing width for slider
                float widthAll = gameObject.GetComponent<RectTransform>().rect.width;
                float widthSlider = rect.rect.width;
                float widthText = CaptionWidth + (isMainMenu? spacing_MainMenu: spacing_GameMenu);

                // it's not pixel-perfect, but it's good enough
                if (widthText + widthSlider + sliderValueWidth > widthAll)
                    rect.sizeDelta = SetVec2x(rect.sizeDelta, widthAll - widthText - sliderValueWidth - widthSlider);
                else if (widthDelta > 0f)
                    rect.sizeDelta = SetVec2x(rect.sizeDelta, -widthDelta);

                Destroy(this);
            }
        }
        internal override Type AdjusterComponent => typeof(SliderOptionAdjust);
    }
}
