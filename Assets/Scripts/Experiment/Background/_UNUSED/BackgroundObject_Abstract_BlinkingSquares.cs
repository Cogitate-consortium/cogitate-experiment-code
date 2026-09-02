// NS_REMOVE
using ExperimentLibrary;
using Experiment.Managers;
using Helpers.Engine;
using System;
using System.Collections;
using TGP.Helpers;
using UnityEngine;

namespace Experiment.Background
{
    public class BackgroundObject_Abstract_BlinkingSquares : BackgroundObject
    {
#if FALSE
        public event EventHandler<EventArgs> onCycleBegin;
        public event EventHandler<EventArgs> onPeak;
        public event EventHandler<EventArgs> onPeakEnd;

        [SerializeField] private Transform pivot = null;

        private Vector3 initPos;
        private float scaleMulti = 1;
        // private Vector2 scaleLimits;

        //protected BackgroundConfig config { get { return ApplicationLibrary.Config.Experiment.stimulus.background; } }
        protected BackgroundConfig config;

        protected override void OnAwake()
        {
            base.OnAwake();
            senderName = "Blinking_Square" + this.gameObject.GetInstanceID();
            StartCoroutine(LogObject());
            config = ExperimentLibraryManager.Config.Experiment.stimulus.background;
        }

        public void SetInitPos(Vector3 initPos)
        {
            this.initPos = initPos;
        }

        public void DetachPivot()
        {
            pivot.parent = null;
        }

        public void ResetToInitPos()
        {
            transform.position = initPos;
        }

        public bool overrideIsStimulusCandidate = false;

        public override bool IsStimulusCandidate()
        {
            return overrideIsStimulusCandidate;
            // return !isSatellite;
        }

        protected override void UpdateMotion(Vector2 motion01)
        {
            base.UpdateMotion(motion01);

            /*
            // Change size
            float newScaleAnimationCoef = motion01.x.Fold(0.50f);
            float newScale01 = newScaleAnimationCoef.RetargetedTo_01(0, 0.25f);

            // NonSats are the opposite
            if (!isSatellite)
                newScale01 = 1 - newScale01;

            float newScale = newScale01.RetargetedFrom_01(scaleLimits);

            GetMainObject().localScale = Vector3.one * newScale;

            Vector3 rotationMultiplier = 360 * Vector3.forward;

            // Satellites rotate inversely
            if (isSatellite)
                rotationMultiplier *= -1;

            // The main motion is PIVOT rotation
            pivot.localRotation = Quaternion.Euler(rotationMultiplier * motion01.x);

            // The off motion is TEXTURE rotation -- 
            if (newScale01 < 0.5f)
                GetMainObject().localRotation = Quaternion.Euler(rotationMultiplier * motion01.y);
            */
        }

        protected override void OnUpdate(float dT)
        {
            base.OnUpdate(dT);

            pivot.transform.position = transform.position;
            pivot.transform.rotation = transform.rotation;
        }

        float timerElapsedTime = 0;
        private IEnumerator SetTimer(float duration, Action<float> t01)
        {
            timerElapsedTime = 0;
            while (timerElapsedTime <= duration)
            {
                while (!canMove) yield return null;

                timerElapsedTime += TimeWrapper.deltaTime_SinceLastUpdate_NotTS;
                t01(timerElapsedTime / duration);
                yield return null;
            }
            t01(1);
        }

        Coroutine timerCR;
        public IEnumerator AnimateIE()
        {
            if (itsamemario == 0)
                itsamemario = gameObject.GetInstanceID();

            //float peakDuration = config.peakEaseIn + config.peakEaseOut + config.peakDuration;
            float peakDuration = config.peakDuration;
            MinMax period = config.periodMinMax - peakDuration;

            UpdateAnimation(0);
            // Give a delay at start, so they dont all together flash
            float startDelay = UnityEngine.Random.Range(0, period.max * 2);
            yield return new WaitForSeconds(startDelay);

            float lastPeak = TimeWrapper.time_NotTS;

            // Period min max 
            // - peak duration
            // remainder *

            // How long is my cycle? (excluding peak sustain of 500ms)
            float cycleDuration;
            // How long is my downtime?
            float stayAtLowestMultiplier;
            float stayAtLowestDuration;
            // The rest is my animation
            float animationDuration;

            while (true)
            {
                while (!base.isVisible)
                {
                    yield return new WaitForSeconds(UnityEngine.Random.value * 0.5f + 0.5f);
                }

                // How long is my cycle? (excluding peak sustain of 500ms)
                cycleDuration = UnityEngine.Random.Range(period.min, period.max);
                // How long is my downtime?
                stayAtLowestMultiplier = UnityEngine.Random.Range(config.stayAtLowest.min, config.stayAtLowest.max);
                stayAtLowestDuration = cycleDuration * stayAtLowestMultiplier.Clamped01();
                // The rest is my animation
                animationDuration = cycleDuration - stayAtLowestDuration;

                //AdjustMain(config.alphaMain, config.brightnessMain);
                //AdjustOverlay(config.alphaMain, config.brightnessMain);

                if (config.resetRandomnessAtBeginOfAnimCycle)
                    ResetRandomizationOffset();

                RaiseOnCycleBegin();

                // Animation In
                if (config.useAnimationOut)
                    animationDuration /= 2;

                float lerp = 0;
                timerElapsedTime = 0;
                while (timerElapsedTime <= animationDuration)
                {
                    while (!canMove) yield return null;

                    timerElapsedTime += TimeWrapper.deltaTime_SinceLastUpdate_NotTS;
                    lerp = timerElapsedTime / animationDuration;
                    UpdateAnimation(lerp);

                    if (lerp >= 1f)
                        break;
                    yield return null;
                }

                // Peak
                RaiseOnPeak();
                lerp = 0;
                timerElapsedTime = 0;
                while (timerElapsedTime <= peakDuration)
                {
                    while (!canMove) yield return null;

                    timerElapsedTime += TimeWrapper.deltaTime_SinceLastUpdate_NotTS;
                    lerp = timerElapsedTime / peakDuration;
                    //SetOverlay();

                    if (lerp >= 1f)
                        break;
                    yield return null;
                }

                RaiseOnPeakEnd();

                // Leave without animation out by default - Toggle it from config
                if (!config.useAnimationOut)
                {
                    // Using UpdateAnimation(0) caused sometimes a bug, where object stayed at small side, instead of disappearing
                    float currentValue = 0;
                    SetLocalScale(currentValue * config.GetScaleMultiplier(mainRenderer) * GetScaleMultiplier());
                    AdjustMain(currentValue * config.alphaMain, currentValue * config.brightnessMain);
                    AdjustOverlay(currentValue * config.alphaMain, currentValue * config.brightnessMain);
                }
                else
                {
                    // Animation Out
                    animationDuration = UnityEngine.Random.Range(period.min, period.max) / 2;
                    lerp = 0;
                    timerElapsedTime = 0;
                    while (timerElapsedTime <= animationDuration)
                    {
                        while (!canMove) yield return null;

                        timerElapsedTime += TimeWrapper.deltaTime_SinceLastUpdate_NotTS;
                        lerp = timerElapsedTime / animationDuration;
                        UpdateAnimation(1f - lerp);

                        if (lerp >= 1f)
                            break;
                        yield return null;
                    }
                }

                // Reset overlay
                //AdjustOverlay(config.alphaMain, config.brightnessMain);

                yield return StartCoroutine(SetTimer(stayAtLowestDuration, (t01) =>
                {

                }));

                yield return null;

                // if (itsamemario == gameObject.GetInstanceID()) Debug.LogWarning(Time.time - lastPeak);

                lastPeak = TimeWrapper.time_NotTS;
            }
        }

        /*
        public bool IsOutOfVisibility()
        {
            if (Camera.main == null) return true;

            Vector2 objectScreenPosition = Camera.main.WorldToScreenPoint(this.transform.position);
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);
#if UNITY_EDITOR
            screenSize = Utility_Helper.GetEditorWindowSize();
#endif
            bool isObjectOutOfScreen = ((objectScreenPosition.x > screenSize.x * 1.2f || objectScreenPosition.x < -screenSize.x * 0.2f) ||
                                    (objectScreenPosition.y > screenSize.y * 1.2f || objectScreenPosition.y < -screenSize.y * 0.2f));

            return isObjectOutOfScreen;
        }*/

        private static int itsamemario = 0;

        private void UpdateAnimation(float t01)
        {
            // List of modifiers
            foreach (AnimationPart_Randomizable rAP in config.blinkingAnimationParts)
            {
                if (rAP.mute) continue;

                float randomSeed01 = GetRandomizationOffset_01();
                if (IsStimulusCandidate())
                    randomSeed01 = 1f;
                AnimationPart animationPart = rAP.GetRandomizedAnimationPart(randomSeed01);

                // Are we within the part?
                if (!t01.IsBetween(animationPart.start01, animationPart.finish01)) continue;

                float currentValue = animationPart.EvaluateAt(t01);
                //currentValue *= randomSeed01;

                switch (rAP.animationType)
                {
                    case AnimationPart_Randomizable.AnimationType.Rotation:
                        SetLocalRotation(currentValue);
                        break;
                    case AnimationPart_Randomizable.AnimationType.Scale:
                        SetLocalScale(currentValue * config.GetScaleMultiplier(mainRenderer) * GetScaleMultiplier());
                        break;
                    case AnimationPart_Randomizable.AnimationType.Luminance:
                        AdjustMain(config.alphaMain, currentValue * config.brightnessMain);
                        break;
                    case AnimationPart_Randomizable.AnimationType.Fade:
                        AdjustMain(currentValue * config.alphaMain, currentValue * config.brightnessMain);
                        //AdjustOverlay(currentValue * config.alphaMain, currentValue * config.brightnessMain);
                        break;
                }
            }
        }

        private void SetOverlay()
        {
            float randomSeed01 = GetRandomizationOffset_01();

            MinMax alphaMinMax = config.alphaOverlay_Peak_Low;
            MinMax brightnessMinMax = config.brightnessOverlay_Peak_Low;

            // Need a randomized upfront value (like random seed - but independent of it)
            float rand = GetRandomizationOffset_HL();
            if (rand > 0.5f)
            {
                alphaMinMax = config.alphaOverlay_Peak_High;
                brightnessMinMax = config.brightnessOverlay_Peak_High;
            }

            AdjustOverlay(
                alphaMinMax.RetargetFrom01(randomSeed01),
                brightnessMinMax.RetargetFrom01(randomSeed01));
        }

        public float GetScaleMultiplier()
        {
            return scaleMulti;
        }

        public void SetScaleMultiplier(float scaleMulti)
        {
            this.scaleMulti = scaleMulti;
            // this.scaleLimits = scaleLimits;
        }

        private void RaiseOnCycleBegin()
        {
            if (onCycleBegin != null)
                onCycleBegin(this, null);
        }

        private void RaiseOnPeakEnd()
        {
            if (onPeakEnd != null)
                onPeakEnd(this, null);
        }

        private void RaiseOnPeak()
        {
            if (onPeak != null)
                onPeak(this, new EventArgs());
        }

        // Logs
        private string senderName;
        private IEnumerator LogObject()
        {
            while (true)
            {
                if (canMove && base.isVisible)
                {
                    ExperimentManagerSession.LogBackgroundLevelData(senderName,
                                                        Utility_Helper.GetRelativeScreenPosition(this.transform.position),
                                                        (int)this.transform.rotation.eulerAngles.z,
                                                        this.transform.localScale.x,
                                                        base.overlayBrightness,
                                                        base.overlayAlpha
                                                        );
                }
                yield return null;
            }
        }
#endif
    }
}