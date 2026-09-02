/// NS_REMOVE | <see cref="Peripherals.Logging"/> 
using System.IO;

// NS_DEBATABLE | It's core but it's the analyzer
using Game.Core;
// NS_DEBATABLE | It's core but it's the analyzer
using Peripherals.UserInput.Core;
// NS_DEBATABLE | Maybe these should be Core (but also then debate whether to add here)
using Experiment.Stimulus;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using TGP.Helpers;
using UnityEngine;
using Peripherals.EyeTracking;
using Experiment.Triggers;

namespace Experiment.Analyzer.Core
{
    /// <summary>
    /// [SEGMENT] ie. _detals and _analyses separate
    /// </summary>
    [System.Serializable]
    public enum FullLogEntryType
    {
        Essense,
        Input,
        Trigger,
        EyeTracker_Gaze,
        EyeTracker_Sacada,
        EyeTracker_Blink,
        Background,
        Stimulus,
        Collision,
        FrameRendered,
        Player,
        GameStart,
        Cumulative
    }

    public static class CumulativeAnalysisHelper
    {
        public static string ListToCsv(this IEnumerable<CumulativeAnalysis> list, string separator, string nullFieldValue, bool printHeader)
        {
            System.Text.StringBuilder csvdata = new System.Text.StringBuilder();
            int index = 0;

            foreach (var o in list)
            {
                csvdata.Append(o._ToCsv(separator, nullFieldValue, printHeader && index == 0));
                index++;
            }

            return csvdata.ToString();
        }
    }


    /// <summary>
    /// Each object of this class corresponds to a single stimulus OR trigger (ie. a single line)
    /// FINAL PRINT TO CUMULATIVE ANALYSIS CSV
    /// </summary>
    public class CumulativeAnalysis : IComparable<CumulativeAnalysis>
    {
        public double timeMS;
        public double timeMS_NoPauses;

        public CumulativeAnalysis(StimulusAnalysis stimulusAnalysis)
        {
            this.stimulusAnalysis = stimulusAnalysis;
            triggerAnalysis = null;

            timeMS = stimulusAnalysis.timeMS;
            timeMS_NoPauses = stimulusAnalysis.timeMS_NoPauses;
        }

        public CumulativeAnalysis(TriggerAnalysis triggerAnalysis)
        {
            stimulusAnalysis = null;
            this.triggerAnalysis = triggerAnalysis;

            timeMS = triggerAnalysis.timeMS;
            timeMS_NoPauses = triggerAnalysis.timeMS_NoPauses;
        }

        public StimulusAnalysis stimulusAnalysis;
        public TriggerAnalysis triggerAnalysis;

        public static List<CumulativeAnalysis> MergeAnalyses(List<StimulusAnalysis> stimulusAnalyses, List<TriggerAnalysis> triggerAnalyses)
        {
            List<CumulativeAnalysis> cumulativeAnalyses = new List<CumulativeAnalysis>();

            foreach (StimulusAnalysis sA in stimulusAnalyses)
                cumulativeAnalyses.Add(new CumulativeAnalysis(sA));

            foreach (TriggerAnalysis tA in triggerAnalyses)
                cumulativeAnalyses.Add(new CumulativeAnalysis(tA));

            cumulativeAnalyses.Sort();

            return cumulativeAnalyses;
        }

        public int CompareTo(CumulativeAnalysis other)
        {
            return timeMS.CompareTo(other.timeMS);
        }

        public string _ToCsv(string separator, string nullFieldValue, bool printHeader)
        {
            System.Text.StringBuilder csvdata = new System.Text.StringBuilder();

            List<FieldInfo> fields = new List<FieldInfo>();

            Type stimulusType = typeof(StimulusAnalysis);
            Type triggerType = typeof(TriggerAnalysis);

            fields.AddRange(stimulusType.GetFields());
            fields.AddRange(triggerType.GetFields());

            fields = fields.RemoveDuplicates(a => a.Name).ToList();

            if (printHeader)
            {
                string header = String.Join(separator, fields.Select(f => f.Name).ToArray());
                csvdata.AppendLine(header);
            }

            csvdata.AppendLine(stimulusAnalysis != null ?
                CSVSerialization.ToCsvFields(separator, nullFieldValue, fields, stimulusAnalysis) :
                CSVSerialization.ToCsvFields(separator, nullFieldValue, fields, triggerAnalysis));

            return csvdata.ToString();
        }
    }

    /// <summary>
    /// Each object of this class corresponds to a single stimulus (ie. a single line)
    /// FINAL PRINT TO STIMULUS ANALYSIS CSV
    /// </summary>
    [System.Serializable]
    public class StimulusAnalysis : IComparable<StimulusAnalysis>
    {
        public StimulusAnalysis(ParseLogEntry_Stimulus stimulus, string subjectID)
        {
            this.subjectID = subjectID;

            versionString = stimulus.version.versionString;
            versionDate = stimulus.version.versionDate.ToString("yyyy-MM-dd");
            versionSettings = stimulus.version.versionSettings.ToString();

            timeMS = stimulus.timeMS;
            timeMS_NoPauses = stimulus.timeMS_NoPauses;
            // Debug.LogError("STIM.MS {0} / STRING {1} -> ANALYSIS.MS {2}"._Format(stimulus.timeMS, stimulus.timeMS_Str, timeMS));

            stimID = stimulus.stimID;
            stimType = stimulus.stimType.ToString();
            stimName = stimulus.stimName;
            stimDirection = stimulus.direction.ToString();
            isProbed = stimulus.isProbe;
            offsetTS = stimulus.offsetTS;
            offsetTS_NoPauses = stimulus.offsetTS_NoPauses;
            probeTS = stimulus.probeTS;
            probeTS_NoPauses = stimulus.probeTS_NoPauses;
            response = stimulus.response;
            responseEvaluation = stimulus.responseEvaluation;
            responseTS = stimulus.responseTS;
            responseTS_NoPauses = stimulus.responseTS_NoPauses;
            onset_difficulty = stimulus.difficulty;
            onset_averageDifficulty = stimulus.averageDifficulty;
            response_difficulty = stimulus.response_Difficulty;
            response_averageDifficulty = stimulus.response_AvgDifficulty;

            world = stimulus.world;
            level = stimulus.level;
            fullLogState = stimulus.fullLogState;
            indexWithinFullLogs = stimulus.indexWithinFullLogs + 1;
            probeIndexWithinDetails = stimulus.detailIndexWithinFile + 1;
        }

        // Default comparer for Part type.
        public int CompareTo(StimulusAnalysis other)
        {
            // A null value means that this object is greater.
            if (other == null)
                return 1;

            else
                return timeMS.CompareTo(other.timeMS);
        }

        public string subjectID;

        public string versionString;
        public string versionDate;
        public string versionSettings;
        public double timeMS = 0;
        public double timeMS_NoPauses = 0;

        public string world = "";
        public string level = "";

        public string fullLogState = "";
        public int indexWithinFullLogs = -1;

        public string stimID = "-1";
        public string stimType = "";
        public string stimName = "";
        public string stimDirection = "";
        public int probeIndexWithinDetails = -1;
        public bool isProbed;
        public double offsetTS = -1;
        public double offsetTS_NoPauses = -1;
        public double probeTS = -1;
        public double probeTS_NoPauses = -1;
        public string response;
        public string responseEvaluation;
        public double responseTS = -1;
        public double responseTS_NoPauses = -1;
        public double onset_difficulty = -1;
        public double onset_averageDifficulty = -1;
        public double response_difficulty = -1;
        public double response_averageDifficulty = -1;

        // [SOS] all defined w.r.t. stim onset

        // 2. Number of falling essences on screen
        public float avgNumFallingEssenses_m1000_p500 = 0;
        public float avgNumFallingEssenses_m1000_0 = 0;
        public float avgNumFallingEssenses_0_p500 = 0;

        // 3. Number of opposite color essences on screen
        public float avgNumOppositeColorEssenses_m1000_p500 = 0;
        public float avgNumOppositeColorEssenses_m1000_0 = 0;
        public float avgNumOppositeColorEssenses_0_p500 = 0;

        // 4. Number of same color essences on screen
        public float avgNumSameColorEssenses_m1000_p500 = 0;
        public float avgNumSameColorEssenses_m1000_0 = 0;
        public float avgNumSameColorEssenses_0_p500 = 0;

        // 5. Number of falling essences on top half of screen
        public int numFallingEssensesTopHalf_m400 = 0;
        public int numFallingEssensesTopHalf_m200 = 0;
        public int numFallingEssensesTopHalf_0 = 0;
        public int numFallingEssensesTopHalf_p200 = 0;
        public int numFallingEssensesTopHalf_p400 = 0;

        // 6. Number of falling essences on bottom half of screen
        public int numFallingEssensesBotHalf_m400 = 0;
        public int numFallingEssensesBotHalf_m200 = 0;
        public int numFallingEssensesBotHalf_0 = 0;
        public int numFallingEssensesBotHalf_p200 = 0;
        public int numFallingEssensesBotHalf_p400 = 0;

        // 7. Average speed of falling essences
        public float averageSpeedFallingEssenses_OverFrames_m1000_p500 = 0;
        public float averageSpeedFallingEssenses_OverEssences_m1000_p500 = 0;

        // 8. Number of button-presses
        public int numButtonPresses_m1000_p500 = 0;
        public int numButtonPresses_m1000_0 = 0;
        public int numButtonPresses_0_p500 = 0;

        // 9. Temporal proximity of pre-stim button-press
        public double tempProxButtonPressPreStim = 0;

        // 10. Temporal proximity of post-stim button-press
        public double tempProxButtonPressPostStim = 0;

        // 11. Spatial proximity from stim location to nearest essence
        public float spatialProxFromStimLocToNearestEssense_m400 = 0;
        public float spatialProxFromStimLocToNearestEssense_m200 = 0;
        public float spatialProxFromStimLocToNearestEssense_0 = 0;
        public float spatialProxFromStimLocToNearestEssense_p200 = 0;
        public float spatialProxFromStimLocToNearestEssense_p400 = 0;

        // 12. Spatial proximity from stim location to cluster mean of all essences
        public float spatialProxFromStimLocToClusterMeanEssenses_m400 = 0;
        public float spatialProxFromStimLocToClusterMeanEssenses_m200 = 0;
        public float spatialProxFromStimLocToClusterMeanEssenses_0 = 0;
        public float spatialProxFromStimLocToClusterMeanEssenses_p200 = 0;
        public float spatialProxFromStimLocToClusterMeanEssenses_p400 = 0;

        // 13. Temporal proximity of last collision with opposite color essence
        public double tempProxOfLastCollision = 0;

        // 14. Temporal proximity of last absorption of same color essence
        public double tempProxOfLastAbsorption = 0;

        // 15. Number of collisions with opposite color essence
        public int numOfCollissions_m2000_p500 = 0;

        // 16. Number of absorptions of same color essence
        public int numOfAbsorptions_m2000_p500 = 0;

        // 17. Spatial proximity of character being controlled and stim location
        public float spatialProxFromStimLocToPlayer_m400 = 0;
        public float spatialProxFromStimLocToPlayer_m200 = 0;
        public float spatialProxFromStimLocToPlayer_0 = 0;
        public float spatialProxFromStimLocToPlayer_p200 = 0;
        public float spatialProxFromStimLocToPlayer_p400 = 0;

        /// <summary>
        /// 2.1 # Sacades within a range of time from stimulus onset
        /// </summary>
        public int numSaccades_m2000_p500 = 0;

        /// <summary>
        /// 2.2 Temporal proximity of latest sacada pre-stimulus onset
        /// </summary>
        public double tempProxOfLastSacada = 0;

        /// <summary>
        /// 2.3 Spatial proximity of last sacada
        /// </summary>
        public float spatialProxFromStimLocToLastSacadaLine = 0;

        /// <summary>
        /// 2.4 Average gaze over a range of time
        /// </summary>
        public Vector2 averageGaze_m2000_p500 = Vector2.zero;

        /// <summary>
        /// 2.5 Temporal proximity of last gaze -> Gaze constantly updates!
        /// </summary>
        // public double tempProxOfLastGaze = 0;

        /// <summary>
        /// 2.5 # Blinks within a range of time from stimulus onset
        /// </summary>
        public int numBlinks_m2000_p500 = 0;

        /// <summary>
        /// 2.6 Temporal proximity of latest blink
        /// </summary>
        public double tempProxOfLastBlink = 0;
    }

    /// <summary>
    /// Each object of this class corresponds to a single trigger (ie. a single line)
    /// FINAL PRINT TO TRIGGER ANALYSIS CSV
    /// </summary>
    [System.Serializable]
    public class TriggerAnalysis : IComparable<TriggerAnalysis>
    {
        public TriggerAnalysis(ParseLogEntry_Trigger trigger, string subjectID)
        {
            this.subjectID = subjectID;

            versionString = trigger.version.versionString;
            versionDate = trigger.version.versionDate.ToString("yyyy-MM-dd");
            versionSettings = trigger.version.versionSettings.ToString();

            timeMS = trigger.timeMS;
            timeMS_NoPauses = trigger.timeMS_NoPauses;
            world = trigger.world;
            level = trigger.level;
            fullLogState = trigger.fullLogState;
            indexWithinFullLogs = trigger.indexWithinFullLogs + 1;
            // Debug.LogError("TRIGGER.MS {0} / STRING {1} -> ANALYSIS.MS {2}"._Format(trigger.timeMS, trigger.timeMS_Str, timeMS));

            sender = trigger.sender;
            triggerState = trigger.triggerState;
            triggerEvent = trigger.triggerEvent;
            triggerCode = trigger.triggerCode;
        }

        // Default comparer for Part type.
        public int CompareTo(TriggerAnalysis other)
        {
            // A null value means that this object is greater.
            if (other == null)
                return 1;

            else
                return timeMS.CompareTo(other.timeMS);
        }

        public string subjectID;

        public string versionString;
        public string versionDate;
        public string versionSettings;
        public double timeMS = 0;
        public double timeMS_NoPauses = 0;

        public string world = "";
        public string level = "";

        // public string fullLogFileName = "";
        public string fullLogState = "";
        public int indexWithinFullLogs = -1;

        public string sender;
        public string triggerState;
        public string triggerEvent;
        public string triggerCode;
    }

    // Extract data from ParseFullFile
    public static class ParseFullFileAnalysis
    {
        private static double GetClosestFrameTime(this ParseFullFile data, double timeMS)
        {
            // They're NOT already ordered!
            if (data.frameRendered.Count == 0) return -1;

            ParseLogEntry_FrameRendered closestFrame = null;

            if (data.frameRendered.Count == 1)
                closestFrame = data.frameRendered[0];
            else
                closestFrame = data.frameRendered.Aggregate((x, y) => Math.Abs(x.timeMS - timeMS) < Math.Abs(y.timeMS - timeMS) ? x : y);

            if (closestFrame == null)
            {
                Debug.LogError("Closest frame shouldnt be null at this point");
                return -1;
            }

            return closestFrame.timeMS;
        }

        // Helper for Essences
        private static List<ParseLogEntry_Essence> GetFiltered_Essences_Range(this ParseFullFile data, double minTimeMS, double maxTimeMS, bool snapToNearestFrame)
        {
            if (snapToNearestFrame)
            {
                minTimeMS = data.GetClosestFrameTime(minTimeMS);
                maxTimeMS = data.GetClosestFrameTime(maxTimeMS);
            }

            // Filter for position < 0 or > 1
            // [Result] Essenses are always not visible at +- 0.1 screen, independant of their screen calibration (DPI) and screen resolution
            float filterBelow = -0.1f;
            float filterAbove = 1.1f;

            return data.essenses.FindAll(essence =>
                essence.timeMS.IsBetween(minTimeMS, maxTimeMS) &&
                essence.position.y.IsBetween(filterBelow, filterAbove));
        }

        // Helper for Inputs
        private static List<ParseLogEntry_Input> GetFiltered_Inputs_Range(this ParseFullFile data, double minTimeMS, double maxTimeMS, bool snapToNearestFrame)
        {
            if (snapToNearestFrame)
            {
                minTimeMS = data.GetClosestFrameTime(minTimeMS);
                maxTimeMS = data.GetClosestFrameTime(maxTimeMS);
            }

            // Filter for Time
            return data.inputs.FindAll(input =>
                input.timeMS.IsBetween(minTimeMS, maxTimeMS));
        }

        // Helper for Collisions
        private static List<ParseLogEntry_Collision> GetFiltered_Collisions_Range(this ParseFullFile data, double minTimeMS, double maxTimeMS, bool snapToNearestFrame)
        {
            if (snapToNearestFrame)
            {
                minTimeMS = data.GetClosestFrameTime(minTimeMS);
                maxTimeMS = data.GetClosestFrameTime(maxTimeMS);
            }

            // Filter for Time
            return data.collisions.FindAll(collision =>
                collision.timeMS.IsBetween(minTimeMS, maxTimeMS));
        }

        // Helper for Collisions
        private static List<ParseLogEntry_EyeTracker_Gaze> GetFiltered_Gazes_Range(this ParseFullFile data, double minTimeMS, double maxTimeMS, bool snapToNearestFrame)
        {
            if (snapToNearestFrame)
            {
                minTimeMS = data.GetClosestFrameTime(minTimeMS);
                maxTimeMS = data.GetClosestFrameTime(maxTimeMS);
            }

            // Filter for Time
            return data.gazes.FindAll(entry => entry.gaze.timestampMS.IsBetween(minTimeMS, maxTimeMS));
        }

        // Helper for Collisions
        private static List<ParseLogEntry_EyeTracker_Sacada> GetFiltered_Sacades_Range(this ParseFullFile data, double minTimeMS, double maxTimeMS, bool snapToNearestFrame)
        {
            if (snapToNearestFrame)
            {
                minTimeMS = data.GetClosestFrameTime(minTimeMS);
                maxTimeMS = data.GetClosestFrameTime(maxTimeMS);
            }

            // Filter for Time
            return data.sacades.FindAll(entry =>
                entry.sacada.start.timestampMS.IsBetween(minTimeMS, maxTimeMS) ||
                entry.sacada.end.timestampMS.IsBetween(minTimeMS, maxTimeMS));
        }

        // Helper for Collisions
        private static List<ParseLogEntry_EyeTracker_Blink> GetFiltered_Blinks_Range(this ParseFullFile data, double minTimeMS, double maxTimeMS, bool snapToNearestFrame)
        {
            if (snapToNearestFrame)
            {
                minTimeMS = data.GetClosestFrameTime(minTimeMS);
                maxTimeMS = data.GetClosestFrameTime(maxTimeMS);
            }

            // Filter for Time
            return data.blinks.FindAll(entry =>
                entry.blink.startTimestampMS.IsBetween(minTimeMS, maxTimeMS) ||
                entry.blink.endTimestampMS.IsBetween(minTimeMS, maxTimeMS));
        }

        // Helper for Player
        private static List<ParseLogEntry_Player> GetFiltered_Player_Range(this ParseFullFile data, double minTimeMS, double maxTimeMS, bool snapToNearestFrame)
        {
            if (snapToNearestFrame)
            {
                minTimeMS = data.GetClosestFrameTime(minTimeMS);
                maxTimeMS = data.GetClosestFrameTime(maxTimeMS);
            }

            // Filter for Time
            return data.player.FindAll(player =>
                player.timeMS.IsBetween(minTimeMS, maxTimeMS));
        }

        // 2. Number of falling essences on screen
        public static float GetAvgNumEssences(this ParseFullFile data, double minTimeMS, double maxTimeMS, bool snapToNearestFrame)
        {
            List<ParseLogEntry_Essence> fallingEssences_TimeFiltered = data.GetFiltered_Essences_Range(minTimeMS, maxTimeMS, snapToNearestFrame);

            if (fallingEssences_TimeFiltered.Count == 0)
                return 0;

            // Group them
            return (float)fallingEssences_TimeFiltered.GroupBy(essence => essence.frameID).Average(g => g.Count());
        }

        // 3. Number of opposite color essences on screen
        // 4. Number of same color essences on screen
        public static float GetAvgNumEssences(this ParseFullFile data, double minTimeMS, double maxTimeMS, WorldType lookForEssencesOfColor, bool snapToNearestFrame)
        {
            List<ParseLogEntry_Essence> fallingEssences_TimeFiltered = data.GetFiltered_Essences_Range(minTimeMS, maxTimeMS, snapToNearestFrame);

            List<ParseLogEntry_Essence> fallingEssences_Filtered = fallingEssences_TimeFiltered.FindAll(essence => IsSameColor(lookForEssencesOfColor, essence.color));

            if (fallingEssences_Filtered.Count == 0)
                return -0;

            return (float)fallingEssences_Filtered.GroupBy(essence => essence.frameID).Average(g => g.Count());
        }

        private static bool IsSameColor(WorldType playerColor, ObjectColorType essenceColor)
        {
            if (playerColor == WorldType.Orange)
                return essenceColor == ObjectColorType.Orange ||
                essenceColor == ObjectColorType.OrangeHigh ||
                essenceColor == ObjectColorType.GrayOrange;

            else if (playerColor == WorldType.Blue)
                return essenceColor == ObjectColorType.Blue ||
                    essenceColor == ObjectColorType.BlueHigh ||
                    essenceColor == ObjectColorType.GrayBlue;

            return false;
        }

        // 5. Number of falling essences on top half of screen
        public static int GetNumFallingEssensesTopHalf(this ParseFullFile data, double timeMS)
        {
            List<ParseLogEntry_Essence> fallingEssences_TimeFiltered = data.GetFiltered_Essences_Range(timeMS, timeMS, true); // absolutely need to snap

            if (fallingEssences_TimeFiltered.Count == 0)
                return 0;

            return fallingEssences_TimeFiltered.FindAll(essence => essence.position.y >= 0.5f).Count;
        }

        // 6. Number of falling essences on bottom half of screen
        public static int GetNumFallingEssensesBotHalf(this ParseFullFile data, double timeMS)
        {
            List<ParseLogEntry_Essence> fallingEssences_TimeFiltered = data.GetFiltered_Essences_Range(timeMS, timeMS, true); // absolutely need to snap

            if (fallingEssences_TimeFiltered.Count == 0)
                return 0;

            return fallingEssences_TimeFiltered.FindAll(essence => essence.position.y < 0.5f).Count;
        }

        // 7a. Average speed of falling essences
        public static float GetAverageSpeedFallingEssenses_AverageOfFrames(this ParseFullFile data, double minTimeMS, double maxTimeMS, bool snapToNearestFrame)
        {
            List<ParseLogEntry_Essence> fallingEssences_TimeFiltered = data.GetFiltered_Essences_Range(minTimeMS, maxTimeMS, snapToNearestFrame); // absolutely need to snap

            if (fallingEssences_TimeFiltered.Count == 0)
                return 0;

            // Method 1 (average each frame's essences, then average the frames)
            return fallingEssences_TimeFiltered.GroupBy(essence => essence.frameID).Select(g => new { frameID = g.Key, avg = g.Average(essence => essence.speed.magnitude) }).Average(g => g.avg);
        }

        // 7b. Average speed of falling essences
        public static float GetAverageSpeedFallingEssenses_AverageOfEssences(this ParseFullFile data, double minTimeMS, double maxTimeMS, bool snapToNearestFrame)
        {
            List<ParseLogEntry_Essence> fallingEssences_TimeFiltered = data.GetFiltered_Essences_Range(minTimeMS, maxTimeMS, snapToNearestFrame); // absolutely need to snap

            if (fallingEssences_TimeFiltered.Count == 0)
                return 0;

            // Method 2 (average each essence over all the frames)
            return fallingEssences_TimeFiltered.Average(essence => essence.speed.magnitude);
        }

        // 8. Number of button-presses
        public static int GetNumButtonPresses(this ParseFullFile data, double minTimeMS, double maxTimeMS, bool snapToNearestFrame)
        {
            List<ParseLogEntry_Input> inputs_TimeFiltered = data.GetFiltered_Inputs_Range(minTimeMS, maxTimeMS, snapToNearestFrame);

            if (inputs_TimeFiltered.Count == 0)
                return 0;

            // Count button-downs
            return inputs_TimeFiltered.FindAll(input => input.keyState == ParseLogEntry_Input.KeyState.KEY_PRESS).Count;
        }

        // 9. Temporal proximity of pre-stim button-press
        public static double GetTemporalProximityPreStimButtonPress(this ParseFullFile data, double timeMS)
        {
            return data.GetTemporalProximityButtonPress(timeMS, true);
        }

        // 10. Temporal proximity of post-stim button-press
        public static double GetTemporalProximityPostStimButtonPress(this ParseFullFile data, double timeMS)
        {
            return data.GetTemporalProximityButtonPress(timeMS, false);
        }

        private static double GetTemporalProximityButtonPress(this ParseFullFile data, double timeMS, bool pre)
        {
            List<ParseLogEntry_Input> filteredInputs = pre ?
                data.GetFiltered_Inputs_Range(0, timeMS, false) :
                data.GetFiltered_Inputs_Range(timeMS, float.MaxValue, false); // dont need to snap

            // They're NOT already ordered!
            // ParseLogEntry_Input closestTimInput = preStimInputs.GetLast();
            if (filteredInputs.Count == 0) return -1;

            ParseLogEntry_Input closestInput = null;
            if (filteredInputs.Count == 1)
                closestInput = filteredInputs[0];
            else
                closestInput = filteredInputs.Aggregate((x, y) => Math.Abs(x.timeMS - timeMS) < Math.Abs(y.timeMS - timeMS) ? x : y);

            if (closestInput == null)
            {
                Debug.LogError("Closest input shouldnt be null at this point");
                return -1;
            }

            return closestInput.timeMS - timeMS;
        }

        // 11. Spatial proximity from stim location to nearest essence
        public static float GetSpatialProximityFromStimLocationToNearestEssense(this ParseFullFile data, double timeMS, Vector2 stimLocation)
        {
            List<ParseLogEntry_Essence> filteredEssences = data.GetFiltered_Essences_Range(timeMS, timeMS, true); // absolutely need to snap

            // ParseLogEntry_Input closestTimInput = preStimInputs.GetLast();
            if (filteredEssences.Count == 0) return -1;

            ParseLogEntry_Essence closestEssence = null;
            if (filteredEssences.Count == 1)
                closestEssence = filteredEssences[0];
            else
                closestEssence = filteredEssences.Aggregate((x, y) => Vector2.Distance(x.position, stimLocation) < Vector2.Distance(y.position, stimLocation) ? x : y);

            if (closestEssence == null)
            {
                Debug.LogError("Closest input shouldnt be null at this point");
                return -1;
            }

            return Vector2.Distance(closestEssence.position, stimLocation);
        }

        // 12. Spatial proximity from stim location to cluster mean of all essences
        public static float GetSpatialProximityFromStimLocationToClusterMeanEssenses(this ParseFullFile data, double timeMS, Vector2 stimLocation)
        {
            List<ParseLogEntry_Essence> filteredEssences = data.GetFiltered_Essences_Range(timeMS, timeMS, true); // absolutely need to snap

            // ParseLogEntry_Input closestTimInput = preStimInputs.GetLast();
            if (filteredEssences.Count == 0) return -1;

            float essencesAverageX = filteredEssences.Average(essences => essences.position.x);
            float essencesAverageY = filteredEssences.Average(essences => essences.position.y);

            Vector2 essencesAverage = new Vector2(essencesAverageX, essencesAverageY);

            return Vector2.Distance(stimLocation, essencesAverage);
        }

        // 13. Temporal proximity of last collision with opposite color essence
        // 14. Temporal proximity of last absorption of same color essence
        public static double GetTemporalProximityOfLastCollision(this ParseFullFile data, double timeMS, WorldType lookingForColor)
        {
            List<ParseLogEntry_Collision> preStimCollisions = data.GetFiltered_Collisions_Range(0, timeMS, false); // dont need to snap
            List<ParseLogEntry_Collision> filteredCollisions = preStimCollisions.FindAll(collision => IsSameColor(lookingForColor, collision.color));

            // They're NOT already ordered!
            // ParseLogEntry_Input closestTimInput = preStimInputs.GetLast();
            if (filteredCollisions.Count == 0) return -1;

            ParseLogEntry_Collision closestCollision = null;

            if (filteredCollisions.Count == 1)
                closestCollision = filteredCollisions[0];
            else
                closestCollision = filteredCollisions.Aggregate((x, y) => Math.Abs(x.timeMS - timeMS) < Math.Abs(y.timeMS - timeMS) ? x : y);

            if (closestCollision == null)
            {
                Debug.LogError("Closest input shouldnt be null at this point");
                return -1;
            }

            return closestCollision.timeMS - timeMS;
        }

        // 15. Number of collisions with opposite color essence
        // 16. Number of absorptions of same color essence
        public static int GetTotalNumCollisions(this ParseFullFile data, double minTimeMS, double maxTimeMS, WorldType lookingForColor, bool snapToNearestFrame)
        {
            List<ParseLogEntry_Collision> filteredCollisions = data.GetFiltered_Collisions_Range(minTimeMS, maxTimeMS, snapToNearestFrame);

            return filteredCollisions.FindAll(collision => IsSameColor(lookingForColor, collision.color)).Count;
        }

        // 17. Spatial proximity of character being controlled and stim location
        public static float GetSpatialProximityFromStimLocationToPlayer(this ParseFullFile data, double timeMS, Vector2 stimLocation)
        {
            List<ParseLogEntry_Player> filteredPlayer = data.GetFiltered_Player_Range(timeMS, timeMS, true); // absolutely need to snap

            // ParseLogEntry_Input closestTimInput = preStimInputs.GetLast();
            if (filteredPlayer.Count == 0) return -1;

            // Handle it in case there's more than one
            if (filteredPlayer.Count > 1)
                Debug.LogWarning("Shouldnt have more than 1 player entry per frame");

            float essencesAverageX = filteredPlayer.Average(player => player.position.x);
            float essencesAverageY = filteredPlayer.Average(essences => essences.position.y);

            Vector2 essencesAverage = new Vector2(essencesAverageX, essencesAverageY);

            return Vector2.Distance(stimLocation, essencesAverage);
        }

        /// 2.1 # Sacades within a range of time from stimulus onset
        public static int GetNumSaccades(this ParseFullFile data, double minTimeMS, double maxTimeMS, bool snapToNearestFrame)
        {
            List<ParseLogEntry_EyeTracker_Sacada> filteredEntries = data.GetFiltered_Sacades_Range(minTimeMS, maxTimeMS, snapToNearestFrame);

            return filteredEntries.Count;
        }

        /// 2.2 Temporal proximity of last sacada pre-stimulus onset
        public static double GetTemporalProximityOfLastSacada(this ParseFullFile data, double timeMS)
        {
            ParseLogEntry_EyeTracker_Sacada lastEntry = data.GetLastSacada(timeMS);
            if (lastEntry == null) return -1;
            return lastEntry.GetTemporalDistance(timeMS);
        }

        /// 2.3 Spatial proximity of last sacada
        public static float GetSpatialProximityFromStimLocationToLastSacada(this ParseFullFile data, double timeMS, Vector2 stimLocation)
        {
            ParseLogEntry_EyeTracker_Sacada lastEntry = data.GetLastSacada(timeMS);
            if (lastEntry == null) return -1;

            return lastEntry.GetSpatialDistance(stimLocation);
        }

        /// 2.4 Average gaze over a range of time
        public static Vector2 GetAverageGaze(this ParseFullFile data, double minTimeMS, double maxTimeMS, bool snapToNearestFrame)
        {
            List<ParseLogEntry_EyeTracker_Gaze> filteredEntries = data.GetFiltered_Gazes_Range(minTimeMS, maxTimeMS, snapToNearestFrame);

            if (filteredEntries.Count == 0) return -Vector2.one;

            float averageX = filteredEntries.Average(a => a.gaze.position.x);
            float averageY = filteredEntries.Average(a => a.gaze.position.y);

            return new Vector2(averageX, averageY);
        }

        /// 2.5 # Blinks within a range of time from stimulus onset
        public static int GetNumBlinks(this ParseFullFile data, double minTimeMS, double maxTimeMS, bool snapToNearestFrame)
        {
            List<ParseLogEntry_EyeTracker_Blink> filteredEntries = data.GetFiltered_Blinks_Range(minTimeMS, maxTimeMS, snapToNearestFrame);

            return filteredEntries.Count;
        }

        /// 2.6 Temporal proximity of last blink pre-stimulus onset
        public static double GetTemporalProximityOfLastBlink(this ParseFullFile data, double timeMS)
        {
            ParseLogEntry_EyeTracker_Blink lastEntry = data.GetLastBlink(timeMS);
            if (lastEntry == null) return -1;
            return lastEntry.GetTemporalDistance(timeMS);
        }

        private static ParseLogEntry_EyeTracker_Sacada GetLastSacada(this ParseFullFile data, double timeMS)
        {
            List<ParseLogEntry_EyeTracker_Sacada> preStimEntries = data.GetFiltered_Sacades_Range(0, timeMS, false); // dont need to snap

            // They're NOT already ordered!
            // ParseLogEntry_Input closestTimInput = preStimInputs.GetLast();
            if (preStimEntries.Count == 0) return null;

            ParseLogEntry_EyeTracker_Sacada closestEntry = null;

            if (preStimEntries.Count == 1)
                closestEntry = preStimEntries[0];
            else
                closestEntry = preStimEntries.Aggregate((x, y) => Math.Abs(x.GetTemporalDistance(timeMS)) < Math.Abs(y.GetTemporalDistance(timeMS)) ? x : y);

            if (closestEntry == null)
            {
                Debug.LogError("Closest input shouldnt be null at this point");
                return null;
            }

            return closestEntry;
        }

        private static ParseLogEntry_EyeTracker_Blink GetLastBlink(this ParseFullFile data, double timeMS)
        {
            List<ParseLogEntry_EyeTracker_Blink> preStimEntries = data.GetFiltered_Blinks_Range(0, timeMS, false); // dont need to snap

            // They're NOT already ordered!
            // ParseLogEntry_Input closestTimInput = preStimInputs.GetLast();
            if (preStimEntries.Count == 0) return null;

            ParseLogEntry_EyeTracker_Blink closestEntry = null;

            if (preStimEntries.Count == 1)
                closestEntry = preStimEntries[0];
            else
                closestEntry = preStimEntries.Aggregate((x, y) => Math.Abs(x.GetTemporalDistance(timeMS)) < Math.Abs(y.GetTemporalDistance(timeMS)) ? x : y);

            if (closestEntry == null)
            {
                Debug.LogError("Closest input shouldnt be null at this point");
                return null;
            }

            return closestEntry;
        }
    }

    [System.Serializable]
    public class ParseDetailsFile
    {
        public List<DetailsEntry> entries = new List<DetailsEntry>();

        public ParseDetailsFile() { }

        public ParseDetailsFile(string filePath, ParseVersionFile versionFile, DetailsEntry.Type type)
        {
            ParseFile(filePath, versionFile, type);
        }

        public void ParseFile(string filePath, ParseVersionFile versionFile, DetailsEntry.Type type)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError("Couldn't file logfile:" + filePath);
                return;
            }

            entries.Clear();

            string[] fileData = File.ReadAllLines(filePath);

            // Ignore the first entry
            for (int i = 1; i < fileData.Length; i++)
            {
                ParseEntry(i, fileData[i], versionFile, type);
            }

            entries = entries.OrderBy(o => o.stimulusTimeMS).ToList();
        }

        private void ParseEntry(int indexWithinFile, string line, ParseVersionFile versionFile, DetailsEntry.Type type)
        {
            // Avoid toLower() for so many data

            DetailsEntry log = new DetailsEntry(indexWithinFile, line, versionFile, type);

            if (log != null)
            {
                entries.Add(log);
            }
            else
            {
                Debug.LogError("Couldn't find any suitable class for data:" + line);
            }
        }

        public void Clear()
        {
            entries.Clear();
        }

        public bool TryGetEntryNearTime(double timeMS, double maxOffsetToleranceMS, out DetailsEntry entry)
        {
            entry = null;

            if (entries.Count == 0)
                return false;

            // Find those probes with an anchored stimulus close to the one we're searching for
            List<DetailsEntry> validEntries = entries.FindAll(a => Math.Abs(a.stimulusTimeMS - timeMS) < maxOffsetToleranceMS);

            if (validEntries.Count == 0)
                return false;

            // Pick the one which has the lowest TimeStamp (for LOCALIZERS only, but shouldn't affect PROBES)
            DetailsEntry possibleEntry = validEntries.Aggregate((x, y) => x.responseTS < y.responseTS ? x : y);

            // The following works for PROBES, but creates issues in LOCALIZERS
            // probe = probeDetails.Aggregate((x, y) => Mathf.Abs(x.timeMS - timeMS) < Math.Abs(y.timeMS - timeMS) ? x : y);

            double absDiff = Math.Abs(possibleEntry.stimulusTimeMS - timeMS);

            if (absDiff > maxOffsetToleranceMS)
                return false; // response = "{0} ({1})"._Format(defResponse, absDiff.ToString("#"));

            entry = possibleEntry;

            return true;
        }
    }

    // 19/05/06 -> 
    public enum VersionSettings
    {
        /// <summary>
        /// No Details
        /// </summary>
        initialVersion,

        /// <summary>
        /// Probe -> 0
        /// </summary>
        post190506,

        /// <summary>
        /// Probe -> 1
        /// </summary>
        post190826,

        /// <summary>
        /// Localizer -> 0
        /// </summary>
        post190913,

        /// <summary>
        /// Filler -> 0, Stimulus -> 0
        /// </summary>
        post190914,

        /// <summary>
        /// Probe -> 2, Localizer -> 1, Logging "isProbed" for stimuli
        /// </summary>
        post191213,

        /// <summary>
        /// Logging timesTamp for stimuli
        /// </summary>
        post200301,

        /// <summary>
        /// Probe -> 3, Localizer -> 2, Stimulus -> 1, Filler -> 1
        /// </summary>
        post200418,

        /// <summary>
        /// Added _COMPLETED / _ABORTED / _INTERRUPTED to the end of every full log file
        /// </summary>
        post200922,

        /// <summary>
        /// Added paused timestamp
        /// </summary>
        post200926,

        /// <summary>
        /// Probe -> 4, Localizer -> 3, Stimulus -> 2, Filler -> 2
        /// </summary>
        post200928,
    }
    
    [System.Serializable]
    public class ParseVersionFile
    {
        public DateTime versionDate;
        public string versionString;
        public VersionSettings versionSettings;

        public ParseVersionFile() { }

        public ParseVersionFile(string filePath)
        {
            ParseFile(filePath);
        }

        public void ParseFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError("Couldn't read version file:" + filePath);
                return;
            }

            versionDate = DateTime.MinValue;

            string[] fileData = File.ReadAllLines(filePath);

            // Should just have 1 line
            if (fileData.Length == 0)
            {
                Debug.LogError("No data in Version file: " + filePath);
                return;
            }

            versionString = fileData[0].Replace(';', '_');
            string versionDateString = Regex.Match(versionString, @"\d\d\d\d\d\d").Value;

            string year_STR = "20" + versionDateString[0] + versionDateString[1];
            string month_STR = "" + versionDateString[2] + versionDateString[3];
            string day_STR = "" + versionDateString[4] + versionDateString[5];

            int year = int.Parse(year_STR);
            int month = int.Parse(month_STR);
            int day = int.Parse(day_STR);

            versionDate = new DateTime(year, month, day);

            // Add new versions in descending order
            versionSettings =
                versionDate >= new DateTime(2020, 09, 28) ? VersionSettings.post200928 :
                versionDate >= new DateTime(2020, 09, 26) ? VersionSettings.post200926 :
                versionDate >= new DateTime(2020, 09, 22) ? VersionSettings.post200922 :
                versionDate >= new DateTime(2020, 04, 18) ? VersionSettings.post200418 :
                versionDate >= new DateTime(2020, 03, 01) ? VersionSettings.post200301 :
                versionDate >= new DateTime(2019, 12, 13) ? VersionSettings.post191213 :
                versionDate >= new DateTime(2019, 09, 14) ? VersionSettings.post190914 :
                versionDate >= new DateTime(2019, 09, 13) ? VersionSettings.post190913 :
                versionDate >= new DateTime(2019, 08, 26) ? VersionSettings.post190826 :
                versionDate >= new DateTime(2019, 05, 06) ? VersionSettings.post190506 :VersionSettings.initialVersion;
        }
    }

    [System.Serializable]
    public class ParseSessionFile
    {
        public ModuleType system = ModuleType.None;
        public Vector2Int screenSize = Vector2Int.zero;
        public float screenWidthToHeight = 1;

        public ParseSessionFile() { }

        public ParseSessionFile(string filePath)
        {
            ParseFile(filePath);
        }

        public void ParseFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError("Couldn't read version file:" + filePath);
                return;
            }

            string[] fileData = File.ReadAllLines(filePath);

            // Should just have 1 line
            if (fileData.Length == 0)
            {
                Debug.LogError("No data in Version file: " + filePath);
                return;
            }

            // Screen Pixel dimensions :: 1920x1080
            Regex screenDimensionsPattern = new Regex(@"Screen Pixel dimensions :: (?<screenWidth>\d+)x(?<screenHeight>\d+)");

            foreach (string s in fileData)
            {
                Match match = screenDimensionsPattern.Match(s);
                if (match.Success)
                {
                    screenSize.x = int.Parse(match.Groups["screenWidth"].Value);
                    screenSize.y = int.Parse(match.Groups["screenHeight"].Value);

                    screenWidthToHeight = (float)screenSize.x / screenSize.y;

                    continue;
                }

                foreach (ModuleType sT in Utility_Helper.EnumGetValues<ModuleType>())
                    if (s.ContainsInvariant(sT.ToString()))
                    {
                        system = sT;
                        break;
                    }
            }
        }
    }

    [System.Serializable]
    public class ParseFullFile
    {
        public bool isLoading { get; private set; }
        public float loadingProgress { get; private set; }
        public string subjectID { get; private set; }

        public string worldID;
        public int levelID;
        public string logState;

        public List<ParseLogEntry> allLogs = new List<ParseLogEntry>();

        public List<ParseLogEntry_GameStart> gameStart = new List<ParseLogEntry_GameStart>();
        public List<ParseLogEntry_Essence> essenses = new List<ParseLogEntry_Essence>();
        public List<ParseLogEntry_Input> inputs = new List<ParseLogEntry_Input>();
        public List<ParseLogEntry_Trigger> triggers = new List<ParseLogEntry_Trigger>();
        public List<ParseLogEntry_Background> backgrounds = new List<ParseLogEntry_Background>();
        public List<ParseLogEntry_Stimulus> stimuli = new List<ParseLogEntry_Stimulus>();
        public List<ParseLogEntry_Collision> collisions = new List<ParseLogEntry_Collision>();
        public List<ParseLogEntry_EyeTracker_Gaze> gazes = new List<ParseLogEntry_EyeTracker_Gaze>();
        public List<ParseLogEntry_EyeTracker_Sacada> sacades = new List<ParseLogEntry_EyeTracker_Sacada>();
        public List<ParseLogEntry_EyeTracker_Blink> blinks = new List<ParseLogEntry_EyeTracker_Blink>();
        public List<ParseLogEntry_FrameRendered> frameRendered = new List<ParseLogEntry_FrameRendered>();
        public List<ParseLogEntry_Player> player = new List<ParseLogEntry_Player>();

        public ParseFullFile() { }

        public ParseFullFile(string filePath, string subjectID, Vector2 WIDTH_LIMITS,
            ParseDetailsFile fillerDetails, ParseDetailsFile stimulusDetails,
            ParseDetailsFile probeDetails, ParseDetailsFile localizerDetails, 
            ParseVersionFile version, bool useParallelFor = true)
        {
            ParseFile(filePath, subjectID, WIDTH_LIMITS, fillerDetails, stimulusDetails, probeDetails, localizerDetails, version, useParallelFor);
        }

        public void ParseFile(string filePath, string subjectID, Vector2 WIDTH_LIMITS,
            ParseDetailsFile fillerDetails, ParseDetailsFile stimulusDetails,
            ParseDetailsFile probeDetails, ParseDetailsFile localizerDetails,
            ParseVersionFile version, bool useParallel)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError("Couldn't file logfile:" + filePath);
                return;
            }
            Clear();

            isLoading = true;
            loadingProgress = 0f;

            this.subjectID = subjectID;

            // Capture "TA652A_FullLog_{Level_1_1}_2019-12-05 09-26-34"
            Match levelIDMatch = Regex.Match(filePath.ToLower(), @"\W*(level)\W*_[a-zA-Z0-9]_[0-9]");
            if (levelIDMatch.Success)
            {
                worldID = levelIDMatch.Value.Split('_')[1].ToUpper();
                levelID = int.Parse(levelIDMatch.Value.Split('_')[2]);
            }

            // Grab state (ie. COMPLETED) from SC209A_FullLogLevel_0_2_2865.6336_COMPLETED.csv
            string filePathNoSuffix = filePath.RemoveLast(filePath.Split('.').GetLast().Length + 1);
            logState = filePathNoSuffix.Split('_').GetLast();

            string[] fileData = File.ReadAllLines(filePath);

            if (!useParallel)
            {
                for (int i = 0; i < fileData.Length; i++)
                {
                    ParseEntry(new ParseLogEntryArgs(worldID, "" + levelID, logState, i, fileData[i], version), WIDTH_LIMITS, fillerDetails, stimulusDetails, probeDetails, localizerDetails);
                    UpdateProgress(fileData.Length);
                }
            }
            else
            {
                System.Threading.Tasks.Parallel.For(0, fileData.Length, (index) =>
                {
                    ParseEntry(new ParseLogEntryArgs(worldID, "" + levelID, logState, index, fileData[index], version), WIDTH_LIMITS, fillerDetails, stimulusDetails, probeDetails, localizerDetails);
                    UpdateProgress(fileData.Length);
                });
            }

            // Allign offsets (this is due to parallelization)
            lock (stimuli)
            {
                List<ParseLogEntry_Stimulus> orderedStimuli = new List<ParseLogEntry_Stimulus>(stimuli.OrderBy(x => x.timeMS));
                ParseLogEntry_Stimulus curr = null;
                ParseLogEntry_Stimulus next = null;

                for (int i = 0; i < orderedStimuli.Count; i++)
                {
                    curr = orderedStimuli[i];
                    if (curr.logStimType != ParseLogEntry_Stimulus.StimLogType.Showing) continue;

                    if (i + 1 >= orderedStimuli.Count)
                    {
                        Debug.LogError("No stim after onset {0}"._Format(curr));
                        continue;
                    }

                    next = orderedStimuli[i + 1];

                    if (next.logStimType != ParseLogEntry_Stimulus.StimLogType.Hiding)
                    {
                        Debug.LogError("The stim after wasn't offset {0};{1}"._Format(curr, next));
                        continue;
                    }

                    curr.offsetTS = next.timeMS;
                    curr.offsetTS_NoPauses = next.timeMS_NoPauses;
                }
            }

            isLoading = false;
            loadingProgress = 1f;
        }

        /// <summary>
        /// Handles the stimulus analysis of a single Full Log file (ie. one level)
        /// </summary>
        public List<StimulusAnalysis> GetAllStimuliAnalyses()
        {
            List<StimulusAnalysis> analyses = new List<StimulusAnalysis>();

            lock (stimuli)
                foreach (ParseLogEntry_Stimulus stim in stimuli)
                {
                    if (stim.logStimType != ParseLogEntry_Stimulus.StimLogType.Showing) continue;
                    analyses.Add(GetStimulusAnalysis(stim, subjectID));
                }

            return analyses;
        }

        /// <summary>
        /// Handles the trigger analysis of a single Full Log file (ie. one level)
        /// </summary>
        public List<TriggerAnalysis> GetAllTriggerAnalyses()
        {
            List<TriggerAnalysis> analyses = new List<TriggerAnalysis>();

            // Loop over all stimuli for that level
            for (int i = 0; i < triggers.Count; i++)
            {
                // Ignore Offsets
                // if (triggers[i].triggerState != "TRIGGER_SENT" && triggers[i].triggerState != "TRIGGER_RECEIVED") continue;

                // Handle Onsets
                analyses.Add(GetTriggerAnalysis(triggers[i], subjectID));
            }

            if (analyses.Count > 0)
                analyses = analyses.OrderBy(a => a.timeMS).ToList();

            return analyses;
        }

        /// <summary>
        /// Creates the analysis (one line) for a certain stimulus
        /// </summary>
        private StimulusAnalysis GetStimulusAnalysis(ParseLogEntry_Stimulus stimulus, string subjectID)
        {
            StimulusAnalysis stimAnalysis = new StimulusAnalysis(stimulus, subjectID);

            WorldType worldColor_Same = GetWorldType(stimulus.stimID);
            WorldType worldColor_Opposite = worldColor_Same.Invert_TS();

            bool snapRangesToNearestFrames = true;

            // 2. Number of falling essences on screen -1000ms to +500ms; -1000ms to 0ms; 0-500ms; 
            stimAnalysis.avgNumFallingEssenses_m1000_p500 = this.GetAvgNumEssences(stimulus.timeMS - 1000f, stimulus.timeMS + 500, snapRangesToNearestFrames);
            stimAnalysis.avgNumFallingEssenses_m1000_0 = this.GetAvgNumEssences(stimulus.timeMS - 1000f, stimulus.timeMS, snapRangesToNearestFrames);
            stimAnalysis.avgNumFallingEssenses_0_p500 = this.GetAvgNumEssences(stimulus.timeMS, stimulus.timeMS + 500, snapRangesToNearestFrames);

            // 3. Number of opposite color essences on screen -1000ms to +500ms; -1000ms to 0ms; 0-500ms; 
            stimAnalysis.avgNumOppositeColorEssenses_m1000_p500 = this.GetAvgNumEssences(stimulus.timeMS - 1000f, stimulus.timeMS + 500, worldColor_Opposite, snapRangesToNearestFrames);
            stimAnalysis.avgNumOppositeColorEssenses_m1000_0 = this.GetAvgNumEssences(stimulus.timeMS - 1000f, stimulus.timeMS, worldColor_Opposite, snapRangesToNearestFrames);
            stimAnalysis.avgNumOppositeColorEssenses_0_p500 = this.GetAvgNumEssences(stimulus.timeMS, stimulus.timeMS + 500, worldColor_Opposite, snapRangesToNearestFrames);

            // 4. Number of same color essences on screen; -1000ms to +500ms; -1000ms to 0ms; 0-500ms; 
            stimAnalysis.avgNumSameColorEssenses_m1000_p500 = this.GetAvgNumEssences(stimulus.timeMS - 1000f, stimulus.timeMS + 500, worldColor_Same, snapRangesToNearestFrames);
            stimAnalysis.avgNumSameColorEssenses_m1000_0 = this.GetAvgNumEssences(stimulus.timeMS - 1000f, stimulus.timeMS, worldColor_Same, snapRangesToNearestFrames);
            stimAnalysis.avgNumSameColorEssenses_0_p500 = this.GetAvgNumEssences(stimulus.timeMS, stimulus.timeMS + 500, worldColor_Same, snapRangesToNearestFrames);

            // 5. Number of falling essences on top half of screen; -400ms; -200ms; 0ms; +200ms; +400ms
            stimAnalysis.numFallingEssensesTopHalf_m400 = this.GetNumFallingEssensesTopHalf(stimulus.timeMS - 400);
            stimAnalysis.numFallingEssensesTopHalf_m200 = this.GetNumFallingEssensesTopHalf(stimulus.timeMS - 200);
            stimAnalysis.numFallingEssensesTopHalf_0 = this.GetNumFallingEssensesTopHalf(stimulus.timeMS);
            stimAnalysis.numFallingEssensesTopHalf_p200 = this.GetNumFallingEssensesTopHalf(stimulus.timeMS + 200);
            stimAnalysis.numFallingEssensesTopHalf_p400 = this.GetNumFallingEssensesTopHalf(stimulus.timeMS + 400);

            // 6. Number of falling essences on bottom half of screen; - 400ms; -200ms; 0ms; +200ms; +400ms
            stimAnalysis.numFallingEssensesBotHalf_m400 = this.GetNumFallingEssensesBotHalf(stimulus.timeMS - 400);
            stimAnalysis.numFallingEssensesBotHalf_m200 = this.GetNumFallingEssensesBotHalf(stimulus.timeMS - 200);
            stimAnalysis.numFallingEssensesBotHalf_0 = this.GetNumFallingEssensesBotHalf(stimulus.timeMS);
            stimAnalysis.numFallingEssensesBotHalf_p200 = this.GetNumFallingEssensesBotHalf(stimulus.timeMS + 200);
            stimAnalysis.numFallingEssensesBotHalf_p400 = this.GetNumFallingEssensesBotHalf(stimulus.timeMS + 400);

            // 7. Average speed of falling essences;  - 1000ms to + 500ms
            stimAnalysis.averageSpeedFallingEssenses_OverFrames_m1000_p500 = this.GetAverageSpeedFallingEssenses_AverageOfFrames(stimulus.timeMS - 1000f, stimulus.timeMS + 500f, snapRangesToNearestFrames);
            stimAnalysis.averageSpeedFallingEssenses_OverEssences_m1000_p500 = this.GetAverageSpeedFallingEssenses_AverageOfEssences(stimulus.timeMS - 1000f, stimulus.timeMS + 500f, snapRangesToNearestFrames);

            // 8. Number of button-presses;  - 1000ms to + 500ms; -1000ms to 0ms; 0 - 500ms;
            stimAnalysis.numButtonPresses_m1000_p500 = this.GetNumButtonPresses(stimulus.timeMS - 1000f, stimulus.timeMS + 500, snapRangesToNearestFrames);
            stimAnalysis.numButtonPresses_m1000_0 = this.GetNumButtonPresses(stimulus.timeMS - 1000f, stimulus.timeMS, snapRangesToNearestFrames);
            stimAnalysis.numButtonPresses_0_p500 = this.GetNumButtonPresses(stimulus.timeMS, stimulus.timeMS + 500, snapRangesToNearestFrames);

            // 9. Temporal proximity of pre-stim button-press; delta time between last button press and stim onset (0ms)
            stimAnalysis.tempProxButtonPressPreStim = this.GetTemporalProximityPreStimButtonPress(stimulus.timeMS);

            // 10. Temporal proximity of post-stim button-press; delta time between last button press and stim onset (0ms)
            stimAnalysis.tempProxButtonPressPostStim = this.GetTemporalProximityPostStimButtonPress(stimulus.timeMS);

            // 11. Spatial proximity from stim location to nearest essence ;  -400ms; -200ms; 0ms; +200ms; +400ms
            stimAnalysis.spatialProxFromStimLocToNearestEssense_m400 = this.GetSpatialProximityFromStimLocationToNearestEssense(stimulus.timeMS - 400, stimulus.screenPosition);
            stimAnalysis.spatialProxFromStimLocToNearestEssense_m200 = this.GetSpatialProximityFromStimLocationToNearestEssense(stimulus.timeMS - 200, stimulus.screenPosition);
            stimAnalysis.spatialProxFromStimLocToNearestEssense_0 = this.GetSpatialProximityFromStimLocationToNearestEssense(stimulus.timeMS, stimulus.screenPosition);
            stimAnalysis.spatialProxFromStimLocToNearestEssense_p200 = this.GetSpatialProximityFromStimLocationToNearestEssense(stimulus.timeMS + 200, stimulus.screenPosition);
            stimAnalysis.spatialProxFromStimLocToNearestEssense_p400 = this.GetSpatialProximityFromStimLocationToNearestEssense(stimulus.timeMS + 400, stimulus.screenPosition);

            // 12. Spatial proximity from stim location to cluster mean of all essences ;  -400ms; -200ms; 0ms; +200ms; +400ms
            stimAnalysis.spatialProxFromStimLocToClusterMeanEssenses_m400 = this.GetSpatialProximityFromStimLocationToClusterMeanEssenses(stimulus.timeMS - 400, stimulus.screenPosition);
            stimAnalysis.spatialProxFromStimLocToClusterMeanEssenses_m200 = this.GetSpatialProximityFromStimLocationToClusterMeanEssenses(stimulus.timeMS - 200, stimulus.screenPosition);
            stimAnalysis.spatialProxFromStimLocToClusterMeanEssenses_0 = this.GetSpatialProximityFromStimLocationToClusterMeanEssenses(stimulus.timeMS, stimulus.screenPosition);
            stimAnalysis.spatialProxFromStimLocToClusterMeanEssenses_p200 = this.GetSpatialProximityFromStimLocationToClusterMeanEssenses(stimulus.timeMS + 200, stimulus.screenPosition);
            stimAnalysis.spatialProxFromStimLocToClusterMeanEssenses_p400 = this.GetSpatialProximityFromStimLocationToClusterMeanEssenses(stimulus.timeMS + 400, stimulus.screenPosition);

            // 13. Temporal proximity of last collision with opposite color essence ;  delta time from collision to stim onset(0ms)
            stimAnalysis.tempProxOfLastCollision = this.GetTemporalProximityOfLastCollision(stimulus.timeMS, worldColor_Opposite);

            // 14. Temporal proximity of last absorption of same color essence  ;  delta time from collision to stim onset(0ms)
            stimAnalysis.tempProxOfLastAbsorption = this.GetTemporalProximityOfLastCollision(stimulus.timeMS, worldColor_Same);

            // 15. Number of collisions with opposite color essence ; - 2000ms to +500ms
            stimAnalysis.numOfCollissions_m2000_p500 = this.GetTotalNumCollisions(stimulus.timeMS - 2000f, stimulus.timeMS + 500f, worldColor_Opposite, snapRangesToNearestFrames);

            // 16. Number of absorptions of same color essence;  - 2000ms to +500ms
            stimAnalysis.numOfAbsorptions_m2000_p500 = this.GetTotalNumCollisions(stimulus.timeMS - 2000f, stimulus.timeMS + 500f, worldColor_Same, snapRangesToNearestFrames);

            // 17. Spatial proximity of character being controlled and stim location;   - 400ms; -200ms; 0ms; +200ms; +400ms
            stimAnalysis.spatialProxFromStimLocToPlayer_m400 = this.GetSpatialProximityFromStimLocationToPlayer(stimulus.timeMS - 400, stimulus.screenPosition);
            stimAnalysis.spatialProxFromStimLocToPlayer_m200 = this.GetSpatialProximityFromStimLocationToPlayer(stimulus.timeMS - 200, stimulus.screenPosition);
            stimAnalysis.spatialProxFromStimLocToPlayer_0 = this.GetSpatialProximityFromStimLocationToPlayer(stimulus.timeMS, stimulus.screenPosition);
            stimAnalysis.spatialProxFromStimLocToPlayer_p200 = this.GetSpatialProximityFromStimLocationToPlayer(stimulus.timeMS + 200, stimulus.screenPosition);
            stimAnalysis.spatialProxFromStimLocToPlayer_p400 = this.GetSpatialProximityFromStimLocationToPlayer(stimulus.timeMS + 400, stimulus.screenPosition);

            /// 2.1 # Sacades within a range of time from stimulus onset
            stimAnalysis.numSaccades_m2000_p500 = this.GetNumSaccades(stimulus.timeMS - 2000f, stimulus.timeMS + 500f, snapRangesToNearestFrames);

            /// 2.2 Temporal proximity of last sacada pre-stimulus onset
            stimAnalysis.tempProxOfLastSacada = this.GetTemporalProximityOfLastSacada(stimulus.timeMS);

            /// 2.3 Spatial proximity of last sacada (-400ms)
            stimAnalysis.spatialProxFromStimLocToLastSacadaLine = this.GetSpatialProximityFromStimLocationToLastSacada(stimulus.timeMS, stimulus.screenPosition);

            /// 2.4 Average gaze over a range of time
            stimAnalysis.averageGaze_m2000_p500 = this.GetAverageGaze(stimulus.timeMS - 2000f, stimulus.timeMS + 500f, snapRangesToNearestFrames);

            /// 2.5 # Blinks within a range of time from stimulus onset
            stimAnalysis.numBlinks_m2000_p500 = this.GetNumBlinks(stimulus.timeMS - 2000f, stimulus.timeMS + 500f, snapRangesToNearestFrames);

            /// 2.6 Temporal proximity of last blink
            stimAnalysis.tempProxOfLastBlink = this.GetTemporalProximityOfLastBlink(stimulus.timeMS);

            return stimAnalysis;
        }

        /// <summary>
        /// Creates the analysis (one line) for a certain trigger
        /// </summary>
        private TriggerAnalysis GetTriggerAnalysis(ParseLogEntry_Trigger trigger, string subjectID)
        {
            TriggerAnalysis triggerAnalysis = new TriggerAnalysis(trigger, subjectID);
            
            return triggerAnalysis;
        }

        public void Clear()
        {
            gameStart.Clear();
            essenses.Clear();
            inputs.Clear();
            triggers.Clear();
            backgrounds.Clear();
            stimuli.Clear();
            collisions.Clear();
            frameRendered.Clear();
            player.Clear();
        }
        
        // [SOS] Need to make these ENUMS to make sure they are consistent between GAME and current
        private void ParseEntry(ParseLogEntryArgs baseArgs, Vector2 WIDTH_LIMITS,
            ParseDetailsFile fillerDetails, ParseDetailsFile stimulusDetails,
            ParseDetailsFile probeDetails, ParseDetailsFile localizerDetails)
        {
            // [SOS] Avoid toLower() for so many data
            string line = baseArgs.line;

            if (line.Contains("collided"))
                TryAddToCollection(collisions, new ParseLogEntry_Collision(baseArgs));

            else if (line.Contains("Interactable") && !line.Contains("collided"))
                TryAddToCollection(essenses, new ParseLogEntry_Essence(baseArgs, WIDTH_LIMITS));

            else if (line.Contains("INPUT_MANAGER") && 
                (line.Contains("KEY_PRESS") || line.Contains("KEY_HOLD") || line.Contains("KEY_RELEASE")))
                TryAddToCollection(inputs, new ParseLogEntry_Input(baseArgs));

            else if (line.Contains("TRIGGER_MANAGER") && 
                (line.Contains("TRIGGER_SENT") || line.Contains("TRIGGER_FAILED") || 
                    (line.Contains("PHOTODIODE") && line.Contains("TRIGGER_INFO") && line.Contains("TOGGLE;ON") && 
                        (line.ContainsInvariant(TriggerOutEvent.LevelBegin.ToString()) || line.ContainsInvariant(TriggerOutEvent.LevelEnd.ToString()))))) // || line.Contains("TRIGGER_REQUESTED")))
                // if ((log as ParseLogEntry_Trigger).triggerCode.ToInt() <= 0) log = null;
                TryAddToCollection(triggers, new ParseLogEntry_Trigger(baseArgs));

            else if (line.Contains("Rotating"))
                TryAddToCollection(backgrounds, new ParseLogEntry_Background(baseArgs, WIDTH_LIMITS));

            // Skip stimulus duration logs
            else if (line.Contains("Stimulus Manager") && !line.Contains("STIMULUS_DURATION"))
                TryAddToCollection(stimuli, new ParseLogEntry_Stimulus(baseArgs, WIDTH_LIMITS, stimulusDetails, probeDetails, localizerDetails));

            else if (line.Contains("FRAME_BEGIN") || line.Contains("FRAME_JUST_RENDERED"))
                TryAddToCollection(frameRendered, new ParseLogEntry_FrameRendered(baseArgs));

            else if (line.Contains("Player_Runner_Screen_Position"))
                TryAddToCollection(player, new ParseLogEntry_Player(baseArgs, WIDTH_LIMITS));

            else if (line.Contains("Game_Start"))
                TryAddToCollection(gameStart, new ParseLogEntry_GameStart(baseArgs));

            else if (line.Contains("GAZE"))
                TryAddToCollection(gazes, new ParseLogEntry_EyeTracker_Gaze(baseArgs, WIDTH_LIMITS));

            else if (line.Contains("SACADA"))
                TryAddToCollection(sacades, new ParseLogEntry_EyeTracker_Sacada(baseArgs, WIDTH_LIMITS));

            else if (line.Contains("BLINK"))
                TryAddToCollection(blinks, new ParseLogEntry_EyeTracker_Blink(baseArgs));
        }

        private void TryAddToCollection<T>(List<T> entries, T entry) where T : ParseLogEntry
        {
            lock (entries)
                entries.Add(entry);

            lock (allLogs)
                allLogs.Add(entry);
        }

        private void UpdateProgress(int maxLength)
        {
            loadingProgress = (allLogs.Count / maxLength);
        }

        // Use stimID as fallback for previous logs
        private WorldType GetWorldType(string stimID)
        {
            if (gameStart.Count > 0)
            {
                return gameStart[0].worldType;
            }
            else
            {
                string[] data = stimID.Split('_');
                if (data.Length >= 1)
                {
                    int worldID = data[0].ToInt();

                    // [TODO] Flipped worlds?
                    if (worldID == 1 || worldID == 3)
                        return WorldType.Blue;
                    else if (worldID == 2 || worldID == 4)
                        return WorldType.Orange;
                    else
                    {
                        //Debug.LogError("Invalid World ({0})"._Format(worldID));
                    }
                }
                else
                {
                    //Debug.LogError("Invalid StimID ({0})"._Format(stimID));
                }
                return WorldType.Blue;
            }
        }
    }

    [System.Serializable]
    public class DetailsEntry
    {
        public double stimulusTimeMS;
        public string levelID;
        public string activeLevelID;
        public StimulusType? stimulusType;
        public int stimulusID;
        public Direction_2D_Diagonal stimulusLocation;
        public float questionTS;
        public float questionTS_NoPauses;
        public float responseTS;
        public float responseTS_NoPauses;
        public float responseDT;
        public string response;
        public string responseEvaluation;
        public double difficulty;
        public double averageDifficulty;

        public int indexWithinFile = -1;

        public enum Type { Filler, Stimulus, LocalizerResponse, InGameProbe }
        
        struct Indices
        {
            public int idxStimOnsetTimeMS;
            public int idxLevelID;
            public int idxActiveLevelID;
            public int idxStimulusType;
            public int idxStimulusID;
            public int idxStimulusLocation;
            public int idxQuestionTS;
            public int idxQuestionTS_NoPauses;
            public int idxResponseTS;
            public int idxResponseTS_NoPauses;
            public int idxResponseDT;
            public int idxUserResponse;
            public int idxUserResponseEvaluation;
            public int idxDifficulty;
            public int idxAverageDifficulty;
            public int maxIndex;

            public static Indices GetIndices(Type type, VersionSettings versionSettings)
            {
                int revision = GetRevision(type, versionSettings);
                return GetIndices(type, revision);
            }

            /// <summary>
            /// Based on version, figure current revision for this type
            /// </summary>
            private static int GetRevision(Type type, VersionSettings versionSettings)
            {
                int revision = -1;
                switch (type)
                {
                    case Type.Filler:
                        revision =
                            versionSettings >= VersionSettings.post200928 ? 2 :
                            versionSettings >= VersionSettings.post200418 ? 1 :
                            versionSettings >= VersionSettings.post190914 ? 0 : -1;
                        break;
                    case Type.Stimulus:
                        revision =
                            versionSettings >= VersionSettings.post200928 ? 2 :
                            versionSettings >= VersionSettings.post200418 ? 1 :
                            versionSettings >= VersionSettings.post190914 ? 0 : -1;
                        break;
                    case Type.InGameProbe:
                        revision =
                            versionSettings >= VersionSettings.post200928 ? 4 :
                            versionSettings >= VersionSettings.post200418 ? 3 :
                            versionSettings >= VersionSettings.post191213 ? 2 :
                            versionSettings >= VersionSettings.post190826 ? 1 :
                            versionSettings >= VersionSettings.post190506 ? 0 : -1;
                        break;
                    case Type.LocalizerResponse:
                        revision =
                            versionSettings >= VersionSettings.post200928 ? 3 :
                            versionSettings >= VersionSettings.post200418 ? 2 :
                            versionSettings >= VersionSettings.post191213 ? 1 :
                            versionSettings >= VersionSettings.post190913 ? 0 : -1;
                        break;
                }

                return revision;
            }

            private static Indices GetIndices(Type type, int revision)
            {
                Indices indices = new Indices();

                if (revision < 0) return indices;

                switch (type)
                {
                    // ====== FILLERS
                    // ===========================

                    case Type.Filler:
                        /*
                        Fillers 0
                            [0]fillerTS, [1]worldID, [2]levelIndex, [3]difficulty, [4]avgDifficulty, 
                            [5]performance, [6]avgPerformance, [7]scorePercentile, [8]averageStarsPerLevel, [9]averageStarsInWorld
                        */
                        if (revision == 0)
                        {
                            indices.idxStimOnsetTimeMS = 0;
                            indices.idxLevelID = 2;
                            indices.idxActiveLevelID = -1;
                            indices.idxStimulusType = -1;
                            indices.idxStimulusID = -1;
                            indices.idxStimulusLocation = -1;
                            indices.idxQuestionTS = -1;
                            indices.idxQuestionTS_NoPauses = -1;
                            indices.idxResponseTS = -1;
                            indices.idxResponseTS_NoPauses = -1;
                            indices.idxResponseDT = -1;
                            indices.idxUserResponse = -1;
                            indices.idxUserResponseEvaluation = -1;
                            indices.idxDifficulty = 3;
                            indices.idxAverageDifficulty = 4;
                            indices.maxIndex = 9;
                        }
                        /*
                        Fillers 1
                            [0] fillerOnsetTS           [1] dTSinceLast_NoPauses
                            [2] currentLevelID	        [3] activeLevelID	        [4] activeWorldID	        [5] activeWorldType	
                            [6] difficulty	            [7] averageDifficulty	    [8] performance	            [9] averagePerformance	
                            [10] scorePercentile	    [11]averageStarsPerLevel	[12] averageStarsInWorld	
                            [13] animCycleID            [14] type            
                            
                            ++ [15] duringSafeEndOfGame
                        */
                        else if (revision == 1)
                        {
                            indices.idxStimOnsetTimeMS = 0;
                            indices.idxLevelID = 2;
                            indices.idxActiveLevelID = 3;
                            indices.idxStimulusType = -1;
                            indices.idxStimulusID = -1;
                            indices.idxStimulusLocation = -1;
                            indices.idxQuestionTS = -1;
                            indices.idxQuestionTS_NoPauses = -1;
                            indices.idxResponseTS = -1;
                            indices.idxResponseTS_NoPauses = -1;
                            indices.idxResponseDT = -1;
                            indices.idxUserResponse = -1;
                            indices.idxUserResponseEvaluation = -1;
                            indices.idxDifficulty = 6;
                            indices.idxAverageDifficulty = 7;
                            indices.maxIndex = 15;
                        }
                        /*
                        Fillers 2
                            [0] onsetTS                 [1] onsetTS_NoPauses        [2] dTSinceLast_NoPauses
                            [3] currentLevelID	        [4] activeLevelID	        [5] activeWorldID	        [6] activeWorldType	
                            [7] difficulty	            [8] averageDifficulty	    [9] performance	            [10] averagePerformance	
                            [11] scorePercentile	    [12]averageStarsPerLevel	[13] averageStarsInWorld	
                            [14] animCycleID            [15] type                   
                            
                            ++ [16] duringSafeEndOfGame
                        */
                        else if (revision == 2)
                        {
                            indices.idxStimOnsetTimeMS = 0;
                            indices.idxLevelID = 3;
                            indices.idxActiveLevelID = 4;
                            indices.idxStimulusType = -1;
                            indices.idxStimulusID = -1;
                            indices.idxStimulusLocation = -1;
                            indices.idxQuestionTS = -1;
                            indices.idxQuestionTS_NoPauses = -1;
                            indices.idxResponseTS = -1;
                            indices.idxResponseTS_NoPauses = -1;
                            indices.idxResponseDT = -1;
                            indices.idxUserResponse = -1;
                            indices.idxUserResponseEvaluation = -1;
                            indices.idxDifficulty = 7;
                            indices.idxAverageDifficulty = 8;
                            indices.maxIndex = 16;
                        }
                        break;

                    // ====== STIMULI
                    // ===========================

                    case Type.Stimulus:
                        /*
                        Stimulus 0
                            [0]stimulusOnsetTS, [1]worldID, [2]levelIndex, [3]stimulusType, [4]stimulusName, 
                            [5]stimulusLocation, [6]difficulty, [7]avgDifficulty, [8]performance, [9]avgPerformance,
                            [10]scorePercentile, [11]averageStarsPerLevel, [12]averageStarsInWorld
                        */
                        if (revision == 0)
                        {
                            indices.idxStimOnsetTimeMS = 0;
                            indices.idxLevelID = 2;
                            indices.idxActiveLevelID = -1;
                            indices.idxStimulusType = 3;
                            indices.idxStimulusID = 4;
                            indices.idxStimulusLocation = 5;
                            indices.idxQuestionTS = -1;
                            indices.idxQuestionTS_NoPauses = -1;
                            indices.idxResponseTS = -1;
                            indices.idxResponseTS_NoPauses = -1;
                            indices.idxResponseDT = -1;
                            indices.idxUserResponse = -1;
                            indices.idxUserResponseEvaluation = -1;
                            indices.idxDifficulty = 6;
                            indices.idxAverageDifficulty = 7;
                            indices.maxIndex = 12;
                        }
                        /*
                        Stimulus 1
                            [0] stimulusOnsetTS         [1] dTSinceLast_NoPauses
                            [2] currentLevelID	        [3] activeLevelID	        [4] activeWorldID	        [5] activeWorldType	
                            [6] difficulty	            [7] averageDifficulty	    [8] performance	            [9] averagePerformance	
                            [10] scorePercentile	    [11]averageStarsPerLevel	[12] averageStarsInWorld	
                            [13] animCycleID            [14] type                                                                           <-- (shared with Fillers 1 up to this point)

                            [15] stimulusID	            [16] stimulusType	        [17] stimulusName	        [18] stimulusLocation
                        */
                        else if (revision == 1)
                        {
                            indices.idxStimOnsetTimeMS = 0;
                            indices.idxLevelID = 2;
                            indices.idxActiveLevelID = 3;
                            indices.idxStimulusType = 16;
                            indices.idxStimulusID = 15;
                            indices.idxStimulusLocation = 18;
                            indices.idxQuestionTS = -1;
                            indices.idxQuestionTS_NoPauses = -1;
                            indices.idxResponseTS = -1;
                            indices.idxResponseTS_NoPauses = -1;
                            indices.idxResponseDT = -1;
                            indices.idxUserResponse = -1;
                            indices.idxUserResponseEvaluation = -1;
                            indices.idxDifficulty = 6;
                            indices.idxAverageDifficulty = 7;
                            indices.maxIndex = 18;
                        }
                        /*
                        Stimulus 2
                            [0] onsetTS                 [1] onsetTS_NoPauses        [2] dTSinceLast_NoPauses
                            [3] currentLevelID	        [4] activeLevelID	        [5] activeWorldID	        [6] activeWorldType	
                            [7] difficulty	            [8] averageDifficulty	    [9] performance	            [10] averagePerformance	
                            [11] scorePercentile	    [12]averageStarsPerLevel	[13] averageStarsInWorld	
                            [14] animCycleID            [15] type                                                                          <-- (shared with Fillers 2 up to this point)

                            [16] stimulusID	            [17] stimulusType	        [18] stimulusName	        [19] stimulusLocation
                        */
                        else if (revision == 2)
                        {
                            indices.idxStimOnsetTimeMS = 0;
                            indices.idxLevelID = 3;
                            indices.idxActiveLevelID = 4;
                            indices.idxStimulusType = 17;
                            indices.idxStimulusID = 16;
                            indices.idxStimulusLocation = 19;
                            indices.idxQuestionTS = -1;
                            indices.idxQuestionTS_NoPauses = -1;
                            indices.idxResponseTS = -1;
                            indices.idxResponseTS_NoPauses = -1;
                            indices.idxResponseDT = -1;
                            indices.idxUserResponse = -1;
                            indices.idxUserResponseEvaluation = -1;
                            indices.idxDifficulty = 7;
                            indices.idxAverageDifficulty = 8;
                            indices.maxIndex = 19;
                        }
                        break;


                    // ====== LOCALIZERS
                    // ===========================

                    case Type.LocalizerResponse:
                        /*
                        Localizer 0
                            [0]responseTS, [1]worldID, [2]levelIndex, [3]stimulusOnsetTS , [4]stimulusType, 
                            [5]stimulusName, [6]stimulusLocation, 7]windowEndTS, [8]responseTS, [9]responseDT, 
                            [10]responseType, [11]difficulty, [12]avgDifficulty, [13]performance, [14]avgPerformance, 
                            [15]scorePercentile, [16]averageStarsPerLevel, [17]averageStarsInWorld
                        */
                        if (revision == 0)
                        {
                            indices.idxStimOnsetTimeMS = 3;
                            indices.idxLevelID = 2;
                            indices.idxActiveLevelID = -1;
                            indices.idxStimulusType = 4;
                            indices.idxStimulusID = 5;
                            indices.idxStimulusLocation = 6;
                            indices.idxResponseTS = 0;
                            indices.idxResponseTS_NoPauses = -1;
                            indices.idxResponseDT = 9;
                            indices.idxUserResponse = 10;
                            indices.idxUserResponseEvaluation = 10;
                            indices.idxDifficulty = 11;
                            indices.idxAverageDifficulty = 12;
                            indices.maxIndex = 17;
                        }
                        /*
                        Localizer 1
                            [0]stimulusOnsetTS, [1]levelIndex, [2]stimulusType, [3]stimulusName, [4]stimulusLocation, 
                            [5]responseTS, [6]responseDT, [7]responseType, [8]difficulty, [9]avgDifficulty,
                            [10]performance, [11]avgPerformance, [12]scorePercentile, [13]averageStarsPerLevel, [14]averageStarsInWorld, 
                            [15]windowEndTS, [16]worldID
                        */
                        else if (revision == 1)
                        {
                            indices.idxStimOnsetTimeMS = 0;
                            indices.idxLevelID = 2;
                            indices.idxActiveLevelID = -1;
                            indices.idxStimulusType = 2;
                            indices.idxStimulusID = 3;
                            indices.idxStimulusLocation = 4;
                            indices.idxResponseTS = 5;
                            indices.idxResponseTS_NoPauses = -1;
                            indices.idxResponseDT = 6;
                            indices.idxUserResponse = -1;
                            indices.idxUserResponseEvaluation = 7;
                            indices.idxDifficulty = 8;
                            indices.idxAverageDifficulty = 9;
                            indices.maxIndex = 16;
                        }
                        /*
                        Localizer 2
                            [0] stimulusOnsetTS         [1] dTSinceLast_NoPauses
                            [2] currentLevelID	        [3] activeLevelID	        [4] activeWorldID	        [5] activeWorldType	
                            [6] difficulty	            [7] averageDifficulty	    [8] performance	            [9] averagePerformance	
                            [10] scorePercentile	    [11]averageStarsPerLevel	[12] averageStarsInWorld	
                            [13] animCycleID            [14] type                                                                           <-- (shared with Fillers 1 up to this point)

                            [15] stimulusID	            [16] stimulusType	        [17] stimulusName	        [18] stimulusLocation       <-- (shared with Stimulus 1 up to this point)

                            [19] questionID	            [20] questionTS	            [21] windowEndTS	
                            [22] responseID	            [23] responseTS	            [24] responseDT	            
                            [25] response	            [26] responseEvaluation     [27] repliedUsingHighAccu
                        */
                        else if (revision == 2)
                        {
                            indices.idxStimOnsetTimeMS = 0;
                            indices.idxLevelID = 2;
                            indices.idxActiveLevelID = 3;
                            indices.idxStimulusType = 16;
                            indices.idxStimulusID = 15;
                            indices.idxStimulusLocation = 18;
                            indices.idxResponseTS = 23;
                            indices.idxResponseTS_NoPauses = -1;
                            indices.idxResponseDT = 24;
                            indices.idxUserResponse = 25;
                            indices.idxUserResponseEvaluation = 26;
                            indices.idxDifficulty = 6;
                            indices.idxAverageDifficulty = 7;
                            indices.maxIndex = 27;
                        }

                        /*
                        Localizer 3
                            [0] stimulusOnsetTS         [1] stimulusOnsetTS_NoPauses[2] dTSinceLast_NoPauses
                            [3] currentLevelID	        [4] activeLevelID	        [5] activeWorldID	        [6] activeWorldType	
                            [7] difficulty	            [8] averageDifficulty	    [9] performance	            [10] averagePerformance	
                            [11] scorePercentile	    [12]averageStarsPerLevel	[13] averageStarsInWorld	
                            [14] animCycleID            [15] type                                                                          <-- (shared with Fillers 2 up to this point)

                            [16] stimulusID	            [17] stimulusType	        [18] stimulusName	        [19] stimulusLocation      <-- (shared with Stimulus 2 up to this point)

                            [20] questionID	            [21] questionTS	            [22] questionTS_NoPauses    [23] windowEndTS        [24] windowEndTS_NoPauses
                            [25] responseID	            [26] responseTS	            [27] responseTS_NoPauses    [28] responseDT	            
                            [29] response	            [30] responseEvaluation     [31] repliedUsingHighAccu     
                        */
                        else if (revision == 3)
                        {
                            indices.idxStimOnsetTimeMS = 0;
                            indices.idxLevelID = 3;
                            indices.idxActiveLevelID = 4;
                            indices.idxStimulusID = 16;
                            indices.idxStimulusType = 17;
                            indices.idxStimulusLocation = 19;
                            indices.idxResponseTS = 26;
                            indices.idxResponseTS_NoPauses = 27;
                            indices.idxResponseDT = 28;
                            indices.idxUserResponse = 29;
                            indices.idxUserResponseEvaluation = 30;
                            indices.idxDifficulty = 7;
                            indices.idxAverageDifficulty = 8;
                            indices.maxIndex = 31;
                        }

                        break;

                    // ====== PROBES
                    // ===========================

                    case Type.InGameProbe:
                        /*
                        Probe 0
                            [0] stimulusTS, [1] levelIndex, [2] stimulusType, [3] stimulusName, [4] stimulusLocation,
                            [5] questionTS, [6] responseTS, [7] responseDT, [8] visibility, [9] responseType, 
                            [10] difficulty
                        */
                        if (revision == 0)
                        {
                            indices.idxStimOnsetTimeMS = 0;
                            indices.idxLevelID = -1;
                            indices.idxActiveLevelID = 1;
                            indices.idxStimulusType = 2;
                            indices.idxStimulusID = 3;
                            indices.idxStimulusLocation = 4;
                            indices.idxQuestionTS = 5;
                            indices.idxQuestionTS_NoPauses = -1;
                            indices.idxResponseTS = 6;
                            indices.idxResponseTS_NoPauses = -1;
                            indices.idxResponseDT = 7;
                            indices.idxUserResponse = -1;
                            indices.idxUserResponseEvaluation = 9;
                            indices.idxDifficulty = 10;
                            indices.idxAverageDifficulty = -1;
                            indices.maxIndex = 10;
                        }
                        /*
                        Probe 1
                            [0] stimulusTS, [1]levelIndex, [2]stimulusType, [3]stimulusName, [4]stimulusLocation,
                            [5]questionTS, [6]responseTS, [7]responseDT, [8] visibility, [9]responseType, 
                            [10]difficulty, [11]avgDifficulty, [12]scorePercentile, [13]averageStarsPerLevel, [14]averageStarsInWorld
                        */
                        else if (revision == 1)
                        {
                            indices.idxStimOnsetTimeMS = 0;
                            indices.idxLevelID = 1;
                            indices.idxActiveLevelID = -1;
                            indices.idxStimulusType = 2;
                            indices.idxStimulusID = 3;
                            indices.idxStimulusLocation = 4;
                            indices.idxQuestionTS = 5;
                            indices.idxQuestionTS_NoPauses = -1;
                            indices.idxResponseTS = 6;
                            indices.idxResponseTS_NoPauses = -1;
                            indices.idxResponseDT = 7;
                            indices.idxUserResponse = -1;
                            indices.idxUserResponseEvaluation = 9;
                            indices.idxDifficulty = 10;
                            indices.idxAverageDifficulty = 11;
                            indices.maxIndex = 14;
                        }
                        /*
                        Probe 2
                            [0]stimulusOnsetTS, [1]levelIndex, [2]stimulusType, [3]stimulusName, [4]stimulusLocation,
                            [5]responseTS, [6]responseDT, [7]responseType, [8] difficulty, [9]avgDifficulty, 
                            [10]performance, [11]avgPerformance, [12]scorePercentile, [13]averageStarsPerLevel, [14]averageStarsInWorld,
                            [15]questionTS, [16]visibility
                        */
                        else if (revision == 2)
                        {
                            indices.idxStimOnsetTimeMS = 0;
                            indices.idxLevelID = 1;
                            indices.idxActiveLevelID = -1;
                            indices.idxStimulusType = 2;
                            indices.idxStimulusID = 3;
                            indices.idxStimulusLocation = 4;
                            indices.idxResponseTS = 5;
                            indices.idxResponseTS_NoPauses = -1;
                            indices.idxResponseDT = 6;
                            indices.idxUserResponse = -1;
                            indices.idxUserResponseEvaluation = 7;
                            indices.idxQuestionTS = 15;
                            indices.idxQuestionTS_NoPauses = -1;
                            indices.idxDifficulty = 8;
                            indices.idxAverageDifficulty = 9;
                            indices.maxIndex = 16;
                        }
                        /*
                        Probe 3
                            [0] stimulusOnsetTS         [1] dTSinceLast_NoPauses
                            [2] currentLevelID	        [3] activeLevelID	        [4] activeWorldID	        [5] activeWorldType	
                            [6] difficulty	            [7] averageDifficulty	    [8] performance	            [9] averagePerformance	
                            [10] scorePercentile	    [11]averageStarsPerLevel	[12] averageStarsInWorld	
                            [13] animCycleID            [14] type                                                                           <-- (shared with Fillers 1 up to this point)

                            [15] stimulusID	            [16] stimulusType	        [17] stimulusName	        [18] stimulusLocation       <-- (shared with Stimulus 1 up to this point)

                            [19] questionID	            [20] questionTS	            [21] windowEndTS	
                            [22] responseID	            [23] responseTS	            [24] responseDT	            
                            [25] response	            [26] responseEvaluation     [27] repliedUsingHighAccu                               <-- (shared with Localizer 2 up to this point)
                        */
                        else if (revision == 3)
                        {
                            indices.idxStimOnsetTimeMS = 0;
                            indices.idxLevelID = 2;
                            indices.idxActiveLevelID = 3;
                            indices.idxStimulusType = 16;
                            indices.idxStimulusID = 15;
                            indices.idxStimulusLocation = 18;
                            indices.idxQuestionTS = 20;
                            indices.idxQuestionTS_NoPauses = -1;
                            indices.idxResponseTS = 23;
                            indices.idxResponseTS_NoPauses = -1;
                            indices.idxResponseDT = 24;
                            indices.idxUserResponse = 25;
                            indices.idxUserResponseEvaluation = 26;
                            indices.idxDifficulty = 6;
                            indices.idxAverageDifficulty = 7;
                            indices.maxIndex = 27;
                        }
                        /*
                        Probe 4
                            [0] stimulusOnsetTS         [1] stimulusOnsetTS_NoPauses[2] dTSinceLast_NoPauses
                            [3] currentLevelID	        [4] activeLevelID	        [5] activeWorldID	        [6] activeWorldType	
                            [7] difficulty	            [8] averageDifficulty	    [9] performance	            [10] averagePerformance	
                            [11] scorePercentile	    [12]averageStarsPerLevel	[13] averageStarsInWorld	
                            [14] animCycleID            [15] type                                                                          <-- (shared with Fillers 2 up to this point)

                            [16] stimulusID	            [17] stimulusType	        [18] stimulusName	        [19] stimulusLocation      <-- (shared with Stimulus 2 up to this point)

                            [20] questionID	            [21] questionTS	            [22] questionTS_NoPauses    [23] windowEndTS        [24] windowEndTS_NoPauses
                            [25] responseID	            [26] responseTS	            [27] responseTS_NoPauses    [28] responseDT	            
                            [29] response	            [30] responseEvaluation     [31] repliedUsingHighAccu                                 <-- (shared with Localizer 3 up to this point)
                        */
                        else if (revision == 4)
                        {
                            indices.idxStimOnsetTimeMS = 0;
                            indices.idxLevelID = 3;
                            indices.idxActiveLevelID = 4;
                            indices.idxStimulusID = 16;
                            indices.idxStimulusType = 17;
                            indices.idxStimulusLocation = 19;
                            indices.idxQuestionTS = 21;
                            indices.idxQuestionTS_NoPauses = 22;
                            indices.idxResponseTS = 26;
                            indices.idxResponseTS_NoPauses = 27;
                            indices.idxResponseDT = 28;
                            indices.idxUserResponse = 29;
                            indices.idxUserResponseEvaluation = 30;
                            indices.idxDifficulty = 7;
                            indices.idxAverageDifficulty = 8;
                            indices.maxIndex = 31;
                        }
                        break;
                }

                return indices;
            }
        }

        public DetailsEntry(int indexWithinFile, string line, ParseVersionFile versionFile, Type type)
        {
            this.indexWithinFile = indexWithinFile;
            
            VersionSettings version = versionFile.versionSettings;
            Indices indices = Indices.GetIndices(type, version);

            string[] data = ParseLogEntry.FromCSVBase(line, indices.maxIndex);

            try
            {
                if (indices.idxStimOnsetTimeMS >= 0)
                    stimulusTimeMS = Utility_Helper.ToFloat_FromCSV(data[indices.idxStimOnsetTimeMS]);

                if (indices.idxLevelID >= 0)
                    levelID = data[indices.idxLevelID]; // int.Parse();

                if (indices.idxActiveLevelID >= 0)
                    activeLevelID = indices.idxActiveLevelID >= 0 ? data[indices.idxActiveLevelID] : "-1"; // int.Parse(

                if (indices.idxStimulusType >= 0)
                {
                    string stimulusTypeSTR = data[indices.idxStimulusType];
                    if (stimulusTypeSTR == "NULL")
                        stimulusType = null;
                    else
                        stimulusType = Utility_Helper.ToEnum<StimulusType>(stimulusTypeSTR);
                }

                if (indices.idxStimulusID >= 0)
                    if (data[indices.idxStimulusID] == "NULL")
                        stimulusID = -1;
                    else
                        stimulusID = int.Parse(data[indices.idxStimulusID]);

                if (indices.idxDifficulty >= 0)
                    difficulty = Utility_Helper.ToFloat_FromCSV(data[indices.idxDifficulty]);

                if (indices.idxAverageDifficulty >= 0)
                    averageDifficulty = Utility_Helper.ToFloat_FromCSV(data[indices.idxAverageDifficulty]);

                if (indices.idxStimulusLocation >= 0)
                    stimulusLocation = Utility_Helper.ToEnum<Direction_2D_Diagonal>(data[indices.idxStimulusLocation]);

                if (indices.idxQuestionTS >= 0)
                    questionTS = Utility_Helper.ToFloat_FromCSV(data[indices.idxQuestionTS]);

                if (indices.idxQuestionTS_NoPauses >= 0)
                    questionTS_NoPauses = indices.idxQuestionTS_NoPauses >= 0 ? Utility_Helper.ToFloat_FromCSV(data[indices.idxQuestionTS_NoPauses]) : -1;

                if (indices.idxResponseTS >= 0)
                    responseTS = Utility_Helper.ToFloat_FromCSV(data[indices.idxResponseTS]);

                if (indices.idxResponseTS_NoPauses >= 0)
                    responseTS_NoPauses = indices.idxResponseTS_NoPauses >= 0 ? Utility_Helper.ToFloat_FromCSV(data[indices.idxResponseTS_NoPauses]) : -1;

                if (indices.idxResponseDT >= 0)
                    responseDT = Utility_Helper.ToFloat_FromCSV(data[indices.idxResponseDT]);

                if (indices.idxUserResponseEvaluation >= 0)
                    responseEvaluation = data[indices.idxUserResponseEvaluation];

                // Older versions have a bug
                string ExFP_String = "AdditionalPress_ExFP";
                int windowLimit = 1040;

                if (version < VersionSettings.post200418 && type == Type.LocalizerResponse &&
                    responseEvaluation == "FalsePositive" && responseDT > windowLimit)
                        responseEvaluation = ExFP_String;

                if (indices.idxUserResponse >= 0)
                    response = data[indices.idxUserResponse];
                else
                {
                    if (type == Type.LocalizerResponse)
                    {
                        // If we are off window
                        if (responseDT > windowLimit)
                            response = "off-window";
                        else if (responseEvaluation.ContainsInvariant("positive"))
                            response = "yes";
                        else if (responseEvaluation.ContainsInvariant("negative"))
                            response = "no-response";
                        else if (responseEvaluation.ContainsInvariant("additional"))
                            response = "additional-press";
                        else if (responseEvaluation.ContainsInvariant("response") &&
                            responseEvaluation.ContainsInvariant("no"))
                            response = "no-response";
                    }
                    else if (type == Type.InGameProbe)
                    {
                        if (stimulusType == StimulusType.None && (
                           responseEvaluation.ContainsInvariant("truepositive") ||
                           responseEvaluation.ContainsInvariant("falsenegative")))
                            response = "error1";
                        else if (stimulusType != StimulusType.None && (
                            responseEvaluation.ContainsInvariant("falsepositive") ||
                            responseEvaluation.ContainsInvariant("truenegative")))
                            response = "error2";
                        else if (responseEvaluation.ContainsInvariant("positive"))
                            response = "yes";
                        else if (responseEvaluation.ContainsInvariant("negative"))
                            response = "no";
                        else if (responseEvaluation.ContainsInvariant("response") &&
                            responseEvaluation.ContainsInvariant("no"))
                            response = "no-response";
                    }

                    if (response == "")
                        response = responseEvaluation + "?";
                }
            }
#if UNITY_EDITOR
            catch (Exception e){
                Debug.LogError("WTF : {0}\n{1}"._Format(e, data.ToReadableString()));
            }
#else
            catch { }
#endif
        }
    }

    public struct ParseLogEntryArgs
    {
        public string world;
        public string level;
        public string fullLogState;
        public int indexWithinFile;
        public string line;
        public ParseVersionFile version;

        public ParseLogEntryArgs(string world, string level, string fullLogState, int indexWithinFile, string line, ParseVersionFile version)
        {
            this.world = world;
            this.level = level;
            this.fullLogState = fullLogState;
            this.indexWithinFile = indexWithinFile;
            this.line = line;
            this.version = version;
        }
    }

    [System.Serializable]
    public class ParseLogEntry
    {
        private static int GetNumPresenderFields(VersionSettings versionSettings)
        {
            if (versionSettings >= VersionSettings.post200926) return 4; // We added unpaused timestamp
            if (versionSettings >= VersionSettings.post200418) return 3; // We added "Next Rendered Frame" vs "As They Come" 

            return 2;
        }

        /// <summary>
        /// This is where it all starts from
        /// </summary>
        public int senderIndex;
        public string world;
        public string level;
        public string fullLogState;
        public int indexWithinFullLogs;

        public int frameID;
        // public string timeMS_Str;
        public double timeMS;
        public double timeMS_NoPauses;
        public ParseVersionFile version;
        public FullLogEntryType type; // 200705 not needed

        public string[] Create(ParseLogEntryArgs args)
        {
            version = args.version;
            int numPreSenderFields = GetNumPresenderFields(version.versionSettings);
            senderIndex = numPreSenderFields;

            world = args.world;
            level = args.level;
            fullLogState = args.fullLogState;
            indexWithinFullLogs = args.indexWithinFile;
            string[] data = FromCSVBase(args.line, numPreSenderFields);
            frameID = int.Parse(data[0]);
            timeMS = Utility_Helper.ToDouble_FromCSV(data[1]);
            timeMS_NoPauses = Utility_Helper.ToDouble_FromCSV(data[2]);
            return data;
        }

        public static string[] FromCSVBase(string line, int numFields)
        {
            char separator = ';';
            string[] data = line.Split(separator);

            if (data.Length < numFields)
            {
                Debug.LogError(string.Format("There was an error for data {0}", line));
                return null;
            }

            return data;
        }
    }

    // Example Data
    // 411;13304.3025;Interactable:Orange at Screen_Position;(0.587, 0.527);2.9659
    [System.Serializable]
    public class ParseLogEntry_Essence : ParseLogEntry
    {
        private static Regex _regex = new Regex(@"([A-Z])\w+:([A-Z])\w+");
        private static Match _regexMatch = null;

        public ObjectColorType color;
        public Vector2 position;
        public Vector2 speed; // normalized

        public ParseLogEntry_Essence(ParseLogEntryArgs baseArgs, Vector2 WIDTH_LIMITS)
        {
            string[] data = base.Create(baseArgs);
            type = FullLogEntryType.Essense;

            int index = senderIndex;

            // Capture "{Interactable:Orange} at Screen_Position"
            _regexMatch = _regex.Match(data[index++]);
            if (_regexMatch.Success)
            {
                string colorData = _regexMatch.Value;
                string _color = colorData.Split(':')[1];
                //color = _color.ToEnum<ObjectColorType>();
                color = EnumHelper.toObjectColor[_color];
            }
            else
            {
                //Debug.LogError("Couldn't parse any suitable Essense color for line" + line);
            }

            position = Utility_Helper.Vector2Parse(data[index++]);
            position.x = position.x.RetargetedFrom_01(WIDTH_LIMITS, false);

            speed = Utility_Helper.Vector2Parse(data[index]);
            speed.x = speed.x.RetargetedFrom_01(WIDTH_LIMITS, false);
        }
    }

    // Example Data
    // 407;13293.358;Input Manager;13293.358;Left;32769;[0] timestamp, [1] keyCode, [2] keyState; 6.0046
    [System.Serializable]
    public class ParseLogEntry_Input : ParseLogEntry
    {
        public KeyCode key;
        public string source;
        public enum KeyState { None, KEY_HOLD, KEY_PRESS, KEY_RELEASE }
        public KeyState keyState;

        public ParseLogEntry_Input(ParseLogEntryArgs baseArgs)
        {
            string[] data = base.Create(baseArgs);
            type = FullLogEntryType.Input;

            int index = senderIndex;
            // Skip next field - SENDER
            index++;

            source = data[index++];
            keyState = data[index++].ToEnum<KeyState>();
            key = data[index++].ToEnum<KeyCode>();
        }
    }

    // Example Data
    // 407;13293.358;Input Manager;13293.358;Left;32769;[0] timestamp, [1] keyCode, [2] keyState; 6.0046
    [System.Serializable]
    public class ParseLogEntry_EyeTracker_Gaze : ParseLogEntry
    {
        public TimestampedPosition gaze;

        //214	10133.9171	AsTheyHappen	EyeTracker	GAZE	566.2198	761.5013	10132.9202	0.9969
        public ParseLogEntry_EyeTracker_Gaze(ParseLogEntryArgs baseArgs, Vector2 WIDTH_LIMITS)
        {
            string[] data = base.Create(baseArgs);
            type = FullLogEntryType.EyeTracker_Gaze;

            gaze = TimestampedPosition.FromCSVString(data[senderIndex + 2], data[senderIndex + 3], data[senderIndex + 4]);
            gaze.position.x = gaze.position.x.RetargetedFrom_01(WIDTH_LIMITS, false);
        }
    }

    // Example Data
    // 407;13293.358;Input Manager;13293.358;Left;32769;[0] timestamp, [1] keyCode, [2] keyState; 6.0046
    [System.Serializable]
    public class ParseLogEntry_EyeTracker_Sacada : ParseLogEntry
    {
        public Sacada sacada;

        /// <summary>
        /// Negative -> Sacada ended BEFORE reference timestamp
        /// </summary>
        /// <param name="referenceTimestampMS"></param>
        /// <returns></returns>
        public double GetTemporalDistance(double referenceTimestampMS)
        {
            if (sacada == null)
                return -1;

            // The ref time is during the sacada ; 0 proximity
            if (referenceTimestampMS.IsBetween(sacada.start.timestampMS, sacada.end.timestampMS))
                return 0;

            // The sacada started AFTER the reference timestamp ?
            if (sacada.start.timestampMS > referenceTimestampMS)
                return sacada.start.timestampMS - referenceTimestampMS;

            // The sacada ended before the reference timestamp
            return sacada.end.timestampMS - referenceTimestampMS;
        }

        internal float GetSpatialDistance(Vector2 point)
        {
            return point.GetDistanceToLine(sacada.start.position, sacada.end.position);
        }

        // 263	12173.1516	AsTheyHappen	EyeTracker	SACADA	186.9169	634.1019	11464.0104	1837.319	689.1152	12172.1146	2.0076
        public ParseLogEntry_EyeTracker_Sacada(ParseLogEntryArgs baseArgs, Vector2 WIDTH_LIMITS)
        {
            string[] data = base.Create(baseArgs);
            type = FullLogEntryType.EyeTracker_Sacada;

            sacada = Sacada.FromCSVString(data[senderIndex + 2], data[senderIndex + 3], data[senderIndex + 4],
                data[senderIndex + 5], data[senderIndex + 6], data[senderIndex + 7]);

            sacada.start.position.x = sacada.start.position.x.RetargetedFrom_01(WIDTH_LIMITS, false);
            sacada.end.position.x = sacada.end.position.x.RetargetedFrom_01(WIDTH_LIMITS, false);
        }
    }

    // Example Data
    // 407;13293.358;Input Manager;13293.358;Left;32769;[0] timestamp, [1] keyCode, [2] keyState; 6.0046
    [System.Serializable]
    public class ParseLogEntry_EyeTracker_Blink : ParseLogEntry
    {
        public Blink blink;

        /// <summary>
        /// Negative -> Sacada ended BEFORE reference timestamp
        /// </summary>
        /// <param name="referenceTimestampMS"></param>
        /// <returns></returns>
        public double GetTemporalDistance(double referenceTimestampMS)
        {
            if (blink == null)
                return -1;

            // The ref time is during the sacada ; 0 proximity
            if (referenceTimestampMS.IsBetween(blink.startTimestampMS, blink.endTimestampMS))
                return 0;

            // The sacada started AFTER the reference timestamp ?
            if (blink.startTimestampMS > referenceTimestampMS)
                return blink.startTimestampMS - referenceTimestampMS;

            // The sacada ended before the reference timestamp
            return blink.endTimestampMS - referenceTimestampMS;
        }

        // 214	10133.9171	AsTheyHappen	EyeTracker	BLINK	Left	9583.5032	10133.9171	0.9969
        public ParseLogEntry_EyeTracker_Blink(ParseLogEntryArgs baseArgs)
        {
            string[] data = base.Create(baseArgs);
            type = FullLogEntryType.EyeTracker_Blink;

            blink = Blink.FromCSVString(data[senderIndex + 2], data[senderIndex + 3], data[senderIndex + 4]);
        }
    }

    // Example Data
    // 407;13293.358;Input Manager;13293.358;Left;32769;[0] timestamp, [1] keyCode, [2] keyState; 6.0046
    [System.Serializable]
    public class ParseLogEntry_Trigger : ParseLogEntry
    {
        public string sender = "";
        public string triggerState = "";
        public string triggerEvent = "";
        public string triggerCode = "";

        public ParseLogEntry_Trigger(ParseLogEntryArgs baseArgs)
        {
            string[] data = base.Create(baseArgs);
            type = FullLogEntryType.Trigger;

            int index = senderIndex;

            sender = data[index++];
            triggerState = data[index++];

            triggerEvent = data[index++];

            // Central and photodiode do not have codes
            if (sender == "TRIGGER_MANAGER_CENTRAL" || sender == "TRIGGER_MANAGER_PHOTODIODE")
            {
                triggerCode = "-";
                index++;
            }
            else
            {
                // Handle eye tracker's prepend
                if (sender == "TRIGGER_MANAGER_EYETRACKING")       // Eye Tracker
                    if (data[index].ToInt() < 0 &&                      // Not INT
                        !(data[index].Split('_').Length == 2 &&     // Not INT_INT either
                        data[index].Split('_')[0].ToInt() >= 0 &&
                        data[index].Split('_')[1].ToInt() >= 0))

                        index++; // Skip prepend

                triggerCode = data[index++];
            }
        }
    }

    // Example Data
    // 411;13304.3025;Rotating_Square-11788;(0.647, 0.647);0;0.7940612;0.4;1;position-rotation-scale-brightness-alpha;0.9977
    [System.Serializable]
    public class ParseLogEntry_Background : ParseLogEntry
    {
        private static Regex _regex = new Regex(@"[0-9]");
        private static Match _regexMatch = null;

        public int id;
        public double timestampMS;
        public Vector2 screenPosition;
        public float rotation;
        public float scale;
        public float brightness;
        public float alpha;

        public ParseLogEntry_Background(ParseLogEntryArgs baseArgs, Vector2 WIDTH_LIMITS)
        {
            string[] data = base.Create(baseArgs);
            type = FullLogEntryType.Background;

            int index = senderIndex;

            // 685;24538.1818;Rotating_Square-92386;(-0.942, -0.300);0;1;0.3334398;0;position-rotation-scale-brightness-alpha

            // Capture "Rotating_Square-{11788}"
            _regexMatch = _regex.Match(data[index++]);
            if (_regexMatch.Success)
                this.id = int.Parse(_regexMatch.Value);
            else
                Debug.LogError("Couldn't parse any suitable Essense color for line" + baseArgs.line);

            this.screenPosition = Utility_Helper.Vector2Parse(data[index++]);
            screenPosition.x = screenPosition.x.RetargetedFrom_01(WIDTH_LIMITS, false);

            this.rotation = Utility_Helper.ToFloat_FromCSV(data[index++]);
            this.scale = Utility_Helper.ToFloat_FromCSV(data[index++]);
            this.brightness = Utility_Helper.ToFloat_FromCSV(data[index++]);
            this.alpha = Utility_Helper.ToFloat_FromCSV(data[index]);
        }
    }
    public enum Triary { UNKNOWN = -1, FALSE = 0, TRUE = 1}

    // Example Data
    // 2459;37547.8631;Stimulus Manager;SHOWING_STIMULUS;1_1_4;Object(20);(-2.6, 3.5, 0.0);(0.4, 0.4);[0] worldLevelTrial, [1] stimulusTypeName, [2] worldPosition, [3] screenPosition;16.9511
    // 2490;38070.576;Stimulus Manager; HIDING_STIMULUS;Object(20); (-2.6, 3.5, 0.0);(0.4, 0.4);[0] stimulusTypeName, [1] worldPosition, [2] screenPosition;16.9511
    [System.Serializable]
    public class ParseLogEntry_Stimulus : ParseLogEntry
    {
        private static Regex _regexType = new Regex(@"[A-Z]\w+");
        private static Match _regexMatchType = null;

        private static Regex _regexName = new Regex(@"[0-9]+");
        private static Match _regexMatchName = null;

        private static Regex _regexID = new Regex(@"([A-Z0-9])+_[0-9]+_[0-9]+");
        private static Match _regexMatchID = null;

        [System.Serializable]
        public enum StimLogType { Showing, Hiding }

        public StimLogType logStimType;
        public StimulusType stimType;
        public string stimName;
        public string stimID;
        public Vector2 screenPosition;
        public Direction_2D_Diagonal direction;
        public bool isProbe;
        public double offsetTS = -1;
        public double offsetTS_NoPauses = -1;
        public double difficulty = -1;
        public double averageDifficulty = -1;
        public double probeTS = -1;
        public double probeTS_NoPauses = -1;
        public string response = "-";
        public string responseEvaluation = "-";
        public double responseTS = -1;
        public double responseTS_NoPauses = -1;
        public double response_Difficulty = -1;
        public double response_AvgDifficulty = -1;

        public int detailIndexWithinFile = -1;

        public ParseLogEntry_Stimulus(ParseLogEntryArgs baseArgs, Vector2 WIDTH_LIMITS,
            ParseDetailsFile stimulusDetailsFile,
            ParseDetailsFile probeDetailsFile, ParseDetailsFile localizerDetailsFile)
        { 
            string[] data = base.Create(baseArgs);
            type = FullLogEntryType.Stimulus;

            VersionSettings version = baseArgs.version.versionSettings;

            bool isLocalizer = false;

            int index = senderIndex;

            // Skip 3rd field - as sender
            index++;

            int showHideIndex = index++;
            if (data[showHideIndex].Contains("HIDING_STIMULUS"))
            {
                logStimType = StimLogType.Hiding;
            }
            else if (data[showHideIndex].Contains("SHOWING_STIMULUS"))
            {
                logStimType = StimLogType.Showing;

                // Capture "1_1_4"
                _regexMatchID = _regexID.Match(data[index++]);
                if (_regexMatchID.Success)
                {
                    stimID = _regexMatchID.Value;// int.Parse(_regexMatchID.Value);
                    isLocalizer = stimID.Contains("L"); // localizers
                }
                else
                    Debug.LogError("Couldn't parse any suitable Stimulus ID for data:" + data[index - 1]);
            }

            // Capture "{Object}(20)"
            _regexMatchType = _regexType.Match(data[index]);
            if (_regexMatchType.Success)
                stimType = EnumHelper.toStimulusType[_regexMatchType.Value];
            else
                Debug.LogError("Couldn't parse any suitable Stimulus Type for data:" + data[index]);

            if (stimType != StimulusType.None)
            {
                _regexMatchName = _regexName.Match(data[index]);
                if (_regexMatchName.Success)
                    stimName = _regexMatchName.Value;
                else
                    Debug.LogError("Couldn't parse any suitable Stimulus Name for data:" + data[index]);
            }
            else
                stimName = "-";

            index++;

            // Skip World Position
            index++;

            this.screenPosition = Utility_Helper.Vector2Parse(data[index++]);
            direction = (screenPosition - Vector2.one / 2f).ToClosestD2D_Diagonal(); // Calculate before scaling

            screenPosition.x = screenPosition.x.RetargetedFrom_01(WIDTH_LIMITS, false);

            // Debug.Log(screenPosition + " : " + direction);
            // Maintain backward compatibility
            isProbe = false;

            // For newer versions, we know if it was a probe or not already from full logs
            // For older versions, we need to wait until the next field to figure out if it's a probe or not
            bool versionPostProbedLogging = version >= VersionSettings.post191213;
            if (versionPostProbedLogging)
                isProbe = bool.Parse(data[index++]);

            bool versionPostOnsetLogging = version >= VersionSettings.post200301;
            if (versionPostOnsetLogging)
                timeMS = Utility_Helper.ToFloat_FromCSV(data[index++]);

            int maxOffsetToleranceMS = 500;

            DetailsEntry stimDetailEntry = null;

            if (stimulusDetailsFile == null)
            {
                Debug.LogError("Missing stimulus details!");
            }
            else
                stimulusDetailsFile.TryGetEntryNearTime(timeMS, maxOffsetToleranceMS, out stimDetailEntry);

            if (stimDetailEntry != null)
            {
                difficulty = stimDetailEntry.difficulty;
                averageDifficulty = stimDetailEntry.averageDifficulty;
            }

            // Unless we know it's not a probe, try
            string defResponse = "N/A";
            response = "";
            responseEvaluation = "";

            DetailsEntry probe = null;

            ParseDetailsFile relevantProbeDetailsFile = isLocalizer ? localizerDetailsFile : probeDetailsFile;

            if (relevantProbeDetailsFile == null)
                response = responseEvaluation = isLocalizer ? "LocalizerDetailsMissing" : "ProbeDetailsMissing";
            // For OLD versions, we need to check for all stimuli
            else if (!versionPostProbedLogging)
                isProbe = relevantProbeDetailsFile.TryGetEntryNearTime(timeMS, maxOffsetToleranceMS, out probe) && !isLocalizer; // Only non localizers can be probes
            // For NEW versions we only check probed stimuli & Localizers
            else if (isProbe || isLocalizer)
                relevantProbeDetailsFile.TryGetEntryNearTime(timeMS, maxOffsetToleranceMS, out probe);
            // If it's not a probe neither a localizer, don't expect a response
            else if (!isProbe && !isLocalizer)
                response = defResponse;
            // That should never happen
            else
                response = "UnknownError";

            if (probe != null)
            {
                response = probe.response;
                responseEvaluation = probe.responseEvaluation;
                detailIndexWithinFile = probe.indexWithinFile;
                responseTS = probe.responseTS;
                responseTS_NoPauses = probe.responseTS_NoPauses;
                probeTS = probe.questionTS;
                probeTS_NoPauses = probe.questionTS_NoPauses;
                response_Difficulty = probe.difficulty;
                response_AvgDifficulty = probe.averageDifficulty;
            }
            else
                response = defResponse;
        }
    }

    // Example Data
    // 1914;28364.5834;Player;Player collided with interactable:Blue;16.944
    [System.Serializable]
    public class ParseLogEntry_Collision : ParseLogEntry
    {
        public ObjectColorType color;

        public ParseLogEntry_Collision(ParseLogEntryArgs baseArgs)
        {
            string[] data = base.Create(baseArgs);
            type = FullLogEntryType.Collision;

            int index = senderIndex;

            // Skip 3rd field - as sender
            index++;

            // Capture "Player collided with {Interactable:Blue}"
            Match regexMatch = Regex.Match(data[index++], @"\w+:\w+");// @"([A-Z])\w+:([A-Z])\w+");
            if (regexMatch.Success)
            {
                string colorData = regexMatch.Value;
                string _color = colorData.Split(':')[1];
                //color = _color.ToEnum<ObjectColorType>();
                color = EnumHelper.toObjectColor[_color];
            }
            else
            {
                Debug.LogError("Couldn't parse any suitable Essense color for line" + baseArgs.line);
            }
        }
    }

    // Example Data
    // 3397;119062.8766;SubjectPerformanceReport;FRAME_BEGIN;3398;119012.9765;33.9098
    [System.Serializable]
    public class ParseLogEntry_FrameRendered : ParseLogEntry
    {
        public ParseLogEntry_FrameRendered(ParseLogEntryArgs baseArgs)
        {
            string[] data = base.Create(baseArgs);
            type = FullLogEntryType.FrameRendered;
        }
    }

    // Example Data
    // 411;13304.3025;Interactable:Orange at Screen_Position;(0.587, 0.527);2.9659
    [System.Serializable]
    public class ParseLogEntry_Player : ParseLogEntry
    {
        //public WorldType color;
        public Vector2 position; // normalized

        public ParseLogEntry_Player(ParseLogEntryArgs baseArgs, Vector2 WIDTH_LIMITS)
        {
            string[] data = base.Create(baseArgs);
            type = FullLogEntryType.Player;

            int index = senderIndex;

            // Skip 3rd field - as sender
            index++;

            this.position = Utility_Helper.Vector2Parse(data[index]);
            position.x = position.x.RetargetedFrom_01(WIDTH_LIMITS, false);
        }
    }

    [System.Serializable]
    public class ParseLogEntry_GameStart : ParseLogEntry
    {
        private static int GetWorldTypeOffset(VersionSettings versionSettings)
        {
            if (versionSettings >= VersionSettings.post200418) return 2;
            return 1;
        }

        //public WorldType color;
        public WorldType worldType; // normalized

        public ParseLogEntry_GameStart(ParseLogEntryArgs baseArgs)
        {
            string[] data = base.Create(baseArgs);
            type = FullLogEntryType.GameStart;

            int worldTypeIndex = senderIndex + GetWorldTypeOffset(baseArgs.version.versionSettings);

            //this.world = data[index].ToEnum<WorldType>();
            this.worldType = EnumHelper.toWorldType[data[worldTypeIndex]];
        }
    }

    public static class EnumHelper
    {
        public static Dictionary<string, ObjectColorType> toObjectColor;
        public static Dictionary<string, WindowsFormKeys> toWindowsFormKeys;
        public static Dictionary<string, StimulusType> toStimulusType;
        public static Dictionary<string, WorldType> toWorldType;

        public static void Initialize()
        {
            toObjectColor = CreateDictionary<ObjectColorType>();
            toWindowsFormKeys = CreateDictionary<WindowsFormKeys>();
            toStimulusType = CreateDictionary<StimulusType>();
            toWorldType = CreateDictionary<WorldType>();
        }

        public static Dictionary<string, T> CreateDictionary<T>()
        {
            Dictionary<string, T> dict = new Dictionary<string, T>();
            foreach (T enumVal in Enum.GetValues(typeof(T)))
            {
                dict[enumVal.ToString()] = enumVal;
            }
            return dict;
        }
    }
}