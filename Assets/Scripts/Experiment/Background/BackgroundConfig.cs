using Helpers.Misc;
using System;
using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;

namespace Experiment.Background
{
    /// <summary>
    /// NS_SEGMENT | Class + Helpers
    /// </summary>
    [Serializable]
    public class BackgroundConfig
    {
        [SerializeField] private string numFramesCorrector_Comment = "Can be used if FillerDetails.csv last longer than Timings.csv";
        public float numFramesCorrector = 2.0f;
        public string wantedOffset01_Comment = "Taking values from -1 to 1, determines how the area outside the active area is distributed";
        public Vector2 wantedOffset01 = Vector2.zero;
        public string useActiveAreaInsteadOfFullScreen_Comment = "If set to true, player & Spawn anchors work w.r.t. _activeAreaVerticalOffsetFromScreenEdges. Otherwise, w.r.t. the screen edges.";
        public bool useActiveAreaInsteadOfFullScreen = true;
        public string cropOutsideActiveArea_Comment = "Only used when useActiveAreaInsteadOfFullScreen is set to true";
        /// <summary>
        /// [SOS] You probably want <see cref="usingActiveAreaAndCropEnabled"/> instead
        /// </summary>
        public bool cropOutsideActiveArea = false;
        public bool usingActiveAreaAndCropEnabled { get { return useActiveAreaInsteadOfFullScreen && cropOutsideActiveArea; } }
        public MinMax periodMinMax = new MinMax();

        public float verticalCenter_Runner = 0.5f;
        public float horizontalCenter_Runner = 0.5f;
        public float verticalCenter_Duet = 0.35f;

        // public float GetVerticalCenter(GameType gameType) { return gameType == GameType.Duet ? verticalCenter_Duet : verticalCenter_Runner; }
        public float GetHorizontalCenter() { return horizontalCenter_Runner; }

        public bool useBlobs = true;
        public bool useCandidateBrightness = true;
        public float percentileToPaint_Satellite = 0.5f;
        public float percentileToPaint_MainBlobs = 0.5f;

        public string factorInCriticalSquares_Comment = "When true, the 4 critical squares are considered main squares as well and factored in when calculating how many main to paint";
        public bool factorInCriticalSquares = true;


        /// <summary>
        /// [DEPRECATED 200508] Only used by <see cref="BackgroundManager_Abstract_RotatingSquares"/>
        /// </summary>
        public float distractorImageChance_General { get { return 0.2f; } }
        /// <summary>
        /// [DEPRECATED 200508] Only used by <see cref="BackgroundManager_Abstract_RotatingSquares"/>
        /// </summary>
        public float distractorImageChance_Main { get { return 0.5f; } }

        public float satelliteScaleMulti = 0.2f;

        public float cycleDuration = 0.5f;
        public float peakDuration { get { return peakDurationMS / 1000f; } }
        [SerializeField] private int peakDurationMS = 500;

        // [DEPRECATED? 200508]
        // public float peakEaseIn = 0.2f;
        // public float peakEaseOut = 0.2f;

        public MinMax stayAtLowest = new MinMax(0f, 0.05f);
        public bool useBlinkingSquaresAnimation = true;
        public bool useAnimationOut = false;
        public Color gridColor = Color.gray;
        public float gridWidth = 0.05f;

        public int NUM_QUADRANT_ROWS = 5;
        public int NUM_QUADRANT_COLUMNS = 5;

        // [TODO] This should be based inversely on SCALE not visual angle
        public int GetNumBackgroundObjects(float wantedBackgroundScale)
        {
            // At a Scale of 1.0 we want  ~73 objects (put 100 to be safe)
            float referenceScale = 1.0f;

            // The closer they are the more they need to be
            float numObjectsAtRefScale = 200; // / mainObjectDistanceMulti;

            // The more the background scale goes up, the less objects we need (they won't fit in the screen)
            float multiplier = 1 / (wantedBackgroundScale / referenceScale);

            int numObjects = Mathf.CeilToInt(numObjectsAtRefScale * multiplier);

            this.Log("Set up Background to scale :: {0} -> {1} objects"._Format(wantedBackgroundScale, numObjects));

            return numObjects;
        }

        // Roughly speaking, divide the screen in 5 columns and 3 rows, and consider only some of them
        public List<Vector2Int> acceptableQuadrants = new List<Vector2Int>()
    {
        new Vector2Int(1, 0),
        new Vector2Int(1, 1),
        new Vector2Int(1, 2),

        new Vector2Int(2, 0),

        new Vector2Int(3, 0),
        new Vector2Int(3, 1),
        new Vector2Int(3, 2),
    };

        public bool resetRandomnessAtBeginOfAnimCycle = true;
        public float alphaMain = 0.2f;
        public float brightnessMain = 0.6f;
        public float brightnessStimulus = 1f;
        public bool useBrightnessStimulus = true;
        public MinMax alphaOverlay_Peak_Low; // 0.6f;
        public MinMax alphaOverlay_Peak_High; // 0.6f;
        public MinMax brightnessOverlay_Peak_Low; // 0.6f;
        public MinMax brightnessOverlay_Peak_High; // 0.6f;

        public List<AnimationPart_Randomizable> mainAnimationParts = new List<AnimationPart_Randomizable>() { };

        public List<AnimationPart_Randomizable> satelliteAnimationParts = new List<AnimationPart_Randomizable>() { };

        public List<AnimationPart_Randomizable> blinkingAnimationParts = new List<AnimationPart_Randomizable>() { };

        // [TODO] Figure why this is needed & remove
        // public float physicalSizeToScaleCorrector = 1.00f;

        // The scale that corresponds to the scale of a fully grown STIMULUS in the EDITOR
        public float wantedVisualAngleStimulus = 2.5f;

        // The scale that corresponds to the scale of the player model
        public float wantedVisualAnglePlayer = 2.5f;

        // The scale that corresponds to the scale of the falling essences
        public float wantedVisualAngleEssences = 2.5f;

        // The scale that corresponds to the scale of the falling essences
        public float wantedVisualAngleLanes = 5.0f;

        /// <summary>
        /// Use <see cref="eccentricityWrtStimulus"/> instead
        /// </summary>
        public float wantedVisualAngleEccentricity = 7.5f;

        public float wantedVisualAngleFixation = 2.5f;

        public float maxVisualAngleDeviationFromFixation = 2.0f;

        public float eccentricityWrtStimulus { get { return wantedVisualAngleEccentricity / wantedVisualAngleStimulus; } }

        public float wantedSubjectDistance_CM = 57;

        /// <summary>
        /// [SOS] UNRELIABLE, could be purged
        /// </summary>
        public const float screenDPI = 120;
        // public MinMax screenDPI_Limits = new MinMax(10, 300);

        public float cameraSize { get { return defaultCameraSize / zoomFactor; } }
        /// <summary>
        /// [SOS] Everything has been set-up assuming this is the camera reference size. Don't change unless you know what you're doing (or something is really, really broken.
        /// </summary>
        public static float defaultCameraSize = 4.6f;

        public float zoomFactor { get { return zoomFactorBase * zoomFactorLab; } }

        public float zoomFactorBase { get { return zoomIndexBase < 0.5f ? zoomIndexBase.Retargeted(0, 0.5f, minZoomFactorBase, 1) : zoomIndexBase.Retargeted(0.5f, 1.0f, 1, maxZoomFactorBase); } }
        public float zoomIndexBase = 0.5f;
        public float minZoomFactorBase = 0.50f;
        public float maxZoomFactorBase = 2.00f;

        public float zoomFactorLab { get { return zoomIndexLab < 0.5f ? zoomIndexLab.Retargeted(0, 0.5f, minZoomFactorLab, 1) : zoomIndexLab.Retargeted(0.5f, 1.0f, 1, maxZoomFactorLab); } }

        public static void SetUseMachine(Machine machine)
        {
            BackgroundConfig.machine = machine;
        }

        private static Machine machine;

        public enum Machine { Behavioral, Experimental }

        public float zoomIndexLab
        {
            get { return machine == Machine.Behavioral ? zoomIndexLab_Behavioral : zoomIndexLab_Experimental; }
            set { if (machine == Machine.Behavioral) zoomIndexLab_Behavioral = value; else zoomIndexLab_Experimental = value; }
        }

        public float zoomIndexLab_Behavioral = 0.5f;
        public float zoomIndexLab_Experimental = 0.5f;

        public float minZoomFactorLab = 0.0f;
        public float maxZoomFactorLab { get { return 1f / activeAreaPercentileUnscaled; } }

        private float activeAreaPercentileUnscaled { get { return 1 - 2 * _activeAreaVerticalOffsetFromScreenEdges; } }

        public float backgroundVerticalPositionPercentileUnscaled = 0.5f;
        public float backgroundVerticalPositionBaseOffset_DO_NOT_CHANGE = 5f;

        public float GetBackgroundVerticalPositionScaled(Camera camera)
        {
            float unscaledOffsetFromHalf = backgroundVerticalPositionPercentileUnscaled - 0.5f;
            return GetVerticalPositionScaled(unscaledOffsetFromHalf, camera.orthographicSize);
        }

        public float fixationVerticalPositionPercentileUnscaled = 0.5f;

        public float GetFixationVerticalPositionScaled(Camera camera)
        {
            float unscaledOffsetFromHalf = fixationVerticalPositionPercentileUnscaled - 0.5f;
            return GetVerticalPositionScaled(unscaledOffsetFromHalf, camera.orthographicSize);
        }
        public string activeAreaVerticalOffsetFromScreenEdges_Comment = "Defines the active area";
        public float _activeAreaVerticalOffsetFromScreenEdges = 0.15f;
        public float activeAreaVerticalOffsetFromScreenEdges { get { return useActiveAreaInsteadOfFullScreen ? _activeAreaVerticalOffsetFromScreenEdges : 0f; } }


        public bool IsInsideActiveArea(Camera camera, Transform transform)
        {
            Rect worldArea = GetActiveAreaWorld(camera);

            return Utility_Helper.IsInsideArea(transform, worldArea);
        }

        public Rect GetActiveAreaWorld(Camera camera)
        {
            // [TODO}
            // if (!usingActiveAreaAndCropEnabled) return Utility_Helper.GetAreaFullScreen();

            float top = GetTopScaled(camera);
            float bottom = GetBottomScaled(camera);
            float height = top - bottom;
            float width = (float)Screen.width / Screen.height * height;
            float left = -width / 2f;
            // float right = width / 2f;

            Vector2 position = new Vector2(left, bottom);
            Vector2 size = new Vector2(width, height);

            Rect worldArea = new Rect(position, size);

            // Debug.Log("TOP :: " + top + "\nBOTTOM :: " + bottom);

            return worldArea;
        }

        public float spawnVerticalPositionPercentileUnscaled_Base { get { return 1 - activeAreaVerticalOffsetFromScreenEdges; } }
        public string spawnVerticalPositionPercentileUnscaled_Offset_Comment = "Not used in the calculation of active area";
        public float spawnVerticalPositionPercentileUnscaled_Offset = 0.0f;
        public float spawnVerticalPositionPercentileUnscaled { get { return spawnVerticalPositionPercentileUnscaled_Base + spawnVerticalPositionPercentileUnscaled_Offset; } }

        public float GetSpawnVerticalPositionScaled(Camera camera)
        {
            float unscaledOffsetFromHalf = spawnVerticalPositionPercentileUnscaled - 0.5f;
            return GetVerticalPositionScaled(unscaledOffsetFromHalf, camera.orthographicSize);
        }

        public float playerVerticalPositionPercentileUnscaled_Base { get { return activeAreaVerticalOffsetFromScreenEdges; } }
        public string playerVerticalPositionPercentileUnscaled_Offset_Comment = "Not used in the calculation of active area";
        public float playerVerticalPositionPercentileUnscaled_Offset = 0.04f;
        public float playerVerticalPositionPercentileUnscaled { get { return playerVerticalPositionPercentileUnscaled_Base + playerVerticalPositionPercentileUnscaled_Offset; } }

        public float GetTopScaled(Camera camera)
        {
            float unscaledOffsetFromHalf = spawnVerticalPositionPercentileUnscaled_Base - 0.5f;
            return GetVerticalPositionScaled(unscaledOffsetFromHalf, camera.orthographicSize);
        }

        public float GetBottomScaled(Camera camera)
        {
            float unscaledOffsetFromHalf = playerVerticalPositionPercentileUnscaled_Base - 0.5f;
            return GetVerticalPositionScaled(unscaledOffsetFromHalf, camera.orthographicSize);
        }

        public float GetPlayerVerticalPositionScaled(Camera camera)
        {
            float unscaledOffsetFromHalf = playerVerticalPositionPercentileUnscaled - 0.5f;
            return GetVerticalPositionScaled(unscaledOffsetFromHalf, camera.orthographicSize);
        }

        // Center is at 0.5
        private float GetVerticalPositionScaled(float unscaledOffsetFromHalfAsPercentileOfFull, float cameraSize)
        {
            float fullSize = cameraSize * 2;

            float unscaledOffsetFromHalf = unscaledOffsetFromHalfAsPercentileOfFull * fullSize;

            // For full screen
            if (!useActiveAreaInsteadOfFullScreen)
                return unscaledOffsetFromHalf;

            float scaledOffsetFromHalf = unscaledOffsetFromHalf * zoomFactor;

            return scaledOffsetFromHalf;
        }

        public float viewportSizePercentile { get { return activeAreaPercentileUnscaled * zoomFactor; } }
        public Color viewportBlockingColor { get { return _viewportBlockingColor.ToColor(); } }
        public string _viewportBlockingColor = Color.black.ToHex();

        // public bool useVisualAngle = true;

        public float GetScaleMultiplierStimulus(Camera camera)
        {
            float DO_NOT_CHANGE_ReferenceScale = 1.000f;
            return GetScaleMultiplier(camera, wantedVisualAngleStimulus) / DO_NOT_CHANGE_ReferenceScale;
        }

        public float GetScaleMultiplierPlayer(Camera camera)
        {
            float DO_NOT_CHANGE_ReferenceScale = 0.900f;
            return GetScaleMultiplier(camera, wantedVisualAnglePlayer) / DO_NOT_CHANGE_ReferenceScale;
        }

        public float GetScaleMultiplierEssences(Camera camera)
        {
            float DO_NOT_CHANGE_ReferenceScale = 2.800f;
            return GetScaleMultiplier(camera, wantedVisualAngleEssences) / DO_NOT_CHANGE_ReferenceScale;
        }

        public float GetScaleMultiplierLanes(Camera camera)
        {
            float DO_NOT_CHANGE_ReferenceScale = 2.800f;
            return GetScaleMultiplier(camera, wantedVisualAngleLanes) / DO_NOT_CHANGE_ReferenceScale;
        }

        public float GetScaleMultiplierFixation(Camera camera)
        {
            float DO_NOT_CHANGE_ReferenceScale = 4.680f;
            return GetScaleMultiplier(camera, wantedVisualAngleFixation) / DO_NOT_CHANGE_ReferenceScale;
        }

        private float GetScaleMultiplier(Camera camera, float visualAngle)
        {
            // if (!useVisualAngle) return 1.0f;

            // Either convert defaultScale to wantedVisual or vice versa to get a ratio
            return GetScaleFromVisualAngle(screenDPI, visualAngle, wantedSubjectDistance_CM, camera.orthographicSize, 1.0f) * zoomFactor;
        }

        public float GetVisualAngle(float size)
        {
            return VisualAngleMath.CalculateVisualAngle(size, wantedSubjectDistance_CM);
        }

        public float GetScaleMultiplier(SpriteRenderer sR)
        {
            if (sR == null)
            {
                // this.LogWarning("Null SR!");
                return 1;
            }

            if (sR.sprite == null)
            {
                // this.LogWarning("Null SR SPRITE");
                return 1;
            }

            // if (!useVisualAngle) return 1.0f;

            float DO_NOT_CHANGE_DefaultScale = 0.3f;

            //float spriteSizeX = sR.sprite.bounds.size.x;

            return 1 / DO_NOT_CHANGE_DefaultScale / sR.sprite.bounds.size.x;
        }

        // Scale everything up or down, not just objects
        public static float GetScaleFromVisualAngle(float dpi, float wantedVisualAngle, float wantedSubjectDistance, float cameraOrthographicSize, float physicalSizeToScaleCorrector)
        {
            float wantedPhysicalSize = GetPhysicalSizeFromVisualAngle(wantedVisualAngle, wantedSubjectDistance);
            float wantedScale = GetScaleFromPhysicalSize(dpi, wantedPhysicalSize, cameraOrthographicSize, physicalSizeToScaleCorrector);

            return wantedScale;
        }

        public static float GetPhysicalSizeFromVisualAngle(float wantedVisualAngle, float wantedSubjectDistance)
        {
            return VisualAngleMath.CalculateSize(wantedVisualAngle, wantedSubjectDistance);
        }

        public static float GetScaleFromPhysicalSize(float dpi, float wantedPhysicalSizeCM, float cameraOrthographicSize, float corrector, bool debug = false)
        {
            // [TODO] This part is calculated WRONGLY
            // float dpi = GetScreenDPI();

            // Works correctly from here on out!!
            // dpi = 145; // <---- TEST WITH THIS on lenovo y720 it should output ~"34"
            bool DISABLE_AUTO_SCALING = true;

            float screenWidthCM = DISABLE_AUTO_SCALING ? GetCMsFromPixels(1920, dpi, debug) : GetScreenWidthCM(dpi, debug);

            // Works correctly from here on out!!
            // screenWidthCM = 34; // <---- TEST WITH THIS on lenovo legion y720 -> Measure that the red box is A7 height (7.4CMs)

            // So now we have physical size of the screen - how much of that we want covered by our object?
            float screenPercentile = wantedPhysicalSizeCM / screenWidthCM;

            // Works correctly from here on out!!
            // screenPercentile = 0.5f; // <---- TEST WITH THIS (measure that the RED box is half the screen)
            float scale = GetScaleFromScreenPercentile(screenPercentile, cameraOrthographicSize, corrector);

            return scale;
        }

        /// <summary>
        /// [SOS] Does NOT work
        /// </summary>
        /// <returns></returns>
        public static float GetScreenDPI()
        {
            // Lenovo DPI :: 143dpiX, 148dpiY -> 145 dpiDiag
            float dpi = Screen.dpi; // <--- THIS IS BULLSHIT

            return dpi;
        }

        public float GetCMFromPixels(float pixels)
        {
            return GetCMsFromPixels(pixels, screenDPI);
        }

        public float GetPixelsFromCM(float cm)
        {
            return GetPixelsFromCM(cm, screenDPI);
        }

        public static float GetScreenWidthCM(float dpiX, bool debug = false)
        {
            // We have physical size (CM) on Screen - first turn this into percentage of the screne (clean number)

            // Lenovo CMs :: 34x18.5

            // Lenovo Inches :: 13.39"x7.28" -> 15.24" diag
            // Lenovo Pixels :: 1920p x1080p -> 2,203p diag

            // Lenovo DPI :: 143dpiX, 148dpiY -> 145 dpiDiag

            return GetCMsFromPixels(Screen.width, dpiX, debug);
        }

        private static float GetCMsFromPixels(float pixels, float dpi, bool debug = false)
        {
            float inchToCM = 2.54f;

            float cms = pixels / dpi * inchToCM; // <---- This is the same!!

            if (debug)
                Debug_Helper.Log(typeof(BackgroundConfig), "CMs :: " + cms);

            return cms;
        }

        private static float GetPixelsFromCM(float cm, float dpi, bool debug = false)
        {
            float inchToCM = 2.54f;

            float pixels = cm * dpi / inchToCM; // <---- This is the same!!

            if (debug)
                Debug_Helper.Log(typeof(BackgroundConfig), "Pixels :: " + pixels);

            return pixels;
        }

        public static float GetScaleFromScreenPercentile(float screenPercentile, float cameraOrthographicSize, float corrector)
        {
            // Starting from scale as percentile of screen
            float scale = screenPercentile;

            // And the orthographic camera size (bigger camera sizes need proportionately smaller scale)
            // Basically, the orthographic size value represents the amount of in-game units (meters) half of your vertical screen size takes. 

            // If instead it's perspective-based, * FoV / 25 (for some reason)
            float inGameUnitsOfScreenHeight = cameraOrthographicSize * 2;
            float fromHeightToWidth = (float)Screen.width / Screen.height;

            float inGameUnitsOfScreenWidth = inGameUnitsOfScreenHeight * fromHeightToWidth;

            scale *= inGameUnitsOfScreenWidth;

            // And magic (helps having this as slider as we figure out what's missing from the equation)
            scale *= corrector;

            return scale;
        }

        public float GetCMFromVisualAngle(float visualAngle)
        {
            return VisualAngleMath.CalculateSize(visualAngle, wantedSubjectDistance_CM);
        }

        /*
        public float GetScaleMultiplier(Camera camera, SpriteRenderer sR)
        {
            if (!useVisualAngle) return 1.0f;

            float DO_NOT_CHANGE_DefaultScale = 0.3f;

            // Either convert defaultScale to wantedVisual or vice versa to get a ratio
            float wantedScaleStimulus = GetScaleFromVisualAngle(GetWantedVisualAngleStimulusSquare(), wantedSubjectDistance_CM, camera.orthographicSize, sR.sprite.bounds.size.x, physicalSizeToScaleCorrector);
            return wantedScaleStimulus / DO_NOT_CHANGE_DefaultScale;
        }

        public static float GetScaleFromVisualAngle(float wantedVisualAngle, float wantedSubjectDistance, float cameraOrthographicSize, float sRSpriteBoundsSizeX, float physicalSizeToScaleCorrector)
        {
            float wantedPhysicalSize = GetPhysicalSizeFromVisualAngle(wantedVisualAngle, wantedSubjectDistance);
            float wantedScale = GetScaleFromPhysicalSize(wantedPhysicalSize, cameraOrthographicSize, sRSpriteBoundsSizeX, physicalSizeToScaleCorrector);

            return wantedScale;
        }

        public static float GetScaleFromPhysicalSize(float wantedPhysicalSizeCM, float cameraOrthographicSize, float sRSpriteBoundsSizeX, float corrector)
        {
            // Starting from scale as percentile of screen
            float screenPercentile = GetPhysicalSizeAsPercentileOfScreen(wantedPhysicalSizeCM);

            float scale = GetScaleFromScreenPercentile(screenPercentile, cameraOrthographicSize, sRSpriteBoundsSizeX, corrector);

            return scale;
        }

        public static float GetPhysicalSizeFromVisualAngle(float wantedVisualAngle, float wantedSubjectDistance)
        {
            return VisualAngleMath.CalculateSize(wantedVisualAngle, wantedSubjectDistance);
        }

        /// <summary>
        /// Check! This part works.
        /// </summary>
        public static float GetPhysicalSizeAsPercentileOfScreen(float wantedPhysicalSizeCM, bool debug = false)
        {
            // We have physical size (CM) on Screen - first turn this into percentage of the screne (clean number)

            float inchToCM = 2.54f;

            float screenWidthPixel = Screen.width;
            float screenInchPerPixel = 1 / Screen.dpi;
            float screenCMPerPixel = screenInchPerPixel * inchToCM;
            float screenWidthCM = screenWidthPixel * screenCMPerPixel;

            if (debug)
            {
                Debug.Log("Screen Width Pixels :: " + screenWidthPixel);
                Debug.Log("Screen Inches Per Pixel :: " + screenInchPerPixel);
                Debug.Log("Screen CMs Per Pixel :: " + screenCMPerPixel);
                Debug.Log("Screen Width CMs :: " + screenWidthCM);
            }

            // So now we have physical size of the screen - how much of that we want covered by our object?
            float percentileOfScreen = wantedPhysicalSizeCM / screenWidthCM;

            return percentileOfScreen;
        }

        public static float GetScaleFromScreenPercentile(float screenPercentile, float cameraOrthographicSize, float sRSpriteBoundsSizeX, float corrector)
        {
            // Starting from scale as percentile of screen
            float scale = screenPercentile;

            // Factor in the sprite's size (bigger sprite sizes need proportionately smaller scale)
            scale /= sRSpriteBoundsSizeX;

            // And the orthographic camera size (bigger camera sizes need proportionately smaller scale)
            // Basically, the orthographic size value represents the amount of in-game units (meters) half of your vertical screen size takes. 

            // If instead it's perspective-based, * FoV / 25 (for some reason)
            float inGameUnitsOfScreenHeight = cameraOrthographicSize * 2;
            float fromHeightToWidth = (float)Screen.width / Screen.height;

            float inGameUnitsOfScreenWidth = inGameUnitsOfScreenHeight * fromHeightToWidth;

            scale *= inGameUnitsOfScreenWidth;

            // And magic (helps having this as slider as we figure out what's missing from the equation)
            scale *= corrector;

            return scale;
        }
        */
    }
}