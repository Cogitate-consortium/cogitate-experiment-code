using Helpers.Engine;
using System;
using System.Collections;
using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;

namespace Helpers.Misc
{
    public class LockedRandomTimer
    {
        public enum RandomType { Uniform, TruncatedExp }

        public RandomType randomType = RandomType.Uniform;
        public bool isLocked;

        private Dictionary<float, float> truncExpCumProb = new Dictionary<float, float>();
        private float min = 1;
        private float max = 2;
        private bool isPaused = false;
        private Coroutine coroutine = null;
        private string name = "";

        private static readonly Color DEBUG_COLOR = Color.red;

        public LockedRandomTimer(string name, MinMax minMax) : this(name, minMax.min, minMax.max) { }

        public LockedRandomTimer(string name, float min, float max)
        {
            this.name = "{0}_{1}"._Format(typeof(LockedRandomTimer).ToString(), name);
            this.min = min;
            this.max = max;

            this.Log("Created! [{0}, {1}]"._Format(min.ToString("#.00"), max.ToString("#.00")));
        }

        public void SetAsTruncatedExp(Dictionary<float, float> truncExpCumProb)
        {
            randomType = RandomType.TruncatedExp;
            this.truncExpCumProb = truncExpCumProb;
        }

        /*
        public void SetAsTruncatedExp(float min, float θ, float b, float dX)
        {
            randomType = RandomType.TruncatedExp;
            truncExpCumProb = Math_Helper.TruncExp_GetCumProb(θ, b, dX, min);
        }
        */

        public float Restart()
        {
            Stop();

            // [SOS] Only the LOckedRandomTimer's Restart chains its UnPause
            // Fire an Unpause
            Pause(false);

            return Start();
        }

        public void Stop()
        {
            Clear();
        }

        public void Pause(bool isPaused)
        {
            this.isPaused = isPaused;
        }

        private void Clear()
        {
            if (coroutine == null) return;
            Utility_Helper_MB.instance.StopCoroutine(coroutine);
            coroutine = null;
            this.Log("Stop!");
        }

        private float Start()
        {
            if (coroutine != null) return -1;

            // Pick a random value to wait for
            float lockTime = GetRandomLockTime() * multiplier;

            coroutine = Utility_Helper_MB.instance.StartCoroutine(ToggleLockUnlockIE(lockTime));

            return lockTime;
        }

        private float GetRandomLockTime()
        {
            switch (randomType)
            {
                case RandomType.Uniform:
                default:
                    return Utility_Helper.RandomRange(min, max);
                case RandomType.TruncatedExp:
                    float r = Utility_Helper.RandomRange(0f, 1f);
                    float v = Math_Helper.QueryCDF(truncExpCumProb, r);
                    if (v >= 0) return v;
                    return Utility_Helper.RandomRange(min, max);
            }
        }

        private float multiplier = 1.0f;

        public void SetMultiplier(float multiplier)
        {
            this.multiplier = multiplier;
        }

        private IEnumerator ToggleLockUnlockIE(float duration)
        {
            this.Log("Start!");

            // Lock
            Lock(true);

            // Wait it out
            this.Log("Waiting for {0}"._Format(duration.ToString("#.00")));
            for (float t = duration; t >= 0; t -= TimeWrapper.deltaTime_SinceLastUpdate_NotTS)
            {
                // Respect pausing
                while (isPaused)
                    yield return null;
                yield return null;
            }

            // Unlock
            Lock(false);
        }

        public void Lock(bool v)
        {
            isLocked = v;

            this.Log(v ? "Locked" : "Unlocked");
        }

        public override string ToString()
        {
            return name.Colorize(DEBUG_COLOR);
        }
    }
}