using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Peripherals.LPT.Core
{
    /// <summary>
    /// Unit Test for <see cref="LPTAccess"/>
    /// </summary>
    public class LPT_Test : MonoBehaviour
    {
        private LPTAccess pa;

        [Header("Editor")]
        public Text debugText;
        public InputField portInput;
        public Button initializeButton;
        public GameObject portOverlay;

        public InputField sendCode1;
        public InputField sendCode2;
        public InputField sendCode3;
        public InputField delayBetweenCodes;
        public Button sendButton;
        public GameObject codeOverlay;

        void Start()
        {
            initializeButton.onClick.AddListener(InitializeButton_OnClick);
            codeOverlay.SetActive(true);
            portOverlay.SetActive(false);
            portInput.text = "888";
        }

        private void InitializeButton_OnClick()
        {
            if (pa != null) return;
            try
            {
                short address = short.Parse(portInput.text);
                pa = new LPTAccess(address);
                sendButton.onClick.AddListener(SendButton_Click);
                codeOverlay.SetActive(false);
                portOverlay.SetActive(true);
            }
            catch (Exception ex)
            {
                debugText.text = ex.Message;
            }
        }

        private void SendButton_Click()
        {
            StartCoroutine(SendCodesIE());
        }

        private void SendData(short data)
        {
            AddLine("Sending Data:" + data);
            try
            {
                pa.Write(data);
            }
            catch (Exception ex)
            {
                AddLine(ex.ToString());
            }
        }

        private IEnumerator SendCodesIE()
        {
            debugText.text = "";
            float delayMS = 0;
            try
            {
                delayMS = TGP.Helpers.Utility_Helper.ToFloat_FromCSV(delayBetweenCodes.text);
            }
            catch (Exception ex)
            {
                AddLine(ex.ToString());
                yield break;
            }

            float delaySec = delayMS / 1000f;

            short data = short.Parse(sendCode1.text);
            SendData(data);

            yield return new WaitForSeconds(delaySec);

            data = short.Parse(sendCode2.text);
            SendData(data);

            yield return new WaitForSeconds(delaySec);

            data = short.Parse(sendCode3.text);
            SendData(data);

            AddLine("Finish sending the codes");
        }

        private void AddLine(string line)
        {
            debugText.text += "\n" + line;
        }
    }
}