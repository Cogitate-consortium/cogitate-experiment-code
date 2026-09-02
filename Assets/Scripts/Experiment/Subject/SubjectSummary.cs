using System;
using TGP.Helpers;
using UnityEngine;

namespace Experiment.Subject
{
    /// <summary>
    /// NS_RENAME SubjectSummary
    /// </summary>
    [System.Serializable]
    public class ProbeSummary
    {
        public string subjectId { get { return subject.id; } }
        public SubjectInfo subject;
        public DateTime sessionStartTime { get; private set; }
        private JourneySummary _journeySum = new JourneySummary();
        private JourneySummary _journeyTutorialSum = new JourneySummary();
        public ReplaySummary replaySum = new ReplaySummary();
        public WorldSummary worldSum = new WorldSummary();

        public JourneySummary GetJourneySummary(bool isTutorial)
        {
            return isTutorial ? _journeyTutorialSum : _journeySum;
        }

        public void ClearTutorialSum()
        {
            _journeyTutorialSum = new JourneySummary();
            this.LogWarning("Cleared Tutorial Summary");
        }
        /*
        public void OverrideStartTime(double offsetMS)
        {
            TimeSpan tS = TimeSpan.FromMilliseconds(offsetMS);
            sessionStartTime = DateTime.UtcNow - tS;

            this.LogWarning("Overriding start time ; set to {0} before Utc Now -> {1}"._Format(tS, sessionStartTime));
        }
        */
        public ProbeSummary(SubjectInfo subject)
        {
            this.subject = subject;
            this.sessionStartTime = DateTime.UtcNow; // Mark the session start time (not related to frames)
        }

        public class WorldSummary
        {
            public void LogNewProbe(int worldID)
            {
                totalNumberProbesPerWorld[worldID]++;
            }

            public int GetNumberProbes(int worldID)
            {
                return totalNumberProbesPerWorld[worldID];
            }

            private int[] totalNumberProbesPerWorld = new int[4];
        }

        [System.Serializable]
        public class JourneySummary
        {
            public int TotalNumberOfProbes;
            public int TotalNumberOfFacesProbed;
            public int TotalNumberOfObjectsProbed;
            public int TotalNumberOfBlanksProbed;

            public int TotalNumberOfAllStimuliSeen;
            public int TotalNumberOfAllStimuliUnseen;
            public int TotalNumberOfAllStimuliMaybe;
            public int TotalNumberOfAllStimuliNoResponse;
            public int TotalNumberFacesSeen;
            public int TotalNumberFacesUnseen;
            public int TotalNumberFacesMaybe;
            public int TotalNumberFacesNoResponse;
            public int TotalNumberObjectsSeen;
            public int TotalNumberObjectsUnseen;
            public int TotalNumberObjectsMaybe;
            public int TotalNumberObjectsNoResponse;
            public int TotalNumberOfBlanksCorrectlyUnseen;
            public int TotalNumberOfBlanksFalseAlarms;
            public int TotalNumberOfBlanksMaybe;
            public int TotalNumberOfBlanksNoResponse;
            // Total time of all Journey Levels 1-13 (excluding level selection, and pauses)
            public float Run_JourneyRunTime_NoPauses;

            public override string ToString()
            {
                return "TotalNumProbes :: {0}"._Format(TotalNumberOfProbes);
            }
        }

        [System.Serializable]
        public class ReplaySummary
        {
            public int TotalNumberOfStimuliPresentedInFaceTarget { get { return TotalNumberOfFacesPresentedInFaceTarget + TotalNumberOfObjectsPresentedInFaceTarget + TotalNumberOfBlanksPresentedInFaceTarget; } }
            public int TotalNumberOfFacesPresentedInFaceTarget;
            public int TotalNumberOfObjectsPresentedInFaceTarget;
            public int TotalNumberOfBlanksPresentedInFaceTarget;

            public int TotalNumberOfStimuliPresentedInObjectTarget { get { return TotalNumberOfFacesPresentedInObjectTarget + TotalNumberOfObjectsPresentedInObjectTarget + TotalNumberOfBlanksPresentedInObjectTarget; } }
            public int TotalNumberOfFacesPresentedInObjectTarget;
            public int TotalNumberOfObjectsPresentedInObjectTarget;
            public int TotalNumberOfBlanksPresentedInObjectTarget;

            public int TotalNumberOfHitsInFaceTarget;
            public int TotalNumberOfMissesInFaceTarget;
            public int TotalNumberOfFalseAlarmsInFaceTarget;

            public int TotalNumberOfHitsInObjectTarget;
            public int TotalNumberOfMissesInObjectTarget;
            public int TotalNumberOfFalseAlarmsInObjectTarget;

            // Total time of 2 Replay Levels L1, L2 (excluding level selection, including pause)
            public float Run_ReplayRunTime_NoPauses;
        }

        internal void SetSummaries(string gameJSON, string replayJSON)
        {
            JourneySummary _journeySum = JsonUtility.FromJson<JourneySummary>(gameJSON);
            ReplaySummary replaySum = JsonUtility.FromJson<ReplaySummary>(replayJSON);

            if (_journeySum != null)
                this._journeySum = _journeySum;

            if (replaySum != null)
                this.replaySum = replaySum;
        }
    }
}