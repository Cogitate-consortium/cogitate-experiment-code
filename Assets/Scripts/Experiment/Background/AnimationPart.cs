using System;
using TGP.Helpers;

namespace Experiment.Background
{
    /// <summary>
    /// [SIMPLIFY] Do we still need randomizations?
    /// [DEPRECATE?] We just have 4 parts, maybe just hardcode them in <see cref="BackgroundManager"/>
    /// </summary>
    [Serializable]
    public struct AnimationPart
    {
        public float start01;
        public float finish01;
        public float valueAt0;
        public float valueAt1;

        public AnimationPart(float start01, float finish01, float valueAt0, float valueAt1)
        {
            this.start01 = start01;
            this.finish01 = finish01;
            this.valueAt0 = valueAt0;
            this.valueAt1 = valueAt1;
        }

        public float EvaluateAt(float t01)
        {
            // Edge case of a single point
            if (start01 == finish01) return (valueAt0 + valueAt1) / 2f;

            return t01.Clamped01().Retargeted(start01, finish01, valueAt0, valueAt1, false);
        }
    }

    [Serializable]
    public struct AnimationPart_Randomizable
    {
        /// <summary>
        /// [SOS] Used only for the json
        /// </summary>
        public string _description;
        public MinMax start01;
        public MinMax finish01;
        public MinMax valueAt0;
        public MinMax valueAt1;
        public AnimationType animationType;
        public bool mute;

        public AnimationPart_Randomizable(AnimationType animationType, MinMax start01, MinMax finish01, MinMax valueAt0, MinMax valueAt1)
        {
            _description = "";
            this.animationType = animationType;

            this.start01 = start01;
            this.finish01 = finish01;
            this.valueAt0 = valueAt0;
            this.valueAt1 = valueAt1;

            mute = false;
        }
        /*
        public float EvaluateAt(float t01, float r01)
        {
            // Get randomized Args
            AnimationPart randomizedArgs = GetRandomizedAnimationPart(r01);

            // Evaluate
            return randomizedArgs.EvaluateAt(t01);
        }
        */

        public AnimationPart GetRandomizedAnimationPart(float r01)
        {
            AnimationPart randomizedAnimationPart = new AnimationPart();

            randomizedAnimationPart.start01 = start01.RetargetFrom01(r01);
            randomizedAnimationPart.finish01 = finish01.RetargetFrom01(r01);
            randomizedAnimationPart.valueAt0 = valueAt0.RetargetFrom01(r01);
            randomizedAnimationPart.valueAt1 = valueAt1.RetargetFrom01(r01);

            return randomizedAnimationPart;
        }

        public enum AnimationType { Rotation, Scale, Luminance, Fade, StimulusFade }
    }
}