// NS_Maybe reposition
using Experiment.Task.Core;

using Experiment.Stimulus;
using Experiment.Triggers.Codes;
using Experiment.Triggers.Core;
using Helpers.Engine;
using Peripherals.Audio;
using System;
using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;
using Experiment.Managers;
using Peripherals.EyeTracking;

namespace Experiment.Triggers
{
    /// <summary>
    /// Remove 
    /// </summary>
    public class TriggerMaster
    {
        private object nonLevelEndLockedLock = new object();
        private bool nonLevelEndLocked = false;

        private ITriggerCodes codesEyeTracking { get { return config.triggerCodes_E; } }
        private ITriggerCodes codesLPT { get { return config.triggerCodes_L; } }
        private ITriggerCodes codesAudio { get { return config.triggerCodes_A; } }
        private ITriggerCodes codesPhotodiode { get { return config.triggerCodes_P; } }
                
        private Config config;
        private RuntimeConfig runtimeConfig;
                
        private readonly Dictionary<ModuleType, bool> systemEnabled_TS = new Dictionary<ModuleType, bool>();
        private readonly Dictionary<TriggerOutEvent, bool> eventEnabled_TS = new Dictionary<TriggerOutEvent, bool>();
                
        private readonly Dictionary<TaskIrrelevantResponse, Dictionary<TriggerOutType, CodePrio>>
            codes_ResponseEventGame_TS = new Dictionary<TaskIrrelevantResponse, Dictionary<TriggerOutType, CodePrio>>();

        private readonly Dictionary<bool, Dictionary<TriggerOutType, CodePrio>>
            codes_ResponseEventReplay_TS = new Dictionary<bool, Dictionary<TriggerOutType, CodePrio>>();

        private TriggerManager_Photodiode triggerManager_Photodiode;
        private TriggerManager_LPT triggerManager_LPT;
        private TriggerManager_EyeTracking triggerManager_EyeTracking;
        private TriggerManager_Audio triggerManager_Audio;
        private bool isMuted = false;
        private Func<TriggerOutEvent, string> checkSend_TS;

        public class TriggerEventArgs : EventArgs
        {
            public TriggerOutEvent type;
            public CodePrio codePrio;

            public TriggerEventArgs(TriggerOutEvent type, CodePrio codePrio)
            {
                this.type = type;
                this.codePrio = codePrio;
            }
        }

        private List<TriggerOutType> orderedTriggerOut_TS;

        /// <summary>
        /// [SOS] The subscriber to this event has the responsibility of being Thread-Safe ; the event may be fired by a Thread
        /// </summary>
        // public  EventHandler<TriggerEventArgs> onTrigger_Thread;
        private void RaiseOnTrigger_Thread(TriggerOutEvent triggerEvent, Dictionary<TriggerOutType, CodePrio> codes)
        {
            TimeWrapper.Timestamp timestamp = TimeWrapper.GetCurrentTimestamp_TS();

            foreach (TriggerOutType tOT in orderedTriggerOut_TS)
            {
                CodePrio codePrio = codes[tOT];
                TriggerEventArgs e = new TriggerEventArgs(triggerEvent, codePrio);

                switch (tOT)
                {
                    case TriggerOutType.Photodiode:
                        triggerManager_Photodiode?.Send_NextFrame_TS(e);
                        break;
                    case TriggerOutType.Audio:
                        triggerManager_Audio?.SendTrigger_TS(e);
                        break;
                    case TriggerOutType.LPT:
                        triggerManager_LPT?.Send_Now_TS(e);
                        break;
                    case TriggerOutType.EyeTracking:
                        triggerManager_EyeTracking?.Send_Now_TS(e);
                        break;
                }
            }
            ExperimentManagerSession.LogData_AsTheyHappen_TS(timestamp, "TRIGGER_MANAGER_CENTRAL",
                "{0};{1}"._Format("TRIGGER_REQUESTED_EXTRA_THREAD", triggerEvent));
        }

        /// <summary>
        /// The subscriber to this event may or may not be Thread-Safe, the event is fired from the Main thread
        /// </summary>
        // public  EventHandler<TriggerEventArgs> onTrigger_Main;
        private  void RaiseOnTrigger_Main(TriggerOutEvent triggerEvent, Dictionary<TriggerOutType, CodePrio> codes)
        {
            TimeWrapper.Timestamp timestamp = TimeWrapper.GetCurrentTimestamp_TS();

            foreach (TriggerOutType tOT in orderedTriggerOut_TS)
            {
                CodePrio code = codes[tOT];
                TriggerEventArgs e = new TriggerEventArgs(triggerEvent, code);

                switch (tOT)
                {
                    case TriggerOutType.Photodiode:
                        triggerManager_Photodiode?.Send_NextFrame_NotTS(e);
                        break;
                    case TriggerOutType.Audio:
                        triggerManager_Audio?.SendTrigger_TS(e);
                        break;
                    case TriggerOutType.LPT:
                        triggerManager_LPT?.Send_NextFrame_TS(e);
                        break;
                    case TriggerOutType.EyeTracking:
                        triggerManager_EyeTracking?.Send_NextFrame_TS(e);
                        break;
                }
            }

            ExperimentManagerSession.LogData_AsTheyHappen_TS(timestamp, "TRIGGER_MANAGER_CENTRAL",
                "{0};{1}"._Format("TRIGGER_REQUESTED_MAIN_THREAD", triggerEvent));
        }

        private  ITriggerCodes GetCodeConfig(TriggerOutType triggerOutType)
        {
            switch (triggerOutType)
            {
                case TriggerOutType.Photodiode:
                    return codesPhotodiode;
                case TriggerOutType.Audio:
                    return codesAudio;
                case TriggerOutType.LPT:
                    return codesLPT;
                case TriggerOutType.EyeTracking:
                    return codesEyeTracking;
            }

            return null;
        }

        [Serializable]
        public class Config : TriggerMasterConfig
        {
            public TriggerCodes_A triggerCodes_A;
            public TriggerCodes_P triggerCodes_P;
            public TriggerCodes_L triggerCodes_L;
            public TriggerCodes_E triggerCodes_E;

            public TriggerManager_Photodiode.Config triggerManagerPhotodiode;
            public TriggerManager_Audio.Config triggerManagerAudio;
            public TriggerManager_LPT.Config triggerManagerLPT;
            public TriggerManager_EyeTracking.Config triggerManagerEyeTracking;
            public bool animationPeakEnd_PreventForFillers = true;
            // public string numFramesDelayLevelBegin_Comment = "Delays the frame begin trigger to avoid any game-start frame hiccups";
            // public int numFramesDelayLevelBegin = 2;
        }

        public class RuntimeConfig
        {
            public ModuleType module;
            public SoundSystem.RuntimeAudioSourceConfig audioTrigger;
            public EyeTrackerManager_Base eyeTracker;

            public RuntimeConfig(ModuleType module, SoundSystem.RuntimeAudioSourceConfig audioTrigger, EyeTrackerManager_Base eyeTracker)
            {
                this.module = module;
                this.audioTrigger = audioTrigger;
                this.eyeTracker = eyeTracker;
            }
        }

        public TriggerMaster(Config config, RuntimeConfig runtimeConfig)
        {
            this.config = config;
            this.runtimeConfig = runtimeConfig;

            SetupTSListsAndDictionaries_NotTS();

            TriggerEventConfig eventConfig = config.GetEventConfig(runtimeConfig.module);

            if (!eventConfig.enabled)
            {
                this.LogWarning("All events disabled! Won't have any triggers");
                return;
            }

            if (eventConfig.doPhotodiode)
            {
                triggerManager_Photodiode = new TriggerManager_Photodiode(config.triggerManagerPhotodiode,
                    new TriggerManager_Photodiode.RuntimeConfig(DoTrigger_Photodiode));
                this.LogWarning("Photodiode enabled");
            }
            else
                this.LogWarning("Photodiode disabled");

            if (eventConfig.doAudio)
            {
                triggerManager_Audio = new TriggerManager_Audio(config.triggerManagerAudio,
                    new TriggerManager_Audio.RuntimeConfig(DoTrigger_Audio, runtimeConfig.audioTrigger));
                this.LogWarning("Audio enabled");
            }
            else
                this.LogWarning("Audio disabled");

            if (eventConfig.doLPT)
            {
                triggerManager_LPT = new TriggerManager_LPT(config.triggerManagerLPT,
                    new TriggerManager_LPT.RuntimeConfig(DoTrigger_LPT));
                this.LogWarning("LPT enabled");
            }
            else
                this.LogWarning("LPT disabled");

            if (eventConfig.doEyeTracking)
            {
                triggerManager_EyeTracking = new TriggerManager_EyeTracking(config.triggerManagerEyeTracking,
                    new TriggerManager_EyeTracking.RuntimeConfig(DoTrigger_EyeTracking, runtimeConfig.eyeTracker));
                this.LogWarning("EyeTracking enabled");
            }
            else
                this.LogWarning("EyeTracking disabled");
        }

        /// <summary>
        /// [SOS] Thread-Safe BUT triggers will be processed on the next frame. Used for High Accu (response events).
        /// Otherwise use <see cref="TryFireTrigger_NotTS"/>
        /// </summary>
        /// <param name="triggerEvent"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        private bool TryFireTrigger_TS(TriggerOutEvent triggerEvent, Dictionary<TriggerOutType, CodePrio> codes)
        {
            if (!DoChecks_TS(triggerEvent))
                return false;

            // Debug.Log("TH A1 " + TimeWrapper.currentTimestampMS);
            RaiseOnTrigger_Thread(triggerEvent, codes);

            return true;
        }

        private  bool TryFireTrigger_NotTS(TriggerOutEvent triggerEvent, Dictionary<TriggerOutType, CodePrio> codes)
        {
            if (!DoChecks_TS(triggerEvent))
                return false;

            // Debug.Log("TH A2 " + TimeWrapper.currentTimestampMS);
            RaiseOnTrigger_Main(triggerEvent, codes);

            return true;
        }

        private bool DoChecks_TS(TriggerOutEvent triggerEvent)
        {
            TimeWrapper.Timestamp timestamp = TimeWrapper.GetCurrentTimestamp_TS();
            // Debug.Log(TimeWrapper.currentTimestampMS + " TRIGGER_HELPER");

            if (isMuted)
            {
                ExperimentManagerSession.LogData_AsTheyHappen_TS(timestamp, "TRIGGER_MANAGER_CENTRAL",
                    "{0};{1};{2}"._Format("TRIGGER_DISABLED", triggerEvent, "IS_MUTED"));
                return false;
            }

            if (nonLevelEndLocked && triggerEvent != TriggerOutEvent.LevelEnd)
            {
                ExperimentManagerSession.LogData_AsTheyHappen_TS(timestamp, "TRIGGER_MANAGER_CENTRAL",
                    "{0};{1};{2}"._Format("TRIGGER_DISABLED", triggerEvent, "NON_LEVEL_END_LOCKED"));
                return false;
            }

            string reason = checkSend_TS?.Invoke(triggerEvent);
            if (reason != null)
            {
                ExperimentManagerSession.LogData_AsTheyHappen_TS(timestamp, "TRIGGER_MANAGER_CENTRAL",
                    "{0};{1};{2};{3}"._Format("TRIGGER_DISABLED", triggerEvent, "CHECK_SEND_FALSE", reason));
                return false;
            }

            if (!IsEventEnabled_TS(ExperimentManagerSession.module))
            {
                ExperimentManagerSession.LogData_AsTheyHappen_TS(timestamp, "TRIGGER_MANAGER_CENTRAL",
                    "{0};{1};{2}"._Format("TRIGGER_DISABLED", triggerEvent, "SYSTEM_NO_TRIGGERS"));
                return false;
            }

            if (!IsEventEnabled_TS(triggerEvent))
            {
                ExperimentManagerSession.LogData_AsTheyHappen_TS(timestamp, "TRIGGER_MANAGER_CENTRAL",
                    "{0};{1};{2}"._Format("TRIGGER_DISABLED", triggerEvent, "EVENT_DISABLED_MASTER"));
                return false;
            }

            return true;
        }

        public void ToggleMuteEvents(bool doMute)
        {
            // Debug.LogError(doMute);
            isMuted = doMute;
        }

        public void SendLevelBeginEvent(bool isGame)
        {
            // Delay level send by one frame to avoid it being slowed down by the clutter of level begin
            // AsyncThread.RunOnMainThread_Delayed_TS(() =>
            {
                if (config.TESTING_ONLY_LOW_FPS)
                {
                    QualitySettings.vSyncCount = 0;
                    Application.targetFrameRate = 1;
                    Debug.LogWarning("Set FPS to 1, to test trigger behavior");
                }

                Dictionary<TriggerOutType, CodePrio> codes = new Dictionary<TriggerOutType, CodePrio>();

                foreach (TriggerOutType tOT in orderedTriggerOut_TS)
                    codes.Add(tOT, GetCodeConfig(tOT).GetTriggerCodeLevelBeginEvent(isGame));

                TryFireTrigger_NotTS(TriggerOutEvent.LevelBegin, codes);
            }//, config.numFramesDelayLevelBegin);
        }

        public  void SendLevelEndEvent(bool isGame)
        {
            Dictionary<TriggerOutType, CodePrio> codes = new Dictionary<TriggerOutType, CodePrio>();

            foreach (TriggerOutType tOT in orderedTriggerOut_TS)
                codes.Add(tOT, GetCodeConfig(tOT).GetTriggerCodeLevelEndEvent(isGame));

            TryFireTrigger_NotTS(TriggerOutEvent.LevelEnd, codes);
        }

        public  void SendStimulusEventGame(StimulusType type, string stimulusName, Direction_2D_Diagonal direction, bool isProbeTrial)
        {
            Dictionary<TriggerOutType, CodePrio> codes = new Dictionary<TriggerOutType, CodePrio>();

            foreach (TriggerOutType tOT in orderedTriggerOut_TS)
                codes.Add(tOT, GetCodeConfig(tOT).GetTriggerCodeStimulusEventGame(type, stimulusName, direction, isProbeTrial));

            TriggerOutEvent triggerEvent = type == StimulusType.None ? TriggerOutEvent.GameBlankOn : TriggerOutEvent.GameStimulusOn;

            TryFireTrigger_NotTS(triggerEvent, codes);
        }

        public  void SendStimulusEventReplay(StimulusType type, string stimulusName, StimulusType targetStimulus, Direction_2D_Diagonal direction)
        {
            Dictionary<TriggerOutType, CodePrio> codes = new Dictionary<TriggerOutType, CodePrio>();

            foreach (TriggerOutType tOT in orderedTriggerOut_TS)
                codes.Add(tOT, GetCodeConfig(tOT).GetStimulusEventReplay(type, stimulusName, targetStimulus, direction, false));

            TriggerOutEvent triggerEvent = type == targetStimulus ? TriggerOutEvent.ReplayTaskOn : TriggerOutEvent.ReplayNonTaskOn;

            TryFireTrigger_NotTS(triggerEvent, codes);
        }

        public  void SendGameFillerEvent()
        {
            Dictionary<TriggerOutType, CodePrio> codes = new Dictionary<TriggerOutType, CodePrio>();

            foreach (TriggerOutType tOT in orderedTriggerOut_TS)
                codes.Add(tOT, GetCodeConfig(tOT).GetGameFillerCode());

            TryFireTrigger_NotTS(TriggerOutEvent.GameFillerOn, codes);
        }

        public  void SendLocalizerFillerEvent()
        {
            Dictionary<TriggerOutType, CodePrio> codes = new Dictionary<TriggerOutType, CodePrio>();

            foreach (TriggerOutType tOT in orderedTriggerOut_TS)
                codes.Add(tOT, GetCodeConfig(tOT).GetLocalizerFillerCode());

            TryFireTrigger_NotTS(TriggerOutEvent.ReplayFillerOn, codes);
        }

        public  void SendAnimationPeakEndEvent(bool isGame, bool isStimulus)
        {
            if (!isStimulus && config.animationPeakEnd_PreventForFillers)
            {
                ExperimentManagerSession.LogData_AsTheyHappen_TS(TimeWrapper.GetCurrentTimestamp_TS(), "TRIGGER_MANAGER_CENTRAL",
                    "TRIGGER_NOT_PROCESSED_FILLER_ANIM_PEAK_END");

                return;
            }

            Dictionary<TriggerOutType, CodePrio> codes = new Dictionary<TriggerOutType, CodePrio>();

            foreach (TriggerOutType tOT in orderedTriggerOut_TS)
                codes.Add(tOT, GetCodeConfig(tOT).GetAnimationPeakEndCode(isGame));

            TryFireTrigger_NotTS(TriggerOutEvent.AnimationPeakEnd, codes);
        }

        public  void SendProbeEvent()
        {
            Dictionary<TriggerOutType, CodePrio> codes = new Dictionary<TriggerOutType, CodePrio>();

            foreach (TriggerOutType tOT in orderedTriggerOut_TS)
                codes.Add(tOT, GetCodeConfig(tOT).GetProbeOnsetCode());

            TryFireTrigger_NotTS(TriggerOutEvent.GameProbeOn, codes);
        }

        /// [SOS] must use <see cref="TryFireTrigger_TS(TriggerOutEvent, Dictionary{TriggerOutType, CodePrio})"/> (not the _NotTS variant)
        public  void SendResponseEventGame_TS(TaskIrrelevantResponse answer)
        {
            TryFireTrigger_TS(TriggerOutEvent.GameResponse, codes_ResponseEventGame_TS[answer]);
        }

        /// [SOS] must use <see cref="TryFireTrigger_TS(TriggerOutEvent, Dictionary{TriggerOutType, CodePrio})"/> (not the _NotTS variant)
        public  void SendResponseEventReplay_TS(bool reportedSomething)
        {
            TriggerOutEvent triggerEvent = reportedSomething ? TriggerOutEvent.ReplayResponse : TriggerOutEvent.ReplayWindowOver;

            TryFireTrigger_TS(triggerEvent, codes_ResponseEventReplay_TS[reportedSomething]);
        }

        private  bool IsEventEnabled_TS(ModuleType sT)
        {
            if (!systemEnabled_TS.ContainsKey(sT)) return false;

            return systemEnabled_TS[sT];
        }

        private  bool IsEventEnabled_TS(TriggerOutEvent tO)
        {
            if (!eventEnabled_TS.ContainsKey(tO)) return false;

            return eventEnabled_TS[tO];
        }

        private  void SetupTSListsAndDictionaries_NotTS()
        {
            orderedTriggerOut_TS = config.GetTriggerOutInOrderOfExecution();

            systemEnabled_TS.Clear();
            eventEnabled_TS.Clear();
            codes_ResponseEventGame_TS.Clear();
            codes_ResponseEventReplay_TS.Clear();

            foreach (ModuleType sT in Utility_Helper.EnumGetValues<ModuleType>())
                systemEnabled_TS.Add(sT, config.IsEventEnabled(sT));
            foreach (TriggerOutEvent tO in Utility_Helper.EnumGetValues<TriggerOutEvent>())
                eventEnabled_TS.Add(tO, config.IsEventEnabled(tO));

            foreach (TaskIrrelevantResponse tIR in Utility_Helper.EnumGetValues<TaskIrrelevantResponse>())
            {
                codes_ResponseEventGame_TS.Add(tIR, new Dictionary<TriggerOutType, CodePrio>());

                foreach (TriggerOutType tOT in orderedTriggerOut_TS)
                    codes_ResponseEventGame_TS[tIR].Add(tOT, GetCodeConfig(tOT).GetResponseEventGame(tIR));
            }

            List<bool> keys = new List<bool>() { false, true };
            foreach (bool b in keys)
            {
                codes_ResponseEventReplay_TS.Add(b, new Dictionary<TriggerOutType, CodePrio>());

                foreach (TriggerOutType tOT in orderedTriggerOut_TS)
                    codes_ResponseEventReplay_TS[b].Add(tOT, GetCodeConfig(tOT).GetReplayResponse(b));
            }
        }

        private bool DoTrigger_Photodiode(TriggerOutEvent triggerOutEvent)
        {
            bool goForModule = config.GetEventConfig(ExperimentManagerSession.module)?.doPhotodiode == true;
            bool goForEvent = config.GetEventConfig(triggerOutEvent)?.doPhotodiode == true;
            return goForModule && goForEvent;
        }

        private bool DoTrigger_Audio(TriggerOutEvent triggerOutEvent)
        {
            bool goForModule = config.GetEventConfig(ExperimentManagerSession.module)?.doAudio == true;
            bool goForEvent = config.GetEventConfig(triggerOutEvent)?.doAudio == true;
            return goForModule && goForEvent;
        }

        private bool DoTrigger_LPT(TriggerOutEvent triggerOutEvent)
        {
            bool goForModule = config.GetEventConfig(ExperimentManagerSession.module)?.doLPT == true;
            bool goForEvent = config.GetEventConfig(triggerOutEvent)?.doLPT == true;
            return goForModule && goForEvent;
        }

        private bool DoTrigger_EyeTracking(TriggerOutEvent triggerOutEvent)
        {
            bool goForModule = config.GetEventConfig(ExperimentManagerSession.module)?.doEyeTracking == true;
            bool goForEvent = config.GetEventConfig(triggerOutEvent)?.doEyeTracking == true;
            return goForModule && goForEvent;
        }

        public void DeInitialize()
        {
            triggerManager_Audio?.Dispose();
            triggerManager_Photodiode?.Dispose();
            triggerManager_LPT?.Dispose();
            triggerManager_EyeTracking?.Dispose();
        }

        internal void SetCheckSend(Func<TriggerOutEvent, string> checkSend_TS)
        {
            this.checkSend_TS = checkSend_TS;
        }

        internal void LockNonLevelEndTriggers(bool v)
        {
            lock (nonLevelEndLockedLock)
                nonLevelEndLocked = v;
        }
    }

    public enum TriggerOutEvent
    {
        MASTER_MEEG,
        MASTER_MEEG_PREP,
        MASTER_ECOG,
        MASTER_FMRI,
        MASTER_FMRI_PREP,
        LevelBegin,
        LevelEnd,
        GameStimulusOn,
        GameBlankOn,
        GameFillerOn,
        GameProbeOn,
        GameResponse,
        ReplayTaskOn,
        ReplayNonTaskOn,
        ReplayFillerOn,
        ReplayResponse,
        ReplayWindowOver,
        /// <summary>
        /// Fired after EVERY game stim on / blank on / filler on / etc (corresponds to HIDE)
        /// </summary>
        AnimationPeakEnd
    }

}