using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;

namespace Helpers.Misc.Test
{
    [ExecuteInEditMode]
    public class TestTruncExp : MonoBehaviour
    {
        public bool continuousCalculations = false;
        public bool doCalculate = false;

        // public float θ, b;
        public float λ, max;
        public float min = 0;
        public int numBins = 100;

        void OnDrawGizmos()
        {
            // Your gizmo drawing thing goes here if required...

#if UNITY_EDITOR
            // Ensure continuous Update calls.
            if (!Application.isPlaying)
            {
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
                UnityEditor.SceneView.RepaintAll();
            }
#endif
        }

        // Start is called before the first frame update
        void Update()
        {
            if (!doCalculate) return;

            if (!continuousCalculations)
                doCalculate = false;

            Dictionary<float, float> pdf = new Dictionary<float, float>();
            Dictionary<float, float> CDF = new Dictionary<float, float>();

            if (min >= max)
            {
                Debug.LogError("Min >= Max\n" + min + ", " + max);
                return;
            }

            CDF = Math_Helper.TruncExp_GetCDF(λ, min, max, numBins);

            Debug.Log(CDF.ToReadableString("CDF"));

            float lastCDF_Val = 0;
            float sumP = 0;

            foreach (KeyValuePair<float, float> kVP in CDF)
            {
                float diff = kVP.Value - lastCDF_Val;
                lastCDF_Val = kVP.Value;
                sumP += diff;
                pdf.Add(kVP.Key, diff);
            }
            // Debug.Log(sumP); [2121] pdf values add up to 1
            Debug.Log(pdf.ToReadableString("PDF"));

            string s = "";

            float mean = 0;
            int N = 500;
            for (int i = 0; i < N; i++)
            {
                float x = Math_Helper.QueryCDF(CDF);
                mean += x;
                s += (x * 1000).ToString("#") + "\n";
            }

            mean /= N;



            float expMean = pdf.SumOfProducts();

            // float expMean = (1 / λ) / (e_λmin - e_λmax);
            Debug.LogWarning("EXP MEAN :: " + expMean + "\nACTUAL MEAN :: " + mean + "\n" + s);

            /*
            Dictionary<float, float> cumProb = Math_Helper.TruncExp_GetCumProb(θ, b, dX, min);

            string sProb = "";

            foreach (KeyValuePair<float, float> kVP in cumProb)
                sProb += kVP.Key + "," + kVP.Value + "\n";

            Debug.Log(sProb);

            Debug.Log(
                "EXPECTED MEAN :: " + Math_Helper.TruncExp_GetExpectedMean(θ, b, min) + "\n" +
                cumProb.ToReadableString());

            string s = "";

            for (int i = 0; i < 500; i++)
                s += (Math_Helper.QueryTruncExpDistribution(cumProb) * 1000).ToString("#") + "\n";

            Debug.LogError(s);
            */
            /*
            return;

            Dictionary<float, float> truncExp = new Dictionary<float, float>();
            Dictionary<float, float> truncExp_Cum = new Dictionary<float, float>();
            float totalP = 0;
            float expectedValue = 0;

            // Quantize to estimate from continuous pdf the discrete pdf
            for (float x = dX / 2; x <= b - dX / 2; x += dX)
            {
                float x_Low = Mathf.Max(float.Epsilon, x - dX / 2f);
                float x_High = Mathf.Min(x + dX / 2f, b);

                float p_Low = Math_Helper.TruncExp_GetPDF(x_Low, θ, b);
                float p_High = Math_Helper.TruncExp_GetPDF(x_High, θ, b);

                float p = (p_High + p_Low) / 2 * (x_High - x_Low);

                truncExp.Add(x, p);
                totalP += p;
                truncExp_Cum.Add(x, totalP);
                expectedValue += p * x;
            }

            Debug.Log(totalP);

            float expectedMean = Math_Helper.TruncExp_GetExpectedMean(θ, b);

            Debug.Log(expectedValue + "\n" + expectedMean);

            Debug.Log(truncExp_Cum.ToReadableString());
            */
        }
    }
}