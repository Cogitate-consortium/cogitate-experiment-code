using Helpers.Engine;
using UnityEngine;
using UnityEngine.UI;

namespace Peripherals.UserInput.Test
{
    public class KeyboardListener_Test : MonoBehaviour
    {
        public Text eventText;
        public Text historyText;

        private EventInfo lastEventInfo;
        private float displayForSeconds = 0.1f;

        // Start is called before the first frame update
        void Start()
        {
            InputManager.onKey_TS += InputManager_onKey_TS;
            InputManager.onKeyDown_TS += InputManager_onKeyDown_TS;
            InputManager.onKeyUp_TS += InputManager_onKeyUp_TS;
        }

        private void InputManager_onKeyUp_TS(object sender, InputManager.HighAccuracyEventArgs e)
        {
            LogKey_TS("KeyUp", e.key);
        }

        private void InputManager_onKeyDown_TS(object sender, InputManager.HighAccuracyEventArgs e)
        {
            LogKey_TS("KeyDown", e.key);
        }

        private void InputManager_onKey_TS(object sender, TGP.Helpers.EventArgs<KeyCode> e)
        {
            LogKey_TS("KeyPress", e.value);
        }

        private void LogKey_TS(string eventName, KeyCode key)
        {
            lastEventInfo = new EventInfo(TimeWrapper.time_NotTS, eventName, key);
        }

        // Update is called once per frame
        void Update()
        {
            if (lastEventInfo == null) return;

            // Are we done ?
            if (TimeWrapper.time_NotTS - lastEventInfo.timestamp > displayForSeconds)
            {
                eventText.text = "Listening...";
                lastEventInfo = null;
            }
            else
            {
                string lastEventInfo_Str = lastEventInfo.eventName + " : " + lastEventInfo.key.ToString();
                eventText.text = lastEventInfo_Str;
                historyText.text = historyText.text.Insert(0, lastEventInfo_Str + "\n");
            }
        }

        private class EventInfo
        {
            public float timestamp = 0;
            public string eventName;
            public KeyCode key;

            public EventInfo(float lastKeyPress, string eventName, KeyCode key)
            {
                this.timestamp = lastKeyPress;
                this.eventName = eventName;
                this.key = key;
            }
        }
    }
}