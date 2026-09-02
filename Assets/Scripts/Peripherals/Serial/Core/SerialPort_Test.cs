// References
using System;
using System.Collections;
using System.IO.Ports;
using UnityEngine;
using UnityEngine.UI;

namespace Peripherals.Serial.Test
{
    public class SerialPort_Test : MonoBehaviour
    {
        private SerialPort serialPort;

        [Header("Editor")]
        public Text debugText;
        public Text readText;
        public Button readClearButton;
        public InputField portInput;
        public InputField portBaudInput;
        public InputField dataSizeInput;
        public Button initializeButton;
        public GameObject portOverlay;

        public InputField sendCode1;
        public InputField sendCode2;
        public InputField sendCode3;
        public InputField delayBetweenCodes;
        public Button sendButton;
        public GameObject codeOverlay;
        public Toggle methodToggle;

        void Start()
        {
            initializeButton.onClick.AddListener(InitializeButton_OnClick);
            readClearButton.onClick.AddListener(ReadClearButton_OnClick);
            codeOverlay.SetActive(true);
            portOverlay.SetActive(false);
            portInput.text = "COM3";
            portBaudInput.text = "115200";
            portBaudInput.text = "9600";
            dataSizeInput.text = "8000";
            dataSizeInput.text = "4096";

            delayBetweenCodes.text = "50";
            sendCode1.text = "0";
            sendCode2.text = "0";
            sendCode3.text = "0";
        }

        private void ReadClearButton_OnClick()
        {
            readText.text = "";
        }

        private void OnDestroy()
        {
            if (serialPort != null)
            {
                serialPort.Close();
                serialPort.Dispose();
            }
        }

        float delayMS = 50;
        private IEnumerator SerialPortReadDataIE()
        {
            while (true)
            {
                ReadSerialPort();
                yield return new WaitForSeconds(delayMS / 1000f);
            }
        }

        private void Update()
        {
            ReadSerialPort();
        }

        private void ReadSerialPort()
        {
            if (serialPort == null || !serialPort.IsOpen) return;

            string data = serialPort.ReadExisting().Trim('\0');
            if (data.Length == 0) return;
            //readText.text = readText.text.Insert(0, data);
            readText.text += data;
        }

        private void InitializeButton_OnClick()
        {
            debugText.text = "";
            if (serialPort != null && serialPort.IsOpen) return;
            try
            {
                int baudRate = int.Parse(portBaudInput.text);
                serialPort = new SerialPort(portInput.text, baudRate);
                int dataSize = int.Parse(dataSizeInput.text);
                serialPort.ReadBufferSize = dataSize;
                serialPort.Open();
                serialPort.DataReceived += SerialPort_DataReceived;


                sendButton.onClick.AddListener(SendButton_Click);
                codeOverlay.SetActive(false);
                portOverlay.SetActive(true);
            }
            catch (Exception ex)
            {
                debugText.text = ex.Message;
            }
        }

        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            AddLine(e.EventType.ToString());
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            ReadSerialPort();
        }

        private void SendButton_Click()
        {
            StartCoroutine(SendCodesIE());
        }

        private void SendData(string data)
        {
            AddLine("Sending Data:" + data);
            try
            {
                if (methodToggle.isOn)
                    serialPort.WriteLine(data);
                else
                    serialPort.Write(data);
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

            SendData(sendCode1.text);

            yield return new WaitForSeconds(delaySec);

            SendData(sendCode2.text);

            yield return new WaitForSeconds(delaySec);

            SendData(sendCode3.text);

            AddLine("Finish sending the codes");
        }

        private void AddLine(string line)
        {
            debugText.text += "\n" + line;
        }
    }
}