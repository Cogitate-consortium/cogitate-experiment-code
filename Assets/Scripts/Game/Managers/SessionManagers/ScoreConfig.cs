namespace Game.Managers.SessionManagers
{
    [System.Serializable]
    public class ScoreConfig
    {
        public Type type = Type.Linear;
        public float scorePercentileToKeepAfterDefeat = 0.5f;
        public float scoreMaxLossAfterDefeat = 3;
        public int rewardPerSecond = 1;
        public int penaltyMissedStimulus = 2;
        public int penaltyFalseStimulus = 2;
        public int rewardFoundStimulus = 3;
        public float beepEveryScore = 10;
        public float essenseScore = 0f;
        public float highEssenseScore = 1f;
        public float scoreMultiplier = 0.5f;
        public float neededScorePerLevelMultiplier = 1.0f;

        public float thresholdForStars_1 = 0.15f;
        public float thresholdForStars_2 = 0.40f;
        public float thresholdForStars_3 = 0.80f;

        public enum Type { Linear, Asymptote }
    }
}