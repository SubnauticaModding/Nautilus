using UnityEngine;

namespace Nautilus.Handlers.TitleScreen;

/// <summary>
/// Handles changes to the sky in the main menu.
/// </summary>
/// <remarks>
/// Only one instance is ever expected to exist at once. Otherwise, one instance will be arbitrarily chosen.
/// </remarks>
/// <seealso cref="uSkyManager"/>
public class SkyChangeTitleAddon : TitleAddon, IManagedUpdateBehaviour
{
    private const float RevertToDefaultSkyDuration = 2f;

    private readonly float _fadeInDuration;
    private readonly Settings _settings;

    private Settings? _previousSettings;
    private float _timeTransitionStarted;
    private bool _transitionActive;
    private bool _revertingToDefaultSky;

    private static Settings? _defaultSettings;
    private static SkyChangeTitleAddon _activeSkyChange;

    int IManagedUpdateBehaviour.managedUpdateIndex { get; set; }

    /// <summary>
    /// Activates the specified <see cref="uSkyManager"/> settings when the mod is selected.
    /// </summary>
    /// <param name="fadeInDuration">How long it takes in seconds for these changes to apply.</param>
    /// <param name="settings">Your custom settings that override the default uSkyManager values.</param>
    /// <param name="requiredGUIDs">The required mod GUIDs for this addon to enable. Each required mod must approve
    /// this addon by using <see cref="TitleScreenHandler.ApproveTitleCollaboration"/>.</param>
    public SkyChangeTitleAddon(float fadeInDuration, Settings settings, params string[] requiredGUIDs) :
        base(requiredGUIDs)
    {
        _fadeInDuration = fadeInDuration;
        _settings = settings;
    }

    /// <summary>
    /// Activates the sky change.
    /// </summary>
    protected override void OnEnable()
    {
        DeregisterPreviousSkyChange();
        _previousSettings = uSkyManager.main != null ? GetSettingsFromSkyManager(uSkyManager.main) : null;
        _transitionActive = true;
        _timeTransitionStarted = Time.time;
        _revertingToDefaultSky = false;
        BehaviourUpdateUtils.Register(this);
        _activeSkyChange = this;
    }

    /// <summary>
    /// De-activates the sky change.
    /// </summary>
    protected override void OnDisable()
    {
        // Don't de-register JUST yet, let's first revert to the default sky until someone else takes over
        var skyManager = uSkyManager.main;
        if (skyManager != null && !_revertingToDefaultSky)
        {
            StartRevertingToDefaultSky(skyManager);
        }
        else
        {
            BehaviourUpdateUtils.Deregister(this);
        }
    }

    protected override void OnEnterLoadScreen()
    {
        _transitionActive = false;
        var skyManager = uSkyManager.main;
        if (skyManager == null)
            return;

        skyManager.Timeline = _defaultSettings.Value.TimeOfDay;
        skyManager.Exposure = _defaultSettings.Value.Exposure;
        skyManager.RayleighScattering = _defaultSettings.Value.RayleighScattering;
        skyManager.skyFogDensity = _defaultSettings.Value.FogDensity;
    }

    /// <summary>
    /// Called every frame while registered.
    /// </summary>
    public virtual void ManagedUpdate()
    {
        if (!_transitionActive)
            return;

        var skyManager = uSkyManager.main;
        if (skyManager == null)
            return;

        if (_defaultSettings == null)
        {
            CacheDefaultSettings(skyManager);
        }
        
        _previousSettings ??= _defaultSettings ?? new Settings();

        var target = _revertingToDefaultSky ? _defaultSettings.Value : _settings;

        var duration = GetTransitionDuration();
        
        skyManager.Timeline = LerpValue(_previousSettings.Value.TimeOfDay, target.TimeOfDay, duration);
        skyManager.Exposure = LerpValue(_previousSettings.Value.Exposure, target.Exposure, duration);
        skyManager.RayleighScattering =
            LerpValue(_previousSettings.Value.RayleighScattering, target.RayleighScattering, duration);
        skyManager.skyFogDensity = LerpValue(_previousSettings.Value.FogDensity, target.FogDensity, duration);

        if (Time.time > _timeTransitionStarted + duration)
        {
            _transitionActive = false;
            if (_revertingToDefaultSky)
            {
                _revertingToDefaultSky = false;
                if (_activeSkyChange == this)
                    _activeSkyChange = null;
            }
            BehaviourUpdateUtils.Deregister(this);
        }
    }

    private float LerpValue(float previous, float target, float duration)
    {
        return Mathf.Lerp(previous, target, (Time.time - _timeTransitionStarted) / duration);
    }

    private float GetTransitionDuration() => _revertingToDefaultSky ? RevertToDefaultSkyDuration : _fadeInDuration;

    private void StartRevertingToDefaultSky(uSkyManager skyManager)
    {
        if (!_transitionActive)
            BehaviourUpdateUtils.Register(this);
        _revertingToDefaultSky = true;
        _transitionActive = true;
        _timeTransitionStarted = Time.time;
        _previousSettings = GetSettingsFromSkyManager(skyManager);
    }

    string IManagedBehaviour.GetProfileTag()
    {
        return "SkyChangeTitleAddon";
    }

    private static void CacheDefaultSettings(uSkyManager skyManager)
    {
        _defaultSettings = GetSettingsFromSkyManager(skyManager);
    }

    private static Settings GetSettingsFromSkyManager(uSkyManager skyManager)
    {
        return new Settings(skyManager.Timeline, skyManager.Exposure, skyManager.RayleighScattering,
            skyManager.skyFogDensity);
    }

    private static void DeregisterPreviousSkyChange()
    {
        if (_activeSkyChange == null)
            return;

        BehaviourUpdateUtils.Deregister(_activeSkyChange);
        _activeSkyChange._transitionActive = false;
        _activeSkyChange._revertingToDefaultSky = false;
        _activeSkyChange = null;
    }

    /// <summary>
    /// Settings pertaining to the <see cref="uSkyManager"/> class.
    /// </summary>
    public struct Settings
    {
        /// <summary>
        /// The time of day, roughly corresponding to Earth hours.
        /// </summary>
        /// <remarks>
        /// The default value is 6.8 in SN1 and 15.1 in BZ.
        /// <list type="bullet">
        /// <item><description><b>Night (AM)</b>: 0.0–5.2</description></item>
        /// <item><description><b>Dawn</b>: 5.2–6.0</description></item>
        /// <item><description><b>Morning</b>: 6.0–12.0</description></item>
        /// <item><description><b>Noon</b>: 12.0</description></item>
        /// <item><description><b>Afternoon</b>: 12.0–18.0</description></item>
        /// <item><description><b>Dusk</b>: 18.0–18.8</description></item>
        /// <item><description><b>Night (PM)</b>: 18.0–24.0</description></item>
        /// </list>
        /// </remarks>
        public float TimeOfDay { get; init; } = 6.8f;

        /// <summary>
        /// The sky's exposure value, directly correlated with the brightness of the sky.
        /// </summary>
        /// <remarks>
        /// Only positive values are supported. The default value is 0.66 in SN1 and 0.9 in BZ.
        /// </remarks>
        public float Exposure { get; init; } = 0.66f;

        /// <summary>
        /// The strength of rayleigh scattering, affecting how strongly light scatters from particles in the atmosphere.
        /// </summary>
        /// <remarks>
        /// Only positive values are supported. The default value is 1.0 in SN1 and 0.9 in BZ. By the way, this is what makes the sky blue.
        /// </remarks>
        public float RayleighScattering { get; init; } = 1f;

        /// <summary>
        /// The strength/density of the fog.
        /// </summary>
        /// <remarks>
        /// Only positive values are supported. The default value is 0.0002 in SN1 and 0.001 in BZ.
        /// </remarks>
        public float FogDensity { get; init; } = 0.0002f;

        /// <summary>
        /// The main constructor.
        /// </summary>
        /// <param name="timeOfDay">The time of day, similar to 24-hour Earth time.</param>
        /// <param name="exposure">The exposure or brightness of the sky.</param>
        /// <param name="rayleighScattering">The strength of light scattering in the sky.</param>
        /// <param name="fogDensity">The strength/density of fog. Can.</param>
        public Settings(
#if SUBNAUTICA
            float timeOfDay = 6.8f, float exposure = 0.66f, float rayleighScattering = 1f, float fogDensity = 0.0002f)
#elif BELOWZERO
            float timeOfDay = 15.1f, float exposure = 0.9f, float rayleighScattering = 0.9f, float fogDensity = 0.0001f)
#endif
        {
            TimeOfDay = timeOfDay;
            Exposure = exposure;
            RayleighScattering = rayleighScattering;
            FogDensity = fogDensity;
        }
    }
}