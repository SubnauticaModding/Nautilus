namespace SMLHelper.V2.Patchers
{
    using FMOD;
    using FMODUnity;
    using HarmonyLib;
    using System.Collections.Generic;
    using FMOD.Studio;
    using Utility;
    using UnityEngine;
    using Logger = Logger;
    
    internal class CustomSoundPatcher
    {
        internal static readonly SelfCheckingDictionary<string, Sound> CustomSounds = new("CustomSounds");
        internal static readonly SelfCheckingDictionary<string, SoundChannel> CustomSoundChannels = new("CustomSoundChannels");
        internal static readonly SelfCheckingDictionary<string, Bus> CustomSoundBuses = new("CustomSoundBuses");
        
        private static readonly Dictionary<string, Channel> PlayedChannels = new();
        private static readonly Dictionary<int, Channel> EmitterPlayedChannels = new();

        internal static void Patch(Harmony harmony)
        {
            harmony.PatchAll(typeof(CustomSoundPatcher));
            Logger.Debug("CustomSoundPatcher is done.");
        }

#if  SUBNAUTICA
        
        [HarmonyPatch(typeof(FMODUWE), nameof(FMODUWE.PlayOneShotImpl))]
        [HarmonyPrefix]
        public static bool FMODUWE_PlayOneShotImpl_Prefix(string eventPath, Vector3 position, float volume)
        {
            if (string.IsNullOrEmpty(eventPath) || !CustomSounds.TryGetValue(eventPath, out Sound soundEvent)) return true;

            if (CustomSoundBuses.TryGetValue(eventPath, out Bus bus))
            {
                var channel = AudioUtils.PlaySound(soundEvent, bus);
                if (soundEvent.getMode(out MODE mode) == RESULT.OK)
                {
                    if ((mode & MODE._3D) == MODE._3D) // if the sound is 3D, we set the position for it.
                    {
                        SetChannel3DAttributes(channel, position);
                    }
                }
            }
            else
            {
                if (!CustomSoundChannels.TryGetValue(eventPath, out SoundChannel soundChannel))
                    soundChannel = SoundChannel.Master;
                
                var channel = AudioUtils.PlaySound(soundEvent, soundChannel);
                if (soundEvent.getMode(out MODE mode) == RESULT.OK)
                {
                    if ((mode & MODE._3D) == MODE._3D) // if the sound is 3D, we set the position for it.
                    {
                        SetChannel3DAttributes(channel, position);
                    }
                }
            }
            
            return false;
        }
        
        [HarmonyPatch(typeof(SoundQueue), nameof(SoundQueue.Play))]
        [HarmonyPrefix]
        public static bool SoundQueue_Play_Prefix(SoundQueue __instance, string sound, string subtitles)
        {
            if (string.IsNullOrEmpty(sound) || !CustomSounds.TryGetValue(sound, out Sound soundEvent)) return true;

            __instance.Stop();
            __instance._current = sound;
            soundEvent.getLength(out var length, TIMEUNIT.MS);
            __instance._length = (int)length*1000;
            __instance._lengthSeconds = length;
            if (CustomSoundBuses.TryGetValue(sound, out Bus bus))
            {
                PlayedChannels[sound] = AudioUtils.PlaySound(soundEvent, bus);
            }
            else
            {
                if (!CustomSoundChannels.TryGetValue(sound, out SoundChannel soundChannel))
                    soundChannel = SoundChannel.Master;
                
                PlayedChannels[sound] = AudioUtils.PlaySound(soundEvent, soundChannel);
            }
            
            if (!string.IsNullOrEmpty(subtitles))
            {
                Subtitles main = Subtitles.main;
                if (main)
                {
                    main.Add(subtitles);
                }
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

            ATTRIBUTES_3D attributes = Player.main.transform.To3DAttributes();
#if SUBNAUTICA_STABLE
            channel.set3DAttributes(ref attributes.position, ref attributes.velocity, ref attributes.forward);
#elif SUBNAUTICA_EXP
            channel.set3DAttributes(ref attributes.position, ref attributes.velocity);
#endif
            
            channel.getPosition(out var position, TIMEUNIT.MS);
            __instance.position = (int)position*1000;
            __instance._positionSeconds = position;
            
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

            channel.setPosition((uint)Mathf.Clamp(value*0.001f, 0, __instance._length), TIMEUNIT.MS);
            
            return false;
        }

        [HarmonyPatch(typeof(FMOD_CustomEmitter), nameof(FMOD_CustomEmitter.Play))]
        [HarmonyPrefix]
        public static bool FMOD_CustomEmitter_Play_Prefix(FMOD_CustomEmitter __instance)
        {
            if (string.IsNullOrEmpty(__instance.asset?.path) || !CustomSounds.TryGetValue(__instance.asset.path, out var sound)) return true;

            if (EmitterPlayedChannels.TryGetValue(__instance.GetInstanceID(), out var channel) &&
                channel.isPlaying(out var playing) == RESULT.OK && (playing || !__instance.restartOnPlay)) // already playing, no need to play it again
            {
                return false;
            }

            var soundPath = __instance.asset.path;

            if (CustomSoundBuses.TryGetValue(soundPath, out Bus bus))
            {
                EmitterPlayedChannels[__instance.GetInstanceID()] = AudioUtils.PlaySound(sound, bus);
            }
            else
            {
                if (!CustomSoundChannels.TryGetValue(soundPath, out SoundChannel soundChannel))
                    soundChannel = SoundChannel.Master;
                
                EmitterPlayedChannels[__instance.GetInstanceID()] = AudioUtils.PlaySound(soundPath, soundChannel);
            }

            SetChannel3DAttributes(channel, __instance.transform);
            __instance._playing = true;
            __instance.OnPlay();

            return false;
        }
        
        [HarmonyPatch(typeof(FMOD_CustomEmitter), nameof(FMOD_CustomEmitter.Stop))]
        [HarmonyPrefix]
        public static bool FMOD_CustomEmitter_Stop_Prefix(FMOD_CustomEmitter __instance)
        {
            if (!EmitterPlayedChannels.TryGetValue(__instance.GetInstanceID(), out var channel)) return true;

            channel.getChannelGroup(out var channelGroup);
            channelGroup.stop();
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
            if (!CustomSounds.ContainsKey(newAsset.path)) return true;

            __instance.ReleaseEvent();
            __instance.asset = newAsset;

            return false;
        }
        
        [HarmonyPatch(typeof(FMOD_CustomEmitter), nameof(FMOD_CustomEmitter.ReleaseEvent))]
        [HarmonyPrefix]
        public static bool FMOD_CustomEmitter_ReleaseEvent_Prefix(FMOD_CustomEmitter __instance)
        {
            if (__instance.asset == null || CustomSounds.ContainsKey(__instance.asset.path)) return true;
            if (!EmitterPlayedChannels.TryGetValue(__instance.GetInstanceID(), out var channel)) return false; // known sound but not played yet

            channel.stop();
            EmitterPlayedChannels.Remove(__instance.GetInstanceID());

            return false;
        }
        
        [HarmonyPatch(typeof(FMOD_CustomLoopingEmitter), nameof(FMOD_CustomLoopingEmitter.PlayStopSound))]
        [HarmonyPrefix]
        public static bool FMOD_CustomLoopingEmitter_PlayStopSound_Prefix(FMOD_CustomLoopingEmitter __instance)
        {
            if (__instance.assetStop == null) return true;
            if (string.IsNullOrEmpty(__instance.assetStop.path) || !CustomSounds.TryGetValue(__instance.assetStop.path, out var sound)) return true;
            
            var soundPath = __instance.assetStop.path;
            Channel channel;
            if (CustomSoundBuses.TryGetValue(soundPath, out Bus bus))
            {
                channel = AudioUtils.PlaySound(sound, bus);
            }
            else
            {
                if (!CustomSoundChannels.TryGetValue(soundPath, out SoundChannel soundChannel))
                    soundChannel = SoundChannel.Master;
                
                channel = AudioUtils.PlaySound(soundPath, soundChannel);
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
            if (string.IsNullOrEmpty(__instance.assetStart.path) || !CustomSounds.TryGetValue(__instance.assetStart.path, out var sound)) return true;
            
            var soundPath = __instance.assetStart.path;
            Channel channel;
            if (CustomSoundBuses.TryGetValue(soundPath, out Bus bus))
            {
                channel = AudioUtils.PlaySound(sound, bus);
            }
            else
            {
                if (!CustomSoundChannels.TryGetValue(soundPath, out SoundChannel soundChannel))
                    soundChannel = SoundChannel.Master;
                
                channel = AudioUtils.PlaySound(soundPath, soundChannel);
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
            if (string.IsNullOrEmpty(eventPath) || !CustomSounds.TryGetValue(eventPath, out Sound soundEvent)) return true;

            if (CustomSoundBuses.TryGetValue(eventPath, out Bus bus))
            {
                var channel = AudioUtils.PlaySound(soundEvent, bus);
                if (soundEvent.getMode(out MODE mode) == RESULT.OK)
                {
                    if ((mode & MODE._3D) == MODE._3D) // if the sound is 3D, we set the position for it.
                    {
                        SetChannel3DAttributes(channel, position);
                    }
                }
            }
            else
            {
                if (!CustomSoundChannels.TryGetValue(eventPath, out SoundChannel soundChannel))
                    soundChannel = SoundChannel.Master;
                
                var channel = AudioUtils.PlaySound(soundEvent, soundChannel);
                if (soundEvent.getMode(out MODE mode) == RESULT.OK)
                {
                    if ((mode & MODE._3D) == MODE._3D) // if the sound is 3D, we set the position for it.
                    {
                        SetChannel3DAttributes(channel, position);
                    }
                }
            }
            
            return false;
        }

        [HarmonyPatch(typeof(SoundQueue), nameof(SoundQueue.PlayImpl))]
        [HarmonyPrefix]
        public static bool SoundQueue_Play_Prefix(SoundQueue __instance, string sound, SoundHost host, string subtitles, int subtitlesLine, int timelinePosition = 0)
        {
            if (string.IsNullOrEmpty(sound) || !CustomSounds.TryGetValue(sound, out Sound soundEvent)) return true;

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
            if (CustomSoundBuses.TryGetValue(sound, out Bus bus))
            {
                PlayedChannels[sound] = AudioUtils.PlaySound(soundEvent, bus);
            }
            else
            {
                if (!CustomSoundChannels.TryGetValue(sound, out SoundChannel soundChannel))
                    soundChannel = SoundChannel.Master;
                
                PlayedChannels[sound] = AudioUtils.PlaySound(soundEvent, soundChannel);
            }
            
            return false;
        }
        
        [HarmonyPatch(typeof(SoundQueue), nameof(SoundQueue.StopImpl))]
        [HarmonyPrefix]
        public static bool SoundQueue_Stop_Prefix(SoundQueue __instance)
        {
            if (__instance._current is null || string.IsNullOrEmpty(__instance._current.Value.sound) || !PlayedChannels.TryGetValue(__instance._current.Value.sound, out var channel)) return true;

            channel.stop();
            PlayedChannels.Remove(__instance._current.Value.sound);
            __instance._current = null;
            
            return false;
        }
        
        [HarmonyPatch(typeof(SoundQueue), nameof(SoundQueue.Update))]
        [HarmonyPrefix]
        public static bool SoundQueue_Update_Prefix(SoundQueue __instance)
        {
            var instanceCurrent = __instance._current ?? default;
            if (string.IsNullOrEmpty(instanceCurrent.sound)  || !PlayedChannels.TryGetValue(instanceCurrent.sound, out var channel)) return true;

            ATTRIBUTES_3D attributes = Player.main.transform.To3DAttributes();
            channel.set3DAttributes(ref attributes.position, ref attributes.velocity);
            channel.getPosition(out var position, TIMEUNIT.MS);
            instanceCurrent.position = (int)position;
            __instance._positionSeconds = (float)instanceCurrent.position * 0.001f;
            __instance._current = instanceCurrent;
            return false;
        }

        [HarmonyPatch(typeof(SoundQueue), nameof(SoundQueue.GetIsStartingOrPlaying))]
        [HarmonyPrefix]
        public static bool SoundQueue_GetIsStartingOrPlaying_Prefix( ref bool __result)
        {
            var instanceCurrent = PDASounds.queue?._current ?? default;
            if (string.IsNullOrEmpty(instanceCurrent.sound)  || !PlayedChannels.TryGetValue(instanceCurrent.sound, out var channel)) return true;

            var result = channel.isPlaying(out __result);
            __result = __result && result == RESULT.OK;
            return false;
        }
        
        [HarmonyPatch(typeof(SoundQueue), nameof(SoundQueue.SetPosition))]
        [HarmonyPrefix]
        public static bool SoundQueue_Position_Setter_Prefix(SoundQueue __instance, int value)
        {
            var instanceCurrent = __instance._current ?? default;
            if (!PlayedChannels.TryGetValue(instanceCurrent.sound, out var channel)) return true;

            channel.setPosition((uint)Mathf.Clamp(value, 0, __instance._length), TIMEUNIT.MS);
            
            return false;
        }
        
        [HarmonyPatch(typeof(FMOD_CustomEmitter), nameof(FMOD_CustomEmitter.Play))]
        [HarmonyPrefix]
        public static bool FMOD_CustomEmitter_Play_Prefix(FMOD_CustomEmitter __instance)
        {
            if (string.IsNullOrEmpty(__instance.asset?.path) || !CustomSounds.TryGetValue(__instance.asset.path, out var sound)) return true;

            if (EmitterPlayedChannels.TryGetValue(__instance.GetInstanceID(), out var channel) &&
                channel.isPlaying(out var playing) == RESULT.OK && (playing || !__instance.restartOnPlay)) // already playing, no need to play it again
            {
                return false;
            }

            var soundPath = __instance.asset.path;

            if (CustomSoundBuses.TryGetValue(soundPath, out Bus bus))
            {
                EmitterPlayedChannels[__instance.GetInstanceID()] = AudioUtils.PlaySound(sound, bus);
            }
            else
            {
                if (!CustomSoundChannels.TryGetValue(soundPath, out SoundChannel soundChannel))
                    soundChannel = SoundChannel.Master;
                
                EmitterPlayedChannels[__instance.GetInstanceID()] = AudioUtils.PlaySound(soundPath, soundChannel);
            }

            SetChannel3DAttributes(channel, __instance.transform);
            __instance._playing = true;
            __instance.OnPlay();

            return false;
        }
        
        [HarmonyPatch(typeof(FMOD_CustomEmitter), nameof(FMOD_CustomEmitter.Stop))]
        [HarmonyPrefix]
        public static bool FMOD_CustomEmitter_Stop_Prefix(FMOD_CustomEmitter __instance)
        {
            if (!EmitterPlayedChannels.TryGetValue(__instance.GetInstanceID(), out var channel)) return true;

            channel.getChannelGroup(out var channelGroup);
            channelGroup.stop();
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
            if (!CustomSounds.ContainsKey(newAsset.path)) return true;

            __instance.ReleaseEvent();
            __instance.asset = newAsset;

            return false;
        }
        
        [HarmonyPatch(typeof(FMOD_CustomEmitter), nameof(FMOD_CustomEmitter.ReleaseEvent))]
        [HarmonyPrefix]
        public static bool FMOD_CustomEmitter_ReleaseEvent_Prefix(FMOD_CustomEmitter __instance)
        {
            if (__instance.asset == null || CustomSounds.ContainsKey(__instance.asset.path)) return true;
            if (!EmitterPlayedChannels.TryGetValue(__instance.GetInstanceID(), out var channel)) return false; // known sound but not played yet

            channel.stop();
            EmitterPlayedChannels.Remove(__instance.GetInstanceID());

            return false;
        }
        
        [HarmonyPatch(typeof(FMOD_CustomLoopingEmitter), nameof(FMOD_CustomLoopingEmitter.PlayStopSound))]
        [HarmonyPrefix]
        public static bool FMOD_CustomLoopingEmitter_PlayStopSound_Prefix(FMOD_CustomLoopingEmitter __instance)
        {
            if (__instance.assetStop == null) return true;
            if (string.IsNullOrEmpty(__instance.assetStop.path) || !CustomSounds.TryGetValue(__instance.assetStop.path, out var sound)) return true;
            
            var soundPath = __instance.assetStop.path;
            Channel channel;
            if (CustomSoundBuses.TryGetValue(soundPath, out Bus bus))
            {
                channel = AudioUtils.PlaySound(sound, bus);
            }
            else
            {
                if (!CustomSoundChannels.TryGetValue(soundPath, out SoundChannel soundChannel))
                    soundChannel = SoundChannel.Master;
                
                channel = AudioUtils.PlaySound(soundPath, soundChannel);
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
            if (string.IsNullOrEmpty(__instance.assetStart.path) || !CustomSounds.TryGetValue(__instance.assetStart.path, out var sound)) return true;
            
            var soundPath = __instance.assetStart.path;
            Channel channel;
            if (CustomSoundBuses.TryGetValue(soundPath, out Bus bus))
            {
                channel = AudioUtils.PlaySound(sound, bus);
            }
            else
            {
                if (!CustomSoundChannels.TryGetValue(soundPath, out SoundChannel soundChannel))
                    soundChannel = SoundChannel.Master;
                
                channel = AudioUtils.PlaySound(soundPath, soundChannel);
            }
            
            SetChannel3DAttributes(channel, __instance.transform);
            __instance.timeLastStopSound = Time.time;
            BehaviourUpdateUtils.Register(__instance);

            return false;
        }
#endif

        private static void SetChannel3DAttributes(Channel channel, Transform transform)
        {
            var attributes = transform.To3DAttributes();
#if SUBNAUTICA_STABLE
            channel.set3DAttributes(ref attributes.position, ref attributes.velocity, ref attributes.forward);
#else
            channel.set3DAttributes(ref attributes.position, ref attributes.velocity);
#endif
        }
        
        private static void SetChannel3DAttributes(Channel channel, Vector3 position)
        {
            var attributes = position.To3DAttributes();
#if SUBNAUTICA_STABLE
            channel.set3DAttributes(ref attributes.position, ref attributes.velocity, ref attributes.forward);
#else
            channel.set3DAttributes(ref attributes.position, ref attributes.velocity);
#endif
        }
    }
}