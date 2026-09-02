// NS_DEBATABLE | Shouldnt connect to test either
using Peripherals.Logging.Test;

// NS_DEBATABLE
using Peripherals.UserInput;

using System;
using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;
using Peripherals.UserInput.Normal;
using Peripherals.UserInput.Core;
using Peripherals.Audio;
using Peripherals.EyeTracking;
using Peripherals.EyeTracking.EyeLink;
using Peripherals.EyeTracking.Tobii;
using Game.Managers.SessionManagers;
using Game.Systems.Bridges;
using Game.Managers.LevelManagers;
using Game.Systems.Adaptive;
using Experiment.Triggers;
using Experiment.Stimulus;
using Experiment.Subject;
using Game.Managers.SessionManagers.UI;
using Game.Managers.GameplayManager.UI;
using Experiment.Managers.Core;
using Experiment.Managers;
using Experiment.UI;
using Experiment.Task.UI;
using Helpers.Engine;

namespace Experiment.Library.Core
{
    [Serializable]
    public class ExperimentConfig
    {
        public string ApplicationTitle = "Seattle Project";
        public string ApplicationSubTitle = "An inattentive blindness experiment";
        public string LabCode = "S1";
        public string module_Comment = "MEEG = 0, ECOG = 1, FMRI = 2";
        /// <summary>
        /// [SOS] Should only be accessed once from <see cref="ExperimentManagerApplication"/>'s initialization to be able to manipulate afterwards
        /// </summary>
        public ModuleType module = ModuleType.MEEG;
        
        [HideInInspector]
        public string PostLevelCleanupSafety_Comment = "After finishing a level, wait til all processes end to stop tracking and return to level selection";
        public float PostLevelCleanupSafety { get { return PostLevelCleanupSafetyMS / 1000f; } }
        public int PostLevelCleanupSafetyMS = 500;
        public string PostLevelLoadupSafety_Comment = "After loading a, wait til all processes end to stop tracking and then initialize managers";
        public float PostLevelLoadupSafety { get { return PostLevelLoadupSafetyMS / 1000f; } }
        public int PostLevelLoadupSafetyMS = 500;
        public int maxNumThreads = 100;

        public SeattleInputConfig Input;

        public SeattleInputKeysConfig_Serial Get_InputSerialPort_ResponseBox(HandType handType)
        {
            switch (handType)
            {
                case HandType.Both:
                default:
                    return InputSerialPort_ResponseBox_TwoHands;
                case HandType.Left:
                    return InputSerialPort_ResponseBox_OneHandL;
                case HandType.Right:
                    return InputSerialPort_ResponseBox_OneHandR;
            }
        }

        public SeattleInputKeysConfig_KeyCode Get_InputKeyCode_Keyboard(HandType handType)
        {
            switch (handType)
            {
                case HandType.Both:
                default:
                    return InputKeyCode_Keyboard_TwoHands;
                case HandType.Left:
                    return InputKeyCode_Keyboard_OneHandL;
                case HandType.Right:
                    return InputKeyCode_Keyboard_OneHandR;
            }
        }

        public SeattleInputKeysConfig_KeyCode Get_InputKeyCode_ResponseBox(HandType handType)
        {
            switch (handType)
            {
                case HandType.Both:
                default:
                    return InputKeyCode_ResponseBox_TwoHands;
                case HandType.Left:
                    return InputKeyCode_ResponseBox_OneHandL;
                case HandType.Right:
                    return InputKeyCode_ResponseBox_OneHandR;
            }
        }

        [SerializeField] private SeattleInputKeysConfig_Serial InputSerialPort_ResponseBox_TwoHands = new SeattleInputKeysConfig_Serial(true);
        [SerializeField] private SeattleInputKeysConfig_Serial InputSerialPort_ResponseBox_OneHandL = new SeattleInputKeysConfig_Serial(true);
        [SerializeField] private SeattleInputKeysConfig_Serial InputSerialPort_ResponseBox_OneHandR = new SeattleInputKeysConfig_Serial(true);
        [SerializeField] private SeattleInputKeysConfig_KeyCode InputKeyCode_Keyboard_TwoHands = new SeattleInputKeysConfig_KeyCode(false);
        [SerializeField] private SeattleInputKeysConfig_KeyCode InputKeyCode_Keyboard_OneHandL = new SeattleInputKeysConfig_KeyCode(false);
        [SerializeField] private SeattleInputKeysConfig_KeyCode InputKeyCode_Keyboard_OneHandR = new SeattleInputKeysConfig_KeyCode(false);
        [SerializeField] private SeattleInputKeysConfig_KeyCode InputKeyCode_ResponseBox_TwoHands = new SeattleInputKeysConfig_KeyCode(true);
        [SerializeField] private SeattleInputKeysConfig_KeyCode InputKeyCode_ResponseBox_OneHandL = new SeattleInputKeysConfig_KeyCode(true);
        [SerializeField] private SeattleInputKeysConfig_KeyCode InputKeyCode_ResponseBox_OneHandR = new SeattleInputKeysConfig_KeyCode(true);

        public SeattleEyeTrackingConfig EyeTracking;
        public SeattleEyelinkConfig EyeLink;
        public SeattleTobiiConfig Tobii;

        public SeattleAudioConfig Audio;
        // NS_RENAME Tracker
        public ExperimentManagerSessionConfig Logging;
        public SeattleLoggingTesterConfig LoggingTesting;
        public ReportConfig Report;
        public ProbeConfig Probes;
        public SeattleSubjectConfig Subject;
        public SeattleDifficultyConfig Difficulty;
        public MoneyConfig Money;
        public UIConfig UI;
        public TextsConfig Texts;
        public SeattlePlayerProgressionConfig PlayerProgression;
        public LeaderboardUI.Config Leaderboard;
        public NarrationConfig Narration;
        public SeattleTriggerOutConfig TriggerOut;
        public TriggerInKeyCodeConfig TriggerInKeyCode;
        public SeattleTriggerInSerialPortConfig TriggerInSerialPort;
        public FMRIConfig FMRI;
        public MEEGConfig MEEG;
        public LevelProgression LevelProgression;
        public SeattleExperimentConfig Experiment;
        public PerformanceObserver.Config PerformanceObserver;

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Error Messages</returns>
        public List<string> CheckForConstraints()
        {
            List<string> errorMessages = new List<string>();

            // Player reaction window (report stimuli for localizers) must be less that background cycles
            if (Report.reactionWindowMS / 1000f >= GetPeriodMinMax_Stimulus(true, true).min)
                errorMessages.Add("Config.Report.reactionWindowMS must be less than GetPeriodMinMax_Stimulus min for FMRI! \nThis will lead in wrong reported data");

            if (Report.reactionWindowMS / 1000f >= GetPeriodMinMax_Stimulus(true, false).min)
                errorMessages.Add("Config.Report.reactionWindowMS must be less than GetPeriodMinMax_Stimulus min for non-FMRI! \nThis will lead in wrong reported data");

            return errorMessages;
        }

        /// <summary>
        /// [DEPRECATED, 200509]
        /// </summary>
        // [NonSerialized] public GameManagerDuetConfig Duet = new GameManagerDuetConfig();

        public bool showTime = true;

        public string runInBackground_Comment = "Should the application run while alt-tabbed?";
        public bool runInBackground = true;
        public bool allowTimeScaleOutsideDebug = false;
        public bool showCalibrationButton = false;
        public int maxNumTweens = 800;
        public bool getSequenceFromFile;
        public bool deInitializeHighAccuInputInMenu = false;
        public string levelSelectionInputSafetyLockSeconds_Comment = "Input is ignored for the first X seconds after entering the level selection";
        public float levelSelectionInputSafetyLockSeconds = 3;
        public bool allowReplayOfCompleted = false;
        public bool warnWhenReplayingOldWorlds = true;
        public int timingAccuracyTestSeconds = 5;
        public bool doTimingAccuracyTest_Normal = true;
        public bool doTimingAccuracyTest_Thread = true;

        // Initialize here as necessary
        // 200619 Should be avoided, Configs are not runtime configs!
        public void Initialize()
        {
        }

        // public Goals prototypeGoals;
        // public Goals pilotGoals;
        /*
        public MinMax GetPeriodMinMax_Probes()
        {
            return GetPeriodMinMax_Stimulus(false) * Probes.StimulusToProbeRatio;
        }
        */
        public float GetPeriodRandom_Stimulus(bool isReplaySystem, bool isFMRIScanner)
        {
            StimulusTimingsConfig timings = GetStimulusTimings(isReplaySystem, isFMRIScanner);

            return timings.GetRandom(GetPeriodMinMax_Background());
        }

        public StimulusTimingsConfig GetStimulusTimings(bool isReplaySystem, bool isFMRIScanner)
        {
            return
                !isFMRIScanner && !isReplaySystem ? Experiment.stimulus.timingsGameplay :
                !isFMRIScanner && isReplaySystem ? Experiment.stimulus.timingsReplay :
                isFMRIScanner && !isReplaySystem ? Experiment.stimulus.timingsGameplay_FMRIScanner :
                isFMRIScanner && isReplaySystem ? Experiment.stimulus.timingsReplay_FMRIScanner : null;
        }

        public MinMax GetPeriodMinMax_Stimulus(bool isReplaySystem, bool isFMRIScanner)
        {
            StimulusTimingsConfig timings = GetStimulusTimings(isReplaySystem, isFMRIScanner);

            return timings.GetPeriodMinMax(GetPeriodMinMax_Background());
        }

        public MinMax GetPeriodMinMax_Background()
        {
            return Experiment.stimulus.background.periodMinMax;
        }

        #region Audio Triggers
        // SPAGHETTI
        
        public SoundSystem.RuntimeAudioSourceConfig GetAudioSourceConfig(ModuleType module, AudioSourceType sourceType)
        {
            bool isTrigger = sourceType == AudioSourceType.TRIGGER;

            return new SoundSystem.RuntimeAudioSourceConfig(sourceType.ToString(),
                isTrigger && !Audio.muteTriggersToo,
                GetAudioChannel(module, isTrigger));
        }

        private SoundSystem.StereoType GetAudioChannel(ModuleType module, bool isTrigger)
        {
            SoundSystem.StereoType stereoType = SoundSystem.StereoType.Both;
            if (TriggerOut.GetEventConfig(module)?.doAudio == true)
            {
                int triggerChannel = TriggerOut.triggerManagerAudio.channel;
                int wantedChannel = isTrigger ? triggerChannel : 1 - triggerChannel;

                stereoType = wantedChannel == 0 ? SoundSystem.StereoType.Left : SoundSystem.StereoType.Right;
            }
            return stereoType;
        }

        #endregion

        #region LABELS
        public bool inputUseSerial
        {
            get
            {
                return Input_UseSerial(ExperimentManagerSession.module);
            }
        }

        public bool Input_UseSerial(ModuleType system)
        {
            return
                Input_UseResponseBox(system) &&
                Input_IsSerialEnabled(system);
        }

        private bool Input_IsSerialEnabled(ModuleType system)
        {
            return Input.IsSerialEnabled(system);
        }

        public bool inputUseResponseBox
        {
            get
            {
                return Input_UseResponseBox(ExperimentManagerSession.module);
            }
        }
        public bool Input_UseResponseBox(ModuleType system)
        {
            return Input.UseResponseBox(system);
        }

        private SeattleInputKeysConfigBase currentInputKeysConfig
        {
            get { return GetCurrentInputKeysConfig(ExperimentManagerSession.module, ExperimentManagerSession.handType); }
        }

        private SeattleInputKeysConfigBase GetCurrentInputKeysConfig(ModuleType system, HandType handType)
        {
            if (inputUseSerial)
                return Get_InputSerialPort_ResponseBox(handType);

            return InputKeyCode;
        }
        public SeattleInputKeysConfig_KeyCode InputKeyCode { get { return GetInputKeyCode(ExperimentManagerSession.module, ExperimentManagerSession.handType); } }
        private SeattleInputKeysConfig_KeyCode GetInputKeyCode(ModuleType systemType, HandType handType)
        {
            return Input.UseResponseBox(systemType) ?
                Get_InputKeyCode_ResponseBox(handType) : Get_InputKeyCode_Keyboard(handType);
        }

        public string moveLeft_String { get { return currentInputKeysConfig.moveLeft_String; } }
        public string moveRight_String { get { return currentInputKeysConfig.moveRight_String; } }
        public string menuOK_String { get { return currentInputKeysConfig.menuOK_String; } }
        public string menuClose_String { get { return currentInputKeysConfig.menuClose_String; } }
        public string probeYes_String { get { return currentInputKeysConfig.probeYes_String; } }
        public string probeNo_String { get { return currentInputKeysConfig.probeNo_String; } }
        public string probeMaybe_String { get { return currentInputKeysConfig.probeMaybe_String; } }
        public string reportFace_String { get { return currentInputKeysConfig.reportFace_String; } }
        public string reportObject_String { get { return currentInputKeysConfig.reportObject_String; } }
        public string gamePause_String { get { return currentInputKeysConfig.gamePause_String; } }
        public string skipNarration_String { get { return currentInputKeysConfig.skipNarration_String; } }
        public string debugToggleStats_String { get { return currentInputKeysConfig.debugToggleStats_String; } }

        public List<SerialInputCheck> GetSerialInputChecks(HandType handType)
        {
            return new List<SerialInputCheck>()
            {
                // LEFT
                new SerialInputCheck(Get_InputSerialPort_ResponseBox(handType).moveLeft.lowToHigh, true, InputKeyCode.Action_Left),
                new SerialInputCheck(Get_InputSerialPort_ResponseBox(handType).moveLeft.highToLow, false, InputKeyCode.Action_Left),

                // RIGHT
                new SerialInputCheck(Get_InputSerialPort_ResponseBox(handType).moveRight.lowToHigh, true, InputKeyCode.Action_Right),
                new SerialInputCheck(Get_InputSerialPort_ResponseBox(handType).moveRight.highToLow, false, InputKeyCode.Action_Right),

                // MENU OK
                new SerialInputCheck(Get_InputSerialPort_ResponseBox(handType).menuOK.lowToHigh, true, InputKeyCode.Menu_OK),
                new SerialInputCheck(Get_InputSerialPort_ResponseBox(handType).menuOK.highToLow, false, InputKeyCode.Menu_OK),

                // MENU CLOSE
                new SerialInputCheck(Get_InputSerialPort_ResponseBox(handType).menuClose.lowToHigh, true, InputKeyCode.Menu_Close),
                new SerialInputCheck(Get_InputSerialPort_ResponseBox(handType).menuClose.highToLow, false, InputKeyCode.Menu_Close),

                // YES
                new SerialInputCheck(Get_InputSerialPort_ResponseBox(handType).probeYes.lowToHigh, true, InputKeyCode.Probe_Yes),
                new SerialInputCheck(Get_InputSerialPort_ResponseBox(handType).probeYes.highToLow, false, InputKeyCode.Probe_Yes),

                // NO
                new SerialInputCheck(Get_InputSerialPort_ResponseBox(handType).probeNo.lowToHigh, true, InputKeyCode.Probe_No),
                new SerialInputCheck(Get_InputSerialPort_ResponseBox(handType).probeNo.highToLow, false, InputKeyCode.Probe_No),

                // MAYBE
                new SerialInputCheck(Get_InputSerialPort_ResponseBox(handType).probeMaybe.lowToHigh, true, InputKeyCode.Probe_Maybe),
                new SerialInputCheck(Get_InputSerialPort_ResponseBox(handType).probeMaybe.highToLow, false, InputKeyCode.Probe_Maybe),

                // FACE
                new SerialInputCheck(Get_InputSerialPort_ResponseBox(handType).reportFace.lowToHigh, true, InputKeyCode.Report_Face),
                new SerialInputCheck(Get_InputSerialPort_ResponseBox(handType).reportFace.highToLow, false, InputKeyCode.Report_Face),

                // OBJECT
                new SerialInputCheck(Get_InputSerialPort_ResponseBox(handType).reportObject.lowToHigh, true, InputKeyCode.Report_Object),
                new SerialInputCheck(Get_InputSerialPort_ResponseBox(handType).reportObject.highToLow, false, InputKeyCode.Report_Object),

                // PAUSE
                new SerialInputCheck(Get_InputSerialPort_ResponseBox(handType).gamePause.lowToHigh, true, InputKeyCode.Game_Pause),
                new SerialInputCheck(Get_InputSerialPort_ResponseBox(handType).gamePause.highToLow, false, InputKeyCode.Game_Pause),

                // NARRATION
                new SerialInputCheck(Get_InputSerialPort_ResponseBox(handType).skipNarration.lowToHigh, true, InputKeyCode.Skip_Narration),
                new SerialInputCheck(Get_InputSerialPort_ResponseBox(handType).skipNarration.highToLow, false, InputKeyCode.Skip_Narration),

                // STATS
                new SerialInputCheck(Get_InputSerialPort_ResponseBox(handType).debugToggleStats.lowToHigh, true, InputKeyCode.Debug_ToggleStats),
                new SerialInputCheck(Get_InputSerialPort_ResponseBox(handType).debugToggleStats.highToLow, false, InputKeyCode.Debug_ToggleStats),
            };
        }


        #endregion
    }

    [Serializable]
    public class SeattleDifficultyConfig : DifficultyManager.Config
    {
        public int evaluationWindow_NumAnimCycles = 15;
    }

    [Serializable]
    public class SeattleExperimentConfig : ExperimentManagerLevel.Config
    {
    }

    [Serializable]
    public class SeattleLevelManagerConfig : LevelMasterManager.Config
    { }

    [Serializable]
    public class SeattleAudioConfig : SoundSystem.Config
    { }

    [Serializable]
    public class ExperimentManagerSessionConfig : ExperimentManagerSession.Config
    { }

    [Serializable]
    public class SeattleLoggingTesterConfig : LoggingTester.Config
    { }

    [Serializable]
    public class ReportConfig
    {
        public int reactionWindowMS = 1000;
    }

    [Serializable]
    public class SeattleSubjectConfig : SubjectConfig
    {
    }

    [Serializable]
    public class ProbeConfig : ProbeSystem.Config
    {
    }

    [Serializable]
    public class Probe
    {
        public int Id;
        public string Question;
        public List<string> Answers;
    }

    [Serializable]
    public class SeattlePlayerProgressionConfig : PlayerProgression.Config
    { }

    [Serializable]
    public class MoneyConfig
    {
        public bool showMoney = true;
        public string format = "#.00";

        public float wantedMoneyStart = 0;
        public float wantedMoneyPerStar = 25;
        public float wantedMoneyPerPoint = 20;
        public float wantedMoneyPerCompletedWorld = 5;
        public float wantedMoneyEnd = 0;
    }

    /*
    [Serializable]
    public class GameManagerDuetConfig : GameManager.Config
    {
        public float rotationCyclesPerSecAtPerimeter = 1.0f;
        // public float fullContractDuration = 0.1f;
        public float leftRightSpawningPoints = 0.07f;
        public float upDownSpawnPoints = 0f;
        public float duetDistance = 0.75f;
    }
    */

    #region GAME / EXPERIMENT INPUT
    [Serializable]
    public class SeattleInputConfig : InputManager.Config
    {
        public string defaultHandType_Comment = "Both = 0, Left = 1, Right = 2";
        public HandType defaultHandType = HandType.Both;

        // GAME
        // ESCAPE is above this list
        public KeyCode EXPERIMENTER_ESCAPE = KeyCode.Escape;

        public List<KeyCode> GetExperimenterKeyCodes()
        {
            return new List<KeyCode>()
            {
                EXPERIMENTER_TIMESCALE_FAST,
                EXPERIMENTER_TIMESCALE_SLOW,
                EXPERIMENTER_TIMESCALE_NORMAL,
                EXPERIMENTER_DEBUG_TIME,
                EXPERIMENTER_LEVELSELECTION_LOCK,
                EXPERIMENTER_TIMESCALE_FAST,
                EXPERIMENTER_LEVELSELECTION_UNLOCK,
                EXPERIMENTER_COMPLETE_LEVEL_KEY,
            };
        }

        public KeyCode EXPERIMENTER_TIMESCALE_FAST = KeyCode.F1;
        public KeyCode EXPERIMENTER_TIMESCALE_SLOW = KeyCode.F2;
        public KeyCode EXPERIMENTER_TIMESCALE_NORMAL = KeyCode.F3;
        public KeyCode EXPERIMENTER_TIMESCALE_MODIFIER = KeyCode.LeftShift;

        public KeyCode EXPERIMENTER_DEBUG_TIME = KeyCode.F4;

        public KeyCode EXPERIMENTER_LEVELSELECTION_LOCK = KeyCode.F5;
        public KeyCode EXPERIMENTER_LEVELSELECTION_UNLOCK = KeyCode.F6;

        public bool EXPERIMENTER_ALLOW_COMPLETE_LEVELS = true;
        public KeyCode EXPERIMENTER_COMPLETE_LEVEL_KEY = KeyCode.F9;

        public string startFlippedStrategy_Comment = "AlwaysStartNormal = 0, AlwaysStartFlipped = 1, StartFlippedForEvenIDs = 2, StartFlippedForOddIDS = 3, ValueFromUI = 4";
        public StartFlipped startFlippedStrategy = StartFlipped.StartFlippedForEvenIDs;

        public string startFlippedUI_DefaultValue_Comment = "Only applies if startFlippedStategy is set to ValueFromUI";
        public bool startFlippedUI_DefaultValue = false;

        public bool doFlipMidWay = false;

        public bool GetFlipped(bool isPastMidWay, int subjectID, bool? startFlippedUI)
        {
            bool subjectID_Even = subjectID % 2 == 0;

            bool startFlipped =
                startFlippedStrategy == StartFlipped.AlwaysStartFlipped ||
                (startFlippedStrategy == StartFlipped.StartFlippedForEvenIDs && subjectID_Even) ||
                (startFlippedStrategy == StartFlipped.StartFlippedForOddIDS && !subjectID_Even) ||
                (startFlippedStrategy == StartFlipped.ValueFromUI && startFlippedUI == true);

            bool value = startFlipped;

            if (isPastMidWay && doFlipMidWay)
                value = !startFlipped;

            return value;
        }

        public bool UseResponseBox(ModuleType system)
        {
            switch (system)
            {
                case ModuleType.None:
                    return false;

                case ModuleType.MEEG:
                    return inputUseResponseBox_MEEG;
                case ModuleType.ECOG:
                    return inputUseResponseBox_ECOG;
                case ModuleType.FMRI_Scanner:
                    return inputUseResponseBox_FMRI_Scanner;

                case ModuleType.FMRI_Preparation:
                    return inputUseResponseBox_FMRI_Preparation;
                case ModuleType.ECOG_Preparation:
                    return inputUseResponseBox_ECOG_Preparation;
                case ModuleType.MEEG_Preparation:
                    return inputUseResponseBox_MEEG_Preparation;

                case ModuleType.MEEG_Screening:
                    return inputUseResponseBox_MEEG_Screening;
                case ModuleType.ECOG_Screening:
                    return inputUseResponseBox_ECOG_Screening;
                case ModuleType.FMRI_Screening:
                    return inputUseResponseBox_FMRI_Screening;
            }

            return false;
        }

        public bool inputUseResponseBox_MEEG = true;
        public bool inputUseResponseBox_ECOG = false;
        public bool inputUseResponseBox_FMRI_Scanner = true;

        public bool inputUseResponseBox_MEEG_Preparation = false;
        public bool inputUseResponseBox_ECOG_Preparation = false;
        public bool inputUseResponseBox_FMRI_Preparation = false;

        public bool inputUseResponseBox_MEEG_Screening = false;
        public bool inputUseResponseBox_ECOG_Screening = false;
        public bool inputUseResponseBox_FMRI_Screening = false;

        public bool IsSerialEnabled(ModuleType system)
        {
            switch (system)
            {
                case ModuleType.None:
                    return false;

                case ModuleType.MEEG:
                    return serialEnabled_MEEG;
                case ModuleType.ECOG:
                    return serialEnabled_ECOG;
                case ModuleType.FMRI_Scanner:
                    return serialEnabled_FMRI_Scanner;

                case ModuleType.MEEG_Preparation:
                    return serialEnabled_MEEG_Preparation;
                case ModuleType.ECOG_Preparation:
                    return serialEnabled_ECOG_Preparation;
                case ModuleType.FMRI_Preparation:
                    return serialEnabled_FMRI_Preparation;

                case ModuleType.MEEG_Screening:
                    return serialEnabled_MEEG_Screening;
                case ModuleType.ECOG_Screening:
                    return serialEnabled_ECOG_Screening;
                case ModuleType.FMRI_Screening:
                    return serialEnabled_FMRI_Screening;
            }

            return false;
        }

        public bool serialEnabled_MEEG = false;
        public bool serialEnabled_ECOG = false;
        public bool serialEnabled_FMRI_Scanner = true;

        public bool serialEnabled_MEEG_Preparation = false;
        public bool serialEnabled_ECOG_Preparation = false;
        public bool serialEnabled_FMRI_Preparation = false;

        public bool serialEnabled_MEEG_Screening = false;
        public bool serialEnabled_ECOG_Screening = false;
        public bool serialEnabled_FMRI_Screening = false;
    }

    [Serializable]
    public class SeattleInputKeysConfigBase
    {
        public virtual string moveLeft_String { get; }
        public virtual string moveRight_String { get; }
        public virtual string menuOK_String { get; }
        public virtual string menuClose_String { get; }
        public virtual string probeYes_String { get; }
        public virtual string probeNo_String { get; }
        public virtual string probeMaybe_String { get; }
        public virtual string reportFace_String { get; }
        public virtual string reportObject_String { get; }
        public virtual string gamePause_String { get; }
        public virtual string skipNarration_String { get; }
        public virtual string debugToggleStats_String { get; }

        public bool isResponseBox { get; private set; }

        public bool debug = false;

        public bool navButtonsAffectedByFlipping = false;

        public bool useProbeYesForReplayLevelReporting = true;
        public string reportComment = "If useProbeYesForReplayLevelReporting is set to true, _Probe_Yes is used for reporting. If set to false, the keys below are used instead";

        public SeattleInputKeysConfigBase(bool isResponseBox)
        {
            this.isResponseBox = isResponseBox;
        }

        public bool isFlipped { get; private set; }
        public void SetFlipped(bool isFlipped)
        {
            this.isFlipped = isFlipped;
        }
    }

    [Serializable]
    public class SeattleInputKeysConfig_Serial : SeattleInputKeysConfigBase
    {
        public override string moveLeft_String { get { return moveLeft.label; } }
        public override string moveRight_String { get { return moveRight.label; } }
        public override string menuOK_String { get { return menuOK.label; } }
        public override string menuClose_String { get { return menuClose.label; } }
        public override string probeYes_String { get { return probeYes.label; } }
        public override string probeNo_String { get { return probeNo.label; } }
        public override string probeMaybe_String { get { return probeMaybe.label; } }
        public override string reportFace_String { get { return reportFace.label; } }
        public override string reportObject_String { get { return reportObject.label; } }
        public override string gamePause_String { get { return gamePause.label; } }
        public override string skipNarration_String { get { return skipNarration.label; } }
        public override string debugToggleStats_String { get { return debugToggleStats.label; } }

        public SerialConfigKey menuOK { get { return navButtonsAffectedByFlipping ? probeYes : _probeYes; } }
        public SerialConfigKey menuClose { get { return navButtonsAffectedByFlipping ? probeNo : _probeNo; } }

        public SerialConfigKey probeYes { get { return isFlipped ? _probeNo : _probeYes; } }
        public SerialConfigKey probeNo { get { return isFlipped ? _probeYes : _probeNo; } }
        public SerialConfigKey reportFace { get { return useProbeYesForReplayLevelReporting ? probeYes : isFlipped ? _reportObject : _reportFace; } }
        public SerialConfigKey reportObject { get { return useProbeYesForReplayLevelReporting ? probeYes : isFlipped ? _reportFace : _reportObject; } }

        public SerialConfigKey moveLeft = new SerialConfigKey("Left Index Finger", "a", "A");
        public SerialConfigKey moveRight = new SerialConfigKey("Right Index Finger", "b", "B");
        /// <summary>
        /// [SOS] Json Only. Use <see cref="probeYes"/> instead.
        /// </summary>
        public SerialConfigKey _probeYes = new SerialConfigKey("Left Middle Finger", "c", "C");
        /// <summary>
        /// [SOS] Json Only. Use <see cref="probeNo"/> instead.
        /// </summary>
        public SerialConfigKey _probeNo = new SerialConfigKey("Right Middle Finger", "d", "D");
        public SerialConfigKey probeMaybe = new SerialConfigKey("Left Thumb", "e", "E");
        /// <summary>
        /// [SOS] Json Only. Use <see cref="reportFace"/> instead.
        /// </summary>
        public SerialConfigKey _reportFace = new SerialConfigKey("Left Middle Finger", "c", "C");
        /// <summary>
        /// [SOS] Json Only. Use <see cref="reportObject"/> instead.
        /// </summary>
        public SerialConfigKey _reportObject = new SerialConfigKey("Left Middle Finger", "c", "C");
        public SerialConfigKey gamePause = new SerialConfigKey("KEY_NOT_BOUND", "", "");
        public SerialConfigKey skipNarration = new SerialConfigKey("KEY_NOT_BOUND", "", "");
        public SerialConfigKey debugToggleStats = new SerialConfigKey("KEY_NOT_BOUND", "", "");

        public SeattleInputKeysConfig_Serial(bool isResponseBox) : base(isResponseBox)
        {
        }
    }

    [Serializable]
    public class SeattleInputKeysConfig_KeyCode : SeattleInputKeysConfigBase
    {
        public override string moveLeft_String { get { return Action_Left.GetDescription(isResponseBox); } }
        public override string moveRight_String { get { return Action_Right.GetDescription(isResponseBox); } }
        public override string menuOK_String { get { return Menu_OK.GetDescription(isResponseBox); } }
        public override string menuClose_String { get { return Menu_Close.GetDescription(isResponseBox); } }
        public override string probeYes_String { get { return Probe_Yes.GetDescription(isResponseBox); } }
        public override string probeNo_String { get { return Probe_No.GetDescription(isResponseBox); } }
        public override string probeMaybe_String { get { return Probe_Maybe.GetDescription(isResponseBox); } }
        public override string reportFace_String { get { return Report_Face.GetDescription(isResponseBox); } }
        public override string reportObject_String { get { return Report_Object.GetDescription(isResponseBox); } }
        public override string gamePause_String { get { return Game_Pause.GetDescription(isResponseBox); } }
        public override string skipNarration_String { get { return Skip_Narration.GetDescription(isResponseBox); } }
        public override string debugToggleStats_String { get { return Debug_ToggleStats.GetDescription(isResponseBox); } }

        public ExtendedKeyCode Menu_OK { get { return navButtonsAffectedByFlipping ? Probe_Yes : _Probe_Yes; } }
        public ExtendedKeyCode Menu_Close { get { return navButtonsAffectedByFlipping ? Probe_No : _Probe_No; } }

        public ExtendedKeyCode Probe_Yes { get { return isFlipped ? _Probe_No : _Probe_Yes; } }
        public ExtendedKeyCode Probe_No { get { return isFlipped ? _Probe_Yes : _Probe_No; } }
        public ExtendedKeyCode Report_Face { get { return useProbeYesForReplayLevelReporting ? Probe_Yes : isFlipped ? _Report_Object : _Report_Face; } }
        public ExtendedKeyCode Report_Object { get { return useProbeYesForReplayLevelReporting ? Probe_Yes : isFlipped ? _Report_Face : _Report_Object; } }

        // Those are NOT flipped!
        public ExtendedKeyCode Action_Left = (KeyCode)102;
        public ExtendedKeyCode Action_Right = (KeyCode)106;
        public ExtendedKeyCode Probe_Maybe = (KeyCode)32;

        /// <summary>
        /// [SOS] Json Only. Use <see cref="Probe_Yes"/> instead.
        /// </summary>
        public ExtendedKeyCode _Probe_Yes = (KeyCode)101;
        /// <summary>
        /// [SOS] Json Only. Use <see cref="Probe_No"/> instead.
        /// </summary>
        public ExtendedKeyCode _Probe_No = (KeyCode)105;

        /// <summary>
        /// [SOS] Json Only. Use <see cref="Report_Face"/> instead.
        /// </summary>
        public ExtendedKeyCode _Report_Face = (KeyCode)101;
        /// <summary>
        /// [SOS] Json Only. Use <see cref="Report_Object"/> instead.
        /// </summary>
        public ExtendedKeyCode _Report_Object = (KeyCode)101;

        public ExtendedKeyCode Game_Pause = (KeyCode)112;
        public ExtendedKeyCode Skip_Narration = (KeyCode)32;
        public ExtendedKeyCode Debug_ToggleStats = (KeyCode)104;

        public SeattleInputKeysConfig_KeyCode(bool isResponseBox) : base(isResponseBox)
        {
        }
    }
    #endregion
    
    [Serializable]
    public class SeattleEyeTrackingConfig : EyeTrackerManager_Base.Config
    {
        public string writingStrategy_Comment = "None = 0, PerLevel = 1, PerTwoLevels = 2, PerWorld = 3";

        public WritingStrategy GetWritingStrategy(ModuleType system)
        {
            return
                system == ModuleType.MEEG ? writingStrategy_MEEG :
                system == ModuleType.ECOG ? writingStrategy_ECOG :
                system == ModuleType.FMRI_Scanner ? writingStrategy_FMRIScanner :
                system == ModuleType.MEEG_Preparation ? writingStrategy_MEEGPreparation :
                system == ModuleType.FMRI_Preparation ? writingStrategy_FMRIPreparation : WritingStrategy.PerWorld;
        }

        public WritingStrategy writingStrategy_MEEG = WritingStrategy.PerWorld;
        public WritingStrategy writingStrategy_ECOG = WritingStrategy.PerWorld;
        public WritingStrategy writingStrategy_FMRIPreparation = WritingStrategy.AllInOne;
        public WritingStrategy writingStrategy_MEEGPreparation = WritingStrategy.AllInOne;
        public WritingStrategy writingStrategy_FMRIScanner = WritingStrategy.PerTwoLevels;

        public bool enabled_MEEG = true;
        public bool enabled_MEEG_Preparation = false;
        public bool enabled_ECOG = true;
        public bool enabled_FMRI = true;
        public bool enabled_FMRI_Preparation = false;

        public string heatmapType_Comment = "Gaze = 0, Sacade = 1";
        public HeatmapType heatmapType = HeatmapType.Gaze;
        public bool showHeatmapAfterLevel = true;

        public float maxPercentileDisconnectForWarning = 0.01f;
        public float maxPercentileNoDataForWarning = 0.03f;
        public bool pauseDuringEyeTrackingCalibration = true;
        public bool unPauseAfterEyeTrackingCalibration = true;

        public bool IsEnabled(ModuleType system)
        {
            return
                system == ModuleType.MEEG ? enabled_MEEG :
                system == ModuleType.ECOG ? enabled_ECOG :
                system == ModuleType.FMRI_Scanner ? enabled_FMRI :
                system == ModuleType.MEEG_Preparation ? enabled_MEEG_Preparation :
                system == ModuleType.FMRI_Preparation ? enabled_FMRI_Preparation : false;
        }
    }

    [Serializable]
    public class SeattleEyelinkConfig : EyeTrackerManager_EyeLink.Config
    {
    }

    [Serializable]
    public class SeattleTobiiConfig : EyeTrackerManager_Tobii.Config
    { }

    [Serializable]
    public class NarrationConfig
    {
        public bool showNarration = false;
        // Scale all text durations up or down. Doesn't affect cut-scenes (these are done via the timeline)
        public float inGameTextDurationMultiplier = 0.5f;
        public float fadeDuration = 1.5f;
        public int userSkipNarrationTime = 500;

        // World 1 Level 1
        public string t_1_1_2 = "Progress in your journey by collecting Vibrant Blue Essences";
        public string t_1_2_1 = "Remember that absorbing Blue Essences will restore Blue’s Health";
        public string t_1_3_0 = "Vibrating Essences are filled with energy: they charge up and when they stop wobbling they will swiftly move to an adjacent lane";
        public string t_2_1_1 = "Orange absorbs Orange World Essences to restore health";
        public string t_2_1_0 = "Use the arrow keys to move Orange left and right";

        public string t_2_2_0 = "Being hit by the Opposite color hurts, move right or left to avoid Blue Essences";
        public string t_2_2_1 = "Remember that absorbing Orange Essences will restore Orange’s Health";
        public string t_2_2_2 = "Keep on absorbing Orange Essences to advance in Orange’s adventure";

        // World 2 Level 3
        public string t_2_3_0 = "Remember: Orange Essences are healthy, Blue Essences are harmful";

        // World 1 Level 2
        public string t_3_2_1 = "Avoid Grey Essences: they hurt the Duet";
        public string t_3_2_2 = "Remember that absorbing Orange Essences with Orange and Blue Essences with Blue will replenish the health bar.";
    }

    [Serializable]
    public class UIConfig : ExperimentMenuConfig
    {
    }

    [Serializable]
    public class TextsConfig
    {
        public string textsFormatting_Comment = "You can use the following notations to <b>bold</b>, <i>italicize</i>, <size=32>scale</size> and <color='#FF9900'>color</color> text - more info at https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/StyledText.html";

        public bool allowClsoeViaEscape_PopUps = false;
        public bool onlyAllowProceedThroughMouse_PopUps = true;
        public string popupOkayText = "OK";
        public string popupOkayText_mouseOnly = "WAIT";

        public bool onlyAllowProceedThroughMouse_LevelSelection = true;
        public string menuProceedInstructions = "Press {0} to Proceed";
        public string menuProceedInstructions_mouseOnly = "Wait Before Next Level";
        public string menuProceedInstructions_LOCKED = "Wait Before Next Level";

        public Game_PopUp.RuntimeConfig tutorialWelcome_KeyboardInput = new Game_PopUp.RuntimeConfig("Tutorial", "Thank you for participating in our experiment!\n\nYou will be playing a video game in which you control a colored circle at the bottom of the screen by pressing the {0} and {1} keys. Try to collect falling essences of the same color and avoid those of different colors.\n\nUse your left index finger on the {0} key to move left and your right on the {1} key to move right.\n\n Finally, keep your eyes on the white dot in the middle of the screen, while using your peripheral attention to play the game.\n\nPlease ask the experimenter if you have any questions... and good luck!");
        public Game_PopUp.RuntimeConfig tutorialWelcome_ResponseBoxInput = new Game_PopUp.RuntimeConfig("Tutorial", "Thank you for participating in our experiment!\n\nYou will be playing a video game in which you control a colored circle at the bottom of the screen by pressing your {0} to move left and your {1} to move right. Try to collect falling essences of the same color and avoid those of different colors.\n\n Finally, keep your eyes on the white 'X' in the middle of the screen, while distributing your attention to the game.\n\nPlease ask the experimenter if you have any questions... and good luck!");
        public Game_PopUp.RuntimeConfig levelCompleted_Tutorial_Generic = new Game_PopUp.RuntimeConfig("Level Completed!", "Congratulations!\n\nYou completed this part of the Tutorial\n\n");

        public Game_PopUp.RuntimeConfig informationWelcome = new Game_PopUp.RuntimeConfig("", "");

        public Game_PopUp.RuntimeConfig practiceTutorialMessage_Screening_2Buttons = new Game_PopUp.RuntimeConfig("Tutorial", "You will be playing a short practice level which is the same game with the addition of probes.\n\nRandom images will appear in the background that are irrelevant to the game (faces, objects, etc.). Sometimes you will notice these images and sometimes you won't (and sometimes no image will be presented). Whenever the game pauses, press the '{0}' key for if you saw a Face or Object, and the '{1}' key if you didnt see anything.\n\nIn order to play the game well, you will need to focus all of your attention on the game. All we ask is that you answer the probes honestly. There is no correct answer\n\n Keep your middle fingers on the response buttons.\n\nPlease ask the experimenter if you have any questions... and good luck!");
        public Game_PopUp.RuntimeConfig practiceTutorialMessage_Screening_3Buttons = new Game_PopUp.RuntimeConfig("Tutorial", "You will be playing a short practice level which is the same game with the addition of probes.\n\nRandom images will appear in the background that are irrelevant to the game (faces, objects, etc.). Sometimes you will notice these images and sometimes you won't (and sometimes no image will be presented). Whenever the game pauses, press the '{0}' key for if you saw a Face or Object, the '{2}' key if you saw something but couldn't tell what, and the '{1}' key if you didnt see anything.\n\nIn order to play the game well, you will need to focus all of your attention on the game. All we ask is that you answer the probes honestly. There is no correct answer\n\n Use your middle fingers and thumb for the response buttons\n\nPlease ask the experimenter if you have any questions... and good luck!");

        public Game_PopUp.RuntimeConfig practiceTutorialMessage_Full_2Buttons = new Game_PopUp.RuntimeConfig("Tutorial", "You will be playing a short practice level which is the same game with the addition of probes.\n\nRandom images will appear in the background that are irrelevant to the game (faces, objects, etc.). Sometimes you will notice these images and sometimes you won't (and sometimes no image will be presented). Whenever the game pauses, press the '{0}' key for if you saw a Face or Object, and the '{1}' key if you didnt see anything.\n\nIn order to play the game well, you will need to focus all of your attention on the game. All we ask is that you answer the probes honestly. There is no correct answer\n\n Keep your middle fingers on the response buttons.\n\nPlease ask the experimenter if you have any questions... and good luck!");
        public Game_PopUp.RuntimeConfig practiceTutorialMessage_Full_3Buttons = new Game_PopUp.RuntimeConfig("Tutorial", "You will be playing a short practice level which is the same game with the addition of probes.\n\nRandom images will appear in the background that are irrelevant to the game (faces, objects, etc.). Sometimes you will notice these images and sometimes you won't (and sometimes no image will be presented). Whenever the game pauses, press the '{0}' key for if you saw a Face or Object, the '{2}' key if you saw something but couldn't tell what, and the '{1}' key if you didnt see anything.\n\nIn order to play the game well, you will need to focus all of your attention on the game. All we ask is that you answer the probes honestly. There is no correct answer\n\n Use your middle fingers and thumb for the response buttons\n\nPlease ask the experimenter if you have any questions... and good luck!");

        public bool levelCompleted_Tutorial_PracticeShowExtendedMessage = true;
        public Game_PopUp.RuntimeConfig levelCompleted_Tutorial_PracticeExtended_Success = new Game_PopUp.RuntimeConfig("Level Completed!", "Congratulations!\n\nYou completed this part of the Tutorial\n\nRecommended to proceed to level 1.");
        public Game_PopUp.RuntimeConfig levelCompleted_Tutorial_PracticeExtended_Failure = new Game_PopUp.RuntimeConfig("Level Completed!", "High number of Blanks ({0}).\n\nRecommended to review slides, and try the Practice level again.");

        public string firstProbeInstructions_2Buttons = "Did you see a face / object in the arrow's direction?\n\nPress {0} for YES\nPress {1} for NO";
        public string firstProbeInstructions_3Buttons = "Did you see a face / object in the arrow's direction?\n\nPress {0} for YES, I saw a Face or Object\nPress {2} for YES, I saw something but can't tell what it was\nPress {1} for NO I did not see anything";

        public Game_PopUp.RuntimeConfig welcomeMessage_1_1 = new Game_PopUp.RuntimeConfig("Instructions", "Welcome to world 1. Try to collect as many {0} essences as possible to earn points that can be exchanged for money at the end of the experiment.", 3);
        public Game_PopUp.RuntimeConfig welcomeMessage_2_1 = new Game_PopUp.RuntimeConfig("Instructions", "In the next world, you will control an {0} orb.\n\nTry to collect {0} essences and avoid hitting {1} essences in order to earn points and extra money.\n\nWarning! The falling essences can now jump from one track to another, so pay close attention.\n\n", 3);
        public Game_PopUp.RuntimeConfig welcomeMessage_3_1 = new Game_PopUp.RuntimeConfig("Instructions", "In the next world, you will go back to controlling a {0} orb.\n\nThe falling essences may jump tracks more often now, so pay attention.\n\nIn addition, you will now have a special power: whenever you see a small orange dot to the right of the central +, if you press right again when you are on the right-most track you will destroy all of the {1} essences on the screen. Use this new power wisely!", 3);
        public Game_PopUp.RuntimeConfig welcomeMessage_4_1 = new Game_PopUp.RuntimeConfig("Instructions", "In the final world, you will control an {0} orb, some of the essences will jump tracks, and you will still have the power to destroy all of the bad essences by moving far right. This is portrayed by a small blue dot to the right of the central +.\n\nIn addition, you will have a new special power: whenever you see a small orange dot to the left of the central +, this means you can press left again while on the left-most track to absorb all of the {0} essences currently on the screen.\n\nUse both powers wisely to earn as many points as possible!", 3);
        public string welcomeMessage_InputsFlippedAppendix = "Inputs have been flipped. Press {0} for YES and {1} for NO.";

        public Game_PopUp.RuntimeConfig levelCompleted = new Game_PopUp.RuntimeConfig("Level Completed!", "Congratulations!\n\nYou reached Level {0}\n\n");
        public Game_PopUp.RuntimeConfig levelCompleted_Pair = new Game_PopUp.RuntimeConfig("Level Completed!", "Congratulations!\n\nYou have completed Levels {0} & {1}\n\n");
        public bool showProbesEarlyOutMessage = false;

        public Game_PopUp.RuntimeConfig levelCompleted_ProbesEarlyOut = new Game_PopUp.RuntimeConfig("Level Completed!", "Congratulations! You completed the level.\nLevel completed earlier because probes are finished for this world!");
        public Game_PopUp.RuntimeConfig levelCompleted_GameCompleted = new Game_PopUp.RuntimeConfig("Game Completed!", "Congratulations!\n\nYou reached the end of the whole game successfully!");

        public Game_PopUp.RuntimeConfig fmriReplayBetweenLevels_Faces = new Game_PopUp.RuntimeConfig("Level Completed!", "<b>In the next level you will respond to <color='FF9900'>FACES</color></b>.\nDo not press anything when you see an object. Next level will start automatically.");
        public Game_PopUp.RuntimeConfig fmriReplayBetweenLevels_Objects = new Game_PopUp.RuntimeConfig("Level Completed!", "<b>In the next level you will respond to <color='FF9900'>OBJECTS</color></b>.\nDo not press anything when you see an object. Next level will start automatically.");
        /// <summary>
        /// {0} for level ID
        /// </summary>
        public string eyeLink_HeatmapTitle = "Heatmap for Level {0}";
        public Game_PopUp.RuntimeConfig eyeLink_NotConnected = new Game_PopUp.RuntimeConfig("Eye Link Report", "EyeLink Not Connected!\n\nRight now, EyeLink is enabled but not connected.\n\nAttempting to Re-Connect.");
        public Game_PopUp.RuntimeConfig eyeLink_Connected_ConnLostDuringLastLevel = new Game_PopUp.RuntimeConfig("Eye Link Report", "Lost Connection during Last Level\n\nRight now, it's connected.");
        public Game_PopUp.RuntimeConfig eyeLink_Connected_DataLostDuringLastLevel = new Game_PopUp.RuntimeConfig("Eye Link Report", "Lost Tracking during Last Level\n\nRight now, it's connected.");

        public Game_PopUp.RuntimeConfig instructionTaskRelevant_Faces = new Game_PopUp.RuntimeConfig("Replay Session", "Next, you will see a replay of the video game, but you will no longer be controlling the blue/orange dots. Please focus all of your attention on the faces and objects that appear in the background.\n\nWhenever you see a FACE press the '{0}' key. Do not press any buttons when you see an object. Remember to keep your eyes on the center X. Good luck!");
        public Game_PopUp.RuntimeConfig instructionTaskRelevant_Faces_Inverted = new Game_PopUp.RuntimeConfig("Replay Session", "Next, you will see a replay of the video game, but you will no longer be controlling the blue/orange dots. Please focus all of your attention on the faces and objects that appear in the background.\n\n<b>Inputs have been flipped. Whenever you see a FACE press the <color='FF9900'>'{0}'</color> key.</b> Do not press any buttons when you see an object. Remember to keep your eyes on the center X. Good luck!");

        public Game_PopUp.RuntimeConfig instructionTaskRelevant_Objects = new Game_PopUp.RuntimeConfig("Replay Session", "Next, you will see a replay of the video game, but you will no longer be controlling the blue/orange dots. Please focus all of your attention on the faces and objects that appear in the background.\n\nWhenever you see an OBJECT press the '{0}' key. Do not press any buttons when you see a face. Remember to keep your eyes on the center X. Good luck!");
        public Game_PopUp.RuntimeConfig instructionTaskRelevant_Objects_Inverted = new Game_PopUp.RuntimeConfig("Replay Session", "Next, you will see a replay of the video game, but you will no longer be controlling the blue/orange dots. Please focus all of your attention on the faces and objects that appear in the background.\n\n<b>Inputs have been flipped. Whenever you see a OBJECT press the <color='FF9900'>'{0}'</color> key.</b>. Do not press any buttons when you see a face. Remember to keep your eyes on the center X. Good luck!");

        public string leaderBoardTitle = "Leaderboard";
        public string leaderBoardSubjectName = "you";
        public bool showCongratulatoryMessages = false;
        public string blueOrangeString_Comment = "Capitalize first letter, use lab language (ie. Chinese, English..)";
        public string blueString = "Blue";
        public string orangeString = "Orange";

        internal Game_PopUp.RuntimeConfig GetPracticeTutorialMessage(bool isBehavioral, bool use3Buttons)
        {
            return
                isBehavioral && !use3Buttons    ? practiceTutorialMessage_Screening_2Buttons :
                isBehavioral && use3Buttons     ? practiceTutorialMessage_Screening_3Buttons :
                !isBehavioral && !use3Buttons   ? practiceTutorialMessage_Full_2Buttons : 
                !isBehavioral && use3Buttons    ? practiceTutorialMessage_Full_3Buttons : default;
        }
    }
    
    [Serializable]
    public class SeattleTriggerOutConfig : TriggerMaster.Config
    { }

    [Serializable]
    public class SeattleTriggerInSerialPortConfig : SerialPortTrigger.Config
    {
        public bool IsEnabled(ModuleType system)
        {
            switch (system)
            {
                case ModuleType.None:
                    return false;
                case ModuleType.MEEG:
                    return enabled_MEEG;
                case ModuleType.ECOG:
                    return enabled_ECOG;
                case ModuleType.FMRI_Scanner:
                    return enabled_FMRI_Scanner;
                case ModuleType.MEEG_Preparation:
                    return enabled_MEEG_Preparation;
                case ModuleType.FMRI_Preparation:
                    return enabled_FMRI_Preparation;
            }

            return false;
        }

        public bool enabled_MEEG = false;
        public bool enabled_ECOG = false;
        public bool enabled_FMRI_Scanner = true;
        public bool enabled_MEEG_Preparation = false;
        public bool enabled_FMRI_Preparation = false;
    }

    [Serializable]
    public class TriggerInKeyCodeConfig : KeyboardTrigger.Config
    {
    }

    [Serializable]
    public class FMRIConfig
    {
        public bool scannerSkipInstructions = true;
        public bool showFixationDuringFade = true;
        public float popUpTimeout = 5f;
        public bool fmriPlayAudioDuringBetweenLevels = true;
        public bool scanner_startGame_SendFakeSerialTriggers = false;

        public int numWorldsFMRIPrep = 1;
        public int numLevelsPerWorldFMRIPrep = 2;
        public int numLocalizersPerWorldFMRIPrep = 0;

        internal int numWorldsFMRIScreening = 1;
        internal int numLevelsPerWorldFMRIScreening = 4;
        internal int numLocalizersPerWorldFMRIScreening = 0;

        public bool Scanner_GetShowTutorial(int tutorialIndex)
        {
            return
                tutorialIndex == 0 ? scannerShowTutorial_T :
                tutorialIndex == 1 ? scannerShowTutorial_I :
                tutorialIndex == 2 ? scannerShowTutorial_P : false;
        }

        public bool scannerShowTutorial_T = false;
        public bool scannerShowTutorial_I = true;
        public bool scannerShowTutorial_P = true;
        public bool preventEscapeDuringNullEvent = true;
        // public string useSerialForTR_Comment = "If set to true, TriggerInSerialPort config is used. Otherwise, TriggerInKeyCode.";
        // public bool useSerialForTR = false;
    }

    [Serializable]
    public class MEEGConfig
    {
        public int numWorldsPrep = 1;
        public int numLevelsPerWorldPrep = 2;
        public int numLocalizersPerWorldPrep = 0;

        internal int numWorldsScreening = 1;
        internal int numLevelsPerWorldScreening = 4;
        internal int numLocalizersPerWorldScreening = 0;

        public bool FullVersion_GetShowTutorial(int tutorialIndex)
        {
            return
                tutorialIndex == 0 ? fullVersion_ShowTutorial_T :
                tutorialIndex == 1 ? fullVersion_ShowTutorial_I :
                tutorialIndex == 2 ? fullVersion_ShowTutorial_P : false;
        }

        public bool fullVersion_ShowTutorial_T = false;
        public bool fullVersion_ShowTutorial_I = true;
        public bool fullVersion_ShowTutorial_P = true;
        // public string useSerialForTR_Comment = "If set to true, TriggerInSerialPort config is used. Otherwise, TriggerInKeyCode.";
        // public bool useSerialForTR = false;
    }

    [Serializable]
    public class LevelProgression
    {
        // NS_SEGMENT
        public int highEnergyEssenceLevel = 2;
        public int jumpingEssenceLevel = 11;
        public int thunderPowerUpLevel = 11;
        public int absorptionPowerUpLevel = 16;
        public string thunderPowerUpDescription = "During this level you will discover a new ability - Thunder! By using it, you destroy all the negative essences!";
        public string absorptionPowerUpDescription = "During this level you will discover a new ability - Absorption! By using it, you absorb all the positive essences!";
    }

}