using TGP.Helpers;
using UnityEngine;

namespace Helpers.Misc.Test
{
    [ExecuteInEditMode]
    public class MathTest : MonoBehaviour
    {
        public bool doCalculate = true;

        // Start is called before the first frame update
        void Start()
        {
            // Debug.Log(DPrime_Helper.DPrime_H_FAs(0.75, 0.1));
        }

        private void Update()
        {
            if (!doCalculate) return;

            Debug.Log(1.AddLeadingSymbols(3, '0'));
            Debug.Log(2.AddLeadingSymbols(3, '0'));
            Debug.Log(12.AddLeadingSymbols(3, '0'));
            Debug.Log(39.AddLeadingSymbols(3, '0'));
            Debug.Log(150.AddLeadingSymbols(3, '0'));
            Debug.Log(13430.AddLeadingSymbols(3, '0'));
        }
    }
}