using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Peripherals.Serial.Test
{
    public class SerialPort_Viewport : MonoBehaviour
    {
        public Text outputText;
        public RectTransform viewport;

        // Update is called once per frame
        void Update()
        {
            Vector2 size = viewport.sizeDelta;
            size.y = outputText.rectTransform.sizeDelta.y;
            viewport.sizeDelta = size;
        }
    }
}