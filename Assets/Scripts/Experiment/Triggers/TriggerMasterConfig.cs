using System;
using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;

namespace Experiment.Triggers
{
    /// <summary>
    /// When adding types add them here but also at <see cref="TriggerMasterConfig.GetTriggerOutInOrderOfExecution"/>
    /// </summary>
    public enum TriggerOutType { Photodiode = 0, Audio = 1, LPT = 2, EyeTracking = 3 }

    [Serializable]
    public class TriggerMasterConfig
    {
        public bool debug = false;

        public string WARNING_0 = "IMPORTANT :: If set to true, TESTING_ONLY_LOW_FPS will set the game to 1 frame per second. Use only to verify trigger behavior w.r.t. framerate. Disable when playing with subjects.";
        public bool TESTING_ONLY_LOW_FPS = false;

        public TriggerEventConfig MASTER_MEEG = new TriggerEventConfig("Master Control for all events in the MEEG setup. Enabled to false disables all triggers. do Audio/LPT/Photodiode to false disables all Audio/LPT/Photodiode events.");
        public TriggerEventConfig MASTER_ECOG = new TriggerEventConfig("Master Control for all events in the ECOG setup. Enabled to false disables all triggers. do Audio/LPT/Photodiode to false disables all Audio/LPT/Photodiode events.");
        public TriggerEventConfig MASTER_FMRI = new TriggerEventConfig("Master Control for all events in the FMRI setup. Enabled to false disables all triggers. do Audio/LPT/Photodiode to false disables all Audio/LPT/Photodiode events.");
        public TriggerEventConfig MASTER_MEEG_Preparation = new TriggerEventConfig("Master Control for all events in the MEEG Preparation setup. Enabled to false disables all triggers. do Audio/LPT/Photodiode to false disables all Audio/LPT/Photodiode events.", false, false, false, false, false);
        public TriggerEventConfig MASTER_FMRI_Preparation = new TriggerEventConfig("Master Control for all events in the FMRI Preparation setup. Enabled to false disables all triggers. do Audio/LPT/Photodiode to false disables all Audio/LPT/Photodiode events.", false, false, false, false, false);

        public TriggerEventConfig levelBegin = new TriggerEventConfig();
        public TriggerEventConfig levelEnd = new TriggerEventConfig();

        public TriggerEventConfig gameStimulusOn = new TriggerEventConfig();
        public TriggerEventConfig gameBlankOn = new TriggerEventConfig();
        public TriggerEventConfig gameFillerOn = new TriggerEventConfig();

        public TriggerEventConfig gameProbeOn = new TriggerEventConfig();
        public TriggerEventConfig gameResponse = new TriggerEventConfig();

        public TriggerEventConfig replayTaskOn = new TriggerEventConfig();
        public TriggerEventConfig replayNonTaskOn = new TriggerEventConfig();
        public TriggerEventConfig replayFillerOn = new TriggerEventConfig();

        public TriggerEventConfig replayResponse = new TriggerEventConfig();
        public TriggerEventConfig replayWindowOver = new TriggerEventConfig();

        public TriggerEventConfig animationPeakEnd = new TriggerEventConfig();

        public int orderOfExecution_Photodiode = 0;
        public int orderOfExecution_Audio = 1;
        public int orderOfExecution_LPT = 2;
        public int orderOfExecution_EyeTracking = 3;

        #region Methods
        /// <summary>
        /// Guarantees to give each event exactly once
        /// </summary>
        /// <returns></returns>
        public List<TriggerOutType> GetTriggerOutInOrderOfExecution()
        {
            List<TriggerOutType> orderOfExecution = new List<TriggerOutType>();
            List<KeyValuePair<TriggerOutType, int>> orderOfExecution_Priorities = new List<KeyValuePair<TriggerOutType, int>>()
            {
                new KeyValuePair<TriggerOutType, int>(TriggerOutType.Photodiode, orderOfExecution_Photodiode),
                new KeyValuePair<TriggerOutType, int>(TriggerOutType.Audio, orderOfExecution_Audio),
                new KeyValuePair<TriggerOutType, int>(TriggerOutType.LPT, orderOfExecution_LPT),
                new KeyValuePair<TriggerOutType, int>(TriggerOutType.EyeTracking, orderOfExecution_EyeTracking),
            };

            foreach (KeyValuePair<TriggerOutType, int> kVP in orderOfExecution_Priorities.CustomOrderBy(x => x.Value, Order.Ascending))
            {
                this.LogWarning("Added {0} with Prio {1}"._Format(kVP.Key, kVP.Value));
                orderOfExecution.Add(kVP.Key);
            }

            return orderOfExecution;
        }

        public bool IsEventEnabled(TriggerOutEvent triggerEvent)
        {
            if (GetEventConfig(triggerEvent)?.enabled != true)
            {
                if (debug)
                    Debug.LogWarning("Event " + triggerEvent + " disabled. Aborting!");
                return false;
            }

            return true;
        }

        public bool IsEventEnabled(ModuleType systemType)
        {
            if (GetEventConfig(systemType)?.enabled != true)
            {
                if (debug)
                    Debug.LogWarning("All events disabled for System Type :: " + systemType + ". Aborting!");
                return false;
            }

            return true;
        }

        public TriggerEventConfig GetEventConfig(ModuleType systemType)
        {
            return GetEventConfig(
                systemType == ModuleType.MEEG ? TriggerOutEvent.MASTER_MEEG :
                systemType == ModuleType.ECOG ? TriggerOutEvent.MASTER_ECOG :
                systemType == ModuleType.FMRI_Scanner ? TriggerOutEvent.MASTER_FMRI : 

                systemType == ModuleType.MEEG_Preparation ? TriggerOutEvent.MASTER_MEEG_PREP :
                systemType == ModuleType.FMRI_Preparation ? TriggerOutEvent.MASTER_FMRI_PREP :

                systemType == ModuleType.MEEG_Screening ? TriggerOutEvent.MASTER_MEEG_PREP :
                systemType == ModuleType.FMRI_Screening ? TriggerOutEvent.MASTER_FMRI_PREP : default(TriggerOutEvent));
        }

        public TriggerEventConfig GetEventConfig(TriggerOutEvent triggerEvent)
        {
            switch (triggerEvent)
            {
                case TriggerOutEvent.MASTER_MEEG:
                    return MASTER_MEEG;
                case TriggerOutEvent.MASTER_MEEG_PREP:
                    return MASTER_MEEG_Preparation;
                case TriggerOutEvent.MASTER_ECOG:
                    return MASTER_ECOG;
                case TriggerOutEvent.MASTER_FMRI:
                    return MASTER_FMRI;
                case TriggerOutEvent.MASTER_FMRI_PREP:
                    return MASTER_FMRI_Preparation;
                case TriggerOutEvent.LevelBegin:
                    return levelBegin;
                case TriggerOutEvent.LevelEnd:
                    return levelEnd;
                case TriggerOutEvent.GameStimulusOn:
                    return gameStimulusOn;
                case TriggerOutEvent.GameBlankOn:
                    return gameBlankOn;
                case TriggerOutEvent.GameFillerOn:
                    return gameFillerOn;
                case TriggerOutEvent.GameProbeOn:
                    return gameProbeOn;
                case TriggerOutEvent.GameResponse:
                    return gameResponse;
                case TriggerOutEvent.ReplayTaskOn:
                    return replayTaskOn;
                case TriggerOutEvent.ReplayNonTaskOn:
                    return replayNonTaskOn;
                case TriggerOutEvent.ReplayFillerOn:
                    return replayFillerOn;
                case TriggerOutEvent.ReplayResponse:
                    return replayResponse;
                case TriggerOutEvent.ReplayWindowOver:
                    return replayWindowOver;
                case TriggerOutEvent.AnimationPeakEnd:
                    return animationPeakEnd;
            }

            return null;
        }
        #endregion
    }

    [Serializable]
    public class TriggerEventConfig
    {
        public string description = "";

        public bool enabled = true;

        public bool doAudio = true;
        public bool doLPT = true;
        public bool doPhotodiode = true;
        public bool doEyeTracking = true;

        public TriggerEventConfig()
        {
        }

        public TriggerEventConfig(string description)
        {
            this.description = description;
        }

        public TriggerEventConfig(string description, bool enabled, bool doAudio, bool doLPT, bool doPhotodiode, bool doEyeTracking) : this(description)
        {
            this.enabled = enabled;
            this.doAudio = doAudio;
            this.doLPT = doLPT;
            this.doPhotodiode = doPhotodiode;
            this.doEyeTracking = doEyeTracking;
        }
    }
}