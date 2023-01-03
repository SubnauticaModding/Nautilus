namespace SMLHelper.Options
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using SMLHelper.Utility;

    /// <summary>
    /// Abstract class that provides the framework for your mod's in-game configuration options.
    /// </summary>
    public abstract class ModOptions
    {
        /// <summary>
        /// The name of this set of configuration options.
        /// </summary>
        public string Name;

        /// <summary>
        /// Obtains the <see cref="ModOption"/>s that belong to this instance. Can be null.
        /// </summary>
        public List<OptionItem> Options => _options == null ? null : new List<OptionItem>(_options.Values);

        // This is a dictionary now in case we want to get the ModOption quickly
        // based on the provided ID.
        private Dictionary<string, OptionItem> _options;

        /// <summary>
        /// <para>Attaches a <see cref="OptionItem"/> to the options menu.</para>
        /// </summary>
        /// <param name="option">The <see cref="OptionItem"/> to add to the options menu.</param>
        public void AddItem(OptionItem option)
        {
            _options.Add(option.Id, option);
            option.SetParent(this);
        }

        internal void AddOptionsToPanel(uGUI_TabbedControlsPanel panel, int tabIndex)
        {
            panel.AddHeading(tabIndex, Name);

            _options = new Dictionary<string, OptionItem>(); // we need to do this every time we adding options
            BuildModOptions();

            _options.Values.ForEach(option => option.AddToPanel(panel, tabIndex));
        }

        /// <summary>
        /// Creates a new instance of <see cref="ModOptions"/>.
        /// </summary>
        /// <param name="name">The name that will display above this section of options in the in-game menu.</param>
        public ModOptions(string name)
        {
            Name = name;
        }

        /// <summary>
        /// <para>Builds up the configuration the options.</para>
        /// <para>This method should be composed of calls into the following method: 
        /// <seealso cref="AddItem"/> .</para>
        /// <para>Make sure you have subscribed to the events in the constructor to handle what happens when the value is changed:
        /// <seealso cref="OnChanged"/>.</para>
        /// </summary>
        public abstract void BuildModOptions();

        /// <summary>
        /// The event that is called whenever an option is changed. Subscribe to this in the constructor.
        /// </summary>
        public event EventHandler<OptionEventArgs> OnChanged;

        /// <summary>
        /// Notifies an option change to all subscribed event handlers.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        internal void OnChange<T, V>(string id, V value) where T : ConfigOptionEventArgs<V>
        {
            OnChanged(this, (T)Activator.CreateInstance(typeof(T), new object[] { id, value }));
        }

        /// <summary> The event that is called whenever a game object created for the option </summary>
        protected event EventHandler<GameObjectCreatedEventArgs> GameObjectCreated;

        internal void OnGameObjectCreated(string id, GameObject gameObject)
        {
            GameObjectCreated?.Invoke(this, new GameObjectCreatedEventArgs(id, gameObject));
        }
    }

    /// <summary> Contains all the information about a created game object event </summary>
    public class GameObjectCreatedEventArgs : ConfigOptionEventArgs<GameObject>
    {
        /// <summary> Constructs a new <see cref="GameObjectCreatedEventArgs"/> </summary>
        /// <param name="id"> The ID of the <see cref="ModOption"/> for which game object was created </param>
        /// <param name="gameObject"> New game object for the <see cref="ModOption"/> </param>
        public GameObjectCreatedEventArgs(string id, GameObject gameObject) : base(id, gameObject) { }
    }

    /// <summary>
    /// The common abstract class to all mod options.
    /// </summary>
    public abstract class ModOption : OptionItem
    {
        private readonly Type MyType;

        /// <summary>
        /// The type of the <see cref="Value"/> for the <see cref="ModOption"/>.
        /// </summary>
        public Type GetValueType => MyType;

        /// <summary>
        /// The value for the <see cref="ModOption"/>.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Base constructor for all mod options.
        /// </summary>
        /// <param name="label">The display text to show on the in-game menus.</param>
        /// <param name="id">The internal ID if this option.</param>
        /// <param name="T">The type of the object for casting purposes if necessary.</param>
        /// <param name="value">The generic value of the <see cref="ModOption"/>.</param>
        internal ModOption(string label, string id, Type T, object value) : base(label, id)
        {
            MyType = T;
            Value = value;
        }
    }

    /// <summary>
    /// The common generic-typed abstract class to all mod options.
    /// </summary>
    public abstract class ModOption<T> : ModOption
    {
        /// <summary>
        /// The value for the <see cref="ModOption{T}"/>.
        /// </summary>
        public new T Value { get; }

        /// <summary>
        /// Base constructor for all mod options.
        /// </summary>
        /// <param name="label">The display text to show on the in-game menus.</param>
        /// <param name="id">The internal ID if this option.</param>
        /// <param name="value">The typed value of the <see cref="ModOption"/></param>
        internal ModOption(string label, string id, T value) : base(label, id, typeof(T), value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// The common abstract class to all items in the mod options page.
    /// </summary>
    public abstract class OptionItem {
        /// <summary>
        /// The internal ID that identifies this option.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The display text to be shown for this option in the in-game menus.
        /// </summary>
        public string Label { get; }

        /// <summary> UI GameObject for this option </summary>
        public GameObject OptionGameObject { get; protected set; }

        /// <summary> Parent <see cref="ModOptions"/> for this option </summary>
        protected ModOptions parentOptions;

        internal void SetParent(ModOptions parent)
        {
            if (parentOptions == null)
            {
                parentOptions = parent;
            }
            else
            {
                InternalLogger.Log($"ModOption.SetParent: parent already setted for {Id}", LogLevel.Warn);
            }
        }

        // adds UI GameObject to panel and updates OptionGameObject
        internal virtual void AddToPanel(uGUI_TabbedControlsPanel panel, int tabIndex)
        {
            if (AdjusterComponent != null)
            {
                OptionGameObject.AddComponent(AdjusterComponent);
            }

            parentOptions.OnGameObjectCreated(Id, OptionGameObject);
        }

        internal OptionItem(string label, string id)
        {
            Id = id;
            Label = label;
        }

        // type of component derived from ModOptionAdjust (for using in base.AddToPanel)
        internal abstract Type AdjusterComponent { get; }

        // base class for 'adjuster' components (so ui elements don't overlap with their text labels)
        // reason for using components is to skip one frame before manually adjust ui elements to make sure that Unity UI Layout components is updated
        internal abstract class ModOptionAdjust : MonoBehaviour
        {
            private const float minCaptionWidth_MainMenu = 480f;
            private const float minCaptionWidth_GameMenu = 360f;
            private GameObject caption = null;

            protected float CaptionWidth { get => caption?.GetComponent<RectTransform>().rect.width ?? 0f; }

            protected bool isMainMenu { get; private set; } = true; // is it main menu or game menu

            protected static Vector2 SetVec2x(Vector2 vec, float val) { vec.x = val; return vec; }

            public void Awake()
            {
                isMainMenu = gameObject.GetComponentInParent<MainMenuOptions>() != null;
            }

            // we add ContentSizeFitter component to text label so it will change width in its Update() based on text
            protected void SetCaptionGameObject(string gameObjectPath, float minWidth = 0f)
            {
                caption = gameObject.transform.Find(gameObjectPath)?.gameObject;

                if (!caption)
                {
                    InternalLogger.Log($"ModOptionAdjust: caption gameobject '{gameObjectPath}' not found", LogLevel.Warn);
                    return;
                }

                caption.AddComponent<LayoutElement>().minWidth = minWidth != 0f ? minWidth : (isMainMenu ? minCaptionWidth_MainMenu : minCaptionWidth_GameMenu);
                caption.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize; // for autosizing captions

                RectTransform transform = caption.GetComponent<RectTransform>();
                transform.pivot = SetVec2x(transform.pivot, 0f);
                transform.anchoredPosition = SetVec2x(transform.anchoredPosition, 0f);
            }
        }
    }
}
