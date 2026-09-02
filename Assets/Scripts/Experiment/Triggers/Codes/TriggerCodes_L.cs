using Experiment.Stimulus;
using Experiment.Task.Core;
using System;
using TGP.Helpers;

namespace Experiment.Triggers.Codes
{
    [Serializable]
    public class TriggerCodes_L : ITriggerCodes
    {
        public int priority_ReplayResponse = 10;
        public int priority_ReplayResponse_Timeout = 10;
        public int priority_GameResponse = 10;
        public int priority_GameResponse_Timeout = 10;

        // Codes sent through Ports
        public short responseNoAnswer = 96;
        public short responseMaybe = 97;
        public short responseYes = 98;
        public short responseNo = 99;

        public short probeOnSet = 100;

        public short localizerWindowOver = 196;
        public short localizerReport = 198;

        public short gameFillerCode = 95;
        public short localizerFillerCode = 195;

        // Game stimulus IDs
        public short gameFaceOffset = 0;
        public short gameObjectOffset = 20;
        public short gameBlankID = 50;

        // Replay Face target Stimulus IDs
        public short replayFaceTargetFaceOffset = 100;
        public short replayFaceTargetObjectOffset = 120;
        public short replayFaceTargetBlankID = 150;

        // Replay Object target Stimulus IDs
        public short replayObjectTargetFaceOffset = 200;
        public short replayObjectTargetObjectOffset = 220;
        public short replayObjectTargetBlankID = 250;

        public short levelBegin = 251;
        public short levelEnd = 252;
        public short animationPeakEnd = 253;

        public short directionTopLeft = 60;
        public short directionTopRight = 70;
        public short directionBottomRight = 80;
        public short directionBottomLeft = 90;

        public short probedTrialOffset = 1;

        public CodePrio GetProbeOnsetCode()
        {
            return probeOnSet;
        }

        private CodePrio GetLocation(Direction_2D_Diagonal direction, bool isProbeTrial)
        {
            short directionID = 0;
            switch (direction)
            {
                case Direction_2D_Diagonal.TopLeft:
                    directionID = directionTopLeft;
                    break;
                case Direction_2D_Diagonal.TopRight:
                    directionID = directionTopRight;
                    break;
                case Direction_2D_Diagonal.BottomRight:
                    directionID = directionBottomRight;
                    break;
                case Direction_2D_Diagonal.BottomLeft:
                    directionID = directionBottomLeft;
                    break;
            }

            if (isProbeTrial)
                directionID += probedTrialOffset;

            return directionID;
        }

        public CodePrio GetResponseEventGame(TaskIrrelevantResponse answer)
        {
            return new CodePrio(
                answer == TaskIrrelevantResponse.Yes ? responseYes :
                answer == TaskIrrelevantResponse.No ? responseNo :
                answer == TaskIrrelevantResponse.Maybe ? responseMaybe :
                answer == TaskIrrelevantResponse.NoResponse ? responseNoAnswer : 0,
                answer != TaskIrrelevantResponse.NoResponse ? priority_GameResponse : priority_GameResponse_Timeout);
        }

        public CodePrio GetReplayResponse(bool reportedSomething)
        {
            return new CodePrio(
                reportedSomething ? localizerReport : localizerWindowOver,
                reportedSomething ? priority_ReplayResponse : priority_ReplayResponse_Timeout);
        }

        public CodePrio GetAnimationPeakEndCode(bool isGame)
        {
            return animationPeakEnd;
        }

        public CodePrio GetGameFillerCode()
        {
            return gameFillerCode;
        }

        public CodePrio GetLocalizerFillerCode()
        {
            return localizerFillerCode;
        }

        public CodePrio GetTriggerCodeLevelBeginEvent(bool isGame)
        {
            return levelBegin;
        }

        public CodePrio GetTriggerCodeLevelEndEvent(bool isGame)
        {
            return levelEnd;
        }

        private CodePrio GetTriggerCodeStimulusEventGame(StimulusType type, string stimulusName)
        {
            int stimCode = 0;
            int stimID_1Based = StimulusManager.GetStimIDFromName_0Based(stimulusName) + 1;

            // Faces 1-...
            if (type == StimulusType.Face)
            {
                stimCode = gameFaceOffset + stimID_1Based;
            }
            // Objects 21-40
            else if (type == StimulusType.Object)
            {
                stimCode = gameObjectOffset + stimID_1Based;
            }
            // Blank 50
            else
            {
                stimCode = gameBlankID;
            }

            short stimCode_Short = (short)stimCode;

            return stimCode_Short;
        }

        private CodePrio GetStimulusEventReplay(StimulusType type, string stimulusName, StimulusType targetStimulus)
        {
            int stimCode = 0;
            int stimID_1Based = StimulusManager.GetStimIDFromName_0Based(stimulusName) + 1;

            // Replay Face target
            if (targetStimulus == StimulusType.Face)
            {
                switch (type)
                {
                    case StimulusType.None:
                        stimCode = replayFaceTargetBlankID;
                        break;
                    case StimulusType.Object:
                        stimCode = replayFaceTargetObjectOffset + stimID_1Based;
                        break;
                    case StimulusType.Face:
                        stimCode = replayFaceTargetFaceOffset + stimID_1Based;
                        break;
                }
            }
            else if (targetStimulus == StimulusType.Object)
            {
                switch (type)
                {
                    case StimulusType.None:
                        stimCode = replayObjectTargetBlankID;
                        break;
                    case StimulusType.Object:
                        stimCode = replayObjectTargetObjectOffset + stimID_1Based;
                        break;
                    case StimulusType.Face:
                        stimCode = replayObjectTargetFaceOffset + stimID_1Based;
                        break;
                }
            }

            short stimCode_Short = (short)stimCode;

            return stimCode_Short;
        }

        public CodePrio GetTriggerCodeStimulusEventGame(StimulusType type, string stimulusName, Direction_2D_Diagonal direction, bool isProbeTrial)
        {
            return GetTriggerCodeStimulusEventGame(type, stimulusName) * 256 + GetLocation(direction, isProbeTrial);
        }

        public CodePrio GetStimulusEventReplay(StimulusType type, string stimulusName, StimulusType targetStimulus, Direction_2D_Diagonal direction, bool isProbeTrial)
        {
            return GetStimulusEventReplay(type, stimulusName, targetStimulus) * 256 + GetLocation(direction, isProbeTrial);
        }
    }
}