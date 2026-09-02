// NS_REMOVE | Config, Logs, Blobs
using ExperimentLibrary;

// NS_DEBATABLE | GameTypes, Levels (Maybe send higher up?)
using Game.Core;

using System;
using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;
using Experiment.Background;
using Helpers.Engine;
using Helpers.Async; // For running on NEXT FRAME
using Experiment.Managers;

namespace Experiment.Stimulus
{
    /// <summary>
    /// [SEGMENT] QueueCreation (PRECALCULATOR?) | QueueAnalysis (CHECKER?) | In-Game Events (STIMULUS MANAGER?)
    /// [CLEANUP]
    /// </summary>
    public class StimulusManager
    {
        public const string EMPTY_STIM_NAME = "NULL";

        public static readonly List<Sprite> blobImagesFaces = new List<Sprite>();
        public static readonly List<Sprite> blobImagesObjects = new List<Sprite>();

        protected RuntimeConfig runtimeConfig;
        protected Config config;

        public StimulusManager(Config config, RuntimeConfig runtimeConfig)
        {
            this.config = config;
            this.runtimeConfig = runtimeConfig;

            if (config.background.useBlobs)
                StimulusManager_AssetsManager.LoadBlobs(blobImagesFaces, blobImagesObjects);

            // -- BACKGROUND
            backgroundManager = BackgroundManager.InitializeAndReturnCurrent(config.background, runtimeConfig.background);

            if (backgroundManager != null)
            {
                // Calculate the min/max
                // MinMax backgroundMinMax = ApplicationLibrary.Config.GetPeriodMinMax_Background();

                // Handle both backgrounds
                backgroundManager.onAnimEvent += BackgroundManager_onAnimEvent;
                backgroundManager.onCriticalSquaresPeak += BackgroundManager_onCriticalSquaresPeak;
                backgroundManager.onBlobsNeedPainting += BackgroundManager_onBlobsNeedPainting;
                // backgroundManager.onSinglePeakBegin += BackgroundManager_onPeakBegin;
            }
            else
            {
                this.LogWarning("No background manager");
            }

            numStimuli = 0;

            isTutorialQueue = false;

            if (runtimeConfig.level.isTutorial)
            {
                StimulusManager_QueueGenerator.CreateTutorialQueue_NEW(runtimeConfig.timings,
                    stimuliTexturesFaces, stimuliTexturesObjects, orderedStimuli_Tutorial);
                isTutorialQueue = true;
            }
        }

        public static LocalizerInfo GetWantedReplayInfo(LevelConfig level)//, bool isFMRI)
        {
            if (!level.isLocalizer)
                return LocalizerInfo.EMPTY;

            return localizerInfos[level.localizerID_0Based];
        }

        public static void ReadInQueues(IList<string> queueContents, Action<bool> callback)
        {
            StimulusManager_AssetsManager.Initialize(stimuliTexturesFaces, stimuliTexturesObjects, stimulusNameToID, assetsLoadedSuccessfully =>
            {
                if (!assetsLoadedSuccessfully)
                {
                    Debug.LogError("Stimulus Assets not loaded!");
                    callback(false);
                    return;
                }

                StimulusManager_QueueReaderManager.TrySetupQueues(queueContents,
                    stimuliTexturesFaces, stimuliTexturesObjects,
                    orderedStimuli_GameWorlds, orderedStimuli_Localizers,
                    queuesSetupSuccessfully =>
                    {
                        if (!queuesSetupSuccessfully)
                        {
                            Debug.LogError("Stimulus Queues not setup!");
                            callback(false);
                            return;
                        }

                        callback(true);
                    });
            });
        }

        public static void GenerateQueues_NEW(bool isFMRI, int subjectID, List<EventInformation> timingSequence, int numGameWorlds, int numLevelsPerWorld, Action<bool> callback)
        {
            List<List<EventType>> sequence_PerGameWorld = new List<List<EventType>>();
            for (int i = 0; i < numGameWorlds; i++)
                sequence_PerGameWorld.Add(new List<EventType>());

            int numLevels = numGameWorlds * numLevelsPerWorld;
            int currentWorldID_0Based = 0;

            List<int> localizerStartIDInSequenceOfWorld = new List<int>();
            Dictionary<int, Dictionary<int, int>> worldToLevelStartPointsWithinWorld_0Based = new Dictionary<int, Dictionary<int, int>>();

            Action<int> handleLevelBegin = levelBegin_LevelID_0Based =>
            {
                currentWorldID_0Based = Mathf.FloorToInt(levelBegin_LevelID_0Based / numLevelsPerWorld);
                // Debug.Log(levelBegin_LevelID_0Based + " : " + currentWorldID_0Based);
                // How many we've had so far (start at the next)
                int startIDInSequenceOfWorld = sequence_PerGameWorld[currentWorldID_0Based].Count;

                if (!worldToLevelStartPointsWithinWorld_0Based.ContainsKey(currentWorldID_0Based))
                    worldToLevelStartPointsWithinWorld_0Based.Add(currentWorldID_0Based, new Dictionary<int, int>());

                int levelID_WithinWorld_0Based = levelBegin_LevelID_0Based - currentWorldID_0Based * numLevelsPerWorld;

                if (!worldToLevelStartPointsWithinWorld_0Based[currentWorldID_0Based].ContainsKey(levelID_WithinWorld_0Based))
                    worldToLevelStartPointsWithinWorld_0Based[currentWorldID_0Based].Add(levelID_WithinWorld_0Based, startIDInSequenceOfWorld);

                // If this new level is a localizer (0-based localizer levels :: 1, 3, 5, ..)
                bool isLocalizer = ExperimentManagerSession.IsIntendedForReplay(levelBegin_LevelID_0Based + 1);
                if (isLocalizer)
                    localizerStartIDInSequenceOfWorld.Add(startIDInSequenceOfWorld);
            };

            handleLevelBegin(0);

            foreach (EventInformation eI in timingSequence)
            {
                if (eI.eventType == EventType.Stimulus.ToString())
                {
                    // Unprobed Stimulus
                    if (eI.triggersEventID < 0)
                        sequence_PerGameWorld[currentWorldID_0Based].Add(EventType.Stimulus);
                    else
                        sequence_PerGameWorld[currentWorldID_0Based].Add(EventType.Probe);
                }
                else if (eI.eventType == EventType.Level.ToString())
                {
                    // Unpacking a bit what happens
                    int levelEnd_levelID_1Based = eI.id;

                    // If it was the last level that ended, don't do anything 
                    if (levelEnd_levelID_1Based >= numLevels)
                    {
                        // Debug.LogWarning("Processed Last level " + levelEnd_levelID_1Based);
                    }
                    else
                    {
                        int levelBegin_levelID_1Based = levelEnd_levelID_1Based + 1;
                        int levelBegin_levelID_0Based = levelBegin_levelID_1Based - 1;

                        handleLevelBegin(levelBegin_levelID_0Based);
                    }
                }
            }

            StimulusManager_AssetsManager.Initialize(stimuliTexturesFaces, stimuliTexturesObjects, stimulusNameToID, assetsLoadedSuccessfully =>
            {
                if (!assetsLoadedSuccessfully)
                {
                    Debug.LogError("Stimulus Assets not loaded!");
                    callback(false);
                    return;
                }

                bool doDebug = false;

                Dictionary<int, QueueScore> scorePerWorld = new Dictionary<int, QueueScore>();
                orderedStimuli_GameWorlds.Clear();
                orderedStimuli_Localizers.Clear();
                StimulusManager_QueueGeneratorManager.TrySetupQueues_NEW(isFMRI, subjectID,
                    sequence_PerGameWorld, localizerStartIDInSequenceOfWorld,
                    stimuliTexturesFaces, stimuliTexturesObjects,
                    orderedStimuli_GameWorlds, orderedStimuli_Localizers, scorePerWorld,
                    worldToLevelStartPointsWithinWorld_0Based,
                    success =>
                    {
                        if (!success)
                        {
                            Debug.LogError("Stimulus Queues not setup!");
                            callback(false);
                            return;
                        }

                        LogQueues_NEW();

                        if (doDebug)
                        {
                            QueueScore scoreFinal = QueueScore.DEFAULT;

                            foreach (QueueScore qS in scorePerWorld.Values)
                                scoreFinal += qS;

                            scoreFinal /= scorePerWorld.Count;

                            Debug.LogWarning("===== TWO-WORLD CHECKS =====");

                            // Per 2 worlds
                            int numTwoWorldPairs = orderedStimuli_GameWorlds.Count / 2;
                            for (int twoWIndex = 0; twoWIndex < numTwoWorldPairs; twoWIndex++)
                            {
                                Debug.LogWarning(twoWIndex);

                                // Final Checks!
                                Debug.LogWarning("===== REPLAYS =====");

                                List<SpriteLocation> twoWorldsReplays = new List<SpriteLocation>();
                                twoWorldsReplays.AddRange(orderedStimuli_Localizers[twoWIndex * 2]);
                                twoWorldsReplays.AddRange(orderedStimuli_Localizers[twoWIndex * 2 + 1]);
                                twoWorldsReplays.AddRange(orderedStimuli_Localizers[twoWIndex * 2 + 2]);
                                twoWorldsReplays.AddRange(orderedStimuli_Localizers[twoWIndex * 2 + 3]);

                                scoreFinal.twoWorldReplay += StimulusManager_QueueGenerator.DoAnalysis100(twoWorldsReplays, 100, false);

                                Debug.LogWarning("===== GAME =====");

                                List<SpriteLocation> twoWorldsGame = new List<SpriteLocation>();
                                twoWorldsGame.AddRange(orderedStimuli_GameWorlds[twoWIndex * 2]);
                                twoWorldsGame.AddRange(orderedStimuli_GameWorlds[twoWIndex * 2 + 1]);

                                Debug.LogWarning("===== PROBED =====");
                                scoreFinal.twoWorldProbed += StimulusManager_QueueGenerator.DoAnalysis100(twoWorldsGame.FindAll(a => a.DEBUG_IS_PROBED), 100, false);

                                Debug.LogWarning("===== UNPROBED =====");
                                scoreFinal.twoWorldUnprobed += StimulusManager_QueueGenerator.DoAnalysis200(twoWorldsGame.FindAll(a => !a.DEBUG_IS_PROBED), 200, true);

                                Debug.LogWarning("===== PROBED + UNPROBED =====");
                                scoreFinal.twoWorldTotal += StimulusManager_QueueGenerator.DoAnalysis300(twoWorldsGame, 300, true);
                            }

                            scoreFinal.twoWorldReplay /= numTwoWorldPairs;
                            scoreFinal.twoWorldProbed /= numTwoWorldPairs;
                            scoreFinal.twoWorldUnprobed /= numTwoWorldPairs;
                            scoreFinal.twoWorldTotal /= numTwoWorldPairs;

                            Debug.LogWarning("===== TOTAL CHECKS =====");
                            Debug.LogWarning("===== REPLAYS =====");

                            // Final Checks!
                            List<SpriteLocation> cumulativeLocalizers = new List<SpriteLocation>();
                            foreach (List<SpriteLocation> lSL in orderedStimuli_Localizers)
                                cumulativeLocalizers.AddRange(lSL);

                            if (orderedStimuli_Localizers.Count == 4)
                                scoreFinal.allReplay += StimulusManager_QueueGenerator.DoAnalysis100(cumulativeLocalizers, 100, false);
                            else if (orderedStimuli_Localizers.Count == 8)
                                scoreFinal.allReplay += StimulusManager_QueueGenerator.DoAnalysis200(cumulativeLocalizers, 200, false);

                            List<SpriteLocation> cumulativeGameWorlds = new List<SpriteLocation>();
                            foreach (List<SpriteLocation> lSL in orderedStimuli_GameWorlds)
                                cumulativeGameWorlds.AddRange(lSL);

                            Debug.LogWarning("===== GAME =====");

                            Debug.LogWarning("===== PROBED =====");
                            if (orderedStimuli_GameWorlds.Count == 2)
                                scoreFinal.allProbed += StimulusManager_QueueGenerator.DoAnalysis100(cumulativeGameWorlds.FindAll(a => a.DEBUG_IS_PROBED), 100, false);
                            else if (orderedStimuli_GameWorlds.Count == 4)
                                scoreFinal.allProbed += StimulusManager_QueueGenerator.DoAnalysis200(cumulativeGameWorlds.FindAll(a => a.DEBUG_IS_PROBED), 200, false);

                            Debug.LogWarning("===== UNPROBED =====");
                            if (orderedStimuli_GameWorlds.Count == 2)
                                scoreFinal.allUnprobed += StimulusManager_QueueGenerator.DoAnalysis200(cumulativeGameWorlds.FindAll(a => !a.DEBUG_IS_PROBED), 200, true);
                            else if (orderedStimuli_GameWorlds.Count == 4)
                                scoreFinal.allUnprobed += StimulusManager_QueueGenerator.DoAnalysis400(cumulativeGameWorlds.FindAll(a => !a.DEBUG_IS_PROBED), 400, true);

                            Debug.LogWarning("===== PROBED + UNPROBED =====");
                            if (orderedStimuli_GameWorlds.Count == 2)
                                scoreFinal.allTotal += StimulusManager_QueueGenerator.DoAnalysis300(cumulativeGameWorlds, 300, true);
                            if (orderedStimuli_GameWorlds.Count == 4)
                                scoreFinal.allTotal += StimulusManager_QueueGenerator.DoAnalysis600(cumulativeGameWorlds, 600, true);

                            LogAnalysis(scoreFinal, scorePerWorld);
                        }

                        callback(true);
                    });
            });
        }

        private static void LogQueues_NEW()
        {
            string queueCSV = "World;Level;Type;ID;Location;IsProbed;ReplayIDWithinWorld\r\n";

            for (int worldIndex_0Based = 0; worldIndex_0Based < orderedStimuli_GameWorlds.Count; worldIndex_0Based++)
                for (int stimIndexWithinWorld_0Based = 0; stimIndexWithinWorld_0Based < orderedStimuli_GameWorlds[worldIndex_0Based].Count; stimIndexWithinWorld_0Based++)
                {
                    SpriteLocation sL = orderedStimuli_GameWorlds[worldIndex_0Based][stimIndexWithinWorld_0Based];
                    // World	Level	Type	ID	Location IS PROBED
                    string ID = sL.type == StimulusType.None ? "" : sL.spriteName.Substring(2, 2);

                    queueCSV += "{0};{1};{2};{3};{4};{5};{6}\r\n"._Format(worldIndex_0Based, sL.DEBUG_LEVEL_ID_WITHIN_WORLD, sL.type, ID, sL.direction, sL.DEBUG_IS_PROBED, sL.DEBUG_REPLAY_ID_WITHIN_WORLD);
                }

            for (int i = 0; i < orderedStimuli_Localizers.Count; i++)
                foreach (SpriteLocation sL in orderedStimuli_Localizers[i])
                {
                    // World	Level	Type	ID	Location
                    string ID = sL.type == StimulusType.None ? "" : sL.spriteName.Substring(2, 2);
                    queueCSV += "{0};{1};{2};{3};{4};{5};{6}\r\n"._Format('L', i, sL.type, ID, sL.direction, sL.DEBUG_IS_PROBED, sL.DEBUG_REPLAY_ID_WITHIN_WORLD);
                }

            ExperimentManagerSession.LogStimulusQueueCSV(queueCSV);
        }

        private static void LogAnalysis(QueueScore scoreFinal, Dictionary<int, QueueScore> scorePerWorld)
        {
            string analysisCSV = "WORLD;" + QueueScore.HEADERS_CSV;

            analysisCSV += "\nALL;" + scoreFinal.ToCSV();
            foreach (KeyValuePair<int, QueueScore> scoreWorld in scorePerWorld)
                analysisCSV += "\n" + scoreWorld.Key + ";" +  scoreWorld.Value.ToCSV();

            Debug.LogError(analysisCSV);

            ExperimentManagerSession.LogStimulusQueueAnalysisCSV(analysisCSV);
        }

        public static void GenerateQueues_OLD(Action<bool> callback)
        {
            StimulusManager_AssetsManager.Initialize(stimuliTexturesFaces, stimuliTexturesObjects, stimulusNameToID, assetsLoadedSuccessfully =>
            {
                if (!assetsLoadedSuccessfully)
                {
                    Debug.LogError("Stimulus Assets not loaded!");
                    callback(false);
                    return;
                }

                StimulusManager_QueueGeneratorManager.TrySetupQueues_OLD(
                    stimuliTexturesFaces, stimuliTexturesObjects,
                    orderedStimuli_GameWorlds, orderedStimuli_Localizers,
                    queuesSetupSuccessfully =>
                    {
                        if (!queuesSetupSuccessfully)
                        {
                            Debug.LogError("Stimulus Queues not setup!");
                            callback(false);
                            return;
                        }

                        LogQueues_OLD();

                        callback(true);
                    });
            });
        }

        private static void LogQueues_OLD()
        {
            string queueCSV = "World;Level;Type;ID;Location;IsProbed\r\n";

            for (int i = 0; i < orderedStimuli_GameWorlds.Count; i++)
                foreach (SpriteLocation sL in orderedStimuli_GameWorlds[i])
                {
                    // World	Level	Type	ID	Location IS PROBED
                    string ID = sL.type == StimulusType.None ? "" : sL.spriteName.Substring(2, 2);
                    queueCSV += "{0};{1};{2};{3};{4};{5}\r\n"._Format(i, "", sL.type, ID, sL.direction, sL.DEBUG_IS_PROBED);
                }
            for (int i = 0; i < orderedStimuli_Localizers.Count; i++)
                foreach (SpriteLocation sL in orderedStimuli_Localizers[i])
                {
                    // World	Level	Type	ID	Location
                    string ID = sL.type == StimulusType.None ? "" : sL.spriteName.Substring(2, 2);
                    queueCSV += "{0};{1};{2};{3};{4};{5}\r\n"._Format('L', i, sL.type, ID, sL.direction, sL.DEBUG_IS_PROBED);
                }

            ExperimentManagerSession.LogStimulusQueueCSV(queueCSV);
        }

        // SHOULD NOT BE PUBLIC
        public static BackgroundManager backgroundManager { get; private set; }

        #region SESSSION

        public static LocalizerInfo GetLevelStimulusTarget(LevelConfig levelConfig)
        {
            if (!levelConfig.isLocalizer)
            {
                Debug_Helper.LogError(typeof(StimulusManager), "This is not a localizer level, but something tries to read the stimulus target");
                return LocalizerInfo.EMPTY;
            }

            int localizerID = levelConfig.localizerID_0Based;
            return localizerInfos[localizerID];
        }

        public static void ReadInLocalizers(IList<string> csvContent, int subjectID_0Based)
        {
            localizerInfos.Clear();

            int choiceIndexTarget = Mathf.FloorToInt(subjectID_0Based / 2) % 16;
            List<StimulusType> REPLAY_ID_0BASED_TO_TARGET = new List<StimulusType>(SUBJECT_ID_DIV_2_TO_REPLAY_ID_0BASED_TO_TARGET[choiceIndexTarget]);

            // Read in all Game Types and Stim Types
            for (int i = 0; i < csvContent.Count; i++)
            {
                string line = csvContent[i];

                LocalizerInfo localizerInfo = LocalizerInfo.FromCSV(line);
                localizerInfo.target = REPLAY_ID_0BASED_TO_TARGET[i];

                localizerInfos.Add(localizerInfo);
            }
        }

        public static readonly List<LocalizerInfo> localizerInfos = new List<LocalizerInfo>();// { StimulusType.Face, StimulusType.Face, StimulusType.Object, StimulusType.Object };

        public static void RandomizeLocalizers(int subjectID_0Based, int numReplays)
        {
            bool doDebug = false;

            localizerInfos.Clear();

            int choiceIndexLevel = subjectID_0Based % 2;
            List<int> REPLAY_ID_0BASED_TO_LEVEL_ID_1BASED = new List<int>(SUBJECT_ID_TO_REPLAY_ID_0BASED_TO_LEVEL_ID_1BASED[choiceIndexLevel]);

            for (int i = 0; i < numReplays; i++)
                localizerInfos.Add(new LocalizerInfo(REPLAY_ID_0BASED_TO_LEVEL_ID_1BASED[i], StimulusType.None));

            string csv = LocalizerInfo.ToCSV(localizerInfos);

#if UNITY_EDITOR
            if (doDebug) Debug.LogError(subjectID_0Based + "\n" + csv);
#endif
            
            ExperimentManagerSession.LogLocalizersCSV(csv);
        }
        #endregion

        // private void Update() { Debug.Log("[{3}] Stim :: {0} ({1}, {2})"._Format(lastStimulusName, lastStimulusLocation, isLastStimulusProbed ? "PROBED" : "NON-PROBED", TimeWrapper.currentTimestampMS)); }

        /// <summary>
        /// Maybe used instead of <see cref="currentCycleIdx"/> (diff update timing)
        /// </summary>
        public static int lastCyclceIdx;

        /// <summary>
        /// True from Selection, throughout Peak (incl. PeakBegin, PeakEnd) until Hiding
        /// </summary>
        public static bool isShowingStimulus { get { return lastStimulus != null; } }

        private static bool isLastStimulusProbed;
        private static SpriteLocation? lastStimulus = null;

        private static StimulusType? lastStimulusType { get { return lastStimulus?.type; } }
        private static BackgroundObject lastStimulusBackgroundObject { get { return GetObjectFromDirection(lastStimulusLocation); } }
        private static Direction_2D_Diagonal? lastStimulusLocation { get { return lastStimulus?.direction; } }
        private static string lastStimulusName { get { return lastStimulus?.spriteName; } }
        private static Sprite lastStimulusSprite { get { return lastStimulus?.sprite; } }

        /// <summary>
        /// [SOS] Is updated early in the cycle FOLLOWING the rendered frame that contained this stim.
        /// [SOS] If you need this value asap, consider using <see cref="TimeWrapper.lastRenderedFrame_TimeOfRenderMS"/> instead, as it updates before
        /// </summary>
        public static double lastTimeStimulusShown_MS { get; private set; }
        public static double lastTimeStimulusShown_MS_RunElapsed_NoPauses { get; private set; }

        /// <summary>
        /// Called where the stimulus is considered "ON"
        /// </summary>
        public event EventHandler<StimulusEventArgs> onStimulusOnset;
        /// <summary>
        /// Called where the stimulus is OFF and the square back at its default position
        /// </summary>
        public event EventHandler<StimulusEventArgs> onStimulusClear;

        /// <summary>
        /// Called where the stimulus is considered "ON"
        /// </summary>
        public event EventHandler<BackgroundManager.AnimEventArgs> onAnimEvent;

        public static bool isLocked { get; private set; }

        // [SerializeField, Range(1, 25)] private int avgSecondsPerStimulus = 15;
        // [SerializeField, Range(100, 10000)] private int millisecondsPerStimulus = 5000;
        // [SerializeField, Range(0, 1)] private float maxPercentileDeviation = 0.2f;
        // [SerializeField, Range(0.1f, 1.0f)] private float stimulusAlpha = 0.6f;

        private int numStimuli = 0;
        private bool isPaused = false;
        private static readonly Dictionary<string, int> stimulusNameToID = new Dictionary<string, int>();
        protected static readonly List<Sprite> stimuliTexturesFaces = new List<Sprite>();
        protected static readonly List<Sprite> stimuliTexturesObjects = new List<Sprite>();

        protected List<SpriteLocation> orderedStimuli_Tutorial = new List<SpriteLocation>();

        protected static readonly List<List<SpriteLocation>> orderedStimuli_GameWorlds = new List<List<SpriteLocation>>();
        protected static readonly List<List<SpriteLocation>> orderedStimuli_Localizers = new List<List<SpriteLocation>>();
        private double windowStartTS = Mathf.Infinity;
        private double windowEndedTS = Mathf.NegativeInfinity;
        protected bool isTutorialQueue = false;
        
        /*
        public enum RelevantPosition { Before, Within, After }

        public RelevantPosition GetPositionWRTWindow(double timestampMS)
        {
            return
                timestampMS > windowEndedTS ? RelevantPosition.After :
                timestampMS < windowStartTS ? RelevantPosition.Before : RelevantPosition.Within;
        }
        */

        public static int GetStimIDFromName_0Based(string name)
        {
            // Don't parse blank stumulus
            if (name == "NULL")
                return -1;

            if (!stimulusNameToID.ContainsKey(name))
                return -1;

            return stimulusNameToID[name];
        }

        // [DEBUG] Average stimiulus delay times
        public List<double> times = new List<double>();

        public static int currentCycleIdx { get; private set; }
        protected static int cycleIdxAtLevelStart { get; private set; }

        public static void SetCycleIdx(int cycleIdx)
        {
            currentCycleIdx = cycleIdx;
            cycleIdxAtLevelStart = currentCycleIdx;
        }
        
        protected double nextStimTimestampMS = Mathf.Infinity;

        public void OnReplayLevelStart()
        {
            PickNextStimTimestampMS(TimeWrapper.currentFrameCycleBeginMS);
        }

        private void BackgroundManager_onBlobsNeedPainting(object sender, List<BackgroundObject> e)
        {
            PaintBlobs(e);
        }

        private void PaintBlobs(List<BackgroundObject> objectsToPaint)
        {
            // Paint peak object with distractive images
            if (config.background.useBlobs)
            {
                // Randomly paint all squares
                for (int i = 0; i < objectsToPaint.Count; i++)
                {
                    objectsToPaint[i].UnPaint();

                    // List already shuffled. Just paint them alternatively
                    bool isFace = i % 2 == 0;
                    if (isFace)
                        objectsToPaint[i].SetOverlay(blobImagesFaces.GetRandom());
                    else
                        objectsToPaint[i].SetOverlay(blobImagesObjects.GetRandom());
                }
            }
        }

        private void BackgroundManager_onCriticalSquaresPeak(object sender, List<BackgroundObject> e)
        {
            PaintMainStimulusSquares(e);
        }

        // Should be moved called separately , unlinked from OnPaintMainAnchorObjects
        private void PaintMainStimulusSquares(List<BackgroundObject> stimulusSquares)
        {
            // Ready them to be painted
            Dictionary<BackgroundObject, StimulusType> stimuliSquareTypes = new Dictionary<BackgroundObject, StimulusType>();

            // Get all main stimulus squares

            // [Mike] Let's always have half of the 4 key locations have face blobs/stim and half have object blobs/stim 
            List<StimulusType> stimuliTypesToPaint = new List<StimulusType>() { StimulusType.Face, StimulusType.Face, StimulusType.Object, StimulusType.Object };

            foreach (BackgroundObject bO in stimulusSquares)
            {
                StimulusType stimulusToPaint = StimulusType.None;

                // For stim trials we already know which thing to stim
                if (bO == lastStimulusBackgroundObject &&
                    // If it's either a Face or an Object
                    lastStimulusType == StimulusType.None)
                    // then paint that
                    stimulusToPaint = StimulusType.None;

                // Otherwise, pick a random one!
                else
                    stimulusToPaint = stimuliTypesToPaint.GetRandom();

                stimuliTypesToPaint.Remove(stimulusToPaint);
                stimuliSquareTypes.Add(bO, stimulusToPaint);
            }

            List<string> stimuliPainted = new List<string>();
            List<string> stimDirections = new List<string>();

            foreach (KeyValuePair<BackgroundObject, StimulusType> kVP in stimuliSquareTypes)
            {
                Sprite stimulusToPaint =
                    kVP.Key == lastStimulusBackgroundObject ? lastStimulusSprite :
                    kVP.Value == StimulusType.Face ? blobImagesFaces.GetRandom() :
                    kVP.Value == StimulusType.Object ? blobImagesObjects.GetRandom() : null;

                if (stimulusToPaint != null)
                    kVP.Key.SetOverlay(stimulusToPaint);

                stimuliPainted.Add(stimulusToPaint ? stimulusToPaint.name : "NULL");
                stimDirections.Add(kVP.Key.GetDirection().ToString());
            }


            bool isLocalizer = runtimeConfig.level.isLocalizer;

            string trialType = "{0}_{1}"._Format(isLocalizer ? "LOCALIZER" : "GAME",
                !lastStimulusBackgroundObject ? "FILLER" :
                isLastStimulusProbed ? "PROBE" : "STIMULUS");

            string log = "PAINTING_CENTRAL_SQUARES;{0};{1};{2};{3};{4};[0] {5}, [1] {6}, [2] {7}, [3] {8}"._Format(trialType,
                stimuliPainted[0], stimuliPainted[1], stimuliPainted[2], stimuliPainted[3],
                stimDirections[0], stimDirections[1], stimDirections[2], stimDirections[3]);

            // Debug.LogError(log);
            ExperimentManagerSession.LogData_AtNextRenderedFrame_TS("BACKGROUND_MANAGER", log);
        }

        private void PickNextStimTimestampMS(double previousStimTimestampMS)
        {
            // Rand works better on wider ranges, so multi the range rather than the result
            double randomOffsetMS = ExperimentLibraryManager.Config.GetPeriodRandom_Stimulus(
                runtimeConfig.level.isLocalizer, ExperimentManagerSession.module == ModuleType.FMRI_Scanner) * 1000f;

            // Account for the fact that we UNLOCK the POSSIBILITY of painting, (instead of actually painting)
            // Intuitively, if we could paint when nextStimeTimestampMS happened, we'd be fine like that
            // But because we will paint on the next available background cycle, that is going to be from
            // 0 to avg seconds from now on average, so avg / 2
            double averageDelayInActualizationMS = ExperimentLibraryManager.Config.GetPeriodMinMax_Background().avg / 2 * 1000;
            double correctorMS = config.backgroundToStimulus_Localizer_CorrectorMS;
            double frameMS = config.backgroundToStimulus_Localizer_CorrectorAddFrame ?
                PerformanceObserver.averageFrameDT_Seconds * 1000 : 0;

            nextStimTimestampMS = previousStimTimestampMS + randomOffsetMS - averageDelayInActualizationMS + correctorMS + frameMS;

            /*
            Debug.LogError("[{0}] Set next stim for {1} ({2}ms later)"._Format(
                previousStimTimestampMS.ToString("#"), 
                nextStimTimestampMS.ToString("#"), 
                randomOffsetMS.ToString("#")));
            */
        }
        
        private void OnDestroy()
        {
            if (backgroundManager)
            {
                backgroundManager.onAnimEvent -= BackgroundManager_onAnimEvent;
                backgroundManager.onCriticalSquaresPeak -= BackgroundManager_onCriticalSquaresPeak;
                backgroundManager.onBlobsNeedPainting -= BackgroundManager_onBlobsNeedPainting;
            }

            // [DEBUG] Average stimiulus delay times
            // 1st entry, bad entry
            if (times.Count > 0)
                times.RemoveAt(0);
            double averageDelay = 0;
            for (int i = 0; i < times.Count; i++)
                averageDelay += times[i];
            averageDelay /= times.Count;
            this.LogWarning("Average Delay Between Stimulus (s):" + averageDelay / 1000);
        }

        private static BackgroundObject GetObjectFromDirection(Direction_2D_Diagonal? direction)
        {
            if (!direction.HasValue) return null;
            return backgroundManager.GetObjectFromDirection(direction.Value);
        }

        private void BackgroundManager_onAnimEvent(object sender, BackgroundManager.AnimEventArgs e)
        {
            onAnimEvent?.Invoke(this, e);

            if (isPaused) return;

            bool shouldFireTriggers = !e.isFirstCycle;

            if (e.animEvent == config.animEvent_StimulusSelection)
                SelectStimulus(e);

            if (e.animEvent == config.animEvent_StimulusOnset)
                StimulusOnset(shouldFireTriggers);

            if (e.animEvent == config.animEvent_StimulusOffset)
                StimulusOffset();

            if (e.animEvent == config.animEvent_StimulusClear)
                StimulusClear(shouldFireTriggers);

            // Debug.Log(currentCycleIdx + " : " + e.animEvent);
        }

        // 1 of 4. CYCLE BEGIN -- CHOOSE STIMULUS
        private void SelectStimulus(BackgroundManager.AnimEventArgs e)
        {
            SpriteLocation? spriteLocation = null;
            bool isGameProbeStimulus = false;

            GetStimulusToShow(e, out spriteLocation, out isGameProbeStimulus);

            if (spriteLocation.HasValue)
                // And show it 
                // Careful, this also increments the cycle idx!
                PaintStimulus(spriteLocation.Value, isGameProbeStimulus);

            // Else paint blobs
        }

        protected virtual void GetStimulusToShow(BackgroundManager.AnimEventArgs e, out SpriteLocation? spriteLocation, out bool isGameProbeStimulus)
        {
            spriteLocation = null;
            isGameProbeStimulus = false;
        }

        // 2 of 4. PEAK BEGUN - STIM FULL CONTRAST (ONSET)
        private void StimulusOnset(bool shouldFireTriggers)
        {
            if (!lastStimulus.HasValue) return;

            StimulusType _lastStimulusType = lastStimulusType.Value; // Should have value if lastSTimulus isnt null

            // At this stage, the stimulus is at its peak
            lastCyclceIdx = currentCycleIdx;

            this.Log("Showing Stimulus! {0}"._Format(_lastStimulusType).Colorize(_lastStimulusType == StimulusType.None ? Color.white : _lastStimulusType == StimulusType.Face ? Color.green : Color.yellow));

            string nameString = lastStimulusSprite != null ? "({0})"._Format(lastStimulusSprite.name) : "";

            string worldLevelTrial = "{0}_{1}"._Format(runtimeConfig.level.levelName, numStimuli);
            bool _isLastStimulusProbed = isLastStimulusProbed;
            int _currentCycleIdx = currentCycleIdx; // Store it because it will be incremented soon

            Vector3 position = lastStimulusBackgroundObject.transform.position;

            // Fire off the events as they will need to raise their own things
            RaiseOnStimulusOnset(_currentCycleIdx, lastStimulusBackgroundObject, lastStimulus.Value, _isLastStimulusProbed, worldLevelTrial, shouldFireTriggers);

            numStimuli++;

            // Debug.LogError("WILL SHOW STIM NEXT FRAME FRAME # " + SubjectPerformanceReport.currentFrameCycle);

            // Stim will ACTUALLY get shown on the next frame
            // That's when we log it
            AsyncThread.RunOnMainThread_NextFrameCycle_TS(() =>
            {
                double lastRenderedFrame_TimeOfRenderMS = TimeWrapper.lastRenderedFrame_TimeOfRenderMS;
                PickNextStimTimestampMS(lastRenderedFrame_TimeOfRenderMS);
                // Debug.LogError("DT SINCE LAST SHOWN STIMULUS :: " + (currentTimestampMS - lastTimeHere).ToString("#")); lastTimeHere = currentTimestampMS;

                ExperimentManagerSession.LogStimulusShown(_lastStimulusType);
                // [SOS] We're already running on the NEXT frame cycle - log instantly
                string data = string.Format("SHOWING_STIMULUS;{0};{1};{2};{3};{4};{5}", // "[0] worldLevelTrial, [1] stimulusTypeName, [2] worldPosition, [3] screenPosition, [4] isProbed, [5] stimulusOnsetTimestampMS"
                        worldLevelTrial, _lastStimulusType + nameString, position, Utility_Helper.GetRelativeScreenPosition(position), _isLastStimulusProbed, lastRenderedFrame_TimeOfRenderMS);

                ExperimentManagerSession.LogData_AsTheyHappen_TS(TimeWrapper.GetLastFrameTimestamp_TS(), "Stimulus Manager", data);

                // [DEBUG] Stimulus delays times
                times.Add(lastRenderedFrame_TimeOfRenderMS - lastTimeStimulusShown_MS);
                lastTimeStimulusShown_MS = lastRenderedFrame_TimeOfRenderMS; // When it ACTUALLY shows on screen
                lastTimeStimulusShown_MS_RunElapsed_NoPauses = TimeWrapper.lastRenderedFrame_TimeOfRender_SessionPlayTime_MS_NoPauses;

                // Debug.LogError("LOGGING STIM MS " + lastTimeStimulusShown_MS + " at frame # " + SubjectPerformanceReport.currentFrameCycle);
            });
        }
        private double lastTimeHere = -1;

        // 3 of 4. PEAK END - STIM NOT FULL CONTRAST (OFFSET)
        private void StimulusOffset()
        {
            if (lastStimulus == null) return;

            string nameString = lastStimulusSprite != null ? "({0})"._Format(lastStimulusSprite.name) : "";

            StimulusType _stimulusType = lastStimulusType.Value;
            bool _isStimulusProbed = isLastStimulusProbed;

            Vector3 position = lastStimulusBackgroundObject.transform.position;

            // Stim will ACTUALLY get shown on the next frame
            // That's when we log it
            AsyncThread.RunOnMainThread_NextFrameCycle_TS(() =>
            {
                double currentTimestampMS = TimeWrapper.lastRenderedFrame_TimeOfRenderMS;
                TimeWrapper.Timestamp timestamp = TimeWrapper.GetLastFrameTimestamp_TS();

                ExperimentManagerSession.LogData_AsTheyHappen_TS(timestamp,
                    "Stimulus Manager", string.Format("HIDING_STIMULUS;{0};{1};{2};{3};{4}", // Stimulus TYPE-NAME, World Pos, Screen Pos // "[0] stimulusTypeName, [1] worldPosition, [2] screenPosition, [3] isProbed, [4] timestampMS"
                    _stimulusType + nameString, position, Utility_Helper.GetRelativeScreenPosition(position), _isStimulusProbed, currentTimestampMS));
                ExperimentManagerSession.LogData_AsTheyHappen_TS(timestamp,
                    "Stimulus Manager", string.Format("STIMULUS_DURATION;{0}", currentTimestampMS - lastTimeStimulusShown_MS));
                //Debug.Log(string.Format("Stimulus Duration (s):{0}", Time.realtimeSinceStartup - lastStimulusTimestamp));
            });

            // [SOS] Make sure to update the cycle index LAST
            this.LogWarning("Hiding Stimulus #" + currentCycleIdx);
        }

        // 4 of 4. CYCLE END - CLEAR STIMULUS (PROBE)
        private void StimulusClear(bool shouldFireTriggers)
        {
            if (!lastStimulus.HasValue) return;

            string worldLevelTrial = "{0}_{1}"._Format(runtimeConfig.level.levelName, numStimuli);
            string nameString = lastStimulusSprite != null ? "({0})"._Format(lastStimulusSprite.name) : "";

            bool _isStimulusProbed = isLastStimulusProbed;

            // Fire this frame so events do happen on the next (photodiode, audio). LPT won't as it will go on the NotTS (run on next frame)
            RaiseOnStimulusClear(currentCycleIdx, lastStimulusBackgroundObject, lastStimulus.Value, _isStimulusProbed, worldLevelTrial, shouldFireTriggers);

            Vector3 position = lastStimulusBackgroundObject.transform.position;

            // Stim will ACTUALLY get shown on the next frame
            // That's when we log it
            ExperimentManagerSession.LogData_AtNextRenderedFrame_TS("STIMULUS_MANAGER", "STIMULUS_CLEAR");

            // [SOS] Make sure to update the cycle index LAST
            currentCycleIdx++;

            lastStimulusBackgroundObject.Lock(false);
            lastStimulusBackgroundObject.UnPaint();

            lastStimulus = null;
            isLastStimulusProbed = false;
        }

        protected SpriteLocation GetRandomStimulus()
        {
            // Decide WHAT to show
            float dice = Utility_Helper.RandomRange(0f, 1f);
            StimulusType stimulusType = StimulusType.None;

            bool isGame = !runtimeConfig.level.isLocalizer;

            float baseProbabilityBlank = isGame ? config.blankVsNormalRatio_Gameplay : config.blankVsNormalRatio_Localizer;
            float baseProbabilityFace = (1 - baseProbabilityBlank) * (isGame ? config.facesVsObjectsRatio_Gameplay : config.facesVsObjectsRatio_Localizer);
            float baseProbabilityObject = 1 - baseProbabilityBlank - baseProbabilityFace;

            stimulusType = GetStimulusTypeFromProbabilities(dice, baseProbabilityBlank, baseProbabilityFace, baseProbabilityObject);
            Direction_2D_Diagonal direction = Utility_Helper.EnumGetRandom<Direction_2D_Diagonal>();

            Sprite stimulus =
                stimulusType == StimulusType.Face ? stimuliTexturesFaces.GetRandom() :
                stimulusType == StimulusType.Object ? stimuliTexturesObjects.GetRandom() : null;

            string stimName = stimulus != null ? stimulus.name : EMPTY_STIM_NAME;

            return new SpriteLocation(stimulus, direction, stimName, stimulusType);
        }

        /// <summary>
        /// This can include BLANK stimuli
        /// </summary>
        private void PaintStimulus(SpriteLocation stimulusToShow, bool probeStimulus)
        {
            BackgroundObject chosenObject = GetObjectFromDirection(stimulusToShow.direction);

            // Is it ok?
            if (chosenObject == null)
            {
                this.LogWarning("No valid object to show stimulus");
                return;
            }

            // Place it
            chosenObject.SetOverlay(stimulusToShow.sprite);
            chosenObject.Lock(true);

            // Log it
            lastStimulus = stimulusToShow;
            isLastStimulusProbed = probeStimulus;
        }
        
        private StimulusType GetStimulusTypeFromProbabilities(float dice, float probabilityBlank, float probabilityFace, float probabilityObject)
        {
            StimulusType stimulusType = StimulusType.None;

            float additiveProbabilityBlank = probabilityBlank;
            float additiveProbabilityFace = additiveProbabilityBlank + probabilityFace;
            float additiveProbabilityObject = additiveProbabilityFace + probabilityObject;

            if (dice < additiveProbabilityBlank)
                stimulusType = StimulusType.None;
            // Only in this case, re-roll and make a choice of faces vs objects
            else if (dice < additiveProbabilityFace)
                stimulusType = StimulusType.Face;
            else if (dice <= additiveProbabilityObject)
                stimulusType = StimulusType.Object;
            else
            {
                stimulusType = StimulusType.None;
                this.LogError("Something went wrong! Dice {0} , Additive probability object {1}"._Format(dice, additiveProbabilityObject));
            }

            return stimulusType;
        }

        private readonly List<BackgroundObject> lastChoices = new List<BackgroundObject>();

        protected BackgroundObject ChooseBackgroundObject(List<BackgroundObject> visibleObjects)
        {
            // Decide WHERE to place it
            BackgroundObject backgroundObject = null;

            // [TEMP]
            // backgroundObject = visibleObjects.GetRandom();

            // Debug.LogError(visibleObjects.ToReadableString());
            // Are we full?
            if (lastChoices.Count == visibleObjects.Count)
            {
                // Clear all except the last choice
                BackgroundObject bO = lastChoices.GetLast();
                lastChoices.Clear();
                lastChoices.Add(bO);
            }

            // Don't reselect somethign from last choices
            visibleObjects.RemoveRange(lastChoices);

            // Pick one at random
            backgroundObject = visibleObjects.GetRandom();

            // Log it as our last choice
            lastChoices.Add(backgroundObject);

            // Return it
            return backgroundObject;
        }

        public void Pause(bool doPause)
        {
            if (backgroundManager != null)
                backgroundManager.Pause(doPause);

            isPaused = doPause;
        }

        public int GetNumStimuliShown()
        {
            return numStimuli;
        }

        private void RaiseOnStimulusOnset(int currentCycleIdx, BackgroundObject backgroundObject, SpriteLocation stimulus, bool probeNextStimulus, string worldLevelTrial, bool shouldFireTriggers)
        {
            onStimulusOnset?.Invoke(this, new StimulusEventArgs(currentCycleIdx, backgroundObject, stimulus, probeNextStimulus, worldLevelTrial, shouldFireTriggers));
        }

        private void RaiseOnStimulusClear(int currentCycleIdx, BackgroundObject backgroundObject, SpriteLocation stimulus, bool probeNextStimulus, string worldLevelTrial, bool shouldFireTriggers)
        {
            onStimulusClear?.Invoke(this, new StimulusEventArgs(currentCycleIdx, backgroundObject, stimulus, probeNextStimulus, worldLevelTrial, shouldFireTriggers));
        }

        /*
        private void RaiseOnStimulusQueueFinish()
        {
            if (onStimulusQueueFinish != null)
                onStimulusQueueFinish(this, new EventArgs());
        }
        */

        #region Config
        public class RuntimeConfig
        {
            public BackgroundManager.RuntimeConfig background;
            public LevelConfig level;
            public List<EventInformation> timings;

            public RuntimeConfig(BackgroundManager.RuntimeConfig background, LevelConfig level, List<EventInformation> timings)
            {
                this.background = background;
                this.level = level;
                this.timings = timings;
            }
        }

        [Serializable]
        public class Config
        {
            // TASK-RELEVANT
            [SerializeField] private string taskRelevant_showRandomStimulusWhenQueueIsEmpty_Comment = "If set to false, it will show a filler instead";
            public bool taskRelevant_showRandomStimulusWhenQueueIsEmpty = false;

            // TASK-IRRELEVANT
            [SerializeField] private string taskIrrelevant_cycleStimuliWheneQueueIsEmpty_Comment = "If set to false, it will show a filler instead";
            public bool taskIrrelevant_cycleStimuliWheneQueueIsEmpty = false;

            public bool createPreExposureSlide_Faces = true;
            public PreExposureSlidesConfig preExposureConfig_Faces = new PreExposureSlidesConfig("PreExposureSlideFaces");
            public bool createPreExposureSlide_Objects = true;
            public PreExposureSlidesConfig preExposureConfig_Objects = new PreExposureSlidesConfig("PreExposureSlideObjects");

            public StimulusTimingsConfig timingsGameplay = new StimulusTimingsConfig(3);
            public StimulusTimingsConfig timingsGameplay_FMRIScanner = new StimulusTimingsConfig(new TruncatedExpConfig(1.3867f, 5.3333f, 8f, 100)); // 6.0f

            public string timingsReplay_Comment = "If SameAsGame, uses the above values";
            public bool timingsReplay_SameAsGame = true;

            public StimulusTimingsConfig timingsReplay
            {
                get
                {
                    return
    timingsReplay_SameAsGame ? timingsGameplay : _timingsReplay;
                }
            }
            public StimulusTimingsConfig _timingsReplay = new StimulusTimingsConfig(2);

            public StimulusTimingsConfig timingsReplay_FMRIScanner
            {
                get
                {
                    return
    timingsReplay_SameAsGame ? timingsGameplay_FMRIScanner : _timingsReplay_FMRIScanner;
                }
            }
            public StimulusTimingsConfig _timingsReplay_FMRIScanner = new StimulusTimingsConfig(2);

            public string backgroundToStimulus_Localizer_CorrectorMS_Comment = "Replay Stim Timings are not precalculated. We use some hacky calculations instead. This can help ensure the wanted distance between stimuli in replays.";
            public float backgroundToStimulus_Localizer_CorrectorMS = 0;
            public bool backgroundToStimulus_Localizer_CorrectorAddFrame = false;

            public float stimulusAlpha = 0.6f;
            public float stimulusBrightness = 0.8f;

            public float blankVsNormalRatio_Gameplay = 0.2f;
            public float facesVsObjectsRatio_Gameplay = 0.5f;

            /// <summary>
            /// [SOS] Use <see cref="ExperimentManagerApplication.wantedNumStimuli_AllLocalizers(float)"/> instead.
            /// </summary>
            public int wantedNumStimulus_Localizer_FullGame = 5;
            public float blankVsNormalRatio_Localizer = 0.5f;
            public float facesVsObjectsRatio_Localizer = 0.5f;

            // public int stimulusDurationMS = 500;
            // public float stimulusDurationSeconds { get { return stimulusDurationMS / 1000f; } }
            public float extraDelayMS_LocalizerEnd = 5000;

            // public string animEvent_Comment = "CycleBegin = 0, PeakBegin = 1, PeakEnd = 2, CycleEnd = 3 ";
            /// Can't really be changed from these values without breaking, so leave them there;
            [NonSerialized] public AnimEvent animEvent_StimulusSelection = AnimEvent.CycleBegin;
            [NonSerialized] public AnimEvent animEvent_StimulusOnset = AnimEvent.PeakBegin;
            [NonSerialized] public AnimEvent animEvent_StimulusOffset = AnimEvent.PeakEnd;
            [NonSerialized] public AnimEvent animEvent_StimulusClear = AnimEvent.CycleEnd;

            public BackgroundManager.Config background;
        }
        #endregion

        #region Localizer Assignments

        private static List<List<int>> SUBJECT_ID_TO_REPLAY_ID_0BASED_TO_LEVEL_ID_1BASED = new List<List<int>>()
        {
            // % 2 == 0
            new List<int>()
            {
                1, 2, 5, 6, 9, 10, 13, 14
            },
            new List<int>()
            {
                3, 4, 7, 8, 11, 12, 15, 16
            },
        };

        private static List<List<StimulusType>> SUBJECT_ID_DIV_2_TO_REPLAY_ID_0BASED_TO_TARGET = new List<List<StimulusType>>()
        {
            // % 16 == 0
            new List<StimulusType>()
            {
                StimulusType.Face, StimulusType.Object, StimulusType.Face, StimulusType.Object, StimulusType.Face, StimulusType.Object, StimulusType.Face, StimulusType.Object
            },
            // % 16 == 1
            new List<StimulusType>()
            {
                StimulusType.Object, StimulusType.Face, StimulusType.Face, StimulusType.Object, StimulusType.Face, StimulusType.Object, StimulusType.Face, StimulusType.Object
            },
            // % 16 == 2
            new List<StimulusType>()
            {
                StimulusType.Face, StimulusType.Object, StimulusType.Object, StimulusType.Face, StimulusType.Face, StimulusType.Object, StimulusType.Face, StimulusType.Object
            },
            // % 16 == 3
            new List<StimulusType>()
            {
                StimulusType.Object, StimulusType.Face, StimulusType.Object, StimulusType.Face, StimulusType.Face, StimulusType.Object, StimulusType.Face, StimulusType.Object
            },
            // % 16 == 4
            new List<StimulusType>()
            {
                StimulusType.Face, StimulusType.Object, StimulusType.Face, StimulusType.Object, StimulusType.Object, StimulusType.Face, StimulusType.Face, StimulusType.Object
            },
            // % 16 == 5
            new List<StimulusType>()
            {
                StimulusType.Object, StimulusType.Face, StimulusType.Face, StimulusType.Object, StimulusType.Object, StimulusType.Face, StimulusType.Face, StimulusType.Object
            },
            // % 16 == 6
            new List<StimulusType>()
            {
                StimulusType.Face, StimulusType.Object, StimulusType.Object, StimulusType.Face, StimulusType.Object, StimulusType.Face, StimulusType.Face, StimulusType.Object
            },
            // % 16 == 7
            new List<StimulusType>()
            {
                StimulusType.Object, StimulusType.Face, StimulusType.Object, StimulusType.Face, StimulusType.Object, StimulusType.Face, StimulusType.Face, StimulusType.Object
            },
            // % 16 == 8
            new List<StimulusType>()
            {
                StimulusType.Face, StimulusType.Object, StimulusType.Face, StimulusType.Object, StimulusType.Face, StimulusType.Object, StimulusType.Object, StimulusType.Face
            },
            // % 16 == 9
            new List<StimulusType>()
            {
                StimulusType.Object, StimulusType.Face, StimulusType.Face, StimulusType.Object, StimulusType.Face, StimulusType.Object, StimulusType.Object, StimulusType.Face
            },
            // % 16 == 10
            new List<StimulusType>()
            {
                StimulusType.Face, StimulusType.Object, StimulusType.Object, StimulusType.Face, StimulusType.Face, StimulusType.Object, StimulusType.Object, StimulusType.Face
            },
            // % 16 == 11
            new List<StimulusType>()
            {
                StimulusType.Object, StimulusType.Face, StimulusType.Object, StimulusType.Face, StimulusType.Face, StimulusType.Object, StimulusType.Object, StimulusType.Face
            },
            // % 16 == 12
            new List<StimulusType>()
            {
                StimulusType.Face, StimulusType.Object, StimulusType.Face, StimulusType.Object, StimulusType.Object, StimulusType.Face, StimulusType.Object, StimulusType.Face
            },
            // % 16 == 13
            new List<StimulusType>()
            {
                StimulusType.Object, StimulusType.Face, StimulusType.Face, StimulusType.Object, StimulusType.Object, StimulusType.Face, StimulusType.Object, StimulusType.Face
            },
            // % 16 == 14
            new List<StimulusType>()
            {
                StimulusType.Face, StimulusType.Object, StimulusType.Object, StimulusType.Face, StimulusType.Object, StimulusType.Face, StimulusType.Object, StimulusType.Face
            },
            // % 16 == 15
            new List<StimulusType>()
            {
                StimulusType.Object, StimulusType.Face, StimulusType.Object, StimulusType.Face, StimulusType.Object, StimulusType.Face, StimulusType.Object, StimulusType.Face
            },
        };
        #endregion
    }

    [Serializable]
    public struct LocalizerInfo
    {
        public int targetLevelID_1Based;
        public StimulusType target;

        public LocalizerInfo(int targetLevelID_1Based, StimulusType target)
        {
            this.targetLevelID_1Based = targetLevelID_1Based;
            this.target = target;
        }

        public static readonly LocalizerInfo EMPTY = new LocalizerInfo(-1, StimulusType.None);

        internal static LocalizerInfo FromCSV(string line)
        {
            string[] fields = line.Split(';');

            int targetLevelID_1Based = fields[0].ToInt();
            StimulusType target = fields[1].ToEnum<StimulusType>();

            return new LocalizerInfo(targetLevelID_1Based, target);
        }

        internal static string ToCSV(List<LocalizerInfo> localizerInfos)
        {
            return ToCSV(localizerInfos.ToArray());
        }

        internal static string ToCSV(params LocalizerInfo[] localizerInfos)
        {
            string lineBreak = "\r\n";

            string csv = "LevelToReplayID_1Based;TargetType" + lineBreak;
            foreach (LocalizerInfo lI in localizerInfos)
                csv += ToCSV(lI) + lineBreak;
            csv = csv.Substring(0, csv.Length - lineBreak.Length);
            return csv;
        }

        private static string ToCSV(LocalizerInfo lI)
        {
            return "{0};{1}"._Format(lI.targetLevelID_1Based, lI.target);
        }
    }
}