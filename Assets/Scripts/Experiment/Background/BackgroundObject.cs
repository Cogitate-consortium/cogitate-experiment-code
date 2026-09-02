using System;
using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;
using Game.Core;

namespace Experiment.Background
{
    /// <summary>
    /// [SEGMENT] Helper with quadrants n stuff
    /// </summary>
    public class BackgroundObject : BaseObject
    {
        public void Initialize(RuntimeConfig config)
        {
            base.Initialize(config);
        }

        public new class RuntimeConfig : BaseObject.RuntimeConfig
        {
            public RuntimeConfig(Rect activeArea) : base(activeArea) { }
        }

        public float overlayAlpha { get { return overlayRenderer != null ? overlayRenderer.color.a : -1; } }
        public float overlayBrightness { get { return overlayRenderer != null ? overlayRenderer.color.grayscale : -1; } }

        [SerializeField, Range(0.05f, 0.5f)] private float alphaMin = 0.35f;
        [SerializeField, Range(0.1f, 1.0f)] private float alphaMax = 0.70f;

        public SpriteRenderer mainRenderer { get; private set; }
        public SpriteRenderer overlayRenderer = null;

        private Sprite originalSprite = null;
        private Color originalColor;

        private bool isLocked = false;

        private QuadrantParameters quadrantParameters;

        // [SerializeField] private QuadrantLocation quadrantLocation;
        #region Public Methods

        public void SetQuadrantParameters(QuadrantParameters quadrantParameters)
        {
            this.quadrantParameters = quadrantParameters;
        }

        // [BUG] Return false direction for bottom sides. it reports them as top
        public Direction_2D_Diagonal GetDirection()
        {
            QuadrantLocation loc = quadrantParameters.GetLocation(mainRenderer.transform.position);

            if (loc.i < quadrantParameters.numColumns / 2f && loc.j < quadrantParameters.numRows / 2f) return Direction_2D_Diagonal.TopLeft;
            if (loc.i > quadrantParameters.numColumns / 2f && loc.j < quadrantParameters.numRows / 2f) return Direction_2D_Diagonal.TopRight;
            if (loc.i > quadrantParameters.numColumns / 2f && loc.j > quadrantParameters.numRows / 2f) return Direction_2D_Diagonal.BottomRight;
            if (loc.i < quadrantParameters.numColumns / 2f && loc.j > quadrantParameters.numRows / 2f) return Direction_2D_Diagonal.BottomLeft;

            return default(Direction_2D_Diagonal);
        }

        public int GetQuadrantIndex()
        {
            return quadrantParameters.GetLocation(mainRenderer.transform.position).q;
        }

        public void SetOverlay(Sprite spriteToShow)
        {
            if (isLocked) return;

            overlayRenderer.gameObject.SetActive(true);
            overlayRenderer.sprite = spriteToShow;
        }

        /// <summary>
        /// Set param to -1 to ignore
        /// </summary>
        public void AdjustOverlay(float alpha, float brightness, bool force = false)
        {
            if (isLocked && !force) return;
            if (alpha >= 0)
                overlayRenderer.color = overlayRenderer.color.Change(ColorProperty.a, alpha);
            if (brightness >= 0)
                overlayRenderer.color = overlayRenderer.color.AdjustBrightness(brightness);
        }

        /// <summary>
        /// Set param to -1 to ignore
        /// </summary>
        public void AdjustMain(float alpha, float brightness)
        {
            if (isLocked) return;

            SetUpdateBrightness(false);

            if (alpha >= 0)
                mainRenderer.color = mainRenderer.color.Change(ColorProperty.a, alpha);
            if (brightness >= 0)
                mainRenderer.color = mainRenderer.color.AdjustBrightness(brightness);
        }

        public void Lock(bool doLock)
        {
            isLocked = doLock;
        }

        /// <summary>
        /// Preserves alpha and brightness
        /// </summary>
        public void SetColorMain(Color color)
        {
            if (isLocked) return;
            //sR.color = color.AdjustBrightness(sR.color.grayscale).Change(ColorProperty.a, sR.color.a);
            LeanTween.color(mainRenderer.gameObject, color, 0.35f).setEaseInQuad();
        }

        /// <summary>
        /// Preserves alpha and brightness
        /// </summary>
        public void SetColorOverlay(Color color)
        {
            if (isLocked) return;
            //overlayRenderer.color = color.AdjustBrightness(overlayRenderer.color.grayscale).Change(ColorProperty.a, overlayRenderer.color.a);
            LeanTween.color(overlayRenderer.gameObject, color, 0.35f).setEaseInQuad();
        }

        public void UnPaint()
        {
            if (isLocked) return;
            overlayRenderer.sprite = originalSprite;
            overlayRenderer.color = originalColor.Change(ColorProperty.a, 0);
            overlayRenderer.gameObject.SetActive(false);
            // [BUG] Causing square to be semi-transparent after unpaint; stand out against other opaque squares
            //SetUpdateBrightness(true);

        }

        /*
        public bool IsVisible()
        {
            Renderer r = GetRenderer();

            // bool scaleOK = r.transform.lossyScale.x * r.transform.lossyScale.y > Mathf.Pow(scaleThreshold, 2);
            // if (checkScale && !scaleOK) return false;

            bool visible = r.isVisible;

            // [TEMP, HACK]
            // bool alphaOK = sR.color.a <= alphaThreshold;

            return visible;// && alphaOK;
        }*/

        public virtual bool IsStimulusCandidate()
        {
            return true;
        }

        public Vector2 GetOverlayPixelSize()
        {
            return (GetOverlayRenderer() as SpriteRenderer).GetPixelSize();
        }

        protected override string GetPixelsReport()
        {
            return base.GetPixelsReport() + ", Overlay Size {0}"._Format(GetOverlayPixelSize());
        }

        #endregion

        protected override void OnAwake()
        {
            base.OnAwake();

            Renderer r = GetRenderer();
            mainRenderer = r as SpriteRenderer;

            if (mainRenderer == null)
            {
                this.Log("Don't have a renderer to show on");
                return;
            }

            originalSprite = overlayRenderer.sprite;
            originalColor = overlayRenderer.color;
        }

        protected Renderer GetOverlayRenderer()
        {
            return overlayRenderer;
        }

        protected override void OnUpdate(float dT)
        {
            base.OnUpdate(dT);

            // overlayRenderer.transform.position = sR.transform.position;
            overlayRenderer.transform.localRotation = Quaternion.identity;
        }

        protected override void UpdateVisibility(float alpha01)
        {
            // Only for valid quadrants
            // if (GetQuadrantIndex() < 0) return;

            float alpha = alpha01.RetargetedFrom_01(alphaMin, alphaMax);
            mainRenderer.color = mainRenderer.color.Change(ColorProperty.a, alpha);
        }

        public override string ToString()
        {
            return "{0} {1} (quadrant index :: {2})"._Format(typeof(BackgroundObject), name, GetQuadrantIndex());
        }
    }

    [Serializable]
    public struct QuadrantParameters
    {
        public Vector2 bottomLeft;
        public Vector2 topRight;
        public int numRows;
        public int numColumns;
        public List<Vector2Int> acceptableQuadrants;

        public QuadrantParameters(Vector2 bottomLeft, Vector2 topRight, int numRows, int numColumns, List<Vector2Int> acceptableQuadrants)
        {
            this.bottomLeft = bottomLeft;
            this.topRight = topRight;
            this.numRows = numRows;
            this.numColumns = numColumns;
            this.acceptableQuadrants = acceptableQuadrants;
        }

        public QuadrantLocation GetLocation(Vector2 pos)
        {
            int i = 0;
            int j = 0;

            // Normalize pos
            Vector2 pos01 = new Vector2();
            pos01.x = pos.x.RetargetedTo_01(bottomLeft.x, topRight.x);
            pos01.y = 1 - pos.y.RetargetedTo_01(bottomLeft.y, topRight.y);

            // Normalize step
            float stepX = 1f / numColumns;
            float stepY = 1f / numRows;

            // See where we are
            i = Mathf.FloorToInt(pos01.x / stepX);
            j = Mathf.FloorToInt(pos01.y / stepY);

            int q = GetQuadrant(i, j);

            return new QuadrantLocation(i, j, q);
        }

        private int GetQuadrant(int i, int j)
        {
            for (int q = 0; q < acceptableQuadrants.Count; q++)
                if (i == acceptableQuadrants[q].x && j == acceptableQuadrants[q].y)
                    return q;
            return -1;
        }
    }

    [Serializable]
    public struct QuadrantLocation
    {
        public int i;
        public int j;
        public int q;

        public QuadrantLocation(int i, int j, int q) : this()
        {
            this.i = i;
            this.j = j;
            this.q = q;
        }
    }
}