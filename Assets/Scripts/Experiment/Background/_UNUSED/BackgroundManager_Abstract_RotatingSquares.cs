// NS_REMOVE
using ExperimentLibrary;
// NS_REMOVE | VICE VERSA and better at the parent
using Experiment.Stimulus;

using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;

namespace Experiment.Background
{
    public class BackgroundManager_Abstract_RotatingSquares : BackgroundManager_Abstract
    {
#if false
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

        protected override void Initialize()
        {
            base.Initialize();

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


        private List<BackgroundObject> _backgroundObjects;
        protected override void HandleMasterAnimation(float t01)
        {
            _backgroundObjects = GetBackgroundObjects();
            BackgroundObject bO;
            BackgroundObject_Abstract_RotatingSquares bO_A_RS;
            AnimationPart animationPart;

            BackgroundConfig config = this.config;

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

                    switch (animParts[rAP].animationType)
                    {
                        case AnimationPart_Randomizable.AnimationType.Rotation:
                            float rotation = currentValue;
                            rotation *= (randomSeed01 > 0.5f) ? 1 : -1;
                            bO_A_RS.SetLocalRotation(rotation);
                            break;
                        case AnimationPart_Randomizable.AnimationType.Scale:
                            bO_A_RS.SetLocalScale(currentValue * config.GetScaleMultiplier(bO_A_RS.mainRenderer) * bO_A_RS.GetScaleMultiplier());
                            break;
                        case AnimationPart_Randomizable.AnimationType.Luminance:
                            bO_A_RS.AdjustMain(config.alphaMain, currentValue * config.brightnessMain);
                            break;
                        case AnimationPart_Randomizable.AnimationType.Fade:
                            bO_A_RS.AdjustMain(currentValue * config.alphaMain, currentValue * config.brightnessMain);
                            bO_A_RS.AdjustOverlay(currentValue * config.alphaMain, currentValue * config.brightnessMain);
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
            // Debug.LogError("MASTER {0}"._Format(t01.PercentileToPercent()));
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

            // How many objects do we have in total
            // We have a grid of main objects
            // And 50% of them have 4 children
            // numSatelliteObjects = 2 * numMainObjects
            // numMainObjects + numSatelliteObjects = numBackgroundObjects

            // So ::
            int numMainObjects = numBackgroundObjects / 3;
            // int numSatelliteObjects = 2 * numMainObjects;

            // How is this broken down to axes
            // numX * numY = numMainObjects
            // numX = numY * x_to_y_ratio
            // numY * numY * x_to_y_ratio = numBackgroundObjects

            // So ::
            int numY = Mathf.FloorToInt(Mathf.Sqrt(numMainObjects / x_to_y_ratio));
            int numX = Mathf.FloorToInt(numY * x_to_y_ratio);

            // Calculate the distances
            // Vector2 worldDimensionToCover = topRightCorner.position - bottomLeftCorner.position;

            // Start from the inside and work outwards
            // No need to factor in scale, as we will scale up the background manager itself
            // Just need to have the eccentricity correct proportionately to the stimuli
            float eccentricityScaleWrtStimulus = config.eccentricityWrtStimulus;

            Vector2 offsetTopRight = new Vector2(x_to_y_ratio, 1).normalized * eccentricityScaleWrtStimulus;
            Vector2 offsetBottomRight = new Vector2(x_to_y_ratio, -1).normalized * eccentricityScaleWrtStimulus;
            Vector2 offsetBottomLeft = new Vector2(-x_to_y_ratio, -1).normalized * eccentricityScaleWrtStimulus;
            Vector2 offsetTopLeft = new Vector2(-x_to_y_ratio, 1).normalized * eccentricityScaleWrtStimulus;

            // Place the four stimuli
            Vector2 center = Vector2.zero;

            // Move things up or down
            center.x = Mathf.Lerp(bottomLeftCorner.position.x, topRightCorner.position.x, config.GetHorizontalCenter());
            center.y = Mathf.Lerp(bottomLeftCorner.position.y, topRightCorner.position.y, config.backgroundVerticalPositionPercentileUnscaled);

            // Propagate outwards
            int numQuadrantX = Mathf.CeilToInt(numX / 2f);
            int numQuadrantY = Mathf.CeilToInt(numY / 2f);

            Vector2 distanceBetweenMainObjects = (offsetTopRight - offsetBottomLeft);// * config.mainObjectDistanceMulti;

            float stepX = distanceBetweenMainObjects.x;
            float stepY = distanceBetweenMainObjects.y;

            PopulateQuadrant(Direction_2D_Diagonal.TopRight, center + offsetTopRight, numQuadrantX, numQuadrantY, stepX, stepY, true);
            PopulateQuadrant(Direction_2D_Diagonal.BottomRight, center + offsetBottomRight, numQuadrantX, numQuadrantY, stepX, -stepY, false);
            PopulateQuadrant(Direction_2D_Diagonal.BottomLeft, center + offsetBottomLeft, numQuadrantX, numQuadrantY, -stepX, -stepY, true);
            PopulateQuadrant(Direction_2D_Diagonal.TopLeft, center + offsetTopLeft, numQuadrantX, numQuadrantY, -stepX, stepY, false);
            DrawLines(center, numQuadrantX, numQuadrantX, stepX, stepY);
        }

        private void PopulateQuadrant(Direction_2D_Diagonal direction, Vector2 initPos, int numX, int numY, float stepX, float stepY, bool doFirst)
        {
            BackgroundObject_Abstract_RotatingSquares mainObject = null;
            BackgroundObject_Abstract_RotatingSquares childObject;
            Vector2 child_Dir;
            Vector2 child_Offset;
            for (int i = 0; i < numX; i++)
                for (int j = 0; j < numY; j++)
                {
                    bool isFirst = i == 0 && j == 0;

                    Vector2 main_Offset = new Vector2(i * stepX, j * stepY);
                    Vector2 main_InitPos = initPos + main_Offset;
                    mainObject = CreateObject(main_InitPos, isFirst);

                    if (isFirst)
                    {
                        SetObjectToDirection(mainObject, direction);
                        //mainObject.sR.color = Color.red;
                    }

                    bool isDiagonal = i % 2 == j % 2;
                    // Add children?
                    if (doFirst && !isDiagonal) continue;
                    if (!doFirst && isDiagonal) continue;


                    foreach (Direction_2D dir2D in Utility_Helper.EnumGetValues<Direction_2D>())
                    {
                        childObject = CreateBackgroundObject(null) as BackgroundObject_Abstract_RotatingSquares;

                        child_Dir = dir2D.ToVector2();

                        // Factor in distance
                        child_Offset = child_Dir.MultiplyBy(new Vector2(stepX, stepY) / 2);
                        mainObject.AssignSatellite(childObject, child_Offset);

                        // Satellites won't be affected by scale via hierarchy
                        childObject.SetScaleMultiplier(config.satelliteScaleMulti * lossyScale);

                        // childObject.SetScaleLimits(new Vector2(satelliteScaleMin, satelliteScaleMax));
                    }
                }
        }

        private BackgroundObject_Abstract_RotatingSquares CreateObject(Vector2 main_InitPos, bool isStimulusCandidate)
        {
            BackgroundObject_Abstract_RotatingSquares mainObject =
                CreateBackgroundObject(pivot) as BackgroundObject_Abstract_RotatingSquares;
            mainObject.DetachPivot();

            mainObject.SetInitPos(main_InitPos);

            mainObject.SetType(isStimulusCandidate ?
                BackgroundManager_Abstract_StaticRotatingSquares.SquareType.CriticalSquare :
                BackgroundManager_Abstract_StaticRotatingSquares.SquareType.MainSquare);

            return mainObject;
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

                bO_A_RS.ResetToInitPos(false);

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
#endif
    }
}