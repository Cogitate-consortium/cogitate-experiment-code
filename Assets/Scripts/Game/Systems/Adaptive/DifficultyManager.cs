using Helpers.Misc;
using System.Collections.Generic;
using TGP.Helpers;
using TGP.Helpers.Filters;
using UnityEngine;
using Helpers.Engine;
using Game.Core;
using System;

namespace Game.Systems.Adaptive
{
    /// <summary>
    /// [CLEANUP] (Most wont need to be static, but some will)
    /// </summary>
    public class DifficultyManager
    {
        public static float dPrime_Final { get; private set; }
        public static float difficulty { get; private set; }
        private static Config config;

        // Per PLAYER (Need to be static!!)
        private readonly List<TimeStamped<float>> goodEssencesCollectedLog = new List<TimeStamped<float>>();
        private readonly List<TimeStamped<float>> goodEssencesMissedLog = new List<TimeStamped<float>>();
        private readonly List<TimeStamped<float>> badEssencesHitLog = new List<TimeStamped<float>>();
        private readonly List<TimeStamped<float>> badEssencesAvoidedLog = new List<TimeStamped<float>>();

        private readonly List<TimeStamped<float>> undamagedIncrementLog = new List<TimeStamped<float>>();
        private readonly List<TimeStamped<float>> deathIncrementLog = new List<TimeStamped<float>>();

        // private static RunningAverage AVG_DIFFICULTY_DIFF = new RunningAverage(0.5f, 1f);
        // {x, y} : {obsrvation, weight}
        private static Dictionary<float, Vector2> dPrimeMomentaryHistory_currentLevel = new Dictionary<float, Vector2>();
        private static Dictionary<float, Vector2> dPrimeFinalHistory_currentLevel = new Dictionary<float, Vector2>();
        private static Dictionary<float, Vector2> difficultyHistory_currentLevel = new Dictionary<float, Vector2>();
        private static int numLevelsPlayed { get { return dPrimeFinalHistory_overall.Count; } } // Could be any of the dictionaries below
        private static Dictionary<int, Vector2> dPrimeMomentaryHistory_overall = new Dictionary<int, Vector2>();
        private static Dictionary<int, Vector2> dPrimeFinalHistory_overall = new Dictionary<int, Vector2>();
        private static Dictionary<int, Vector2> difficultyHistory_overall = new Dictionary<int, Vector2>();

        // private float baseLevelDifficulty { get { return runtimeConfig.baseLevelDifficulty; } }
        // private float flatIncreaseDifficulty { get { return AVG_DIFFICULTY_DIFF.value; } }

        /// <summary>
        /// Called from <see cref="GameManager"/> when using absorb powerups
        /// </summary>
        public bool preventLogPositive = false;

        public static void Initialize(Config config)
        {
            DifficultyManager.config = config;
        }

        private static float Count_TimeDiscounted(List<TimeStamped<float>> incrementLog, float windowLength, float referenceTime)
        {
            float eval = 0;

            foreach (TimeStamped<float> logEntry in incrementLog)
            {
                float timeDiff = logEntry.GetTimeSinceLastUpdate(referenceTime);

                // The closer we are to window length, the less weight we have
                // Linear
                float weight01 = 1 - timeDiff.RetargetedTo_01(0, windowLength);

                // Square it!
                if (config.discountType == Config.DiscountType.Quadratic)
                    weight01 *= weight01;
                // Or ignore it
                else if (config.discountType == Config.DiscountType.None)
                    weight01 = 1;

                if (weight01 <= 0) continue;

                eval += logEntry.value * weight01;
            }

            return eval;
        }

        public static float GetAverageDPrimeMomentary_CurrentLevel()
        {
            return GetAverage(dPrimeMomentaryHistory_currentLevel, config.dPrimeAtStartOfLevel);
        }

        public static float GetAverageDPrimeFinal_CurrentLevel()
        {
            return GetAverage(dPrimeFinalHistory_currentLevel, config.dPrimeAtStartOfLevel);
        }

        public static float GetAverageDifficulty_CurrentLevel()
        {
            return GetAverage(difficultyHistory_currentLevel, config.initialDifficulty);
        }

        public static float GetAverageDPrimeMomentary_Overall()
        {
            return GetAverage(dPrimeMomentaryHistory_overall, config.dPrimeAtStartOfLevel);
        }

        public static float GetAverageDPrimeFinal_Overall()
        {
            return GetAverage(dPrimeFinalHistory_overall, config.dPrimeAtStartOfLevel);
        }

        public static float GetAverageDifficulty_Overall()
        {
            float average = GetAverage(difficultyHistory_overall, config.initialDifficulty);
            /*
            Debug.LogError(average.PercentileToPercent());
            foreach (KeyValuePair<int, Vector2> kVP in difficultyHistory_overall)
                Debug.LogError("{0} :: {1}"._Format(kVP.Value.x.PercentileToPercent(), kVP.Value.y.ToString("#")));
            */
            return average;
        }

        private static float GetAverage<T>(Dictionary<T, Vector2> dic, float defValue)
        {
            if (dic == null || dic.Count == 0) return defValue;

            float weight = 0;
            float sum = 0;
            foreach (KeyValuePair<T, Vector2> kVP in dic)
            {
                sum += kVP.Value.x * kVP.Value.y;
                weight += kVP.Value.y;
            }

            return sum / weight;
        }

        private int currentLevelID_0Based { get { return runtimeConfig.level.levelID_1Based - 1; } }

        /// Called at the start of level 
        public void StartLevel()
        {
            dPrimeMomentaryHistory_currentLevel.Clear();
            dPrimeFinalHistory_currentLevel.Clear();
            difficultyHistory_currentLevel.Clear();
        }

        private RuntimeConfig runtimeConfig;

        public class RuntimeConfig
        {
            public LevelConfig level;
            public bool doCalculatePerformance;
            public float windowLength;
            public float numLevels;
            // public float baseLevelDifficulty;

            public RuntimeConfig(LevelConfig level, bool doCalculatePerformance, float windowLength, float numLevels)//, float baseLevelDifficulty)
            {
                this.level = level;
                this.doCalculatePerformance = doCalculatePerformance;
                this.windowLength = windowLength;
                this.numLevels = numLevels;
                // this.baseLevelDifficulty = baseLevelDifficulty;
            }
        }

        public void EndLevel(float elapsedTimeSeconds_Level_NoPauses)
        {
            // Debug.LogError(currentLevelID_0Based);
            if (config.averageUpdateMode == Config.AverageUpdateMode.EndOfLevel)
            {
                dPrimeMomentaryHistory_overall.AddOrUpdate(currentLevelID_0Based, new Vector2(GetAverageDPrimeMomentary_CurrentLevel(), elapsedTimeSeconds_Level_NoPauses));
                dPrimeFinalHistory_overall.AddOrUpdate(currentLevelID_0Based, new Vector2(GetAverageDPrimeFinal_CurrentLevel(), elapsedTimeSeconds_Level_NoPauses));
                difficultyHistory_overall.AddOrUpdate(currentLevelID_0Based, new Vector2(GetAverageDifficulty_CurrentLevel(), elapsedTimeSeconds_Level_NoPauses));
            }
        }

        public static void SubjectReset()
        {
            // Subject starts - set perf to 0.7
            dPrime_Final = Mathf.Lerp(config.targetDPrime_Lower, config.targetDPrime_Upper, 0.5f);
            //AVG_DIFFICULTY_DIFF.Reset(0);

            dPrimeMomentaryHistory_overall.Clear();
            dPrimeFinalHistory_overall.Clear();
            difficultyHistory_overall.Clear();
        }

        public DifficultyManager(RuntimeConfig runtimeConfig)
        {
            this.runtimeConfig = runtimeConfig;

            //if (AVG_DIFFICULTY_DIFF.runningAverage.IsNaN())
            //    AVG_DIFFICULTY_DIFF.Reset(0);

            Reset(true);
        }

        /*
        private List<float> tempKeysToRemove_Performance = new List<float>();
        private float[] tempCachedKeys_Performance;

        private void RemoveOldPerformanceEntries()
        {
            tempKeysToRemove_Performance.Clear();
            tempCachedKeys_Performance = performanceHistory.Keys.ToArray();
            float time = Time.time;
            //foreach (float key in difficultyHistory.Keys)
            for (int i = 0; i < tempCachedKeys_Performance.Length; i++)
            {
                if (time - tempCachedKeys_Performance[i] > config.evaluationWindow_NumAnimCycles)
                    tempKeysToRemove_Performance.Add(tempCachedKeys_Performance[i]);
            }

            for (int i = 0; i < tempKeysToRemove_Performance.Count; i++)
                difficultyHistory.Remove(tempKeysToRemove_Performance[i]);
        }

        private List<float> tempKeysToRemove_Difficulty = new List<float>();
        private float[] tempCachedKeys_Difficulty;

        private void RemoveOldDifficultyEntries()
        {
            tempKeysToRemove_Difficulty.Clear();
            tempCachedKeys_Difficulty = difficultyHistory.Keys.ToArray();
            float time = Time.time;
            //foreach (float key in difficultyHistory.Keys)
            for (int i = 0; i < tempCachedKeys_Difficulty.Length; i++)
            {
                if (time - tempCachedKeys_Difficulty[i] > config.evaluationWindow_NumAnimCycles)
                    tempKeysToRemove_Difficulty.Add(tempCachedKeys_Difficulty[i]);
            }

            for (int i = 0; i < tempKeysToRemove_Difficulty.Count; i++)
                difficultyHistory.Remove(tempKeysToRemove_Difficulty[i]);
        }
        */

        private float CalculateMomentaryDPrime(float timeSinceStart)
        {
            float earlyLevelPerformanceAnchor = config.dPrimeAtStartOfLevel;

            if (!runtimeConfig.doCalculatePerformance)
                return earlyLevelPerformanceAnchor;

            bool debug = false;


            float windowLength = runtimeConfig.windowLength;

            // In that window length, total number of good essences collected?
            float numGoodEssencesCollected = Count_TimeDiscounted(goodEssencesCollectedLog, windowLength, timeSinceStart);// goodEssencesCollectedLog.Count; // True Positive (Was good and I hit it)
            float numGoodEssencesMissed = Count_TimeDiscounted(goodEssencesMissedLog, windowLength, timeSinceStart);//  goodEssencesMissedLog.Count; // False Negative (was good but I missed it)
            float numBadEssencesAvoided = Count_TimeDiscounted(badEssencesAvoidedLog, windowLength, timeSinceStart);// badEssencesAvoidedLog.Count; // True Negative (was bad and I missed it)
            float numBadEssencesHit = Count_TimeDiscounted(badEssencesHitLog, windowLength, timeSinceStart);// badEssencesHitLog.Count;        // False Positive (was bad but I hit it)

            float numGoodEssencesTotal = numGoodEssencesCollected + numGoodEssencesMissed;
            float numBadEssencesTotal = numBadEssencesAvoided + numBadEssencesHit;

            float totalNumEssences = numGoodEssencesTotal + numBadEssencesTotal;

            // If we don't have any readings yet just return default
            if (numGoodEssencesTotal == 0 || numBadEssencesTotal == 0) return earlyLevelPerformanceAnchor;

            // Calculating D-Prime (https://psychology.stackexchange.com/a/9283)
            // http://phonetics.linguistics.ucla.edu/facilities/statistics/dprime.htm
            float H = (float)numGoodEssencesCollected / numGoodEssencesTotal;   // H = P (yes|present) = P (hit|good)
            float FA = (float)numBadEssencesHit / numBadEssencesTotal;         // FA = P (yes|absent) = P (hit|bad)

            // Go from [0..1] (ie. with μ = 0.5, σ = 1/6) to a standard normal distribution (μ = 0, σ = 1)
            float dPrime = (float)DPrime_Helper.DPrime_H_FAs(H, FA);

            // Smoothen out performance until we have enough samples
            float numEssencesThisLevel = goodEssencesCollectedLog.Count + goodEssencesMissedLog.Count + badEssencesAvoidedLog.Count + badEssencesHitLog.Count;
            float performanceCoef = (numEssencesThisLevel / config.minEssencesToTrustCurrentPerformance).Clamped01();

            float dPrime_Smoothened = Mathf.Lerp(earlyLevelPerformanceAnchor, dPrime, performanceCoef);

            if (debug)
            {
                this.LogWarning("Good Collected :: {0} || Good Missed :: {1} || Bad Avoided :: {2} || Bad Hit :: {3} || Total :: {4}"._Format(
                    numGoodEssencesCollected, numGoodEssencesMissed, numBadEssencesAvoided, numBadEssencesHit, totalNumEssences));

                this.LogWarning("H :: {0} || FA :: {1} || d-prime :: {2}"._Format( // || zH :: {2} || zFA :: {3} 
                    H, FA, dPrime)); // zH, zFA, 

                this.LogWarning("Smoothened d-Prime :: {0} || Ready for Current :: {1} || Current :: {2}"._Format(
                    dPrime_Smoothened.PercentileToPercent(), performanceCoef.PercentileToPercent(), dPrime.PercentileToPercent()));
            }

            return dPrime_Smoothened;
        }

        public void UpdateDifficulty(float timeSinceStart, float dT, bool useOverridePerformance, float elapsedTimeSeconds_Level_NoPauses)
        {
            float dPrime_Momentary;

            // In replay levels, ignore this part
            if (!useOverridePerformance)
            {
                dPrime_Momentary = CalculateMomentaryDPrime(timeSinceStart);

                float dPrime_Average =
                    config.averageSource == Config.AverageSource.Momentary ? GetAverageDPrimeMomentary_Overall() :
                    config.averageSource == Config.AverageSource.Final ? GetAverageDPrimeFinal_Overall() : 0;

                float levelsPlayed01 = numLevelsPlayed / (float)runtimeConfig.numLevels;

                float lerp01 = 0.5f;

                // Factor in Older d-primes       
                switch (config.weightOfAveragevsCurrentPerLevel)
                {
                    case Config.WeightOfAverageVsCurrentPerLevel.CurrentOnly:
                        lerp01 = 0; // Running only
                        break;
                    case Config.WeightOfAverageVsCurrentPerLevel.AverageOnly:
                        lerp01 = 1; // Average only
                        break;
                    case Config.WeightOfAverageVsCurrentPerLevel.Static:
                        lerp01 = Mathf.Lerp(config.weightPerLevel_MinMax.min, config.weightPerLevel_MinMax.max, 0.5f);
                        break;
                    case Config.WeightOfAverageVsCurrentPerLevel.Linear:
                        lerp01 = Mathf.Lerp(config.weightPerLevel_MinMax.min, config.weightPerLevel_MinMax.max, levelsPlayed01);
                        break;
                    case Config.WeightOfAverageVsCurrentPerLevel.Sqrt:
                        lerp01 = Mathf.Lerp(config.weightPerLevel_MinMax.min, config.weightPerLevel_MinMax.max, Mathf.Sqrt(levelsPlayed01));
                        break;
                    case Config.WeightOfAverageVsCurrentPerLevel.Quadratic:
                        lerp01 = Mathf.Lerp(config.weightPerLevel_MinMax.min, config.weightPerLevel_MinMax.max, Mathf.Pow(levelsPlayed01, 2));
                        break;
                }

                dPrime_Final = Mathf.Lerp(dPrime_Momentary, dPrime_Average, lerp01);
            }
            else
                dPrime_Momentary = dPrime_Final;

            switch (config.adaptationMode)
            {
                case Config.AdaptationMode.Debug:
                    difficulty = Input.mousePosition.x / Screen.width;
                    break;
                case Config.AdaptationMode.None:
                    break;
                case Config.AdaptationMode.Direct:
                    difficulty = dPrime_Final.RetargetedTo_01(config.dPrimeToDifficulty0, config.dPrimeToDifficulty1);
                    break;
                case Config.AdaptationMode.Indirect:
                    // Check the deviation
                    float deviation =
                        dPrime_Final < config.targetDPrime_Lower ? dPrime_Final - config.targetDPrime_Lower :
                        dPrime_Final > config.targetDPrime_Upper ? dPrime_Final - config.targetDPrime_Upper : 0;

                    float step = 0;

                    if (deviation != 0)
                        switch (config.indirectType)
                        {
                            case Config.IndirectType.Sign:
                                step = Mathf.Sign(deviation);
                                break;
                            case Config.IndirectType.Linear:
                                step = deviation;
                                break;
                            case Config.IndirectType.Sqrt:
                                step = Mathf.Sign(deviation) * Mathf.Sqrt(Mathf.Abs(deviation)); // Smoothen deviation because dprime is not normalized 
                                break;
                        }

                    float delta = step * dT / (step > 0 ? config.adaptMinToMax_Seconds : config.adaptMaxToMin_Seconds);

                    difficulty += delta;
                    // Debug.LogError(difficulty.PercentileToPercent());
                    break;
            }

            /*
            if (config.preventDiffAwayFromPerf)
                difficulty = Mathf.Min(Mathf.Max(difficulty, config.minDiffBelowPerf * dPrime), config.maxDiffAbovePerf * dPrime);
            */

            difficulty = Mathf.Min(Mathf.Max(difficulty, 0), 1);

            // Debug.Log("{0} :: {1}"._Format(performance.PercentileToPercent(), difficulty.PercentileToPercent()));

            //float difficultyDiff = difficulty - baseLevelDifficulty;
            //AVG_DIFFICULTY_DIFF.AddAndQuery(difficultyDiff, dT);

            dPrimeMomentaryHistory_currentLevel.Add(TimeWrapper.realtimeSinceStartup_NotTS, new Vector2(dPrime_Momentary, TimeWrapper.deltaTime_SinceLastUpdate_NotTS));
            dPrimeFinalHistory_currentLevel.Add(TimeWrapper.realtimeSinceStartup_NotTS, new Vector2(dPrime_Final, TimeWrapper.deltaTime_SinceLastUpdate_NotTS));
            difficultyHistory_currentLevel.Add(TimeWrapper.realtimeSinceStartup_NotTS, new Vector2(difficulty, TimeWrapper.deltaTime_SinceLastUpdate_NotTS));

            if (config.averageUpdateMode == Config.AverageUpdateMode.Realtime)
            {
                dPrimeMomentaryHistory_overall.AddOrUpdate(currentLevelID_0Based, new Vector2(GetAverageDPrimeFinal_CurrentLevel(), elapsedTimeSeconds_Level_NoPauses));
                dPrimeFinalHistory_overall.AddOrUpdate(currentLevelID_0Based, new Vector2(GetAverageDPrimeFinal_CurrentLevel(), elapsedTimeSeconds_Level_NoPauses));
                difficultyHistory_overall.AddOrUpdate(currentLevelID_0Based, new Vector2(GetAverageDifficulty_CurrentLevel(), elapsedTimeSeconds_Level_NoPauses));
            }

            // RemoveOldDifficultyEntries();
            // RemoveOldPerformanceEntries();
        }

        /// <summary>
        /// [SOS] Only use this from localizers - when the performance is not being calculated, but replayed
        /// </summary>
        public static void SetOverridePerformance_LocalizerOnly(float p)
        {
            if (p.IsNaN()) return;
            // dPrime = Mathf.Min(1, Mathf.Max(0, p));
            dPrime_Final = Mathf.Min(4, Mathf.Max(-4, p));
        }

        // Reset score when player dies
        public void ResetScore()
        {
            goodEssencesCollectedLog.Clear();
        }

        public void Reset(bool resetScore = false)
        {
            goodEssencesCollectedLog.Clear();
            goodEssencesMissedLog.Clear();
            badEssencesHitLog.Clear();
            badEssencesAvoidedLog.Clear();

            deathIncrementLog.Clear();
            undamagedIncrementLog.Clear();

            if (resetScore)
                difficulty = GetAverageDifficulty_Overall();// config.initialDifficulty;
        }

        public void LogGoodEssenceCollect(float health, float timeSinceStart)
        {
            if (preventLogPositive)
            {
                this.Log("Not LOGGING good essence collected (powerup)");
                return;
            }

            goodEssencesCollectedLog.Add(new TimeStamped<float>(health, timeSinceStart));
        }

        public void LogGoodEssenceMissed(float health, float timeSinceStart)
        {
            goodEssencesMissedLog.Add(new TimeStamped<float>(health, timeSinceStart));
        }

        public void LogBadEssenceHit(float damage, float timeSinceStart)
        {
            // undamagedIncrementLog.Clear();
            badEssencesHitLog.Add(new TimeStamped<float>(damage, timeSinceStart));
        }

        public void LogBadEssenceAvoided(float damage, float timeSinceStart)
        {
            badEssencesAvoidedLog.Add(new TimeStamped<float>(damage, timeSinceStart));
        }

        public void LogUndamagedIncrement(float timeSinceStart)
        {
            undamagedIncrementLog.Add(new TimeStamped<float>(1, timeSinceStart));
        }

        public void LogDeathIncrement(float timeSinceStart)
        {
            undamagedIncrementLog.Clear();
            deathIncrementLog.Add(new TimeStamped<float>(1, timeSinceStart));
        }

        /*
        void Update()
        {
            int numChoice = Input_Helper.GetNumericalChoiceUp();
            if (numChoice >= 1)
                SetOverrideDifficulty(((float)numChoice).RetargetedTo_01(1f, 9f));
            else if (numChoice == 0)
                SetOverrideDifficulty(-1);
        }
        */

        [Serializable]
        public class Config
        {
            // INITIAL STUFF
            public float initialDifficulty = 0.5f;
            public int minEssencesToTrustCurrentPerformance = 100;
            public float dPrimeAtStartOfLevel = 0;

            // WINDOW
            public DiscountType discountType = DiscountType.Linear;

            // ADAPTATION
            public string adaptationMode_Comment = "Debug = -1, None = 0, Direct = 1, Indirect = 2";
            public AdaptationMode adaptationMode = AdaptationMode.Direct;

            // DIRECT
            public float dPrimeToDifficulty0 = -1;
            public float dPrimeToDifficulty1 = 1;

            // INDIRECT
            public IndirectType indirectType = IndirectType.Sign;
            public float targetDPrime_Lower = 1.75f;
            public float targetDPrime_Upper = 2.25f;
            public float adaptMinToMax_Seconds = 15;
            public float adaptMaxToMin_Seconds = 20;

            // AVERAGE CALC
            public AverageUpdateMode averageUpdateMode = AverageUpdateMode.Realtime;
            public WeightOfAverageVsCurrentPerLevel weightOfAveragevsCurrentPerLevel = WeightOfAverageVsCurrentPerLevel.CurrentOnly;
            public MinMax weightPerLevel_MinMax = new MinMax(0.3f, 0.7f);

            public enum AverageSource { Momentary = 0, Final }
            public string averageSourceComment = "The d' to be used when calculating average d' (0: Momentary d', 1: Final d')";
            public AverageSource averageSource = AverageSource.Momentary;

            /*
            public float incSpeedAfter = 0.9f;
            public float maxSpeedBoostAt1 = 0.3f;
            */

            /*
            public float hitDistributionMean = 0.5f;
            public float hitDistributionSTD = 1 / 6f;
            public float falseAlarmDistributionMean = 0.5f;
            public float falseAlarmDistributionSTD = 1 / 6f;
            */

            /*
            public float performance0_DPrime = -3;
            public float performance05_DPrime = 0;
            public float performance1_DPrime = 3;
            */

            /*
            // Overall difficulty increase
            public float performanceMulti = 1.3f;

            // More importance on positive, negative, balanced?
            public float badGoodCoefficient = 0.5f;

            public float goodHitImportance = 1.5f;
            public float goodMissImportance = 0.1f;
            public float badMissImportance = 1;
            public float badHitImportance = 0.9f;
            */

            public enum DiscountType { None = 0, Linear, Quadratic }
            public enum AdaptationMode { Debug = -1, None = 0, Direct = 1, Indirect = 2 }
            public enum IndirectType { Sign = 0, Linear, Sqrt }
            public enum WeightOfAverageVsCurrentPerLevel { CurrentOnly = 0, AverageOnly, Static, Linear, Sqrt, Quadratic }
            public enum AverageUpdateMode { Realtime = 0, EndOfLevel }
        }

        /*
        public void LogAccuracyIncrement(float accuracyIncrement)
        {
            accuracyIncrementLog.Add(new TimeStamped<float>(accuracyIncrement));
        }
        */

        // Potential subclass for guesses
        /*
        private readonly List<TimeStamped<float>> correctGuessesIncrementLog = new List<TimeStamped<float>>();
        private readonly List<TimeStamped<float>> wrongGuessesIncrementLog = new List<TimeStamped<float>>();

        public void LogGuessIncrement(bool isCorrect, float timeSinceStart)
        {
            (isCorrect ?
                correctGuessesIncrementLog :
                wrongGuessesIncrementLog).Add(new TimeStamped<float>(1, timeSinceStart));
        }

        public void Reset(bool resetScore = false)
        {
            wrongGuessesIncrementLog.Clear();
            correctGuessesIncrementLog.Clear();

            if (resetScore)
                difficulty = GetAverageDifficulty_Overall();// config.initialDifficulty;
        }
        */
    }

    [Serializable]
    public class DifficultyKeyFrame
    {
        public float atDifficulty = 0.5f;
        public float essencesPerSecond = 1;
        public float fallingSpeed = 1;
        public float deadlockMultiplier = 1;
        public float badGoodEssenceRatio = 1;

        public DifficultyKeyFrame() { }

        public DifficultyKeyFrame(float atDifficulty, float essencesPerSecond, float fallingSpeed, float deadlockMultiplier, float badGoodEssenceRatio)
        {
            this.atDifficulty = atDifficulty;
            this.essencesPerSecond = essencesPerSecond;
            this.fallingSpeed = fallingSpeed;
            this.deadlockMultiplier = deadlockMultiplier;
            this.badGoodEssenceRatio = badGoodEssenceRatio;
        }

        public static DifficultyKeyFrame Multiply(DifficultyKeyFrame frame1, DifficultyKeyFrame frame2)
        {
            DifficultyKeyFrame frame = new DifficultyKeyFrame();
            frame.essencesPerSecond = frame1.essencesPerSecond * frame2.essencesPerSecond;
            frame.fallingSpeed = frame1.fallingSpeed * frame2.fallingSpeed;
            frame.deadlockMultiplier = frame1.deadlockMultiplier * frame2.deadlockMultiplier;
            frame.badGoodEssenceRatio = frame1.badGoodEssenceRatio * frame2.badGoodEssenceRatio;
            return frame;
        }
        public static DifficultyKeyFrame Lerp(DifficultyKeyFrame frame1, DifficultyKeyFrame frame2, float val)
        {
            DifficultyKeyFrame frame = new DifficultyKeyFrame();
            frame.atDifficulty = Mathf.Lerp(frame1.atDifficulty, frame2.atDifficulty, val);
            frame.essencesPerSecond = Mathf.Lerp(frame1.essencesPerSecond, frame2.essencesPerSecond, val);
            frame.fallingSpeed = Mathf.Lerp(frame1.fallingSpeed, frame2.fallingSpeed, val);
            frame.deadlockMultiplier = Mathf.Lerp(frame1.deadlockMultiplier, frame2.deadlockMultiplier, val);
            frame.badGoodEssenceRatio = Mathf.Lerp(frame1.badGoodEssenceRatio, frame2.badGoodEssenceRatio, val);
            return frame;
        }
    }
}