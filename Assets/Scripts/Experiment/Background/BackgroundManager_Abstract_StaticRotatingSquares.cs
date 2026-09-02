using System.Collections.Generic;
using System.Linq;
using TGP.Helpers;
using UnityEngine;

namespace Experiment.Background
{
    /// <summary>
    /// [SEGMENT] It's a bit large, but coherent ; maybe give it helper script
    /// </summary>
    public class BackgroundManager_Abstract_StaticRotatingSquares : BackgroundManager_Abstract
    {
        [SerializeField] private Transform bottomLeftCorner = null;
        [SerializeField] private Transform topRightCorner = null;

        // [SerializeField, Range(0.2f, 0.6f)] private float mainScaleMin = 0.4f;
        // [SerializeField, Range(0.1f, 0.5f)] private float satelliteScaleMin = 0.3f;
        // [SerializeField, Range(0.6f, 1.0f)] private float satelliteScaleMax = 0.8f;

        [SerializeField] private bool debugDynamicGrid = false;

        private Vector2 distanceBetweenMainObjects;

        /*
#if UNITY_EDITOR
private void Start()
{
    if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ToLower().Contains("test"))
    {
        Debug.LogError("AUTO STARTING BackgroundManager for testing");
        string data = System.IO.File.ReadAllText(@"C:\Coding\ReedCollege\Seattle Project (Unity Project)\Logs\gameTiming.json");
        Precalculator.gameTimings = JsonUtility.FromJson<GameTimings>(data);
        MinMax backgroundMinMax = ApplicationLibrary.Config.GetPeriodMinMax_Background();
        Initialize(backgroundMinMax);
    }
}
#endif
        */

        protected override void Initialize(Config config, RuntimeConfig runtimeConfig)
        {
            base.Initialize(config, runtimeConfig);

            bottomLeftCorner.gameObject.SetActive(false);
            topRightCorner.gameObject.SetActive(false);
        }

        protected override BackgroundType GetBackgroundType()
        {
            return BackgroundType.Abstract_RotatingSquares;
        }

        protected override void BeginNewAnimationCycle()
        {
            base.BeginNewAnimationCycle();

            if (config.resetRandomnessAtBeginOfAnimCycle)
                foreach (BackgroundObject bO in GetBackgroundObjects())
                    bO.ResetRandomizationOffset();
        }

        private bool IsWithinBounds(BackgroundObject bO)
        {
            // This should work but it doesnt - we need a definitive way of checking if object is being rendered.
            return true;

            return bO.transform.position.x > bottomLeftCorner.position.x &&
                    bO.transform.position.y > bottomLeftCorner.position.y &&
                    bO.transform.position.x < topRightCorner.position.x &&
                    bO.transform.position.y < topRightCorner.position.y;
        }

        protected override List<BackgroundObject> OnPaintSatellites()
        {
            // Unpaint all Satellites
            UnpaintAll(false, true);
            // Select all visible satellites in background objects
            List<BackgroundObject> validSatellites = GetBackgroundObjects().FindAll(bjObject =>
                                            bjObject.isVisible &&
                                            (bjObject as BackgroundObject_Abstract_RotatingSquares).isSatellite)
                                            .Shuffle().ToList();

            // Paint only some of them
            if (validSatellites.Count == 0)
                this.LogError("No valid satellites to paint. Try increasing zoom.");
            else
            {
                int numSatellitesToPaint = Mathf.RoundToInt(validSatellites.Count * config.percentileToPaint_Satellite);


                if (numSatellitesToPaint <= 0)
                    this.LogWarning("No need to paint any SATELLITE squares - try raising the config percentile.");
                else
                {
                    numSatellitesToPaint = Mathf.Clamp(numSatellitesToPaint, 0, validSatellites.Count - 1);
                    validSatellites = validSatellites.GetRange(0, numSatellitesToPaint);
                    RaiseOnBlobsNeedPainting(validSatellites);
                }
            }

            return validSatellites;
        }

        protected override List<BackgroundObject> OnPaintMainAnchorObjects()
        {
            // Unpaint all Main squares
            UnpaintAll(true, false);

            // [NOT USED (there are no anchor squares in this setup, they are all considered SATELLITES)
            // Select all visible anchor squares - BUT Exclude main stimulus squares (4 center)
            List<BackgroundObject> validMainSquares = GetBackgroundObjects().FindAll(bgObject =>
                                            bgObject.isVisible &&
                                            !(bgObject as BackgroundObject_Abstract_RotatingSquares).isSatellite &&
                                            !bgObject.IsStimulusCandidate())
                                            .Shuffle().ToList();

            // Paint only some of them
            if (validMainSquares.Count == 0)
                this.LogError("No MAIN squares to paint. Try increasing zoom to fit some in the screen.");
            else
            {
                int numTotalSquaresShowing = validMainSquares.Count;

                if (config.factorInCriticalSquares) // 3-4 of those are painted already, so we need 
                    numTotalSquaresShowing += 4;

                // Paint only some of them
                int numMainSquaresToPaint = Mathf.RoundToInt(numTotalSquaresShowing * config.percentileToPaint_MainBlobs);

                if (config.factorInCriticalSquares)
                    numMainSquaresToPaint -= 4; // the 4 main are already painted

                if (numMainSquaresToPaint <= 0)
                    this.LogWarning("No need to paint any MAIN squares - the 4 CRITICAL squares were sufficient - try raising the config percentile");
                else
                {
                    numMainSquaresToPaint = Mathf.Clamp(numMainSquaresToPaint, 0, validMainSquares.Count - 1);
                    validMainSquares = validMainSquares.GetRange(0, numMainSquaresToPaint);
                    RaiseOnBlobsNeedPainting(validMainSquares);
                }
            }

            // Paint main objects with respect to chosen Stimulus
            RaiseOnCriticalSquaresPeak(GetBackgroundObjects().FindAll(bgObject => bgObject.IsStimulusCandidate()).ToList());

            return new List<BackgroundObject>();
        }


        protected void UnpaintAll(bool unpaintMain, bool unpaintSatellite)
        {
            // Randomly paint all squares
            for (int i = 0; i < backgroundObjects.Count; i++)
            {
                bool isSatellite = (backgroundObjects[i] as BackgroundObject_Abstract_RotatingSquares).isSatellite;
                if ((!isSatellite && unpaintMain) || (isSatellite && unpaintSatellite))
                {
                    backgroundObjects[i].UnPaint();
                }
            }
        }

        private List<BackgroundObject> _backgroundObjects;
        protected override void HandleMasterAnimation(float t01)
        {
            _backgroundObjects = GetBackgroundObjects();
            BackgroundObject bO;
            BackgroundObject_Abstract_RotatingSquares bO_A_RS;
            AnimationPart animationPart;

            for (int i = 0; i < _backgroundObjects.Count; i++)
            {
                bO = _backgroundObjects[i];

                //if (!IsWithinBounds(bO))
                //    continue;

                if (!bO.isVisible)
                    continue;

                // Factor in randomization (But NOT at their timing)
                // float bO_R01 = bO.GetRandomizationOffset_01();
                // float multiplier_05_2 = bO_R01.RetargetedFrom0_1To05_2();
                // float t01_bO = t01; // (t01 + bO_R01).NegMod(1);

                bO_A_RS = bO as BackgroundObject_Abstract_RotatingSquares;

                // Make them match each other
                bO_A_RS.AdjustMain(config.alphaMain, config.brightnessMain);
                bO_A_RS.AdjustOverlay(config.alphaMain, config.brightnessMain);

                List<AnimationPart_Randomizable> animParts = bO_A_RS.isSatellite ? config.satelliteAnimationParts : config.mainAnimationParts;
                // List of modifiers
                for (int rAP = 0; rAP < animParts.Count; rAP++)
                {
                    if (animParts[rAP].mute) continue;

                    float randomSeed01 = bO_A_RS.GetRandomizationOffset_01();
                    animationPart = animParts[rAP].GetRandomizedAnimationPart(randomSeed01);

                    // Are we within the part?
                    if (!t01.IsBetween(animationPart.start01, animationPart.finish01)) continue;

                    float currentValue = animationPart.EvaluateAt(t01);

                    float alphaMain = currentValue * config.alphaMain;
                    float brightnessMain = currentValue * config.brightnessMain;

                    float alphaStimulus = currentValue * config.alphaMain;
                    float brightnessStimulus = currentValue * config.brightnessStimulus;
                    switch (animParts[rAP].animationType)
                    {
                        case AnimationPart_Randomizable.AnimationType.Rotation:
                            float rotation = currentValue;
                            rotation *= (randomSeed01 > 0.5f) ? 1 : -1;
                            bO_A_RS.SetLocalRotation(rotation);
                            break;
                        case AnimationPart_Randomizable.AnimationType.Scale:

                            bO_A_RS.SetLocalScale(currentValue * bO_A_RS.GetScaleMultiplier());
                            bO_A_RS.MainParentScale(config.GetScaleMultiplier(bO_A_RS.mainRenderer));
                            bO_A_RS.SetOverlayParentScale(config.GetScaleMultiplier(bO_A_RS.overlayRenderer ? bO.overlayRenderer : bO_A_RS.mainRenderer)); // Fallback for overlays that are missing

                            break;
                        case AnimationPart_Randomizable.AnimationType.Luminance:
                            bO_A_RS.AdjustMain(config.alphaMain, brightnessMain);
                            break;
                        case AnimationPart_Randomizable.AnimationType.Fade:
                            bO_A_RS.AdjustMain(alphaMain, brightnessMain);
                            bO_A_RS.AdjustOverlay(alphaStimulus, brightnessStimulus);
                            break;
                        case AnimationPart_Randomizable.AnimationType.StimulusFade:
                            bO_A_RS.AdjustOverlay(alphaStimulus, config.useBrightnessStimulus ? brightnessStimulus : 1f, true);
                            break;
                    }
                }

                // Debug :: Show "Visible" squares
                if (debugDynamicGrid)
                {
                    bO_A_RS.AdjustMain(config.alphaMain, bO_A_RS.isVisible ? 1.0f : 0.0f);
                    if (bO_A_RS.GetQuadrantIndex() >= 0)
                        bO_A_RS.SetColorMain(DEBUG_COLORS.GetSafe(bO_A_RS.GetQuadrantIndex()));
                }
            }
            // Debug.LogError(Time.renderedFrameCount + "MASTER {0}"._Format(t01.PercentileToPercent()));
            // Debug.LogError("MASTER {0}"._Format(dT.ToString("#.00")));
        }

        protected override void HandlePeakAnimation_EaseIn(List<BackgroundObject> objectsToPeak, float t01)
        {
            BackgroundObject bO;
            BackgroundObject_Abstract_RotatingSquares bO_A_RS;
            for (int i = 0; i < objectsToPeak.Count; i++)
            {
                bO = objectsToPeak[i];
                if (!IsWithinBounds(bO))
                    continue;

                bO_A_RS = bO as BackgroundObject_Abstract_RotatingSquares;
                bO_A_RS.AdjustOverlay(config.alphaMain, config.brightnessMain);
            }
        }

        protected override void HandlePeakAnimation_Main(List<BackgroundObject> objectsToPeak, float t01)
        {
            BackgroundObject bO;
            BackgroundObject_Abstract_RotatingSquares bO_A_RS;
            for (int i = 0; i < objectsToPeak.Count; i++)
            {
                bO = objectsToPeak[i];
                if (!IsWithinBounds(bO))
                    continue;

                bO_A_RS = bO as BackgroundObject_Abstract_RotatingSquares;

                float randomSeed01 = bO_A_RS.GetRandomizationOffset_01();

                MinMax alphaMinMax = config.alphaOverlay_Peak_Low;
                MinMax brightnessMinMax = config.brightnessOverlay_Peak_Low;

                // Need a randomized upfront value (like random seed - but independent of it)
                float rand = bO_A_RS.GetRandomizationOffset_HL();
                if (rand > 0.5f)
                {
                    alphaMinMax = config.alphaOverlay_Peak_High;
                    brightnessMinMax = config.brightnessOverlay_Peak_High;
                }

                bO_A_RS.AdjustOverlay(
                    alphaMinMax.RetargetFrom01(randomSeed01),
                    brightnessMinMax.RetargetFrom01(randomSeed01));
            }
        }

        protected override void HandlePeakAnimation_EaseOut(List<BackgroundObject> objectsToPeak, float t01)
        {
            BackgroundObject bO;
            BackgroundObject_Abstract_RotatingSquares bO_A_RS;
            for (int i = 0; i < objectsToPeak.Count; i++)
            {
                bO = objectsToPeak[i];
                if (!IsWithinBounds(bO))
                    continue;

                bO_A_RS = bO as BackgroundObject_Abstract_RotatingSquares;
                bO_A_RS.AdjustOverlay(config.alphaMain, config.brightnessMain);
            }
        }

        protected override void CreateBackgroundObjects()
        {
            float x_to_y_ratio = (float)Screen.width / Screen.height;

            // numMainObjects_PerColumn * numMainObjects_PerRow = numBackgroundObjects + 1 (there's an empty spot in the middle)
            // numMainObjects_PerColumn = numMainObjects_PerRow / x_to_y_ratio
            // numMainObjects_PerRow * numMainObjects_PerRow / x_to_y_ratio = numBackgroundObjects + 1
            // So ::
            int numObjects_PerRow = Mathf.FloorToInt(Mathf.Sqrt((numBackgroundObjects + 1) * x_to_y_ratio));
            int numObjects_PerColumn = Mathf.FloorToInt(numObjects_PerRow / x_to_y_ratio);

            // Start from the inside and work outwards
            // Just need to have the eccentricity correct proportionately to the stimuli
            float eccentricityScaleWrtStimulus = config.eccentricityWrtStimulus;
            Vector2 offset = new Vector2(x_to_y_ratio, 1).normalized * eccentricityScaleWrtStimulus * lossyScale;

            // Place the four stimuli
            Vector2 center = Vector2.zero;
            // Move things up or down
            // SHOULD SCALE AUTOMATICALLY (because corners move automatically)
            center.x = Mathf.Lerp(bottomLeftCorner.position.x, topRightCorner.position.x, config.GetHorizontalCenter());
            center.y = Mathf.Lerp(bottomLeftCorner.position.y, topRightCorner.position.y, config.backgroundVerticalPositionPercentileUnscaled);

            BackgroundObject_Abstract_RotatingSquares object_Object = null;

            // Starting from the center, and spreading horizontally and vertically
            for (int i = -numObjects_PerRow / 2; i < numObjects_PerRow / 2; i++)
                for (int j = -numObjects_PerColumn / 2; j < numObjects_PerColumn / 2; j++)
                {
                    // Don't want anything dead-center (that's where the fixation is)
                    if (i == 0 && j == 0) continue;

                    Vector2 object_Offset = Vector2.right * offset.x * i + Vector2.up * offset.y * j;
                    Vector2 object_Position = center + object_Offset * lossyScale;

                    // Is this a main object or satelite?
                    // [TODO]
                    SquareType object_Type =
                        // (1, 1), (-1, 1), (-1, -1) and (1, -1) are the only crit squares
                        Mathf.Abs(i) == 1 && Mathf.Abs(j) == 1 ? SquareType.CriticalSquare :
                        Mathf.Abs(i - j) % 2 == 0 ? SquareType.MainSquare : SquareType.Satellite;

                    // Debug.Log("{0}, {1} -> {2}"._Format(i, j, object_Type));

                    object_Object = CreateObject(object_Position, object_Type);

                    if (object_Type == SquareType.CriticalSquare)
                        SetObjectToDirection(object_Object, object_Offset.ToClosestD2D_Diagonal());
                }

            DrawLines(center, numObjects_PerRow, numObjects_PerColumn, offset.x, offset.y);
        }

        public enum SquareType { CriticalSquare, MainSquare, Satellite }

        private BackgroundObject_Abstract_RotatingSquares CreateObject(Vector2 initPosition, SquareType type)
        {
            BackgroundObject_Abstract_RotatingSquares backgroundObject =
                CreateBackgroundObject(pivot) as BackgroundObject_Abstract_RotatingSquares;
            backgroundObject.DetachPivot();

            backgroundObject.SetInitPos(initPosition);
            backgroundObject.SetType(type);

            return backgroundObject;
        }

        protected override void InitializeBackgroundObjectPositions(List<BackgroundObject> backgroundObjects)
        {
            base.InitializeBackgroundObjectPositions(backgroundObjects);

            QuadrantParameters quadrantParameters = new QuadrantParameters(
                bottomLeftCorner.position, topRightCorner.position,
                config.NUM_QUADRANT_ROWS, config.NUM_QUADRANT_COLUMNS, config.acceptableQuadrants);

            foreach (BackgroundObject bO in backgroundObjects)
            {
                BackgroundObject_Abstract_RotatingSquares bO_A_RS = bO as BackgroundObject_Abstract_RotatingSquares;

                bO_A_RS.ResetToInitPos(true);

                bO.SetQuadrantParameters(quadrantParameters);
            }
        }

        private Color[] DEBUG_COLORS = new Color[]
        {
            new Color(0.0f, 0.0f, 1.0f),
            new Color(0.0f, 1.0f, 0.0f),
            new Color(0.0f, 1.0f, 1.0f),
            new Color(1.0f, 0.0f, 0.0f),
            new Color(1.0f, 0.0f, 1.0f),
            new Color(1.0f, 1.0f, 0.0f),

            new Color(0.2f, 0.2f, 0.6f),
            new Color(0.2f, 0.6f, 0.2f),
            new Color(0.2f, 0.6f, 0.6f),
            new Color(0.6f, 0.2f, 0.2f),
            new Color(0.6f, 0.2f, 0.6f),
            new Color(0.6f, 0.6f, 0.2f),
        };
    }
}