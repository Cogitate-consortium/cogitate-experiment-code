using Experiment.Triggers.Core;
using UnityEngine;

namespace Experiment.Triggers.Audio
{
    public class TriggerWaveGenerator : MonoBehaviour
    {
        private TriggerManager_Audio.Config config = new TriggerManager_Audio.Config();
        [SerializeField] private int numBits = 6;
        public void Start()
        {
            TriggerManager_Audio.Prepare(config, 6, progressUpdate =>
            {

            });
        }
    }
}