using Helpers.Engine;
using System.Collections;
using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;

namespace Helpers.Misc
{
    /// <summary>
    /// [CHECK]
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ParallaxAnimation : MonoBehaviour
    {
        public Vector2 speed = Vector2.up;
        public Material material;
        private Material _material;
        private SpriteRenderer sR;

        private void Awake()
        {
            sR = this.GetComponent<SpriteRenderer>();
            if (sR == null)
            {
                this.LogError("Couldn't find any SpriteRenderer for ParallaxAnimation");
                return;
            }
            sR.material = new Material(material);
            _material = sR.material;
        }

        // Update is called once per frame
        void Update()
        {
            _material.mainTextureOffset = _material.mainTextureOffset + speed * TimeWrapper.deltaTime_SinceLastUpdate_NotTS;
        }

        private void OnDestroy()
        {
            Destroy(_material);
        }
    }
}