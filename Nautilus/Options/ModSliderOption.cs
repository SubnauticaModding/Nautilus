using System;
using System.Collections;
using System.Reflection;
using BepInEx.Logging;
using Nautilus.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Nautilus.Options;

/// <summary>
/// Contains all the information about a slider changed event.
/// </summary>
public class SliderChangedEventArgs : ConfigOptionEventArgs<float>
{
    /// <summary>
    /// Constructs a new <see cref="SliderChangedEventArgs"/>.
    /// </summary>
    /// <param name="id">The ID of the <see cref="ModSliderOption"/> that was changed.</param>
    /// <param name="value">The new value for the <see cref="ModSliderOption"/>.</param>
    public SliderChangedEventArgs(string id, float value) : base(id, value) { }
}

/// <summary>
/// A mod option class for handling an option that can have any floating point value between a minimum and maximum.
/// </summary>
public class ModSliderOption : ModOption<float, SliderChangedEventArgs>
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
    /// The default value of the <see cref="ModSliderOption"/>.
    /// Showed on the slider by small gray circle. Slider's handle will snap to the default value near it.
    /// </summary>
    public float DefaultValue { get; }

    /// <summary>
    /// The step value of the <see cref="ModSliderOption"/> defaults to 1.
    /// </summary>
    public float Step { get; } = 1;

    /// <summary> Float Format for value field (<see cref="Create(string, string, float, float, float, float?, string, float, string)"/>) </summary>
    public string ValueFormat { get; }

    /// <summary>
    /// The tooltip to show when hovering over the option.
    /// </summary>
    public string Tooltip { get; }

    private SliderValue sliderValue = null;

    /// <summary>
    /// The base method for adding an object to the options panel
    /// </summary>
    /// <param name="panel">The panel to add the option to.</param>
    /// <param name="tabIndex">Where in the panel to add the option.</param>
    public override void AddToPanel(uGUI_TabbedControlsPanel panel, int tabIndex)
    {
        UnityAction<float> callback = new((value) => {
            OnChange(Id, sliderValue?.ConvertToDisplayValue(value) ?? value);
            parentOptions.OnChange<float, SliderChangedEventArgs>(Id, sliderValue?.ConvertToDisplayValue(value) ?? value); 
        });

        panel.AddSliderOption(tabIndex, Label, (float)Value, MinValue, MaxValue, DefaultValue, Step, callback, SliderLabelMode.Default, ValueFormat, Tooltip);

        // AddSliderOption for some reason doesn't return created GameObject, so we need this little hack
        Transform options = panel.tabs[tabIndex].container.transform;
        OptionGameObject = options.GetChild(options.childCount - 1).gameObject; // last added game object

        // if we using custom value format, we need to replace vanilla uGUI_SliderWithLabel with our component
        if (ValueFormat != null)
        {
            OptionGameObject.transform.Find("Slider").gameObject.AddComponent<SliderValue>().ValueFormat = ValueFormat;
        }

        // fixing tooltip for slider
        OptionGameObject.transform.Find("Slider/Caption").GetComponent<TextMeshProUGUI>().raycastTarget = true;

        base.AddToPanel(panel, tabIndex);

        sliderValue = OptionGameObject.GetComponentInChildren<SliderValue>(); // we can also add custom SliderValue in OnGameObjectCreated event
    }

    private ModSliderOption(string id, string label, float minValue, float maxValue, float value, float? defaultValue, string valueFormat, float step, string tooltip) : base(label, id, value)
    {
        MinValue = minValue;
        MaxValue = maxValue;
        DefaultValue = defaultValue ?? value;
        ValueFormat = valueFormat;
        Step = step;
        Tooltip = tooltip;
    }

    /// <summary>
    /// Creates a new <see cref="ModSliderOption"/> to this instance.
    /// </summary>
    /// <param name="id">The internal ID for the slider option.</param>
    /// <param name="label">The display text to use in the in-game menu.</param>
    /// <param name="minValue">The minimum value for the range.</param>
    /// <param name="maxValue">The maximum value for the range.</param>
    /// <param name="value">The starting value.</param>
    /// <param name="defaultValue">The default value for the slider. If this is null then 'value' used as default.   uses value</param>
    /// <param name="step">Step for the slider, ie. round to nearest X.   defaults to 1</param>
    /// <param name="tooltip">The tooltip to show when hovering over the option. defaults to no tooltip.</param>
    /// <param name="valueFormat"> format for values when labelMode is set to <see cref="SliderLabelMode.Float"/>, e.g. "{0:F2}" for 2 decimals or "{0:F0} for no decimals %"
    /// (more on this <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings">here</see>)</param>
    public static ModSliderOption Create(string id, string label, float minValue, float maxValue, float value, float? defaultValue = null, string valueFormat = "{0:F0}", float step = 1, string tooltip = null)
    {
        return new ModSliderOption(id, label, minValue, maxValue, value, defaultValue, valueFormat, step, tooltip);
    }

    /// <summary>
    /// Component for customizing slider's value behaviour.
    /// If you need more complex behaviour than just custom value format then you can inherit this component 
    /// and add it to "Slider" game object in OnGameObjectCreated event (see <see cref="AddToPanel"/> for details on adding component)
    /// You can override value converters <see cref="ConvertToDisplayValue"/> and <see cref="ConvertToSliderValue"/>,
    /// in that case internal range for slider will be changed to [0.0f : 1.0f] and you can control displayed value with these converters
    /// (also this value will be passed to <see cref="ModOptions.OnChange"/> event)
    /// </summary>
    public class SliderValue : MonoBehaviour
    {
        /// <summary> The value label of the <see cref="SliderValue"/> </summary>
        protected TextMeshProUGUI label;

        /// <summary> The slider controlling this <see cref="SliderValue"/> </summary>
        protected Slider slider;

        /// <summary>
        /// The minimum value of the <see cref="SliderValue"/>.
        /// In case of custom value converters it can be not equal to internal minimum value for slider
        /// </summary>
        protected float minValue;

        /// <summary>
        /// The maximum value of the <see cref="SliderValue"/>.
        /// In case of custom value converters it can be not equal to internal maximum value for slider
        /// </summary>
        protected float maxValue;

        /// <summary> Custom value format property. Set it right after adding component to game object for proper behaviour </summary>
        public string ValueFormat
        {
            get => valueFormat;
            set => valueFormat = value ?? "{0}";
        }

        /// <summary> Custom value format </summary>
        protected string valueFormat = "{0}";

        /// <summary> Step for the slider </summary>
        internal float Step;

        /// <summary>
        /// Width for value text field. Used by <see cref="SliderOptionAdjust"/> to adjust label width.
        /// It is calculated in <see cref="UpdateValueWidth"/>, but you can override this property.
        /// </summary>
        public virtual float ValueWidth { get; protected set; } = -1f;

        /// <summary> Override this if you need to initialize custom value converters </summary>
        protected virtual void InitConverters() { }

        /// <summary> Converts internal slider value [0.0f : 1.0f] to displayed value </summary>
        public virtual float ConvertToDisplayValue(float sliderValue)
        {
            if (Step >= Mathf.Epsilon)
            {
                float value = Mathf.Round(slider.value / Step) * Step;

                if (value != sliderValue)
                {
                    slider.value = value;
                }

                return value;
            }
            else
            {
                return sliderValue;
            }
        }

        /// <summary> Converts displayed value to internal slider value [0.0f : 1.0f] </summary>
        public virtual float ConvertToSliderValue(float displayValue)
        {
            return displayValue;
        }

        /// <summary> Component initialization. If you overriding this, make sure that you calling base.Awake() </summary>
        protected virtual void Awake()
        {
            bool _isOverrided(string methodName)
            {
                MethodInfo method = GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
                return method.DeclaringType != method.GetBaseDefinition().DeclaringType;
            }

            bool useConverters = _isOverrided(nameof(SliderValue.ConvertToDisplayValue)) &&
                                 _isOverrided(nameof(SliderValue.ConvertToSliderValue));

            if (GetComponent<uGUI_SliderWithLabel>() is uGUI_SliderWithLabel sliderLabel)
            {
                label = sliderLabel.label;
                slider = sliderLabel.slider;
                Destroy(sliderLabel);
            }
            else
            {
                InternalLogger.Log("uGUI_SliderWithLabel not found", LogLevel.Error);
            }

            if (GetComponent<uGUI_SnappingSlider>() is uGUI_SnappingSlider snappingSlider)
            {
                minValue = snappingSlider.minValue;
                maxValue = snappingSlider.maxValue;

                // if we use overrided converters, we change range of the slider to [0.0f : 1.0f]
                if (useConverters)
                {
                    InitConverters();

                    snappingSlider.minValue = 0f;
                    snappingSlider.maxValue = 1f;
                    snappingSlider.defaultValue = ConvertToSliderValue(snappingSlider.defaultValue);
                    snappingSlider.value = ConvertToSliderValue(snappingSlider.value);
                }
            }
            else
            {
                InternalLogger.Log("uGUI_SnappingSlider not found", LogLevel.Error);
            }

            slider.onValueChanged.AddListener(new UnityAction<float>(OnValueChanged));
            UpdateLabel();
        }

        /// <summary> <see cref="MonoBehaviour"/>.Start() </summary>
        protected virtual IEnumerator Start()
        {
            return UpdateValueWidth();
        }

        /// <summary>
        /// Method for calculating necessary label's width. Creates temporary label and compares widths of min and max values,
        /// then sets <see cref="ValueWidth"/> to the wider. Be aware that in case of using custom converters some intermediate value may be wider than min/max values.
        /// </summary>
        protected virtual IEnumerator UpdateValueWidth()
        {
            // we need to know necessary width for value text field based on min/max values and value format
            GameObject tempLabel = Instantiate(label.gameObject);
            tempLabel.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            // we'll add formatted min value to the label and skip one frame for updating ContentSizeFitter
            tempLabel.GetComponent<TextMeshProUGUI>().text = string.Format(valueFormat, minValue);
            yield return null;
            float widthForMin = tempLabel.GetComponent<RectTransform>().rect.width;

            // same for max value
            tempLabel.GetComponent<TextMeshProUGUI>().text = string.Format(valueFormat, maxValue);
            yield return null;
            float widthForMax = tempLabel.GetComponent<RectTransform>().rect.width;

            Destroy(tempLabel);
            ValueWidth = Math.Max(widthForMin, widthForMax);
        }

        /// <summary> Called when user changes slider value </summary>
        protected virtual void OnValueChanged(float value)
        {
            UpdateLabel();
        }

        /// <summary>
        /// Updates label's text with formatted and converted slider's value.
        /// Override this if you need even more control on slider's value behaviour.
        /// </summary>
        protected virtual void UpdateLabel()
        {
            float val = ConvertToDisplayValue(slider.value); // doing it separately in case that valueFormat is changing in ConvertToDisplayValue
            label.text = string.Format(valueFormat, val);
        }
    }

    internal class SliderOptionAdjust : ModOptionAdjust
    {
        private const string sliderBackground = "Slider/Slider/Background";
        private const float spacing_MainMenu = 30f;
        private const float spacing_GameMenu = 10f;
        private const float valueSpacing = 15f; // used in game menu

        public IEnumerator Start()
        {
            SetCaptionGameObject("Slider/Caption", isMainMenu ? 488f : 364.7f); // need to use custom width for slider's captions
            yield return null; // skip one frame

            float sliderValueWidth = 0f;

            if (gameObject.GetComponentInChildren<SliderValue>() is SliderValue sliderValue)
            {
                // wait while SliderValue calculating ValueWidth (one or two frames)
                while (sliderValue.ValueWidth < 0)
                {
                    yield return null;
                }

                sliderValueWidth = sliderValue.ValueWidth + (isMainMenu ? 0f : valueSpacing);
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
            {
                sliderValueWidth = sliderValueRect.rect.width;
            }

            RectTransform rect = gameObject.transform.Find(sliderBackground) as RectTransform;

            if (widthDelta != 0f)
            {
                rect.localPosition = SetVec2x(rect.localPosition, rect.localPosition.x - widthDelta);
            }

            // changing width for slider
            float widthAll = gameObject.GetComponent<RectTransform>().rect.width;
            float widthSlider = rect.rect.width;
            float widthText = CaptionWidth + (isMainMenu ? spacing_MainMenu : spacing_GameMenu);

            // it's not pixel-perfect, but it's good enough
            if (widthText + widthSlider + sliderValueWidth > widthAll)
            {
                rect.sizeDelta = SetVec2x(rect.sizeDelta, widthAll - widthText - sliderValueWidth - widthSlider);
            }
            else if (widthDelta > 0f)
            {
                rect.sizeDelta = SetVec2x(rect.sizeDelta, -widthDelta);
            }

            Destroy(this);
        }
    }
    /// <summary>
    /// The Adjuster for this <see cref="OptionItem"/>.
    /// </summary>
    public override Type AdjusterComponent => typeof(SliderOptionAdjust);
}