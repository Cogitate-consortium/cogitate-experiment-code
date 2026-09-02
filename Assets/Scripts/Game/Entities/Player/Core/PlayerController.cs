// NS_DEBATABLE
using Peripherals.UserInput;
// NS_DEBATABLE
using Game.Systems.Replay;

using System;
using TGP.Helpers;
using UnityEngine;
using System.Collections.Generic;

namespace Game.Entities.Player
{
    /// <summary>
    /// [CHECK]
    /// </summary>
    public class PlayerController
    {
        /// <summary>
        /// [SOS] Keep Handlers Thread Safe
        /// </summary>
        public EventHandler<EventArgs<Command>> onCommand_TS;

        public bool listenToReplay = false;

        private bool isActive = true;
        public ReplaySystem replaySystem;

        private Config config;
        private RuntimeConfig runtimeConfig;

        [Serializable]
        public class Config
        {
            public bool ignoreHeldForMovement;
        }

        public class RuntimeConfig
        {
            public List<KeyCode> actionLeft;
            public List<KeyCode> actionRight;

            public RuntimeConfig(IList<KeyCode> actionLeft, IList<KeyCode> actionRight)
            {
                this.actionLeft = new List<KeyCode>(actionLeft);
                this.actionRight = new List<KeyCode>(actionRight);
            }
        }

        protected virtual void HandleInput_TS(KeyCode keyCode, Command.Type type)
        {
            Command command = new Command();
            command.type = type;

            if (type == Command.Type.Hold && config.ignoreHeldForMovement)
                return;

            if (runtimeConfig.actionLeft.Contains(keyCode))
            {
                command.direction = Direction_2D.Left;
                command.magnitude = 1;// dT;//Mathf.Abs(Input.GetAxis("Horizontal"));
            }
            else if (runtimeConfig.actionRight.Contains(keyCode))
            {
                command.direction = Direction_2D.Right;
                command.magnitude = 1; // dT;// Mathf.Abs(Input.GetAxis("Horizontal"));
            }
            else if (keyCode == KeyCode.UpArrow)
            {
                command.direction = Direction_2D.Up;
                command.magnitude = 1; // dT;// Mathf.Abs(Input.GetAxis("Vertical"));
            }
            else if (keyCode == KeyCode.DownArrow)
            {
                command.direction = Direction_2D.Down;
                command.magnitude = 1; // dT;// Mathf.Abs(Input.GetAxis("Vertical"));
            }
            else
            {
                return;
            }

            if (type == Command.Type.Up)
                command.magnitude = -1;

            // this.LogError(Helpers.Engine.TimeWrapper.GetCurrentTimestamp_TS() + " : INPUT received " + command);
            FireCommandEvent_TS(command);
        }

        private void FireCommandEvent_TS(Command command)
        {
            if (!isActive) return;

            if (!listenToReplay && replaySystem?.isRecording == true)
                replaySystem.AddPlayerCommand_TS(command);

            onCommand_TS?.Invoke(this, command);
        }

        public virtual void Initialize(Config config, RuntimeConfig runtimeConfig)
        {
            this.Log("Init!");

            this.config = config;
            this.runtimeConfig = runtimeConfig;

            InputManager.onKeyUp_TS += InputManager_OnKeyUp_TS;
            InputManager.onKeyDown_TS += InputManager_OnKeyDown_TS;
            InputManager.onKey_TS += InputManager_OnKey_TS;

            ReplaySystem.onPlayerCommand += ReplaySystem_OnPlayerCommand;
        }

        public virtual void DeInitialize()
        {
            this.Log("De-Init!");

            InputManager.onKeyUp_TS -= InputManager_OnKeyUp_TS;
            InputManager.onKeyDown_TS -= InputManager_OnKeyDown_TS;
            InputManager.onKey_TS -= InputManager_OnKey_TS;

            ReplaySystem.onPlayerCommand -= ReplaySystem_OnPlayerCommand;
        }

        private void InputManager_OnKeyUp_TS(object sender, InputManager.HighAccuracyEventArgs e)
        {
            if (listenToReplay) return;

            HandleInput_TS(e.key, Command.Type.Up);
        }

        private void InputManager_OnKeyDown_TS(object sender, InputManager.HighAccuracyEventArgs e)
        {
            if (listenToReplay) return;

            HandleInput_TS(e.key, Command.Type.Down);
        }

        private void InputManager_OnKey_TS(object sender, EventArgs<KeyCode> e)
        {
            if (listenToReplay) return;

            HandleInput_TS(e, Command.Type.Hold);
        }

        private void ReplaySystem_OnPlayerCommand(object sender, EventArgs<ReplayRecord> e)
        {
            if (!listenToReplay) return;

            CommandRecord record = e.value as CommandRecord;

            if (record == null)
            {
                this.LogError("Shouldnt happen");
                return;
            }

            FireCommandEvent_TS(record.command);
        }

        [Serializable]
        public struct Command
        {
            public Direction_2D direction;
            public float magnitude;
            public Type type;

            public enum Type { Down, Up, Hold }

            public override string ToString()
            {
                return "{0} {1} {2}"._Format(direction, magnitude, type);
            }
        }

        public void Restart()
        {
            this.Log("Restart");
        }

        public void Pause(bool doFreeze)
        {
            isActive = !doFreeze;
        }
    }
}