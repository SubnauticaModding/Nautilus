using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using Nautilus.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Nautilus.Options;

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
    /// Obtains the <see cref="OptionItem"/>s that belong to this instance. Can be null.
    /// </summary>
    public IReadOnlyCollection<OptionItem> Options => _options.Values;

    // This is a dictionary now in case we want to get the ModOption quickly
    // based on the provided ID.
    private readonly Dictionary<string, OptionItem> _options = new Dictionary<string, OptionItem>();

    /// <summary>
    /// <para>Attaches a <see cref="OptionItem"/> to the options menu.</para>
    /// </summary>
    /// <param name="option">The <see cref="OptionItem"/> to add to the options menu.</param>
    public bool AddItem(OptionItem option)
    {
        if(_options.ContainsKey(option.Id))
        {
            return false;
        }
        _options.Add(option.Id, option);
        option.SetParent(this);
        return true;
    }

    /// <summary>
    /// <para>Attaches a <see cref="OptionItem"/> to the options menu.</para>
    /// </summary>
    /// <param name="id">The id of the <see cref="OptionItem"/> to remove from the options menu.</param>
    public bool RemoveItem(string id)
    {
        if(!_options.TryGetValue(id, out OptionItem optionItem))
        {
            return false;
        }

        _options.Remove(id);
        optionItem.SetParent(null);
        if(optionItem.OptionGameObject != null)
            GameObject.Destroy(optionItem.OptionGameObject);
        return true;
    }


    internal void AddOptionsToPanel(uGUI_TabbedControlsPanel panel, int modsTabIndex)
    {
        try
        {
            BuildModOptions(panel, modsTabIndex, Options);
        }
        catch(Exception ex)
        {
            InternalLogger.Error($"{Name} Failed to BuildModOptions with exception: \n {ex}");
        }
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
    /// Builds up the configuration the options.
    /// </summary>
    public virtual void BuildModOptions(uGUI_TabbedControlsPanel panel, int modsTabIndex, IReadOnlyCollection<OptionItem> options)
    {
        panel.AddHeading(modsTabIndex, Name);
        options.ForEach(option => option.AddToPanel(panel, modsTabIndex));
    }

    /// <summary>
    /// The event that is called whenever an option is changed.
    /// </summary>
    public event EventHandler<OptionEventArgs> OnChanged;

    /// <summary>
    /// Gets the Invocation List for the OnChanged event or returns null if none present.
    /// </summary>
    public List<EventHandler<OptionEventArgs>> GetDelegates() => OnChanged?.GetInvocationList().Cast<EventHandler<OptionEventArgs>>().ToList();


    /// <summary>
    /// Notifies an option change to all subscribed event handlers.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="value"></param>
    public void OnChange<T, E>(string id, T value) where E : ConfigOptionEventArgs<T>
    {
        if(_options.TryGetValue(id, out var option) && option is ModChoiceOption<T> modChoiceOption)
        {
            OnChanged?.Invoke(this, (E)Activator.CreateInstance(typeof(E), new object[] { id, modChoiceOption.Index, value }));
            return;
        }
        OnChanged?.Invoke(this, (E)Activator.CreateInstance(typeof(E), new object[] { id, value }));
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
    /// <param name="id"> The ID of the <see cref="OptionItem"/> for which game object was created </param>
    /// <param name="gameObject"> New game object for the <see cref="OptionItem"/> </param>
    public GameObjectCreatedEventArgs(string id, GameObject gameObject) : base(id, gameObject) { }
}

/// <summary>
/// The common generic-typed abstract class to all mod options.
/// </summary>
public abstract class ModOption<T, E> : OptionItem where E: ConfigOptionEventArgs<T>
{
    /// <summary>
    /// The value for the <see cref="ModOption{T, E}"/>.
    /// </summary>
    public T Value { get; set; }

    /// <summary>
    /// The event that is called whenever an option is changed.
    /// </summary>
    public event EventHandler<E> OnChanged;

    /// <summary>
    /// Gets the Invocation List for the OnChanged event or returns null if none present.
    /// </summary>
    public IEnumerable<EventHandler<E>> GetDelegates()
    {
        return OnChanged?.GetInvocationList().Cast<EventHandler<E>>();
    }

    /// <summary>
    /// Notifies an option change to all subscribed event handlers.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="value"></param>
    public void OnChange(string id, T value)
    {
        Value = value;
        if(this is ModChoiceOption<T> modChoiceOption)
        {
            OnChanged?.Invoke(this, (E)Activator.CreateInstance(typeof(E), new object[] { id, modChoiceOption.Index, value }));
            return;
        }
        OnChanged?.Invoke(this, (E)Activator.CreateInstance(typeof(E), new object[] { id, value }));
    }

    /// <summary>
    /// Base constructor for all typed mod options.
    /// </summary>
    /// <param name="label">The display text to show on the in-game menus.</param>
    /// <param name="id">The internal ID if this option.</param>
    /// <param name="value">The typed value of the <see cref="OptionItem"/></param>
    public ModOption(string label, string id, T value) : base(label, id)
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
        parentOptions = parent;
    }

    // adds UI GameObject to panel and updates OptionGameObject
    /// <summary>
    /// The base method for adding an object to the options panel
    /// </summary>
    /// <param name="panel">The panel to add the option to.</param>
    /// <param name="tabIndex">Where in the panel to add the option.</param>
    public virtual void AddToPanel(uGUI_TabbedControlsPanel panel, int tabIndex)
    {
        if (AdjusterComponent != null)
        {
            OptionGameObject.AddComponent(AdjusterComponent);
        }

        parentOptions.OnGameObjectCreated(Id, OptionGameObject);
    }

    /// <summary>
    /// Base constructor for all items in the options.
    /// </summary>
    /// <param name="label">The display text to show on the in-game menus.</param>
    /// <param name="id">The internal ID if this option.</param>
    public OptionItem(string label, string id)
    {
        Id = id;
        Label = label;
    }

    // type of component derived from ModOptionAdjust (for using in base.AddToPanel)
    /// <summary>
    /// The Adjuster for this <see cref="OptionItem"/>.
    /// </summary>
    public abstract Type AdjusterComponent { get; }

    // base class for 'adjuster' components (so ui elements don't overlap with their text labels)
    // reason for using components is to skip one frame before manually adjust ui elements to make sure that Unity UI Layout components is updated
    /// <summary>
    /// The base 'adjuster' component to prevent UI elements overlapping
    /// </summary>
    public abstract class ModOptionAdjust : MonoBehaviour
    {
        private const float minCaptionWidth_MainMenu = 480f;
        private const float minCaptionWidth_GameMenu = 360f;
        private GameObject caption = null;

        /// <summary>
        /// The width of the caption for the component
        /// </summary>
        protected float CaptionWidth { get => caption?.GetComponent<RectTransform>().rect.width ?? 0f; }

        /// <summary>
        /// Whether we are in the main menu or in game in the options
        /// </summary>
        protected bool isMainMenu { get; private set; } = true; // is it main menu or game menu

        /// <summary>
        /// Sets the X coordinate of a <see cref="Vector2"/>.
        /// </summary>
        /// <param name="vec">The <see cref="Vector2"/> to set the value on.</param>
        /// <param name="val">The value to set to the x coordinate.</param>
        /// <returns></returns>
        protected static Vector2 SetVec2x(Vector2 vec, float val) { vec.x = val; return vec; }

        /// <summary>
        /// The function called after this <see cref="MonoBehaviour"/> is awakened.
        /// </summary>
        public void Awake()
        {
            isMainMenu = gameObject.GetComponentInParent<uGUI_MainMenu>() != null;
        }

        // we add ContentSizeFitter component to text label so it will change width in its Update() based on text
        /// <summary>
        /// Creates and adds a caption to this GameObject
        /// </summary>
        /// <param name="gameObjectPath"></param>
        /// <param name="minWidth"></param>
        protected void SetCaptionGameObject(string gameObjectPath, float minWidth = 0f)
        {
            caption = gameObject.transform.Find(gameObjectPath)?.gameObject;

            if (!caption)
            {
                InternalLogger.Log($"ModOptionAdjust: caption gameobject '{gameObjectPath}' not found", LogLevel.Warning);
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