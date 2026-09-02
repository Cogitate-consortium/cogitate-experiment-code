using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TGP.Helpers;
using UnityEngine;
using Game.Core;
using Game.Entities.Player;
using Game.Entities.Interactables;
using Game.Managers.GameplayManager;

namespace Game.Systems.Replay
{
    public class ReplaySystem : MonoBehaviour
    {
        public static event EventHandler<EventArgs<ReplayRecord>> onPlayerCommand;
        public static event EventHandler<EventArgs<ReplayRecord>> onInteractableSpawn;
        public static event EventHandler<EventArgs<ReplayRecord>> onPlayerUsePowerUp;
        public static event EventHandler<EventArgs<ReplayRecord>> onInteractableJump;

        public static string TryGetRecordedLevelJSON(string replayLevelName)
        {
            ReplaySystemFile replay = GetReplay(replayLevelName);

            if (replay == null)
                return null;

            replay.PrepareBeforeWriting();
            return JsonUtility.ToJson(replay, true);
        }

        public static void SetRecordedLevelsJSON(Dictionary<string, string> recordedLevelsJSON)
        {
            recordedLevels.Clear();
            foreach (KeyValuePair<string, string> kVP in recordedLevelsJSON)
            {
                ReplaySystemFile replay = JsonUtility.FromJson<ReplaySystemFile>(kVP.Value);
                replay.PrepareAfterReading();
                recordedLevels.Add(kVP.Key, replay);
            }
        }

        private static readonly Dictionary<string, ReplaySystemFile> recordedLevels = new Dictionary<string, ReplaySystemFile>();
        private ReplaySystemFile activeRecordLevel;

        public bool isRecording { get; private set; }
        public bool isReplaying { get; private set; }
        public bool isPaused { get; private set; }

        private Coroutine replayCR;

        #region Public Methods

        public void Initialize()
        {
        }

        public void DeInitialize()
        {
            StopRecording();
            StopReplay();
            activeRecordLevel = null;
        }

        /// <summary>
        /// [190717, KON] The logic for whether to record or not is now inside replay system.
        /// </summary>
        public void TryStartRecording(string levelName, int levelID_1Based)
        {
            if (isReplaying)
                StopReplay();

            activeRecordLevel = new ReplaySystemFile();
            activeRecordLevel.levelID_1Based = levelID_1Based;
            activeRecordLevel.levelName = levelName;

            // Throw away the previous one
            recordedLevels.AddOrUpdate(activeRecordLevel.levelName, activeRecordLevel);

            isRecording = true;
            StartCoroutine(RecordPerformance());
            this.Log("Replay started Recording!");
        }

        public void StopRecording()
        {
            //if (activeRecordLevel != null && isRecording)
            //    SaveReplayToFile();

            isRecording = false;
            //Debug.LogWarning("Replay stopped Recording!");
        }

        public void StartReplay()
        {
            if (isRecording)
                StopRecording();

            isReplaying = true;
            if (replayCR != null)
                StopCoroutine(replayCR);

            replayCR = StartCoroutine(ReplayIE());
            this.LogWarning("Replay Started!");
        }

        public void StopReplay()
        {
            isReplaying = false;
            if (replayCR != null)
                StopCoroutine(replayCR);
            //Debug.LogWarning("Replay Stopped!");
        }

        /// <summary>
        /// 
        /// </summary>
        public void LoadReplay(string levelName)
        {
            activeRecordLevel = GetReplay(levelName);

            string errorMessage = "";

            if (activeRecordLevel == null)
                errorMessage = string.Format("ERROR:: Couldn't load replay for world {0}, and no replay data for other worlds available. Play some of the _2, _4 levels before playing localizers.", levelName);
            else if (!activeRecordLevel.levelName.EqualsInvariant(levelName))
                errorMessage = string.Format("WARNING :: Couldn't load replay for world {0}, loaded replay for world {1} instead", levelName, activeRecordLevel.levelName);
                
            if (errorMessage != "")
            {
                this.LogError(errorMessage);
                onErrorMessage?.Invoke(this, errorMessage);
            }
        }

        /// <summary>
        /// 1-based
        /// </summary>
        public static int GetRecordedReplayID(string replayLevelName)
        {
            ReplaySystemFile replay = GetReplay(replayLevelName);
            return replay == null ? -1 : replay.levelID_1Based;
        }

        public static bool HasReplay()
        {
            return recordedLevels.Count > 0;
        }

        // Used only for incomplete runs (ie. when we only have 1-2 replay levels
        private static int lastRecordedLevelIndexUsed = 0;
        private static Dictionary<string, ReplaySystemFile> temporaryAssignments = new Dictionary<string, ReplaySystemFile>();

        private static ReplaySystemFile GetReplay(string replayLevelName)
        {
            if (recordedLevels.Count == 0)
            {
                Debug_Helper.LogWarning(typeof(ReplaySystem), "Did not have any records, returning null");
                return null;
            }

            if (!recordedLevels.ContainsKey(replayLevelName))
            {
                // Have we already assigned it ?
                if (temporaryAssignments.ContainsKey(replayLevelName))
                    return temporaryAssignments[replayLevelName];

                ReplaySystemFile output = recordedLevels.Values.ToList().GetMod(lastRecordedLevelIndexUsed++);
                temporaryAssignments.Add(replayLevelName, output);

                Debug_Helper.LogWarning(typeof(ReplaySystem), "Did not have a record for " + replayLevelName + " | Picked " + output?.levelName);

                return output;
            }

            return recordedLevels[replayLevelName];
        }

        public void Pause(bool pause)
        {
            isPaused = pause;
        }

        public void AddPlayerCommand_TS(PlayerController.Command command)
        {
            if (!isRecording) return;

            ReplayRecord frame = new CommandRecord(GameManager.elapsedTimeSeconds_Level_NoPauses, command);

            SafeAddFrame(frame);
        }

        public void AddInteractableSpawn(Interactable interactable)
        {
            if (!isRecording) return;

            ReplayRecord frame = new SpawnRecord(GameManager.elapsedTimeSeconds_Level_NoPauses, interactable.GetInstanceID(), interactable.transform.position, interactable.colorType);

            SafeAddFrame(frame);
        }

        public void RecordPowerUp(PowerUp powerUp, ConsumeOrAcquire effect)
        {
            if (!isRecording) return;

            ReplayRecord frame = new PowerUpRecord(GameManager.elapsedTimeSeconds_Level_NoPauses, powerUp, effect);

            SafeAddFrame(frame);
        }

        public void RecordInteractableJump(Interactable interactable, float directionX)
        {
            if (!isRecording) return;

            ReplayRecord frame = new InteractableJumpRecord(GameManager.elapsedTimeSeconds_Level_NoPauses, interactable.GetInstanceID(), directionX);

            SafeAddFrame(frame);
        }

        public void RecordPerformance(float performance)
        {
            if (!isRecording) return;

            ReplayRecord frame = new PerformanceRecord(GameManager.elapsedTimeSeconds_Level_NoPauses, performance);

            SafeAddFrame(frame);
        }

        private void SafeAddFrame(ReplayRecord frame)
        {
            lock (activeRecordLevel.recordedFrames)
                activeRecordLevel.recordedFrames.Add(frame);
        }

        #endregion

        #region public Logic


        public static event EventHandler<string> onErrorMessage;
        public static event EventHandler<StatusArgs> onStatusUpdate;

        public enum State { Start, End, LoopingFromBegin, Finished }
        public class StatusArgs : EventArgs
        {
            public State state;
            public int levelID_1Based;
            public string levelName;
            public int numFrames;

            public StatusArgs(State state, ReplaySystemFile activeRecordLevel)
            {
                this.state = state;
                levelID_1Based = activeRecordLevel.levelID_1Based;
                levelName = activeRecordLevel.levelName;
                numFrames = activeRecordLevel.recordedFrames.Count;
            }
        }

        private IEnumerator ReplayIE()
        {
            if (activeRecordLevel.recordedFrames.Count == 0)
            {
                this.LogWarning("There are no replay data to playback!");
                yield break;
            }

            onStatusUpdate?.Invoke(this, new StatusArgs(State.Start, activeRecordLevel));
            
            // Fire up the Performance Interpolator
            Coroutine performanceReplayCR = StartCoroutine(ReplayPerformance());
            lastRec = null;
            nextRec = null;

            int replayIndex = 0;
            float timeLastLoop = 0;
            while (isReplaying)
            {
                if (replayIndex >= activeRecordLevel.recordedFrames.Count)
                {
                    replayIndex = replayIndex % (activeRecordLevel.recordedFrames.Count - 1);

                    onStatusUpdate?.Invoke(this, new StatusArgs(State.End, activeRecordLevel));
                    onStatusUpdate?.Invoke(this, new StatusArgs(State.LoopingFromBegin, activeRecordLevel));
                    this.Log(string.Format("Spawned:{0} | Recorded:{1}", GameManager.___TEMP_NUMBEROFSPAWNS, activeRecordLevel.GetTotalNumberOfSpawns()));
                    timeLastLoop = GameManager.elapsedTimeSeconds_Level_NoPauses;
                    this.Log("Replay reached its end... Looping from beginning");
                }

                ReplayRecord record = activeRecordLevel.recordedFrames[replayIndex];
                if (record.elapsedTime + timeLastLoop <= GameManager.elapsedTimeSeconds_Level_NoPauses)
                {
                    switch (record.type)
                    {

                        case ReplayRecord.IRecordType.Spawn:
                            SpawnRecord spawnRec = record as SpawnRecord;
                            this.Log(string.Format("Reading record TimesinceLevel:{4} | time:{0} | type:{1} | id:{2} | pos:{3}", spawnRec.elapsedTime, spawnRec.interactableType, spawnRec.instanceID, spawnRec.position, GameManager.elapsedTimeSeconds_Level_NoPauses));
                            RaiseRecordedInteractableSpawnEvent(record);
                            break;
                        case ReplayRecord.IRecordType.Command:
                            RaiseRecordedPlayerCommandEvent(record);
                            break;
                        case ReplayRecord.IRecordType.PowerUp:
                            RaiseRecordedPlayerPowerUpEvent(record);
                            break;
                        case ReplayRecord.IRecordType.InteractableJump:
                            RaiseRecordedInteractableJumpEvent(record);
                            break;
                        case ReplayRecord.IRecordType.Performance:
                            lastRec = nextRec;
                            PerformanceRecord perfRec = activeRecordLevel.GetNextPerformanceEntry(replayIndex);

                            if (perfRec == null)
                            {
                                this.LogWarning("Reached end of replay - should restart soon");
                                break;
                            }

                            nextRec = perfRec;
                            break;
                    }

                    replayIndex++;
                }

                yield return null;

                while (isPaused)
                    yield return null;
            }

            onStatusUpdate?.Invoke(this, new StatusArgs(State.Finished, activeRecordLevel));

            if (performanceReplayCR != null)
                StopCoroutine(performanceReplayCR);
        }

        internal static void RemoveRecordedLevel(string replayLevelName)
        {
            if (!recordedLevels.ContainsKey(replayLevelName))
            {
                Debug_Helper.LogWarning(typeof(ReplaySystem), "Did not have a record for " + replayLevelName);
                return;
            }

            recordedLevels.Remove(replayLevelName);
            Debug_Helper.LogWarning(typeof(ReplaySystem), "Removed record for " + replayLevelName);
            return;
        }

        PerformanceRecord lastRec = null;
        PerformanceRecord nextRec = null;

        private IEnumerator ReplayPerformance()
        {
            while (true)
            {
                while (nextRec == null) // We're not going somewhere yet
                    yield return null;

                if (lastRec == null) // Handle first time in
                    lastRec = nextRec;

                float t_Last = lastRec.elapsedTime;
                float t_Next = nextRec.elapsedTime;
                float t = GameManager.elapsedTimeSeconds_Level_NoPauses;

                float p_Last = lastRec.performance;
                float p_Next = nextRec.performance;
                float p = t.Retargeted(t_Last, t_Next, p_Last, p_Next);

                GameManager.SetOverridePerformance_LocalizerOnly(p);
                // Debug.Log("{0} :: {1}"._Format(t.ToString("#.00"), p.PercentileToPercent()));
                yield return null;
            }
        }

        private IEnumerator RecordPerformance()
        {
            WaitForSeconds waitFor = new WaitForSeconds(1f);
            while (true)
            {
                RecordPerformance(GameManager.GetCurrentPerformance_GameOnly());
                yield return waitFor;
            }
        }

        private void RaiseRecordedPlayerCommandEvent(ReplayRecord replayFrame)
        {
            if (!isReplaying) return;

            onPlayerCommand?.Invoke(this, new EventArgs<ReplayRecord>(replayFrame));
        }

        private void RaiseRecordedInteractableSpawnEvent(ReplayRecord replayFrame)
        {
            if (!isReplaying) return;

            onInteractableSpawn?.Invoke(this, new EventArgs<ReplayRecord>(replayFrame));
        }

        private void RaiseRecordedPlayerPowerUpEvent(ReplayRecord replayFrame)
        {
            if (!isReplaying) return;

            onPlayerUsePowerUp?.Invoke(this, new EventArgs<ReplayRecord>(replayFrame));
        }

        private void RaiseRecordedInteractableJumpEvent(ReplayRecord replayFrame)
        {
            if (!isReplaying) return;

            onInteractableJump?.Invoke(this, new EventArgs<ReplayRecord>(replayFrame));
        }

        public class RuntimeConfig
        {
            public bool shouldRecord;
            public string replayLevelName;

            public RuntimeConfig(bool shouldRecord, string replayLevelName)
            {
                this.shouldRecord = shouldRecord;
                this.replayLevelName = replayLevelName;
            }
        }

        #endregion

        /*
         *  New Replay data structure doesn't allow to save data to file, as treaded with abstract class.
         * 

        public void SaveReplayToFile()
        {
            FileWrapper.WriteToFile(ApplicationLibrary.replayFilePath, JsonUtility.ToJson(activeRecordLevel, true));
            Debug.LogWarning("Saved replay to file!");
        }

        public void LoadReplayFromFile()
        {
            string jsonFile = System.IO.File.ReadAllText(ApplicationLibrary.replayFilePath);
            activeRecordLevel = JsonUtility.FromJson<ReplaySystemFile>(jsonFile);
            Debug.LogWarning("loaded replay to file!");
        }

         */
    }

    [System.Serializable]
    public class ReplaySystemFile
    {
        /// <summary>
        /// 1-based
        /// </summary>
        public int levelID_1Based;
        public string levelName;

        [NonSerialized]
        public List<ReplayRecord> recordedFrames = new List<ReplayRecord>();

        public void PrepareAfterReading()
        {
            foreach (ReplayRecord rR in spawnRecords)
                recordedFrames.Add(rR);
            foreach (ReplayRecord rR in commandRecords)
                recordedFrames.Add(rR);
            foreach (ReplayRecord rR in powerUpRecords)
                recordedFrames.Add(rR);
            foreach (ReplayRecord rR in interactableJumpRecords)
                recordedFrames.Add(rR);
            foreach (ReplayRecord rR in performanceRecords)
                recordedFrames.Add(rR);

            recordedFrames = recordedFrames.CustomOrderBy(a => a.elapsedTime);
        }

        public void PrepareBeforeWriting()
        {
            foreach (ReplayRecord rR in recordedFrames)
            {
                switch (rR.type)
                {
                    case ReplayRecord.IRecordType.Spawn:
                        spawnRecords.Add(rR as SpawnRecord);
                        break;
                    case ReplayRecord.IRecordType.Command:
                        commandRecords.Add(rR as CommandRecord);
                        break;
                    case ReplayRecord.IRecordType.PowerUp:
                        powerUpRecords.Add(rR as PowerUpRecord);
                        break;
                    case ReplayRecord.IRecordType.InteractableJump:
                        interactableJumpRecords.Add(rR as InteractableJumpRecord);
                        break;
                    case ReplayRecord.IRecordType.Performance:
                        performanceRecords.Add(rR as PerformanceRecord);
                        break;
                }
            }
        }

        public List<SpawnRecord> spawnRecords = new List<SpawnRecord>();
        public List<CommandRecord> commandRecords = new List<CommandRecord>();
        public List<PowerUpRecord> powerUpRecords = new List<PowerUpRecord>();
        public List<InteractableJumpRecord> interactableJumpRecords = new List<InteractableJumpRecord>();
        public List<PerformanceRecord> performanceRecords = new List<PerformanceRecord>();

        public int GetTotalNumberOfSpawns()
        {
            int numberOfSpawns = 0;
            for (int i = 0; i < recordedFrames.Count; i++)
            {
                if (recordedFrames[i].type == ReplayRecord.IRecordType.Spawn)
                    numberOfSpawns++;
            }
            return numberOfSpawns;
        }

        public PerformanceRecord GetNextPerformanceEntry(int currentIndex)
        {
            int nextIdx = currentIndex + 1;

            if (nextIdx >= recordedFrames.Count) return null;

            for (int i = nextIdx; i < recordedFrames.Count; i++)
            {
                if (recordedFrames[i].type == ReplayRecord.IRecordType.Performance)
                    return recordedFrames[i] as PerformanceRecord;
            }
            return null;
        }
    }

    [System.Serializable]
    public class ReplayRecord
    {
        [System.Serializable]
        public enum IRecordType { Spawn, Command, PowerUp, InteractableJump, Performance } // Key,

        public IRecordType type;
        public float elapsedTime;

        public ReplayRecord(IRecordType type, float elapsedTime)
        {
            this.type = type;
            this.elapsedTime = elapsedTime;
        }
    }

    [System.Serializable]
    public class SpawnRecord : ReplayRecord
    {
        public Vector3 position;
        public ObjectColorType interactableType;
        public int instanceID;

        public SpawnRecord(float elapsedTime, int instanceID, Vector3 position, ObjectColorType interactableType)
            : base(IRecordType.Spawn, elapsedTime)
        {
            // this.Log(string.Format("Creating record GameTime:{0} | type:{1} | id:{2} | pos:{3}", elapsedTime, interactableType, instanceID, position));
            this.instanceID = instanceID;
            this.position = position;
            this.interactableType = interactableType;
        }
    }

    [System.Serializable]
    public class CommandRecord : ReplayRecord
    {
        public PlayerController.Command command;

        public CommandRecord(float elapsedTime, PlayerController.Command command)
            : base(IRecordType.Command, elapsedTime)
        {
            this.command = command;
        }
    }

    [System.Serializable]
    public class PowerUpRecord : ReplayRecord
    {
        public PowerUp powerUp;
        public ConsumeOrAcquire effect;

        public PowerUpRecord(float elapsedTime, PowerUp powerUp, ConsumeOrAcquire effect)
            : base(IRecordType.PowerUp, elapsedTime)
        {
            this.powerUp = powerUp;
            this.effect = effect;
        }
    }

    [System.Serializable]
    public class InteractableJumpRecord : ReplayRecord
    {
        public int instanceID;
        public float positionX;

        public InteractableJumpRecord(float elapsedTime, int instanceID, float positionX)
            : base(IRecordType.InteractableJump, elapsedTime)
        {
            this.instanceID = instanceID;
            this.positionX = positionX;
        }
    }

    [System.Serializable]
    public class PerformanceRecord : ReplayRecord
    {
        public float performance;

        public PerformanceRecord(float elapsedTime, float performance)
            : base(IRecordType.Performance, elapsedTime)
        {
            this.performance = performance;
        }
    }
}