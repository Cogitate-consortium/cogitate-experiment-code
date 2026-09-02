// [NS_DEBATABLE] | Could get fed ASSETS instead of PATHS
using Helpers.Assets;

using System;
using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;

namespace Peripherals.Audio
{
    /// <summary>
    /// CLEANUP
    /// NS_SEGMENT | Base, Game (SFX/Music), Experiment (Triggers, Fixation)
    /// </summary>
    public class SoundSystem : MonoBehaviour
    {
        private static Config config;
        private static string audioFolderPath;

        public static void Initialize(Config config, string audioFolderPath)
        {
            SoundSystem.config = config;
            SoundSystem.audioFolderPath = audioFolderPath;
            ResetAudioSources();
            Mute(SoundSystem.config.startMuted);
        }

        public static void ResetAudioSources()
        {
            for (int i = 0; i < AUDIO_SOURCES_ONE_OFF.Count; i++)
                if (AUDIO_SOURCES_ONE_OFF[i])
                    Destroy(AUDIO_SOURCES_ONE_OFF[i].gameObject);
            for (int i = 0; i < ALL_AUDIO_SOURCES.Count; i++)
                if (ALL_AUDIO_SOURCES[i]?.audioSource)
                    Destroy(ALL_AUDIO_SOURCES[i].audioSource.gameObject);

            AUDIO_SOURCES_ONE_OFF.Clear();
            ALL_AUDIO_SOURCES.Clear();
        }

        public static bool isMuted { get; private set; }
        public static void Mute(bool doMute, string reason = "")
        {
            foreach (ExtendedAudioSource aS in ALL_AUDIO_SOURCES)
            {
                if (aS.audioSource == null) continue;

                if (aS.config.isExemptFromMuting) continue;
                
                aS.audioSource.mute = doMute;
            }

            isMuted = doMute;

            if (config.debug)
                Debug_Helper.LogError(typeof(SoundSystem), "MUTE -> " + isMuted.BoolToOnOff());

            onStatusUpdate_Mute?.Invoke(null, new MuteStatusArgs(isMuted, reason));
        }

        public static void PlayAudio(AudioClipConfig audioToPlay, RuntimeAudioSourceConfig audioSourceConfig, float volume = 1)
        {
            if (audioToPlay.name.IsNullOrEmpty()) return;
            if (audioToPlay.enabled == false) return;

            PlayAudioClip(GetAudioFullPath(audioToPlay), audioSourceConfig,
                audioToPlay.volume * volume);
        }

        /*
        /// <summary>
        /// Play music to Camera component
        /// </summary>
        public static void PlayAudioMusic(string path, float volume = 1.0f)
        {
            StreamingAssetsManager.LoadClipFromFile(path, (clip) =>
            {
                PlayAudioMusic(clip, volume);
            });
        }

        /// <summary>
        /// Play Music at Camera position (loop)
        /// </summary>
        public static void PlayAudioMusic(AudioClip clip, float volume = 1.0f)
        {
            AudioSource cameraAudio = Camera.main.gameObject.AddComponentIfNotExists<AudioSource>();
            cameraAudio.volume = volume;
            cameraAudio.loop = true;

            // Dont reload clip if it's the same
            if (cameraAudio.clip == clip) return;

            cameraAudio.Stop();
            cameraAudio.clip = clip;
            cameraAudio.Play();
        }

        /// <summary>
        /// Stop music from Camera component
        /// </summary>
        public static void PauseAudioMusic()
        {
            AudioSource cameraAudio = Camera.main.gameObject.GetComponent<AudioSource>();
            if (cameraAudio == null) return;
            cameraAudio.Pause();
        }

        /// <summary>
        /// Stop music from Camera component
        /// </summary>
        public static void ResumeAudioMusic()
        {
            AudioSource cameraAudio = Camera.main.gameObject.GetComponent<AudioSource>();
            if (cameraAudio == null) return;
            cameraAudio.UnPause();
        }
        */

        public static void PlayAudioMusic()
        {
            if (MUSIC_AUDIO_SOURCE == null) return;
            if (MUSIC_AUDIO_SOURCE.isPlaying) return;

            RaiseAudioEvent(AudioEvent.Play, MUSIC_AUDIO_SOURCE);

            MUSIC_AUDIO_SOURCE.Play();
        }

        public static void TogglePersistentMusic(bool on)
        {
            if (MUSIC_AUDIO_SOURCE == null) return;

            if (on)
                DontDestroyOnLoad(MUSIC_AUDIO_SOURCE.gameObject);
            else
                Utility_Helper.DestroyOnLoad(MUSIC_AUDIO_SOURCE.gameObject);
        }

        public static void ResumeAudioMusic()
        {
            if (MUSIC_AUDIO_SOURCE == null) return;
            if (MUSIC_AUDIO_SOURCE.isPlaying) return;

            RaiseAudioEvent(AudioEvent.Resume, MUSIC_AUDIO_SOURCE);
            
            MUSIC_AUDIO_SOURCE.UnPause();
        }

        public static void PauseAudioMusic()
        {
            if (MUSIC_AUDIO_SOURCE == null) return;
            if (!MUSIC_AUDIO_SOURCE.isPlaying) return;

            RaiseAudioEvent(AudioEvent.Pause, MUSIC_AUDIO_SOURCE);
#if UNITY_EDITOR
            // Debug.LogError("PAUSING");
#endif
            MUSIC_AUDIO_SOURCE.Pause();
        }

        private static void RaiseAudioEvent(AudioEvent audioEvent, AudioSource audioSource)
        {
            RaiseAudioEvent(audioEvent, audioSource, audioSource.clip);
        }

        // Clip may not be the active clip on the source! (ie. play oneshot)
        private static void RaiseAudioEvent(AudioEvent audioEvent, AudioSource audioSource, AudioClip audioClip)
        {
            onStatusUpdate_AudioSource?.Invoke(null, new AudioSourceStatusArgs(audioEvent,
                audioSource.name, audioClip.name, audioSource.time, audioSource.volume, audioSource.panStereo));
        }

        public static EventHandler<AudioSourceStatusArgs> onStatusUpdate_AudioSource;
        public static EventHandler<MuteStatusArgs> onStatusUpdate_Mute;

        public enum AudioEvent { Play, Resume, Pause }
        public class AudioSourceStatusArgs
        {
            public AudioEvent eventType;
            public string sourceName;
            public string clipName;
            public float time;
            public float volume;
            public float panStereo;

            public AudioSourceStatusArgs(AudioEvent eventType, string sourceName, string clipName, float time, float volume, float panStereo)
            {
                this.eventType = eventType;
                this.sourceName = sourceName;
                this.clipName = clipName;
                this.time = time;
                this.volume = volume;
                this.panStereo = panStereo;
            }
        }

        public static void SetKoreographySource(AudioSource audioSource, RuntimeAudioSourceConfig audioSourceConfig)
        {
            if (MUSIC_AUDIO_SOURCE != null)
            {
                MUSIC_AUDIO_SOURCE.Log("Destroying!");
                Destroy(MUSIC_AUDIO_SOURCE.gameObject);
            }

            MUSIC_AUDIO_SOURCE = audioSource;
            MUSIC_AUDIO_SOURCE.name = audioSourceConfig.name;
            InitializeAudioSource(MUSIC_AUDIO_SOURCE, audioSourceConfig);
        }

        private static AudioSource MUSIC_AUDIO_SOURCE = null;
        private static AudioSource aS_Tone = null;
        /// <summary>
        /// Play Music at Camera position (loop)
        /// </summary>
        public static void InitializeAudioTone(AudioClipConfig audioClipConfig, RuntimeAudioSourceConfig audioSourceConfig)
        {
            string path = GetAudioFullPath(audioClipConfig);
            StreamingAssetsManager.LoadClipFromFile(path, (clip) =>
            {
                if (aS_Tone == null)
                {
                    GameObject gO = new GameObject("AudioTone");
                    DontDestroyOnLoad(gO);
                    aS_Tone = gO.AddComponentIfNotExists<AudioSource>();
                }

                InitializeAudioSource(aS_Tone, audioSourceConfig);

                aS_Tone.loop = true;
                aS_Tone.Stop();
                aS_Tone.clip = clip;
                aS_Tone.Play();
                aS_Tone.volume = 0;
            });
        }

        public static void SetAudioToneVolume(float volume)
        {
            if (aS_Tone == null)
            {
                // Debug_Helper.LogError(typeof(SoundSystem), "Initialize Audio Tone first");
                return;
            }

            aS_Tone.volume = volume;
        }

        /// <summary>
        /// Load AudioClip from file and play it
        /// </summary>
        private static void PlayAudioClip(string path, RuntimeAudioSourceConfig audioSourceConfig, float volume = 1.0f)
        {
            StreamingAssetsManager.LoadClipFromFile(path, (clip) =>
            {
                PlayAudioClip(clip, audioSourceConfig, volume);
            });
        }

        /// <summary>
        /// Play an AudioClip at Camera position
        /// </summary>
        private static void PlayAudioClip(AudioClip clip, RuntimeAudioSourceConfig audioSourceConfig, float volume = 1.0f)
        {
            if (clip == null)
            {
                Debug_Helper.LogWarning(typeof(SoundSystem), "Clip was empty! Aborting play.");
                return;
            }

            AudioSource aS = GetAvailableAudioSource(audioSourceConfig);
            aS.transform.position = Camera.main.transform.position;
            aS.PlayOneShot(clip, volume);
            // Careful NOT aS.clip (it's empty as we are playing one-shot)
            RaiseAudioEvent(AudioEvent.Play, aS, clip);
        }

        /// <summary>
        /// Finds or Creates an audio source, and sets it up
        /// </summary>
        /// <returns></returns>
        private static AudioSource GetAvailableAudioSource(RuntimeAudioSourceConfig audioSourceConfig)
        {
            foreach (AudioSource _aS in AUDIO_SOURCES_ONE_OFF)
                if (_aS != null && !_aS.isPlaying)
                    return _aS;

            AudioSource aS = CreateAudioSource("AudioSourcePoint_" + AUDIO_SOURCES_ONE_OFF.Count, audioSourceConfig);
            AUDIO_SOURCES_ONE_OFF.Add(aS);
            return aS;
        }

        public static AudioSource GetAudioSourceTrigger(RuntimeAudioSourceConfig audioSourceConfig)
        {
            if (AUDIO_SOURCE_TRIGGER == null)
                AUDIO_SOURCE_TRIGGER = CreateAudioSource("AUDIO_SOURCE_TRIGGER", audioSourceConfig);

            return AUDIO_SOURCE_TRIGGER;
        }

        private static AudioSource CreateAudioSource(string name, RuntimeAudioSourceConfig audioSourceConfig)
        {
            GameObject temp = new GameObject(name);
            AudioSource aS = temp.AddComponent<AudioSource>();
            aS.loop = false;
            aS.playOnAwake = false;
            DontDestroyOnLoad(aS.gameObject);
            InitializeAudioSource(aS, audioSourceConfig);

            return aS;
        }

        private static readonly List<AudioSource> AUDIO_SOURCES_ONE_OFF = new List<AudioSource>();
        private static readonly List<ExtendedAudioSource> ALL_AUDIO_SOURCES = new List<ExtendedAudioSource>();

        public class ExtendedAudioSource
        {
            public AudioSource audioSource;
            public RuntimeAudioSourceConfig config;

            public ExtendedAudioSource(AudioSource audioSource, RuntimeAudioSourceConfig config)
            {
                this.audioSource = audioSource;
                this.config = config;
            }
        }

        public class RuntimeAudioSourceConfig
        {
            public string name;
            public bool isExemptFromMuting;
            public StereoType stereoType;

            public RuntimeAudioSourceConfig(string name, bool isExemptFromMuting, StereoType stereoType)
            {
                this.name = name;
                this.isExemptFromMuting = isExemptFromMuting;
                this.stereoType = stereoType;
            }
        }

        private static AudioSource AUDIO_SOURCE_TRIGGER = null;

        private static void InitializeAudioSource(AudioSource audioSource, RuntimeAudioSourceConfig audioSourceCOnfig)
        {
            if (audioSource == null) return;
            audioSource.spatialBlend = 0; // 2D Sound!
                        
            if (!audioSourceCOnfig.isExemptFromMuting)
                audioSource.mute = isMuted;

            audioSource.panStereo = 
                audioSourceCOnfig.stereoType == StereoType.Both ? 0 :
                audioSourceCOnfig.stereoType == StereoType.Left ? -1 : 1;

            // Useful when testing if everything is passing through initialization
            // Commenting out -> you shouldnt hear a single sound in-game
            // audioSource.enabled = false;

            audioSource.LogWarning("{0} set my pan to {1}"._Format(typeof(SoundSystem), audioSource.panStereo));
            ALL_AUDIO_SOURCES.Add(new ExtendedAudioSource(audioSource, audioSourceCOnfig));
        }

        public enum Type { Music, SFX }

        private static string GetAudioFullPath(AudioClipConfig audioFile)
        {
            return audioFolderPath + "/" + audioFile.name;
        }

        [Serializable]
        public class Config
        {
            public bool debug = false;
            public bool useAudioMixer = true;

            // --- EXPERIMENT
            public bool localizerSounds_Response = false;
            public bool muteTriggersToo = false;
            public bool startMuted = false;
            public AudioClipConfig ResponseReward;
            public AudioClipConfig ResponsePenalty;
            public AudioClipConfig FixationBreak;
            public float eyeTrackerNoConnection = 0.0f;
            public float eyeTrackerNoDataWarningVolume = 0.0f;
            public float eyeTrackerBlinkVolume = 0.0f;
            public float eyeTrackerOutOfScreenVolume = 0.5f;
            public float eyeTrackerOffFixationVolume = 0.0f;

            public float Volume = 1.0f;
            // public AudioClipConfig Click;
            // public AudioClipConfig Music;
        }

        public enum StereoType { Left = -1, Both = 0, Right = 1 }


        [System.Serializable]
        public struct AudioClipConfig
        {
            public bool enabled;
            public string name;
            public float volume;
        }

        public class MuteStatusArgs
        {
            public bool isMuted;
            public string reason;

            public MuteStatusArgs(bool isMuted, string reason)
            {
                this.isMuted = isMuted;
                this.reason = reason;
            }
        }
    }
}