using System;
using System.Collections.Generic;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using HarmonyLib;
using Nautilus.Extensions;
using Nautilus.FMod.Interfaces;
using Nautilus.Handlers;
using Nautilus.Utility;
using UnityEngine;
using UnityEngine.Playables;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace Nautilus.Patchers;

internal class CustomSoundPatcher
{
    internal record struct AttachedChannel(Channel Channel, Transform Transform);
    internal record struct FadeInfo(Sound Sound, float Seconds);
    
    internal static readonly SelfCheckingDictionary<string, Sound> CustomSounds = new("CustomSounds");
    internal static readonly SelfCheckingDictionary<string, Bus> CustomSoundBuses = new("CustomSoundBuses");
    internal static readonly SelfCheckingDictionary<string, IFModSound> CustomFModSounds = new("CustoomFModSounds");
    internal static readonly Dictionary<int, Channel> EmitterPlayedChannels = new();
    internal static readonly Dictionary<IntPtr, FadeInfo> FadeOuts = new();
    internal static List<AttachedChannel> AttachedChannels = new();

    private static readonly Dictionary<string, Channel> PlayedChannels = new();
    private static readonly Dictionary<FMODEventPlayableBehavior, Channel> PlayableBehaviorChannels = new();
    private static readonly List<AttachedChannel> _attachedChannelsToRemove = new();

    internal static void Patch(Harmony harmony)
    {
        harmony.PatchAll(typeof(CustomSoundPatcher));
        InternalLogger.Debug("CustomSoundPatcher is done.");
    }
        
    [HarmonyPatch(typeof(PDASounds), nameof(PDASounds.Deinitialize))]
    [HarmonyPrefix]
    public static void PDASounds_Deinitialize_Postfix()
    {
        EmitterPlayedChannels.Clear();
        PlayedChannels.Clear();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(FMODExtensions), nameof(FMODExtensions.GetLength))]
    public static bool FMODExtension_GetLength_Prefix(string path, ref int __result)
    {
        if(string.IsNullOrEmpty(path))
        {
            __result = 0;
            return false;
        }

        InternalLogger.Debug($"FMODExtensions.GetLength(\"{path}\") executed. Checking if it's a custom sound...");

        if (!CustomSounds.ContainsKey(path))
            return true;

        InternalLogger.Debug($"FMODExtensions.GetLength(\"{path}\") executed. It was a custom sound.");

        if(CustomSounds.TryGetValue(path, out Sound sound))
        {
            RESULT res = sound.getLength(out uint length, TIMEUNIT.MS);
            if(res == RESULT.OK)
            {
                __result = (int)length;
                return false;
            }
            else
            {
                InternalLogger.Log($"An error occured while trying to get length of a sound.\n{res}");
            }
            res.CheckResult();
        }

        InternalLogger.Debug($"FMODExtensions.GetLength(\"{path}\") executed. It was maybe not a CustomSounds but a CustomFModSounds ?");
        __result = 0;
        return false;
    }
    
    [HarmonyPatch(typeof(FMODEventPlayableBehavior), nameof(FMODEventPlayableBehavior.PerformSeek))]
    [HarmonyPrefix]
    public static bool FMODEventPlayableBehavior_PerformSeek_Prefix(FMODEventPlayableBehavior __instance)
    {
        if (!PlayableBehaviorChannels.TryGetValue(__instance, out var channel))
        {
            return true;
        }
        
        if (__instance.seek < 0)
        {
            return true;
        }
        
        channel.setPosition((uint)__instance.seek, TIMEUNIT.MS);
        __instance.seek = -1;
        return false;
    }

    [HarmonyPatch(typeof(FMODEventPlayableBehavior), nameof(FMODEventPlayableBehavior.PlayEvent))]
    [HarmonyPrefix]
    public static bool FMODEventPlayableBehavior_PlayEvent_Prefix(FMODEventPlayableBehavior __instance)
    {
        if (string.IsNullOrEmpty(__instance.eventName))
        {
            return true;
        }
        
        if (string.IsNullOrEmpty(__instance.eventName) || !CustomSounds.TryGetValue(__instance.eventName, out Sound soundEvent) 
            && !CustomFModSounds.ContainsKey(__instance.eventName)) return true;
        
        Channel channel;
        if (CustomFModSounds.TryGetValue(__instance.eventName, out var fModSound))
        {
            if (!fModSound.TryPlaySound(out channel))
                return false;
        }
        else if (CustomSoundBuses.TryGetValue(__instance.eventName, out Bus bus))
        {
            if (!AudioUtils.TryPlaySound(soundEvent, bus, out channel))
                return false;
        }
        else
        {
            return false;
        }

        PlayableBehaviorChannels[__instance] = channel;

        channel.setPaused(true);
        
        __instance.PerformSeek();

        if (__instance.TrackTargetObject)
        {
            CustomSoundHandler.AttachChannelToGameObject(channel, __instance.TrackTargetObject.transform);
        }
        else
        {
            SetChannel3DAttributes(channel, Vector3.zero);
        }
        
        channel.setVolume(__instance.currentVolume);
        
        channel.setPaused(false);

        return false;
    }

    [HarmonyPatch(typeof(FMODEventPlayableBehavior), nameof(FMODEventPlayableBehavior.OnExit))]
    [HarmonyPrefix]
    public static bool FMODEventPlayableBehavior_OnExit_Prefix(FMODEventPlayableBehavior __instance)
    {
        if (!PlayableBehaviorChannels.TryGetValue(__instance, out var channel))
        {
            return true;
        }

        if (!__instance.isPlayheadInside)
        {
            return false;
        }

        if (__instance.stopType == FMODUnity.STOP_MODE.Immediate)
        {
            channel.stop();
        }
        else if (__instance.stopType == FMODUnity.STOP_MODE.AllowFadeout)
        {
            TryFadeOutBeforeStop(channel);
        }

        PlayableBehaviorChannels.Remove(__instance);
        __instance.isPlayheadInside = false;
        
        return false;
    }

    [HarmonyPatch(typeof(FMODEventPlayableBehavior), nameof(FMODEventPlayableBehavior.ProcessFrame))]
    [HarmonyPrefix]
    public static bool FMODEventPlayableBehavior_ProcessFrame_Prefix(FMODEventPlayableBehavior __instance)
    {
        return !PlayableBehaviorChannels.ContainsKey(__instance);
    }

    [HarmonyPatch(typeof(FMODEventPlayableBehavior), nameof(FMODEventPlayableBehavior.UpdateBehavior))]
    [HarmonyPrefix]
    public static bool FMODEventPlayableBehavior_UpdateBehavior_Prefix(FMODEventPlayableBehavior __instance, float time, float volume)
    {
        if (!PlayableBehaviorChannels.TryGetValue(__instance, out var channel))
        {
            return true;
        }

        if (volume != __instance.currentVolume)
        {
            __instance.currentVolume = volume;
            channel.setVolume(volume);
        }
        
        if (time >= __instance.OwningClip.start && time < __instance.OwningClip.end)
        {
            __instance.OnEnter();
        }
        else
        {
            __instance.OnExit();
        }

        return false;
    }

    [HarmonyPatch(typeof(FMODEventPlayableBehavior), nameof(FMODEventPlayableBehavior.OnGraphStop))]
    [HarmonyPostfix]
    public static void FMODEventPlayableBehavior_OnGraphStop_Postfix(FMODEventPlayableBehavior __instance)
    {
        if (!PlayableBehaviorChannels.TryGetValue(__instance, out var channel))
        {
            channel.stop();
            PlayableBehaviorChannels.Remove(__instance);
        }
    }
    
    [HarmonyPatch(typeof(FMODEventPlayableBehavior), nameof(FMODEventPlayableBehavior.Evaluate))]
    [HarmonyPrefix]
    public static bool FMODEventPlayableBehavior_Evaluate_Postfix(FMODEventPlayableBehavior __instance, double time, FrameData info, bool evaluate)
    {
        if (!PlayableBehaviorChannels.TryGetValue(__instance, out var channel))
        {
            return true;
        }
        
        if (!info.timeHeld && time >= __instance.OwningClip.start && time < __instance.OwningClip.end)
        {
            if (!__instance.isPlayheadInside)
            {
                if (time - __instance.OwningClip.start > 0.1)
                {
                    __instance.seek = __instance.GetPosition(time);
                }
                __instance.OnEnter();
                return false;
            }
            if ((evaluate || info.seekOccurred || info.timeLooped || info.evaluationType == FrameData.EvaluationType.Evaluate))
            {
                __instance.seek = __instance.GetPosition(time);
                __instance.PerformSeek();
                return false;
            }
        }
        else
        {
            __instance.OnExit();
        }

        return false;
    }

    [HarmonyPatch(typeof(RuntimeManager), nameof(RuntimeManager.Update))]
    [HarmonyPostfix]
    public static void RuntimeManager_Update_Postfix(RuntimeManager __instance)
    {
        if (!__instance.studioSystem.isValid())
        {
            return;
        }

        foreach (var attachedChannel in AttachedChannels)
        {
            attachedChannel.Channel.isPlaying(out var isPlaying);
            if (!isPlaying || !attachedChannel.Transform)
            {
                _attachedChannelsToRemove.Add(attachedChannel);
                continue;
            }
            
            SetChannel3DAttributes(attachedChannel.Channel, attachedChannel.Transform);
        }

        if (_attachedChannelsToRemove.Count > 0)
        {
            foreach (var toRemove in _attachedChannelsToRemove)
            {
                AttachedChannels.Remove(toRemove);
            }
            _attachedChannelsToRemove.Clear();
        }
    }
    
#if SUBNAUTICA
        
    [HarmonyPatch(typeof(FMODUWE), nameof(FMODUWE.PlayOneShotImpl))]
    [HarmonyPrefix]
    public static bool FMODUWE_PlayOneShotImpl_Prefix(string eventPath, Vector3 position, float volume)
    {
        if (string.IsNullOrEmpty(eventPath) || !CustomSounds.TryGetValue(eventPath, out Sound soundEvent) 
            && !CustomFModSounds.ContainsKey(eventPath)) return true;

        Channel channel;
        if (CustomFModSounds.TryGetValue(eventPath, out var fModSound))
        {
            if (!fModSound.TryPlaySound(out channel))
                return false;
        }
        else if (CustomSoundBuses.TryGetValue(eventPath, out Bus bus))
        {
            if (!AudioUtils.TryPlaySound(soundEvent, bus, out channel))
                return false;
        }
        else
        {
            return false;
        }
            
        SetChannel3DAttributes(channel, position);
        channel.setVolume(volume);
            
        return false;
    }
        
    [HarmonyPatch(typeof(SoundQueue), nameof(SoundQueue.Play))]
    [HarmonyPrefix]
    public static bool SoundQueue_Play_Prefix(SoundQueue __instance, string sound, string subtitles)
    {
        if (string.IsNullOrEmpty(sound) || !CustomSounds.TryGetValue(sound, out Sound soundEvent) && !CustomFModSounds.ContainsKey(sound)) return true;

        __instance.Stop();
        __instance._current = sound;
        soundEvent.getLength(out var length, TIMEUNIT.MS);
        __instance._length = (int)length;
        __instance._lengthSeconds = length * 0.001f;
        Channel channel;
        if (CustomFModSounds.TryGetValue(sound, out var fModSound))
        {
            if (!fModSound.TryPlaySound(out channel))
                return false;
        }
        else if (CustomSoundBuses.TryGetValue(sound, out Bus bus))
        {
            if (!AudioUtils.TryPlaySound(soundEvent, bus, out channel))
                return false;
        }
        else
        {
            return false;
        }
        PlayedChannels[sound] = channel;

        if (!string.IsNullOrEmpty(subtitles))
        {
            Subtitles.Add(subtitles);
        }
        return false;
    }
        
    [HarmonyPatch(typeof(SoundQueue), nameof(SoundQueue.Stop))]
    [HarmonyPrefix]
    public static bool SoundQueue_Stop_Prefix(SoundQueue __instance)
    {
        if (string.IsNullOrEmpty(__instance._current) || !PlayedChannels.TryGetValue(__instance._current, out var channel)) return true;

        channel.stop();
        __instance._current = null;
            
        return false;
    }
        
    [HarmonyPatch(typeof(SoundQueue), nameof(SoundQueue.Update))]
    [HarmonyPrefix]
    public static bool SoundQueue_Update_Prefix(SoundQueue __instance)
    {
        if (__instance is null || string.IsNullOrEmpty(__instance._current)  || !PlayedChannels.TryGetValue(__instance._current, out var channel)) return true;
        if (!SoundQueue.GetIsStartingOrPlaying(__instance.eventInstance)) return true;

        ATTRIBUTES_3D attributes = Player.main.transform.To3DAttributes();
        channel.set3DAttributes(ref attributes.position, ref attributes.velocity);

        channel.getPosition(out var position, TIMEUNIT.MS);
        __instance._position = (int)position;
        __instance._positionSeconds = position * 0.001f;
            
        return false;
    }

    [HarmonyPatch(typeof(SoundQueue), nameof(SoundQueue.GetIsStartingOrPlaying))]
    [HarmonyPrefix]
    public static bool SoundQueue_GetIsStartingOrPlaying_Prefix(ref bool __result)
    {
        if (PDASounds.queue is null || string.IsNullOrEmpty(PDASounds.queue._current)) return true;
        if (!PlayedChannels.TryGetValue(PDASounds.queue?._current, out var channel)) return true;
        var result = channel.isPlaying(out __result);
        __result = __result && result == RESULT.OK;
        return false;
    }
        
    [HarmonyPatch(typeof(SoundQueue), nameof(SoundQueue.position), MethodType.Setter)]
    [HarmonyPrefix]
    public static bool SoundQueue_Position_Setter_Prefix(SoundQueue __instance, int value)
    {
        if (!PlayedChannels.TryGetValue(__instance._current, out var channel)) return true;

        channel.setPosition((uint)Mathf.Clamp(value, 0, __instance._length), TIMEUNIT.MS);
            
        return false;
    }

    [HarmonyPatch(typeof(FMOD_CustomEmitter), nameof(FMOD_CustomEmitter.Play))]
    [HarmonyPrefix]
    public static bool FMOD_CustomEmitter_Play_Prefix(FMOD_CustomEmitter __instance)
    {
        if (string.IsNullOrEmpty(__instance.asset?.path) || !CustomSounds.TryGetValue(__instance.asset.path, out var sound) 
            && !CustomFModSounds.ContainsKey(__instance.asset.path)) return true;

        if (EmitterPlayedChannels.TryGetValue(__instance.GetInstanceID(), out var channel) &&
            channel.isPlaying(out var playing) == RESULT.OK && playing && !__instance.restartOnPlay) // already playing, no need to play it again
        {
            return false;
        }

        var soundPath = __instance.asset.path;
        if (CustomFModSounds.TryGetValue(soundPath, out var fModSound))
        {
            if (!fModSound.TryPlaySound(out channel))
                return false;
                
            EmitterPlayedChannels[__instance.GetInstanceID()] = channel;
        }
        else if (CustomSoundBuses.TryGetValue(soundPath, out Bus bus))
        {
            if (!AudioUtils.TryPlaySound(sound, bus, out channel))
                return false;
                
            EmitterPlayedChannels[__instance.GetInstanceID()] = channel;
        }
        else
        {
            return false;
        }

        SetChannel3DAttributes(EmitterPlayedChannels[__instance.GetInstanceID()], __instance.transform);
        __instance._playing = true;
        __instance.OnPlay();

        return false;
    }
        
    [HarmonyPatch(typeof(FMOD_CustomEmitter), nameof(FMOD_CustomEmitter.Stop))]
    [HarmonyPrefix]
    public static bool FMOD_CustomEmitter_Stop_Prefix(FMOD_CustomEmitter __instance, STOP_MODE stopMode)
    {
        if (!EmitterPlayedChannels.TryGetValue(__instance.GetInstanceID(), out var channel)) return true;

        if (stopMode == STOP_MODE.IMMEDIATE)
        {
            channel.stop();
        }
        else
        {
            TryFadeOutBeforeStop(channel);
        }
        __instance._playing = false;
        __instance.OnStop();

        return false;
    }

    [HarmonyPatch(typeof(FMOD_CustomEmitter), nameof(FMOD_CustomEmitter.ManagedUpdate))]
    [HarmonyPrefix]
    public static bool FMOD_CustomEmitter_ManagedUpdate_Prefix(FMOD_CustomEmitter __instance)
    {
        if (!EmitterPlayedChannels.TryGetValue(__instance.GetInstanceID(), out var channel)) return true;

        if (__instance.followParent && channel.isPlaying(out var playing) == RESULT.OK && playing)
        {
            __instance.attributes = __instance.transform.To3DAttributes();
            SetChannel3DAttributes(channel, __instance.transform);
        }

        return false;
    }
        
    [HarmonyPatch(typeof(FMOD_CustomEmitter), nameof(FMOD_CustomEmitter.SetAsset))]
    [HarmonyPrefix]
    public static bool FMOD_CustomEmitter_SetAsset_Prefix(FMOD_CustomEmitter __instance, FMODAsset newAsset)
    {
        if (newAsset == null) return false;
        if (!CustomSounds.ContainsKey(newAsset.path) && !CustomFModSounds.ContainsKey(newAsset.path)) return true;

        __instance.ReleaseEvent();
        __instance.asset = newAsset;

        return false;
    }
        
    [HarmonyPatch(typeof(FMOD_CustomEmitter), nameof(FMOD_CustomEmitter.ReleaseEvent))]
    [HarmonyPrefix]
    public static bool FMOD_CustomEmitter_ReleaseEvent_Prefix(FMOD_CustomEmitter __instance)
    {
        if (__instance.asset == null || !CustomSounds.ContainsKey(__instance.asset.path) && !CustomFModSounds.ContainsKey(__instance.asset.path)) return true;
        if (!EmitterPlayedChannels.TryGetValue(__instance.GetInstanceID(), out var channel)) return false; // known sound but not played yet

        TryFadeOutBeforeStop(channel);
        
        EmitterPlayedChannels.Remove(__instance.GetInstanceID());

        return false;
    }
        
    [HarmonyPatch(typeof(FMOD_CustomLoopingEmitter), nameof(FMOD_CustomLoopingEmitter.PlayStopSound))]
    [HarmonyPrefix]
    public static bool FMOD_CustomLoopingEmitter_PlayStopSound_Prefix(FMOD_CustomLoopingEmitter __instance)
    {
        if (__instance.assetStop == null) return true;
        if (string.IsNullOrEmpty(__instance.assetStop.path) || !CustomSounds.TryGetValue(__instance.assetStop.path, out var sound) 
            && !CustomFModSounds.ContainsKey(__instance.assetStop.path)) return true;
            
        var soundPath = __instance.assetStop.path;
        Channel channel;
        if (CustomFModSounds.TryGetValue(soundPath, out var fModSound))
        {
            if (!fModSound.TryPlaySound(out channel)) 
                return false;
        }
        else if (CustomSoundBuses.TryGetValue(soundPath, out Bus bus))
        {
            if (!AudioUtils.TryPlaySound(sound, bus, out channel)) 
                return false;
        }
        else
        {
            return false;
        }
            
        SetChannel3DAttributes(channel, __instance.transform);
        __instance.timeLastStopSound = Time.time;

        return false;
    }
        
    [HarmonyPatch(typeof(FMOD_CustomLoopingEmitter), nameof(FMOD_CustomLoopingEmitter.OnPlay))]
    [HarmonyPrefix]
    public static bool FMOD_CustomLoopingEmitter_OnPlay_Prefix(FMOD_CustomLoopingEmitter __instance)
    {
        if (__instance.assetStart == null) return true;
        if (string.IsNullOrEmpty(__instance.assetStart.path) || !CustomSounds.TryGetValue(__instance.assetStart.path, out var sound) 
            && !CustomFModSounds.ContainsKey(__instance.assetStart.path)) return true;
            
        var soundPath = __instance.assetStart.path;
        Channel channel;
        if (CustomFModSounds.TryGetValue(soundPath, out var fModSound))
        {
            if (!fModSound.TryPlaySound(out channel))
                return false;                
        }
        else if (CustomSoundBuses.TryGetValue(soundPath, out Bus bus))
        {
            if (!AudioUtils.TryPlaySound(sound, bus, out channel))
                return false;
        }
        else
        {
            return false;
        }
            
        SetChannel3DAttributes(channel, __instance.transform);
        __instance.timeLastStopSound = Time.time;
        BehaviourUpdateUtils.Register(__instance);

        return false;
    }
#elif BELOWZERO
        [HarmonyPatch(typeof(FMODUWE), nameof(FMODUWE.PlayOneShotImpl))]
        [HarmonyPrefix]
        public static bool FMODUWE_PlayOneShotImpl_Prefix(string eventPath, Vector3 position, float volume)
        {
            if (string.IsNullOrEmpty(eventPath) || (!CustomSounds.TryGetValue(eventPath, out Sound soundEvent) 
                                                && !CustomFModSounds.ContainsKey(eventPath)))
            {
                return true;
            }

            Channel channel;
            if (CustomFModSounds.TryGetValue(eventPath, out IFModSound fModSound))
            {
                if(!fModSound.TryPlaySound(out channel))
                    return false;
            }
            else if (CustomSoundBuses.TryGetValue(eventPath, out Bus bus))
            {
                if(!AudioUtils.TryPlaySound(soundEvent, bus, out channel))
                    return false;
            }
            else
            {
                return false;
            }
            
            SetChannel3DAttributes(channel, position);
            channel.setVolume(volume);
            
            return false;
        }

        [HarmonyPatch(typeof(SoundQueue), nameof(SoundQueue.PlayImpl))]
        [HarmonyPrefix]
        public static bool SoundQueue_Play_Prefix(SoundQueue __instance, string sound, SoundHost host, string subtitles, int subtitlesLine)
        {
            if (string.IsNullOrEmpty(sound) || (!CustomSounds.TryGetValue(sound, out Sound soundEvent) && !CustomFModSounds.ContainsKey(sound)))
            {
                return true;
            }

            __instance.StopImpl();
            __instance._current =  new SoundQueue.Entry?(new SoundQueue.Entry
            {
                sound = sound,
                subtitles = subtitles,
                subtitleLine = subtitlesLine,
                host = host
            });
            __instance._length = sound.Length;
            __instance._lengthSeconds = __instance._length * 0.001f;
            Channel channel;
            if (CustomFModSounds.TryGetValue(sound, out IFModSound fModSound))
            {
                if (!fModSound.TryPlaySound(out channel))
                    return false;
            }
            else if (CustomSoundBuses.TryGetValue(sound, out Bus bus))
            {
                if (!AudioUtils.TryPlaySound(soundEvent, bus, out channel))
                    return false;
            }
            else
            {
                return false;
            }
            
            PlayedChannels[sound] = channel;

            if (!string.IsNullOrEmpty(subtitles))
            {
                Subtitles.Add(subtitles);
            }
            return false;
        }
        
        [HarmonyPatch(typeof(SoundQueue), nameof(SoundQueue.StopImpl))]
        [HarmonyPrefix]
        public static bool SoundQueue_Stop_Prefix(SoundQueue __instance)
        {
            if (__instance._current is null || string.IsNullOrEmpty(__instance._current.Value.sound) 
                                            || !PlayedChannels.TryGetValue(__instance._current.Value.sound, out Channel channel))
            {
                return true;
            }

            channel.stop();
            PlayedChannels.Remove(__instance._current.Value.sound);
            __instance._current = null;
            
            return false;
        }
        
        [HarmonyPatch(typeof(SoundQueue), nameof(SoundQueue.Update))]
        [HarmonyPrefix]
        public static bool SoundQueue_Update_Prefix(SoundQueue __instance)
        {
            SoundQueue.Entry instanceCurrent = __instance._current ?? default;
            if (string.IsNullOrEmpty(instanceCurrent.sound)  || !PlayedChannels.TryGetValue(instanceCurrent.sound, out Channel channel))
            {
                return true;
            }
#if BELOWZERO
            if (SoundQueue.GetPlaybackState(__instance.eventInstance) is not (PLAYBACK_STATE.STARTING or PLAYBACK_STATE.PLAYING))
            {
                return true;
            }
#else
            if (!SoundQueue.GetIsStartingOrPlaying(__instance.eventInstance)) return true;
#endif

        ATTRIBUTES_3D attributes = Player.main.transform.To3DAttributes();
            channel.set3DAttributes(ref attributes.position, ref attributes.velocity);
            channel.getPosition(out uint position, TIMEUNIT.MS);
            instanceCurrent.position = (int)position;
            __instance._positionSeconds = (float)instanceCurrent.position * 0.001f;
            __instance._current = instanceCurrent;
            return false;
        }

        [HarmonyPrefix]
#if BELOWZERO
        [HarmonyPatch(typeof(SoundQueue), nameof(SoundQueue.GetPlaybackState))]
        public static bool SoundQueue_GetIsStartingOrPlaying_Prefix(ref PLAYBACK_STATE __result)
#else
        [HarmonyPatch(typeof(SoundQueue), nameof(SoundQueue.GetIsStartingOrPlaying))]
        public static bool SoundQueue_GetIsStartingOrPlaying_Prefix(ref bool __result)
#endif
        {
            SoundQueue.Entry instanceCurrent = PDASounds.queue?._current ?? default;
            if (string.IsNullOrEmpty(instanceCurrent.sound)  || !PlayedChannels.TryGetValue(instanceCurrent.sound, out Channel channel))
            {
                return true;
            }

#if BELOWZERO
            channel.isPlaying(out bool isPlaying);
            __result = isPlaying ? PLAYBACK_STATE.PLAYING : PLAYBACK_STATE.STOPPED;
#else
            var result = channel.isPlaying(out __result);
            __result = __result && result == RESULT.OK;
#endif
            return false;
        }
        
        [HarmonyPatch(typeof(SoundQueue), nameof(SoundQueue.SetPosition))]
        [HarmonyPrefix]
        public static bool SoundQueue_Position_Setter_Prefix(SoundQueue __instance, int value)
        {
            SoundQueue.Entry instanceCurrent = __instance._current ?? default;
            if (!PlayedChannels.TryGetValue(instanceCurrent.sound, out Channel channel))
            {
                return true;
            }

            channel.setPosition((uint)Mathf.Clamp(value, 0, __instance._length), TIMEUNIT.MS);
            
            return false;
        }
        
        [HarmonyPatch(typeof(FMOD_CustomEmitter), nameof(FMOD_CustomEmitter.Play))]
        [HarmonyPrefix]
        public static bool FMOD_CustomEmitter_Play_Prefix(FMOD_CustomEmitter __instance)
        {
            if (string.IsNullOrEmpty(__instance.asset?.path) || (!CustomSounds.TryGetValue(__instance.asset.path, out Sound sound) 
                && !CustomFModSounds.ContainsKey(__instance.asset.path)))
            {
                return true;
            }

            if (EmitterPlayedChannels.TryGetValue(__instance.GetInstanceID(), out Channel channel) &&
                channel.isPlaying(out bool playing) == RESULT.OK && playing && !__instance.restartOnPlay) // already playing, no need to play it again
            {
                return false;
            }

            string soundPath = __instance.asset.path;

            if (CustomFModSounds.TryGetValue(soundPath, out IFModSound fModSound))
            {
                if (!fModSound.TryPlaySound(out channel))
                    return false;
                
                EmitterPlayedChannels[__instance.GetInstanceID()] = channel;
            }
            else if (CustomSoundBuses.TryGetValue(soundPath, out Bus bus))
            {
                if (!AudioUtils.TryPlaySound(sound, bus, out channel))
                    return false;
                
                EmitterPlayedChannels[__instance.GetInstanceID()] = channel;
            }
            else
            {
                return false;
            }

            SetChannel3DAttributes(EmitterPlayedChannels[__instance.GetInstanceID()], __instance.transform);
            __instance._playing = true;
            __instance.OnPlay();

            return false;
        }
        
        [HarmonyPatch(typeof(FMOD_CustomEmitter), nameof(FMOD_CustomEmitter.Stop))]
        [HarmonyPrefix]
        public static bool FMOD_CustomEmitter_Stop_Prefix(FMOD_CustomEmitter __instance)
        {
            if (!EmitterPlayedChannels.TryGetValue(__instance.GetInstanceID(), out Channel channel))
            {
                return true;
            }

            TryFadeOutBeforeStop(channel);
            
            __instance._playing = false;
            __instance.OnStop();

            return false;
        }

        [HarmonyPatch(typeof(FMOD_CustomEmitter), nameof(FMOD_CustomEmitter.ManagedUpdate))]
        [HarmonyPrefix]
        public static bool FMOD_CustomEmitter_ManagedUpdate_Prefix(FMOD_CustomEmitter __instance)
        {
            if (!EmitterPlayedChannels.TryGetValue(__instance.GetInstanceID(), out Channel channel))
            {
                return true;
            }

            if (__instance.followParent && channel.isPlaying(out bool playing) == RESULT.OK && playing)
            {
                __instance.attributes = __instance.transform.To3DAttributes();
                SetChannel3DAttributes(channel, __instance.transform);
            }

            return false;
        }
        
        [HarmonyPatch(typeof(FMOD_CustomEmitter), nameof(FMOD_CustomEmitter.SetAsset))]
        [HarmonyPrefix]
        public static bool FMOD_CustomEmitter_SetAsset_Prefix(FMOD_CustomEmitter __instance, FMODAsset newAsset)
        {
            if (newAsset == null)
            {
                return false;
            }

            if (!CustomSounds.ContainsKey(newAsset.path) && !CustomFModSounds.ContainsKey(newAsset.path))
            {
                return true;
            }

            __instance.ReleaseEvent();
            __instance.asset = newAsset;

            return false;
        }
        
        [HarmonyPatch(typeof(FMOD_CustomEmitter), nameof(FMOD_CustomEmitter.ReleaseEvent))]
        [HarmonyPrefix]
        public static bool FMOD_CustomEmitter_ReleaseEvent_Prefix(FMOD_CustomEmitter __instance)
        {
            if (__instance.asset == null || (!CustomSounds.ContainsKey(__instance.asset.path) && !CustomFModSounds.ContainsKey(__instance.asset.path)))
            {
                return true;
            }

            if (!EmitterPlayedChannels.TryGetValue(__instance.GetInstanceID(), out Channel channel))
            {
                return false; // known sound but not played yet
            }

            TryFadeOutBeforeStop(channel);

            
            EmitterPlayedChannels.Remove(__instance.GetInstanceID());

            return false;
        }
        
        [HarmonyPatch(typeof(FMOD_CustomLoopingEmitter), nameof(FMOD_CustomLoopingEmitter.PlayStopSound))]
        [HarmonyPrefix]
        public static bool FMOD_CustomLoopingEmitter_PlayStopSound_Prefix(FMOD_CustomLoopingEmitter __instance)
        {
            if (__instance.assetStop == null)
            {
                return true;
            }

            if (string.IsNullOrEmpty(__instance.assetStop.path) || (!CustomSounds.TryGetValue(__instance.assetStop.path, out Sound sound) 
                && !CustomFModSounds.ContainsKey(__instance.asset.path)))
            {
                return true;
            }

            string soundPath = __instance.assetStop.path;
            Channel channel;
            if (CustomFModSounds.TryGetValue(soundPath, out IFModSound fModSound))
            {
                if (!fModSound.TryPlaySound(out channel)) 
                    return false;
            }
            else if (CustomSoundBuses.TryGetValue(soundPath, out Bus bus))
            {
                if (!AudioUtils.TryPlaySound(sound, bus, out channel)) 
                    return false;
            }
            else
            {
                return false;
            }
            
            SetChannel3DAttributes(channel, __instance.transform);
            __instance.timeLastStopSound = Time.time;

            return false;
        }
        
        [HarmonyPatch(typeof(FMOD_CustomLoopingEmitter), nameof(FMOD_CustomLoopingEmitter.OnPlay))]
        [HarmonyPrefix]
        public static bool FMOD_CustomLoopingEmitter_OnPlay_Prefix(FMOD_CustomLoopingEmitter __instance)
        {
            if (__instance.assetStart == null)
            {
                return true;
            }

            if (string.IsNullOrEmpty(__instance.assetStart.path) || (!CustomSounds.TryGetValue(__instance.assetStart.path, out Sound sound) 
                && !CustomFModSounds.ContainsKey(__instance.asset.path)))
            {
                return true;
            }

            string soundPath = __instance.assetStart.path;
            Channel channel;
            
            if (CustomFModSounds.TryGetValue(soundPath, out IFModSound fModSound))
            {
                if (!fModSound.TryPlaySound(out channel))
                    return false; 
            }
            else if (CustomSoundBuses.TryGetValue(soundPath, out Bus bus))
            {
                if (!AudioUtils.TryPlaySound(sound, bus, out channel))
                    return false;
            }
            else
            {
                return false;
            }
            
            SetChannel3DAttributes(channel, __instance.transform);
            __instance.timeLastStopSound = Time.time;
            BehaviourUpdateUtils.Register(__instance);

            return false;
        }
#endif

    internal static void SetChannel3DAttributes(Channel channel, Transform transform)
    {
        ATTRIBUTES_3D attributes = transform.To3DAttributes();
        channel.set3DAttributes(ref attributes.position, ref attributes.velocity);
    }
        
    internal static void SetChannel3DAttributes(Channel channel, Vector3 position)
    {
        ATTRIBUTES_3D attributes = position.To3DAttributes();
        channel.set3DAttributes(ref attributes.position, ref attributes.velocity);
    }
    
    private static bool TryFadeOutBeforeStop(Channel channel)
    {
        if (channel.getCurrentSound(out var sound) != RESULT.OK || !FadeOuts.TryGetValue(sound.handle, out var fadeOut))
        {
            channel.stop();
            return false;
        }

        channel.getDelay(out ulong _, out ulong _, out bool stopChannels);
        
        if (stopChannels)
            return false;
            
        RuntimeManager.CoreSystem.getSoftwareFormat(out var samplesRate, out _, out _);
        channel.AddFadeOut(fadeOut.Seconds, out var dspClock);
        channel.setDelay(0, dspClock + (ulong)(samplesRate * fadeOut.Seconds));
        return true;
    }
}