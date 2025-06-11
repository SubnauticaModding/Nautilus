using System;
using System.Collections.Generic;
using Nautilus.Patchers;
using Nautilus.Utility;
using Story;
using UnityEngine;
using UnityEngine.UI;

namespace Nautilus.Handlers.LoadingScreen;

internal class LoadingScreenSetter : MonoBehaviour
{
    internal static readonly Dictionary<string, LoadingScreenHandler.LoadingScreenData[]> LoadingScreenDatas = new();

    private uGUI_SceneLoading _sceneLoading;
    private Sprite _defaultLoadingSprite;
    private Image _loadingScreenImage;
    private Image _loadingScreenImage2;
    
    private Image _targetFadeImage;
    private Image BufferImage => _targetFadeImage == _loadingScreenImage ? _loadingScreenImage2 : _loadingScreenImage;

    private bool _wasLoading;
    private bool _isFading;
    private int _currentScreenIndex;
    private float _timeNextScreen;
    private float _fadeProgress;

    private LoadingScreenHandler.LoadingScreenData[] _possibleLoadingScreens;
    
    private void Start()
    {
        _sceneLoading = GetComponent<uGUI_SceneLoading>();
        _loadingScreenImage = transform.Find("LoadingScreen/LoadingArtwork").GetComponent<Image>();
        _loadingScreenImage2 = Instantiate(_loadingScreenImage.gameObject, _loadingScreenImage.transform.parent)
            .GetComponent<Image>();
        _loadingScreenImage2.transform.SetSiblingIndex(_loadingScreenImage.transform.GetSiblingIndex() + 1);

        _defaultLoadingSprite = _loadingScreenImage.sprite;
        _targetFadeImage = _loadingScreenImage;

        MainMenuPatcher.onActiveModChanged += UpdatePotentialBackgrounds;
        SetCurrentImage(null, true);
    }

    private void Update()
    {
        if (_sceneLoading.isLoading && !_wasLoading)
        {
            _currentScreenIndex = 0;
            IncrementTimer();
            SetCurrentImage(GetNextImage(), true);
        }
        
        _wasLoading = _sceneLoading.isLoading;
        
        if (!_sceneLoading.isLoading) return;

        if (!StoryGoalManager.main) return;

        if (_possibleLoadingScreens == null) return;

        if (Time.realtimeSinceStartup > _timeNextScreen)
        {
            SetCurrentImage(GetNextImage());
            IncrementTimer();
        }

        HandleTransition();
    }

    private void SetCurrentImage(Sprite sprite, bool instant = false)
    {
        if (_targetFadeImage && sprite == BufferImage.sprite) return;
        
        sprite ??= _defaultLoadingSprite;
        _targetFadeImage = BufferImage;
        _targetFadeImage.sprite = sprite;

        if (instant)
        {
            _targetFadeImage.color = Color.white;
            BufferImage.color = Color.clear;
            return;
        }

        _isFading = true;
        _fadeProgress = 0;
    }

    private Sprite GetNextImage()
    {
        if (_possibleLoadingScreens == null) return null;

        if (_possibleLoadingScreens.Length == 0) return null;

        if (_possibleLoadingScreens.Length == 1 && ScreenIsValid(_possibleLoadingScreens[0]))
            return _possibleLoadingScreens[0].loadingScreenImage;
        
        _currentScreenIndex++;
        _currentScreenIndex %= _possibleLoadingScreens.Length;
        
        Sprite nextImage = null;
        for (int i = _currentScreenIndex; i < _possibleLoadingScreens.Length; i++)
        {
            var screen = _possibleLoadingScreens[i];

            if (ScreenIsValid(screen))
            {
                nextImage = screen.loadingScreenImage;
                break;
            }
        }
        
        return nextImage;
    }

    private bool ScreenIsValid(LoadingScreenHandler.LoadingScreenData screen)
    {
        if (screen.storyGoalRequirement == null) return true;

        if (StoryGoalManager.main.IsGoalComplete(screen.storyGoalRequirement)) return true;

        return false;
    }

    private void HandleTransition()
    {
        if (!_isFading) return;
        
        _fadeProgress = Mathf.Clamp01(_fadeProgress + Time.unscaledDeltaTime * 0.5f);
        _targetFadeImage.color = new Color(1, 1, 1, _fadeProgress);
        BufferImage.color = new Color(1, 1, 1, 1 - _fadeProgress);
        if (_fadeProgress >= 1)
        {
            _isFading = false;
        }
    }

    private void IncrementTimer()
    {
        float timeIncrement = _possibleLoadingScreens == null
            ? 2
            : _possibleLoadingScreens[_currentScreenIndex].minTimeToNextScreen;
        _timeNextScreen = Time.realtimeSinceStartup + timeIncrement;
        
        InternalLogger.Log($"Incrementing timer. Index = {_currentScreenIndex} | Time next screen = {_timeNextScreen} | Time = {Time.realtimeSinceStartup}");
    }

    private void UpdatePotentialBackgrounds()
    {
        var currentModGUID = MainMenuPatcher.GetActiveModGUID();
        if (currentModGUID == "Subnautica")
        {
            _possibleLoadingScreens = null;
            return;
        }

        LoadingScreenDatas.TryGetValue(currentModGUID, out _possibleLoadingScreens);
    }
}