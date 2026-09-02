// NS_REMOVE
using ExperimentLibrary;

using System;
using System.Collections;
using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;

namespace Experiment.Background
{
    public class BackgroundManager_Abstract_BlinkingSquares : BackgroundManager_Abstract
    {
#if false
        [SerializeField] private Transform bottomLeftCorner = null;
        [SerializeField] private Transform topRightCorner = null;

        // private SpriteLocation nextSl;
        //private bool soonToShowStimulus = false;

        private Vector2 distanceBetweenMainObjects;

        /*
        // Debug
        private void Start()
        {
            if (!UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ContainsInvariant("test"))
                return;

            this.LogWarning("Starting background in debug mode");
            // Calculate the min/max
            // MinMax backgroundMinMax = ApplicationLibrary.Config.GetPeriodMinMax_Background();
            base.Initialize(backgroundMinMax);
            InitializeObjects();
            // base.Restart();
            //base.Pause(false);
            this.LogWarning("Disable debug start up");
        }
        */

        protected override void Initialize()
        {
            base.Initialize();
            InitializeObjects();

            bottomLeftCorner.gameObject.SetActive(false);
            topRightCorner.gameObject.SetActive(false);
        }

        protected override BackgroundType GetBackgroundType()
        {
            return BackgroundType.Abstract_BlinkingSquares;
        }

        protected override void BeginNewAnimationCycle()
        {
            base.BeginNewAnimationCycle();

            if (config.resetRandomnessAtBeginOfAnimCycle)
                foreach (BackgroundObject bO in GetBackgroundObjects())
                    bO.ResetRandomizationOffset();
        }

        private void InitializeObjects()
        {
            // Turn everything on!
            foreach (BackgroundObject backgroundObject in backgroundObjects)
            {
                BackgroundObject_Abstract_BlinkingSquares bO = (backgroundObject as BackgroundObject_Abstract_BlinkingSquares);
                bO.onCycleBegin += BackgroundManager_Abstract_BlinkingSquares_onCycleBegin;
                bO.onPeak += BackgroundManager_Abstract_BlinkingSquares_onPeak;
                bO.onPeakEnd += BackgroundManager_Abstract_BlinkingSquares_onPeakEnd;
                bO.Toggle(true);
                bO.UnPaint();
                StartCoroutine(bO.AnimateIE());
            }
        }

        private void BackgroundManager_Abstract_BlinkingSquares_onCycleBegin(object sender, EventArgs e)
        {
            //BackgroundObject_Abstract_BlinkingSquares bO = sender as BackgroundObject_Abstract_BlinkingSquares;
            //bO.Toggle(true);

            //if (bO.IsStimulusCandidate())
            //{
            //    if (StimulusManager.ReadNextSpriteLocation().direction == bO.GetDirection())
            //    {
            //        //soonToShowStimulus = true;
            //    }
            //}
        }

        private void BackgroundManager_Abstract_BlinkingSquares_onPeak(object sender, EventArgs e)
        {
            BackgroundObject_Abstract_BlinkingSquares bO = sender as BackgroundObject_Abstract_BlinkingSquares;

            float dice = UnityEngine.Random.Range(0f, 1f);
            float pickingChance = (!bO.IsStimulusCandidate()) ? config.distractorImageChance_General : config.distractorImageChance_Main;

            // Paint objects with distractive images
            if (ApplicationLibrary.Config.Experiment.stimulus.background.useBlobs && dice <= pickingChance)
            {
                if (!bO.IsStimulusCandidate())
                {
                    SetRandomDistractor(bO);
                }
                else
                {
                    // Paint all non main squares with blob (with chance)
                    // DONT Show blobs when about to show stimulus
                    //if (!soonToShowStimulus)
                    //{
                    //    SetRandomDistractor(bO);
                    //}

                    // SHOW blobs when showing a stimulus
                    //if (StimulusManager.ReadNextSpriteLocation().direction != bO.GetDirection())
                    //{
                    //    SetRandomDistractor(bO);
                    //}
                }
            }

            if (bO.IsStimulusCandidate())
                RaiseBackgroundSinglePeakBeginEvent(bO);
        }

        public event EventHandler<AnimEventArgs> onSinglePeakBegin;

        private void RaiseBackgroundSinglePeakBeginEvent(BackgroundObject peakObject)
        {
            AnimEventArgs peakEventArgs = new AnimEventArgs();

            // [SOS] Create a copy of objects! We need the original to later paint distractor images
            peakEventArgs.visibleObjects = new List<BackgroundObject>() { peakObject };

            if (onSinglePeakBegin != null)
                onSinglePeakBegin(this, peakEventArgs);
        }

        private void SetRandomDistractor(BackgroundObject bO)
        {
            float dice = UnityEngine.Random.Range(0f, 1f);
            if (dice >= 0.5f)
                bO.SetOverlay(ApplicationLibrary.blobImagesFaces.GetRandom(), ApplicationLibrary.Config.Experiment.stimulus);
            else
                bO.SetOverlay(ApplicationLibrary.blobImagesObjects.GetRandom(), ApplicationLibrary.Config.Experiment.stimulus);
        }

        private void BackgroundManager_Abstract_BlinkingSquares_onPeakEnd(object sender, EventArgs e)
        {
            BackgroundObject_Abstract_BlinkingSquares bO = sender as BackgroundObject_Abstract_BlinkingSquares;
            // Reset any images
            if (ApplicationLibrary.Config.Experiment.stimulus.background.useBlobs)
            {
                bO.UnPaint();
            }

            if (bO.IsStimulusCandidate())
            {
                //if (StimulusManager.ReadNextSpriteLocation().direction == bO.GetDirection())
                //{
                //    //soonToShowStimulus = false;
                //}
            }
        }

        protected override IEnumerator AnimateIE()
        {
            yield return null;
            InitializeBackgroundObjectPositions(backgroundObjects);
        }

        protected override void HandlePeakAnimation_EaseIn(List<BackgroundObject> objectsToPeak, float t01)
        {
            BackgroundObject bO;
            BackgroundObject_Abstract_BlinkingSquares bO_A_RS;
            for (int i = 0; i < objectsToPeak.Count; i++)
            {
                bO = objectsToPeak[i];
                bO_A_RS = bO as BackgroundObject_Abstract_BlinkingSquares;
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
            int numMainObjects = numBackgroundObjects / 1;
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
            int numQuadrantX = Mathf.CeilToInt(numX / 1f);
            int numQuadrantY = Mathf.CeilToInt(numY / 1f);

            Vector2 distanceBetweenMainObjects = (offsetTopRight - offsetBottomLeft) / 2f;
            distanceBetweenMainObjects.x = (offsetTopRight - offsetBottomLeft).x / 3f;
            //distanceBetweenMainObjects = config.backgroundObjectsDistance;

            //distanceBetweenMainObjects.x = offsetTopRight.x / 2;

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
            BackgroundObject_Abstract_BlinkingSquares mainObject = null;
            for (int i = -1; i < numX; i++)
            {
                for (int j = -1; j < numY; j++)
                {
                    //bool isFirst = i == 0 && j == 0;
                    bool isFirst = i == 0 && j == 0;
                    //bool isFirst = i == config.mainObjectIndex && j == config.mainObjectIndex;
                    if (!doFirst && j == -1) continue;

                    Vector2 main_Offset = new Vector2(i * stepX, j * stepY);
                    Vector2 main_InitPos = initPos + main_Offset;
                    mainObject = CreateObject(main_InitPos, isFirst);
                    if (isFirst)
                    {
                        SetObjectToDirection(mainObject, direction);
                        mainObject.mainRenderer.color = Color.red;
                    }
                }
            }
        }

        private BackgroundObject_Abstract_BlinkingSquares CreateObject(Vector2 main_InitPos, bool isStimulusCandidate)
        {
            BackgroundObject_Abstract_BlinkingSquares mainObject = CreateBackgroundObject(pivot) as BackgroundObject_Abstract_BlinkingSquares;
            mainObject.DetachPivot();
            mainObject.SetInitPos(main_InitPos);
            mainObject.overrideIsStimulusCandidate = isStimulusCandidate;
            mainObject.name = "BackgroundObject_" + mainObject.GetInstanceID();

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
                BackgroundObject_Abstract_BlinkingSquares bO_A_RS = bO as BackgroundObject_Abstract_BlinkingSquares;

                bO_A_RS.ResetToInitPos();

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