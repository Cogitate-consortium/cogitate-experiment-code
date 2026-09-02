// NS_ABSORB
using Helpers.Engine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TGP.Helpers
{
    public static class UI_Helper
    {
        #region Transforms
        public static void AnchorToCorners(this RectTransform rT)
        {
            if (rT == null) return;

            RectTransform pt = rT.parent as RectTransform;

            if (pt == null) return;

            Vector2 newAnchorsMin = new Vector2(rT.anchorMin.x + rT.offsetMin.x / pt.rect.width,
                                                rT.anchorMin.y + rT.offsetMin.y / pt.rect.height);
            Vector2 newAnchorsMax = new Vector2(rT.anchorMax.x + rT.offsetMax.x / pt.rect.width,
                                                rT.anchorMax.y + rT.offsetMax.y / pt.rect.height);

            rT.anchorMin = newAnchorsMin;
            rT.anchorMax = newAnchorsMax;
            rT.offsetMin = rT.offsetMax = new Vector2(0, 0);
        }

        public static bool FitToParent(this RectTransform rT)
        {
            RectTransform parent = rT.parent as RectTransform;
            if (parent == null) return false;

            rT.anchorMax = new Vector2(0.5f, 0.5f);
            rT.anchorMin = new Vector2(0.5f, 0.5f);
            rT.pivot = new Vector2(0.5f, 0.5f);

            Image im = rT.GetComponent<Image>();
            if (im)
                im.SetNativeSize();
            RawImage rawImage = rT.GetComponent<RawImage>();
            if (rawImage)
                rawImage.SetNativeSize();

            Vector3 canvasScale = rT.GetHighestCanvas().transform.lossyScale;

            Vector2 parentSize = new Vector2(parent.rect.width, parent.rect.height);
            Vector2 rectSize = new Vector2(rT.rect.width, rT.rect.height);
            float parentToChildScaleFactor = Mathf.Max(parentSize.x / rectSize.x, parentSize.y / rectSize.y);

            rT.sizeDelta *= parentToChildScaleFactor;

            return true;
        }

        public static void ScaleAndCrop(this RectTransform rectTransform, float originalWidth, float originalHeight)
        {
            if (rectTransform == null) return;
            if (rectTransform.parent == null) return;
            RectTransform container = rectTransform.parent.GetComponent<RectTransform>();
            if (container == null) return;

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.one * 0.5f;

            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, originalWidth);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalHeight);

            // First match the player with the mask - player should extend beyond it
            float widthPercentile = rectTransform.rect.width / container.rect.width;
            float heightPercentile = rectTransform.rect.height / container.rect.height;
            float scaleMultiplier = 1 / Mathf.Min(widthPercentile, heightPercentile);

            // Look at the ratio between player and mask (container)
            // float r_player = rectTransform.rect.width / rectTransform.rect.height;
            // float r_mask = container.rect.width / container.rect.height;

            rectTransform.localScale = Vector3.one * scaleMultiplier;
        }

        public static float Lerp_Fill(this Image img, float desiredPercentile, float speed = 1, float maxFill = 1)
        {
            float diff = desiredPercentile * maxFill - img.fillAmount;
            if (Mathf.Abs(diff) > maxFill * TimeWrapper.deltaTime_SinceLastUpdate_NotTS)
                img.fillAmount += Mathf.Sign(diff) * maxFill * TimeWrapper.deltaTime_SinceLastUpdate_NotTS * speed;
            else
                img.fillAmount = desiredPercentile * maxFill;
            return img.fillAmount / maxFill;
        }

        public static Vector2 GetPositionAsScreenPercentile(this RectTransform rect)
        {
            Vector2 absPos = rect.transform.position;
            absPos.x /= Screen.width;
            absPos.y /= Screen.height;
            return absPos;
        }

        /// <summary>
        /// [SOS] This might not work if the parent is rotated!!!!
        /// </summary>
        public static Vector2 GetAbsoluteAnchoredPosition(this RectTransform rect)
        {
            if (rect.parent == null)
                return rect.anchoredPosition;

            RectTransform parentRectTransform = rect.parent.GetComponent<RectTransform>();

            if (parentRectTransform == null)
                return Vector2.zero;

            return parentRectTransform.GetAbsoluteAnchoredPosition() + rect.anchoredPosition;
        }

        /// <summary>
        /// [SOS] This might not work if the parent is rotated!!!!
        /// </summary>
        public static void SetAbsoluteAnchoredPosition(this RectTransform rect, Vector2 wantedAbsolutePos, bool debug = false)
        {
            if (wantedAbsolutePos.IsNaN())
            {
                if (debug)
                    Debug_Helper.Log(typeof(Utility_Helper), "Wanted position was NaN!! Aborting ({0})"._Format(wantedAbsolutePos));
                return;
            }

            Vector2 currentAnchoredPos = rect.GetAbsoluteAnchoredPosition();
            Vector2 difference = wantedAbsolutePos - currentAnchoredPos;
            rect.anchoredPosition += difference;
        }

        /// <summary>
        /// [SOS] Does NOT work!!
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="onlyIfExceeding"></param>
        public static void FitChildrenToGroup_SquareCells_NOT_WORKING_PROPERLY(this GridLayoutGroup grid, bool onlyIfExceeding = true)
        {
            float totalWidth = grid.GetComponent<RectTransform>().rect.width;
            float totalHeight = grid.GetComponent<RectTransform>().rect.height;
            float numElements = grid.transform.childCount;

            // We already fit
            if (onlyIfExceeding && numElements * grid.cellSize.x * grid.cellSize.y < totalWidth * totalHeight)
                return;

            float px = Mathf.Ceil(Mathf.Sqrt(numElements * totalWidth / totalHeight));
            float sx, sy;

            if (Mathf.Floor(px * totalHeight / totalWidth) * px < numElements)  //does not fit, y/(x/px)=px*y/x
                sx = totalHeight / Mathf.Ceil(px * totalHeight / totalWidth);
            else
                sx = totalWidth / px;

            float py = Mathf.Ceil(Mathf.Sqrt(numElements * totalHeight / totalWidth));
            if (Mathf.Floor(py * totalWidth / totalHeight) * py < numElements)  //does not fit
                sy = totalWidth / Mathf.Ceil(totalWidth * py / totalHeight);
            else
                sy = totalHeight / py;

            float newSize = Mathf.Max(sx, sy);

            grid.cellSize = Vector2.one * newSize;

            grid.Log("New size :: {0}"._FormatBold(newSize), LogType.Error);
        }
        #endregion


        #region Texture Editing
        public static Texture2D MirrorToNew(this Texture2D original, bool mirrorHorizontally, bool mirrorVertically)
        {
            if (original == null) return null;
            Color[] output = new Color[original.width * original.height];
            Color[] pixels = original.GetPixels();

            for (int rowIdx = 0; rowIdx < original.height; rowIdx++)
                for (int columnIdx = 0; columnIdx < original.width; columnIdx++)
                {
                    int sourcePixelIdx = rowIdx * original.width + columnIdx;

                    int rowIdx_mirroredVertically = original.height - rowIdx - 1;
                    int columnIdx_mirroredHorizontally = original.width - columnIdx - 1;

                    int destinationPixelIdx =
                        (mirrorVertically ? rowIdx_mirroredVertically : rowIdx) * original.width +
                        (mirrorHorizontally ? columnIdx_mirroredHorizontally : columnIdx);

                    output[destinationPixelIdx] = pixels[sourcePixelIdx];
                }

            Texture2D result = new Texture2D(original.width, original.height);

            result.SetPixels(output);
            result.Apply();

            return result;
        }

        public static Sprite ToSprite(this Texture2D original)
        {
            if (original == null) return null;

            return Sprite.Create(original, new Rect(Vector2.zero, new Vector2(original.width, original.height)), Vector2.one * 0.5f);
        }

        /// <summary>
        ///  Provide output texture to write onto.
        /// </summary>
        public static Texture2D Crop_FromCenter(this Texture2D original, Texture2D output, float xPercentile, float yPercentile)
        {
            // Tried resizing texture, didn't work - compromised elegance for not spending more time to look into it; 

            if (!original) return null;

            if (xPercentile < 0 && yPercentile < 0) return original;

            int windowSize_X = (int)(original.width * xPercentile);
            int windowSize_Y = (int)(original.height * yPercentile);

            if (xPercentile < 0)
                windowSize_X = windowSize_Y;
            if (yPercentile < 0)
                windowSize_Y = windowSize_X;

            int x_window = (original.width - windowSize_X) / 2;
            int y_window = (original.height - windowSize_Y) / 2;

            Color[] pixels_In = original.GetPixels(x_window, y_window, windowSize_X, windowSize_Y);
            Color[] pixels_Out = new Color[pixels_In.Length];

            if (output == null)
                output = new Texture2D(windowSize_X, windowSize_Y);
            else
                output.Resize(windowSize_X, windowSize_Y);

            for (int i = 0; i < windowSize_X; i++)
                for (int j = 0; j < windowSize_Y; j++)
                {
                    int idx_In = j * windowSize_X + i;
                    int idx_Out = (windowSize_Y - 1 - j) * windowSize_X + (windowSize_X - 1 - i);
                    Color c = pixels_In[idx_In];
                    //if (c.grayscale > 0.2f)
                    //    c = c.AdjustBrightness(c.grayscale.Retargeted(0.2f, 1, 0.2f, 0.35f));
                    pixels_Out[idx_Out] = c;
                }

            output.SetPixels(pixels_Out);
            output.Apply();

            return output;
        }

        public static Texture2D Crop_FromCenter_X(this Texture2D original, Texture2D output, float xPercentile)
        {
            return original.Crop_FromCenter(output, xPercentile, -1);
        }

        public static Texture2D Crop_FromCenter_Y(this Texture2D original, Texture2D output, float yPercentile)
        {
            return original.Crop_FromCenter(output, -1, yPercentile);
        }

        public static Texture2D Crop_FromCenter_X(this Texture2D original, float xPercentile)
        {
            return original.Crop_FromCenter(xPercentile, -1);
        }

        public static Texture2D Crop_FromCenter_Y(this Texture2D original, float yPercentile)
        {
            return original.Crop_FromCenter(-1, yPercentile);
        }

        public static Texture2D Crop_FromCenter(this Texture2D original, float xPercentile, float yPercentile)
        {
            if (xPercentile < 0 && yPercentile < 0) return original;

            int windowSize_X = (int)(original.width * xPercentile);
            int windowSize_Y = (int)(original.height * yPercentile);

            if (xPercentile < 0)
                windowSize_X = windowSize_Y;
            if (yPercentile < 0)
                windowSize_Y = windowSize_X;

            int x_window = (original.width - windowSize_X) / 2;
            int y_window = (original.height - windowSize_Y) / 2;

            Color[] pixels_In = original.GetPixels(x_window, y_window, windowSize_X, windowSize_Y);
            Color[] pixels_Out = new Color[pixels_In.Length];

            Texture2D t = new Texture2D(windowSize_X, windowSize_Y);
            for (int i = 0; i < windowSize_X; i++)
                for (int j = 0; j < windowSize_Y; j++)
                {
                    int idx_In = j * windowSize_X + i;
                    int idx_Out = (windowSize_Y - 1 - j) * windowSize_X + (windowSize_X - 1 - i);
                    Color c = pixels_In[idx_In];
                    //if (c.grayscale > 0.2f)
                    //    c = c.AdjustBrightness(c.grayscale.Retargeted(0.2f, 1, 0.2f, 0.35f));
                    pixels_Out[idx_Out] = c;
                }

            t.SetPixels(pixels_Out);
            t.Apply();

            return t;
        }

        public static void ToGrayscale(this RectTransform rT)
        {
            if (rT == null) return;
            Image im = rT.GetComponent<Image>();
            if (im == null) return;
            im.ToGrayscale();
        }

        public static void ToGrayscale(this Image im)
        {
            if (im == null) return;
            Sprite s = im.sprite;
            if (s == null) return;
            im.sprite = s.ToGrayscale();
        }

        public static Sprite ToGrayscale(this Sprite s)
        {
            Texture2D t = s.texture;
            if (t == null) return s;

            Rect r = new Rect(0, 0, t.width, t.height);
            t = t.ToGrayscale();

            return Sprite.Create(t, r, Vector2.one * 0.5f);
        }

        public static Texture2D ToGrayscale(this Texture2D t2D)
        {
            Texture2D tGS = new Texture2D(t2D.width, t2D.height);

            var texColors = t2D.GetPixels();
            for (int i = 0; i < texColors.Length; i++)
            {
                var grayValue = texColors[i].grayscale;
                texColors[i] = new Color(grayValue, grayValue, grayValue, texColors[i].a);
            }

            tGS.SetPixels(texColors);
            tGS.Apply();

            return tGS;
        }
        #endregion


        #region Utility
        public static Vector2 GetPixelSize(this SpriteRenderer spriteRenderer, Camera camera = null)
        {
            if (spriteRenderer == null) return Vector2.zero;

            if (spriteRenderer.sprite == null) return Vector2.zero;

            float pixelsPerUnit = spriteRenderer.sprite.pixelsPerUnit;

            // Get top left corner
            float offsetRight = spriteRenderer.sprite.rect.size.x / 2f / pixelsPerUnit;
            float offsetUp = spriteRenderer.sprite.rect.size.y / 2f / pixelsPerUnit;

            Vector2 localRight = Vector2.right * offsetRight;
            Vector2 localUp = Vector2.up * offsetUp;

            // Go to world
            Vector2 worldRight = spriteRenderer.transform.TransformPoint(localRight);
            Vector2 worldUp = spriteRenderer.transform.TransformPoint(localUp);
            Vector2 worldCenter = spriteRenderer.transform.position;

            // Go to pixels
            Vector2 coordsRight = GetPixelCoordinates(worldRight, camera);
            Vector2 coordsUp = GetPixelCoordinates(worldUp, camera);
            Vector2 coordsCenter = GetPixelCoordinates(worldCenter, camera);

            // Get sizes
            float pixelsRight = Vector2.Distance(coordsCenter, coordsRight);
            float pixelsUp = Vector2.Distance(coordsCenter, coordsUp);

            Vector2 itemSize = Vector2.right * pixelsRight * 2 + Vector2.up * pixelsUp * 2;

            return itemSize;
        }

        public static Vector2 GetPixelCoordinates(this Transform transform, Camera camera = null)
        {
            if (transform == null) return Vector2.zero;

            return GetPixelCoordinates(transform.position, camera);
        }

        private static Vector2 GetPixelCoordinates(Vector3 position, Camera camera)
        {
            if (camera == null)
                camera = Camera.main;

            if (camera == null) return Vector2.zero;

            return camera.WorldToScreenPoint(position);
        }

        public static bool IsCurrentEventSystemFree()
        {
            return EventSystem.current.currentSelectedGameObject == null;
        }

        public static bool IsSelected(this Selectable selectable)
        {
            if (selectable == null) return false;
            return EventSystem.current.currentSelectedGameObject == selectable.gameObject;
        }

        public static Canvas GetHighestCanvas<T>(this T component) where T : Component
        {
            Canvas[] parentCanvases = component.GetComponentsInParent<Canvas>();
            if (parentCanvases != null && parentCanvases.Length > 0)
            {
                return parentCanvases[parentCanvases.Length - 1];
            }
            return null;
        }

        public static Canvas GetLowestCanvas<T>(this T component) where T : Component
        {
            Canvas[] parentCanvases = component.GetComponentsInParent<Canvas>();
            if (parentCanvases != null && parentCanvases.Length > 0)
            {
                return parentCanvases[0];
            }
            return null;
        }

        public static void Toggle(this IList<CanvasGroup> canvasGroupList, bool on)
        {
            if (canvasGroupList == null) return;
            foreach (CanvasGroup cG in canvasGroupList)
                cG.Toggle(on);
        }

        // private static readonly Dictionary<CanvasGroup, Coroutine> cGCRs = new Dictionary<CanvasGroup, Coroutine>();

        public static void Toggle(this CanvasGroup cG, bool on)
        {
            if (!cG) return;

            cG.alpha = on ? 1 : 0;
            cG.blocksRaycasts = on;
            LayoutElement lE = cG.GetComponent<LayoutElement>();
            if (lE)
            {
                lE.enabled = !on;
                lE.ignoreLayout = !on;
            }
        }

        public static bool LogicalAnd(this IList<Toggle> list)
        {
            bool and = true;
            foreach (Toggle t in list)
            {
                if (!t.isOn)
                {
                    and = false;
                    break;
                }
            }
            return and;
        }

        public static bool LogicalOr(this IList<Toggle> list)
        {
            bool or = false;
            foreach (Toggle t in list)
            {
                if (t.isOn)
                {
                    or = true;
                    break;
                }
            }
            return or;
        }
        #endregion
    }
}