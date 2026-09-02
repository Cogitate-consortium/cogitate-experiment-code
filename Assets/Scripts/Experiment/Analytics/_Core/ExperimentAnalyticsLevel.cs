using System;

namespace Experiment.Analytics.Core
{
    [Serializable]
    public class ExperimentAnalyticsLevel
    {
        public string levelName;
        public float totalTime;
        public float playTime;
        public float probesShown;
        public float objectsShown;
        public float facesShown;
        public float blankShown;
        public float stars;
        public float score;
        public float scorePercentile;

        public float downTime { get { return totalTime - playTime; } }
        public float totalStimuliShown { get { return objectsShown + facesShown + blankShown; } }

        public ExperimentAnalyticsLevel(string levelName)
        {
            this.levelName = levelName;
        }

        public static ExperimentAnalyticsLevel operator +(ExperimentAnalyticsLevel a, ExperimentAnalyticsLevel b)
        {
            ExperimentAnalyticsLevel c = new ExperimentAnalyticsLevel(a.levelName + "+" + b.levelName);

            c.totalTime = a.totalTime + b.totalTime;
            c.playTime = a.playTime + b.playTime;
            c.probesShown = a.probesShown + b.probesShown;
            c.objectsShown = a.objectsShown + b.objectsShown;
            c.facesShown = a.facesShown + b.facesShown;
            c.blankShown = a.blankShown + b.blankShown;
            c.stars = a.stars + b.stars;
            c.score = a.score + b.score;
            c.scorePercentile = a.scorePercentile + b.scorePercentile;

            return c;
        }

        public static ExperimentAnalyticsLevel operator /(ExperimentAnalyticsLevel a, float divider)
        {
            a.totalTime /= divider;
            a.playTime /= divider;
            a.probesShown /= divider;
            a.objectsShown /= divider;
            a.facesShown /= divider;
            a.blankShown /= divider;
            a.stars /= divider;
            a.score /= divider;
            a.scorePercentile /= divider;

            return a;
        }


        public override string ToString()
        {
            return string.Format("Level_{0} | {1} Playtime  (s) | {2} Total time  (s) | {3} Downtime (s) | {4} probes | {5} blank | {6} faces | {7} objects | {8} total stimuli | {9} stars | {10} score | {11} score percentile",
                levelName, playTime, totalTime, downTime, probesShown, blankShown, facesShown, objectsShown, totalStimuliShown, stars, score, scorePercentile);
        }
    }
}