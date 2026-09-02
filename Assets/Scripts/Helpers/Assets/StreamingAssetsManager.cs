using NAudio.Wave;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TGP.Helpers;
using UnityEngine;
using UnityEngine.Networking;
using Helpers.Engine;

namespace Helpers.Assets
{
    /// <summary>
    /// [RENAME] Streaming Assets
    /// <see cref="Peripherals.IO"/> ??
    /// </summary>
    public static class StreamingAssetsManager
    {
        private static Dictionary<string, AudioClip> cachedAudioClips = new Dictionary<string, AudioClip>();

        #region Public Methods

        /// <summary>
        /// Load AudioClip from wav or mp3 file
        /// </summary>
        public static void LoadClipFromFile(string path, Action<AudioClip> callback)
        {
            if (cachedAudioClips.ContainsKey(path))
            {
                callback(cachedAudioClips[path]);
                return;
            }

            if (!File.Exists(path))
            {
                if (Debug.isDebugBuild)
                    Debug.LogWarning("Audio clip doesn't exist. " + path);
                callback(null);
                return;
            }

            if (path.Contains(".mp3"))
            {
                EngineWrapper.StartCoroutine(LoadMp3FileIE(path, (clip) =>
                {
                    if (!cachedAudioClips.ContainsKey(path))
                        cachedAudioClips.Add(path, clip);
                    callback(clip);
                }));
            }
            else if (path.Contains(".wav"))
            {
                EngineWrapper.StartCoroutine(LoadAudioFile(path, (clip) =>
                {
                    if (!cachedAudioClips.ContainsKey(path))
                        cachedAudioClips.Add(path, clip);
                    callback(clip);
                }));
            }
            else
            {
                if (Debug.isDebugBuild)
                    Debug.LogError("Unsupported file format: " + path.Split('.')[1]);
                callback(null);
            }
        }


        /// <summary>
        /// Load Sprite from file (StreamingAssets) 
        /// </summary>
        public static void GetSprite(string path, Action<Sprite> callback)
        {
            EngineWrapper.StartCoroutine(LoadSprite(path, (sprite) =>
            {
                callback(sprite);
            }));
        }

        /// <summary>
        /// Load all Sprites from directory (StreamingAssets) 
        /// </summary>
        public static void GetAllSprites(string path, Action<List<Sprite>> callback)
        {
            List<Sprite> sprites = new List<Sprite>();
            // Check if Directory Exists
            if (!Directory.Exists(path))
            {
                callback(sprites);
                return;
            }

            string[] files = Directory.GetFiles(path);
            int completed = 0;
            for (int i = 0; i < files.Length; i++)
            {
                EngineWrapper.StartCoroutine(LoadSprite(files[i], (sprite) =>
                {
                    if (sprite != null)
                        sprites.Add(sprite);
                    completed++;

                    if (completed == files.Length)
                    {
                        if (callback != null)
                            callback(sprites);
                    }
                }));
            }
        }

        public static void ClearCache()
        {
            cachedAudioClips.Clear();
        }

        #endregion

        /// <summary>
        /// Load AudioClip enumerator
        /// </summary>
        private static IEnumerator LoadAudioFile(string file, Action<AudioClip> callback)
        {
            if (file.Contains(".meta"))
            {
                callback(null);
                yield break;
            }

            string url = string.Format("file://{0}", file);
            UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV);
            yield return req.SendWebRequest();
            AudioClip clip = ((DownloadHandlerAudioClip)req.downloadHandler).audioClip;
            clip.name = file.Split('/').GetLast();
            if (callback != null)
                callback(clip);
        }

        /// <summary>
        /// Load Sprite enumerator
        /// </summary>
        private static IEnumerator LoadSprite(string file, Action<Sprite> callback)
        {
            if (file.Contains(".meta"))
            {
                callback(null);
                yield break;
            }
            string url = string.Format("file://{0}", file);
            UnityWebRequest req = UnityWebRequestTexture.GetTexture(url);
            yield return req.SendWebRequest();
            Texture2D myTexture = ((DownloadHandlerTexture)req.downloadHandler).texture;
            Sprite sprite = Sprite.Create(myTexture as Texture2D, new Rect(0, 0, myTexture.width, myTexture.height), Vector2.one / 2);

            // Get the name
            string[] s = file.Split('\\', '/');
            string name = s[s.Length - 1];
            name = name.Remove(name.Length - 4, 4);

            sprite.name = name;

            if (callback != null)
                callback(sprite);
        }

        #region Load From MP3

        private static AudioFileReader audioReader;
        private static float[] audioData;

        private static IEnumerator LoadMp3FileIE(string path, Action<AudioClip> callback)
        {
            if (callback == null) yield break;

            // Parse the file with NAudio
            audioReader = new AudioFileReader(path);

            // Create an empty float to fill with song data
            audioData = new float[audioReader.Length];

            int readStartIndex = 0;
            int maxLength = 50000;
            float timeStarted = TimeWrapper.realtimeSinceStartup_NotTS;

            // Read file progressingly so we don't lock main Thread
            while (true)
            {
                //Read the file and fill the float
                audioReader.Read(audioData, readStartIndex, maxLength);

                // Check if remaining part is less than maxLength
                maxLength = (readStartIndex + maxLength <= (int)audioReader.Length) ? maxLength : (int)audioReader.Length - readStartIndex;

                // Increment read index
                readStartIndex += maxLength;

                // Wait for end of file
                if (readStartIndex + maxLength >= (int)audioReader.Length)
                    break;

                // Allow main thread to update
                if (TimeWrapper.realtimeSinceStartup_NotTS - timeStarted > 1 / 60f)
                {
                    yield return null;
                    timeStarted = TimeWrapper.realtimeSinceStartup_NotTS;
                }
            }

            // Create a clip file the size needed to collect the sound data
            AudioClip clip = AudioClip.Create(path, (int)audioReader.Length, audioReader.WaveFormat.Channels, audioReader.WaveFormat.SampleRate, false);
            clip.name = audioReader.FileName;
            // Maintain FPS
            yield return null;

            // Fill the file with the sound data
            clip.SetData(audioData, 0);
            callback(clip);
        }

        #endregion
    }
}