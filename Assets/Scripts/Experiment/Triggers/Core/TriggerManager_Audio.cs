// NS_REMOVE
using Peripherals.Logging.Core;

using Experiment.Managers;
using Helpers.Async;
using Helpers.Engine;
using Peripherals.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;

namespace Experiment.Triggers.Core
{
    /// Some of this playback functionality should go to <see cref="SoundSystem"/> or a 3rd script | This should not know the TECH used to play audio
    public class TriggerManager_Audio : TriggerManager
    {
        #region TEMP LOW LEVEL
        private static string DUMMY_TRIGGER_TO_USE;

        // Start is called before the first frame update
        public static void Prepare(Config config, int numBits, Action<ProgressReport> progressReportCB)
        {
            GenerateAllIE(config, numBits, a =>
            {
                if (a.isDone)
                {
                    WavePlayerManager.BUFFER_SIZE = config.BUFFER_SIZE;

                    // Load all the files inside of 
                    List<string> fileNames = FileWrapper.GetFolderContents(GetAudioTriggersPath(), FilesFolders.Files, false);
                    for (int i = 0; i < fileNames.Count; i++)
                    {
                        string fileName = fileNames[i];
                        string filePath = GetFilePath(fileName);

                        string[] fileParts = fileName.Split(true, ".");
                        int numToRemove = fileParts.GetLast().Length + 1; // suffix length + the dot
                        string fileNameStripped = fileName.RemoveLast(numToRemove);

                        WavePlayerManager.LoadFile(fileNameStripped, filePath);
                    }

                    int EMPTY_TRIGGER_MS = config.bitDurationMS * DUMMY_TRIGGER_TO_USE.Length;

                    // Play the "empty" code once
                    WavePlayerManager.TryPlay_TS(DUMMY_TRIGGER_TO_USE, EMPTY_TRIGGER_MS, success =>
                    {
                        AsyncThread.RunOnMainThread_ASAP_TS(() =>
                        {
                            progressReportCB?.Invoke(new ProgressReport(true, 1));
                        });
                    });
                }
                else
                    progressReportCB?.Invoke(new ProgressReport(false, a.value01 - float.Epsilon));
            });
        }

        private static void GenerateAllIE(Config config, int numBits, Action<ProgressReport> progressReportCB = null)
        {
            float amplification_Onset = config.amplification_Base * config.amplification_Multi_Onset;
            float[] pulseSignal_Silence = new float[0];
            float[] pulseSignal_Base = new float[0];
            float[] pulseSignal_Amplified = new float[0];
            CreatePulseSignals(config.fs, config.bitDurationSeconds, config.amplification_Base, amplification_Onset, out pulseSignal_Silence, out pulseSignal_Base, out pulseSignal_Amplified);

            TriggerManager_Audio_Helper.ChannelType channelType = config._channel == LeftRight.Left ?
                TriggerManager_Audio_Helper.ChannelType.StereoLeft :
                TriggerManager_Audio_Helper.ChannelType.StereoRight;

            AudioClip triggerAC = null;
            List<string> triggersToGenerate = GetListOfTriggers(config, numBits);

            if (triggersToGenerate.Contains(config.DUMMY_TRIGGER))
                DUMMY_TRIGGER_TO_USE = Config.DEFAULT_DUMMY_TRIGGER;
            else
                DUMMY_TRIGGER_TO_USE = config.DUMMY_TRIGGER;

            triggersToGenerate.Add(DUMMY_TRIGGER_TO_USE);
            // Debug.Log(DateTime.UtcNow);

            progressReportCB?.Invoke(new ProgressReport(false, 0));

            for (int i = 0; i < triggersToGenerate.Count; i++)
            {
                string trigger_Bit_String = triggersToGenerate[i];
                string filePath = GetFilePath(trigger_Bit_String);
                triggerAC = TriggerManager_Audio_Helper.CreateAudioTrigger(trigger_Bit_String, config.fs, channelType, false, pulseSignal_Silence, pulseSignal_Base, pulseSignal_Amplified);
                SavWav.Save(filePath, triggerAC);
                progressReportCB?.Invoke(new ProgressReport(false, (float)i + 1 / triggersToGenerate.Count - float.Epsilon));
            }

            progressReportCB?.Invoke(new ProgressReport(true, 1));
            // Debug.Log(DateTime.UtcNow);
        }
        private static string GetAudioTriggersPath()
        {
            return Application.dataPath + "/StreamingAssets/Content/Audio/Triggers/";
        }

        private static string GetFilePath(string fileName)
        {
            return string.Format("{0}{1}", GetAudioTriggersPath(), fileName);
        }

        public static List<string> GetListOfTriggers(Config config, int numBits)
        {
            List<string> listOfTriggers = new List<string>();

            for (int i = 0; i < Mathf.Pow(2, numBits); i++)
                listOfTriggers.Add(GetCode(i, config, false));

            return listOfTriggers;
        }

        #endregion

        private static Coroutine processRequestsCR = null;

        private TriggerOutEvent lastRequest = default;

        private List<KeyValuePair<TriggerOutEvent, string>> triggerRequests = new List<KeyValuePair<TriggerOutEvent, string>>();

        private Config config;
        private RuntimeConfig runtimeConfig;
        
        public TriggerManager_Audio(Config config, RuntimeConfig runtimeConfig)
            : base(config, runtimeConfig)
        {
            this.config = config;
            this.runtimeConfig = runtimeConfig;
            
            triggerRequests.Clear();
            processRequestsCR = EngineWrapper.StartCoroutine(IE_ProcessRequests(Utility_Helper_MB.instance));
        }

        public override void Dispose()
        {
            EngineWrapper.StopCoroutine(processRequestsCR);
        }

        bool CheckShouldSend_TS(TriggerMaster.TriggerEventArgs e)
        {
            TimeWrapper.Timestamp timestamp = TimeWrapper.GetCurrentTimestamp_TS();

            if (!isInitialized)
                return false;

            //ExperimentManagerSession.LogData_AsTheyHappen_TS(timestamp,
            //    "TRIGGER_MANAGER_AUDIO", "REQUEST_RECEIVED;{0};{1};[0] type, [1] code"._Format(e.type, e.code), config.debug);

            // Only log things if we are meant to
            if (!ShouldSend(e.type))
            {
             //   ExperimentManagerSession.LogData_AsTheyHappen_TS(timestamp,
            //        "TRIGGER_MANAGER_AUDIO", "REQUEST_DENIED;{0};{1};[0] type, [1] code"._Format(e.type, e.code), config.debug);
                return false;
            }

            return true;
        }

        private IEnumerator IE_ProcessRequests(MonoBehaviour caller)
        {
            while (true)
            {
                if (triggerRequests.Count > 1)
                {
                    bool? success = null;
                    string message = "";

                    Action<bool, string> callback = (_success, _message) =>
                    {
                        success = _success;
                        message = _message;
                    };

                    TrySendTrigger_TS(triggerRequests[0], callback);

                    yield return new WaitUntil(() => success.HasValue);

                    // either we succeeded, 
                    if (success == true ||
                        //or we're config'ed not to try again
                        config.abortIfPlaying)
                    {
                        triggerRequests.RemoveAt(0);
                    }
                }

                yield return null;
            }
        }

        private static string GetCode(int baseCode, Config config, bool doDoubleOnset)
        {
            string code = config.triggerPrefix;

            // Double onset for levels
            if (doDoubleOnset)
                code += config.triggerPrefix;

            // Convert it and pan it
            if (config.appendTriggerCode)
            {
                string triggerCode = Convert.ToString(baseCode, 2);

                if (config.padUntilCertainBitLength > triggerCode.Length)
                    triggerCode = triggerCode.PadLeft(config.padUntilCertainBitLength, '0');

                code += triggerCode;
            }

            code += config.triggerSuffixBits;

            return code;
        }

        public static void CreatePulseSignals(float _fs, float bitDurationSeconds, float amplification_Base, float amplification_Amplified, out float[] pulseSignal0, out float[] pulseSignal1, out float[] pulseSignal2)
        {
            int numSamples = Mathf.CeilToInt(_fs * bitDurationSeconds);

            if (numSamples % 2 != 0) // Force it to be even
                numSamples++;

            pulseSignal0 = new float[numSamples];
            pulseSignal1 = new float[numSamples];
            pulseSignal2 = new float[numSamples];

            float max = Mathf.Max(amplification_Base, amplification_Amplified);

            if (max > 1)
            {
                amplification_Base /= max;
                amplification_Amplified /= max;

                amplification_Base = amplification_Base.Clamped01();
                amplification_Amplified = amplification_Amplified.Clamped01();
            }

            for (int i = 0; i < numSamples / 2; i++)
            {
                pulseSignal1[i] = amplification_Base;
                pulseSignal2[i] = amplification_Amplified;
            }
            for (int i = numSamples / 2; i < numSamples; i++)
            {
                pulseSignal1[i] = -amplification_Base;
                pulseSignal2[i] = -amplification_Amplified;
            }
        }

        public new class RuntimeConfig : TriggerManager.RuntimeConfig
        {
            public SoundSystem.RuntimeAudioSourceConfig audioTrigger;

            public RuntimeConfig(Func<TriggerOutEvent, bool> shouldFire, SoundSystem.RuntimeAudioSourceConfig audioTrigger) : base(shouldFire)
            {
                this.audioTrigger = audioTrigger;
            }
        }

        public void SendTrigger_TS(TriggerMaster.TriggerEventArgs e)
        {
            if (!CheckShouldSend_TS(e)) return;

            // Ready to play!
            bool doDoubleOnset =
                (e.type == TriggerOutEvent.LevelBegin && config.doubleOnsetForLevelStart) ||
                (e.type == TriggerOutEvent.LevelEnd && config.doubleOnsetForLevelEnd);

            int codeInt = e.codePrio.code;
            string code = GetCode(codeInt, config, doDoubleOnset);
            KeyValuePair<TriggerOutEvent, string> kVP = new KeyValuePair<TriggerOutEvent, string>(e.type, code);

            TrySendTrigger_TS(kVP, (success, message) =>
            {
                if (!success)
                {
                    this.LogWarning(kVP.Key + " : " + kVP.Value + " REJECTED !");
                    triggerRequests.Add(kVP);
                }
            });

            ExperimentManagerSession.LogData_AsTheyHappen_TS(TimeWrapper.GetCurrentTimestamp_TS(),
                "TRIGGER_MANAGER_AUDIO", "REQUEST_PROCESSED;{0};{1}"._Format(e.type, codeInt)); // [0] type, [1] code
        }

        private void TrySendTrigger_TS(KeyValuePair<TriggerOutEvent, string> kVP, Action<bool, string> callback)
        {
            TimeWrapper.Timestamp requestTS = TimeWrapper.GetCurrentTimestamp_TS();

            Action<bool> playerCB = playbackSuccess =>
            {
                if (playbackSuccess)
                {
                    string msg = "Sent audio trigger {0}"._Format(kVP.Value);
                    callback?.Invoke(true, msg);
                }

                else
                {
                    string msgError = "Couldn't play Audio Trigger :: " + kVP.Key + ". Was busy with " + lastRequest + ".";

                    if (config.debug)
                        Debug.LogError(msgError);

                    ExperimentManagerSession.LogData_AsTheyHappen_TS(requestTS, "TRIGGER_MANAGER_AUDIO",
                        string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8}",
                        "TRIGGER_FAILED", kVP.Key, kVP.Value, config.fs, config.channel,
                        config.bitDurationSeconds, config.amplification_Base, config.amplification_Multi_Onset, lastRequest));
                    // "[0] Trigger Event, [1] Trigger String, [2] Fs, [3] Channel, [4] bit duration (s)," +
                    // "[5] amplification (base), [6] amplification multi (onset), [7] last request"

                    callback?.Invoke(false, msgError);
                };
            };

            switch (config.playerType)
            {
                case Config.PlayerType.LowLevel:
                    try
                    {
                        TrySendTrigger_LowLatency_TS(kVP.Key, kVP.Value, playerCB);
                    }
                    catch(Exception e)
                    {
                        Debug.LogError(e);
                    }
                    break;
                case Config.PlayerType.Legacy:
                    TrySendTrigger_Legacy_TS(kVP.Key, kVP.Value, playerCB);
                    break;
            }
        }

        private void TrySendTrigger_LowLatency_TS(TriggerOutEvent triggerEvent, string trigger_String, Action<bool> onDonePlaying)
        {
            if (WavePlayerManager.isPlaying)
            {
                onDonePlaying(false);
                return;
            }

            float durationMS = config.bitDurationMS * trigger_String.Length;

            TimeWrapper.Timestamp timestamp = TimeWrapper.GetCurrentTimestamp_TS();

            WavePlayerManager.TryPlay_TS(trigger_String, durationMS, success =>
            {
                onDonePlaying(success);
            });

            ReportAudioSent_AsItHappens(timestamp, triggerEvent, trigger_String, config._channel == LeftRight.Left ? -1 : 1);
        }

        private void TrySendTrigger_Legacy_TS(TriggerOutEvent triggerEvent, string trigger_String, Action<bool> callback)
        {
            AsyncThread.RunOnMainThread_ASAP_TS(() =>
            {
                TrySendTrigger_Legacy_NotTS(triggerEvent, trigger_String, callback);
            });
        }

        private void TrySendTrigger_Legacy_NotTS(TriggerOutEvent triggerEvent, string trigger_String, Action<bool> callback)
        {
            AudioSource aS = SoundSystem.GetAudioSourceTrigger(runtimeConfig.audioTrigger);

            if (aS.isPlaying)
            {
                callback(false);
                return;
            }

            EngineWrapper.StartCoroutine(SendTrigger_Legacy(aS, triggerEvent, trigger_String, config.fs,
                config.channel, config.bitDurationSeconds, config.amplification_Base, config.amplification_Multi_Onset, () =>
                {
                    callback(true);
                }));
        }

        private IEnumerator SendTrigger_Legacy(AudioSource aS, TriggerOutEvent triggerEvent,
            string trigger_String, int fs, int channel, float bitDurationSeconds, float amplification_Base, float amplification_Multi, Action onDone = null)
        {
            float amplification_Onset = amplification_Base * amplification_Multi;

            lastRequest = triggerEvent;
            float[] pulseSignal_Silence = new float[0];
            float[] pulseSignal_Base = new float[0];
            float[] pulseSignal_Amplified = new float[0];

            CreatePulseSignals(fs, bitDurationSeconds, amplification_Base, amplification_Onset, out pulseSignal_Silence, out pulseSignal_Base, out pulseSignal_Amplified);

            // For bits to ACCURATELY be logged separately
            if (config.logEachBitSeparately)
            {
                // they need to be PLAYED separately
                for (int i = 0; i < trigger_String.Length; i++)
                {
                    string trigger_Bit_String = "" + trigger_String[i];
                    aS.clip = TriggerManager_Audio_Helper.CreateAudioTrigger(trigger_Bit_String, fs, TriggerManager_Audio_Helper.ChannelType.Mono, false, pulseSignal_Silence, pulseSignal_Base, pulseSignal_Amplified);
                    aS.Play();
                    ReportAudioSent_NextRenderedFrame(triggerEvent, trigger_Bit_String, aS.panStereo);

                    // Wait while we are still playing
                    // [SOS] Give it a frame to take effect!
                    yield return null;
                    yield return new WaitWhile(() => aS.isPlaying);
                }
            }
            else
            {
                aS.clip = TriggerManager_Audio_Helper.CreateAudioTrigger(trigger_String, fs, TriggerManager_Audio_Helper.ChannelType.Mono, false, pulseSignal_Silence, pulseSignal_Base, pulseSignal_Amplified);
                aS.Play();
                ReportAudioSent_NextRenderedFrame(triggerEvent, trigger_String, aS.panStereo);

                // Wait while we are still playing
                // [SOS] Give it a frame to take effect!
                yield return null;
                yield return new WaitWhile(() => aS.isPlaying);
            }

            onDone?.Invoke();
        }

        private void ReportAudioSent_AsItHappens(TimeWrapper.Timestamp timestamp, TriggerOutEvent triggerEvent, string trigger_String, float panStereo)
        {
            string msg = string.Format("{0};{1};{2};{3}", "TRIGGER_SENT", triggerEvent, trigger_String, panStereo);

            if (config.debug)
                Debug_Helper.LogWarning(typeof(TriggerManager_Audio), msg);

            ExperimentManagerSession.LogData_AsTheyHappen_TS(timestamp, "TRIGGER_MANAGER_AUDIO", msg);
        }

        private void ReportAudioSent_NextRenderedFrame(TriggerOutEvent triggerEvent, string trigger_String, float panStereo)
        {
            string msg = string.Format("{0};{1};{2};{3}", "TRIGGER_SENT", triggerEvent, trigger_String, panStereo);

            if (config.debug)
                Debug_Helper.LogWarning(typeof(TriggerManager_Audio), msg);

            ExperimentManagerSession.LogData_AtNextRenderedFrame_TS("TRIGGER_MANAGER_AUDIO", msg);
        }

        [Serializable]
        public new class Config : TriggerManager.Config
        {
            public string playerType_Comment = "LowLevel = 0, Legacy = 1 | Use LowLevel unless otherwise needed";
            public PlayerType playerType = PlayerType.LowLevel;
            public enum PlayerType { LowLevel = 0, Legacy = 1 }
            public string logEachBitSeparately_Comment = "[Legacy Only] Setting to true will also PLAY them separately, as we need to accurately know the time each bit was played";
            public bool logEachBitSeparately = false;
            public bool oneBitPerFrame = false;

            public string doubleOnsetForLevelStart_Comment = "[Legacy Only]";
            public bool doubleOnsetForLevelStart = true;
            public bool doubleOnsetForLevelEnd = false;

            public bool amplifyOnset = true;

            /// <summary>
            /// Add a "1" in front
            /// Normally the trigger audio channel will be in silence, so the only way to know a trigger has been sent is to break it
            /// Since some codes start with "0" , we just prepend a "1" before the 8 bits, and that "1" is meant to be disregarded.
            /// </summary>
            public string triggerPrefix { get { return (int)(amplifyOnset ? Pulse.Amplified : Pulse.Base) + additionalTriggerPrefixBits; } }
            public string additionalTriggerPrefixBits = "";

            public bool appendTriggerCode = true;
            public int padUntilCertainBitLength = 6;

            public string triggerSuffixBits = "";

            public int fs = 96000;
            public float bitDurationSeconds { get { return bitDurationMS / 1000f; } }
            public int bitDurationMS = 13;
            public float amplification_Base = 1;
            public float amplification_Multi_Onset = 2;

            public string BUFFER_SIZE_COMMENT = "[SOS] Lower values reduce latency but may cause the playback engine and the game to crash. 4096 has been tested and found safe so far with no noticeable latency.";
            public int BUFFER_SIZE = 4096;

            public int channel { get { return (int)_channel; } }

            public string SEND_DUMMY_TRIGGER_COMMENT = "[SOS] This is used with the Low Latency plugin as initialization, because the first audio output has extra latency and hitter. If that's not an issue, you can disable it.";
            public bool SEND_DUMMY_TRIGGER = true;
            public string DUMMY_TRIGGER_COMMENT = "[SOS] Set it to something that is NOT an actual trigger. if it is, game will default to sending " + DEFAULT_DUMMY_TRIGGER;
            public string DUMMY_TRIGGER = DEFAULT_DUMMY_TRIGGER;
            public const string DEFAULT_DUMMY_TRIGGER = "2211001122";

            public LeftRight _channel = LeftRight.Left;

            public bool abortIfPlaying = true;
        }
    }

    public enum Pulse { Silence = 0, Base, Amplified }

    public static class TriggerManager_Audio_Helper
    {
        public enum ChannelType { StereoLeft = 0, StereoRight = 1, StereoBoth = 2, Mono = 3 }

        public static AudioClip CreateAudioTrigger(string trigger_String, int fs, ChannelType channel, bool doStream, float[] pulseSignal_Silence, float[] pulseSignal_Base, float[] pulseSignal_Amplified)
        {
            // dostream needs set to false apparently
            doStream = false;

            Pulse[] trigger_Pulse = Trigger_StringToPulse(trigger_String);

            float[] trigger_AudioData_Channel_Info = Trigger_PulseToAudioData(trigger_Pulse, pulseSignal_Silence, pulseSignal_Base, pulseSignal_Amplified);
            float[] trigger_AudioData_Channel_Zeros = new float[trigger_AudioData_Channel_Info.Length];

            List<float[]> trigger_AudioData_Cumulative = new List<float[]>();

#if UNITY_EDITOR
            // [TEST] 
            // channel = ChannelType.Mono;
#endif

            switch (channel)
            {
                case ChannelType.StereoLeft:
                    trigger_AudioData_Cumulative.Add(trigger_AudioData_Channel_Info);
                    trigger_AudioData_Cumulative.Add(trigger_AudioData_Channel_Zeros);
                    break;
                case ChannelType.StereoRight:
                    trigger_AudioData_Cumulative.Add(trigger_AudioData_Channel_Zeros);
                    trigger_AudioData_Cumulative.Add(trigger_AudioData_Channel_Info);
                    break;
                case ChannelType.StereoBoth:
                    trigger_AudioData_Cumulative.Add(trigger_AudioData_Channel_Info);
                    trigger_AudioData_Cumulative.Add(trigger_AudioData_Channel_Info);
                    break;
                case ChannelType.Mono:
                    trigger_AudioData_Cumulative.Add(trigger_AudioData_Channel_Info);
                    break;
            }

            AudioClip trigger_AudioClip = Trigger_AudioDataToAudioClip(trigger_AudioData_Cumulative, "Trigger_" + trigger_String, fs, doStream);

            return trigger_AudioClip;
        }

        private static AudioClip Trigger_AudioDataToAudioClip(List<float[]> trigger_AudioData_PerChannel, string trigger_Name, int fs, bool doStream)
        {
            // This is per channel BUT it's expected differently ::
            // B :: 0 0 1 0 | 1 0 1 1 will become
            // L :: 0 1 1 1
            // R :: 0 0 0 1
            // So if we wanted
            // L :: 0 0 1 0
            // R :: 1 0 1 1
            // We need this to be
            // B :: 0 1 0 0 1 1 0 1

            Dictionary<int, float> samples = new Dictionary<int, float>();

            int numChannels = trigger_AudioData_PerChannel.Count;
            int maxSamplesInChannel = 0;

            for (int c = 0; c < numChannels; c++)
            {
                int numSamplesInChannel = trigger_AudioData_PerChannel[c].Length;
                maxSamplesInChannel = Mathf.Max(numSamplesInChannel, maxSamplesInChannel);

                for (int s = 0; s < numSamplesInChannel; s++)
                {
                    int index = s * numChannels + c;
                    samples.Add(index, trigger_AudioData_PerChannel[c][s]);
                }
            }

            float[] cumulativeAudioDataArray = new float[maxSamplesInChannel * numChannels];

            foreach (KeyValuePair<int, float> kVP in samples)
                cumulativeAudioDataArray[kVP.Key] = kVP.Value;

            AudioClip trigger_Audio = AudioClip.Create(trigger_Name, cumulativeAudioDataArray.Length, numChannels, fs, doStream);
            trigger_Audio.SetData(cumulativeAudioDataArray, 0);
            return trigger_Audio;
        }

        private static float[] Trigger_PulseToAudioData(Pulse[] trigger_Pulse, float[] pulseSignal_Silence, float[] pulseSignal_Base, float[] pulseSignal_Amplified)
        {
            int numSamplesPerBit = pulseSignal_Silence.Length;
            float[] trigger_AudioData = new float[numSamplesPerBit * trigger_Pulse.Length];
            for (int i = 0; i < trigger_Pulse.Length; i++)
            {
                Pulse trigger_Pulse_i = trigger_Pulse[i];

                float[] signal =
                    trigger_Pulse_i == Pulse.Silence ? pulseSignal_Silence :
                    trigger_Pulse_i == Pulse.Base ? pulseSignal_Base :
                    trigger_Pulse_i == Pulse.Amplified ? pulseSignal_Amplified : null;

                signal.CopyTo(trigger_AudioData, i * numSamplesPerBit);
            }
            return trigger_AudioData;
        }


        private static float[] Trigger_BinaryToAudioData(bool[] trigger_Binary, float[] pulseSignal_Silence, float[] pulseSignal_Base)
        {
            Pulse[] trigger_Pulse = new Pulse[trigger_Binary.Length];
            for (int i = 0; i < trigger_Binary.Length; i++)
                trigger_Pulse[i] = trigger_Binary[i] ? Pulse.Base : Pulse.Silence;
            return Trigger_PulseToAudioData(trigger_Pulse, pulseSignal_Silence, pulseSignal_Base, pulseSignal_Base);
        }


        private static Pulse[] Trigger_StringToPulse(string message_String)
        {
            Pulse[] message_Pulse = new Pulse[message_String.Length];

            // Convert to Binary
            for (int i = 0; i < message_String.Length; i++)
            {
                char c = message_String[i];
                int cValue = int.Parse(c.ToString());
                message_Pulse[i] = (Pulse)cValue;
            }

            return message_Pulse;
        }

        private static bool[] Trigger_StringToBinary(string message_String)
        {
            bool[] message_Binary = new bool[message_String.Length];

            // Convert to Binary
            for (int i = 0; i < message_String.Length; i++)
            {
                char c = message_String[i];
                int cValue = int.Parse(c.ToString());
                message_Binary[i] = cValue > 0;
            }

            return message_Binary;
        }
    }
}