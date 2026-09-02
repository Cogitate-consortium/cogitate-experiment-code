// NS_REMOVE | Configs, 1 Log
using ExperimentLibrary;

using UnityEngine;
using TGP.Helpers;
using System.Collections.Generic;
using Game.Managers.SessionManagers; // Just takes the LEVELS
using Game.Core; // Just takes LevelInfo
using Experiment.Managers;
using System;
using System.Linq;

namespace Experiment.Task
{
    public static class Precalculator
    {
        private static bool USE_OLD = false;

        private static int maxAttempts = 100000;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="level">The level we are STARTING now</param>
        /// <returns></returns>
        public static AllIndices GetAllIndicesAtStartOfLevel(LevelConfig level)
        {
            int levelID_1Based = level.levelID_1Based; // level ID is 1-based - tutorial will be -1 but that's okay!

            // Is it a localizer level?
            if (level.isLocalizer)
            {
                // Get the idx of the level it is replaying at the start of the level it's replaying
                levelID_1Based = ExperimentManagerSession.GetRecordedReplayID(level);

                if (levelID_1Based < 0)
                    Debug_Helper.LogWarning(typeof(Precalculator), "Didn't find a recorded level for level name {0} (localizer {1}). Picking default indices."._Format(level.levelName, level.localizerID_0Based));
            }

            return GetAllIndicesAtStart(levelID_1Based);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="levelIdx_1Based">The level we are STARTING now</param>
        /// <returns></returns>
        private static AllIndices GetAllIndicesAtStart(int levelIdx_1Based)
        {
            if (levelIdx_1Based < 2) // negatives (ie. tutorial) and 1 (first level, which has no "triggering" anything, we just start it!)
                return new AllIndices(0, 0, 0, 0);

            AllIndices allIndices = new AllIndices();
            allIndices.levelIdx_1Based = levelIdx_1Based;

            // Probe that ended the previous level
            int previousLevelIdx_1Based = levelIdx_1Based - 1;
            int previousLevel_ARRAY_INDEX = previousLevelIdx_1Based - 1;
            int endingProbeIdx = gameTimings.cumulativeLevelDurations_Actual[previousLevel_ARRAY_INDEX].triggeredByEventID;

            // Stim that triggered that probe
            int endingStimIdx = gameTimings.probeOccurences_Actual[endingProbeIdx].triggeredByEventID;

            // Bckg that triggered that stim
            int endingBckgIdx = gameTimings.stimulusOccurences_Actual[endingStimIdx].triggeredByEventID;

            // Level starts at the next of all these
            allIndices.probeIdx = endingProbeIdx + 1;
            allIndices.stimIdx = endingStimIdx + 1;
            allIndices.bckgIdx = endingBckgIdx + 1;

            /*
            Debug.LogError("Starting level {0} (next Bckg {1}, next stim {2}, next probe {3})".
                _Format(levelIdx, allIndices.bckgIdx, allIndices.stimIdx, allIndices.probeIdx));
            */
            return allIndices;
        }

        public static bool initialized = false;

        // 200619 TODO NS_SEGMENT -> This has to be moved to the level layer (initialize then use as-is)
        /// <summary>
        /// [SOS] This is what you call for Duration, not <see cref="LevelsLibrary.GetLevelDuration(LevelConfig)"/>
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static double GetLevelDuration(LevelConfig level)
        {
            if (level == null)
            {
                Debug_Helper.LogError(typeof(Precalculator), "Level was null");
                return 0;
            }

            if (!initialized || gameTimings.cumulativeLevelDurations_Actual.Count == 0)
            {
                Debug_Helper.LogError(typeof(Precalculator), "Precalculator not initialized");
                return 0;
            }

            if (level.isTutorial)
                return level.wantedDuration;

            if (!level.isLocalizer)
            {
                if (level.levelID_1Based > gameTimings.cumulativeLevelDurations_Actual.Count)
                {
                    Debug_Helper.LogError(typeof(Precalculator), "Level ID (1-Based) out of range:" + level.levelID_1Based);
                    return 0;
                }

                return gameTimings.cumulativeLevelDurations_Actual[level.levelID_1Based - 1].delayFromPrevious; //1-based id for 0-based indexing
            }

            // [200730] For replays get an average from level 1
            EventInformation lastStimulusOfReplay = gameTimings.stimulusOccurences_Actual.GetMod(ExperimentManagerSession.wantedNumStimuli_PerLocalizer);
            return lastStimulusOfReplay.timestamp;

            // Get the idx of the level it is replaying at the start of the level it's replaying
            /*
            int levelID_1Based = ExperimentManagerSession.GetRecordedReplayID(level);// ExperimentManagerSession.isFMRI);

            if (levelID_1Based >= 0) // valid level
            {
                int levelID_0Based = levelID_1Based - 1;

                if (levelID_0Based < 0)
                {
                    Debug_Helper.LogError(typeof(Precalculator), "Level ID (1-Based) out of range:" + level.levelID_1Based);
                    return 0;
                }

                int gameTimingsCount = gameTimings.cumulativeLevelDurations_Actual.Count;
                if (levelID_0Based >= gameTimingsCount)
                {
                    Debug_Helper.LogError(typeof(Precalculator), "Level ID (1-Based) out of range: " + level.levelID_1Based + " > gameTimingsCount: " + gameTimingsCount);
                    return 0;
                }

                return gameTimings.cumulativeLevelDurations_Actual[levelID_0Based].delayFromPrevious;
            }

            return 0;
            */
        }

        /// <summary>
        /// [SOS] Do not use before <see cref="initialized"/> is true
        /// </summary>
        public static GameTimings gameTimings;

        // public static List<LocalizerTimings> localizerTimings;

        // public static int wantedNumPeaks_Localizer;

        public static GameTimings ReadInAllTimings(IList<string> timingsFileContents)
        {
            gameTimings = ReadInGameTimings(timingsFileContents);

            initialized = true;

            return gameTimings;
        }

        private static GameTimings ReadInGameTimings(IList<string> timingsFileContents)
        {
            return GameTimings.FromCSV(timingsFileContents);
        }

        /// <summary>
        /// [SOS] Call after <see cref="ExperimentManagerSession.module"/> has been set!
        /// </summary>
        public static GameTimings PrecalculateAllTimings(int subjectID, List<int> replayLevelIDs_0Based)
        {
            // wantedNumPeaks_Localizer = ExperimentManagerSession.wantedNumStimuli_PerLocalizer;

            gameTimings = PrecalculateGameTimings(subjectID, replayLevelIDs_0Based);

#if UNITY_EDITOR
            //FileWrapper.WriteToFile(@"C:\Coding\ReedCollege\Seattle Project (Unity Project)\Logs\gameTiming.json", JsonUtility.ToJson(gameTimings));
            //Debug.Log("Wrote all game timings to a file for testing purposes (Run BackgroundManager_Abstract_StaticRotatingSquares offline)");
#endif

            /*
            localizerTimings = new List<LocalizerTimings>();
            for (int i = 0; i < numLocalizers; i++)
                localizerTimings.Add(PrecalculateLocalizerTimings());
            */

            initialized = true;

            return gameTimings;
        }

        /*
        private static LocalizerTimings PrecalculateLocalizerTimings()
        {
            LocalizerTimings timings = new LocalizerTimings(0);

            // ---- Calculate intended (also actual) background delays
            #region Background
            timings.backgroundPeaks = new List<EventInformation>();

            MinMax minMax_Background = ExperimentLibraryManager.Config.GetPeriodMinMax_Background();

            float currentBackgroundCumulativeDelay = 0;
            // We need enough background to cover all our stimulus
            // (we actually need a bit more)
            float peakDistanceFromCycleEnd =
                ExperimentLibraryManager.Config.Experiment.stimulus.background.peakDuration;// + ApplicationLibrary.Config.Experiment.stimulus.background.peakEaseOut; [DEPRECATED? 200508]

            for (int i = 0; i < wantedNumPeaks_Localizer; i++)
            //  while (currentBackgroundCumulativeDelay < currentStimulusCumulativeDelay + peakDistanceFromCycleEnd)
            {
                float delay = Utility_Helper.RandomRange(minMax_Background.min, minMax_Background.max);
                currentBackgroundCumulativeDelay += delay;

                timings.backgroundPeaks.Add(new EventInformation(
                    timings.backgroundPeaks.Count, delay, currentBackgroundCumulativeDelay));
            }


            Debug_Helper.LogError(typeof(Precalculator), "Background {0}, {1}, {2}"._Format(currentBackgroundCumulativeDelay,
                timings.backgroundPeaks.Count,
                timings.backgroundPeaks.ToReadableString())); // [CHECK!]
            #endregion

            return timings;
        }
        */

        private static GameTimings PrecalculateGameTimings(int subjectID, List<int> replayLevelIDs_0Based)
        {
            GameTimings timings = new GameTimings();

            for (int attempts = 1; attempts <= maxAttempts; attempts++)
            {
                // SUCCESS
                if (USE_OLD)
                {
                    if (_PrecalculateGameTimings_OLD(out timings)) return timings;
                }
                else
                {
                    if (_PrecalculateGameTimings(out timings, subjectID, replayLevelIDs_0Based)) return timings;
                }
                

                // Debug_Helper.LogWarning(typeof(Precalculator), "Failed @PrecalculateGameTimings {0} times (max {1}). Retrying."._Format(attempts, maxAttempts));
            }

            Debug_Helper.LogError(typeof(Precalculator), "CRITICAL ERROR: Failed @PrecalculateGameTimings {0} times, RESTART GAME".
                _Format(maxAttempts));
            return timings;
        }

        private static bool _PrecalculateGameTimings(out GameTimings timings, int subjectID, List<int> replayLevelIDs_0Based)
        {
            timings = new GameTimings(0);

            List<int> DEBUG_PROBE_INTENDED = new List<int>();
            List<int> DEBUG_STIM_INTENDED = new List<int>();
            List<int> DEBUG_BACKGROUND = new List<int>();
            List<int> DEBUG_STIM_ACTUAL = new List<int>();
            List<int> DEBUG_PROBE_ACTUAL = new List<int>();
            List<int> DEBUG_LEVEL_INTENDED = new List<int>();
            List<int> DEBUG_LEVEL_ACTUAL = new List<int>();

            // ---- Calculate intended probe delays!
            #region Intended Probe
            bool isFMRIScanner = ExperimentManagerSession.module == ModuleType.FMRI_Scanner;

            Dictionary<float, float> probeCumDistrib = ProbeManager.GetTruncatedExpDistribution(isFMRIScanner);
            // Debug.LogError(probeCumDistrib.ToReadableString());

            // For all probes
            int numWorlds = PlayerProgression.numGameWorlds;
            int numLevelsPerWorld = PlayerProgression.numLevelsPerWorld;
            int probesPerWorld = ExperimentManagerSession.GetWantedNumProbesPerWorld();
            int probesTotal = ExperimentManagerSession.GetWantedNumProbesTotal();

            // For each of the intended probes
            int backgroundIdx = 0;
            int stimulusIdx = 0; 
            int probeIdx = 0;
            int levelIdx = 1; // First level is manually started

            double lastBackgroundTS = 0;
            double lastStimulusTS = 0;
            double lastProbeTS = 0;
            double lastLevelTS = 0;

            Dictionary<int, int> STIM_TO_PROBE_RATIOS_COUNT = new Dictionary<int, int>();
            Dictionary<int, int> BACKG_TO_STIM_RATIOS_COUNT = new Dictionary<int, int>();

            // Shuffle everything
            List<int> keyProbesFMRI = new List<int>(PROBE_TO_STIM_FMRI.Keys);
            List<int> keyProbes = new List<int>(PROBE_TO_STIM.Keys);
            List<int> keyStimFMRI = new List<int>(STIM_TO_BCKG_FMRI.Keys);
            List<int> keyStim = new List<int>(STIM_TO_BCKG.Keys);

            foreach (int key in keyProbesFMRI)
                PROBE_TO_STIM_FMRI[key] = PROBE_TO_STIM_FMRI[key].Shuffle().CustomToList();
            foreach (int key in keyProbes)
                PROBE_TO_STIM[key] = PROBE_TO_STIM[key].Shuffle().CustomToList();
            foreach (int key in keyStimFMRI)
                STIM_TO_BCKG_FMRI[key] = STIM_TO_BCKG_FMRI[key].Shuffle().CustomToList();
            foreach (int key in keyStim)
                STIM_TO_BCKG[key] = STIM_TO_BCKG[key].Shuffle().CustomToList();

            List<int> PROBE_DELAYS = new List<int>();

            Dictionary<int, double> PROBE_FREQUENCIES = new Dictionary<int, double>(isFMRIScanner ? Precalculator.PROBE_FREQUENCIES_FMRI : Precalculator.PROBE_FREQUENCIES);

            // Normalize
            double sum = PROBE_FREQUENCIES.Values.Sum(a => a);
            List<int> keys = new List<int>(PROBE_FREQUENCIES.Keys);
            foreach (int key in keys)
                PROBE_FREQUENCIES[key] /= sum;

            // layer it up (it's already shuffled)
            foreach (KeyValuePair<int, double> kVP in PROBE_FREQUENCIES)
                for (int i = 0; i < Math.Floor(kVP.Value * probesTotal); i++)
                    PROBE_DELAYS.Add(kVP.Key);

            // Finish it off 
            List<int> PROBE_DELAYS_COPY = new List<int>(PROBE_DELAYS); // We already have properly distributed delays
            while (PROBE_DELAYS.Count < probesTotal)
            {
                int choice = PROBE_DELAYS_COPY.GetRandom();
                PROBE_DELAYS.Add(choice);
                PROBE_DELAYS_COPY.Remove(choice);
            }

            // Shuffle once more to be unbiased
            PROBE_DELAYS = PROBE_DELAYS.Shuffle().CustomToList();

            int wantedNumProbesInFirst25 = GetNumProbesInFirst25(isFMRIScanner);

            Dictionary<int, int> LAST_PROBE_TO_STIM_INDEX = new Dictionary<int, int>();
            Dictionary<int, int> LAST_STIM_TO_BACKG_INDEX = new Dictionary<int, int>();

            int intendedNumberStimuli = 3;
            bool intervene = false;

            // Break them up into levels
            for (int w = 0; w < numWorlds; w++)
            {
                // pick levels 
                List<int> levelsToProbes = WORLD_TO_PROBE.GetMod(subjectID + w);

                // For each of the levels
                foreach (int numProbesInLevel in levelsToProbes)
                {
                    double totalLevelDuration = 0;
                    bool isReplayLevel = replayLevelIDs_0Based.Contains(levelIdx-1); // level idx is already incremented

                    int replaySafety = 0;
                    int numStimuliLevel = 0;

                    // For each of the probes
                    for (int probeInLevel = 0; probeInLevel < numProbesInLevel; probeInLevel++)
                    {
                        // Pick a probe
                        int probeDelayMS = PROBE_DELAYS.GetRandom();

                        PROBE_DELAYS.Remove(probeDelayMS);

                        double totalProbeDuration = 0;

                        // Figure out its stimuli
                        List<int> probeStimuliDelaysMS = null;
                        try
                        {
                            List<List<int>> stimulusSetsToUse = (isFMRIScanner ? PROBE_TO_STIM_FMRI : PROBE_TO_STIM)[probeDelayMS];

                            int indexToUse = 0;

                            int wantedIndex = !LAST_PROBE_TO_STIM_INDEX.ContainsKey(probeDelayMS) ? 0 :
                                (LAST_PROBE_TO_STIM_INDEX[probeDelayMS] + 1) % stimulusSetsToUse.Count;

                            // how would that go?
                            int diff = stimulusSetsToUse[wantedIndex].Count - intendedNumberStimuli;

                            // Either it would go
                            if (!intervene || diff == 0 ||
                                // or we don't care
                                Mathf.Abs(replaySafety) < 1 || probeInLevel > 7 || !isReplayLevel)
                            {
                                indexToUse = wantedIndex;
                                LAST_PROBE_TO_STIM_INDEX.AddOrUpdate(probeDelayMS, indexToUse);
                            }
                            else
                            {
                                // Get the first that has a good number of stim
                                indexToUse = stimulusSetsToUse.FindIndex(a => a == stimulusSetsToUse.CustomOrderBy(b => (intendedNumberStimuli - b.Count),
                                    replaySafety > 0 ? Order.Descending : Order.Ascending)[0]);

                                Debug.Log("Safety was " + replaySafety + " for probe ID " + probeInLevel + " (" + probeDelayMS +
                                    "ms) overriding index to set #" + indexToUse + "which has " +
                                    stimulusSetsToUse[indexToUse].Count + " stimuli.");

                                int actualDiff = stimulusSetsToUse[indexToUse].Count - intendedNumberStimuli;
                                if (Mathf.Abs(actualDiff) == Mathf.Abs(diff))
                                    Debug.LogWarning("We did not achieve anything better");
                                if (Mathf.Abs(actualDiff) > Mathf.Abs(diff))
                                    Debug.LogError("We somehow got worse results");
                            }

                            probeStimuliDelaysMS = stimulusSetsToUse[indexToUse];
                        }
                        catch
                        {
                            Debug.LogError(probeDelayMS);
                            Debug.LogError((isFMRIScanner ? PROBE_TO_STIM_FMRI : PROBE_TO_STIM).ToReadableString());
                        }

                        if (isReplayLevel)
                            replaySafety += (probeStimuliDelaysMS.Count - intendedNumberStimuli);

                        // Debug.Log("Added " + (probeStimuliDelaysMS.Count - 1) + " unprobed stimuli for this probe");

                        // For each of its stimuli
                        int numStimuliInProbe = probeStimuliDelaysMS.Count;
                        if (!STIM_TO_PROBE_RATIOS_COUNT.ContainsKey(numStimuliInProbe))
                            STIM_TO_PROBE_RATIOS_COUNT.Add(numStimuliInProbe, 0);
                        STIM_TO_PROBE_RATIOS_COUNT[numStimuliInProbe]++;

                        for (int stimWithinProbeIdx = 0; stimWithinProbeIdx < numStimuliInProbe; stimWithinProbeIdx ++)
                        {
                            int stimDelayMS = probeStimuliDelaysMS[stimWithinProbeIdx];
                            double totalStimDuration = 0;

                            // Figure out its backgroudns
                            // Debug.Log(stimDelayMS);
                            List<int> stimulusBackgroundDelaysMS = null;
                            try
                            {
                                List<List<int>> backgSetsToUse = (isFMRIScanner ? STIM_TO_BCKG_FMRI : STIM_TO_BCKG)[stimDelayMS];

                                if (!LAST_STIM_TO_BACKG_INDEX.ContainsKey(stimDelayMS))
                                    LAST_STIM_TO_BACKG_INDEX.Add(stimDelayMS, 0);
                                else
                                    LAST_STIM_TO_BACKG_INDEX[stimDelayMS] = (LAST_STIM_TO_BACKG_INDEX[stimDelayMS] + 1) % backgSetsToUse.Count;

                                int indexToUse = LAST_STIM_TO_BACKG_INDEX[stimDelayMS];
                                stimulusBackgroundDelaysMS = backgSetsToUse[indexToUse];
                            }
                            catch
                            {
                                Debug.LogError(probeDelayMS);
                                Debug.LogError((isFMRIScanner ? PROBE_TO_STIM_FMRI : PROBE_TO_STIM).ToReadableString());
                            }

                            // For each of its backgrounds
                            int numBackgroundsInStimulus = stimulusBackgroundDelaysMS.Count;
                            if (!BACKG_TO_STIM_RATIOS_COUNT.ContainsKey(numBackgroundsInStimulus))
                                BACKG_TO_STIM_RATIOS_COUNT.Add(numBackgroundsInStimulus, 0);
                            BACKG_TO_STIM_RATIOS_COUNT[numBackgroundsInStimulus]++;

                            foreach (int backgroundDelayMS in stimulusBackgroundDelaysMS)
                            {
                                // Create the background
                                EventInformation backgroundEventInfo = EventInformation.DEFAULT;

                                backgroundEventInfo.id = backgroundIdx++;

                                // First Background doesn't have a delay from previous
                                double delayToAdd = (double)backgroundDelayMS / 1000;

                                // if (backgroundEventInfo.id == 0)
                                //    backgroundEventInfo.delayFromPrevious = 0;
                                // else
                                    backgroundEventInfo.delayFromPrevious = delayToAdd;

                                totalStimDuration += delayToAdd;

                                backgroundEventInfo.timestamp = lastBackgroundTS + delayToAdd;
                                lastBackgroundTS = backgroundEventInfo.timestamp;

                                // Tag it
                                timings.backgroundPeaks.Add(backgroundEventInfo);
                            }

                            // Create the Stimulus
                            EventInformation stimulusEventInfo = EventInformation.DEFAULT;

                            stimulusEventInfo.id = stimulusIdx++;

                            // Shift 1st stim forward from cycle begin
                            // if (stimulusEventInfo.id == 0)
                            //    totalStimDuration += stimOnsetFromCycleBegin;

                            stimulusEventInfo.delayFromPrevious = totalStimDuration;
                            totalProbeDuration += stimulusEventInfo.delayFromPrevious;

                            stimulusEventInfo.timestamp = lastStimulusTS + stimulusEventInfo.delayFromPrevious;
                            lastStimulusTS = stimulusEventInfo.timestamp;

                            // The last background triggers this stimulus
                            EventInformation lastBackgroundEventInfo = timings.backgroundPeaks[timings.backgroundPeaks.Count - 1];
                            lastBackgroundEventInfo.triggersEventID = stimulusEventInfo.id;
                            timings.backgroundPeaks[timings.backgroundPeaks.Count - 1] = lastBackgroundEventInfo;

                            // This stimulus is triggered by the last background
                            stimulusEventInfo.triggeredByEventID = lastBackgroundEventInfo.id;

                            /// [SOS] Do that at the end (structs aren't updated)
                            // Tag it
                            timings.stimulusOccurences_Actual.Add(stimulusEventInfo);

                            // We just added a stimulus. Is it the 25th?
                            numStimuliLevel++;

                            if (isReplayLevel && numStimuliLevel == 25)
                            {
                                int numProbesInFirst25 = probeInLevel;

                                // If this is the last stimulus it will create one more probe
                                if (stimWithinProbeIdx == probeStimuliDelaysMS.Count - 1)
                                    numProbesInFirst25++;

                                // If we have exactly 8 probes we're good to go otherwise it's an issue!
                                if (numProbesInFirst25 != wantedNumProbesInFirst25)
                                {
                                    // Debug.LogError("Had {0} probes in the first 25 stimuli of level {1}, aborting!"._Format(numProbesInFirst25, levelIdx));
                                    return false;
                                }
                                else
                                {
                                    // Debug.Log("Had {0} probes in the first 25 stimuli of level {1}"._Format(numProbesInFirst25, levelIdx));
                                }
                            }
                        }

                        // Create the Probe
                        EventInformation probeEventInfo = EventInformation.DEFAULT;

                        probeEventInfo.id = probeIdx++;

                        // Shift 1st prbe forward from stim onset
                        // if (probeEventInfo.id == 0)
                        //    totalProbeDuration += probeFromStimOnset;

                        probeEventInfo.delayFromPrevious = totalProbeDuration;
                        totalLevelDuration += probeEventInfo.delayFromPrevious;

                        probeEventInfo.timestamp = lastProbeTS + probeEventInfo.delayFromPrevious;
                        lastProbeTS = probeEventInfo.timestamp;

                        // The last stimulus triggers this probe
                        EventInformation lastStimulusEventInfo = timings.stimulusOccurences_Actual[timings.stimulusOccurences_Actual.Count - 1];
                        lastStimulusEventInfo.triggersEventID = probeEventInfo.id;
                        timings.stimulusOccurences_Actual[timings.stimulusOccurences_Actual.Count - 1] = lastStimulusEventInfo;

                        // This probe is triggered by the last stimulus
                        probeEventInfo.triggeredByEventID = lastStimulusEventInfo.id;

                        /// [SOS] Do that at the end (structs aren't updated)
                        // Tag it
                        timings.probeOccurences_Actual.Add(probeEventInfo);
                    }

                    // Create the Level
                    EventInformation levelEventInfo = EventInformation.DEFAULT;

                    levelEventInfo.id = levelIdx++;

                    // Shift 1st level forward from probe onset
                    // if (levelEventInfo.id == 1)
                    //    totalLevelDuration += levelFromProbe;

                    levelEventInfo.delayFromPrevious = totalLevelDuration;
                    levelEventInfo.timestamp = lastLevelTS + levelEventInfo.delayFromPrevious;
                    lastLevelTS = levelEventInfo.timestamp;

                    // The last stimulus triggers this probe
                    EventInformation lastProbeEventInfo = timings.probeOccurences_Actual[timings.probeOccurences_Actual.Count - 1];
                    lastProbeEventInfo.triggersEventID = levelEventInfo.id;
                    timings.probeOccurences_Actual[timings.probeOccurences_Actual.Count - 1] = lastProbeEventInfo;

                    // This probe is triggered by the last stimulus
                    levelEventInfo.triggeredByEventID = lastProbeEventInfo.id;

                    /// [SOS] Do that at the end (structs aren't updated)
                    // Tag it
                    timings.cumulativeLevelDurations_Actual.Add(levelEventInfo);
                }
            }

            // Debug.LogError(BACKG_TO_STIM_RATIOS_COUNT.ToReadableString("BACKG_TO_STIM"));
            // Debug.LogError(STIM_TO_PROBE_RATIOS_COUNT.ToReadableString("STIM_TO_PROBE"));

            #endregion


            #region FINAL CHECK
            int currentWorld = 0;
            List<int> check_ProbesPerWorld = new List<int>();
            for (int i = 0; i < PlayerProgression.numGameWorlds; i++)
                check_ProbesPerWorld.Add(0);

            List<EventInformation> totalLogs = timings.GenerateTotalLogs();

            foreach (EventInformation eI in totalLogs)
            {
                if (eI.eventType == EventType.Level.ToString())
                    currentWorld = Mathf.FloorToInt((float)eI.id / PlayerProgression.numLevelsPerWorld);

                if (eI.eventType == EventType.Probe.ToString())
                    check_ProbesPerWorld[currentWorld]++;
            }

            for (int i = 0; i < PlayerProgression.numGameWorlds; i++)
                if (check_ProbesPerWorld[i] != probesPerWorld)
                {
                    Debug_Helper.LogWarning(typeof(Precalculator), "World {0} had {1} instead of {2} Probes. Recalculating.".
                        _Format(i, check_ProbesPerWorld[i], probesPerWorld));
                    return false;
                }
            #endregion


#if UNITY_EDITOR
            /*
            string s = "";
            s = ""; foreach (int tS in DEBUG_BACKGROUND)        s += tS + "\n"; Debug.Log("DEBUG_BACKGROUND\n"      + s);
            s = ""; foreach (int tS in DEBUG_STIM_INTENDED)     s += tS + "\n"; Debug.Log("DEBUG_STIM_INTENDED\n"   + s);
            s = ""; foreach (int tS in DEBUG_STIM_ACTUAL)       s += tS + "\n"; Debug.Log("DEBUG_STIM_ACTUAL\n"     + s);
            s = ""; foreach (int tS in DEBUG_PROBE_INTENDED)    s += tS + "\n"; Debug.Log("DEBUG_PROBE_INTENDED\n"  + s);
            s = ""; foreach (int tS in DEBUG_PROBE_ACTUAL)      s += tS + "\n"; Debug.Log("DEBUG_PROBE_ACTUAL\n"    + s);
            s = ""; foreach (int tS in DEBUG_LEVEL_INTENDED)    s += tS + "\n"; Debug.Log("DEBUG_LEVEL_INTENDED\n"  + s);
            s = ""; foreach (int tS in DEBUG_LEVEL_ACTUAL)      s += tS + "\n"; Debug.Log("DEBUG_LEVEL_ACTUAL\n"    + s);
            */
#endif
            string debug = GameTimings.GetTotalLogsAsString(totalLogs);
            Debug_Helper.Log(typeof(Precalculator), debug);

            ExperimentManagerSession.LogStimulusTimings_Queue(debug);

            return true;
        }

        public static int GetNumProbesInFirst25(bool isFMRIScanner)
        {
            return isFMRIScanner ? NUM_PROBES_IN_FIRST_25_FMRI : NUM_PROBES_IN_FIRST_25_NON_FMRI;
        }

        private static List<List<int>> WORLD_TO_PROBE = new List<List<int>>()
        {
            new List<int>() { 12, 13, 12, 13 },
            new List<int>() { 12, 13, 13, 12 },
            new List<int>() { 13, 12, 12, 13 },
            new List<int>() { 13, 12, 13, 12 },
        };

        #region FMRI
        public static int NUM_PROBES_IN_FIRST_25_FMRI = 10;

        private static Dictionary<int, double> PROBE_FREQUENCIES_FMRI = new Dictionary<int, double>()
        {
            { 10875, 0.300 },
            { 11625, 0.200 },
            { 12375, 0.150 },
            { 13125, 0.110 },
            { 13875, 0.080 },
            { 14625, 0.060 },
            { 15375, 0.040 },
            { 16125, 0.030 },
            { 16875, 0.020 },
            { 17625, 0.010 },
        };

        private static Dictionary<int, List<List<int>>> PROBE_TO_STIM_FMRI = new Dictionary<int, List<List<int>>>()
        {
            { 10875, new List<List<int>>()
            {
                new List<int>() { 7000, 4000,  },
                new List<int>() { 6700, 4000,  },
                new List<int>() { 6400, 4600,  },
                new List<int>() { 6400, 4300,  },
                new List<int>() { 6100, 4600,  },
                new List<int>() { 6100, 4900,  },
                new List<int>() { 5800, 4900,  },
                new List<int>() { 5800, 5200,  },
                new List<int>() { 5500, 5200,  },
                new List<int>() { 5500, 5500,  },
                new List<int>() { 6100, 4600,  },
            } },
            { 11625, new List<List<int>>()
            {
                new List<int>() { 7600, 4000,  },
                new List<int>() { 7300, 4300,  },
                new List<int>() { 7000, 4600,  },
                new List<int>() { 6700, 4900,  },
                new List<int>() { 6400, 5200,  },
                new List<int>() { 7600, 4000,  },
                new List<int>() { 7300, 4300,  },
                new List<int>() { 4000, 4000, 4000,  },
            } },
            { 12375, new List<List<int>>()
            {
                new List<int>() { 4000, 4000, 4300,  },
                new List<int>() { 4300, 4000, 4000,  },
                new List<int>() { 4300, 4300, 4000,  },
                new List<int>() { 4300, 4300, 4000,  },
                new List<int>() { 4300, 4000, 4000,  },
                new List<int>() { 4000, 4000, 4300,  },
            } },
            { 13125, new List<List<int>>()
            {
                new List<int>() { 4000, 4600, 4600,  },
                new List<int>() { 4000, 4300, 4900,  },
                new List<int>() { 4000, 4300, 4900,  },
                new List<int>() { 4000, 4600, 4600,  },
            } },
            { 13875, new List<List<int>>()
            {
                new List<int>() { 4900, 4300, 4600,  },
                new List<int>() { 4000, 4900, 4900,  },
                new List<int>() { 4600, 4600, 4600,  },
            } },
            { 14625, new List<List<int>>()
            {
                new List<int>() { 5200, 4300, 5200,  },
                new List<int>() { 5500, 4300, 4900,  },
            } },
            { 15375, new List<List<int>>()
            {
                new List<int>() { 5200, 5800, 4300,  },
            } },
            { 16125, new List<List<int>>()
            {
                new List<int>() { 4300, 4000, 4000, 4000 },
            } },
            { 16875, new List<List<int>>()
            {
                new List<int>() { 4600, 4300, 4000, 4000 },
            } },
            { 17625, new List<List<int>>()
            {
                new List<int>() { 4900, 4600, 4300, 4000 },
            } },
        };

        private static Dictionary<int, List<List<int>>> STIM_TO_BCKG_FMRI = new Dictionary<int, List<List<int>>>()
        {
            { 4000, new List<List<int>>()
            {
                new List<int>() { 2000, 2000,  },
                new List<int>() { 1900, 2000,  },
                new List<int>() { 1800, 1000, 1200,  },
                new List<int>() { 1600, 1000, 1400,  },
                new List<int>() { 1700, 1300, 1000,  },
                new List<int>() { 1800, 1000, 1200,  },
                new List<int>() { 1400, 1400, 1200,  },
                new List<int>() { 1300, 1300, 1400,  },
                new List<int>() { 1500, 1500, 1000,  },
                new List<int>() { 1300, 1100, 1600,  },
                new List<int>() { 1700, 1200, 1100,  },
                new List<int>() { 1400, 1600, 1000,  },
            } },
            { 4300, new List<List<int>>()
            {
                new List<int>() { 2000, 1300, 1000,  },
                new List<int>() { 1900, 1400, 1000,  },
                new List<int>() { 1700, 1600, 1000,  },
                new List<int>() { 1800, 1500, 1000,  },
                new List<int>() { 2000, 1100, 1200,  },
                new List<int>() { 1900, 1100, 1300,  },
                new List<int>() { 1700, 1200, 1400,  },
                new List<int>() { 1000, 1100, 1200, 1000 },
            } },
            { 4600, new List<List<int>>()
            {
                new List<int>() { 1600, 1000, 1000, 1000 },
                new List<int>() { 1300, 1000, 1200, 1100 },
                new List<int>() { 1400, 1100, 1000, 1100 },
                new List<int>() { 1500, 1100, 1000, 1000 },
                new List<int>() { 1000, 1100, 1200, 1300 },
                new List<int>() { 1000, 1100, 1200, 1300 },
            } },
            { 4900, new List<List<int>>()
            {
                new List<int>() { 1700, 1000, 1100, 1100 },
                new List<int>() { 1000, 1100, 1800, 1000 },
                new List<int>() { 1500, 1100, 1200, 1100 },
                new List<int>() { 1000, 1000, 1900, 1000 },
            } },
            { 5200, new List<List<int>>()
            {
                new List<int>() { 1000, 1100, 1800, 1300 },
                new List<int>() { 1600, 1200, 1200, 1200 },
                new List<int>() { 1400, 1300, 1200, 1300 },
                new List<int>() { 1100, 1100, 1500, 1500 },
            } },
            { 5500, new List<List<int>>()
            {
                new List<int>() { 1700, 1200, 1500, 1100 },
                new List<int>() { 1600, 1700, 1100, 1100 },
            } },
            { 5800, new List<List<int>>()
            {
                new List<int>() { 1900, 1500, 1400, 1000 },
                new List<int>() { 1800, 1600, 1100, 1300 },
            } },
            { 6100, new List<List<int>>()
            {
                new List<int>() { 1900, 1000, 1400, 1800 },
                new List<int>() { 1900, 1800, 1200, 1200 },
            } },
            { 6400, new List<List<int>>()
            {
                new List<int>() { 2000, 1600, 1800, 1000 },
                new List<int>() { 1900, 1700, 1300, 1500 },
            } },
            { 6700, new List<List<int>>()
            {
                new List<int>() { 2000, 1500, 1200, 1000 , 1000},
                new List<int>() { 1700, 1600, 1200, 1100 , 1100},
            } },
            { 7000, new List<List<int>>()
            {
                new List<int>() { 1900, 1600, 1500, 1000 , 1000},
                new List<int>() { 1800, 1600, 1200, 1400 , 1000},
            } },
            { 7300, new List<List<int>>()
            {
                new List<int>() { 1900, 1800, 1300, 1200 , 1100},
                new List<int>() { 2000, 1700, 1400, 1100 , 1100},
            } },
            { 7600, new List<List<int>>()
            {
                new List<int>() { 1900, 1700, 1300, 1300 , 1400},
                new List<int>() { 2000, 1300, 1500, 1800 , 1000},
            } },
        };
        #endregion

        #region NON-FMRI

        public static int NUM_PROBES_IN_FIRST_25_NON_FMRI = 8;

        private static Dictionary<int, double> PROBE_FREQUENCIES = new Dictionary<int, double>()
        { 
            { 9000,     1/11f },
            { 9900,     1/11f },
            { 10800,    1/11f },
            { 11700,    1/11f },
            { 12600,    1/11f },
            { 13500,    1/11f },
            { 14400,    1/11f },
            { 15300,    1/11f },
            { 16200,    1/11f },
            { 17100,    1/11f },
            { 18000,    1/11f },
        };    

        private static Dictionary<int, List<List<int>>> PROBE_TO_STIM = new Dictionary<int, List<List<int>>>()
        {
            { 9000, new List<List<int>>()
            {
                new List<int>() { 6000, 3000, },
                new List<int>() { 5700, 3300, },
                new List<int>() { 5400, 3600, },
            } },
            { 9900, new List<List<int>>()
            {
                new List<int>() { 5100, 4800, },
                new List<int>() { 5400, 4500, },
                new List<int>() { 6000, 3900, },
            } },
            { 10800, new List<List<int>>()
            {
                new List<int>() { 5100, 5700, },
                new List<int>() { 5100, 5400, },
                new List<int>() { 5400, 5400, },
            } },
            { 11700, new List<List<int>>()
            {
                new List<int>() { 6000, 5700, },
                new List<int>() { 5700, 6000, },
                new List<int>() { 4200, 3600, 3900, },
            } },
            { 12600, new List<List<int>>()
            {
                new List<int>() { 3300, 5100, 4200, },
                new List<int>() { 3000, 5400, 4200, },
                new List<int>() { 3000, 3600, 6000, },
            } },
            { 13500, new List<List<int>>()
            {
                new List<int>() { 5100, 3600, 4800, },
                new List<int>() { 4800, 4200, 4500, },
                new List<int>() { 5700, 3300, 4500, },
            } },
            { 14400, new List<List<int>>()
            {
                new List<int>() { 3600, 6000, 4800, },
                new List<int>() { 3900, 5700, 4800, },
                new List<int>() { 5700, 5400, 3300, },
            } },
            { 15300, new List<List<int>>()
            {
                new List<int>() { 5100, 5700, 4500, },
                new List<int>() { 4200, 3000, 3000, 5100,  },
                new List<int>() { 3300, 4200, 3000, 4800,  },
            } },
            { 16200, new List<List<int>>()
            {
                new List<int>() { 4500, 3900, 4500, 3300,  },
                new List<int>() { 3600, 4200, 4800, 3600,  },
                new List<int>() { 6000, 3000, 3300, 3900,  },
            } },
            { 17100, new List<List<int>>()
            {
                new List<int>() { 3600, 5400, 3900, 4500,  },
                new List<int>() { 3900, 5100, 3900, 4200,  },
                new List<int>() { 4800, 3300, 6000, 3000,  },
            } },
            { 18000, new List<List<int>>()
            {
                new List<int>() { 4200, 4800, 4500, 4500,  },
                new List<int>() { 5100, 3900, 6000, 3000,  },
                new List<int>() { 5400, 3600, 5700, 3300,  },
            } },
        };

        private static Dictionary<int, List<List<int>>> STIM_TO_BCKG = new Dictionary<int, List<List<int>>>()
        {
            { 3000, new List<List<int>>()
            {
                new List<int>() { 2000, 1000, },
                new List<int>() { 1900, 1100, },
                new List<int>() { 1800, 1200, },
            } },
            { 3300, new List<List<int>>()
            {
                new List<int>() { 1700, 1600, },
                new List<int>() { 1800, 1500, },
                new List<int>() { 2000, 1300, },
            } },
            { 3600, new List<List<int>>()
            {
                new List<int>() { 1900, 1700, },
                new List<int>() { 2000, 1600, },
                new List<int>() { 1800, 1700, },
            } },
            { 3900, new List<List<int>>()
            {
                new List<int>() { 2000, 1900, },
                new List<int>() { 1900, 2000, },
                new List<int>() { 1300, 1400, 1200, },
            } },
            { 4200, new List<List<int>>()
            {
                new List<int>() { 1400, 1500, 1300, },
                new List<int>() { 1400, 1600, 1200, },
                new List<int>() { 1500, 1600, 1100, },
            } },
            { 4500, new List<List<int>>()
            {
                new List<int>() { 1800, 1400, 1300, },
                new List<int>() { 2000, 1200, 1400, },
                new List<int>() { 1700, 1800, 1000, },
            } },
            { 4800, new List<List<int>>()
            {
                new List<int>() { 1900, 1100, 1800, },
                new List<int>() { 1900, 1000, 1900, },
                new List<int>() { 2000, 1200, 1600, },
            } },
            { 5100, new List<List<int>>()
            {
                new List<int>() { 1700, 1600, 1800, },
                new List<int>() { 1200, 1400, 1500, 1000 },
                new List<int>() { 1100, 1700, 1300, 1000 },
            } },
            { 5400, new List<List<int>>()
            {
                new List<int>() { 1400, 1100, 1900, 1000 },
                new List<int>() { 1400, 1300, 1600, 1100 },
                new List<int>() { 1500, 1600, 1300, 1000 },
            } },
            { 5700, new List<List<int>>()
            {
                new List<int>() { 1500, 1300, 1700, 1200 },
                new List<int>() { 1700, 1100, 1800, 1100 },
                new List<int>() { 1000, 1500, 2000, 1200 },
            } },
            { 6000, new List<List<int>>()
            {
                new List<int>() { 1400, 1600, 1300, 1700 },
                new List<int>() { 1200, 1800, 1900, 1100 },
                new List<int>() { 2000, 1000, 1500, 1500 },
            } },
        };
        #endregion

        private static bool _PrecalculateGameTimings_OLD(out GameTimings timings)
        {
            timings = new GameTimings();
#if false

            List<int> DEBUG_PROBE_INTENDED = new List<int>();
            List<int> DEBUG_STIM_INTENDED = new List<int>();
            List<int> DEBUG_BACKGROUND = new List<int>();
            List<int> DEBUG_STIM_ACTUAL = new List<int>();
            List<int> DEBUG_PROBE_ACTUAL = new List<int>();
            List<int> DEBUG_LEVEL_INTENDED = new List<int>();
            List<int> DEBUG_LEVEL_ACTUAL = new List<int>();

            // ---- Calculate intended probe delays!
#region Intended Probe
            List<EventInformation> cumulativeProbeDelays_Intended = new List<EventInformation>();

            bool isFMRIScanner = ExperimentManagerSession.module == ModuleType.FMRI_Scanner;

            Dictionary<float, float> probeCumDistrib = ProbeManager.GetTruncatedExpDistribution(isFMRIScanner);
            // Debug.LogError(probeCumDistrib.ToReadableString());

            // For all probes
            float currentProbeCumulativeDelay = 0;

            int numWorlds = PlayerProgression.numGameWorlds;
            int numLevelsPerWorld = PlayerProgression.numLevelsPerWorld;
            int probesPerWorld = ExperimentManagerSession.GetWantedNumProbesPerWorld();
            int probesTotal = ExperimentManagerSession.GetWantedNumProbesTotal();

#if UNITY_EDITOR
            Debug.LogWarning("Calculating Timings for {0} Probes"._Format(probesTotal));
#endif

            for (int i = 0; i < probesTotal; i++)
            {
                float delay = Math_Helper.QueryCDF(probeCumDistrib);
                currentProbeCumulativeDelay += delay;

                DEBUG_PROBE_INTENDED.Add(Mathf.RoundToInt(delay * 1000));

                cumulativeProbeDelays_Intended.Add(new EventInformation(
                    cumulativeProbeDelays_Intended.Count, delay, currentProbeCumulativeDelay));
            }

            Debug_Helper.Log(typeof(Precalculator), "Intended Probes :: {0}, {1}, {2}"._Format(currentProbeCumulativeDelay,
                cumulativeProbeDelays_Intended.Count,
                cumulativeProbeDelays_Intended.ToReadableString())); // [CHECK!]
#endregion


            // ---- Calculate intended stimulus delays!
#region Intended Stimulus
            List<EventInformation> cumulativeStimulusDelays_Intended = new List<EventInformation>();


            float currentStimulusCumulativeDelay = 0;
            // We need enough stimulus to cover all our probes
            // (we actually need a bit less but it's okay)
            while (currentStimulusCumulativeDelay < currentProbeCumulativeDelay)
            {
                float randomDelay = ExperimentLibraryManager.Config.GetPeriodRandom_Stimulus(false, isFMRIScanner); // Game

                currentStimulusCumulativeDelay += randomDelay;
                DEBUG_STIM_INTENDED.Add(Mathf.RoundToInt(randomDelay * 1000));

                cumulativeStimulusDelays_Intended.Add(new EventInformation(
                    cumulativeStimulusDelays_Intended.Count, randomDelay, currentStimulusCumulativeDelay));
            }

            Debug_Helper.Log(typeof(Precalculator), "Intended Stimuli :: {0}, {1}, {2}"._Format(currentStimulusCumulativeDelay,
                cumulativeStimulusDelays_Intended.Count,
                cumulativeStimulusDelays_Intended.ToReadableString())); // [CHECK!]
#endregion


            // ---- Calculate intended (also actual) background delays
#region Background
            timings.backgroundPeaks = new List<EventInformation>();

            MinMax minMax_Background = ExperimentLibraryManager.Config.GetPeriodMinMax_Background();

            float currentBackgroundCumulativeDelay = 0;
            // We need enough background to cover all our stimulus
            // (we actually need a bit more)
            float peakDistanceFromCycleEnd =
                ExperimentLibraryManager.Config.Experiment.stimulus.background.peakDuration; // [DEPRECATED? 200508]  + ApplicationLibrary.Config.Experiment.stimulus.background.peakEaseOut;

            while (currentBackgroundCumulativeDelay < currentStimulusCumulativeDelay + peakDistanceFromCycleEnd)
            {
                float delay = Utility_Helper.RandomRange(minMax_Background.min, minMax_Background.max);
                currentBackgroundCumulativeDelay += delay;
                DEBUG_BACKGROUND.Add(Mathf.RoundToInt(delay * 1000));

                timings.backgroundPeaks.Add(new EventInformation(
                    timings.backgroundPeaks.Count, delay, currentBackgroundCumulativeDelay));
            }


            Debug_Helper.Log(typeof(Precalculator), "Background {0}, {1}, {2}"._Format(currentBackgroundCumulativeDelay,
                timings.backgroundPeaks.Count,
                timings.backgroundPeaks.ToReadableString())); // [CHECK!]
#endregion


            // ---- Calculate actual stimulus delays
#region Actual Stimulus
            timings.stimulusOccurences_Actual = new List<EventInformation>();

            // For each of the stimuli find the background cycle with the closest cumulative delay and snap to it
            int lastBckgIdx = 0;
            double timestampOfLastStimulus_Actual = 0;

            for (int stimIdx = 0; stimIdx < cumulativeStimulusDelays_Intended.Count; stimIdx++)
            {
                EventInformation stimulusDelay_Intended = cumulativeStimulusDelays_Intended[stimIdx];
                double tStimulus_Intended = stimulusDelay_Intended.timestamp;

                double largestUnder = -1;
                int bckgCycleIdx = -1;
                double cumDelay = -1;

                for (int bckgIdx = lastBckgIdx; bckgIdx < timings.backgroundPeaks.Count; bckgIdx++)
                {
                    double tBackgroundCycleEnd = timings.backgroundPeaks[bckgIdx].timestamp;

                    // Stimulus appears at PEAK of background
                    tBackgroundCycleEnd -= peakDistanceFromCycleEnd;

                    // Are we over?
                    if (tBackgroundCycleEnd > tStimulus_Intended)
                    {
                        // Are we closest to the largest under (or the smallest over)?
                        if (tStimulus_Intended - largestUnder < tBackgroundCycleEnd - tStimulus_Intended)
                        {
                            cumDelay = largestUnder;
                            bckgCycleIdx = bckgIdx - 1;
                        }
                        else
                        {
                            cumDelay = tBackgroundCycleEnd;
                            bckgCycleIdx = bckgIdx;
                        }

                        break;
                    }

                    largestUnder = tBackgroundCycleEnd;
                }

                // So we have aligned this stimulus with one of the background peaks
                EventInformation stimulusDelay_Actual = new EventInformation(0);
                stimulusDelay_Actual.id = stimulusDelay_Intended.id;
                stimulusDelay_Actual.triggeredByEventID = bckgCycleIdx;
                stimulusDelay_Actual.timestamp = cumDelay;
                stimulusDelay_Actual.delayFromPrevious = stimulusDelay_Actual.timestamp - timestampOfLastStimulus_Actual;
                stimulusDelay_Actual.delay_Intended = stimulusDelay_Intended.delayFromPrevious;
                stimulusDelay_Actual.timestamp_Intended = stimulusDelay_Intended.timestamp;

                DEBUG_STIM_ACTUAL.Add(Mathf.RoundToInt((float)stimulusDelay_Actual.delayFromPrevious * 1000));

                // Update the Background info
                EventInformation eI_Background_Temp = timings.backgroundPeaks[bckgCycleIdx];
                eI_Background_Temp.triggersEventID = stimulusDelay_Actual.id;
                timings.backgroundPeaks[bckgCycleIdx] = eI_Background_Temp;

                timings.stimulusOccurences_Actual.Add(stimulusDelay_Actual);

                timestampOfLastStimulus_Actual = stimulusDelay_Actual.timestamp;

                lastBckgIdx = bckgCycleIdx;
            }

            Debug_Helper.Log(typeof(Precalculator), "Actual Stimuli :: {0}, {1}, {2}"._Format(timestampOfLastStimulus_Actual,
                timings.stimulusOccurences_Actual.Count,
                timings.stimulusOccurences_Actual.ToReadableString())); // [CHECK!]
#endregion


            // ---- Calculate actual probe delays
#region Actual Probe
            timings.probeOccurences_Actual = new List<EventInformation>();

            // For each of the probes find the stimulus cycle with the closest cumulative delay and snap to it
            int lastStimIdx = 0;
            double cumulativeProbeDelay_Actual = 0;
            double stimulusDuration = ExperimentLibraryManager.Config.Experiment.stimulus.background.peakDuration; // ApplicationLibrary.Config.Experiment.stimulus.stimulusDurationSeconds;
            double probeDelayAfterStimulus = ExperimentLibraryManager.Config.Probes.probeDelayAfterStimulusMS / 1000f;

            for (int probeIdx = 0; probeIdx < cumulativeProbeDelays_Intended.Count; probeIdx++)
            {
                EventInformation probeDelay_Intended = cumulativeProbeDelays_Intended[probeIdx];
                double tProbe_Intended = probeDelay_Intended.timestamp;

                double largestUnder = -1;
                int stimCycleIdx = -1;
                double cumDelay = -1;

                for (int stimIdx = lastStimIdx; stimIdx < timings.stimulusOccurences_Actual.Count; stimIdx++)
                {
                    double tStimulus = timings.stimulusOccurences_Actual[stimIdx].timestamp;

                    // We need to wait for the stimulus to DISAPPEAR (after it appears)
                    tStimulus += stimulusDuration;

                    // We need to wait a bit after the stimulus disappeared, before we can actually show probe
                    tStimulus += probeDelayAfterStimulus;

                    // Are we over?
                    if (tStimulus > tProbe_Intended)
                    {
                        // Are we closest to the largest under (or the smallest over)?
                        if (tProbe_Intended - largestUnder < tStimulus - tProbe_Intended)
                        {
                            cumDelay = largestUnder;
                            stimCycleIdx = stimIdx - 1;
                        }
                        else
                        {
                            cumDelay = tStimulus;
                            stimCycleIdx = stimIdx;
                        }

                        break;
                    }

                    largestUnder = tStimulus;
                }

                // So we have aligned this stimulus with one of the background peaks
                EventInformation probeDelay_Actual = new EventInformation(0);
                probeDelay_Actual.id = probeDelay_Intended.id;
                probeDelay_Actual.triggeredByEventID = stimCycleIdx;
                probeDelay_Actual.timestamp = cumDelay;
                probeDelay_Actual.delayFromPrevious = probeDelay_Actual.timestamp - cumulativeProbeDelay_Actual;
                probeDelay_Actual.delay_Intended = probeDelay_Intended.delayFromPrevious;
                probeDelay_Actual.timestamp_Intended = probeDelay_Intended.timestamp;

                DEBUG_PROBE_ACTUAL.Add(Mathf.RoundToInt((float)probeDelay_Actual.delayFromPrevious * 1000));

                // Update the stimulus info
                if (stimCycleIdx >= timings.stimulusOccurences_Actual.Count)
                {
                    Debug_Helper.LogWarning(typeof(Precalculator), "Stim cycle Idx :: {0} >= {1} - trying again!".
                        _Format(stimCycleIdx, timings.stimulusOccurences_Actual.Count));
                    return false;
                }

                if (stimCycleIdx < 0)
                {
                    Debug_Helper.LogWarning(typeof(Precalculator), "Stim cycle Idx :: {0} < 0 - trying again!".
                        _Format(stimCycleIdx));
                    return false;
                }

                EventInformation dC_Stim_temp = timings.stimulusOccurences_Actual[stimCycleIdx];
                dC_Stim_temp.triggersEventID = probeDelay_Actual.id;
                timings.stimulusOccurences_Actual[stimCycleIdx] = dC_Stim_temp;

                timings.probeOccurences_Actual.Add(probeDelay_Actual);

                cumulativeProbeDelay_Actual = probeDelay_Actual.timestamp;

                lastStimIdx = stimCycleIdx;
            }

            Debug_Helper.Log(typeof(Precalculator), "Actual Probes :: {0}, {1}, {2}"._Format(cumulativeProbeDelay_Actual,
                timings.probeOccurences_Actual.Count,
                timings.probeOccurences_Actual.ToReadableString())); // [CHECK!]
#endregion


            // ---- Calculate level durations
#region Intended Level
            double experimentDuration_Actual = timings.probeOccurences_Actual.GetLast().timestamp;
            double worldDuration_Intended = experimentDuration_Actual / numWorlds;


            Dictionary<LevelConfig, double> worldLevelDurations_Config = new Dictionary<LevelConfig, double>();
            List<EventInformation> cumulativeLevelDurations_Intended = new List<EventInformation>();

            double cumulativeDuration_Intended = 0;

            for (int worldIdx = 0; worldIdx < numWorlds; worldIdx++)
            {
                double totalDurationWorld_Config = 0;

                for (int l = 1; l <= numLevelsPerWorld; l++)
                {
                    int levelIdx = worldIdx * numLevelsPerWorld + l;

                    LevelConfig level = LevelsLibrary.GetLevel(levelIdx);
                    double duration = LevelsLibrary.GetLevelDuration(level);
                    totalDurationWorld_Config += duration;

                    worldLevelDurations_Config.Add(level, duration);
                }

                foreach (LevelConfig level in worldLevelDurations_Config.Keys)
                {
                    double durationIntended = worldLevelDurations_Config[level] /
                        totalDurationWorld_Config * worldDuration_Intended;
                    cumulativeDuration_Intended += durationIntended;

                    DEBUG_LEVEL_INTENDED.Add(Mathf.RoundToInt((float)durationIntended * 1000));

                    cumulativeLevelDurations_Intended.Add(new EventInformation(level.levelID_1Based, durationIntended, cumulativeDuration_Intended));
                }

                worldLevelDurations_Config.Clear();
            }

            Debug_Helper.Log(typeof(Precalculator), "Level Durations Intended :: {0}, {1}, {2}"._Format(cumulativeDuration_Intended,
                cumulativeLevelDurations_Intended.Count,
                cumulativeLevelDurations_Intended.ToReadableString())); // [CHECK!]
#endregion


            // ---- Calculate actual level durations
#region Actual Level
            timings.cumulativeLevelDurations_Actual = new List<EventInformation>();

            // For each of the levels find the probe with the closest cumulative delay and snap to it
            int lastProbeIdx = 0;
            double cumulativeLevelDuration_Actual = 0;
            double probeShieldBeforeEndOfLevel = ExperimentLibraryManager.Config.Probes.probeShieldBeforeEndOfLevelMS / 1000f;

            for (int levelIdx = 0; levelIdx < cumulativeLevelDurations_Intended.Count; levelIdx++)
            {
                EventInformation levelDuration_Intended = cumulativeLevelDurations_Intended[levelIdx];
                double tLevel_Intended = levelDuration_Intended.timestamp;

                int probeCycleIdx = -1;
                double cumDelay = -1;

                /* // If we are the last level of each world, override
                 if (levelIdx % PlayerProgression.numLevelsPerWorld == PlayerProgression.numLevelsPerWorld - 1)
                 {
                     int worldID = Mathf.FloorToInt((float)levelIdx / TheManager.numWorlds);
                     probeCycleIdx = ProbeManager.numProbesPerWorld * worldID;

                     cumDelay = timings.probeOccurences_Actual[probeCycleIdx].timestamp;
                 }
                 else */
                {
                    double largestUnder = -1;

                    for (int probeIdx = lastProbeIdx; probeIdx < timings.probeOccurences_Actual.Count; probeIdx++)
                    {
                        double tProbe = timings.probeOccurences_Actual[probeIdx].timestamp;

                        // Factor in probe shield (we can't end a level right after a probe
                        tProbe += probeShieldBeforeEndOfLevel;

                        // Are we over?
                        if (tProbe > tLevel_Intended)
                        {
                            // Are we closest to the largest under (or the smallest over)?
                            if (tLevel_Intended - largestUnder < tProbe - tLevel_Intended)
                            {
                                cumDelay = largestUnder;
                                probeCycleIdx = probeIdx - 1;
                            }
                            else
                            {
                                cumDelay = tProbe;
                                probeCycleIdx = probeIdx;
                            }

                            break;
                        }

                        largestUnder = tProbe;
                    }
                }

                // So we have aligned this stimulus with one of the background peaks
                EventInformation levelDuration_Actual = new EventInformation(0);
                levelDuration_Actual.id = levelDuration_Intended.id;
                levelDuration_Actual.triggeredByEventID = probeCycleIdx;
                levelDuration_Actual.timestamp = cumDelay;
                levelDuration_Actual.delayFromPrevious = levelDuration_Actual.timestamp - cumulativeLevelDuration_Actual;
                levelDuration_Actual.delay_Intended = levelDuration_Intended.delayFromPrevious;
                levelDuration_Actual.timestamp_Intended = levelDuration_Intended.timestamp;

                DEBUG_LEVEL_ACTUAL.Add(Mathf.RoundToInt((float)levelDuration_Actual.delayFromPrevious * 1000));

                // Update the Probe info
                EventInformation eI_Probe_Temp = timings.probeOccurences_Actual[probeCycleIdx];
                eI_Probe_Temp.triggersEventID = levelDuration_Actual.id;
                timings.probeOccurences_Actual[probeCycleIdx] = eI_Probe_Temp;

                timings.cumulativeLevelDurations_Actual.Add(levelDuration_Actual);

                cumulativeLevelDuration_Actual = levelDuration_Actual.timestamp;

                lastProbeIdx = probeCycleIdx;
            }

            Debug_Helper.Log(typeof(Precalculator), "Level Durations Actual :: {0}, {1}, {2}"._Format(cumulativeLevelDuration_Actual,
                timings.cumulativeLevelDurations_Actual.Count,
                timings.cumulativeLevelDurations_Actual.ToReadableString())); // [CHECK!]
#endregion


#region CONDENSE LOGS
            // Log
            List<EventInformation> totalLogs = new List<EventInformation>();

            // Backgrounds
            foreach (EventInformation eI in timings.backgroundPeaks)
            {
                // Tag it
                eI.SetEventType(EventType.Background.ToString());

                // Bag it
                totalLogs.Add(eI);
            }

            // Stimuli
            foreach (EventInformation eI in timings.stimulusOccurences_Actual)
            {
                // Tag it
                eI.SetEventType(EventType.Stimulus.ToString());

                // Bag it
                totalLogs.Add(eI);
            }

            // Probes
            foreach (EventInformation eI in timings.probeOccurences_Actual)
            {
                // Tag it
                eI.SetEventType(EventType.Probe.ToString());

                // Bag it
                totalLogs.Add(eI);
            }

            // Levels
            foreach (EventInformation eI in timings.cumulativeLevelDurations_Actual)
            {
                // Tag it
                eI.SetEventType(EventType.Level.ToString());

                // Bag it
                totalLogs.Add(eI);
            }

            totalLogs = totalLogs.CustomOrderBy(a => a.timestamp, Order.Ascending);
#endregion


#region FINAL CHECK
            int currentWorld = 0;
            List<int> check_ProbesPerWorld = new List<int>();
            for (int i = 0; i < PlayerProgression.numGameWorlds; i++)
                check_ProbesPerWorld.Add(0);

            foreach (EventInformation eI in totalLogs)
            {
                if (eI.eventType == EventType.Level.ToString())
                    currentWorld = Mathf.FloorToInt((float)eI.id / PlayerProgression.numLevelsPerWorld);

                if (eI.eventType == EventType.Probe.ToString())
                    check_ProbesPerWorld[currentWorld]++;
            }

            for (int i = 0; i < PlayerProgression.numGameWorlds; i++)
                if (check_ProbesPerWorld[i] != probesPerWorld)
                {
                    Debug_Helper.LogWarning(typeof(Precalculator), "World {0} had {1} instead of {2} Probes. Recalculating.".
                        _Format(i, check_ProbesPerWorld[i], probesPerWorld));
                    return false;
                }
#endregion


#if UNITY_EDITOR
            /*
            string s = "";
            s = ""; foreach (int tS in DEBUG_BACKGROUND)        s += tS + "\n"; Debug.Log("DEBUG_BACKGROUND\n"      + s);
            s = ""; foreach (int tS in DEBUG_STIM_INTENDED)     s += tS + "\n"; Debug.Log("DEBUG_STIM_INTENDED\n"   + s);
            s = ""; foreach (int tS in DEBUG_STIM_ACTUAL)       s += tS + "\n"; Debug.Log("DEBUG_STIM_ACTUAL\n"     + s);
            s = ""; foreach (int tS in DEBUG_PROBE_INTENDED)    s += tS + "\n"; Debug.Log("DEBUG_PROBE_INTENDED\n"  + s);
            s = ""; foreach (int tS in DEBUG_PROBE_ACTUAL)      s += tS + "\n"; Debug.Log("DEBUG_PROBE_ACTUAL\n"    + s);
            s = ""; foreach (int tS in DEBUG_LEVEL_INTENDED)    s += tS + "\n"; Debug.Log("DEBUG_LEVEL_INTENDED\n"  + s);
            s = ""; foreach (int tS in DEBUG_LEVEL_ACTUAL)      s += tS + "\n"; Debug.Log("DEBUG_LEVEL_ACTUAL\n"    + s);
            */
#endif

            string debug = "timestamp, eventType, delayFromPrevious, id, triggeredByEventID, triggersEventID\r\n";
            foreach (EventInformation eI in totalLogs)
                debug += eI.ToStringDetailed();
            Debug_Helper.Log(typeof(Precalculator), debug);

            ExperimentManagerSession.LogStimulusTimings(debug);

#endif
            return true;
        }
    }

    public struct AllIndices
    {
        public int levelIdx_1Based;
        public int probeIdx;
        public int stimIdx;
        public int bckgIdx;

        public AllIndices(int levelIdx, int probeIdx, int stimIdx, int bckgIdx)
        {
            this.levelIdx_1Based = levelIdx;
            this.probeIdx = probeIdx;
            this.stimIdx = stimIdx;
            this.bckgIdx = bckgIdx;
        }

        public override string ToString()
        {
            return "Level :: {0} | Bckg :: {1} | Stim :: {2} | Probe :: {3}"._Format(levelIdx_1Based, bckgIdx, stimIdx, probeIdx);
        }
    }

    [System.Serializable]
    public struct GameTimings
    {
        // cumulativeBackgroundDelays to define when the background peaks
        public List<EventInformation> backgroundPeaks;

        // Define when stimuli are shown (easiest to use lowerCycleIdx and count background peaks)
        public List<EventInformation> stimulusOccurences_Actual;

        // Define when probes are shown (easiest to use lowerCycleIdx and count stimuli shown)
        public List<EventInformation> probeOccurences_Actual;

        // Define when levels end (easiest to use lowerCycleIdx and count probes shown)
        public List<EventInformation> cumulativeLevelDurations_Actual;

        public GameTimings(int args)
        {
            backgroundPeaks = new List<EventInformation>();
            stimulusOccurences_Actual = new List<EventInformation>();
            probeOccurences_Actual = new List<EventInformation>();
            cumulativeLevelDurations_Actual = new List<EventInformation>();
        }

        internal static GameTimings FromCSV(IList<string> timingsFileContents)
        {
            GameTimings gameTimings = new GameTimings(1);

            // Account for the config differences
            // We need enough background to cover all our stimulus
            // (we actually need a bit more)
            float stimOnsetFromCycleBegin =
                ExperimentLibraryManager.Config.Experiment.stimulus.background.cycleDuration / 2f;

            // We need enough background to cover all our stimulus
            // (we actually need a bit more)
            float probeFromStimOnset =
                ExperimentLibraryManager.Config.Experiment.stimulus.background.peakDuration +       // Peak ends
                ExperimentLibraryManager.Config.Experiment.stimulus.background.cycleDuration / 2f + // We shrink
                ExperimentLibraryManager.Config.Probes.probeDelayAfterStimulus;                     // - and then we probe

            float levelEndFromProbeOnset =
                ExperimentLibraryManager.Config.PostLevelCleanupSafety;
            // ExperimentLibraryManager.Config.Probes.probeShieldBeforeEndOfLevelMS / 1000f;

            double globalOffset = 0;

            double stimDT = stimOnsetFromCycleBegin;
            double probeDT = stimDT + probeFromStimOnset;
            double levelDT = probeDT + levelEndFromProbeOnset;

            foreach (string line in timingsFileContents)
            {
                EventInformation lineEventInfo = EventInformation.FromCSV(line);

                EventType eventType = lineEventInfo.eventType.ToEnum<EventType>();

                lineEventInfo.timestamp += globalOffset;

                switch (eventType)
                {
                    case EventType.None:
                        break;
                    case EventType.Background:
                        gameTimings.backgroundPeaks.Add(lineEventInfo);
                        break;
                    case EventType.Stimulus:
                        lineEventInfo.timestamp += stimDT;
                        gameTimings.stimulusOccurences_Actual.Add(lineEventInfo);
                        break;
                    case EventType.Probe:
                        lineEventInfo.timestamp += probeDT;
                        gameTimings.probeOccurences_Actual.Add(lineEventInfo);
                        break;
                    case EventType.Level:
                        lineEventInfo.timestamp += levelDT;
                        globalOffset += levelDT;
                        gameTimings.cumulativeLevelDurations_Actual.Add(lineEventInfo);
                        break;
                }
            }

            return gameTimings;
        }

        public string GetTotalLogsAsString()
        {
            return GetTotalLogsAsString(GenerateTotalLogs());
        }

        public static string GetTotalLogsAsString(List<EventInformation> totalLogs)
        {
            string debug = "timestamp, eventType, delayFromPrevious, id, triggeredByEventID, triggersEventID\r\n";
            foreach (EventInformation eI in totalLogs)
                debug += eI.ToStringDetailed();
            return debug;
        }

        public List<EventInformation> GenerateTotalLogs()
        {
            List<EventInformation> totalLogs = new List<EventInformation>();

            // Backgrounds
            foreach (EventInformation eI in backgroundPeaks)
            {
                // Tag it
                eI.SetEventType(EventType.Background.ToString());

                // Bag it
                totalLogs.Add(eI);
            }

            // Stimuli
            foreach (EventInformation eI in stimulusOccurences_Actual)
            {
                // Tag it
                eI.SetEventType(EventType.Stimulus.ToString());

                // Bag it
                totalLogs.Add(eI);
            }

            // Probes
            foreach (EventInformation eI in probeOccurences_Actual)
            {
                // Tag it
                eI.SetEventType(EventType.Probe.ToString());

                // Bag it
                totalLogs.Add(eI);
            }

            // Levels
            foreach (EventInformation eI in cumulativeLevelDurations_Actual)
            {
                // Tag it
                eI.SetEventType(EventType.Level.ToString());

                // Bag it
                totalLogs.Add(eI);
            }

            totalLogs = totalLogs.CustomOrderBy(a => a.timestamp * 10 + 
                (a.eventType.ContainsInvariant("background") ? 1 :
                a.eventType.ContainsInvariant("stimulus") ? 2 :
                a.eventType.ContainsInvariant("probe") ? 3 : 
                a.eventType.ContainsInvariant("level") ? 4 : 0),
            Order.Ascending);

            return totalLogs;
        }
    }

    /*
    [System.Serializable]
    public struct LocalizerTimings
    {
        // cumulativeBackgroundDelays to define when the background peaks
        public List<EventInformation> backgroundPeaks;

        public LocalizerTimings(int args)
        {
            backgroundPeaks = new List<EventInformation>();
        }
    }
    */
}