using System;
using TGP.Helpers;
using UnityEngine;

namespace Game.Systems.Cameras
{
    /// <summary>
    /// [RENAME] CameraFrameControl
    /// </summary>
    public class CameraBlockControl : MonoBehaviour
    {
        [SerializeField] private Camera camera = null;
        [SerializeField] private Transform cut = null;
        [SerializeField] private SpriteRenderer block = null;

        // Make it really large to ensure moving it around won't break it
        private float baseScale = 10;

        // Update is called once per frame
        void Update()
        {
            float ratio = Screen.width / (float)Screen.height;
            float scaleY = camera.orthographicSize / 2;
            float scaleX = scaleY * ratio;
            Vector3 scale = new Vector3(scaleX, scaleY, 1);
            transform.SetLossyScale(scale * baseScale);
        }

        public void SetBlockColor(Color color)
        {
            block.color = color;
        }

        public void SetViewport(float percentile)
        {
            SetViewport(percentile, percentile);
        }

        public void SetViewport(float percentileX, float percentileY)
        {
            cut.localScale = new Vector3(percentileX, percentileY, 1) / baseScale;
        }
    }
}