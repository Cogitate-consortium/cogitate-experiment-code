using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;

namespace Experiment.Background
{
    public class BackgroundManager_Abstract_Spiral : BackgroundManager_Abstract
    {
        protected override void HandleMasterAnimation(float dT)
        {
            base.HandleMasterAnimation(dT);

            float degrees = -360 * dT / fullAnimationDuration;
            pivot.Rotate(Vector3.forward * degrees);
        }

        protected override void InitializeBackgroundObjectPositions(List<BackgroundObject> backgroundObjects)
        {
            base.InitializeBackgroundObjectPositions(backgroundObjects);

            // Archimedes spiral (http://mathworld.wolfram.com/ArchimedesSpiral.html)
            // r = a * θ
            float a = 1;
            // We will quantize θ to have 20% of our objects per loop (making 5 loops)
            int numLoops = 5;
            // Ensure we have enough to complete all 5 loops
            int numObjectsPerLoop = Mathf.FloorToInt((float)backgroundObjects.Count / numLoops);
            float dΘ = 2 * Mathf.PI / numObjectsPerLoop;

            float θ = 0;

            pivot.localScale = Vector3.one * 0.4f;

            foreach (BackgroundObject bO in backgroundObjects)
            {
                // Calculate the spiral pos
                Polar p = new Polar();
                p.θ = θ % (2 * Mathf.PI);
                p.r = a * θ;

                // Now we can convert and place
                Vector2 wantedRelativePos = p.GetCartessian();
                Vector2 wantedAbsolutePos = pivot.TransformPoint(wantedRelativePos);
                bO.SetPosition(wantedAbsolutePos);

                // Make their scale depend on r!
                float baseScale = .1f;
                float wantedScale = baseScale + p.r / 10;
                bO.SetLocalScale(wantedScale);

                // Make em look at the middle
                Vector3 degrees = Vector3.forward * p.θ_Deg;
                Quaternion wantedLocalRot = Quaternion.Euler(degrees);
                bO.SetLocalRotation(wantedLocalRot);

                // Sync em
                float wantedRandOffset = 1 - p.θ % 1;
                bO.SetRandomizationOffset(wantedRandOffset);

                // Increment
                θ += dΘ;
            }
        }
    }
}