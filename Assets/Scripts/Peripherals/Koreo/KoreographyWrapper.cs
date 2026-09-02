using Helpers.Assets;
using Helpers.Engine;
using SonicBloom.Koreo;
using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Peripherals.Koreo
{
    public static class KoreographyWrapper
    {
        private const string KOREOGRAPHER_PATH = "Koreography/Koreography_{0}";

        public static Koreographer koreographer;
        public static event EventHandler<EventArgs> onRhythm;
        public static event EventHandler<EventArgs> onMelody;

        private static void RaiseOnRhythmEvent()
        {
            onRhythm?.Invoke(null, new EventArgs());
        }

        private static void RaiseOnMelodyEvent()
        {
            onMelody?.Invoke(null, new EventArgs());
        }

        public static AudioSource InstantiateKoreographer(RuntimeConfig runtimeConfig)
        {
            if (koreographer != null)
                EngineWrapper.Destroy(koreographer.gameObject);

            GameObject _newKoreo = ResourceHelper.InstantiateResource<GameObject>(string.Format(KOREOGRAPHER_PATH, runtimeConfig.index));
            koreographer = _newKoreo.GetComponent<Koreographer>();
            koreographer.RegisterForEvents("Rhythm", (e) => { RaiseOnRhythmEvent(); });
            koreographer.RegisterForEvents("Melody", (e) => { RaiseOnMelodyEvent(); });
            AudioSource audioSource = _newKoreo.GetComponentInChildren<AudioSource>();
            audioSource.volume = runtimeConfig.volume;
            audioSource.outputAudioMixerGroup = runtimeConfig.audioMixerGroup;

            return audioSource;
        }

        public class RuntimeConfig
        {
            public int index;
            public float volume;
            public UnityEngine.Audio.AudioMixerGroup audioMixerGroup;

            public RuntimeConfig(int index, float volume, AudioMixerGroup audioMixerGroup)
            {
                this.index = index;
                this.volume = volume;
                this.audioMixerGroup = audioMixerGroup;
            }
        }
    }
}