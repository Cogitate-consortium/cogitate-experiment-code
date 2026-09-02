using UnityEngine;

namespace Game.Systems.Cameras
{
    /// <summary>
    /// [RENAME] PlayerCamera
    /// </summary>
    public class CameraControl : MonoBehaviour
    {
        private Transform target = null;
        private Camera myCamera = null;

        private void Awake()
        {
            myCamera = gameObject.GetComponentInChildren<Camera>();
        }

        public void FollowTarget(Transform target)
        {
            this.target = target;
        }

        // Update is called once per frame
        void Update()
        {
            if (target == null) return;

            transform.position = target.position;
        }

        public Camera GetCamera()
        {
            return myCamera;
        }
    }
}