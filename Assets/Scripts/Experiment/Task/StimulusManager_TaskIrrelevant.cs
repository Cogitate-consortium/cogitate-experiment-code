using Experiment.Background;
using Experiment.Managers;
using Experiment.Stimulus;
using Game.Core;
using System;
using System.Collections.Generic;
using TGP.Helpers;

namespace Experiment.Task
{
    // Choose Stim-Loc pair
    public class StimulusManager_TaskIrrelevant : StimulusManager
    {
        // private new Config config;
        private new RuntimeConfig runtimeConfig;

        public StimulusManager_TaskIrrelevant(StimulusManager.Config baseConfig, StimulusManager.RuntimeConfig baseRuntimeConfig) : base(baseConfig, baseRuntimeConfig)
        {
            // config = baseConfig as Config;
            runtimeConfig = baseRuntimeConfig as RuntimeConfig;
        }

        [Serializable]
        public new class Config : StimulusManager.Config
        {
            /// [SOS] do not use before segmenting <see cref="Experiment.Managers.Core.ExperimentManagerLevel"/> to rel / irrel
        }

        public new class RuntimeConfig : StimulusManager.RuntimeConfig
        {
            public bool doProbes;

            public RuntimeConfig(BackgroundManager.RuntimeConfig background, LevelConfig level, bool doProbes, List<EventInformation> timings) : base(background, level, timings)
            {
                this.doProbes = doProbes;
            }
        }

        protected override void GetStimulusToShow(BackgroundManager.AnimEventArgs e, out SpriteLocation? spriteLocation, out bool isGameProbeStimulus)
        {
            spriteLocation = null;
            isGameProbeStimulus = false;

            List<EventInformation> timings = Precalculator.gameTimings.stimulusOccurences_Actual;

            if (currentCycleIdx >= timings.Count)
            {
                this.LogWarning("Reached end of precalculated STIM timings. Level should end!");
                return;
            }

            if (currentCycleIdx < 0)
            {
                this.LogError("Current Cycle Idx negative: {0} (shouldn't happen)"._Format(currentCycleIdx));
                return;
            }

            EventInformation currentCycleInfo = timings[currentCycleIdx];

            int waitingOnBackgroundIndex = currentCycleInfo.triggeredByEventID;

            // This shouldn't happen
            if (e.backgroundCycleIdx > waitingOnBackgroundIndex)
            {
                string errorMessage = "[ERROR_LOST_CYCLE] Was waiting for bck cycle id {0}, got {1}"._Format(waitingOnBackgroundIndex, e.backgroundCycleIdx);
                this.LogWarning(errorMessage);
                ExperimentManagerSession.LogData_AtNextRenderedFrame_TS("StimulusManager", errorMessage);
            }

            bool isItTime = e.backgroundCycleIdx >= waitingOnBackgroundIndex;

            if (!isItTime)
                return;

            if (isTutorialQueue && orderedStimuli_Tutorial.Count == 0)
            {
                this.LogWarning("End of tutorial stimuli queue... Returning Random Stim");
                spriteLocation = GetRandomStimulus();
                return;
            }

            // Check if probed
            isGameProbeStimulus = runtimeConfig.doProbes && currentCycleInfo.triggersEventID >= 0;

            int stimIdx = currentCycleInfo.id;

            int worldIndex = runtimeConfig.level.worldID;

            List<SpriteLocation> orderedStimuliThisWorld =
                isTutorialQueue ? orderedStimuli_Tutorial :
                orderedStimuli_GameWorlds[worldIndex];

            // Tutorials are offset based on the cycle they have when they start
            int stimOffset;

            if (isTutorialQueue)
                stimOffset = cycleIdxAtLevelStart;
            else
            {
                stimOffset = 0;
                for (int i = 0; i <= worldIndex - 1; i++)
                    stimOffset += orderedStimuli_GameWorlds[i].Count;
            }
                

            int stimIdxWithinWorld = stimIdx - stimOffset;

            if (stimIdxWithinWorld < 0)
            {
                this.LogError("Something went wrong, probe idx within world was {0} for probe idx {1}. Setting Probe Idx within world to 0."._Format(stimIdxWithinWorld, stimIdx));
                stimIdxWithinWorld = 0;
            }

            if (stimIdxWithinWorld >= orderedStimuliThisWorld.Count)
            {
                if (config.taskIrrelevant_cycleStimuliWheneQueueIsEmpty)
                {
                    int falseStimIdxWithinWorld = stimIdxWithinWorld;
                    // Restart it if that happens
                    stimIdxWithinWorld = falseStimIdxWithinWorld % orderedStimuliThisWorld.Count;

                    this.LogError("Requested stim IDX {0}, indexed {1} within world {2} of a total of {3} probes within that world. Level should end! Setting Probe Idx within world to {4}".
                        _Format(stimIdx, falseStimIdxWithinWorld, worldIndex, orderedStimuliThisWorld.Count, stimIdxWithinWorld));
                }
                else
                {
                    this.LogError("Requested stim IDX {0}, indexed {1} within world {2} of a total of {3} probes within that world. Level should end!".
                         _Format(stimIdx, stimIdxWithinWorld, worldIndex, orderedStimuliThisWorld.Count));

                    return;
                }
            }

            spriteLocation = orderedStimuliThisWorld[stimIdxWithinWorld];

            this.LogWarning("Returning from ordered stimuli sequence");
        }
    }
}