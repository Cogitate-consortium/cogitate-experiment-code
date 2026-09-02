using Experiment.Stimulus;
using Experiment.Task.Core;
using TGP.Helpers;

namespace Experiment.Triggers.Codes
{
    public interface ITriggerCodes
    {
        CodePrio GetProbeOnsetCode();
        CodePrio GetResponseEventGame(TaskIrrelevantResponse answer);
        CodePrio GetReplayResponse(bool reportedSomething);
        CodePrio GetAnimationPeakEndCode(bool isGame);
        CodePrio GetGameFillerCode();
        CodePrio GetLocalizerFillerCode();
        CodePrio GetTriggerCodeLevelBeginEvent(bool isGame);
        CodePrio GetTriggerCodeLevelEndEvent(bool isGame);
        CodePrio GetTriggerCodeStimulusEventGame(StimulusType type, string stimulusName, Direction_2D_Diagonal direction, bool isProbeTrial);
        CodePrio GetStimulusEventReplay(StimulusType type, string stimulusName, StimulusType targetStimulus, Direction_2D_Diagonal direction, bool isProbeTrial);
    }

    public struct CodePrio
    {
        public int code;
        public int prio;

        public CodePrio(int code) : this(code, 0) { }

        public CodePrio(int code, int prio)
        {
            this.code = code;
            this.prio = prio;
        }

        public static implicit operator CodePrio(int code) => new CodePrio(code);
        public static implicit operator int(CodePrio codePrio) => codePrio.code;
    }
}