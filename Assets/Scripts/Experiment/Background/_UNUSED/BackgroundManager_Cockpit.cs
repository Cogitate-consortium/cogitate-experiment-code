// NS_DEBATABLE
using Game.Systems.Cameras;

using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;

namespace Experiment.Background
{
    public class BackgroundManager_Cockpit : BackgroundManager
    {
#if false
        [SerializeField] private Transform objectsParent = null;

        private Camera targetCamera;

        protected override void Initialize()
        {
            base.Initialize();

            // Find the camera
            CameraControl cFP = Utility_Helper.GetComponentInScene<CameraControl>();
            if (cFP != null)
                targetCamera = cFP.GetCamera();
        }

        protected override BackgroundType GetBackgroundType()
        {
            return BackgroundType.Cockpit;
        }

        protected override void CreateBackgroundObjects()
        {
            // Grab the children as referneces
            List<Transform> objectsPlaceHolders = new List<Transform>();
            for (int i = 0; i < objectsParent.childCount; i++)
                objectsPlaceHolders.Add(objectsParent.GetChild(i));

            foreach (Transform objectPlaceHolder in objectsPlaceHolders)
            {
                // Create
                BackgroundObject backgroundObject = CreateBackgroundObject(objectPlaceHolder);

                // Reparent it again
                backgroundObject.transform.parent = objectsParent;
            }

            // Kill the placeholders
            objectsPlaceHolders.ClearAndDestroy();
        }

        protected override void HandleMasterAnimation(float dT)
        {
            base.HandleMasterAnimation(dT);

            if (targetCamera == null) return;

            transform.position = targetCamera.transform.position;
            transform.rotation = targetCamera.transform.rotation;
        }

        // Nothing to do here
        protected override void InitializeBackgroundObjectPositions(List<BackgroundObject> backgroundObjects)
        {
            base.InitializeBackgroundObjectPositions(backgroundObjects);
        }
#endif
    }
}