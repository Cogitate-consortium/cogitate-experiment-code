using Experiment.Stimulus;
using Experiment.Task.Core;
using System;
using TGP.Helpers;
using UnityEngine;

namespace Experiment.Triggers.Codes
{
    [Serializable]
    public class TriggerCodes_A : ITriggerCodes
    {
        // 
        public int numBits = 6;

        // Level Begin
        public int levelBegin_Game = 62;
        public int levelBegin_Replay = 30;

        // Level End
        public int levelEnd_Game = 63;
        public int levelEnd_Replay = 31;

        // Game Stimulus On (Non-Probed, Face)
        public int gameStimulusOn_NonProbed_Face_TopLeft = 48;
        public int gameStimulusOn_NonProbed_Face_TopRight = 49;
        public int gameStimulusOn_NonProbed_Face_BottomRight = 50;
        public int gameStimulusOn_NonProbed_Face_BottomLeft = 51;

        // Game Stimulus On (Non-Probed, Object)
        public int gameStimulusOn_NonProbed_Object_TopLeft = 52;
        public int gameStimulusOn_NonProbed_Object_TopRight = 53;
        public int gameStimulusOn_NonProbed_Object_BottomRight = 54;
        public int gameStimulusOn_NonProbed_Object_BottomLeft = 55;

        // Game Stimulus On (Probed, Face)
        public int gameStimulusOn_Probed_Face_TopLeft = 36;
        public int gameStimulusOn_Probed_Face_TopRight = 37;
        public int gameStimulusOn_Probed_Face_BottomRight = 38;
        public int gameStimulusOn_Probed_Face_BottomLeft = 39;

        // Game Stimulus On (Probed, Object)
        public int gameStimulusOn_Probed_Object_TopLeft = 40;
        public int gameStimulusOn_Probed_Object_TopRight = 41;
        public int gameStimulusOn_Probed_Object_BottomRight = 42;
        public int gameStimulusOn_Probed_Object_BottomLeft = 43;

        // Game Blank On (Non-Probed)
        public int gameBlankOn_NonProbed_TopLeft = 56;
        public int gameBlankOn_NonProbed_TopRight = 57;
        public int gameBlankOn_NonProbed_BottomRight = 58;
        public int gameBlankOn_NonProbed_BottomLeft = 59;

        // Game Blank On (Probed)
        public int gameBlankOn_Probed_TopLeft = 44;
        public int gameBlankOn_Probed_TopRight = 45;
        public int gameBlankOn_Probed_BottomRight = 46;
        public int gameBlankOn_Probed_BottomLeft = 47;

        // Game Filler On
        public int gameFillerOn = 60;

        // Game Probe On
        public int gameProbeOn = 2;

        // Game Response
        public int gameResponse_NoResponse = 32;
        public int gameResponse_Yes = 33;
        public int gameResponse_No = 34;
        public int gameResponse_Maybe = 35;

        // Replay Task On (Face)
        public int replayTaskOn_Face_TopLeft = 4;
        public int replayTaskOn_Face_TopRight = 5;
        public int replayTaskOn_Face_BottomRight = 6;
        public int replayTaskOn_Face_BottomLeft = 7;

        // Replay Task On (Object)
        public int replayTaskOn_Object_TopLeft = 8;
        public int replayTaskOn_Object_TopRight = 9;
        public int replayTaskOn_Object_BottomRight = 10;
        public int replayTaskOn_Object_BottomLeft = 11;

        // Replay Non-Task On (Face)
        public int replayNonTaskOn_Face_TopLeft = 16;
        public int replayNonTaskOn_Face_TopRight = 17;
        public int replayNonTaskOn_Face_BottomRight = 18;
        public int replayNonTaskOn_Face_BottomLeft = 19;

        // Replay Non-Task On (Object)
        public int replayNonTaskOn_Object_TopLeft = 20;
        public int replayNonTaskOn_Object_TopRight = 21;
        public int replayNonTaskOn_Object_BottomRight = 22;
        public int replayNonTaskOn_Object_BottomLeft = 23;

        // Replay Non-Task On (Blank)
        public int replayNonTaskOn_Blank_TopLeft = 24;
        public int replayNonTaskOn_Blank_TopRight = 25;
        public int replayNonTaskOn_Blank_BottomRight = 26;
        public int replayNonTaskOn_Blank_BottomLeft = 27;

        // Replay Filler On
        public int replayFillerOn = 28;

        // Replay Window Over
        public int replayWindowOver = 0;

        // Replay Response
        public int replayResponse = 1;

        // Animation Peak End
        public int animationPeakEnd_Game = 61;
        public int animationPeakEnd_Replay = 29;

        public CodePrio GetAnimationPeakEndCode(bool isGame)
        {
            return isGame ? animationPeakEnd_Game : animationPeakEnd_Replay;
        }

        public CodePrio GetGameFillerCode()
        {
            return gameFillerOn;
        }

        public CodePrio GetLocalizerFillerCode()
        {
            return replayFillerOn;
        }

        public CodePrio GetProbeOnsetCode()
        {
            return gameProbeOn;
        }

        public CodePrio GetReplayResponse(bool reportedSomething)
        {
            return reportedSomething ? replayResponse : replayWindowOver;
        }

        public CodePrio GetResponseEventGame(TaskIrrelevantResponse answer)
        {
            switch (answer)
            {
                case TaskIrrelevantResponse.Yes:
                    return gameResponse_Yes;
                case TaskIrrelevantResponse.No:
                    return gameResponse_No;
                case TaskIrrelevantResponse.Maybe:
                    return gameResponse_Maybe;
                case TaskIrrelevantResponse.NoResponse:
                    return gameResponse_NoResponse;
            }

            return -1;
        }

        public CodePrio GetStimulusEventReplay(StimulusType type, string stimulusName, StimulusType targetStimulus, Direction_2D_Diagonal direction, bool isProbeTrial)
        {
            // Non-Task
            if (type != targetStimulus)
            {
                switch (type)
                {
                    // Blanks
                    case StimulusType.None:

                        switch (direction)
                        {
                            // Top Left
                            case Direction_2D_Diagonal.TopLeft:
                                return replayNonTaskOn_Blank_TopLeft;

                            // Top Right
                            case Direction_2D_Diagonal.TopRight:
                                return replayNonTaskOn_Blank_TopRight;

                            // Bottom Right
                            case Direction_2D_Diagonal.BottomRight:
                                return replayNonTaskOn_Blank_BottomRight;

                            // Bottom Left
                            case Direction_2D_Diagonal.BottomLeft:
                                return replayNonTaskOn_Blank_BottomLeft;
                        }

                        break;

                    // Objects
                    case StimulusType.Object:

                        switch (direction)
                        {
                            // Top Left
                            case Direction_2D_Diagonal.TopLeft:
                                return replayNonTaskOn_Object_TopLeft;

                            // Top Right
                            case Direction_2D_Diagonal.TopRight:
                                return replayNonTaskOn_Object_TopRight;

                            // Bottom Right
                            case Direction_2D_Diagonal.BottomRight:
                                return replayNonTaskOn_Object_BottomRight;

                            // Bottom Left
                            case Direction_2D_Diagonal.BottomLeft:
                                return replayNonTaskOn_Object_BottomLeft;
                        }

                        break;

                    // Faces
                    case StimulusType.Face:

                        switch (direction)
                        {
                            // Top Left
                            case Direction_2D_Diagonal.TopLeft:
                                return replayNonTaskOn_Face_TopLeft;

                            // Top Right
                            case Direction_2D_Diagonal.TopRight:
                                return replayNonTaskOn_Face_TopRight;

                            // Bottom Right
                            case Direction_2D_Diagonal.BottomRight:
                                return replayNonTaskOn_Face_BottomRight;

                            // Bottom Left
                            case Direction_2D_Diagonal.BottomLeft:
                                return replayNonTaskOn_Face_BottomLeft;
                        }

                        break;
                }
            }

            // Task
            else
            {
                switch (type)
                {
                    // Blanks
                    case StimulusType.None:

                        // Shouldn't happen
#if UNITY_EDITOR
                        Debug.LogError("Requested code for Task/Blank combination, shouldn't happen");
#endif
                        return -1;

                    // Objects
                    case StimulusType.Object:

                        switch (direction)
                        {
                            // Top Left
                            case Direction_2D_Diagonal.TopLeft:
                                return replayTaskOn_Object_TopLeft;

                            // Top Right
                            case Direction_2D_Diagonal.TopRight:
                                return replayTaskOn_Object_TopRight;

                            // Bottom Right
                            case Direction_2D_Diagonal.BottomRight:
                                return replayTaskOn_Object_BottomRight;

                            // Bottom Left
                            case Direction_2D_Diagonal.BottomLeft:
                                return replayTaskOn_Object_BottomLeft;
                        }

                        break;

                    // Faces
                    case StimulusType.Face:

                        switch (direction)
                        {
                            // Top Left
                            case Direction_2D_Diagonal.TopLeft:
                                return replayTaskOn_Face_TopLeft;

                            // Top Right
                            case Direction_2D_Diagonal.TopRight:
                                return replayTaskOn_Face_TopRight;

                            // Bottom Right
                            case Direction_2D_Diagonal.BottomRight:
                                return replayTaskOn_Face_BottomRight;

                            // Bottom Left
                            case Direction_2D_Diagonal.BottomLeft:
                                return replayTaskOn_Face_BottomLeft;
                        }

                        break;
                }
            }

            return -1;
        }

        public CodePrio GetTriggerCodeLevelBeginEvent(bool isGame)
        {
            return isGame ? levelBegin_Game : levelBegin_Replay;
        }

        public CodePrio GetTriggerCodeLevelEndEvent(bool isGame)
        {
            return isGame ? levelEnd_Game : levelEnd_Replay;
        }

        public CodePrio GetTriggerCodeStimulusEventGame(StimulusType type, string stimulusName, Direction_2D_Diagonal direction, bool isProbeTrial)
        {
            // Non-Probed
            if (!isProbeTrial)
            {
                switch (type)
                {
                    // Blanks
                    case StimulusType.None:

                        switch (direction)
                        {
                            // Top Left
                            case Direction_2D_Diagonal.TopLeft:
                                return gameBlankOn_NonProbed_TopLeft;

                            // Top Right
                            case Direction_2D_Diagonal.TopRight:
                                return gameBlankOn_NonProbed_TopRight;

                            // Bottom Right
                            case Direction_2D_Diagonal.BottomRight:
                                return gameBlankOn_NonProbed_BottomRight;

                            // Bottom Left
                            case Direction_2D_Diagonal.BottomLeft:
                                return gameBlankOn_NonProbed_BottomLeft;
                        }

                        break;

                    // Objects
                    case StimulusType.Object:

                        switch (direction)
                        {
                            // Top Left
                            case Direction_2D_Diagonal.TopLeft:
                                return gameStimulusOn_NonProbed_Object_TopLeft;

                            // Top Right
                            case Direction_2D_Diagonal.TopRight:
                                return gameStimulusOn_NonProbed_Object_TopRight;

                            // Bottom Right
                            case Direction_2D_Diagonal.BottomRight:
                                return gameStimulusOn_NonProbed_Object_BottomRight;

                            // Bottom Left
                            case Direction_2D_Diagonal.BottomLeft:
                                return gameStimulusOn_NonProbed_Object_BottomLeft;
                        }

                        break;

                    // Faces
                    case StimulusType.Face:

                        switch (direction)
                        {
                            // Top Left
                            case Direction_2D_Diagonal.TopLeft:
                                return gameStimulusOn_NonProbed_Face_TopLeft;

                            // Top Right
                            case Direction_2D_Diagonal.TopRight:
                                return gameStimulusOn_NonProbed_Face_TopRight;

                            // Bottom Right
                            case Direction_2D_Diagonal.BottomRight:
                                return gameStimulusOn_NonProbed_Face_BottomRight;

                            // Bottom Left
                            case Direction_2D_Diagonal.BottomLeft:
                                return gameStimulusOn_NonProbed_Face_BottomLeft;
                        }

                        break;
                }
            }

            // Probed
            else
            {
                switch (type)
                {
                    // Blanks
                    case StimulusType.None:

                        switch (direction)
                        {
                            // Top Left
                            case Direction_2D_Diagonal.TopLeft:
                                return gameBlankOn_Probed_TopLeft;

                            // Top Right
                            case Direction_2D_Diagonal.TopRight:
                                return gameBlankOn_Probed_TopRight;

                            // Bottom Right
                            case Direction_2D_Diagonal.BottomRight:
                                return gameBlankOn_Probed_BottomRight;

                            // Bottom Left
                            case Direction_2D_Diagonal.BottomLeft:
                                return gameBlankOn_Probed_BottomLeft;
                        }

                        break;

                    // Objects
                    case StimulusType.Object:

                        switch (direction)
                        {
                            // Top Left
                            case Direction_2D_Diagonal.TopLeft:
                                return gameStimulusOn_Probed_Object_TopLeft;

                            // Top Right
                            case Direction_2D_Diagonal.TopRight:
                                return gameStimulusOn_Probed_Object_TopRight;

                            // Bottom Right
                            case Direction_2D_Diagonal.BottomRight:
                                return gameStimulusOn_Probed_Object_BottomRight;

                            // Bottom Left
                            case Direction_2D_Diagonal.BottomLeft:
                                return gameStimulusOn_Probed_Object_BottomLeft;
                        }

                        break;

                    // Faces
                    case StimulusType.Face:

                        switch (direction)
                        {
                            // Top Left
                            case Direction_2D_Diagonal.TopLeft:
                                return gameStimulusOn_Probed_Face_TopLeft;

                            // Top Right
                            case Direction_2D_Diagonal.TopRight:
                                return gameStimulusOn_Probed_Face_TopRight;

                            // Bottom Right
                            case Direction_2D_Diagonal.BottomRight:
                                return gameStimulusOn_Probed_Face_BottomRight;

                            // Bottom Left
                            case Direction_2D_Diagonal.BottomLeft:
                                return gameStimulusOn_Probed_Face_BottomLeft;
                        }

                        break;
                }
            }

            return -1;
        }
    }
}