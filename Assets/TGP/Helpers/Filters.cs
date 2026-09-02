using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TGP
{
    namespace Helpers
    {
        namespace Filters
        {
            public class PeakDetection
            {
                private List<float> pastValues = new List<float>();
                public int windowSize;
                public float threshold;
                public bool additiveThreshold = false;

                /// <summary>
                /// The larger the window, the harder it is for a peak to register (favors global maxima). This filter has a delay of windowSize / 2.
                /// </summary>
                /// <param name="windowSize"></param>
                public PeakDetection(int windowSize, float threshold, bool additiveThreshold)
                {
                    this.windowSize = windowSize;
                    this.threshold = threshold;
                    this.additiveThreshold = additiveThreshold;
                }

                public bool AddAndQuery(float newValue, bool debug = false)
                {
                    pastValues.Add(newValue);

                    // We are only interested if we found a peak.
                    // We will detect the peak when it is at -WINDOW_SIZE / 2
                    // Tested with intensity = Mathf.Cos(2 * Mathf.PI * Time.time); (peak every second)
                    int midIdx = pastValues.Count - windowSize;
                    bool isPeak = true;
                    float diff = 0;
                    if (pastValues.Count >= 2 * windowSize)
                    {
                        for (int i = pastValues.Count - 1; i >= pastValues.Count - 2 * windowSize; i--)
                        {
                            if (i == midIdx) continue;
                            diff += Mathf.Abs(pastValues[midIdx] - pastValues[i]);
                            isPeak &= (pastValues[midIdx] > pastValues[i]);
                        }

                        // The peak has to be high above the average value
                        if (additiveThreshold)
                            isPeak &= pastValues[midIdx] > pastValues.Average() + threshold;
                        else
                        {
                            float avg = pastValues.Average();
                            if (avg > 0)
                                isPeak &= pastValues[midIdx] > pastValues.Average() * threshold;
                            else
                                isPeak &= pastValues[midIdx] > pastValues.Average() / threshold;
                        }
                    }

                    if (isPeak && debug)
                        this.Log("Peak!");

                    return isPeak;
                }
            }

            public class RunningAverage_Vector3
            {
                RunningAverage x, y, z;

                public Vector3 runningAverage
                {
                    get
                    {
                        return new Vector3(
                      x != null ? x.runningAverage : 0,
                      y != null ? y.runningAverage : 0,
                      z != null ? z.runningAverage : 0);
                    }
                }

                public RunningAverage_Vector3(float newObservationWeight, float amplifier = 1)
                {
                    x = new RunningAverage(newObservationWeight, amplifier);
                    y = new RunningAverage(newObservationWeight, amplifier);
                    z = new RunningAverage(newObservationWeight, amplifier);
                }

                public void Reset()
                {
                    x.Reset();
                    y.Reset();
                    z.Reset();
                }

                /// <param name="observationMultiplier">Pass DT here if you want</param>
                public Vector3 AddAndQuery(Vector3 newObservation, float observationMultiplier = 1)
                {
                    x.AddAndQuery(newObservation.x, observationMultiplier);
                    y.AddAndQuery(newObservation.y, observationMultiplier);
                    z.AddAndQuery(newObservation.z, observationMultiplier);

                    return runningAverage;
                }
            }

            public class RunningAverage
            {
                public RunningAverage(float newObservationWeight, float amplifier = 1)
                {
                    this.newObservationWeight = newObservationWeight; this.amplifier = amplifier;
                }
                public float value { get { return runningAverage; } }
                public float runningAverage = float.NaN;
                public float newObservationWeight = 0.5f;
                private float amplifier = 1;

                public void Reset(float initValue = float.NaN)
                {
                    runningAverage = initValue;
                }
                
                /// <param name="observationMultiplier">Pass DT here if you want</param>
                public float AddAndQuery(float newObservation, float observationMultiplier = 1)
                {
                    float val = newObservation;
                    // Recalculate the average
                    if (float.IsNaN(runningAverage))
                        runningAverage = val;
                    else
                        runningAverage = Mathf.Lerp(runningAverage,
                            val, newObservationWeight * observationMultiplier);

                    // Amplify it (to see what's going on)
                    val -= runningAverage;
                    val *= amplifier;
                    val += runningAverage;

                    return val;
                }
            }

            public class IIR
            {
                private bool initialized = false;

                List<float> pastInputValues = new List<float>(), pastOutputValues = new List<float>();
                List<float> a = new List<float>(), b = new List<float>();
                float a0;

                public void MakeBandpass_Min_Max(float f_min, float f_max, float f_sample)
                {
                    if (f_max < f_min)
                    {
                        float temp = f_min;
                        f_min = f_max;
                        f_max = temp;
                    }
                    MakeBandpass_Center_Bandwidth((f_min + f_max) / 2f, f_max - f_min, f_sample);
                }
                // http://dspguide.com/ch19/3.htm
                // The narrowest bandwidth that can be obtain with
                // single precision is about 0.0003 of the sampling frequency
                public void MakeBandpass_Center_Bandwidth(float f_center, float bandwidth, float f_sample)
                {
                    if (f_sample <= 0 || f_center <= 0 || bandwidth <= 0)
                    {
                        this.Log("All inputs must be positive!");
                        return;
                    }
                    if (f_center > f_sample / 2f || bandwidth > f_sample / 2f)
                    {
                        this.Log("Insufficient sampling frequency!");
                        return;
                    }

                    // Express everything as a fraction of f_sample
                    f_center /= f_sample;
                    bandwidth /= f_sample;

                    float cos_two_PI_f = Mathf.Cos(2 * Mathf.PI * f_center);

                    float R = 1 - 3 * bandwidth;
                    float R_squared = Mathf.Pow(R, 2);

                    float K = (1 - 2 * R * cos_two_PI_f + R_squared)
                        / (2 - 2 * cos_two_PI_f);

                    a.Clear();
                    b.Clear();

                    a0 = 1 - K;
                    a.Add(2 * (K - R) * cos_two_PI_f);
                    a.Add(R_squared);

                    b.Add(2 * R * cos_two_PI_f);
                    b.Add(-R_squared);

                    a.SetCapacity(a.Count);
                    b.SetCapacity(b.Count);

                    pastInputValues.SetCapacity(a.Count);
                    pastOutputValues.SetCapacity(a.Count);

                    initialized = true;
                }

                public IIR() { }

                /// <summary>
                /// Initialize a new filter
                /// </summary>
                /// <param name="a">Input Coefficients</param>
                /// <param name="b">Output Coefficients</param>
                public IIR(float a0, List<float> a, List<float> b)
                {
                    if (a.Count != b.Count)
                    {
                        this.Log("Different count in filter past values " + a.Count + " vs " + b.Count, LogType.Error);
                    initialized = true;
                        return;
                    }

                    pastInputValues.SetCapacity(a.Count);
                    pastOutputValues.SetCapacity(a.Count);

                    initialized = true;
                }

                /// <summary>
                /// Advances a moment in the filter.
                /// </summary>
                /// <param name="newInputValue">The next input received (x0)</param>
                /// <returns>The latest output (y0). Returns the input anything goes wrong.</returns>
                public float AddAndQuery(float newInputValue)
                {
                    if (!initialized)
                    {
                        this.Log("Filter not initialized!", LogType.Error);
                        return newInputValue;
                    }

                    float weightSum = 0;

                    float newOutputValue = a0 * newInputValue;
                    weightSum += a0;

                    for (int i = 0; i < pastInputValues.Count; i++)
                    {
                        // this.Log(i + " / " + pastInputValues.Count);
                        newOutputValue += a[a.Count - i - 1] * pastInputValues[i];
                        newOutputValue += b[b.Count - i - 1] * pastOutputValues[i];

                        // weightSum += a[i] + b[i];
                    }

                    // newOutputValue /= weightSum;

                    pastInputValues.Enqueue(newInputValue);

                    pastOutputValues.Enqueue(newOutputValue);

                    // Wait until we are ready
                    if (pastInputValues.Count == pastInputValues.Capacity)
                        return newOutputValue;
                    else
                        return newInputValue;
                }

                public enum Type { Bandpass }

            }
        }
    }
}
