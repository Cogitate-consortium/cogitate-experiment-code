using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TGP.Helpers;
using UnityEngine;
using Helpers.Engine;
using Helpers.Assets;
using Helpers.Async; // Used just to debug on main thread NS_SEGMENT to AsyncMain?

namespace Experiment.Background
{
    /// <summary>
    /// [CLEANUP] <see cref="AnimateIE"/> is a 250 line mess
    /// </summary>
    public class BackgroundManager : MonoBehaviour
    {
        public event EventHandler<AnimEventArgs> onAnimEvent;

        [SerializeField, Range(10, 60)] protected int fullAnimationDuration = 35;
        [SerializeField] private bool debugInitialization = false;

        protected static Config config { get; private set; }

        protected bool isPaused = false;

        protected readonly List<BackgroundObject> backgroundObjects = new List<BackgroundObject>();
        private const string BACKGROUND_OBJECT_RESOURCE_NAME_FORMAT = "Background/BackgroundObject_{0}";

        private int colorSwapIndex = 0;

        public static int currentCycleIdx { get; private set; }
        private Coroutine animateCR;

        private readonly Dictionary<Direction_2D_Diagonal, BackgroundObject> directionToObject = new Dictionary<Direction_2D_Diagonal, BackgroundObject>();

        public static BackgroundManager InitializeAndReturnCurrent(Config config, RuntimeConfig runtimeConfig)
        {
            // [TOOD] Clean up
            BackgroundManager blinkingSquaresBG = Utility_Helper.GetComponentInScene<BackgroundManager_Abstract_BlinkingSquares>();
            blinkingSquaresBG.gameObject.SetActive(false);

            BackgroundManager rotatingSquaresBG = Utility_Helper.GetComponentInScene<BackgroundManager_Abstract_RotatingSquares>();
            rotatingSquaresBG.gameObject.SetActive(false);

            BackgroundManager rotatingStaticSquaresBG = Utility_Helper.GetComponentInScene<BackgroundManager_Abstract_StaticRotatingSquares>();
            rotatingStaticSquaresBG.gameObject.SetActive(false);

            BackgroundManager backgroundManager = config.useBlinkingSquaresAnimation ? blinkingSquaresBG : rotatingStaticSquaresBG;
            backgroundManager.gameObject.SetActive(true);

            backgroundManager.Initialize(config, runtimeConfig);

            return backgroundManager;
        }

        #region Public Methods

        public static void SetCycleIdx(int cycleIdx)
        {
            currentCycleIdx = cycleIdx;
        }
        
        public class RuntimeConfig
        {
            // MinMax timerLock_MinMax;
            public List<EventInformation> timings;

            public RuntimeConfig(List<EventInformation> timings)
            {
                this.timings = timings;
            }
        }

        /*
        public void Restart()
        {
            this.Log("Restart!");

            // [SOS] Only the Stimulus's Restart chains its UnPause
            // Fire an Unpause
            Pause(false);
            lockedRandomTimer.Restart();
        }
        */

        public void Pause(bool doPause)
        {
            this.Log("Pause -> {0}"._Format(doPause.BoolToOnOff()));
            isPaused = doPause;

            for (int i = 0; i < backgroundObjects.Count; i++)
                backgroundObjects[i].Pause(doPause);
        }

        public List<BackgroundObject> GetBackgroundObjects()
        {
            return backgroundObjects;
        }

        /*
        public void ListenToKoregraphyEvents()
        {
            Peripherals.Koreo.KoreographyWrapper.onMelody += KoreographyEventWrapper_onMelody;
        }

        private void KoreographyEventWrapper_onMelody(object sender, EventArgs e)
        {
            for (int i = 0; i < backgroundObjects.Count; i++)
            {
                Color color = (colorSwapIndex == 0) ? ApplicationLibrary.DefaultTheme.LightTone_1 : ApplicationLibrary.DefaultTheme.LightTone_2;
                backgroundObjects[i].SetColorOverlay(color);
            }
            colorSwapIndex = (colorSwapIndex + 1) % 2;
        }
        */

        #endregion

        #region Iternal Logic

        // [1 of 4]
        protected void HandleCycleBegin(List<BackgroundObject> peakObjects, int cycleIdxOfPeak, bool isStimulusPeak, double distanceToPeakBeginS, bool isFirstCycle)
        {
            AnimEventArgs peakEventArgs = new AnimEventArgs();

            // [SOS] Create a copy of objects! We need the original to later paint distractor images
            peakEventArgs.visibleObjects = new List<BackgroundObject>(peakObjects);
            peakEventArgs.backgroundCycleIdx = cycleIdxOfPeak;
            peakEventArgs.isStimulusPeak = isStimulusPeak;
            peakEventArgs.distanceToPeakBeginS = distanceToPeakBeginS;
            peakEventArgs.isFirstCycle = isFirstCycle;

            RaiseAnimEvent(AnimEvent.CycleBegin, peakEventArgs);
        }

        // [2 of 4]
        protected void HandlePeakBegin(List<BackgroundObject> peakObjects, int cycleIdxOfPeak, bool isStimulusPeak, bool isFirstCycle)
        {
            AnimEventArgs peakEventArgs = new AnimEventArgs();

            // [SOS] Create a copy of objects! We need the original to later paint distractor images
            peakEventArgs.visibleObjects = new List<BackgroundObject>(peakObjects);
            peakEventArgs.backgroundCycleIdx = cycleIdxOfPeak;
            peakEventArgs.isStimulusPeak = isStimulusPeak;
            peakEventArgs.isFirstCycle = isFirstCycle;

            RaiseAnimEvent(AnimEvent.PeakBegin, peakEventArgs);
        }

        // [3 of 4]
        protected void HandlePeakEnd(List<BackgroundObject> peakObjects, int cycleIdxOfPeak, bool isStimulusPeak, bool isFirstCycle)
        {
            AnimEventArgs peakEventArgs = new AnimEventArgs();

            // [SOS] Create a copy of objects! We need the original to later paint distractor images
            peakEventArgs.visibleObjects = new List<BackgroundObject>(peakObjects);
            peakEventArgs.backgroundCycleIdx = cycleIdxOfPeak;
            peakEventArgs.isStimulusPeak = isStimulusPeak;
            peakEventArgs.isFirstCycle = isFirstCycle;

            RaiseAnimEvent(AnimEvent.PeakEnd, peakEventArgs);
        }

        // [4 of 4]
        protected void HandleCycleEnd(List<BackgroundObject> peakObjects, int cycleIdxOfPeak, bool isStimulusPeak, double distanceToPeakBeginS, bool isFirstCycle)
        {
            AnimEventArgs peakEventArgs = new AnimEventArgs();

            // [SOS] Create a copy of objects! We need the original to later paint distractor images
            peakEventArgs.visibleObjects = new List<BackgroundObject>(peakObjects);
            peakEventArgs.backgroundCycleIdx = cycleIdxOfPeak;
            peakEventArgs.isStimulusPeak = isStimulusPeak;
            peakEventArgs.distanceToPeakBeginS = distanceToPeakBeginS;
            peakEventArgs.isFirstCycle = isFirstCycle;

            RaiseAnimEvent(AnimEvent.CycleEnd, peakEventArgs);
        }


        private void RaiseAnimEvent(AnimEvent animEvent, AnimEventArgs e)
        {
            e.animEvent = animEvent;
            onAnimEvent?.Invoke(this, e);
        }

        /// <summary>
        /// Fires after a dead frame, last call is of lerp 1
        /// </summary>
        /// <param name="totalTime"></param>
        /// <param name="progressCB"></param>
        /// <returns></returns>
        private IEnumerator WaitForTime(float totalTime, Action<float> progressCB)
        {
            // yield return new WaitForSeconds(totalTime); progressCB?.Invoke(1); yield break;
            // Debug.Log("Intended Wait For :: " + totalTime);
            float elapsedTime = 0;
            while (elapsedTime < totalTime)
            {
                while (isPaused)
                    yield return null;

                yield return null;

                // Add the frame time
                elapsedTime += TimeWrapper.deltaTime_SinceLastUpdate_NotTS;

                // Debug.Log(elapsedTime);
                float lerp = Mathf.Clamp01(elapsedTime / totalTime);

                // Debug.Log(TimeWrapper.currentTimestampMS + " : " + lerp.PercentileToPercent());

                progressCB?.Invoke(lerp);
            }
        }

        bool doDebugParts = false;
        bool doDebugWhole = false;
        public float correctorA = 1f;
        public float correctorB = 1f;

        protected virtual IEnumerator AnimateIE(List<EventInformation> backgroundPeaks)
        {
            // Wait for new GameObjects to load
            yield return null;

            InitializeBackgroundObjectPositions(backgroundObjects);

            // Turn everything on!
            for (int i = 0; i < backgroundObjects.Count; i++)
            {
                backgroundObjects[i].Toggle(true);
                // Reset any images
                backgroundObjects[i].UnPaint();
                if (debugInitialization)
                    backgroundObjects[i].SetLocalScale(0.1f);
            }

            if (debugInitialization)
                yield break;

            float peakDuration = config.peakDuration; // config.peakEaseIn + config.peakEaseOut + [DEPRECATED?, 200508]

            // int currentLocalizerID = PlayerProgression.GetCurrentLocalizerID();
            
            //Debug.LogError("Current localizer ID :: " + currentLocalizerID);
            bool isFirst = true;

            EventInformation currentCycleInfo = new EventInformation(0);
            while (true)
            {
                while (isPaused)
                    yield return null;

                double currentCycleDuration = 0;

                if (currentCycleIdx > backgroundPeaks.Count)
                {
                    this.LogWarning("Ran out of background peaks to show! Hopefully level about to end");
                    currentCycleInfo = backgroundPeaks.GetRandom();
                    currentCycleDuration = currentCycleInfo.delayFromPrevious; // we don't care
                }
                else
                {
                    currentCycleInfo = backgroundPeaks[currentCycleIdx];

                    if (currentCycleIdx <= backgroundPeaks.Count - 2)
                        currentCycleDuration = backgroundPeaks[currentCycleIdx + 1].delayFromPrevious;
                    else
                        currentCycleDuration = backgroundPeaks.GetRandom().delayFromPrevious; // Doesn't matter it's just the last one, give it a random duration
                }

                int numAnimEventsPerCycle = 4;
                float mainAnimationDuration = config.cycleDuration;
                float mainAnimationDuration_1_to_2 = mainAnimationDuration / 2f;
                float mainAnimationDuration_2_to_3 = config.peakDuration;
                float mainAnimationDuration_3_to_4 = mainAnimationDuration / 2f;

                // Inverse phase of main squares peak
                double satellitePeakDuration = currentCycleDuration - mainAnimationDuration - config.peakDuration;
                double distanceToPeakBeginS = 0;
                float wantedPeakDurationCorrector = 0;
                double t1 = 0;
                double t2 = 0;
                double t3 = 0;
                double t4 = 0;

                this.LogWarning(string.Format("Current Cycle:{0} | Anim Duration:{1}", currentCycleDuration, mainAnimationDuration));

                // Find Main Sqaures (center 4)
                List<BackgroundObject> mainStimuliObjects = GetBackgroundObjects().FindAll(bgObject => bgObject.IsStimulusCandidate());

                // Select half the background object to paint to them blobs
                List<BackgroundObject> blobCandidateObjects = new List<BackgroundObject>();

                BeginNewAnimationCycle();
                // RaiseTrialBeginEvent();

                if (!isFirst)
                {
                    // Main Animation Out-InversePeak-In
                    // Main Squares are at Maximum - Satellites at minimum
                    // These will take affect on the NEXT frame
                    onStatusUpdate?.Invoke(this, new StatusArgs(1, numAnimEventsPerCycle, AnimEvent.CycleBegin, "CRIT_SQUARES_GROWING", doDebugParts));
                    distanceToPeakBeginS = mainAnimationDuration_1_to_2 + PerformanceObserver.averageFrameDT_Seconds; // config.peakEaseIn + [DEPRECATED?, 200508]
                    HandleCycleBegin(mainStimuliObjects, currentCycleInfo.id, currentCycleInfo.triggersEventID >= 0, distanceToPeakBeginS, isFirst);

                    bool first12 = true;
                    // Half Animation Out
                    // Main squares growing
                    yield return StartCoroutine(WaitForTime(mainAnimationDuration_1_to_2 + PerformanceObserver.averageFrameDT_Seconds - PerformanceObserver.averageFrameDT_Seconds / 2, (lerp) =>
                    {
                        if (first12)
                        {
                            AsyncThread.RunOnMainThread_NextFrameCycle_TS(() =>
                            { // That's when we rendered the current frame
                                t1 = TimeWrapper.lastRenderedFrame_TimeOfRenderMS;
                            });

                            first12 = false;
                        }

                        // 2nd Half animation (50%-100%)
                        HandleMasterAnimation(0.5f + lerp * 0.5f);
                    }));

                    /* [DEPRECATED? 200508]
                    // Handle the Animation PEAK EASE IN
                    if (config.peakEaseIn > 0)
                        yield return StartCoroutine(WaitForTime(config.peakEaseIn - SubjectPerformanceReport.averageFrameDT_Seconds / 2f, (lerp) =>
                        {
                            if (config.useCandidateBrightness)
                                HandlePeakAnimation_EaseIn(blobCandidateObjects, lerp);
                        }));
                    */

                    // 2 of 4. Let them know we are on peak!
                    // [SOS] This is accurately reflecting what happens on screen - These will take affect on the NEXT frame
                    // These will take affect on the NEXT frame
                    onStatusUpdate?.Invoke(this, new StatusArgs(2, numAnimEventsPerCycle, AnimEvent.PeakBegin, "CRIT_SQUARES_MAXIMUM", doDebugParts));
                    HandlePeakBegin(mainStimuliObjects, currentCycleInfo.id, currentCycleInfo.triggersEventID >= 0, isFirst);

                    double peakBeginTS = TimeWrapper.currentTimestampMS;

                    float waitFor = 0;

                    // Handle the Animation PEAK MAIN
                    if (mainAnimationDuration_2_to_3 > 0)
                    {
                        // [HACK] If there's a peak, make it last one frame less than the wanted duration,
                        // as the last frame of the peak will stay on screen for a frame
                        wantedPeakDurationCorrector = PerformanceObserver.averageFrameDT_Seconds;

                        AsyncThread.RunOnMainThread_NextFrameCycle_TS(() =>
                        {
                            // That's when we rendered the current frame
                            t2 = TimeWrapper.lastRenderedFrame_TimeOfRenderMS;
                            float actualDurationMS = (float)(t2 - t1);
                            float wantedDurationMS = mainAnimationDuration_1_to_2 * 1000;
                            if (EngineWrapper.Debug_IsDebugBuild && doDebugParts)
                                Debug.LogError("t2 - t1 from " + wantedDurationMS.ToString("#") + " :: " + Mathf.RoundToInt(actualDurationMS - wantedDurationMS)); // [200514] ACCURATELY MATCHES VIDEO DURATION FOR PEAK ON !!! Note EDITOR and BUILD behave differently (this is accurate in both cases)
                        });
                        // 
                        waitFor = mainAnimationDuration_2_to_3 - wantedPeakDurationCorrector - PerformanceObserver.averageFrameDT_Seconds / 2f;

                        yield return StartCoroutine(WaitForTime(waitFor, (lerp) =>
                        {
                            if (config.useCandidateBrightness)
                                HandlePeakAnimation_Main(blobCandidateObjects, lerp);
                        }));
                    }

                    // Debug.Log("PEAK_DURATION_CODE " + (TimeWrapper.currentTimestampMS - peakBeginTS).ToString("#"));

                    // Unpaint objects
                    //if (config.useCandidateDistractorImages)
                    //{
                    //    for (int i = 0; i < objectsToPeak.Count; i++)
                    //    {
                    //        objectsToPeak[i].UnPaint();
                    //    }
                    //}

                    /* [DEPRECATED? 200508]
                    if (config.peakEaseOut > 0)
                        yield return StartCoroutine(WaitForTime(config.peakEaseOut - SubjectPerformanceReport.averageFrameDT_Seconds / 2, (lerp) =>
                        {
                            if (config.useCandidateBrightness)
                                HandlePeakAnimation_EaseOut(blobCandidateObjects, lerp);
                        }));
                    */
                }

                //Debug.LogWarning("Main Animation 0");

                // 3 of 4. Let them know peak ended
                // [SOS] This is accurately reflecting what happens on screen - These will take affect on the NEXT frame
                onStatusUpdate?.Invoke(this, new StatusArgs(3, numAnimEventsPerCycle, AnimEvent.PeakEnd, "CRIT_SQUARES_SHRINKING", doDebugParts));

                // Find all visible and not main stimuli squares
                // Main Squares are at Maximum - Satellites at minimum
                // RePaint satellites with blobs
                blobCandidateObjects.AddRange(OnPaintSatellites());

                bool firedPeakEndThisCycle = false;
                // We have split animation in 2 parts, so to ensure we don't lose event (lerp = 0.5) in fast-forward
                // Half Animation In
                yield return StartCoroutine(WaitForTime(mainAnimationDuration_1_to_2 + wantedPeakDurationCorrector - PerformanceObserver.averageFrameDT_Seconds / 2f, (lerp) =>
                {
                    if (!firedPeakEndThisCycle)
                    {
                        // This is when the peak will actually happen
                        AsyncThread.RunOnMainThread_NextFrameCycle_TS(() =>
                            {
                                t3 = TimeWrapper.lastRenderedFrame_TimeOfRenderMS;
                                float actualDurationMS = (float)(t3 - t2);
                                float wantedDurationMS = mainAnimationDuration_2_to_3 * 1000;
                                if (EngineWrapper.Debug_IsDebugBuild && doDebugParts)
                                    Debug.LogError("t3 - t2 from " + wantedDurationMS + " :: " + Mathf.RoundToInt(actualDurationMS - wantedDurationMS)); // [200514] ACCURATELY MATCHES VIDEO DURATION FOR PEAK ON !!! Note EDITOR and BUILD behave differently (this is accurate in both cases)
                        });
                        // Debug.LogWarning("t360 - t310 from 250 :: " + Mathf.RoundToInt((float)(TimeWrapper.currentTimestampMS - t310 - config.peakDurationMS)));
                        HandlePeakEnd(mainStimuliObjects, currentCycleInfo.id, currentCycleInfo.triggersEventID >= 0, isFirst);
                        firedPeakEndThisCycle = true;
                    }

                    // Debug.Log(TimeWrapper.currentFrameCycleID + " : " + lerp);
                    // 1st Half animation (0-50%)
                    HandleMasterAnimation(lerp * 0.5f);
                }));

                // Main Squares are at MINIMUM
                // 4 of 4. Let StimulusManager to choose next Stimulus and paint it
                // distanceToPeakBeginS = satellitePeakDuration + mainAnimationDuration_0_to_1 + SubjectPerformanceReport.averageFrameDT_Seconds; // config.peakEaseIn + [DEPRECATED?, 200508]

                // These will take affect on the NEXT frame
                onStatusUpdate?.Invoke(this, new StatusArgs(4, numAnimEventsPerCycle, AnimEvent.CycleEnd, "CRIT_SQUARES_MINIMUM", doDebugParts));
                HandleCycleEnd(mainStimuliObjects, currentCycleInfo.id, currentCycleInfo.triggersEventID >= 0, -1, isFirst); // [SOS] we do NOT know the distance to peak at this point
                OnPaintMainAnchorObjects();

                // This is when the peak will actually happen
                AsyncThread.RunOnMainThread_NextFrameCycle_TS(() =>
                {
                    t4 = TimeWrapper.lastRenderedFrame_TimeOfRenderMS;
                    float actualDurationMS = (float)(t4 - t3);
                    float wantedDurationMS = mainAnimationDuration_1_to_2 * 1000;
                    if (EngineWrapper.Debug_IsDebugBuild && doDebugParts)
                        Debug.LogError("t4 - t3 from " + wantedDurationMS.ToString("#") + " :: " + Mathf.RoundToInt(actualDurationMS - wantedDurationMS)); // [200514] ACCURATELY MATCHES VIDEO DURATION FOR PEAK ON !!! Note EDITOR and BUILD behave differently (this is accurate in both cases)
                });

                // Wait for some time while Satellites are on Peak
                float waitForSeconds = (float)(satellitePeakDuration - config.numFramesCorrector * PerformanceObserver.averageFrameDT_Seconds);

                yield return StartCoroutine(WaitForTime(waitForSeconds, null)); // Factors in pauses

                // This is when the peak will actually happen
                AsyncThread.RunOnMainThread_NextFrameCycle_TS(() =>
                {
                    double t5 = TimeWrapper.lastRenderedFrame_TimeOfRenderMS;
                    float actualDurationMS = (float)(t5 - t1);
                    float wantedDurationMS = ((float)(currentCycleDuration * 1000));
                    if (EngineWrapper.Debug_IsDebugBuild && doDebugWhole)
                        Debug.LogError("t5 - t1 from " + wantedDurationMS.ToString("#") + " :: " + Mathf.RoundToInt(actualDurationMS - wantedDurationMS)); // [200514] ACCURATELY MATCHES VIDEO DURATION FOR PEAK ON !!! Note EDITOR and BUILD behave differently (this is accurate in both cases)
                });

                currentCycleIdx++;
                isFirst = false;
            }
        }
         
        public static EventHandler<StatusArgs> onStatusUpdate;

        public class StatusArgs : EventArgs
        {
            public int currentAnimEvent_1Based;
            public int totalNumAnimEventsPerCycle;
            public AnimEvent animEventType;
            public string animEventExplanation;
            public bool doDebug;

            public StatusArgs(int currentAnimEvent_1Based, int totalNumAnimEventsPerCycle, AnimEvent animEventType, string animEventExplanation, bool doDebug)
            {
                this.currentAnimEvent_1Based = currentAnimEvent_1Based;
                this.totalNumAnimEventsPerCycle = totalNumAnimEventsPerCycle;
                this.animEventType = animEventType;
                this.animEventExplanation = animEventExplanation;
                this.doDebug = doDebug;
            }
        }

        protected void DrawLines(Vector3 center, int numQuadrantX, int numQuadrantY, float stepX, float stepY)
        {
            Color gridColor = config.gridColor;
            Vector3 leftEdge = (Vector3)center - (Vector3.up * (numQuadrantY * stepY));
            DrawHorizontalLines(leftEdge, numQuadrantX, numQuadrantY * 2, stepX, stepY);

            Vector3 bottomEdge = (Vector3)center - (Vector3.right * (numQuadrantX * stepX));
            DrawVerticalLines(bottomEdge, numQuadrantX * 2, numQuadrantY, stepX, stepY);
        }

        protected void DrawHorizontalLines(Vector3 initPos, int numX, int numY, float stepX, float stepY)
        {
            float width = stepX * numX;

            // Offset half box
            initPos -= Vector3.up * stepY / 2;

            // Create a left of left-right vectors
            List<Vector3> positions = new List<Vector3>();
            for (int j = 0; j < numY; j++)
            {
                Vector3 leftEdge = new Vector3(-width, j * stepY, 0) + initPos;
                Vector3 rightEdge = new Vector3(width, j * stepY, 0) + initPos;
                // We use one line renderer, so ping pong the line
                if (j % 2 == 0)
                {
                    positions.Add(leftEdge);
                    positions.Add(rightEdge);
                }
                else
                {
                    positions.Add(rightEdge);
                    positions.Add(leftEdge);
                }
            }

            // Create line
            CreateLineRenderer(positions.ToArray());
        }

        protected void DrawVerticalLines(Vector3 initPos, int numX, int numY, float stepX, float stepY)
        {
            float height = stepY * numY;

            // Create a left of left-right vectors
            List<Vector3> positions = new List<Vector3>();
            for (int i = 0; i < numX; i++)
            {
                Vector3 bottomEdge = new Vector3(i * stepX, -height, 0) + initPos;
                Vector3 topEdge = new Vector3(i * stepX, height, 0) + initPos;
                // We use one line renderer, so ping pong the line
                if (i % 2 == 0)
                {
                    positions.Add(bottomEdge);
                    positions.Add(topEdge);
                }
                else
                {
                    positions.Add(topEdge);
                    positions.Add(bottomEdge);
                }
            }

            // Create line
            CreateLineRenderer(positions.ToArray());
        }

        protected void CreateLineRenderer(Vector3[] positions)
        {
            Material lineRendererMat = Resources.Load<Material>("GridLineRenderer");
            lineRendererMat.color = config.gridColor;
            LineRenderer lR = new GameObject().AddComponent<LineRenderer>();
            lR.positionCount = positions.Length;
            lR.SetPositions(positions);
            lR.startWidth = lR.endWidth = config.gridWidth;
            lR.startColor = lR.endColor = config.gridColor;
            lR.material = lineRendererMat;
        }

        /*
        private void OnDestroy()
        {
            KoreographyWrapper.onMelody -= KoreographyEventWrapper_onMelody;
        }
        */

        #endregion

        protected BackgroundObject CreateBackgroundObject(Transform parent)
        {
            return CreateBackgroundObject(parent, GetBackgroundType());
        }

        protected BackgroundObject CreateBackgroundObject(Transform parent, BackgroundType type)
        {
            string backgroundObjectResourceName = BACKGROUND_OBJECT_RESOURCE_NAME_FORMAT._Format(type);
            GameObject temp = ResourceHelper.InstantiateResource<GameObject>(backgroundObjectResourceName);
            temp.ReparentAndReset(parent);
            temp.transform.localScale = Vector3.one;
            BackgroundObject bO = temp.GetComponentInChildren<BackgroundObject>();
            backgroundObjects.Add(bO);
            return bO;
        }

        protected virtual void Initialize(Config config, RuntimeConfig runtimeConfig)
        {
            BackgroundManager.config = config;

            this.Log("Init!");

            // Create objects for game type
            CreateBackgroundObjects();

            // Turn everything off!

            BackgroundObject.RuntimeConfig objectConfig = GetBackgroundObjectConfig();

            for (int i = 0; i < backgroundObjects.Count; i++)
                backgroundObjects[i].Initialize(objectConfig);

            if (animateCR != null)
                StopCoroutine(animateCR);

            animateCR = StartCoroutine(AnimateIE(runtimeConfig.timings));
        }

        public static Rect GetActiveAreaWorld()
        {
            // [TODO] Whole screen is the active area
            if (!config.usingActiveAreaAndCropEnabled)
                return new Rect(Vector2.negativeInfinity, Vector2.positiveInfinity);

            return config.GetActiveAreaWorld(Camera.main);
        }

        private static BackgroundObject.RuntimeConfig GetBackgroundObjectConfig()
        {
            return new BackgroundObject.RuntimeConfig(GetActiveAreaWorld());
        }

        // This is the "load-up"
        protected virtual void BeginNewAnimationCycle() { }
        protected virtual List<BackgroundObject> OnPaintSatellites()
        {
            List<BackgroundObject> satellites = GetBackgroundObjects().FindAll(bjObject =>
                                            bjObject.isVisible &&
                                            (bjObject as BackgroundObject_Abstract_RotatingSquares).isSatellite)
                                            .Shuffle().ToList();
            // Paint Exact half of them
            satellites = satellites.GetRange(0, satellites.Count / 2);
            return satellites;
        }

        public event EventHandler<List<BackgroundObject>> onCriticalSquaresPeak;

        protected void RaiseOnCriticalSquaresPeak(List<BackgroundObject> bO)
        {
            onCriticalSquaresPeak?.Invoke(null, bO);
        }

        public event EventHandler<List<BackgroundObject>> onBlobsNeedPainting;

        protected void RaiseOnBlobsNeedPainting(List<BackgroundObject> bO)
        {
            onBlobsNeedPainting?.Invoke(null, bO);
        }

        protected virtual List<BackgroundObject> OnPaintMainAnchorObjects()
        {
            // Select all visible anchor squares - BUT Exclude main stimulus squares (4 center)
            List<BackgroundObject> anchorSquares = GetBackgroundObjects().FindAll(bgObject =>
                                            bgObject.isVisible &&
                                            !(bgObject as BackgroundObject_Abstract_RotatingSquares).isSatellite &&
                                            !bgObject.IsStimulusCandidate())
                                            .Shuffle().ToList();

            // Paint Exact half of them
            anchorSquares = anchorSquares.GetRange(0, anchorSquares.Count / 2);
            return anchorSquares;
        }

        protected virtual void HandleMasterAnimation(float dT) { }
        protected virtual void HandlePeakAnimation_EaseIn(List<BackgroundObject> backgroundObjects, float t01) { }
        protected virtual void HandlePeakAnimation_Main(List<BackgroundObject> objectsToPeak, float t01) { }
        protected virtual void HandlePeakAnimation_EaseOut(List<BackgroundObject> objectsToPeak, float t01) { }

        protected virtual void CreateBackgroundObjects() { }

        protected virtual BackgroundType GetBackgroundType() { return default(BackgroundType); }

        protected virtual void InitializeBackgroundObjectPositions(List<BackgroundObject> backgroundObjects) { }

        /// <summary>
        /// [SEGMENT]
        /// </summary>
        public class AnimEventArgs : EventArgs
        {
            public bool isFirstCycle;
            public int backgroundCycleIdx;
            /// <summary>
            /// [SOS] Only works for PRECALCULATED timings (ie. game, not replays)
            /// </summary>
            public bool isStimulusPeak;
            public List<BackgroundObject> visibleObjects;
            /// <summary>
            /// [HACK, 200501] Wanted to predict distance from Painting to Peak Begin (Stim Onset)
            /// </summary>
            public double distanceToPeakBeginS;
            public AnimEvent animEvent;
        }

        public BackgroundObject GetObjectFromDirection(Direction_2D_Diagonal direction)
        {
            return directionToObject.TryGet(direction);
        }

        public Direction_2D_Diagonal GetObjectsDirection(BackgroundObject bO)
        {
            foreach (KeyValuePair<Direction_2D_Diagonal, BackgroundObject> keyValuePair in directionToObject)
            {
                if (keyValuePair.Value == bO)
                    return keyValuePair.Key;
            }
            this.LogWarning("Couldn't find any direction for object:" + bO.name);
            return Direction_2D_Diagonal.BottomLeft;
        }

        // Call that when you creeate the four main squares
        protected void SetObjectToDirection(BackgroundObject bO, Direction_2D_Diagonal dir)
        {
            directionToObject.AddOrUpdate(dir, bO);
        }

        private void Update()
        {
            // if (Input.GetKeyUp(KeyCode.F9)) doDebugParts = Debug.isDebugBuild && !doDebugParts;
        }

        [Serializable]
        public class Config : BackgroundConfig
        {
        }
    }

    public enum BackgroundType { Abstract, Abstract_RotatingSquares, Cockpit, Abstract_BlinkingSquares }
    public enum AnimEvent { CycleBegin = 0, PeakBegin = 1, PeakEnd = 2, CycleEnd = 3 }
}