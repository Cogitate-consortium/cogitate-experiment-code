using Experiment.Stimulus;
using Experiment.Task.Core;
using System;
using TGP.Helpers;

namespace Experiment.Triggers.Codes
{
    [Serializable]
    public class TriggerCodes_P : ITriggerCodes
    {
        public bool doubleOnsetForLevelStart = true;
        public bool doubleOnsetForLevelEnd = true;

        public CodePrio GetAnimationPeakEndCode(bool isGame)
        {
            return 1;
        }

        public CodePrio GetGameFillerCode()
        {
            return 1;
        }

        public CodePrio GetLocalizerFillerCode()
        {
            return 1;
        }

        public CodePrio GetProbeOnsetCode()
        {
            return 1;
        }

        public CodePrio GetReplayResponse(bool reportedSomething)
        {
            return 1;
        }

        public CodePrio GetResponseEventGame(TaskIrrelevantResponse answer)
        {
            return 1;
        }

        public CodePrio GetStimulusEventReplay(StimulusType type, string stimulusName, StimulusType targetStimulus, Direction_2D_Diagonal direction, bool isProbeTrial)
        {
            return 1;
        }

        public CodePrio GetTriggerCodeLevelBeginEvent(bool isGame)
        {
            return (int)(doubleOnsetForLevelStart ? 3 : 1); // 3 is "11" in binary
        }

        public CodePrio GetTriggerCodeLevelEndEvent(bool isGame)
        {
            return (int)(doubleOnsetForLevelEnd ? 3 : 1); // 3 is "11" in binary
        }

        public CodePrio GetTriggerCodeStimulusEventGame(StimulusType type, string stimulusName, Direction_2D_Diagonal direction, bool isProbeTrial)
        {
            return 1;
        }
    }
}