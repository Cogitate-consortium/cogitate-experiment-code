// NS_REMOVE
using ExperimentLibrary;
// NS_REMOVE
using Experiment.Managers;

// NS_DEBATABLE | Should it best be centralized?
using Peripherals.UserInput;
// NS_DEBATABLE | Is it needed?
using Helpers.Async;

using System;
using TGP.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Helpers.Engine;
using Helpers.UI.Core;
using Experiment.Task.Core;
using System.Collections.Generic;
using System.Threading;

namespace Experiment.Task.UI
{
    public class ProbeSystem : MonoBehaviour
    {
        public static ProbeSystem instance;
        private bool _isProbeShown;
        public static bool? isProbeShown { get { return instance?._isProbeShown; } }

        [Header("Cross System")]
        public GameObject crossParent;
        public GameObject tutorialTipParent;
        public Text tutorialText;
        public ArrowTipController arrowTipController;

        [Header("Old System")]
        public TextMeshProUGUI QuestionText;
        public GameObject AnswersParent;
        public Button AnswerPrefab;

        public struct ResponseArgs
        {
            public int crossSelectedIndex;
            public TaskIrrelevantResponse response;
            public int cycleID;
            public double responseTS;
            public double responseTS_NoPauses;
            public bool isMainThread;

            public ResponseArgs(int crossSelectedIndex, TaskIrrelevantResponse response, int cycleID, double responseTS, double responseTS_NoPauses, bool isMainThread)
            {
                this.crossSelectedIndex = crossSelectedIndex;
                this.response = response;
                this.cycleID = cycleID;
                this.responseTS = responseTS;
                this.responseTS_NoPauses = responseTS_NoPauses;
                this.isMainThread = isMainThread;
            }
        }

        private Action<ResponseArgs> callback_TS;
        private object callbackLock = new object();
        private int crossSelectedIndex = 0;

        private double lastTimeShownMS = 0f;

        private static Config config { get { return ExperimentLibraryManager.Config.Probes; } }
        private static RuntimeConfig runtimeConfig;

        private static List<KeyCode> config_InputKeyCodes_Probe_Yes = new List<KeyCode>();
        private static List<KeyCode> config_InputKeyCodes_Probe_No = new List<KeyCode>();
        private static List<KeyCode> config_InputKeyCodes_Probe_Maybe = new List<KeyCode>();
        private static float config_NoAnswerInFirst_MS;
        private static bool config_use3Buttons;

        public int cycleID { get; private set; }

        private void Awake()
        {
            _isProbeShown = false;
        }

        public static void Initialize(RuntimeConfig runtimeConfig)
        {
            ProbeSystem.runtimeConfig = runtimeConfig;

            // Debug.LogError("INIT A!");
            if (instance == null)
            {
                instance = Resources_Helper.ResourceInstantiate<ProbeSystem>("ProbeSystem");
                instance.name = "Probe System";
            }
            else if (GameObject.Find("EventSystem") == null)
            {
                Debug_Helper.LogError(typeof(ProbeSystem), "ProbeSystem need UI 'EventSystem' GameObject in order to work");
                return;
            }

            Vector3 pos = instance.crossParent.transform.position;
            pos.y = ExperimentLibraryManager.Config.Experiment.stimulus.background.GetFixationVerticalPositionScaled(Camera.main);
            instance.SetColors(config.colorMainHex.ToColor(), config.colorTipsHex.ToColor());
            instance.crossParent.transform.SetLossyScale(Vector3.one * GetFixationLossyScale(Camera.main));
            // Debug.LogError(instance.crossParent.transform.localScale);
            // Debug.LogError(instance.crossParent.transform.parent.localScale);
            // Debug.LogError(instance.crossParent.transform.lossyScale);

            instance.crossParent.transform.position = pos;

            string firstProbeInstructions = config.use3Buttons ?
                ExperimentLibraryManager.Config.Texts.firstProbeInstructions_3Buttons :
                ExperimentLibraryManager.Config.Texts.firstProbeInstructions_2Buttons;

            instance.tutorialText.text = string.Format(firstProbeInstructions,
                ExperimentLibraryManager.Config.probeYes_String, ExperimentLibraryManager.Config.probeNo_String, ExperimentLibraryManager.Config.probeMaybe_String);
            instance.Hide();
            instance.arrowTipController.lerp = ExperimentLibraryManager.Config.Probes.arrowBetweenCircleAndTarget;
            ToggleTutorialTip(false);
        }

        public static float GetFixationLossyScale(Camera camera)
        {
            float baseSize = config.crossSizeMultiplier;
            float scaleMultiCamera = ExperimentLibraryManager.Config.Experiment.stimulus.background.GetScaleMultiplierFixation(camera);

            float lossyScale = baseSize * scaleMultiCamera;

            // Debug.Log("LOssy Scale : " + lossyScale);

            return lossyScale;
        }

        private void Start()
        {
            InputManager.onKeyDown_TS += InputManager_onKeyDown_TS;
        }

        private void OnDestroy()
        {
            _isProbeShown = false;
            InputManager.onKeyDown_TS -= InputManager_onKeyDown_TS;
        }

        private void InputManager_onKeyDown_TS(object sender, InputManager.HighAccuracyEventArgs e)
        {
            KeyCode keyCode = e.key;

            TimeWrapper.Timestamp responseTS = e.timestamp;

            if (config_InputKeyCodes_Probe_Yes.Contains(keyCode))
            {
                HandleResponse_TS(TaskIrrelevantResponse.Yes, responseTS);
            }
            else if (config_InputKeyCodes_Probe_No.Contains(keyCode))
            {
                HandleResponse_TS(TaskIrrelevantResponse.No, responseTS);
            }
            // Handle 3 - buttons
            else if (config_InputKeyCodes_Probe_Maybe.Contains(keyCode))
            {
                if (config_use3Buttons)
                    HandleResponse_TS(TaskIrrelevantResponse.Maybe, responseTS);
            }
        }

        private void HandleResponse_TS(TaskIrrelevantResponse probeAnswer, TimeWrapper.Timestamp e, bool allowEarlyAnswers = false)
        {
            if (!EngineWrapper.Debug_IsDebugBuild)
                allowEarlyAnswers = false;

            lock (callbackLock)
                if (callback_TS == null) return;

            if (e.timestampMS - lastTimeShownMS < config_NoAnswerInFirst_MS && !allowEarlyAnswers)
            {
                if (EngineWrapper.Debug_IsDebugBuild)
                    Debug.LogError(TimeWrapper.currentTimestampMS + " PREVENTED ANSWER WITHIN FIRST SECONDS OF PROBE");

                ExperimentManagerSession.LogData_AsTheyHappen_TS(e, "PROBE_SYSTEM", "PREVENTED_EARLY_PROBE");

                return;
            }

            AsyncThread.RunOnMainThread_ASAP_TS(() =>
            {
                // CancelInvoke("Timeout");
                Hide();
            });

            lock (callbackLock)
                if (callback_TS != null)
                {
                    // [SOS] FIRST mark as hidden - THEN callback (or unpausing wont work)
                    // This also happens in Hide() but that's on the main thread so it wont make it in time
                    _isProbeShown = false;
                    callback_TS(new ResponseArgs(crossSelectedIndex, probeAnswer, cycleID, e.timestampMS, e.timestampMS_NoPauses, AsyncThread.isMainThread_TS));
                    callback_TS = null;
                    // Debug.Log(TimeWrapper.currentTimestampMS + " DONE!");
                }
                else if (EngineWrapper.Debug_IsDebugBuild)
                    Debug.LogError(TimeWrapper.currentTimestampMS + " PREVENTED DOUBLE-LOGGING");
        }

        private void SetColors(Color mainColor, Color tipsColor)
        {
            crossParent.GetComponent<SpriteRenderer>().color = mainColor;
        }
        /*
        /// <summary>
        /// Display a window with the probe question/answers.
        /// Results in id and text of answer
        /// </summary>
        public static void ShowProbe(Probe probe, Action<int, string> result)
        {
            Initialize();
            instance.showProbe(probe, result);
        }
        */

        /// <summary>
        /// Display a window with the probe question/answers.
        /// Results in id and text of answer
        /// </summary>
        public static void ShowProbe(int currentCycleId, int indexToPointAt, Transform targetSquare)
        {
            // Timeout for FMRI
            instance.ShowCross(currentCycleId, indexToPointAt, targetSquare, runtimeConfig.probeTimeout_Seconds);
        }

        public static void AssignResultCallback(Action<ResponseArgs> result_TS)
        {
            instance._AssignResultCallback(result_TS);
        }

        public static void ToggleTutorialTip(bool show)
        {
            instance.tutorialTipParent.SetActive(show);
        }

        private void Hide()
        {
            // Maybe add effect later (fade out)
            // this.gameObject.SetActive(false);
            arrowTipController.Hide();
            SetCrossSortingOrder(config.crossDefaultsInFront ? 15 : 0);
            instance.tutorialTipParent.SetActive(false);

            // This should happen on the next frame
            AsyncThread.RunOnMainThread_NextFrameCycle_TS(() =>
            {
                _isProbeShown = false;
            });
        }

        public void Show()
        {
            // Maybe add effect later (fade in)
            this.gameObject.SetActive(true);

            // This should happen on the next frame
            AsyncThread.RunOnMainThread_NextFrameCycle_TS(() =>
            {
                _isProbeShown = true;
                lastTimeShownMS = TimeWrapper.lastRenderedFrame_TimeOfRenderMS;
                EXPERIMENTER_ONLY_AUTO_ANSWER_TIME_MS = Utility_Helper.RandomRange(500, 5000);
            });
        }

        private float EXPERIMENTER_ONLY_AUTO_ANSWER_TIME_MS;

        private void Update()
        {
            // Handle auto-answer
            if (ExperimentManagerSession.EXPERIMENTER_AUTO_ANSWER &&
                TimeWrapper.currentTimestampMS - lastTimeShownMS > EXPERIMENTER_ONLY_AUTO_ANSWER_TIME_MS)
            {
                lock (callbackLock)
                    if (callback_TS != null)
                    {
                        TaskIrrelevantResponse response = Utility_Helper.EnumGetRandom<TaskIrrelevantResponse>();
                        Debug.LogWarning("[{0} ({1})] Auto-answering probe :: {2}"._Format(
                            TimeWrapper.currentFrameCycleID,
                            TimeWrapper.currentTimestampMS, response));
                        HandleResponse_TS(response, TimeWrapper.GetCurrentTimestamp_TS(), true);
                    }
            }
        }

        private void ShowCross(int currentCycleId, int indexToPointAt, Transform targetSquare, float timeout)
        {
            // Refresh those every time we show a probe! Input flipping may have occured.
            config_InputKeyCodes_Probe_Yes = ExperimentLibraryManager.Config.InputKeyCode.Probe_Yes;
            config_InputKeyCodes_Probe_No = ExperimentLibraryManager.Config.InputKeyCode.Probe_No;
            config_InputKeyCodes_Probe_Maybe = ExperimentLibraryManager.Config.InputKeyCode.Probe_Maybe;
            config_NoAnswerInFirst_MS = config.probeNoAnswerInFirstMS;
            config_use3Buttons = config.use3Buttons;

            cycleID = currentCycleId;
            Show();
            SetCrossSortingOrder(15);

            crossSelectedIndex = indexToPointAt;
            arrowTipController.PointTo(indexToPointAt, targetSquare);

            // Debug.LogError(TimeWrapper.currentTimestampMS);

            int numFramesRendered = TimeWrapper.numRenderedFrames;
            AsyncThread.RequestRunOnNewThread(() =>
            {
                while (TimeWrapper.numRenderedFrames == numFramesRendered) Thread.Sleep(1);

                // Debug.LogError(TimeWrapper.currentTimestampMS);
                Thread.Sleep((int) (timeout * 1000));
                // Debug.LogError(TimeWrapper.currentTimestampMS);
                HandleResponse_TS(TaskIrrelevantResponse.NoResponse, TimeWrapper.GetCurrentTimestamp_TS());
            });
        }

        private void _AssignResultCallback(Action<ResponseArgs> result_TS)
        {
            lock (callbackLock)
                callback_TS = result_TS;
        }

        private void SetCrossSortingOrder(int order)
        {
            SpriteRenderer sR = crossParent.GetComponent<SpriteRenderer>();
            sR.sortingOrder = order;
            arrowTipController.arrowTip.sortingOrder = order;
        }

        public class RuntimeConfig
        {
            public float probeTimeout_Seconds;

            public RuntimeConfig(float probeTimeout_Seconds)
            {
                this.probeTimeout_Seconds = probeTimeout_Seconds;
            }
        }

        [Serializable]
        public class Config
        {
            public bool keepMusicOnDuringProbes = false;

            public bool showProbes = true;
            public bool use3Buttons = false;

            public string scaleNumProbesToNumWorlds_Comment = "Default number of worlds is 4. For some setups, this is reduced to 2. When that happens, should we show proportionately less probes?";
            public bool scaleNumProbesToNumWorlds = true;
            public string wantedNumProbes_4Worlds_Comment = "Default number of worlds is 4. How many probes should 4 worlds have in total?";

            /// <summary>
            /// [SOS] Use <see cref="GetWantedNumProbesTotal(int)"/> and <seealso cref="ExperimentManagerApplication.GetWantedNumProbesPerWorld(int)"/> instead
            /// </summary>
            public int wantedNumProbes_4Worlds = 200;
            public int probeShieldBeforeEndOfLevelMS = 1000;

            public float probeDelayAfterStimulus { get { return probeDelayAfterStimulusMS / 1000; } }
            [SerializeField] private float probeDelayAfterStimulusMS = 50f;
            public float probeTimeout_Seconds { get { return probeTimeoutMS / 1000f; } }
            public int probeTimeoutMS = 3000;
            public int probeNoAnswerInFirstMS = 3000;

            public bool earlyOutWorldWhenProbesComplete = false;

            /// <summary>
            /// [SOS] Use <see cref="GetNumWantedProbesPerWorld"/> instead
            /// </summary>
            /// <param name="numWorlds"></param>
            /// <returns></returns>
            public int GetWantedNumProbesPerWorld(int numWorlds)
            {
                return GetWantedNumProbesTotal(numWorlds) / numWorlds;
            }

            private int GetWantedNumProbesTotal(int numWorlds)
            {
                return scaleNumProbesToNumWorlds ? Mathf.CeilToInt((float)wantedNumProbes_4Worlds * numWorlds / 4) : wantedNumProbes_4Worlds;
            }

            public string colorMainHex = "CACACA";
            public string colorTipsHex = "FFD600";
            // public float StimulusToProbeRatio = 5;
            public TruncatedExpConfig expConfig = new TruncatedExpConfig(0.6164f, 12.0f, 18.0f, 100); // 13.5f
            public TruncatedExpConfig expConfig_FMRIScanner = new TruncatedExpConfig(0.4623f, 16.0f, 24.0f, 100); // 18.0f

            public bool crossDefaultsInFront = true;
            public float crossSizeMultiplier = 1.0f;

            public string crossSizeVerticalPos_Multiplier_Comment = "[DEV-ONLY] Do not change this value";
            public float minResetT_Threshold = 0.1f;
            public int minResetT_maxSearch = 10;

            public float arrowBetweenCircleAndTarget = 1f;
            [SerializeField] private int acceptableNumberofBlanksFalseAlarmsPractice_Full = 1;
            [SerializeField] private int acceptableNumberofBlanksFalseAlarmsPractice_PrepScreening = 1;
            [SerializeField] private int numProbesInPractice_Full = 8;
            [SerializeField] private int numProbesInPractice_PrepScreening = 8;

            public int GetAcceptableNumberOfFalseAlarmsPractice(PrepVsFull prepVsFull)
            {
                return prepVsFull == PrepVsFull.Full ?
                    acceptableNumberofBlanksFalseAlarmsPractice_Full :
                    acceptableNumberofBlanksFalseAlarmsPractice_PrepScreening;
            }

            public int GetNumProbesInPractice(PrepVsFull prepVsFull)
            {
                return prepVsFull == PrepVsFull.Full ?
                    numProbesInPractice_Full :
                    numProbesInPractice_PrepScreening;
            }
        }
    }
}