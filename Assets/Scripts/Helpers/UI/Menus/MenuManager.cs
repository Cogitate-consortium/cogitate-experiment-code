// NS_REMOVE | Config
using ExperimentLibrary;
// NS_REMOVE | TheManager
using Experiment.Managers;

using System;
using TGP.Helpers;
using UnityEngine;
using Helpers.Engine;
using Helpers.Async; // Just runs on Main Thread
using Peripherals.UserInput;
using System.Collections.Generic;

namespace Helpers.UI.Menus
{
    /// <summary>
    /// [RENAME] Maybe it's just a menu?
    /// </summary>
    public class MenuManager : MonoBehaviour
    {
        private bool doDebugAutoAnswer = false;
        private float EDITOR_LAST_ANSWER_TIME = 0;
        protected float EDITOR_TIME_SINCE_LAST_ANSWER { get { return TimeWrapper.time_NotTS - EDITOR_LAST_ANSWER_TIME; } }

        private const float MIN_WINDOW_DURATION = 1.0f;
        private const float MIN_ANSWER_DISTANCE = 0.5f;

        public event EventHandler<EventArgs> onDestroy;

        private List<KeyCode> hasRegisteredKeyDown = new List<KeyCode>();

        protected float timeAlive { get { return TimeWrapper.time_NotTS - timeCreated; } }
        protected float timeCreated { get; private set; }

        protected virtual void DebugInput() { }

        public virtual void Initialize()
        {
            timeCreated = TimeWrapper.time_NotTS;

            this.Log("Init!");
            InputManager.onKeyDown_TS += InputManager_OnKeyDown_TS;
            InputManager.onKeyUp_TS += InputManager_OnKeyUp_TS;
        }

        public virtual void DeInitialize()
        {
            this.Log("De-Init!");

            InputManager.onKeyDown_TS -= InputManager_OnKeyDown_TS;
            InputManager.onKeyUp_TS -= InputManager_OnKeyUp_TS;
        }

        protected virtual void OnUpdate(float dT) { }

        private void Update()
        {
            // We want this to run BESIDES what happens on input 
            if (InputManager.GetKeyUp(ExperimentLibraryManager.Config.Input.EXPERIMENTER_ESCAPE))
            {
                HandlePress(PressType.Escape);
            }

            OnUpdate(TimeWrapper.deltaTime_SinceLastUpdate_NotTS);

            if (ExperimentManagerSession.EXPERIMENTER_AUTO_ANSWER && EDITOR_TIME_SINCE_LAST_ANSWER > MIN_WINDOW_DURATION && timeAlive > MIN_WINDOW_DURATION)
            {
                if (doDebugAutoAnswer)
                    this.LogWarning("AUTO_ANSWER - Replying Yes :: " + name);
                HandlePress(PressType.Yes);
            }

            // [200505] Ghosts
            if (gameObject?.name == null)
                Debug.LogError(this, this);

#if UNITY_EDITOR
            // if (Input.GetKey(KeyCode.M)) DebugInput();
#endif
        }

        private void InputManager_OnKeyDown_TS(object sender, InputManager.HighAccuracyEventArgs e)
        {
            lock (hasRegisteredKeyDown)
                hasRegisteredKeyDown.Add(e.key);
        }

        private void InputManager_OnKeyUp_TS(object sender, InputManager.HighAccuracyEventArgs e)
        {
            if (!this)
            {
                Debug.LogError("WTF");
                return;
            }
            
            lock (hasRegisteredKeyDown)
                if (!hasRegisteredKeyDown.Contains(e.key))
                {
                    // Debug.LogWarning("Released Key that was pressed before the window appeared");
                    return;
                }

            // Debug.Log("MM A :: " + TimeWrapper.currentTimestampMS); // [200505] +1ms from IM B (probably debug log)
            // We don't need anything related to menus to run on high accuracy
            AsyncThread.RunOnMainThread_ASAP_TS(() =>
            {
                // Debug.Log(_name);
                // Debug.Log("MM B :: " + TimeWrapper.currentTimestampMS); // [200505] +1ms from MM A (probably debug log)
                HandleInput(e.key);
            });
        }

        protected virtual void HandleInput(KeyCode keyCode)
        {
            PressType? pT = null;

            if (keyCode == ExperimentLibraryManager.Config.InputKeyCode.Menu_OK)
                pT = PressType.Yes;
            else if (keyCode == ExperimentLibraryManager.Config.InputKeyCode.Menu_Close)
                pT = PressType.No;

            if (pT == null) return;

            if (ExperimentLibraryManager.Config.UI.Get_AllPopups_OnlyProceedViaClick(ExperimentManagerSession.module))
            {
                this.LogWarning("Only proceeding via click!");
                return;
            }

            HandlePress(pT.Value);
        }

        private void HandlePress(PressType pressType)
        {
            if (EDITOR_TIME_SINCE_LAST_ANSWER <= MIN_ANSWER_DISTANCE)
            {
                // this.LogError("Attempted to double-press within min distance - aborting");
                return;
            }

            switch (pressType)
            {
                case PressType.Yes:
                    OnYesPressed();
                    break;
                case PressType.No:
                    OnNoPressed();
                    break;
                case PressType.Escape:
                    OnEscapePressed();
                    break;
            }

            EDITOR_LAST_ANSWER_TIME = TimeWrapper.time_NotTS;
        }

        protected virtual void OnEscapePressed() { }

        protected virtual void OnYesPressed() { }

        protected virtual void OnNoPressed() { }

        protected void OnDestroy()
        {
            // Debug.Log("ONDESTROY");
            DeInitialize();
            onDestroy?.Invoke(this, new EventArgs());
        }

        private enum PressType { Yes, No, Escape }
    }
}