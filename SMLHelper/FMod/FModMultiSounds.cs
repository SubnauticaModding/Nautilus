using FMOD;
using FMOD.Studio;
using FMODUnity;
using SMLHelper.V2.FMod.Interfaces;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace SMLHelper.V2.FMod
{
    /// <summary>
    /// This class is used to register FMOD events with multiple sounds in one event.
    /// </summary>
    public class FModMultiSounds : IFModSound
    {
        private Sound[] _sounds;

        private Bus _bus;

        private int _currentIndex;
        private int Index
        {
            get
            {
                if (_currentIndex >= _sounds.Length - 1)
                {
                    _currentIndex = 0;
                }

                return _currentIndex++;
            }
        }

        /// <summary>
        /// Constructs a new instance of <see cref="FModMultiSounds"/>. Used to register FMOD events with multiple sounds in one event.
        /// </summary>
        /// <param name="sounds">The sounds to register for this object. Please ensure that none of the sounds are on <see cref="MODE.LOOP_NORMAL"/> or <see cref="MODE.LOOP_BIDI"/> modes.</param>
        /// <param name="busPath"><see cref="Bus"/> path to play these sounds under.</param>
        public FModMultiSounds(Sound[] sounds, string busPath)
        {
            _sounds = sounds;
            _bus = RuntimeManager.GetBus(busPath);
        }
        
        /// <summary>
        /// Makes the sounds play in a randomized order. when <c>false</c>, sounds will play subsequently.<br/>
        /// Defaults to <c>false</c>.
        /// </summary>
        public bool RandomizeSounds { get; set; }

        Channel IFModSound.PlaySound()
        {
            if (_sounds is {Length: > 0})
            {
                if (RandomizeSounds)
                {
                    return AudioUtils.PlaySound(_sounds[Random.Range(0, _sounds.Length)], _bus);
                }

                return AudioUtils.PlaySound(_sounds[Index], _bus);
            }

            Logger.Error("MultiSounds must have some sounds.");
            return default;
        }
    }
}