// NS_REMOVE | Configs, 1 Log
using ExperimentLibrary;
using Experiment.Managers;

// NS_DEBATABLE | Could become runtime config
using Game.Managers.SessionManagers;

using Helpers.Async;
using Helpers.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using TGP.Helpers;
using UnityEngine;
using Experiment.Task;

namespace Experiment.Stimulus
{
    public class StimulusManager_QueueGeneratorManager
    {
        // public static EventHandler onQueuesSetup = null;

#if UNITY_EDITOR
        private static bool editorOnlyJustOneQueue = false;
#endif
        private static readonly bool EDITOR_ONLY_RUN_ON_THREAD = false;

        public static readonly bool queuesOnThread =
#if UNITY_EDITOR
        EDITOR_ONLY_RUN_ON_THREAD
#else
        true // Always opt for thread outside the editor!
#endif
        ;

        private static int queuesSetup = 0;
        private static int queuesNeeded = 0;

        // This runs sequentially
        public static void TrySetupQueues_NEW(bool isFMRI, int subjectID,
            List<List<EventType>> sequence_PerGameWorld,
            List<int> localizerStartIDInSequenceOfWorld,
            List<Sprite> stimuliTexturesFaces, List<Sprite> stimuliTexturesObjects,
            List<List<SpriteLocation>> orderedStimuli_GameWorlds,
            List<List<SpriteLocation>> orderedStimuli_Localizers,
            Dictionary<int, QueueScore> scorePerWorld,
            Dictionary<int, Dictionary<int, int>> worldToLevelStartPointsWithinWorld_0Based,
            Action<bool> callback)
        {
            bool doDebug = false;

            Debug_Helper.Log(typeof(StimulusManager), "SETTING UP QUEUES");

            int numberOfWorlds = sequence_PerGameWorld.Count;
            int numberOfLocalizers = PlayerProgression.numLocalizers;
            int numberOfLocalizersPerWorld = numberOfLocalizers / numberOfWorlds;
            int numWantedPerLocalizer = ExperimentManagerSession.wantedNumStimuli_PerLocalizer;
            scorePerWorld.Clear();

            queuesSetup = 0;
            queuesNeeded = numberOfWorlds + numberOfLocalizers;

            float percentileBlank_Gameplay = ExperimentLibraryManager.Config.Experiment.stimulus.blankVsNormalRatio_Gameplay;
            float percentileBlank_Localizer = ExperimentLibraryManager.Config.Experiment.stimulus.blankVsNormalRatio_Localizer;

            Action<List<Option>, string> check = (options, label) =>
            {
                Dictionary<Direction_2D_Diagonal, int> counts = new Dictionary<Direction_2D_Diagonal, int>();

                foreach (Direction_2D_Diagonal d2D in Utility_Helper.EnumGetValues<Direction_2D_Diagonal>())
                    counts.Add(d2D, options.Count(a => a.type == StimulusType.None && a.location == d2D));

                // Debug.Log(label + "\n" + counts.ToReadableString());
            };

            check(StimulusManager_QueueGenerator_Helper_FMRI.optionsLocalizerW1L1, "W1L1");
            check(StimulusManager_QueueGenerator_Helper_FMRI.optionsLocalizerW1L2, "W1L2");
            check(StimulusManager_QueueGenerator_Helper_FMRI.optionsLocalizerW2L1, "W2L1");
            check(StimulusManager_QueueGenerator_Helper_FMRI.optionsLocalizerW2L2, "W2L2");
            check(StimulusManager_QueueGenerator_Helper_FMRI.optionsLocalizerW3L1, "W3L1");
            check(StimulusManager_QueueGenerator_Helper_FMRI.optionsLocalizerW3L2, "W3L2");
            check(StimulusManager_QueueGenerator_Helper_FMRI.optionsLocalizerW4L1, "W4L1");
            check(StimulusManager_QueueGenerator_Helper_FMRI.optionsLocalizerW4L2, "W4L2");
            // return;
            for (int i = 0; i < numberOfWorlds; i++)
            {
                string worldName = "W{0}"._Format(i + 1);
                string fileName_Queue_World = string.Format("queue-{0}.txt", worldName);

                List<SpriteLocation> spriteLocations_World = new List<SpriteLocation>();
                orderedStimuli_GameWorlds.Add(spriteLocations_World);

                Dictionary<int, int> localizerID_ToStartIDWithinWorld = new Dictionary<int, int>();

                for (int j = 0; j < numberOfLocalizersPerWorld; j++)
                {
                    int localizerID = i * numberOfLocalizersPerWorld + j;
                    // Debug.LogError(localizerID + " : " + localizerStartIDInSequenceOfWorld.ToReadableString());

                    int startID = localizerStartIDInSequenceOfWorld[localizerID];

                    localizerID_ToStartIDWithinWorld.Add(localizerID, startID);
                }

                StimulusManager_QueueGenerator.SetupQueue_NEW(isFMRI, subjectID, sequence_PerGameWorld[i], i, localizerID_ToStartIDWithinWorld.Values.ToList(), worldToLevelStartPointsWithinWorld_0Based[i],
                    numWantedPerLocalizer, spriteLocations_World, stimuliTexturesFaces, stimuliTexturesObjects, percentileBlank_Gameplay, percentileBlank_Localizer, fileName_Queue_World, scoreWorld =>
                    {
                        string fileName_Analysis_World = string.Format("analysis-{0}.txt", worldName);
                        StimulusManager_QueueAnalyzer.AnalyzeQueue(spriteLocations_World, fileName_Analysis_World);

                        // Mark this one as done
                        scorePerWorld.Add(i, scoreWorld);
                        CompleteQueue(callback);

                        // Work the relevant localizers of this world
                        foreach (KeyValuePair<int, int> kVP in localizerID_ToStartIDWithinWorld)
                        {
                            int localizerID = kVP.Key;
                            int startID = kVP.Value;

                            string localizerName = "L{0}"._Format(localizerID + 1);
                            string fileName_Queue_Localizer = string.Format("queue-{0}.txt", localizerName);

                            List<SpriteLocation> spriteLocations_Localizer = new List<SpriteLocation>();

                            for (int q = startID; q < startID + numWantedPerLocalizer; q++)
                                spriteLocations_Localizer.Add(spriteLocations_World[q]);

                            string finalReport =
                                "Stimulus Queue {0}"._Format(spriteLocations_Localizer.ToReadableString("Queue Contents", queuesOnThread));

                            if (doDebug) ExperimentManagerSession.LogStimulusQueue(fileName_Queue_Localizer, finalReport);

                            orderedStimuli_Localizers.Add(spriteLocations_Localizer);

                            string fileName_Analysis_Localizer = string.Format("analysis-{0}.txt", localizerName);
                            StimulusManager_QueueAnalyzer.AnalyzeQueue(spriteLocations_Localizer, fileName_Analysis_Localizer);
                            CompleteQueue(callback);
                        }

                    }, queuesOnThread);

#if UNITY_EDITOR
                if (editorOnlyJustOneQueue)
                {
                    Debug.LogWarning("[Editor Only] Early out because one queue requested");
                    return;
                }
#endif
            }
        }

        public static void TrySetupQueues_OLD(
            List<Sprite> stimuliTexturesFaces, List<Sprite> stimuliTexturesObjects,
            List<List<SpriteLocation>> orderedStimuli_GameWorlds,
            List<List<SpriteLocation>> orderedStimuli_Localizers,
            Action<bool> callback)
        {
            Debug_Helper.Log(typeof(StimulusManager), "SETTING UP QUEUES");

            int numberOfWorlds = PlayerProgression.numGameWorlds;
            int numberOfLocalizers = PlayerProgression.numLocalizers;
            queuesSetup = 0;
            queuesNeeded = numberOfWorlds + numberOfLocalizers;

            float threshold = ExperimentLibraryManager.Config.Probes.minResetT_Threshold;
            int maxSearch = ExperimentLibraryManager.Config.Probes.minResetT_maxSearch;

            // --- GAME
            int numWantedPerWorld = ExperimentManagerSession.GetWantedNumProbesPerWorld();
            float percentileBlank_Game = ExperimentLibraryManager.Config.Experiment.stimulus.blankVsNormalRatio_Gameplay;
            float percentileFaces_Game = (1 - percentileBlank_Game) * ExperimentLibraryManager.Config.Experiment.stimulus.facesVsObjectsRatio_Gameplay;
            float percentileObjects_Game = 1 - percentileFaces_Game - percentileBlank_Game;

            // Least amount of stim for a proper distribution of types (40-40-20 -> 2-2-1 -> 5)
            int reset_TypeConstraintEvery_Game = GetMinResetT(percentileFaces_Game,
                percentileObjects_Game, percentileBlank_Game, threshold, maxSearch);
            // 5; // Mathf.RoundToInt(numWantedPerWorld * reset_TypeConstraintEvery);

            for (int i = 0; i < numberOfWorlds; i++)
            {
                string worldName = "W{0}"._Format(i + 1);
                string fileName_Queue = string.Format("queue-{0}.txt", worldName);

                List<SpriteLocation> spriteLocations = new List<SpriteLocation>();
                orderedStimuli_GameWorlds.Add(spriteLocations);

                StimulusManager_QueueGenerator.SetupQueue_OLD(
                    spriteLocations, numWantedPerWorld,
                    stimuliTexturesFaces, stimuliTexturesObjects,
                    percentileBlank_Game, percentileFaces_Game, reset_TypeConstraintEvery_Game, fileName_Queue, () =>
                    {
                        string fileName_Analysis = string.Format("analysis-{0}.txt", worldName);
                        StimulusManager_QueueAnalyzer.AnalyzeQueue(spriteLocations, fileName_Analysis);
                        CompleteQueue(callback);
                    }, queuesOnThread);

#if UNITY_EDITOR
                if (editorOnlyJustOneQueue)
                {
                    Debug.LogWarning("[Editor Only] Early out because one queue requested");
                    return;
                }
#endif
            }

            // --- LOCALIZERS
            int numWantedPerLocalizer = ExperimentManagerSession.wantedNumStimuli_PerLocalizer;
            float percentileBlank_Localizers = ExperimentLibraryManager.Config.Experiment.stimulus.blankVsNormalRatio_Localizer;
            float percentileFaces_Localizers = (1 - percentileBlank_Localizers) * ExperimentLibraryManager.Config.Experiment.stimulus.facesVsObjectsRatio_Localizer;
            float percentileObjects_Localizers = 1 - percentileFaces_Localizers - percentileBlank_Localizers;

            // Least amount of stim for a proper distribution of types (40-40-20 -> 2-2-1 -> 5)
            int reset_TypeConstraintEvery_Localizers = GetMinResetT(percentileFaces_Localizers,
                percentileObjects_Localizers, percentileBlank_Localizers, threshold, maxSearch);
            // 5; // Mathf.RoundToInt(numWantedPerWorld * reset_TypeConstraintEvery);

            for (int i = 0; i < numberOfLocalizers; i++)
            {
                string localizerName = "L{0}"._Format(i + 1);
                string fileName_Queue = string.Format("queue-{0}.txt", localizerName);

                List<SpriteLocation> spriteLocations = new List<SpriteLocation>();
                orderedStimuli_Localizers.Add(spriteLocations);

                StimulusManager_QueueGenerator.SetupQueue_OLD(
                    spriteLocations, numWantedPerLocalizer,
                    stimuliTexturesFaces, stimuliTexturesObjects,
                    percentileBlank_Localizers, percentileFaces_Localizers, reset_TypeConstraintEvery_Localizers,
                    fileName_Queue, () =>
                    {
                        string fileName_Analysis = string.Format("analysis-{0}.txt", localizerName);
                        StimulusManager_QueueAnalyzer.AnalyzeQueue(spriteLocations, fileName_Analysis);
                        CompleteQueue(callback);
                    }, queuesOnThread);
            }
        }

        private static void CompleteQueue(Action<bool> callback)
        {
            queuesSetup++;

            if (queuesSetup < queuesNeeded) return;

            // onQueuesSetup?.Invoke(null, null);
            callback?.Invoke(true);
        }

        private static int GetMinResetT(float percentFace, float percentObject, float percentBlank, float threshold, int maxSearch)
        {
            // Normalize
            float tot = percentFace + percentObject + percentBlank;
            percentFace /= tot;
            percentObject /= tot;
            percentBlank /= tot;

            // Raise up by minimum
            float min = Mathf.Min(percentFace, percentObject, percentBlank);
            percentFace /= min;
            percentObject /= min;
            percentBlank /= min;

            // Start multiplying until we got only little bit of error
            for (int i = 1; i < maxSearch; i++)
            {
                float _percentFace = percentFace * i;
                float _percentObject = percentObject * i;
                float _percentBlank = percentBlank * i;

                if (Mathf.Abs(percentFace - Mathf.RoundToInt(percentFace)) < threshold &&
                    Mathf.Abs(percentObject - Mathf.RoundToInt(percentObject)) < threshold &&
                    Mathf.Abs(percentBlank - Mathf.RoundToInt(percentBlank)) < threshold)
                    break;
            }

            int minResetT = Mathf.RoundToInt(percentFace) + Mathf.RoundToInt(percentObject) + Mathf.RoundToInt(percentBlank);

            return minResetT;
        }

    }

    public struct Option
    {
        public bool isProbed;
        public int stimID_1Based;
        public StimulusType type;
        public Direction_2D_Diagonal location;

        public Option(bool isProbed, int stimID_1Based, StimulusType type, Direction_2D_Diagonal location)
        {
            this.isProbed = isProbed;
            this.stimID_1Based = stimID_1Based;
            this.type = type;
            this.location = location;
        }

        public Option(string v) : this (-1, v)
        {
        }

        public Option(int stimID_1Based, string v)
        {
            this.stimID_1Based = stimID_1Based;

            // uLbO, uRtF
            isProbed = v[0] == 'p';

            if (v[1] == 'L' && v[2] == 'b')
                location = Direction_2D_Diagonal.BottomLeft;
            else if (v[1] == 'L' && v[2] == 't')
                location = Direction_2D_Diagonal.TopLeft;
            else if (v[1] == 'R' && v[2] == 'b')
                location = Direction_2D_Diagonal.BottomRight;
            else
                location = Direction_2D_Diagonal.TopRight;

            if (v.Length == 3)
                type = StimulusType.None;
            else
                type = v[3] == 'O' ? StimulusType.Object : StimulusType.Face;
        }

        public override bool Equals(object obj)
        {
            Option other = (Option)obj;
            return other.isProbed == isProbed && other.stimID_1Based == stimID_1Based && other.type == type && other.location == location;
        }

        public override string ToString()
        {
            // uLbO, uRtF
            return "[{0}] {1}{2}{3}{4}"._Format(
                stimID_1Based <= 0 ? "B" : stimID_1Based.ToString(),
                isProbed ? "p" : "u",
                location.ToLeftRight() == LeftRight.Left ? "L" : "R", 
                location.ToTopBottom() == TopBottom.Top ? "t" : "b",
                type == StimulusType.Face ? "F" : type == StimulusType.Object ? "O" : "");
        }

        public override int GetHashCode()
        {
            var hashCode = 557527014;
            hashCode = hashCode * -1521134295 + isProbed.GetHashCode();
            hashCode = hashCode * -1521134295 + stimID_1Based.GetHashCode();
            hashCode = hashCode * -1521134295 + type.GetHashCode();
            hashCode = hashCode * -1521134295 + location.GetHashCode();
            return hashCode;
        }
    }

    public class StimulusManager_QueueGenerator
    {
        static int debugSetupQueue_Count = -1;//100;
        static bool USE_OLD = false;
        public static bool MASS_GENERATION_MODE = false;

        /// <summary>
        /// Generates the queue for ONE World
        /// </summary>
        public static void SetupQueue_NEW(bool isFMRI, int subjectID, List<EventType> sequence, int worldIdx, IList<int> localizerStartIDs, Dictionary<int, int> levelStartPointsWithinWorld_0Based,
            int numLocalizerStimuli, List<SpriteLocation> queue_Final, List<Sprite> stimuliTexturesFaces, List<Sprite> stimuliTexturesObjects,
            float percentileBlank_Gameplay, float percentileBlank_Localizer,
            string fileName, Action<QueueScore> callback, bool runOnThread)
        {
#if UNITY_EDITOR
            // runOnThread = false;
#endif
            bool wantToDebug = true;
            bool doExtras = false;
            bool doPerItem = false;

            if (MASS_GENERATION_MODE == true)
                wantToDebug = false;

            bool doDebug = wantToDebug && !runOnThread;
            bool doDebug_Extras = doDebug && doExtras;
            bool doDebug_PerItem = doDebug && doPerItem;

            QueueScore score = QueueScore.DEFAULT;
#if !UNITY_EDITOR
        doDebug = false;
#endif
            float timeStarted = TimeWrapper.realtimeSinceStartup_NotTS;

            Func<int, int> GetLevelIdWithinWorldFromStimIndex = stimIndexWithinWorld_0Based =>
            {
                int levelIDWithinWorld_0Based = -1;
                foreach (KeyValuePair<int, int> levelStartPointWithinWorld_0Based in levelStartPointsWithinWorld_0Based)
                    if (stimIndexWithinWorld_0Based >= levelStartPointWithinWorld_0Based.Value)
                        levelIDWithinWorld_0Based = levelStartPointWithinWorld_0Based.Key; // we are at LEAST at this level
                    else
                        break;
                return levelIDWithinWorld_0Based;
            };

            // Debug.LogError("numWanted {0} - percentile_blank {1} - percentile_faces {2} - reset_T_Every {3}"._Format(numWanted, percentile_Blank, percentile_Faces, reset_T_Every));
            Dictionary<Sprite, KeyValuePair<string, StimulusType>> spriteInfo = new Dictionary<Sprite, KeyValuePair<string, StimulusType>>();
            Dictionary<Sprite, int> spriteIDs = new Dictionary<Sprite, int>();

            for (int i = 0; i < stimuliTexturesFaces.Count; i++)
            {
                Sprite s = stimuliTexturesFaces[i];
                spriteIDs.Add(s, i);
                spriteInfo.AddOrUpdate(s, new KeyValuePair<string, StimulusType>(s.name, StimulusType.Face));
            }

            for (int i = 0; i < stimuliTexturesObjects.Count; i++)
            {
                Sprite s = stimuliTexturesObjects[i];
                spriteIDs.Add(s, i);
                spriteInfo.AddOrUpdate(s, new KeyValuePair<string, StimulusType>(s.name, StimulusType.Object));
            }

            Action action = () =>
            {
                // ----- Start creating stuff
                queue_Final.Clear();
                string debug = "";

                // List<SpriteLocation> ALL_OPTIONS = GenerateAllOptions(spriteInfo, stimuliTexturesFaces, stimuliTexturesObjects);

                int numRuns = 2;
                List<List<EventType>> sequencePerRun = new List<List<EventType>>();
                sequencePerRun.Add(new List<EventType>());
                sequencePerRun.Add(new List<EventType>());

                int _runID = 0;

                for (int i = 0; i < sequence.Count; i++)
                {
                    for (int levelID = 0; levelID < levelStartPointsWithinWorld_0Based.Count; levelID++)
                        if (i == levelStartPointsWithinWorld_0Based[levelID])
                            _runID = levelID / 2;

                    sequencePerRun[_runID].Add(sequence[i]);
                }

                List<int> localizerLevels = new List<int>();
                foreach (int stimID in localizerStartIDs)
                    localizerLevels.Add(GetLevelIdWithinWorldFromStimIndex(stimID));

                int runID_Localizer = localizerLevels[0] == 0 ? 0 : 1;
                bool isEven = runID_Localizer == 0;

                // Assign based on SUBJECT and WORLD id
                int choiceID = 
                    ((subjectID % 2) * 2 // EVEN subjects have W0 as W0, ODD subjects have W2 as W0
                    + worldIdx) % 4;
                // Debug.Log("Subject ID {0}, WorldIdx {1} -> ChoiceID {2}"._Format(subjectID, worldIdx, choiceID));

                List<Option> optionsLocalizerL1 = isFMRI ?

                    new List<Option>(
                        choiceID == 0 ? StimulusManager_QueueGenerator_Helper_FMRI.optionsLocalizerW1L1 :
                        choiceID == 1 ? StimulusManager_QueueGenerator_Helper_FMRI.optionsLocalizerW2L1 :
                        choiceID == 2 ? StimulusManager_QueueGenerator_Helper_FMRI.optionsLocalizerW3L1 :
                                        StimulusManager_QueueGenerator_Helper_FMRI.optionsLocalizerW4L1) :

                    new List<Option>(
                        choiceID == 0 ? StimulusManager_QueueGenerator_Helper_MEEG_ECOG.optionsLocalizerW1L1 :
                        choiceID == 1 ? StimulusManager_QueueGenerator_Helper_MEEG_ECOG.optionsLocalizerW2L1 :
                        choiceID == 2 ? StimulusManager_QueueGenerator_Helper_MEEG_ECOG.optionsLocalizerW3L1 :
                                        StimulusManager_QueueGenerator_Helper_MEEG_ECOG.optionsLocalizerW4L1);

                List<Option> optionsLocalizerL2 = isFMRI ?

                    new List<Option>(
                        choiceID == 0 ? StimulusManager_QueueGenerator_Helper_FMRI.optionsLocalizerW1L2 :
                        choiceID == 1 ? StimulusManager_QueueGenerator_Helper_FMRI.optionsLocalizerW2L2 :
                        choiceID == 2 ? StimulusManager_QueueGenerator_Helper_FMRI.optionsLocalizerW3L2 :
                                        StimulusManager_QueueGenerator_Helper_FMRI.optionsLocalizerW4L2) :

                    new List<Option>(
                        choiceID == 0 ? StimulusManager_QueueGenerator_Helper_MEEG_ECOG.optionsLocalizerW1L2 :
                        choiceID == 1 ? StimulusManager_QueueGenerator_Helper_MEEG_ECOG.optionsLocalizerW2L2 :
                        choiceID == 2 ? StimulusManager_QueueGenerator_Helper_MEEG_ECOG.optionsLocalizerW3L2 :
                                        StimulusManager_QueueGenerator_Helper_MEEG_ECOG.optionsLocalizerW4L2);

                List<Option> optionsGameplay_ReplayRun_Probed = new List<Option>();
                List<Option> optionsGameplay_ReplayRun_Unprobed = new List<Option>();
                List<Option> optionsGameplay_NonReplayRun_Probed = new List<Option>();
                List<Option> optionsGameplay_NonReplayRun_Unprobed = new List<Option>();

                int numProbesInFirst25 = Precalculator.GetNumProbesInFirst25(isFMRI);
                int numUnprobedInFirst25 = 25 - numProbesInFirst25;

                {
                    List<Option> MEEG_ECOG_optionsGameplay_Probed = new List<Option>(
                        choiceID == 0 ? StimulusManager_QueueGenerator_Helper_MEEG_ECOG.optionsGameplayW1_Probed :
                        choiceID == 1 ? StimulusManager_QueueGenerator_Helper_MEEG_ECOG.optionsGameplayW2_Probed :
                        choiceID == 2 ? StimulusManager_QueueGenerator_Helper_MEEG_ECOG.optionsGameplayW3_Probed :
                                        StimulusManager_QueueGenerator_Helper_MEEG_ECOG.optionsGameplayW4_Probed);

                    List<Option> MEEG_ECOG_optionsGameplay_Unprobed = new List<Option>(
                        choiceID == 0 ? StimulusManager_QueueGenerator_Helper_MEEG_ECOG.optionsGameplayW1_Unprobed :
                        choiceID == 1 ? StimulusManager_QueueGenerator_Helper_MEEG_ECOG.optionsGameplayW2_Unprobed :
                        choiceID == 2 ? StimulusManager_QueueGenerator_Helper_MEEG_ECOG.optionsGameplayW3_Unprobed :
                                        StimulusManager_QueueGenerator_Helper_MEEG_ECOG.optionsGameplayW4_Unprobed);

                    List<Option> FMRI_optionsGameplay_ReplayRun_Probed = new List<Option>(
                        choiceID == 0 ? StimulusManager_QueueGenerator_Helper_FMRI.optionsGameplayW1_ReplayRun_Probed :
                        choiceID == 1 ? StimulusManager_QueueGenerator_Helper_FMRI.optionsGameplayW2_ReplayRun_Probed :
                        choiceID == 2 ? StimulusManager_QueueGenerator_Helper_FMRI.optionsGameplayW3_ReplayRun_Probed :
                                        StimulusManager_QueueGenerator_Helper_FMRI.optionsGameplayW4_ReplayRun_Probed);

                    List<Option> FMRI_optionsGameplay_ReplayRun_Unprobed = new List<Option>(
                        choiceID == 0 ? StimulusManager_QueueGenerator_Helper_FMRI.optionsGameplayW1_ReplayRun_Unprobed :
                        choiceID == 1 ? StimulusManager_QueueGenerator_Helper_FMRI.optionsGameplayW2_ReplayRun_Unprobed :
                        choiceID == 2 ? StimulusManager_QueueGenerator_Helper_FMRI.optionsGameplayW3_ReplayRun_Unprobed :
                                        StimulusManager_QueueGenerator_Helper_FMRI.optionsGameplayW4_ReplayRun_Unprobed);

                    List<Option> FMRI_optionsGameplay_NonReplayRun_Probed = new List<Option>(
                        choiceID == 0 ? StimulusManager_QueueGenerator_Helper_FMRI.optionsGameplayW1_NonReplayRun_Probed :
                        choiceID == 1 ? StimulusManager_QueueGenerator_Helper_FMRI.optionsGameplayW2_NonReplayRun_Probed :
                        choiceID == 2 ? StimulusManager_QueueGenerator_Helper_FMRI.optionsGameplayW3_NonReplayRun_Probed :
                                        StimulusManager_QueueGenerator_Helper_FMRI.optionsGameplayW4_NonReplayRun_Probed);

                    List<Option> FMRI_optionsGameplay_NonReplayRun_Unprobed = new List<Option>(
                        choiceID == 0 ? StimulusManager_QueueGenerator_Helper_FMRI.optionsGameplayW1_NonReplayRun_Unprobed :
                        choiceID == 1 ? StimulusManager_QueueGenerator_Helper_FMRI.optionsGameplayW2_NonReplayRun_Unprobed :
                        choiceID == 2 ? StimulusManager_QueueGenerator_Helper_FMRI.optionsGameplayW3_NonReplayRun_Unprobed :
                                        StimulusManager_QueueGenerator_Helper_FMRI.optionsGameplayW4_NonReplayRun_Unprobed);


                    // === GAMEPLAY (REPLAY RUN, PROBED)
                    optionsGameplay_ReplayRun_Probed.AddRange(isFMRI ?
                        FMRI_optionsGameplay_ReplayRun_Probed :
                        MEEG_ECOG_optionsGameplay_Probed);

                    optionsGameplay_ReplayRun_Probed.RemoveRange(optionsLocalizerL1);
                    optionsGameplay_ReplayRun_Probed.RemoveRange(optionsLocalizerL2);

                    int probesInReplayRun_CoveredByLocalizers =
                        optionsLocalizerL1.FindAll(a => a.isProbed).Count +
                        optionsLocalizerL2.FindAll(a => a.isProbed).Count;

                    optionsGameplay_ReplayRun_Probed.Shuffle();
                    while (optionsGameplay_ReplayRun_Probed.Count + probesInReplayRun_CoveredByLocalizers > 25)
                        optionsGameplay_ReplayRun_Probed.RemoveAt(0);

                    if (doDebug)
                    {
                        Debug.Log("GAMEPLAY (REPLAY RUN, PROBED) Num In Replay :: " + probesInReplayRun_CoveredByLocalizers);
                        Debug.Log("GAMEPLAY (REPLAY RUN, PROBED) Num Outside Replay :: " + optionsGameplay_ReplayRun_Probed.Count);
                    }


                    // === GAMEPLAY (REPLAY RUN, UNPROBED)
                    optionsGameplay_ReplayRun_Unprobed.AddRange(isFMRI ?
                        FMRI_optionsGameplay_ReplayRun_Unprobed :
                        MEEG_ECOG_optionsGameplay_Unprobed);

                    optionsGameplay_ReplayRun_Unprobed.RemoveRange(optionsLocalizerL1);
                    optionsGameplay_ReplayRun_Unprobed.RemoveRange(optionsLocalizerL2);

                    int unprobedInReplayRun_CoveredByLocalizers =
                        optionsLocalizerL1.FindAll(a => !a.isProbed).Count +
                        optionsLocalizerL2.FindAll(a => !a.isProbed).Count;

                    optionsGameplay_ReplayRun_Unprobed.Shuffle();
                    while (optionsGameplay_ReplayRun_Unprobed.Count + unprobedInReplayRun_CoveredByLocalizers > 50)
                        optionsGameplay_ReplayRun_Unprobed.RemoveAt(0);
                    if (doDebug)
                    {
                        Debug.Log("GAMEPLAY (REPLAY RUN, UNPROBED) Num In Replay :: " + unprobedInReplayRun_CoveredByLocalizers);
                        Debug.Log("GAMEPLAY (REPLAY RUN, UNPROBED) Num Outside Replay :: " + optionsGameplay_ReplayRun_Unprobed.Count);
                    }

                    // === GAMEPLAY (NON-REPLAY RUN, PROBED)
                    optionsGameplay_NonReplayRun_Probed.AddRange(isFMRI ?
                        FMRI_optionsGameplay_NonReplayRun_Probed :
                        MEEG_ECOG_optionsGameplay_Probed);

                    if (!isFMRI) // FMRI has those already split!
                    {
                        /// Starting with <see cref="MEEG_ECOG_optionsGameplay_Probed"/>
                        // We need to remove (for ECOG / MEEG)
                        // Everything that has been added to the REPLAY run, which is
                        // {Gameplay of replay run + replays of replay run}
                        optionsGameplay_NonReplayRun_Probed.RemoveRange(optionsGameplay_ReplayRun_Probed);
                        optionsGameplay_NonReplayRun_Probed.RemoveRange(optionsLocalizerL1);
                        optionsGameplay_NonReplayRun_Probed.RemoveRange(optionsLocalizerL2);
                    }

                    if (doDebug)
                        Debug.Log("GAMEPLAY (NON-REPLAY RUN, PROBED) Num Total:: " + optionsGameplay_NonReplayRun_Probed.Count);


                    // === GAMEPLAY (NON-REPLAY RUN, UNPROBED)
                    optionsGameplay_NonReplayRun_Unprobed.AddRange(isFMRI ?
                        FMRI_optionsGameplay_NonReplayRun_Unprobed :
                        MEEG_ECOG_optionsGameplay_Unprobed);

                    if (!isFMRI) // FMRI has those already split!
                    {
                        /// Starting with <see cref="MEEG_ECOG_optionsGameplay_Probed"/>
                        // We need to remove (for ECOG / MEEG)
                        // Everything that has been added to the REPLAY run, which is
                        // {Gameplay of replay run + replays of replay run}
                        optionsGameplay_NonReplayRun_Unprobed.RemoveRange(optionsGameplay_ReplayRun_Unprobed);
                        optionsGameplay_NonReplayRun_Unprobed.RemoveRange(optionsLocalizerL1);
                        optionsGameplay_NonReplayRun_Unprobed.RemoveRange(optionsLocalizerL2);
                    }

                    if (doDebug)
                        Debug.Log("GAMEPLAY (NON-REPLAY RUN, UNPROBED) Num Total:: " + optionsGameplay_NonReplayRun_Unprobed.Count);
                }

                /*
                Debug.Log("Num Faces (L1 [P]) :: " + optionsLocalizerL1.FindAll(a => a.type == StimulusType.Face && a.isProbed).Count);
                Debug.Log("Num Faces (L1 [U]) :: " + optionsLocalizerL1.FindAll(a => a.type == StimulusType.Face && !a.isProbed).Count);
                Debug.Log("Num Faces (L2 [P]) :: " + optionsLocalizerL2.FindAll(a => a.type == StimulusType.Face && a.isProbed).Count);
                Debug.Log("Num Faces (L2 [U]) :: " + optionsLocalizerL2.FindAll(a => a.type == StimulusType.Face && !a.isProbed).Count);

                Debug.Log("Num Faces (GP_RR_ALL) :: " + optionsGameplay_ReplayRun_Probed.FindAll(a => a.type == StimulusType.Face).Count);
                Debug.Log("Num Faces (GU_RR_ALL) :: " + optionsGameplay_Unprobed.FindAll(a => a.type == StimulusType.Face).Count);
                Debug.Log("Num Faces (GP_RR_NL) :: " + optionsGameplay_Probed_NonLocalizer.FindAll(a => a.type == StimulusType.Face).Count);
                Debug.Log("Num Faces (GU_RR_NL) :: " + optionsGameplay_Unprobed_NonLocalizer.FindAll(a => a.type == StimulusType.Face).Count);
                Debug.Log("Num Faces (GU_NRR_ALL) :: " + optionsGameplay_Unprobed.FindAll(a => a.type == StimulusType.Face).Count);
                Debug.Log("Num Faces (GU_NRR_ALL) :: " + optionsGameplay_Unprobed.FindAll(a => a.type == StimulusType.Face).Count);
                */

                // <-- LOCALIZERS FIRST -->
                Dictionary<int, SpriteLocation> choicesCoveredByLocalizers = new Dictionary<int, SpriteLocation>();
                List<SpriteLocation> stimulusQueue_Localizer_TOTAL_UNPROBED = new List<SpriteLocation>();
                List<SpriteLocation> stimulusQueue_Localizer_TOTAL_PROBED = new List<SpriteLocation>();
                List<SpriteLocation> stimulusQueue_Localizer_TOTAL_BOTH = new List<SpriteLocation>();

                // For each of the localizers
                Dictionary<int, int> faceIDsToChoices_FirstHalf = new Dictionary<int, int>();
                
                // if (doDebug) DoDebug();
                int numLocalizers = localizerStartIDs.Count;
                for (int localizerID_WithinWorld = 0; localizerID_WithinWorld < numLocalizers; localizerID_WithinWorld++)
                {

                    Dictionary<int, EventType> localizerSequence = new Dictionary<int, EventType>();

                    int localizerStartID = localizerStartIDs[localizerID_WithinWorld];

                    for (int localizerSequenceItem_IDwithinWorld = localizerStartID; localizerSequenceItem_IDwithinWorld < localizerStartID + numLocalizerStimuli; localizerSequenceItem_IDwithinWorld++)
                        localizerSequence.Add(localizerSequenceItem_IDwithinWorld, sequence[localizerSequenceItem_IDwithinWorld]);

                    NumWanted? numWantedLocalizer = GetNumWantedFromSequence(localizerSequence.Values.ToList());

                    if (numWantedLocalizer == null) return;
                    if (numWantedLocalizer.Value.probed != numProbesInFirst25) { Debug.LogError("Should have been {0} - was {1}"._Format(numProbesInFirst25, numWantedLocalizer.Value.probed)); return; }
                    if (numWantedLocalizer.Value.unprobed != numUnprobedInFirst25) { Debug.LogError("Should have been {0} - was {1}"._Format(numUnprobedInFirst25, numWantedLocalizer.Value.unprobed)); return; }
                    
                    if (doDebug)
                        Debug.Log(localizerSequence.Values.ToList().ToReadableString("localizerSequence for localizerID_WithinWorld - " + localizerID_WithinWorld));

                    // ==== PREPARE THE SPRITE-LOCATIONS (FROM THE OPTIONS)
                    List <SpriteLocation> stimulusQueue_Localizer_UNPROBED = new List<SpriteLocation>();
                    List<SpriteLocation> stimulusQueue_Localizer_PROBED = new List<SpriteLocation>();

                    List<Option> optionsLocalizer = localizerID_WithinWorld == 0 ? optionsLocalizerL1 : optionsLocalizerL2;
                    optionsLocalizer.Shuffle();

                    foreach (Option o in optionsLocalizer)
                    {
                        Sprite s = o.type == StimulusType.None ? null :
                                    o.type == StimulusType.Face ? stimuliTexturesFaces[o.stimID_1Based - 1] :
                                                                  stimuliTexturesObjects[o.stimID_1Based - 1];

                        SpriteLocation sL = new SpriteLocation(s, o.location, s?.name, o.type);

                        sL.DEBUG_IS_PROBED = o.isProbed;
                        sL.DEBUG_REPLAY_ID_WITHIN_WORLD = localizerID_WithinWorld;

                        if (o.isProbed)
                            stimulusQueue_Localizer_PROBED.Add(sL);
                        else
                            stimulusQueue_Localizer_UNPROBED.Add(sL);
                    }

                    List<SpriteLocation> stimulusQueue_Localizer_BOTH = new List<SpriteLocation>();

                    // ==== ASSIGN THEM TO THE SEQUENCE
                    foreach (KeyValuePair<int, EventType> kVP in localizerSequence)
                    {
                        EventType sequenceItem = kVP.Value;
                        int localizerSequenceItem_IDwithinWorld = kVP.Key;

                        // Debug.Log("Covering item ID " + localizerSequenceItem_IDwithinWorld + " " + sequenceItem + " for localizer");

                        // Pick the right stim queue (GLOBAL QUEUES)
                        List<SpriteLocation> stimulusQueueToSet =
                            sequenceItem == EventType.Stimulus ?
                            stimulusQueue_Localizer_TOTAL_UNPROBED : stimulusQueue_Localizer_TOTAL_PROBED;

                        // Pick the right stim queue (GLOBAL QUEUES)
                        List<SpriteLocation> stimulusQueueToGet =
                            sequenceItem == EventType.Stimulus ?
                            stimulusQueue_Localizer_UNPROBED : stimulusQueue_Localizer_PROBED;

                        SpriteLocation choice;

                        if (stimulusQueueToGet.Count > 0)
                        {
                            choice = stimulusQueueToGet[0]; // they are already shuffled
                        }
                        else
                        {
                            Debug.LogError(worldIdx + " : " + localizerID_WithinWorld + "WTF queue emptied out : " + sequenceItem);
                            choice = GetStimRandom(spriteInfo, stimuliTexturesFaces, stimuliTexturesObjects);
                        }

                        stimulusQueueToGet.Remove(choice);
                        stimulusQueueToSet.Add(choice);
                        stimulusQueue_Localizer_BOTH.Add(choice);
                        stimulusQueue_Localizer_TOTAL_BOTH.Add(choice);
                        choicesCoveredByLocalizers.Add(localizerSequenceItem_IDwithinWorld, choice);
                    }

                    if (doDebug)
                    {
                        Debug.LogWarning("==== LOC SINGLE BEGIN ====");
                        // At this point both stim queues should be empty
                        Debug.LogWarning(worldIdx + " : " + localizerID_WithinWorld);
                        score.replaySingle += DoAnalysis25(stimulusQueue_Localizer_BOTH, worldIdx);
                        Debug.LogWarning("==== LOC SINGLE END ====");
                    }
                }

                if (doDebug)
                {
                    score.replaySingle /= numLocalizers;

                    Debug.LogWarning("==== LOC PAIR BEGIN ====");
                    Debug.LogWarning(worldIdx);
                    score.runReplay = DoAnalysis50(stimulusQueue_Localizer_TOTAL_BOTH, worldIdx); // choicesCoveredByLocalizers.Values.ToList()
                    Debug.LogWarning("==== LOC PAIR END ====");
                }

                NumWanted numTotalLocalizer = new NumWanted(
                    stimulusQueue_Localizer_TOTAL_BOTH.FindAll(a => !a.DEBUG_IS_PROBED).Count,
                    stimulusQueue_Localizer_TOTAL_BOTH.FindAll(a => a.DEBUG_IS_PROBED).Count);

                // Generate the rest
                for (int runID = 0; runID < numRuns; runID++)
                {
                    bool isReplayRun = runID == runID_Localizer;
                    if (doDebug)
                        Debug.Log("===== RUN ID " + runID + (isReplayRun ? " REPLAY" : " NORMAL"));

                    // [DEPRECATED] Even subjects are working so ODD subjects will be draw from the same generator (one version) but with probed / unprobed runs 0 & 1 swapped (runIndex_ODD_withinWorld = 1 - runIndex_EVEN_withinWOrld)
                    int runToLoad_IdWithinWorld = runID;// isEven ? runID : (1 - runID);

                    List<EventType> sequenceRun = sequencePerRun[runID]; // Timings must be respected! (even if we run out by a little)
                    NumWanted? numWantedRun = GetNumWantedFromSequence(sequenceRun);
                    if (numWantedRun == null) return;

                    // ==== PREPARE THE SPRITE-LOCATIONS (FROM THE OPTIONS)
                    List<SpriteLocation> stimulusQueue_Run_UNPROBED = new List<SpriteLocation>();
                    List<SpriteLocation> stimulusQueue_Run_PROBED = new List<SpriteLocation>();

                    List<Option> optionsGameplay_Run_ExclLocalizer = new List<Option>();

                    optionsGameplay_Run_ExclLocalizer.AddRange(isReplayRun ?
                        optionsGameplay_ReplayRun_Probed : 
                        optionsGameplay_NonReplayRun_Probed);

                    optionsGameplay_Run_ExclLocalizer.AddRange(isReplayRun ?
                        optionsGameplay_ReplayRun_Unprobed :
                        optionsGameplay_NonReplayRun_Unprobed);

                    foreach (Option o in optionsGameplay_Run_ExclLocalizer)
                    {
                        Sprite s = o.type == StimulusType.None ? null :
                                    o.type == StimulusType.Face ? stimuliTexturesFaces[o.stimID_1Based - 1] :
                                                                  stimuliTexturesObjects[o.stimID_1Based - 1];

                        SpriteLocation sL = new SpriteLocation(s, o.location, s?.name, o.type);

                        sL.DEBUG_IS_PROBED = o.isProbed;

                        if (o.isProbed)
                            stimulusQueue_Run_PROBED.Add(sL);
                        else
                            stimulusQueue_Run_UNPROBED.Add(sL);
                    }

                    if (doDebug)
                        Debug.Log("==== STIM RUN OPTIONS PROBED :: {0}\n{1}"._Format(stimulusQueue_Run_PROBED.Count, stimulusQueue_Run_PROBED.ToReadableString()));

                    for (int indexWithinRun = 0; indexWithinRun < sequenceRun.Count; indexWithinRun++)
                    {
                        int indexWithinWorldQueue = queue_Final.Count;

                        EventType sequenceItem = sequenceRun[indexWithinRun];

                        if (doDebug_PerItem) Debug.Log("Queue Item :: {0}"._Format(queue_Final.Count));

                        // Pick the next one from the queue
                        bool isProbed = sequenceItem != EventType.Stimulus;
                        List<SpriteLocation> stimulusQueue_Run = isProbed ?
                            stimulusQueue_Run_PROBED : stimulusQueue_Run_UNPROBED;
                        
                        SpriteLocation choice;

                        if (choicesCoveredByLocalizers.ContainsKey(indexWithinWorldQueue))
                        {
                            if (doDebug_PerItem) Debug.LogWarning("Skipping item :: " + indexWithinWorldQueue + " (already set by localizer)");
                            choice = choicesCoveredByLocalizers[indexWithinWorldQueue];
                        }
                        else if (stimulusQueue_Run.Count > 0)
                        {
                            // choice = GetStimToRemoveBalanced(spriteInfo, stimuliTexturesFaces, stimuliTexturesObjects, stimulusQueue_Run, queue_Final.FindAll(x => x.DEBUG_IS_PROBED == isProbed));//.GetRandom();
                            choice = GetStimBalanced(spriteInfo, stimuliTexturesFaces, stimuliTexturesObjects, queue_Final.FindAll(x => x.DEBUG_IS_PROBED == isProbed), stimulusQueue_Run);
                            choice.DEBUG_IS_PROBED = isProbed;

                            if (!stimulusQueue_Run.Contains(choice))
                            {
                                Debug.LogError("WRONG STIM TO REMOVE - picked random");
                                choice = stimulusQueue_Run.GetRandom();
                            }

                            stimulusQueue_Run.Remove(choice);
                        }
                        else
                        {
                            if (sequenceItem == EventType.Probe)
                                Debug.LogError("Stim queue empty for :: " + sequenceItem + " | Should not happen!!");

                            // choice = GetStimRandom(spriteInfo, stimuliTexturesFaces, stimuliTexturesObjects);
                            // Debug.LogError("Created random for idx within run {0}\n{1}"._Format(indexWithinRun, choice.ToString()));

                            List<SpriteLocation> queueToBalanceAgainst = queue_Final.FindAll(x => x.DEBUG_IS_PROBED == isProbed);
                            // choice = GetStimBalanced(ALL_OPTIONS, queueToBalanceAgainst);
                            choice = GetStimBalanced(spriteInfo, stimuliTexturesFaces, stimuliTexturesObjects, queueToBalanceAgainst);
                            // choice = GetStimBalanced_LocType(spriteInfo, stimuliTexturesFaces, stimuliTexturesObjects, queueToBalanceAgainst);
                            // choice = GetStimBalanced_TypeLoc(spriteInfo, stimuliTexturesFaces, stimuliTexturesObjects, queueToBalanceAgainst);

                            // Debug.Log(choice.DEBUG_IS_PROBED);
                            choice.DEBUG_IS_PROBED = isProbed;

                            if (doDebug)
                                Debug.LogError("Created BALANCED stim for idx within run {0} :: {1}\n{2}"._Format(indexWithinRun, choice.ToString(), queueToBalanceAgainst.ToReadableString()));
                        }

                        choice.DEBUG_WORLD_ID = worldIdx;
                        choice.DEBUG_LEVEL_ID_WITHIN_WORLD = GetLevelIdWithinWorldFromStimIndex(indexWithinWorldQueue);

                        queue_Final.Add(choice);

                        // Update our logs!
                        if (doDebug_PerItem) Debug.Log("Picked & Logged {0}, {1}, {2}, {3}, {4}"._Format(choice.direction, choice.type, choice.sprite, choice.ToString(), choice.typeLocation));
                    }

                    if (doDebug && isFMRI)
                    {
                        Debug.LogWarning("==== RUN PROBED BEGIN ====");
                        Debug.LogWarning(worldIdx + " : " + runID + " (loaded run :: " + runToLoad_IdWithinWorld + ")");
                        score.runProbed += DoAnalysis25(queue_Final.FindAll(a => a.DEBUG_IS_PROBED && Mathf.FloorToInt(a.DEBUG_LEVEL_ID_WITHIN_WORLD / 2) == runID), worldIdx);
                        Debug.LogWarning("==== RUN PROBED END ====");

                        Debug.LogWarning("==== RUN UNPROBED BEGIN ====");
                        Debug.LogWarning(worldIdx + " : " + runID + " (loaded run :: " + runToLoad_IdWithinWorld + ")");
                        score.runUnprobed += DoAnalysis50(queue_Final.FindAll(a => !a.DEBUG_IS_PROBED && Mathf.FloorToInt(a.DEBUG_LEVEL_ID_WITHIN_WORLD / 2) == runID), worldIdx, true);
                        Debug.LogWarning("==== RUN UNPROBED END ====");

                        Debug.LogWarning("==== RUN TOTAL BEGIN ====");
                        Debug.LogWarning(worldIdx + " : " + runID + " (loaded run :: " + runToLoad_IdWithinWorld + ")");
                        score.runTotal += DoAnalysis75(queue_Final.FindAll(a => Mathf.FloorToInt(a.DEBUG_LEVEL_ID_WITHIN_WORLD / 2) == runID), worldIdx, true);
                        Debug.LogWarning("==== RUN TOTAL END ====");
                    }
                }

                if (doDebug)
                {
                    score.runProbed /= numRuns;
                    score.runUnprobed /= numRuns;
                    score.runTotal /= numRuns;

                    Debug.LogWarning("==== WORLD PROBED BEGIN ====");
                    Debug.LogWarning(worldIdx);
                    score.worldProbed = DoAnalysis50(queue_Final.FindAll(a => a.DEBUG_IS_PROBED), worldIdx);
                    Debug.LogWarning("==== WORLD PROBED END ====");

                    Debug.LogWarning("==== WORLD UNPROBED BEGIN ====");
                    Debug.LogWarning(worldIdx);
                    score.worldUnprobed = DoAnalysis100(queue_Final.FindAll(a => !a.DEBUG_IS_PROBED), worldIdx, true);
                    Debug.LogWarning("==== WORLD UNPROBED END ====");

                    Debug.LogWarning("==== WORLD TOTAL BEGIN ====");
                    Debug.LogWarning(worldIdx);
                    score.worldTotal = DoAnalysis150(queue_Final, worldIdx, true);
                    Debug.LogWarning("==== WORLD TOTAL END ====");

                    string finalReport =
                        "Stimulus Queue {0}"._Format(queue_Final.ToReadableString("Queue Contents", runOnThread));

                    ExperimentManagerSession.LogStimulusQueue(fileName, finalReport);
                }
            };

            if (!runOnThread)
            {
                action();
                callback(score);
                return;
            }

            AsyncThread.RequestRunOnNewThread(() =>
            {
                try
                {
                    action();

                    AsyncThread.RunOnMainThread_ASAP_TS(() =>
                    {
                        Debug.Log("Queue Built in {0}s - check {1}"._Format((TimeWrapper.realtimeSinceStartup_NotTS - timeStarted).ToString("0.0"), fileName));
                        callback(score);
#if UNITY_EDITOR
                        if (fileName.Contains("game"))
                        {
                            debugSetupQueue_Count--;
                            if (debugSetupQueue_Count > 0)
                                SetupQueue_NEW(isFMRI, subjectID, sequence, worldIdx, localizerStartIDs, levelStartPointsWithinWorld_0Based,
                                    numLocalizerStimuli, queue_Final, stimuliTexturesFaces, stimuliTexturesObjects,
                                    percentileBlank_Gameplay, percentileBlank_Localizer, fileName, callback, runOnThread);
                        }
#endif
                    });
                }
                catch (Exception ex)
                {

                    AsyncThread.RunOnMainThread_ASAP_TS(() =>
                    {
                        Debug.LogException(ex);
                        callback(score);
                    });
                }
            });
        }

        private static void GenerateWorld(
            List<Option> optionsLocalizerL1, List<Option> optionsLocalizerL2, 
            List<Option> optionsGameplay_Probed, List<Option> optionsGameplay_Unprobed, 
            List<Option> optionsGameplayV1W1_Probed_NonLocalizer, List<Option> optionsGameplayV1W1_Unprobed_NonLocalizer)
        {
            optionsGameplayV1W1_Probed_NonLocalizer.Clear();
            optionsGameplayV1W1_Probed_NonLocalizer.AddRange(optionsGameplay_Probed);
            optionsGameplayV1W1_Probed_NonLocalizer.RemoveRange(optionsLocalizerL1);
            optionsGameplayV1W1_Probed_NonLocalizer.RemoveRange(optionsLocalizerL2);

            optionsGameplayV1W1_Unprobed_NonLocalizer.Clear();
            optionsGameplayV1W1_Unprobed_NonLocalizer.AddRange(optionsGameplay_Unprobed);
            optionsGameplayV1W1_Unprobed_NonLocalizer.RemoveRange(optionsLocalizerL1);
            optionsGameplayV1W1_Unprobed_NonLocalizer.RemoveRange(optionsLocalizerL2);

            // CHECKS
            Debug.Log("== CHECKS BEGIN ==");
            if (optionsLocalizerL1.Count != 25) Debug.LogError("optionsLocalizerV1W1L1 Should have had 25 stimuli!");
            if (optionsLocalizerL2.Count != 25) Debug.LogError("optionsLocalizerV1W1L2 Should have had 25 stimuli!");
            if (optionsGameplay_Probed.Count != 50) Debug.LogError("optionsGameplayV1W1_Probed Should have had 50 stimuli!");
            if (optionsGameplay_Unprobed.Count != 100) Debug.LogError("optionsGameplayV1W1_Unprobed Should have had 100 stimuli!");
            if (optionsGameplayV1W1_Probed_NonLocalizer.Count != 34) Debug.LogError("optionsGameplayV1W1_Probed_NonLocalizer Should have had 100 stimuli!"); // 50 - 8 - 8
            if (optionsGameplayV1W1_Unprobed_NonLocalizer.Count != 66) Debug.LogError("optionsGameplayV1W1_Unprobed_NonLocalizer Should have had 100 stimuli!"); // 100 - 17 - 17
            Debug.Log("== CHECKS END ==");

            /*
            Debug.Log(optionsLocalizerV1W1L1.ToReadableString("optionsLocalizerV1W1L1"));
            Debug.Log(optionsLocalizerV1W1L2.ToReadableString("optionsLocalizerV1W1L2"));
            Debug.Log(optionsGameplayV1W1_Probed.ToReadableString("optionsGameplayV1W1_Probed"));
            Debug.Log(optionsGameplayV1W1_Probed_NonLocalizer.ToReadableString("optionsGameplayV1W1_Probed_NonLocalizer"));
            */
        }

        private static List<SpriteLocation> GenerateAllOptions(Dictionary<Sprite, KeyValuePair<string, StimulusType>> spriteInfo, List<Sprite> stimuliTexturesFaces, List<Sprite> stimuliTexturesObjects)
        {
            List<SpriteLocation> ALL_OPTIONS = new List<SpriteLocation>();

            foreach (Direction_2D_Diagonal d2D in Utility_Helper.EnumGetValues<Direction_2D_Diagonal>())
            {
                // Blank
                ALL_OPTIONS.Add(GetStimFrom_Blank(d2D));

                for (int i = 0; i < 10; i++)
                {
                    // Faces
                    ALL_OPTIONS.Add(GetStimFrom(stimuliTexturesFaces, spriteInfo, i, d2D));

                    // Objects
                    ALL_OPTIONS.Add(GetStimFrom(stimuliTexturesObjects, spriteInfo, i, d2D));
                }
            }

            return ALL_OPTIONS;
        }

        private static SpriteLocation GetStimBalanced_LocType(Dictionary<Sprite, KeyValuePair<string, StimulusType>> spriteInfo, List<Sprite> stimuliTexturesFaces, List<Sprite> stimuliTexturesObjects, List<SpriteLocation> balanceAgainst)
        {
            QueueAnalysis analysis = Analyze(balanceAgainst);
            // Debug.LogError(analysis);

            // Pick the location
            Direction_2D_Diagonal dir = default;

            {
                int numLocationMin = int.MaxValue;

                foreach (Direction_2D_Diagonal d2D in Utility_Helper.EnumGetValues<Direction_2D_Diagonal>().Shuffle())
                {
                    int numLocation = analysis.locCount[d2D];

                    if (numLocation >= numLocationMin) continue;

                    dir = d2D;
                    numLocationMin = numLocation;
                }
            }

            // Pick the type
            StimulusType sT = default;

            {
                float numTypeMin = float.MaxValue;

                foreach (StimulusType _sT in Utility_Helper.EnumGetValues<StimulusType>().Shuffle())
                {
                    TypeLocation tL = new TypeLocation(_sT, dir);
                    float numType = analysis.typeLocCount[tL];

                    if (_sT != StimulusType.None)
                        numType /= 2f; // 2 : 2 : 1 ratio intended

                    if (numType >= numTypeMin) continue;

                    numTypeMin = numType;
                    sT = _sT;
                }
            }

            // If we're blank - we're done! 
            if (sT == StimulusType.None)
                return GetStimFrom_Blank(dir);

            // Pick the ID
            int ID = -1;

            {
                int numIDMin = int.MaxValue;
                List<int> IDs = new List<int>();
                for (int i = 0; i < 10; i++)
                    IDs.Add(i);

                foreach (int i in IDs.Shuffle())
                {
                    string spriteName = spriteInfo[(sT == StimulusType.Face ? stimuliTexturesFaces : stimuliTexturesObjects)[i]].Key;
                    int numID = analysis.spriteLocCount[spriteName][dir];

                    if (numID >= numIDMin) continue;

                    ID = i;
                    numIDMin = numID;
                }
            }

            if (sT == StimulusType.Face)
                return GetStimFrom(stimuliTexturesFaces, spriteInfo, ID, dir);
            else
                return GetStimFrom(stimuliTexturesObjects, spriteInfo, ID, dir);
        }

        private static SpriteLocation GetStimToRemoveBalanced(Dictionary<Sprite, KeyValuePair<string, StimulusType>> spriteInfo, List<Sprite> stimuliTexturesFaces, List<Sprite> stimuliTexturesObjects, List<SpriteLocation> removeFrom, List<SpriteLocation> balanceAgainst)
        {
            QueueAnalysis analysis_BalanceAgainst = Analyze(balanceAgainst);
            QueueAnalysis analysis_RemoveFrom = Analyze(removeFrom);
            // Debug.LogError(analysis_BalanceAgainst);

            // Limit the Type
            List<StimulusType> sT_Options = new List<StimulusType>();

            // Type first ; do we have too much of one type?
            {
                float numTypeMax = float.MinValue;

                foreach (StimulusType _sT in Utility_Helper.EnumGetValues<StimulusType>().Shuffle())
                {
                    // None of them to remove, so just abort this thought
                    if (analysis_RemoveFrom.typeCount[_sT] == 0) continue;

                    float numType = analysis_BalanceAgainst.typeCount[_sT];

                    if (_sT != StimulusType.None)
                        numType /= 2f; // 2 : 2 : 1 ratio intended

                    if (numType < numTypeMax) continue;

                    if (numType == numTypeMax)
                    {
                        sT_Options.Add(_sT);
                        continue;
                    }

                    sT_Options.Clear();
                    sT_Options.Add(_sT);
                    numTypeMax = numType;
                }
            }

            // For the options we have, pick the best DIR
            List<TypeLocation> typeLoc = new List<TypeLocation>();

            {
                float numLocationMax = int.MinValue;

                foreach (StimulusType _sT in sT_Options)
                    foreach (Direction_2D_Diagonal d2D in Utility_Helper.EnumGetValues<Direction_2D_Diagonal>().Shuffle())
                    {
                        TypeLocation tL = new TypeLocation(_sT, d2D);

                        // None of them to remove, so just abort this thought
                        if (analysis_RemoveFrom.typeLocCount[tL] == 0) continue;

                        float numLocation = analysis_BalanceAgainst.typeLocCount[tL];

                        if (tL.type != StimulusType.None)
                            numLocation /= 2f; // 2 : 2 : 1 ratio intended

                        if (numLocation < numLocationMax) continue;

                        if (numLocation == numLocationMax)
                        {
                            typeLoc.Add(tL);
                            continue;
                        }

                        typeLoc.Clear();
                        typeLoc.Add(tL);
                        numLocationMax = numLocation;
                    }
            }

            // For the TypeLocation options we have, pick the best ID
            List<SpriteLocation> finalOptions = new List<SpriteLocation>();

            {
                float numSL_Max = int.MinValue;

                Action<SpriteLocation> checkUpdate = sL =>
                {
                    // None of them to remove, so just abort this thought
                    if (analysis_RemoveFrom.spriteLocCount[sL.spriteName][sL.direction] == 0) return;

                    float numSL = analysis_BalanceAgainst.spriteLocCount[sL.spriteName][sL.direction];

                    if (sL.typeLocation.type != StimulusType.None)
                        numSL /= 2f; // 2 : 2 : 1 ratio intended

                    if (numSL < numSL_Max) return;

                    if (numSL == numSL_Max)
                    {
                        finalOptions.Add(sL);
                        return;
                    }

                    finalOptions.Clear();
                    finalOptions.Add(sL);
                    numSL_Max = numSL;
                };

                foreach (TypeLocation tL in typeLoc)
                {

                    if (tL.type == StimulusType.None)
                    {
                        SpriteLocation sL = GetStimFrom_Blank(tL.direction);
                        checkUpdate(sL);
                    }
                    else
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            SpriteLocation sL = GetStimFrom(tL.type == StimulusType.Face ? stimuliTexturesFaces : stimuliTexturesObjects, spriteInfo, i, tL.direction);
                            checkUpdate(sL);
                        }
                    }
                }
            }

            return finalOptions.GetRandom();
        }

        private static SpriteLocation GetStimBalanced(Dictionary<Sprite, KeyValuePair<string, StimulusType>> spriteInfo, List<Sprite> stimuliTexturesFaces, List<Sprite> stimuliTexturesObjects, List<SpriteLocation> balanceAgainst, List<SpriteLocation> pickFrom = null)
        {
            QueueAnalysis analysis = Analyze(balanceAgainst);
            QueueAnalysis analysis_PickFrom = pickFrom != null ? Analyze(pickFrom) : default;

            // Debug.LogError(analysis);

            // Limit the Type
            List<StimulusType> sT_Options = new List<StimulusType>();

            {
                float numTypeMin = float.MaxValue;

                foreach (StimulusType _sT in Utility_Helper.EnumGetValues<StimulusType>().Shuffle())
                {
                    // None of them to remove, so just abort this thought
                    if (pickFrom != null && analysis_PickFrom.typeCount[_sT] == 0) continue;

                    float numType = analysis.typeCount[_sT];

                    if (_sT != StimulusType.None)
                        numType /= 2f; // 2 : 2 : 1 ratio intended

                    if (numType > numTypeMin) continue;

                    if (numType == numTypeMin)
                    {
                        sT_Options.Add(_sT);
                        continue;
                    }

                    sT_Options.Clear();
                    sT_Options.Add(_sT);
                    numTypeMin = numType;
                }
            }

            // For the options we have, pick the best DIR
            List<TypeLocation> typeLoc = new List<TypeLocation>();

            {
                float numLocationMin = int.MaxValue;

                foreach (StimulusType _sT in sT_Options)
                    foreach (Direction_2D_Diagonal d2D in Utility_Helper.EnumGetValues<Direction_2D_Diagonal>().Shuffle())
                    {
                        TypeLocation tL = new TypeLocation(_sT, d2D);

                        // None of them to remove, so just abort this thought
                        if (pickFrom != null && analysis_PickFrom.typeLocCount[tL] == 0) continue;

                        float numLocation = analysis.typeLocCount[tL];

                        if (tL.type != StimulusType.None)
                            numLocation /= 2f; // 2 : 2 : 1 ratio intended

                        if (numLocation > numLocationMin) continue;

                        if (numLocation == numLocationMin)
                        {
                            typeLoc.Add(tL);
                            continue;
                        }

                        typeLoc.Clear();
                        typeLoc.Add(tL);
                        numLocationMin = numLocation;
                    }
            }

            // For the TypeLocation options we have, pick the best ID
            List<SpriteLocation> finalOptions = new List<SpriteLocation>();

            {
                float numSL_Min = int.MaxValue;

                Action<SpriteLocation> checkUpdate = sL =>
                {
                    // None of them to remove, so just abort this thought
                    if (pickFrom != null && analysis_PickFrom.spriteLocCount[sL.spriteName][sL.direction] == 0) return;

                    float numSL = analysis.spriteLocCount[sL.spriteName][sL.direction];

                    if (sL.typeLocation.type != StimulusType.None)
                        numSL /= 2f; // 2 : 2 : 1 ratio intended

                    if (numSL > numSL_Min) return;

                    if (numSL == numSL_Min)
                    {
                        finalOptions.Add(sL);
                        return;
                    }

                    finalOptions.Clear();
                    finalOptions.Add(sL);
                    numSL_Min = numSL;
                };

                foreach (TypeLocation tL in typeLoc)
                {

                    if (tL.type == StimulusType.None)
                    {
                        SpriteLocation sL = GetStimFrom_Blank(tL.direction);
                        checkUpdate(sL);
                    }
                    else
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            SpriteLocation sL = GetStimFrom(tL.type == StimulusType.Face ? stimuliTexturesFaces : stimuliTexturesObjects, spriteInfo, i, tL.direction);
                            checkUpdate(sL);
                        }
                    }
                }
            }

            return finalOptions.GetRandom();
        }

        private static SpriteLocation GetStimBalanced_TypeLoc(Dictionary<Sprite, KeyValuePair<string, StimulusType>> spriteInfo, List<Sprite> stimuliTexturesFaces, List<Sprite> stimuliTexturesObjects, List<SpriteLocation> balanceAgainst)
        {
            QueueAnalysis analysis = Analyze(balanceAgainst);
            // Debug.LogError(analysis);

            // Pick the type
            StimulusType sT = default;

            {
                float numTypeMin = float.MaxValue;
                float numTypeMax = float.MinValue;

                foreach (StimulusType _sT in Utility_Helper.EnumGetValues<StimulusType>().Shuffle())
                {
                    float numType = analysis.typeCount[_sT];

                    if (_sT != StimulusType.None)
                        numType /= 2f; // 2 : 2 : 1 ratio intended

                    if (numType > numTypeMax)
                        numTypeMax = numType;

                    if (numType >= numTypeMin) continue;

                    numTypeMin = numType;
                    sT = _sT;
                }

                // If we don't really have anything to balance Type-wise
                if (numTypeMin == numTypeMax)
                    // Go for Location first
                    return GetStimBalanced_LocType(spriteInfo, stimuliTexturesFaces, stimuliTexturesObjects, balanceAgainst);
            }

            // Pick the location
            Direction_2D_Diagonal dir = default;

            {
                int numLocationMin = int.MaxValue;

                foreach (Direction_2D_Diagonal d2D in Utility_Helper.EnumGetValues<Direction_2D_Diagonal>().Shuffle())
                {
                    TypeLocation tL = new TypeLocation(sT, d2D);
                    int numLocation = analysis.typeLocCount[tL];

                    if (numLocation >= numLocationMin) continue;

                    dir = d2D;
                    numLocationMin = numLocation;
                }
            }

            // If we're blank - we're done! 
            if (sT == StimulusType.None)
                return GetStimFrom_Blank(dir);

            // Pick the ID
            int ID = -1;

            {
                int numIDMin = int.MaxValue;
                List<int> IDs = new List<int>();
                for (int i = 0; i < 10; i++)
                    IDs.Add(i);

                foreach (int i in IDs.Shuffle())
                {
                    string spriteName = spriteInfo[(sT == StimulusType.Face ? stimuliTexturesFaces : stimuliTexturesObjects)[i]].Key;
                    int numID = analysis.spriteLocCount[spriteName][dir];

                    if (numID >= numIDMin) continue;

                    ID = i;
                    numIDMin = numID;
                }
            }

            if (sT == StimulusType.Face)
                return GetStimFrom(stimuliTexturesFaces, spriteInfo, ID, dir);
            else
                return GetStimFrom(stimuliTexturesObjects, spriteInfo, ID, dir);
        }

        private static SpriteLocation GetStimBalanced(List<SpriteLocation> options, List<SpriteLocation> balanceAgainst)
        {
            SpriteLocation bestChoice = default;

            // Similarity approach
            {
                int leastSimilarity = int.MaxValue;

                foreach (SpriteLocation option in options)
                {
                    int similarityScore = option.GetSimilarity(balanceAgainst);

                    // Are we less?
                    if (similarityScore >= leastSimilarity) continue;

                    leastSimilarity = similarityScore;
                    bestChoice = option;
                }
            }

            return bestChoice;
        }

        private static SpriteLocation GetStimRandom(Dictionary<Sprite, KeyValuePair<string, StimulusType>> spriteInfo, List<Sprite> stimuliTexturesFaces, List<Sprite> stimuliTexturesObjects)
        {
            int draw = Utility_Helper.RandomRange(1, 5, true);
            return
                draw == 0 ? GetStimFrom_Blank(Utility_Helper.EnumGetRandom<Direction_2D_Diagonal>()) :
                draw <= 2 ? GetStimFrom(stimuliTexturesFaces, spriteInfo, Utility_Helper.RandomRange(0, 9, true), Utility_Helper.EnumGetRandom<Direction_2D_Diagonal>()) :
                GetStimFrom(stimuliTexturesObjects, spriteInfo, Utility_Helper.RandomRange(0, 9, true), Utility_Helper.EnumGetRandom<Direction_2D_Diagonal>());
        }
        private static Option GetOptionRandom(bool probed)
        {
            int draw = Utility_Helper.RandomRange(1, 5, true);
            int id = Utility_Helper.RandomRange(1, 10, true);
            Direction_2D_Diagonal dir = Utility_Helper.EnumGetRandom<Direction_2D_Diagonal>();
            return
                draw == 0 ? new Option(probed, -1, StimulusType.None, dir) :
                draw <= 2 ? new Option(probed, id, StimulusType.Face, dir) : new Option(probed, id, StimulusType.Object, dir);
        }

        private static void DoDebug()
        {
            string s = "";
            foreach (Dictionary<int, List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>> dic in REPLAY_ID_TO_TRIAL_AND_DIRECTION)
            {
                foreach (List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>> list in dic.Values)
                {
                    foreach (Triplet<bool?, Direction_2D_Diagonal, StimulusType> triplet in list)
                        s += "{0}{1}{2}, "._Format(
                            triplet.a == true ? "p" :
                            triplet.a == false ? "u" : "x",
                            triplet.b == Direction_2D_Diagonal.BottomLeft ? "Lb" :
                            triplet.b == Direction_2D_Diagonal.TopLeft ? "Lt" :
                            triplet.b == Direction_2D_Diagonal.TopRight ? "Rt" : "Rb",
                            triplet.c == StimulusType.Face ? "F" :
                            triplet.c == StimulusType.Object ? "O" : ""
                            );
                    s = s.Substring(0, s.Length - 2);
                    s += ";";
                }
                s = s.Substring(0, s.Length - 1);
                s += ";;;;";
            }
            s = s.Substring(0, s.Length - 4);
            Debug.Log(s);
        }

        private static void Flow(List<SpriteLocation> list, StimulusType stimulusType, int flow, Direction_2D_Diagonal from, Direction_2D_Diagonal to)
        {
            if (flow == 0) return;

            if (flow < 0)
            {
                Flow(list, stimulusType, - flow, to, from);
                return;
            }

            for (int i = 0; i < flow; i ++)
            {
                int indexFrom = list.FindIndex(a => a.type == stimulusType && a.direction == from);

                if (indexFrom < 0)
                {
                    if (!MASS_GENERATION_MODE) Debug.LogError("Requested to move an item from {0} to {1} but there isn't any left\n{2}"._Format(from, to, list.ToReadableString()));
                    continue;
                }

                SpriteLocation temp = list[indexFrom];
                temp.typeLocation.direction = to;
                list[indexFrom] = temp;
            }
        }

        private static float DoAnalysis25(List<SpriteLocation> queue, int label, bool acceptHigherNumbers = false)
        {
            return DoAnalysis(queue, label, 25, new Vector2Int(6, 7), new Dictionary<StimulusType, int>() { { StimulusType.Face, 10 }, { StimulusType.Object, 10 }, { StimulusType.None, 5 } }, 1, 1, 1, 1, 1, acceptHigherNumbers);
        }

        private static float DoAnalysis50(List<SpriteLocation> queue, int label, bool acceptHigherNumbers = false)
        {
            return DoAnalysis(queue, label, 50, new Vector2Int(12, 13), new Dictionary<StimulusType, int>() { { StimulusType.Face, 20 }, { StimulusType.Object, 20 }, { StimulusType.None, 10 } }, 2, 0, 0, 1, 1, acceptHigherNumbers);
        }

        private static float DoAnalysis75(List<SpriteLocation> queue, int label, bool acceptHigherNumbers = false)
        {
            return DoAnalysis(queue, label, 75, new Vector2Int(18, 19), new Dictionary<StimulusType, int>() { { StimulusType.Face, 30 }, { StimulusType.Object, 30 }, { StimulusType.None, 15 } }, 3, 0, 0, 1, 1, acceptHigherNumbers); // maybe last 1-2 arg(s) 0
        }

        public static float DoAnalysis100(List<SpriteLocation> queue, int label, bool acceptHigherNumbers = false)
        {
            return DoAnalysis(queue, label, 100, new Vector2Int(25, 25), new Dictionary<StimulusType, int>() { { StimulusType.Face, 40 }, { StimulusType.Object, 40 }, { StimulusType.None, 20 } }, 4, 0, 0, 0, 0, acceptHigherNumbers);
        }

        public static float DoAnalysis150(List<SpriteLocation> queue, int label, bool acceptHigherNumbers = false)
        {
            return DoAnalysis(queue, label, 150, new Vector2Int(37, 38), new Dictionary<StimulusType, int>() { { StimulusType.Face, 60 }, { StimulusType.Object, 60 }, { StimulusType.None, 30 } }, 6, 0, 0, 0, 0, acceptHigherNumbers);
        }

        public static float DoAnalysis200(List<SpriteLocation> queue, int label, bool acceptHigherNumbers = false)
        {
            return DoAnalysis(queue, label, 200, new Vector2Int(50, 50), new Dictionary<StimulusType, int>() { { StimulusType.Face, 80 }, { StimulusType.Object, 80 }, { StimulusType.None, 40 } }, 8, 0, 0, 0, 0, acceptHigherNumbers);
        }

        public static float DoAnalysis300(List<SpriteLocation> queue, int label, bool acceptHigherNumbers = false)
        {
            return DoAnalysis(queue, label, 300, new Vector2Int(75, 75), new Dictionary<StimulusType, int>() { { StimulusType.Face, 120 }, { StimulusType.Object, 120 }, { StimulusType.None, 60 } }, 12, 0, 0, 0, 0, acceptHigherNumbers);
        }

        public static float DoAnalysis400(List<SpriteLocation> queue, int label, bool acceptHigherNumbers = false)
        {
            return DoAnalysis(queue, label, 400, new Vector2Int(100, 100), new Dictionary<StimulusType, int>() { { StimulusType.Face, 160 }, { StimulusType.Object, 160 }, { StimulusType.None, 80 } }, 16, 0, 0, 0, 0, acceptHigherNumbers);
        }

        public static float DoAnalysis600(List<SpriteLocation> queue, int label, bool acceptHigherNumbers = false)
        {
            return DoAnalysis(queue, label, 600, new Vector2Int(150, 150), new Dictionary<StimulusType, int>() { { StimulusType.Face, 240 }, { StimulusType.Object, 240 }, { StimulusType.None, 120 } }, 24, 0, 0, 0, 0, acceptHigherNumbers);
        }
        
        private static float DoAnalysis(List<SpriteLocation> queue, int label, int total,
            Vector2Int countLimit, Dictionary<StimulusType, int> wantedCountPerType, int wantedCountPerID, 
            int leftRightThresholdType, int leftRightThresholdID, int locationThresholdType, int locationThresholdID, bool acceptHigherNumbers = false)
        {
            // If we accept higher numbers, it will get messier ; make sure we have that flexibility
            if (acceptHigherNumbers)
            {
                leftRightThresholdType = Math.Max(leftRightThresholdType, 1);
                leftRightThresholdID = Math.Max(leftRightThresholdID, 1);
                locationThresholdType = Math.Max(locationThresholdType, 1);
                locationThresholdID = Math.Max(locationThresholdID, 1);
            }
            
            QueueAnalysis analysis = Analyze(queue);
            Debug.Log(queue.ToReadableString());

            float numChecks = 0; // Normalizer
            float score = 0; // Errors -1, warnings 0, logs +1
            float REWARD_HIGH = 1.0f;
            float REWARD_MID = 0.5f;
            float REWARD_LOW = 0.25f;

            float weightOverall = 1;
            float weightLocations = 1;
            float weightTypes = 1;
            float weightIDs = 1;

            // Overall
            numChecks++;
            if (queue.Count == total)
            {
                score += REWARD_HIGH;
                Debug.Log("Total :: " + total);
            }
            else if (queue.Count > total && acceptHigherNumbers)
            {
                score += REWARD_MID;
                Debug.LogWarning("Total :: " + queue.Count + " > " + total);
            }
            else
            {
                score += REWARD_LOW;
                Debug.LogError("Total :: " + queue.Count + " != " + total);
            }

            // Locations
            int numChecksLocations = 0;
            float scoreLocations = 0;
            foreach (KeyValuePair<Direction_2D_Diagonal, int> kVP in analysis.locCount)
            {
                Direction_2D_Diagonal direction = kVP.Key;
                int count = kVP.Value;

                numChecksLocations++;

                // We are over, and we won't take it
                if ((count > countLimit.y && !acceptHigherNumbers) ||
                    count < countLimit.x)
                {
                    // Do we have the wanted number for our directions?
                    scoreLocations += REWARD_MID;
                    Debug.LogError(label + " :: " + direction + " -> Wrong Count " + count + " outside " + countLimit);
                    continue;
                }

                scoreLocations += REWARD_HIGH;

                Debug.Log(label + " :: " + direction + " -> Correct Count " + count + " inside " + countLimit);
            }

            numChecks += weightLocations;
            score += (scoreLocations / numChecksLocations) * weightLocations;

            // Types
            int numChecksTypes = 0;
            float scoreTypes = 0;
            foreach (KeyValuePair<StimulusType, int> kVP in analysis.typeCount)
            {
                StimulusType stimType = kVP.Key;
                int count = kVP.Value;

                int wantedCountType = wantedCountPerType[stimType];

                numChecksTypes++;

                // Do we have the wanted number of this type?
                if ((count > wantedCountType && !acceptHigherNumbers) ||
                    count < wantedCountType)
                {
                    scoreTypes += REWARD_LOW;
                    Debug.LogError(label + " :: " + stimType + " -> Wrong Count " + count + " != " + wantedCountType);
                    continue;
                }

                // Balanced Left Right?
                int countLeft = analysis.typeLocCount.Sum(a => a.Key.type == stimType &&
                    (a.Key.direction == Direction_2D_Diagonal.TopLeft || a.Key.direction == Direction_2D_Diagonal.BottomLeft) ? a.Value : 0);
                int countRight = analysis.typeLocCount.Sum(a => a.Key.type == stimType &&
                    (a.Key.direction == Direction_2D_Diagonal.TopRight || a.Key.direction == Direction_2D_Diagonal.BottomRight) ? a.Value : 0);

                if (Math.Abs(countLeft - countRight) > leftRightThresholdType)
                {
                    scoreTypes += REWARD_LOW;
                    Debug.LogError(label + " :: " + stimType + " -> Correct Count (" + count + ") | Non-Balanced Left-Right |L(" + countLeft + "), R (" + countRight + ")| > " + leftRightThresholdType);
                    continue;
                }

                // Balanced Per Location?
                Dictionary<Direction_2D_Diagonal, int> locCountForType = new Dictionary<Direction_2D_Diagonal, int>();
                foreach (Direction_2D_Diagonal d in Utility_Helper.EnumGetValues<Direction_2D_Diagonal>())
                    locCountForType.Add(d, analysis.typeLocCount[new TypeLocation(stimType, d)]);

                int minLocCountForType = locCountForType.Values.ToList().Min();
                int maxLocCountForType = locCountForType.Values.ToList().Max();

                if (maxLocCountForType - minLocCountForType > locationThresholdType)
                {
                    scoreTypes += REWARD_MID;
                    Debug.LogError(label + " :: " + stimType + " -> Correct Count (" + count + ") | Balanced Left-Right |L(" + countLeft + "), R (" + countRight + ")| <= " + leftRightThresholdType + " | Non-Balanced Locations :: " +
                         locCountForType.ToReadableString() + " max - min > " + locationThresholdType);
                    continue;
                }

                scoreTypes += REWARD_HIGH;
                Debug.Log(label + " :: " + stimType + " -> Correct Count (" + count + ") + Balanced Left-Right |L(" + countLeft + "), R (" + countRight + ")| <= " + leftRightThresholdType + " | Balanced Locations :: " +
                         locCountForType.ToReadableString() + " max - min <= " + locationThresholdType);
            }

            numChecks += weightTypes;
            score += (scoreTypes / numChecksTypes) * weightTypes;

            // IDs
            int numChecksIDs = 0;
            float scoreIDs = 0;
            foreach (KeyValuePair<string, int> kVP in analysis.spriteCount)
            {
                string spriteName = kVP.Key;
                int count = kVP.Value;

                if (spriteName == StimulusManager.EMPTY_STIM_NAME) continue; // Nothing to check for stim IDs

                numChecksIDs++;

                if ((count > wantedCountPerID && !acceptHigherNumbers) ||
                    count < wantedCountPerID)
                {
                    scoreIDs += REWARD_LOW;
                    Debug.LogError(label + " :: " + spriteName + " -> Wrong Count " + count + " != " + wantedCountPerID);
                    continue;
                }

                // Balanced Left Right?
                int countLeft =
                    analysis.spriteLocCount[spriteName][Direction_2D_Diagonal.TopLeft] +
                    analysis.spriteLocCount[spriteName][Direction_2D_Diagonal.BottomLeft];

                int countRight =
                    analysis.spriteLocCount[spriteName][Direction_2D_Diagonal.TopRight] +
                    analysis.spriteLocCount[spriteName][Direction_2D_Diagonal.BottomRight];
                
                if (Math.Abs(countLeft - countRight) > leftRightThresholdID)
                {
                    scoreIDs += REWARD_LOW;
                    Debug.LogError(label + " :: " + spriteName + " -> Correct Count (" + count + ") | Non-Balanced Left-Right |L(" + countLeft + "), R (" + countRight + ")| > " + leftRightThresholdID);
                    continue;
                }

                // Balanced Per Location?
                Dictionary<Direction_2D_Diagonal, int> locCountForID = new Dictionary<Direction_2D_Diagonal, int>();
                foreach (Direction_2D_Diagonal d in Utility_Helper.EnumGetValues<Direction_2D_Diagonal>())
                    locCountForID.Add(d, analysis.spriteLocCount[spriteName][d]);

                int minLocCountForID = locCountForID.Values.ToList().Min();
                int maxLocCountForID = locCountForID.Values.ToList().Max();

                if (maxLocCountForID - minLocCountForID > locationThresholdID)
                {
                    scoreIDs += REWARD_MID;
                    Debug.LogError(label + " :: " + spriteName + " -> Correct Count (" + count + ") | Balanced Left-Right |L(" + countLeft + "), R (" + countRight + ")| <= " + leftRightThresholdType + " | Non-Balanced Locations :: " +
                         locCountForID.ToReadableString() + " max - min > " + locationThresholdID);
                    continue;
                }

                scoreIDs += REWARD_HIGH;
                Debug.Log(label + " :: " + spriteName + " -> Correct Count (" + count + ") + Balanced Left-Right |L(" + countLeft + "), R (" + countRight + ")| <= " + leftRightThresholdType + " | Balanced Locations :: " +
                         locCountForID.ToReadableString() + " max - min <= " + locationThresholdID);
            }

            numChecks += weightIDs;
            score += (scoreIDs / numChecksIDs) * weightIDs;

            return score / numChecks;
        }

        private static List<SpriteLocation> Balance(Dictionary<StimulusType, int> wantedPerType_Total, List<SpriteLocation> stimulusQueue_Total_TEMP, List<SpriteLocation> stimulusQueue_Replay)
        {
            List<SpriteLocation> stimulusQueue_Total = new List<SpriteLocation>(stimulusQueue_Total_TEMP);

            QueueAnalysis replay = Analyze(stimulusQueue_Replay);
            QueueAnalysis world = Analyze(stimulusQueue_Total);

            // Make sure the numbers add up (remove extras)
            foreach (StimulusType sT in Utility_Helper.EnumGetValues<StimulusType>())
            {
                int numWanted = wantedPerType_Total[sT];
                int numSelf = world.typeCount[sT];
                int numReplay = replay.typeCount[sT];
                int numTotal = numSelf + numReplay;
                int numExtra = numTotal - numWanted;

                if (numExtra == 0) continue;

                if (numExtra < 0)
                {
                    Debug.LogError("??");
                    continue;
                }

                List<SpriteLocation> toRemove = stimulusQueue_Total.FindAll(sL => sL.type == sT);

                for (int i = 0; i < numExtra; i++)
                {
                    // Pick a good one to remove
                    SpriteLocation choice = toRemove.Aggregate((a, b) =>
                    {
                        QueueAnalysis current = Analyze(stimulusQueue_Total);

                        // In choosing which one to REMOVE, choose the one with the highest occurences
                        int numOccurencesA;
                        int numOccurencesB;

                        // Balance per Direction
                        // int numOccurencesA = current.locCount[a.direction] + replay.locCount[a.direction];
                        // int numOccurencesB = current.locCount[b.direction] + replay.locCount[b.direction];

                        // Balance per Direction (of this type)
                        numOccurencesA = current.typeLocCount[a.typeLocation] + replay.typeLocCount[a.typeLocation];
                        numOccurencesB = current.typeLocCount[b.typeLocation] + replay.typeLocCount[b.typeLocation];

                        if (numOccurencesA > numOccurencesB) return a;
                        if (numOccurencesA < numOccurencesB) return b;

                        // Balance per count of ID
                        numOccurencesA = current.spriteCount[a.spriteName] + replay.spriteCount[a.spriteName];
                        numOccurencesB = current.spriteCount[b.spriteName] + replay.spriteCount[a.spriteName];

                        if (numOccurencesA > numOccurencesB) return a;
                        if (numOccurencesA < numOccurencesB) return b;

                        return a;
                    });

                    toRemove.Remove(choice);
                    stimulusQueue_Total.Remove(choice);
                }
            }
            return stimulusQueue_Total;
        }

        [Serializable]
        private struct QueueAnalysis
        {
            public int numTotal;
            public Dictionary<string, int> spriteCount;
            public Dictionary<string, Dictionary<Direction_2D_Diagonal, int>> spriteLocCount;
            public Dictionary<StimulusType, int> typeCount;
            public Dictionary<Direction_2D_Diagonal, int> locCount;
            public Dictionary<TypeLocation, int> typeLocCount;

            public override string ToString()
            {
                return "{0}\n{1}\n{2}\n{3}\n{4}"._Format(
                    spriteCount.ToReadableString("spriteCount"),
                    spriteLocCount.ToReadableString("spriteLocCount"),
                    typeCount.ToReadableString("typeCount"),
                    locCount.ToReadableString("locCount"),
                    typeLocCount.ToReadableString("typeLocCount")
                    );
            }

            public static QueueAnalysis EMPTY { get { return new QueueAnalysis(0); } }

            private QueueAnalysis(int sth)
            {
                numTotal = 0;

                spriteCount = new Dictionary<string, int>();
                for (int i = 1; i <= 10; i++)
                {
                    spriteCount.Add("SF" + i.AddLeadingSymbols(2, '0'), 0);
                    spriteCount.Add("SO" + i.AddLeadingSymbols(2, '0'), 0);
                }
                spriteCount.Add(StimulusManager.EMPTY_STIM_NAME, 0);

                spriteLocCount = new Dictionary<string, Dictionary<Direction_2D_Diagonal, int>>();
                foreach (string sprite in spriteCount.Keys)
                {
                    spriteLocCount.Add(sprite, new Dictionary<Direction_2D_Diagonal, int>());
                    foreach (Direction_2D_Diagonal d in Utility_Helper.EnumGetValues<Direction_2D_Diagonal>())
                        spriteLocCount[sprite].Add(d, 0);
                }

                typeCount = new Dictionary<StimulusType, int>();
                foreach (StimulusType sT in Utility_Helper.EnumGetValues<StimulusType>())
                    typeCount.Add(sT, 0);

                locCount = new Dictionary<Direction_2D_Diagonal, int>();
                foreach (Direction_2D_Diagonal d in Utility_Helper.EnumGetValues<Direction_2D_Diagonal>())
                    locCount.Add(d, 0);

                typeLocCount = new Dictionary<TypeLocation, int>();
                foreach (StimulusType sT in Utility_Helper.EnumGetValues<StimulusType>())
                    foreach (Direction_2D_Diagonal d in Utility_Helper.EnumGetValues<Direction_2D_Diagonal>())
                        typeLocCount.Add(new TypeLocation(sT, d), 0);
            }
        }

        private static QueueAnalysis Analyze(List<SpriteLocation> stimulusQueue_World_PROBED)
        {
            QueueAnalysis queueAnalysis = QueueAnalysis.EMPTY;

            foreach (SpriteLocation sL in stimulusQueue_World_PROBED)
            {
                queueAnalysis.numTotal++;

                if (sL.spriteName == null)
                    Debug.LogError(sL);
                else
                {
                    queueAnalysis.spriteCount[sL.spriteName]++;
                    queueAnalysis.spriteLocCount[sL.spriteName][sL.direction]++;
                }

                queueAnalysis.typeCount[sL.type]++;
                queueAnalysis.locCount[sL.direction]++;
                queueAnalysis.typeLocCount[sL.typeLocation]++;
            }

            return queueAnalysis;
        }

        private static Dictionary<StimulusType, int> GetNumGame(int numWantedUnprobed, float percentileBlank_Gameplay)
        {
            int numFacesUnprobed = Mathf.RoundToInt(numWantedUnprobed * (1 - percentileBlank_Gameplay) / 2f);
            int numObjectsUnprobed = Mathf.RoundToInt(numWantedUnprobed * (1 - percentileBlank_Gameplay) / 2f);
            int numBlanksUnprobed = numWantedUnprobed - numFacesUnprobed - numObjectsUnprobed;

            return new Dictionary<StimulusType, int>()
            {
                { StimulusType.Face, numFacesUnprobed },
                { StimulusType.Object, numObjectsUnprobed },
                { StimulusType.None, numBlanksUnprobed },
            };
        }

        private static Dictionary<StimulusType, int> GetNumLocalizer(int localizer_numWantedUnprobed, Dictionary<Direction_2D_Diagonal, List<int>> setToUseUnprobed)
        {
            int localizer_numBlanksUnprobed = 0;
            foreach (KeyValuePair<Direction_2D_Diagonal, List<int>> kVP in setToUseUnprobed)
                localizer_numBlanksUnprobed += kVP.Value.FindAll(id => id < 0).Count;
            int localizer_numFacesUnprobed = Mathf.RoundToInt((localizer_numWantedUnprobed - localizer_numBlanksUnprobed) / 2f);
            int localizer_numObjetsnprobed = localizer_numWantedUnprobed - localizer_numBlanksUnprobed - localizer_numFacesUnprobed;

            return new Dictionary<StimulusType, int>()
            {
                { StimulusType.Face, localizer_numFacesUnprobed },
                { StimulusType.Object, localizer_numObjetsnprobed },
                { StimulusType.None, localizer_numBlanksUnprobed },
            };
        }

        /*
        private static List<SpriteLocation> GenerateQueue(Dictionary<Direction_2D_Diagonal, List<int>> directionToSet,
            List<Sprite> stimuliTexturesFaces, List<Sprite> stimuliTexturesObjects, Dictionary<Sprite, KeyValuePair<string, StimulusType>> spriteInfo)
        {
            List<SpriteLocation> queue = new List<SpriteLocation>();

            // Add Faces & Objects for each ID
            foreach (Direction_2D_Diagonal d in Utility_Helper.EnumGetValues<Direction_2D_Diagonal>())
                foreach (int ID in directionToSet[d])
                {
                    queue.Add(GetStimFrom(stimuliTexturesFaces, spriteInfo, ID, d));
                    queue.Add(GetStimFrom(stimuliTexturesObjects, spriteInfo, ID, d));
                }

            // TODO BLANKS
            // GetStimFrom_Blank(d)

            return queue;
        }
        */
        private static NumWanted? GetNumWantedFromSequence(List<EventType> sequence)
        {
            int numWantedThisWorld_UNPROBED = sequence.FindAll(a => a == EventType.Stimulus).Count;
            int numWantedThisWorld_PROBED = sequence.FindAll(a => a == EventType.Probe).Count;

            if (numWantedThisWorld_UNPROBED + numWantedThisWorld_PROBED != sequence.Count)
            {
                Debug.LogError("Weird :: {0} + {1} != {2} | ABORTING"._Format(numWantedThisWorld_UNPROBED, numWantedThisWorld_PROBED, sequence.Count));
                return null;
            }

            return new NumWanted(numWantedThisWorld_UNPROBED, numWantedThisWorld_PROBED);
        }

        private struct NumWanted
        {
            public int unprobed;
            public int probed;

            public NumWanted(int unprobed, int probed)
            {
                this.unprobed = unprobed;
                this.probed = probed;
            }
        }

        private struct StimulusQueues
        {
            public List<SpriteLocation> unprobed;
            public List<SpriteLocation> probed;

            public StimulusQueues(List<SpriteLocation> unprobed, List<SpriteLocation> probed)
            {
                this.unprobed = unprobed;
                this.probed = probed;
            }
        }

        private static SpriteLocation GetStimFrom(List<Sprite> stimuliTexturesFaces, Dictionary<Sprite, KeyValuePair<string, StimulusType>> spriteInfo, int ID, Direction_2D_Diagonal l)
        {
            Sprite sprite = stimuliTexturesFaces[ID];
            return new SpriteLocation(sprite, l, spriteInfo[sprite].Key, spriteInfo[sprite].Value);
        }

        private static SpriteLocation GetStimFrom_Blank(Direction_2D_Diagonal l)
        {
            return new SpriteLocation(null, l, StimulusManager.EMPTY_STIM_NAME, StimulusType.None);
        }

        private static List<SpriteLocation> GenerateFromSet(
            Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>> typeDirectionToSet,
            Dictionary<StimulusType, int> numLocalizerUnprobed,
            List<SpriteLocation> usedItems,
            List<Sprite> stimuliTexturesFaces, List<Sprite> stimuliTexturesObjects,
            Dictionary<Sprite, KeyValuePair<string, StimulusType>> spriteInfo)
        {
            List<SpriteLocation> BASE_FACES = new List<SpriteLocation>();
            List<SpriteLocation> BASE_OBJECTS = new List<SpriteLocation>();
            List<SpriteLocation> BASE_BLANKS = new List<SpriteLocation>();

            foreach (Direction_2D_Diagonal l in Utility_Helper.EnumGetValues<Direction_2D_Diagonal>())
            {
                foreach (int ID in typeDirectionToSet[StimulusType.Face][l])
                    BASE_FACES.Add(GetStimFrom(stimuliTexturesFaces, spriteInfo, ID, l));

                foreach (int ID in typeDirectionToSet[StimulusType.Object][l])
                    BASE_OBJECTS.Add(GetStimFrom(stimuliTexturesObjects, spriteInfo, ID, l));

                foreach (int ID in typeDirectionToSet[StimulusType.None][l])
                    BASE_BLANKS.Add(GetStimFrom_Blank(l));
            }

            List<SpriteLocation> result = new List<SpriteLocation>();

            result.AddRange(GenerateListCopies(BASE_FACES, numLocalizerUnprobed[StimulusType.Face], usedItems.FindAll(sL => sL.type == StimulusType.Face)));
            result.AddRange(GenerateListCopies(BASE_OBJECTS, numLocalizerUnprobed[StimulusType.Object], usedItems.FindAll(sL => sL.type == StimulusType.Object)));
            result.AddRange(GenerateListCopies(BASE_BLANKS, numLocalizerUnprobed[StimulusType.None], usedItems.FindAll(sL => sL.type == StimulusType.None)));

            return result;
        }

        /// <summary>
        /// Does not affect the provided list. May return less than the wanted number (even 0) depending on avoid items
        /// </summary>
        /// <param name="_baseList"></param>
        /// <param name="numWanted"></param>
        /// <returns></returns>
        private static List<T> GenerateListCopies<T>(List<T> _baseList, int numWanted, List<T> _avoidItems)
        {
            List<T> result = new List<T>();
            List<T> baseList = new List<T>(_baseList).Shuffle().CustomToList(); // don't destruct the argument
            List<T> avoidItems = new List<T>(_avoidItems);

            // Fixed number of attempts
            for (int i = 0; i < numWanted; i++)
            {
                // Pick the next item on the list
                T choice = baseList[i % baseList.Count];

                // "Burn Up" once
                if (avoidItems.Contains(choice))
                    avoidItems.Remove(choice);
                else
                    result.Add(choice);
            }

            return result;
        }

        private static readonly bool BLANK_PAIR_OVER_SINGE = true;

        // DONE
        private static Dictionary<int, Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>> RUN_TYPE_DIRECTION_TO_SET_PROBED = new Dictionary<int, Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>>()
        {
            // == RUN 0 | DONE
            { 0, new Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>()
            {
                // == FACES
                { StimulusType.Face, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 2, 4, 9 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 8, 6} },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 3, 1 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 7, 0, 5 } },
                } },
                
                // == OBJECTS
                { StimulusType.Object, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 2, 0 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 6, 8, 5} },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 1, 3 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 7, 4, 9 } },
                } },
                
                // == BLANK
                { StimulusType.None, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { -1,  } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { -1,  } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { -1, -1 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { -1,  } },
                } },
            } },

            // == RUN 1 | DONE
            { 1, new Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>()
            {
                // == FACES
                { StimulusType.Face, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 0, 1 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 2, 3, 4 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 5, 6, 7 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 8, 9 } },
                } },
                
                // == OBJECTS
                { StimulusType.Object, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 9, 8, 7 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 6, 5} },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 4, 3 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 2, 1, 0 } },
                } },
                
                // == BLANK
                { StimulusType.None, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { -1,  } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { -1,  } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { -1,  } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { -1, -1 } },
                } },
            } },

            // == RUN 2 | DONE
            { 2, new Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>()
            {
                // == FACES
                { StimulusType.Face, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 9, 8 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 7, 6, 5} },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 4, 3, 2 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 1, 0 } },
                } },
                
                // == OBJECTS
                { StimulusType.Object, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 0, 1, 2 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 3, 4 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 5, 6 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 7, 8, 9 } },
                } },
                
                // == BLANK
                { StimulusType.None, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { -1, -1 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { -1,  } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { -1,  } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { -1,  } },
                } },
            } },

            // == RUN 3 | DONE
            { 3, new Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>()
            {
                // == FACES
                { StimulusType.Face, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 5, 4, 8 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 9, 0 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 2, 7 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 6, 1 , 3 } },
                } },
                
                // == OBJECTS
                { StimulusType.Object, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 5, 8 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 0, 9, 1 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 2, 7 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 6, 3, 4 } },
                } },
                
                // == BLANK
                { StimulusType.None, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { -1,  } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { -1, -1 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { -1,  } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { -1, } },
                } },
            } },
        };

        // DONE
        private static Dictionary<int, Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>> RUN_TYPE_DIRECTION_TO_SET_UNPROBED = new Dictionary<int, Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>>()
        {
            // == RUN 0 | DONE
            { 0, new Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>()
            {
                // == FACES
                { StimulusType.Face, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 6, 7, 0, 4, 8 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 1, 5, 2, 7, 8} },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 9, 5, 1, 2, 3 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 0, 4, 3, 9, 6 } },
                } },
                
                // == OBJECTS
                { StimulusType.Object, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 0, 4, 8, 6, 7 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 2, 1, 5, 7, 8 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 5, 9, 1, 2, 3 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 3, 9, 0, 4, 6 } },
                } },
                
                // == BLANK
                { StimulusType.None, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { -1, -1, -1 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { -1, -1 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { -1, -1 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { -1, -1, -1 } },
                } },
            } },

            // == RUN 1 | DONE
            { 1, new Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>()
            {
                // == FACES
                { StimulusType.Face, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 0, 1, 2, 3, 4 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 1, 3, 5, 7, 9} },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 5, 6, 7, 8, 9 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 0, 2, 4, 6, 8 } },
                } },
                
                // == OBJECTS
                { StimulusType.Object, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 9, 8, 7, 6, 5 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 1, 4, 7, 0, 3 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 4, 3, 2, 1, 0 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 2, 5, 6, 8, 9 } },
                } },
                
                // == BLANK
                { StimulusType.None, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { -1, -1 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { -1, -1, -1 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { -1, -1, -1 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { -1, -1 } },
                } },
            } },
            
            // == RUN 2 | DONE
            { 2, new Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>()
            {
                // == FACES
                { StimulusType.Face, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 9, 8, 7, 6, 5 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 0, 3, 6, 9, 2 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 4, 3, 2, 1, 0 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 1, 4, 5, 7, 8 } },
                } },
                
                // == OBJECTS
                { StimulusType.Object, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 0, 1, 2, 3, 4 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 0, 2, 4, 6, 8 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 5, 6, 7, 8, 9 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 1, 3, 5, 7, 9 } },
                } },
                
                // == BLANK
                { StimulusType.None, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { -1, -1, -1 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { -1, -1 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { -1, -1 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { -1, -1, -1 } },
                } },
            } },

            // == RUN 3 | DONE
            { 3, new Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>()
            {
                // == FACES
                { StimulusType.Face, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 1, 3, 9, 2, 5 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 3, 7, 4, 0, 6 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 0, 4, 8, 6, 7 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 2, 1, 5, 8, 9 } },
                } },
                
                // == OBJECTS
                { StimulusType.Object, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 3, 9, 1, 2, 5 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 4, 3, 7, 0, 6 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 6, 0, 4, 8, 7 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 1, 5, 8, 2, 9 } },
                } },
                
                // == BLANK
                { StimulusType.None, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { -1, -1 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { -1, -1, -1 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { -1, -1, -1 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { -1, -1 } },
                } },
            } },
        };

        // DONE
        private static readonly List<Dictionary<int, List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>>> REPLAY_ID_TO_TRIAL_AND_DIRECTION = new List<Dictionary<int, List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>>>()
        {
            // REPLAY 0 WORLD 0 | DONE
            new Dictionary<int, List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>>()
            {

                { 0,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Face),
                    }
                },
                { 1,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Face),
                    }
                },
                { 2,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Object),
                    }
                },
                { 3,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Object),
                    }
                },
                { 4,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Face),
                    }
                },
                { 5,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Face),
                    }
                },
                { 6,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomRight, StimulusType.Object),
                    }
                },
                { 7,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopRight, StimulusType.Object),
                    }
                },
                { 8,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomRight, StimulusType.Face),
                    }
                },
                { 9,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Object),
                    }
                },
                { -1,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopLeft, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.None), // false
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomRight, StimulusType.None), // true
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.None),
                        // new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, BLANK_PAIR_OVER_SINGE ? Direction_2D_Diagonal.BottomRight : Direction_2D_Diagonal.BottomLeft, StimulusType.None),
                    }
                },
            },

            // REPLAY 1 WORLD 0 | DONE
            new Dictionary<int, List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>>()
            {

                { 0,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Object),
                    }
                },
                { 1,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Object),
                    }
                },
                { 2,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Face),
                    }
                },
                { 3,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Face),
                    }
                },
                { 4,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Object),
                    }
                },
                { 5,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Object),
                    }
                },
                { 6,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomRight, StimulusType.Face),
                    }
                },
                { 7,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopRight, StimulusType.Face),
                    }
                },
                { 8,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomRight, StimulusType.Object),
                    }
                },
                { 9,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Face),
                    }
                },
                { -1,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopLeft, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomRight, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.None),
                    }
                },
            },
            
            // REPLAY 0 WORLD 1 | DONE
            new Dictionary<int, List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>>()
            {
                { 0,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomRight, StimulusType.Object),
                    }
                },
                { 1,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Object),
                    }
                },
                { 2,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Face),
                    }
                },
                { 3,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Face),
                    }
                },
                { 4,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Object),
                    }
                },
                { 5,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Object),
                    }
                },
                { 6,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopRight, StimulusType.Face),
                    }
                },
                { 7,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Face),
                    }
                },
                { 8,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Object),
                    }
                },
                { 9,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomRight, StimulusType.Face),
                    }
                },
                { -1,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(null, Direction_2D_Diagonal.TopLeft, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomLeft, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(null, Direction_2D_Diagonal.TopRight, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomRight, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.None),
                    }
                },
            },

            // REPLAY 1 WORLD 1 | DONE
            new Dictionary<int, List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>>()
            {
                { 0,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomRight, StimulusType.Face),
                    }
                },
                { 1,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Face),
                    }
                },
                { 2,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Object),
                    }
                },
                { 3,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Object),
                    }
                },
                { 4,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Face),
                    }
                },
                { 5,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Face),
                    }
                },
                { 6,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopRight, StimulusType.Object),
                    }
                },
                { 7,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Object),
                    }
                },
                { 8,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Face),
                    }
                },
                { 9,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomRight, StimulusType.Object),
                    }
                },
                { -1,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopLeft, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(null, Direction_2D_Diagonal.TopRight, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(null, Direction_2D_Diagonal.TopRight, StimulusType.None),
                    }
                },
            },
        };

        public static void SetupQueue_OLD(List<SpriteLocation> queue, int numWanted,
            List<Sprite> stimuliTexturesFaces, List<Sprite> stimuliTexturesObjects,
            float percentile_Blank, float percentile_Faces, int reset_T_Every_Minimum,
            string fileName, Action callback, bool runOnThread)
        {
            bool wantToDebug = false;
            bool doDebug = wantToDebug && !runOnThread;
#if !UNITY_EDITOR
        doDebug = false;
#endif

            float timeStarted = TimeWrapper.realtimeSinceStartup_NotTS;

            // Debug.LogError("numWanted {0} - percentile_blank {1} - percentile_faces {2} - reset_T_Every {3}"._Format(numWanted, percentile_Blank, percentile_Faces, reset_T_Every));
            Dictionary<Sprite, KeyValuePair<string, StimulusType>> spriteInfo = new Dictionary<Sprite, KeyValuePair<string, StimulusType>>();

            foreach (Sprite s in stimuliTexturesFaces)
                spriteInfo.AddOrUpdate(s, new KeyValuePair<string, StimulusType>(s.name, StimulusType.Face));

            foreach (Sprite s in stimuliTexturesObjects)
                spriteInfo.AddOrUpdate(s, new KeyValuePair<string, StimulusType>(s.name, StimulusType.Object));

            Action action = () =>
            {
                // All Sprites
                List<Sprite> full_S = new List<Sprite>();
                full_S.AddRange(stimuliTexturesFaces);
                full_S.AddRange(stimuliTexturesObjects);
                full_S = full_S.Shuffle().CustomToList();

                // All Types
                List<StimulusType> full_T = Utility_Helper.EnumGetValues<StimulusType>();
                full_T = full_T.Shuffle().CustomToList();

                // Ordered Types
                float percentile_Objects = 1 - percentile_Blank - percentile_Faces;
                Dictionary<StimulusType, int> wantedTypes = new Dictionary<StimulusType, int>();
                wantedTypes[StimulusType.None] = Mathf.RoundToInt(percentile_Blank * reset_T_Every_Minimum);
                wantedTypes[StimulusType.Face] = Mathf.RoundToInt(percentile_Faces * reset_T_Every_Minimum);
                wantedTypes[StimulusType.Object] = reset_T_Every_Minimum - wantedTypes[StimulusType.None] - wantedTypes[StimulusType.Face];

                if (doDebug) Debug.LogError(wantedTypes.ToReadableString());
                /*
                List<StimulusType> full_T_Collection = new List<StimulusType>();
                for (int i = 0; i < resetEvery; i ++)
                {
                    float p = i / (float)resetEvery;
                    if (p <= percentile_Blank)
                        full_T_Collection.Add(StimulusType.None);
                    else if (p <= percentile_Blank + percentile_Faces)
                        full_T_Collection.Add(StimulusType.Face);
                    else
                        full_T_Collection.Add(StimulusType.Object);
                }
                */

                // All directions
                List<Direction_2D_Diagonal> full_L = Utility_Helper.EnumGetValues<Direction_2D_Diagonal>();
                full_L = full_L.Shuffle().CustomToList();

                // All possible Type x Location combinations
                List<TypeLocation> full_TL = new List<TypeLocation>();
                foreach (StimulusType t in full_T)
                    foreach (Direction_2D_Diagonal l in full_L)
                        full_TL.Add(new TypeLocation(t, l));
                full_TL = full_TL.Shuffle().CustomToList();

                // All possible Sprite x Location combinations
                List<SpriteLocation> full_SL = new List<SpriteLocation>();

                // [FAULTY LOGIC (but okay for now)]
                // We got 40 sprites, they represent 1 - percentileBlank, so in total we should 1/ , and percentile_blank of that should be null
                int numNull = Mathf.RoundToInt(full_S.Count / (1 - percentile_Blank) * percentile_Blank);

                foreach (Direction_2D_Diagonal l in full_L)
                {
                    foreach (Sprite s in full_S)
                    {
                        string name = spriteInfo[s].Key;
                        StimulusType type = spriteInfo[s].Value;
                        full_SL.Add(new SpriteLocation(s, l, name, type));
                    }

                    // Add enough options for null (Stim.None)
                    for (int i = 0; i < numNull; i++)
                        full_SL.Add(new SpriteLocation(null, l, StimulusManager.EMPTY_STIM_NAME, StimulusType.None));
                }
                full_SL = full_SL.Shuffle().CustomToList();

                // -- Keep track of what we create

                // Used locations - resets every |Direction_2D_Diagonal|
                List<Direction_2D_Diagonal> used_L = new List<Direction_2D_Diagonal>();
                int reset_L = full_L.Count;

                // Used types - resets every X
                List<StimulusType> used_T = new List<StimulusType>();
                int reset_T = reset_T_Every_Minimum;// full_T_Collection.Count;

                // Used sprites - resets every # sprites
                List<Sprite> used_S = new List<Sprite>();
                int reset_S = full_S.Count;

                // Used Type-Location pairs - resets every # full TL
                List<TypeLocation> used_TL = new List<TypeLocation>();
                int reset_TL = full_TL.Count;

                // Used sprite-location pairs - resets every # full SL
                List<SpriteLocation> used_SL = new List<SpriteLocation>();
                int reset_SL = full_SL.Count;

                // ----- Start creating stuff
                queue.Clear();

                // Create a number of fallbacks
                List<SpriteLocation> options_A = new List<SpriteLocation>();
                List<SpriteLocation> options_B = new List<SpriteLocation>();
                List<SpriteLocation> options_C = new List<SpriteLocation>();
                List<SpriteLocation> options_D = new List<SpriteLocation>();

                string debug = "";

                int times_OptionA = 0;
                int times_OptionB = 0;
                int times_OptionC = 0;
                int times_OptionD = 0;

                while (queue.Count < numWanted)
                {
                    // Map out our options
                    options_A.Clear();
                    options_B.Clear();
                    options_C.Clear();
                    options_D.Clear();

                    if (doDebug) debug = "Queue Item :: {0}"._Format(queue.Count);

                    foreach (SpriteLocation sL in full_SL)
                    {
                        if (doDebug) debug += "\nEvaluating option {0} :: "._Format(sL.ToString());

                        // Do we have more of that type than we want?
                        int numOfType = used_T.FindAll(a => a == sL.type).Count;
                        int wantedNumOfType = wantedTypes[sL.type];
                        if (numOfType >= wantedNumOfType)
                        // if (used_T.Contains(GetType(sL)))
                        {
                            string debug_T = " Skipped, Used Types had {0} {1} (max {2})."._Format(numOfType, sL.type, wantedNumOfType);
                            if (doDebug) debug += debug_T;
                            // Debug.Log(debug_T + "\n{0}"._Format(used_T.ToReadableString()));
                            continue;
                        }
                        // At this point we have the individual constraint C1 (Type) satisfied
                        options_D.Add(sL);
                        if (doDebug) debug += "C1 -> Check (D).";

                        // Trim what we have used so far
                        if (used_L.Contains(sL.direction))
                        {
                            if (doDebug) debug += "Skipped, Used Locations contained {0}."._Format(sL.direction);
                            continue;
                        }
                        if (sL.type != StimulusType.None && used_S.Contains(sL.sprite))
                        {
                            if (doDebug) debug += "Skipped, Used Sprites contained {0}."._Format(sL.sprite);
                            continue;
                        }

                        // At this point, we even have C2 & C3 satisfied (Sprites, Locations)
                        options_C.Add(sL);
                        if (doDebug) debug += " C2, C3 -> Check (C).";


                        if (used_TL.Contains(sL.typeLocation))
                        {
                            if (doDebug) debug += " Skipped, Used Type-Locations contained {0}."._Format(sL.typeLocation);
                            continue;
                        }

                        // At this point, we even have C4 satisfied (Type-Location)
                        options_B.Add(sL);
                        if (doDebug) debug += " C4 -> Check (B).";

                        if (sL.type != StimulusType.None && used_SL.Contains(sL))
                        {
                            if (doDebug) debug += " Skipped, Used Sprite-Locations contained {0}."._Format(sL.ToString());
                            continue;
                        }

                        // At this point, we even have C5 satisfied (Sprite-Location)
                        if (doDebug) debug += " C5 -> Check (A).";

                        // This is perfect, we have 
                        options_A.Add(sL);
                    }

                    if (doDebug) Debug.Log(debug);

                    // Pick one
                    SpriteLocation choice = default(SpriteLocation);

                    // Prime Options
                    if (options_A.Count > 0)
                    {
                        if (doDebug)
                            Debug.Log("Options A ::\n{0}"._Format(options_A.ToReadableString()));

                        choice = options_A.GetRandom();

                        times_OptionA++;

                        int nulls = 0;
                        foreach (SpriteLocation option in options_A)
                            if (option.sprite == null)
                                nulls++;

                        float percentileChanceBlank = nulls / (float)options_A.Count;

                        if (doDebug)
                            Debug.Log("Had a {0} chance of Blank - selected {1}"._Format(
                                percentileChanceBlank.PercentileToPercent(), choice.sprite));
                    }
                    // Fallback 1 :: Options B
                    else if (options_B.Count > 0)
                    {
                        if (doDebug)
                            Debug.LogWarning("No options A! Using Options B ::\n{0}"._Format(options_B.ToReadableString()));
                        choice = options_B.GetRandom();
                        times_OptionB++;
                    }
                    // Fallback 2 :: Options C
                    else if (options_C.Count > 0)
                    {
                        if (doDebug)
                            Debug.LogWarning("No options A or B! Using Options C ::\n{0}"._Format(options_C.ToReadableString()));
                        choice = options_C.GetRandom();
                        times_OptionC++;
                    }
                    // Fallback 3 :: Options D
                    else if (options_D.Count > 0)
                    {
                        if (doDebug)
                            Debug.LogWarning("No options A, B or C! Using Options D ::\n{0}"._Format(options_D.ToReadableString()));
                        choice = options_D.GetRandom();
                        times_OptionD++;
                    }
                    // Deadlock!
                    else
                    {
                        if (doDebug)
                            Debug.LogError("No options, no back-up options either. Shouldn't happen!");
                        return;
                    }

                    queue.Add(choice);
                    if (doDebug)
                        Debug.Log("Picked :: {0}"._Format(choice.ToString()));

                    // Update our logs!
                    used_L.Add(choice.direction);
                    used_T.Add(choice.type);
                    if (choice.type != StimulusType.None) used_S.Add(choice.sprite);
                    used_SL.Add(choice);
                    used_TL.Add(choice.typeLocation);

                    if (doDebug)
                        Debug.Log("Logged {0}, {1}, {2}, {3}, {4}"._Format(
                            choice.direction, choice.type, choice.sprite, choice.ToString(), choice.typeLocation));

                    // See if anything needs to reset
                    if (used_L.Count >= reset_L)
                    {
                        if (doDebug)
                            Debug.Log("Cleared Used Locations");
                        used_L.Clear();
                    }
                    if (used_T.Count >= reset_T)
                    {
                        if (doDebug)
                            Debug.Log("Cleared Used Types");
                        used_T.Clear();
                    }
                    if (used_S.Count >= reset_S)
                    {
                        if (doDebug)
                            Debug.Log("Cleared Used Sprites");
                        used_S.Clear();
                    }
                    if (used_SL.Count >= reset_SL)
                    {
                        if (doDebug)
                            Debug.Log("Cleared Used Sprite-Locations");
                        used_SL.Clear();
                    }
                    if (used_TL.Count >= reset_TL)
                    {
                        if (doDebug)
                            Debug.Log("Cleared Used Type-Locations");
                        used_TL.Clear();
                    }
                }

                // 50% chance to flip the queue
                bool reverseQueue = Utility_Helper.RandomBool();

                if (reverseQueue)
                {
                    if (doDebug)
                        Debug.Log("Flipped queue!");

                    List<SpriteLocation> _queue = new List<SpriteLocation>(queue);
                    int N = _queue.Count;
                    for (int i = 0; i < N; i++)
                        queue[i] = _queue[N - 1 - i];
                }

                // We try to satisfy  those constraints in order
                int times_C5_Satisfied = times_OptionA;
                int times_C4_Satisfied = times_C5_Satisfied + times_OptionB;
                int times_C3_Satisfied = times_C4_Satisfied + times_OptionC;
                int times_C1_C2_Satisfied = times_C3_Satisfied + times_OptionD;

                string finalReport =
                    "Stimulus Queue {0}- Constraints Satisfied :: C1-C2 -> {1}, C3 -> {2}, C4 -> {3}, C5 -> {4}\r\n\r\n{5}"._Format(
                        reverseQueue ? "(Reversed) " : "",
                    times_C1_C2_Satisfied, times_C3_Satisfied, times_C4_Satisfied, times_C5_Satisfied, queue.ToReadableString("Queue Contents", runOnThread));

                ExperimentManagerSession.LogStimulusQueue(fileName, finalReport);
            };

            if (!runOnThread)
            {
                action();
                callback();
                return;
            }

            AsyncThread.RequestRunOnNewThread(() =>
            {
                try
                {
                    action();

                    AsyncThread.RunOnMainThread_ASAP_TS(() =>
                    {
                        Debug.Log("Queue Built in {0}s - check {1}"._Format((TimeWrapper.realtimeSinceStartup_NotTS - timeStarted).ToString("0.0"), fileName));
                        callback();

#if UNITY_EDITOR
                        if (fileName.Contains("game"))
                        {
                            debugSetupQueue_Count--;
                            if (debugSetupQueue_Count > 0)
                                SetupQueue_OLD(queue, numWanted, 
                                    stimuliTexturesFaces, stimuliTexturesObjects,
                                    percentile_Blank, percentile_Faces,
                                    reset_T_Every_Minimum, fileName, callback, runOnThread);
                        }
#endif
                    });
                }
                catch (Exception ex)
                {

                    AsyncThread.RunOnMainThread_ASAP_TS(() =>
                    {
                        Debug.LogException(ex);
                        callback();
                    });
                }
            });
        }

        // All these timings are STIMULI
        public static void CreateTutorialQueue_NEW(List<EventInformation> timings, List<Sprite> stimuliTexturesFaces, List<Sprite> stimuliTexturesObjects, List<SpriteLocation> orderedStimuli_Tutorial)
        {
            orderedStimuli_Tutorial.Clear();

            List<SpriteLocation> probeList_TEMP = new List<SpriteLocation>();
            CreateTutorialQueue_OLD(stimuliTexturesFaces, stimuliTexturesObjects, probeList_TEMP);

            foreach(EventInformation eI in timings)
            {
                // PROBED STIMULUS
                if (eI.triggersEventID >= 0)
                {
                    orderedStimuli_Tutorial.Add(probeList_TEMP[0]);
                    probeList_TEMP.RemoveAt(0);

                    if (probeList_TEMP.Count == 0)
                        break;
                }

                // UNPROBED STIMULUS
                else
                {
                    int draw = Utility_Helper.RandomRange(0, int.MaxValue) % 5;

                    StimulusType sT =
                        draw == 0 ? StimulusType.None :
                        draw == 1 || draw == 2 ? StimulusType.Face : StimulusType.Object;

                    if (sT == StimulusType.None)
                        orderedStimuli_Tutorial.Add(new SpriteLocation(null,
                            Utility_Helper.EnumGetRandom<Direction_2D_Diagonal>(), StimulusManager.EMPTY_STIM_NAME, sT));
                    else
                    {
                        Sprite faceObject =
                            sT == StimulusType.Face ? stimuliTexturesFaces.GetRandom() :
                            sT == StimulusType.Object ? stimuliTexturesObjects.GetRandom() : null;

                        orderedStimuli_Tutorial.Add(new SpriteLocation(faceObject,
                            Utility_Helper.EnumGetRandom<Direction_2D_Diagonal>(), faceObject.name, sT));
                    }
                }
            }
        }

        public static void CreateTutorialQueue_OLD(List<Sprite> stimuliTexturesFaces, List<Sprite> stimuliTexturesObjects, List<SpriteLocation> orderedStimuli_Tutorial)
        {
            orderedStimuli_Tutorial.Clear();

            // 4 Blanks across all directions
            foreach (Direction_2D_Diagonal direction in Enum.GetValues(typeof(Direction_2D_Diagonal)))
            {
                SpriteLocation blankStim = new SpriteLocation(null, direction, StimulusManager.EMPTY_STIM_NAME, StimulusType.None);
                orderedStimuli_Tutorial.Add(blankStim);
            }

            // 2 faces + 2 objects across all directions
            List<Direction_2D_Diagonal> allDirections = Utility_Helper.EnumGetValues<Direction_2D_Diagonal>().Shuffle().ToList();
            for (int i = 0; i < allDirections.Count; i++)
            {
                StimulusType sT = (i < 2) ? StimulusType.Face : StimulusType.Object;
                Sprite faceObject = sT == StimulusType.Face ? stimuliTexturesFaces.GetRandom() : stimuliTexturesObjects.GetRandom();

                SpriteLocation faceObjectStim = new SpriteLocation(faceObject, allDirections[i], faceObject.name, sT);
                orderedStimuli_Tutorial.Add(faceObjectStim);
            }

            orderedStimuli_Tutorial = orderedStimuli_Tutorial.Shuffle().ToList();
        }
    }

    public struct QueueScore
    {
        public float replaySingle;
        public float runReplay;
        public float runUnprobed;
        public float runProbed;
        public float runTotal;
        public float worldUnprobed;
        public float worldProbed;
        public float worldTotal;
        public float twoWorldReplay;
        public float twoWorldUnprobed;
        public float twoWorldProbed;
        public float twoWorldTotal;
        public float allReplay;
        public float allUnprobed;
        public float allProbed;
        public float allTotal;

        public static readonly QueueScore DEFAULT = new QueueScore(0);

        public const string HEADERS_CSV = "replaySingle;replayPair;runUnprobed;runProbed;runTotal;worldUnprobed;worldProbed;worldTotal;twoWorldReplay;twoWorldUnprobed;twoWorldProbed;twoWorldTotal;allReplay;allUnprobed;allProbed;allTotal";

        public string ToCSV()
        {
            return "{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};{13};{14};{15}"._Format(
                replaySingle,
                runReplay,
                runUnprobed,
                runProbed,
                runTotal,
                worldUnprobed,
                worldProbed,
                worldTotal,
                twoWorldReplay,
                twoWorldUnprobed,
                twoWorldProbed,
                twoWorldTotal,
                allReplay,
                allUnprobed,
                allProbed,
                allTotal);
        }

        public static QueueScore operator +(QueueScore a, QueueScore b)
        {
            QueueScore sum = DEFAULT;

            sum.replaySingle = a.replaySingle + b.replaySingle;
            sum.runReplay = a.runReplay + b.runReplay;
            sum.runUnprobed = a.runUnprobed + b.runUnprobed;
            sum.runProbed = a.runProbed + b.runProbed;
            sum.runTotal = a.runTotal + b.runTotal;
            sum.worldUnprobed = a.worldUnprobed + b.worldUnprobed;
            sum.worldProbed = a.worldProbed + b.worldProbed;
            sum.worldTotal = a.worldTotal + b.worldTotal;
            sum.twoWorldReplay = a.twoWorldReplay + b.twoWorldReplay;
            sum.twoWorldUnprobed = a.twoWorldUnprobed + b.twoWorldUnprobed;
            sum.twoWorldProbed = a.twoWorldProbed + b.twoWorldProbed;
            sum.twoWorldTotal = a.twoWorldTotal + b.twoWorldTotal;
            sum.allReplay = a.allReplay + b.allReplay;
            sum.allUnprobed = a.allUnprobed + b.allUnprobed;
            sum.allProbed = a.allProbed + b.allProbed;
            sum.allTotal = a.allTotal + b.allTotal;

            return sum;
        }

        public static QueueScore operator /(QueueScore a, int b)
        {
            QueueScore res = a;

            res.replaySingle /= b;
            res.runReplay /= b;
            res.runUnprobed /= b;
            res.runProbed /= b;
            res.runTotal /= b;
            res.worldUnprobed /= b;
            res.worldProbed /= b;
            res.worldTotal /= b;
            res.twoWorldReplay /= b;
            res.twoWorldUnprobed /= b;
            res.twoWorldProbed /= b;
            res.twoWorldTotal /= b;
            res.allReplay /= b;
            res.allUnprobed /= b;
            res.allProbed /= b;
            res.allTotal /= b;

            return res;
        }

        private QueueScore(int sth)
        {
            replaySingle = 0;
            runReplay = 0;
            runUnprobed = 0;
            runProbed = 0;
            runTotal = 0;
            worldUnprobed = 0;
            worldProbed = 0;
            worldTotal = 0;
            twoWorldReplay = 0;
            twoWorldUnprobed = 0;
            twoWorldProbed = 0;
            twoWorldTotal = 0;
            allReplay = 0;
            allUnprobed = 0;
            allProbed = 0;
            allTotal = 0;
        }
    }




    #region Even - Odd
#if false
    
        private static Dictionary<int, Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>> RUN_TYPE_DIRECTION_TO_SET_PROBED_EVEN = new Dictionary<int, Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>>()
        {
            // == RUN 0 | DONE
            { 0, new Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>()
            {
                // == FACES
                { StimulusType.Face, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 2, 4, 9 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 8, 6} },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 3, 1 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 7, 0, 5 } },
                } },
                
                // == OBJECTS
                { StimulusType.Object, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 2, 0 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 6, 8, 5} },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 1, 3 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 7, 4, 9 } },
                } },
                
                // == BLANK
                { StimulusType.None, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { -1,  } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { -1,  } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { -1, -1 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { -1,  } },
                } },
            } },

            // == RUN 1 | DONE
            { 1, new Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>()
            {
                // == FACES
                { StimulusType.Face, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 0, 1 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 2, 3, 4 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 5, 6, 7 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 8, 9 } },
                } },
                
                // == OBJECTS
                { StimulusType.Object, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 9, 8, 7 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 6, 5} },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 4, 3 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 2, 1, 0 } },
                } },
                
                // == BLANK
                { StimulusType.None, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { -1,  } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { -1,  } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { -1,  } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { -1, -1 } },
                } },
            } },

            // == RUN 2
            { 2, new Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>()
            {
                // == FACES
                { StimulusType.Face, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 2, 4, 9 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 8, 6} },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 3, 1 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 7, 0, 5 } },
                } },
                
                // == OBJECTS
                { StimulusType.Object, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 2, 0 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 6, 8, 5} },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 1, 3 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 7, 4, 9 } },
                } },
                
                // == BLANK
                { StimulusType.None, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { -1,  } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { -1,  } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { -1, -1 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { -1,  } },
                } },
            } },

            // == RUN 3
            { 3, new Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>()
            {
                // == FACES
                { StimulusType.Face, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 0, 1 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 2, 3, 4 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 5, 6, 7 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 8, 9 } },
                } },
                
                // == OBJECTS
                { StimulusType.Object, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 9, 8, 7 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 6, 5} },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 4, 3 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 2, 1, 0 } },
                } },
                
                // == BLANK
                { StimulusType.None, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { -1,  } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { -1,  } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { -1,  } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { -1, -1 } },
                } },
            } },
        };


        private static Dictionary<int, Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>> RUN_TYPE_DIRECTION_TO_SET_PROBED_ODD = new Dictionary<int, Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>>()
        {
            // == RUN 0 | DONE
            { 0, new Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>()
            {
                // == FACES
                { StimulusType.Face, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 0, 3 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 7, 4, 9 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 1, 6, 5 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 2, 8 } },
                } },
                
                // == OBJECTS
                { StimulusType.Object, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 0, 4, 5 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 7, 3} },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 6, 1 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 2, 8, 9 } },
                } },
                
                // == BLANK
                { StimulusType.None, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { -1,  } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { -1,  } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { -1,  } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { -1, -1 } },
                } },
            } },

            // == RUN 1 | DONE
            { 1, new Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>()
            {
                // == FACES
                { StimulusType.Face, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 0, 2, 4 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 6, 8 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 1, 3 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 5, 7, 9 } },
                } },
                
                // == OBJECTS
                { StimulusType.Object, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 8, 6 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 4, 2, 0} },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 9, 7, 5 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 3, 1 } },
                } },
                
                // == BLANK
                { StimulusType.None, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { -1,  } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { -1,  } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { -1, -1 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { -1,  } },
                } },
            } },

            // == RUN 2
            { 2, new Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>()
            {
                // == FACES
                { StimulusType.Face, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 2, 4, 9 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 8, 6} },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 3, 1 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 7, 0, 5 } },
                } },
                
                // == OBJECTS
                { StimulusType.Object, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 2, 0 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 6, 8, 5} },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 1, 3 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 7, 4, 9 } },
                } },
                
                // == BLANK
                { StimulusType.None, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { -1,  } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { -1,  } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { -1, -1 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { -1,  } },
                } },
            } },

            // == RUN 3
            { 3, new Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>()
            {
                // == FACES
                { StimulusType.Face, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 0, 1 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 2, 3, 4 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 5, 6, 7 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 8, 9 } },
                } },
                
                // == OBJECTS
                { StimulusType.Object, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 9, 8, 7 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 6, 5} },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 4, 3 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 2, 1, 0 } },
                } },
                
                // == BLANK
                { StimulusType.None, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { -1,  } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { -1,  } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { -1,  } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { -1, -1 } },
                } },
            } },
        };


        private static Dictionary<int, Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>> RUN_TYPE_DIRECTION_TO_SET_UNPROBED_EVEN = new Dictionary<int, Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>>()
        {
            // == RUN 0 | DONE
            { 0, new Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>()
            {
                // == FACES
                { StimulusType.Face, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 6, 7, 0, 4, 8 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 1, 5, 2, 7, 8} },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 9, 5, 1, 2, 3 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 0, 4, 3, 9, 6 } },
                } },
                
                // == OBJECTS
                { StimulusType.Object, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 0, 4, 8, 6, 7 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 2, 1, 5, 7, 8 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 5, 9, 1, 2, 3 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 3, 9, 0, 4, 6 } },
                } },
                
                // == BLANK
                { StimulusType.None, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { -1, -1, -1 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { -1, -1 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { -1, -1 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { -1, -1, -1 } },
                } },
            } },

            // == RUN 1 | DONE
            { 1, new Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>()
            {
                // == FACES
                { StimulusType.Face, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 0, 1, 2, 3, 4 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 1, 3, 5, 7, 9} },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 5, 6, 7, 8, 9 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 0, 2, 4, 6, 8 } },
                } },
                
                // == OBJECTS
                { StimulusType.Object, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 9, 8, 7, 6, 5 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 1, 4, 7, 0, 3 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 4, 3, 2, 1, 0 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 2, 5, 6, 8, 9 } },
                } },
                
                // == BLANK
                { StimulusType.None, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { -1, -1 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { -1, -1, -1 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { -1, -1, -1 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { -1, -1 } },
                } },
            } },
            
            // == RUN 2
            { 2, new Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>()
            {
                // == FACES
                { StimulusType.Face, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 6, 7, 0, 4, 8 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 1, 5, 2, 7, 8} },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 9, 5, 1, 2, 3 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 0, 4, 3, 9, 6 } },
                } },
                
                // == OBJECTS
                { StimulusType.Object, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 0, 4, 8, 6, 7 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 2, 1, 5, 7, 8 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 5, 9, 1, 2, 3 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 3, 9, 0, 4, 6 } },
                } },
                
                // == BLANK
                { StimulusType.None, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { -1, -1, -1 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { -1, -1 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { -1, -1 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { -1, -1, -1 } },
                } },
            } },

            // == RUN 3
            { 3, new Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>()
            {
                // == FACES
                { StimulusType.Face, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 0, 1, 2, 3, 4 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 1, 3, 5, 7, 9} },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 5, 6, 7, 8, 9 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 0, 2, 4, 6, 8 } },
                } },
                
                // == OBJECTS
                { StimulusType.Object, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 9, 8, 7, 6, 5 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 1, 4, 7, 0, 3 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 4, 3, 2, 1, 0 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 2, 5, 6, 8, 9 } },
                } },
                
                // == BLANK
                { StimulusType.None, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { -1, -1 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { -1, -1, -1 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { -1, -1, -1 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { -1, -1 } },
                } },
            } },
        };


        private static Dictionary<int, Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>> RUN_TYPE_DIRECTION_TO_SET_UNPROBED_ODD = new Dictionary<int, Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>>()
        {
            // == RUN 0 | DONE
            { 0, new Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>()
            {
                // == FACES
                { StimulusType.Face, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 3, 5, 4, 8, 6 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 6, 1, 3, 2, 7 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 7, 9, 2, 0, 1 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 0, 5, 9, 4, 8 } },
                } },
                
                // == OBJECTS
                { StimulusType.Object, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 4, 8, 3, 5, 0 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 1, 3, 4, 6, 7 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 2, 7, 9, 1, 6 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 5, 9, 0, 2, 8 } },
                } },
                
                // == BLANK
                { StimulusType.None, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { -1, -1 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { -1, -1, -1 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { -1, -1, -1 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { -1, -1 } },
                } },
            } },

            // == RUN 1 | DONE
            { 1, new Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>()
            {
                // == FACES
                { StimulusType.Face, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 1, 5, 7, 6, 9 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 4, 8, 3, 1, 2 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 2, 8, 3, 4, 0 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 7, 6, 9, 5, 0 } },
                } },
                
                // == OBJECTS
                { StimulusType.Object, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 0, 8, 5, 7, 4 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 8, 3, 4, 9, 1 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 3, 9, 6, 1, 2 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 0, 2, 7, 5, 6 } },
                } },
                
                // == BLANK
                { StimulusType.None, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { -1, -1, -1 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { -1, -1 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { -1, -1 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { -1, -1, -1 } },
                } },
            } },
            
            // == RUN 2
            { 2, new Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>()
            {
                // == FACES
                { StimulusType.Face, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 6, 7, 0, 4, 8 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 1, 5, 2, 7, 8} },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 9, 5, 1, 2, 3 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 0, 4, 3, 9, 6 } },
                } },
                
                // == OBJECTS
                { StimulusType.Object, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 0, 4, 8, 6, 7 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 2, 1, 5, 7, 8 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 5, 9, 1, 2, 3 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 3, 9, 0, 4, 6 } },
                } },
                
                // == BLANK
                { StimulusType.None, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { -1, -1, -1 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { -1, -1 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { -1, -1 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { -1, -1, -1 } },
                } },
            } },

            // == RUN 3
            { 3, new Dictionary<StimulusType, Dictionary<Direction_2D_Diagonal, List<int>>>()
            {
                // == FACES
                { StimulusType.Face, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 0, 1, 2, 3, 4 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 1, 3, 5, 7, 9} },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 5, 6, 7, 8, 9 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 0, 2, 4, 6, 8 } },
                } },
                
                // == OBJECTS
                { StimulusType.Object, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { 9, 8, 7, 6, 5 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { 1, 4, 7, 0, 3 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { 4, 3, 2, 1, 0 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { 2, 5, 6, 8, 9 } },
                } },
                
                // == BLANK
                { StimulusType.None, new Dictionary<Direction_2D_Diagonal, List<int>>()
                {
                    { Direction_2D_Diagonal.BottomLeft,     new List<int>() { -1, -1 } },
                    { Direction_2D_Diagonal.BottomRight,    new List<int>() { -1, -1, -1 } },
                    { Direction_2D_Diagonal.TopLeft,        new List<int>() { -1, -1, -1 } },
                    { Direction_2D_Diagonal.TopRight,       new List<int>() { -1, -1 } },
                } },
            } },
        };

        // EVEN SUBJECTS (Version 0)
        private static readonly List<Dictionary<int, List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>>> REPLAY_ID_TO_TRIAL_AND_DIRECTION_EVEN = new List<Dictionary<int, List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>>>()
        {
            // REPLAY 0 WORLD 0 | DONE
            new Dictionary<int, List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>>()
            {

                { 0,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Face),
                    }
                },
                { 1,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Face),
                    }
                },
                { 2,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Object),
                    }
                },
                { 3,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Object),
                    }
                },
                { 4,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Face),
                    }
                },
                { 5,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Face),
                    }
                },
                { 6,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomRight, StimulusType.Object),
                    }
                },
                { 7,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopRight, StimulusType.Object),
                    }
                },
                { 8,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomRight, StimulusType.Face),
                    }
                },
                { 9,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Object),
                    }
                },
                { -1,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopLeft, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.None), // false
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomRight, StimulusType.None), // true
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.None),
                        // new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, BLANK_PAIR_OVER_SINGE ? Direction_2D_Diagonal.BottomRight : Direction_2D_Diagonal.BottomLeft, StimulusType.None),
                    }
                },
            },

            // REPLAY 1 WORLD 0 | DONE
            new Dictionary<int, List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>>()
            {

                { 0,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Object),
                    }
                },
                { 1,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Object),
                    }
                },
                { 2,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Face),
                    }
                },
                { 3,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Face),
                    }
                },
                { 4,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Object),
                    }
                },
                { 5,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Object),
                    }
                },
                { 6,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomRight, StimulusType.Face),
                    }
                },
                { 7,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopRight, StimulusType.Face),
                    }
                },
                { 8,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomRight, StimulusType.Object),
                    }
                },
                { 9,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Face),
                    }
                },
                { -1,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopLeft, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomRight, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.None),
                    }
                },
            },
            
            // REPLAY 0 WORLD 1 | DONE
            new Dictionary<int, List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>>()
            {
                { 0,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomRight, StimulusType.Object),
                    }
                },
                { 1,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Object),
                    }
                },
                { 2,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Face),
                    }
                },
                { 3,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Face),
                    }
                },
                { 4,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Object),
                    }
                },
                { 5,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Object),
                    }
                },
                { 6,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopRight, StimulusType.Face),
                    }
                },
                { 7,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Face),
                    }
                },
                { 8,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Object),
                    }
                },
                { 9,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomRight, StimulusType.Face),
                    }
                },
                { -1,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(null, Direction_2D_Diagonal.TopLeft, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomLeft, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(null, Direction_2D_Diagonal.TopRight, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomRight, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.None),
                    }
                },
            },

            // REPLAY 1 WORLD 1 | DONE
            new Dictionary<int, List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>>()
            {
                { 0,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomRight, StimulusType.Face),
                    }
                },
                { 1,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Face),
                    }
                },
                { 2,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Object),
                    }
                },
                { 3,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Object),
                    }
                },
                { 4,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Face),
                    }
                },
                { 5,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Face),
                    }
                },
                { 6,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopRight, StimulusType.Object),
                    }
                },
                { 7,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Object),
                    }
                },
                { 8,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Face),
                    }
                },
                { 9,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomRight, StimulusType.Object),
                    }
                },
                { -1,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopLeft, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(null, Direction_2D_Diagonal.TopRight, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(null, Direction_2D_Diagonal.TopRight, StimulusType.None),
                    }
                },
            },
        };

        // ODD SUBJECTS (Version 1)
        private static readonly List<Dictionary<int, List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>>> REPLAY_ID_TO_TRIAL_AND_DIRECTION_ODD = new List<Dictionary<int, List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>>>()
        {
            // REPLAY 0 WORLD 0 | DONE
            new Dictionary<int, List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>>()
            {

                { 0,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Face),
                    }
                },
                { 1,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Object),
                    }
                },
                { 2,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopRight, StimulusType.Face),
                    }
                },
                { 3,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Object),
                    }
                },
                { 4,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Face),
                    }
                },
                { 5,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Object),
                    }
                },
                { 6,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Face),
                    }
                },
                { 7,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomRight, StimulusType.Object),
                    }
                },
                { 8,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopRight, StimulusType.Face),
                    }
                },
                { 9,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Object),
                    }
                },
                { -1,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopLeft, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.None), // false
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomRight, StimulusType.None), // true
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.None),
                        // new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, BLANK_PAIR_OVER_SINGE ? Direction_2D_Diagonal.BottomRight : Direction_2D_Diagonal.BottomLeft, StimulusType.None),
                    }
                },
            },

            // REPLAY 1 WORLD 0 | DONE
            new Dictionary<int, List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>>()
            {

                { 0,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Object),
                    }
                },
                { 1,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Face),
                    }
                },
                { 2,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopRight, StimulusType.Object),
                    }
                },
                { 3,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Face),
                    }
                },
                { 4,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Object),
                    }
                },
                { 5,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Face),
                    }
                },
                { 6,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Object),
                    }
                },
                { 7,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomRight, StimulusType.Face),
                    }
                },
                { 8,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopRight, StimulusType.Object),
                    }
                },
                { 9,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Face),
                    }
                },
                { -1,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomLeft, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopRight, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.None),
                    }
                },
            },
            
            // REPLAY 0 WORLD 1
            new Dictionary<int, List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>>()
            {
                { 0,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomRight, StimulusType.Object),
                    }
                },
                { 1,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Object),
                    }
                },
                { 2,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Face),
                    }
                },
                { 3,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Face),
                    }
                },
                { 4,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Object),
                    }
                },
                { 5,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Object),
                    }
                },
                { 6,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopRight, StimulusType.Face),
                    }
                },
                { 7,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Face),
                    }
                },
                { 8,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Object),
                    }
                },
                { 9,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomRight, StimulusType.Face),
                    }
                },
                { -1,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(null, Direction_2D_Diagonal.TopLeft, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomLeft, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(null, Direction_2D_Diagonal.TopRight, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomRight, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.None),
                    }
                },
            },

            // REPLAY 1 WORLD 1
            new Dictionary<int, List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>>()
            {
                { 0,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomRight, StimulusType.Face),
                    }
                },
                { 1,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Face),
                    }
                },
                { 2,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Object),
                    }
                },
                { 3,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Object),
                    }
                },
                { 4,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Face),
                    }
                },
                { 5,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Face),
                    }
                },
                { 6,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopRight, StimulusType.Object),
                    }
                },
                { 7,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.Object),
                    }
                },
                { 8,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopLeft, StimulusType.Object),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.TopRight, StimulusType.Face),
                    }
                },
                { 9,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.Face),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.BottomRight, StimulusType.Object),
                    }
                },
                { -1,
                    new List<Triplet<bool?, Direction_2D_Diagonal, StimulusType>>()
                    {
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(true, Direction_2D_Diagonal.TopLeft, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomLeft, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(null, Direction_2D_Diagonal.TopRight, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(false, Direction_2D_Diagonal.BottomRight, StimulusType.None),
                        new Triplet<bool?, Direction_2D_Diagonal, StimulusType>(null, Direction_2D_Diagonal.TopRight, StimulusType.None),
                    }
                },
            },
        };

#endif
    #endregion

}