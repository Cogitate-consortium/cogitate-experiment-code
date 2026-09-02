using TGP.Helpers;
using UnityEngine;

namespace Experiment.Background
{
    /// <summary>
    /// [DEPRECATE?] We don't really need the diff between _Abstract and NonAbstract I think
    /// </summary>
    public class BackgroundManager_Abstract : BackgroundManager
    {
        [SerializeField] protected Transform pivot = null;

        protected float lossyScale { get; private set; }
        protected int numBackgroundObjects { get; private set; }

        protected override void Initialize(Config config, RuntimeConfig runtimeConfig)
        {
            // [SOS] First get scale -> NUM OBJECTS
            lossyScale = config.GetScaleMultiplierStimulus(Camera.main);
            pivot.transform.localPosition = Vector3.up * config.backgroundVerticalPositionBaseOffset_DO_NOT_CHANGE;

            numBackgroundObjects = config.GetNumBackgroundObjects(lossyScale);

            // [SOS] Then initialize based on num objects
            base.Initialize(config, runtimeConfig);

            // [SOS] Finally do the actual scaling
            transform.SetLossyScale(Vector3.one * lossyScale);

            this.Log("Set up Background to scale :: {0}"._Format(lossyScale));
        }

        protected override BackgroundType GetBackgroundType()
        {
            return BackgroundType.Abstract;
        }

        protected override void CreateBackgroundObjects()
        {
            for (int i = 0; i < config.GetNumBackgroundObjects(lossyScale); i++)
            {
                CreateBackgroundObject(pivot);
            }
        }
    }
}