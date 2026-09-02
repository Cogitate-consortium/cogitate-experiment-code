using Experiment.Background;
using Experiment.Stimulus;
using Game.Core;
using Helpers.Engine;
using System;
using System.Collections.Generic;
using TGP.Helpers;

namespace Experiment.Task
{
    public class StimulusManager_TaskRelevant : StimulusManager
    {
        // private new Config config;
        private new RuntimeConfig runtimeConfig;

        public event EventHandler<EventArgs> onLastStimulusShown;
        private bool hasReportedLastStimulus;

        public StimulusManager_TaskRelevant(StimulusManager.Config baseConfig, StimulusManager.RuntimeConfig baseRuntimeConfig) : base(baseConfig, baseRuntimeConfig)
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
            public RuntimeConfig(BackgroundManager.RuntimeConfig background, LevelConfig level, List<EventInformation> timings) : base(background, level, timings)
            {
            }
        }

        protected override void GetStimulusToShow(BackgroundManager.AnimEventArgs e, out SpriteLocation? stimulusToShow, out bool isGameProbeStimulus)
        {
            stimulusToShow = null;
            isGameProbeStimulus = false;

            double currentTimestampMS = TimeWrapper.currentFrameCycleBeginMS;
            // How much is from here to onset?
            double expectedDelayMS = e.distanceToPeakBeginS * 1000;
            // Debug.Log(SubjectPerformanceReport.currentFrameCycleTimestampMS + " Expected to reach PEAK in :: " + expectedDelayMS);// [200501] confirm reach peak expectedDelayMS after this point (+- frame dT)

            // Current + Delay > Next Stim
            bool isItTime = currentTimestampMS + expectedDelayMS >= nextStimTimestampMS;
            if (!isItTime) return;

            int localizerIndex = runtimeConfig.level.localizerID_0Based;

            int stimulusIndexWithinLocalizer = currentCycleIdx - cycleIdxAtLevelStart;

            if (stimulusIndexWithinLocalizer < 0)
            {
                this.LogError("Something went wrong, stimulus idx within localizer {0}. Setting Stimulus Idx within localizer to 0."._Format(stimulusIndexWithinLocalizer));
                stimulusIndexWithinLocalizer = 0;
            }

            // Last Stimulus
            if (stimulusIndexWithinLocalizer >= orderedStimuli_Localizers[localizerIndex].Count - 1)
            {
                LastStimulusShown();

                if (stimulusIndexWithinLocalizer >= orderedStimuli_Localizers[localizerIndex].Count)
                {
                    if (config.taskRelevant_showRandomStimulusWhenQueueIsEmpty)
                    {
                        stimulusToShow = GetRandomStimulus();

                        this.LogWarning("Requested Stimulus IDX {0}, indexed {1} within localizer {2} of a total of {3} probes within that world. Localizer should end! Showing random stimulus".
                            _Format(currentCycleIdx, stimulusIndexWithinLocalizer, localizerIndex, orderedStimuli_Localizers[localizerIndex].Count));
                    }
                    else
                    {
                        this.LogWarning("Requested Stimulus IDX {0}, indexed {1} within localizer {2} of a total of {3} probes within that world. Localizer should end! Showing random stimulus".
                            _Format(currentCycleIdx, stimulusIndexWithinLocalizer, localizerIndex, orderedStimuli_Localizers[localizerIndex].Count));
                        return;
                    }

                    return;
                }
            }

            stimulusToShow = orderedStimuli_Localizers[localizerIndex][stimulusIndexWithinLocalizer];

            return;
        }

        private void LastStimulusShown()
        {
            if (!hasReportedLastStimulus)
            {
                hasReportedLastStimulus = true;
                onLastStimulusShown?.Invoke(this, new EventArgs());
            }
        }
    }
}