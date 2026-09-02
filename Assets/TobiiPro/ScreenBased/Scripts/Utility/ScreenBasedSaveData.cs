//-----------------------------------------------------------------------
// Copyright © 2019 Tobii Pro AB. All rights reserved.
//-----------------------------------------------------------------------

using Peripherals.Logging.Core;
using System;
using System.Xml;
using UnityEngine;

namespace Tobii.Research.Unity
{
    public class ScreenBasedSaveData : MonoBehaviour
    {
        #region 200723 Added
        public void SetOverrideFilePath(string fileName)
        {
            overrideFileName = fileName;
        }
        private string overrideFileName;

        internal void TransferFileTo(string fullPath)
        {
            Debug.Log(latestActiveFilePath + " -> " + fullPath);
            FileWrapper.MoveFile(latestActiveFilePath, fullPath, true);
        }

        internal void WriteMessage_TS(string msg)
        {
            lock (_file_lock)
            {
                if (_file == null)
                    return;

                _file.WriteStartElement("Message");
                                
                _file.WriteAttributeString("SystemTimestampMS_Sync", _eyeTracker ? _eyeTracker.currentSystemTimestampMS.ToString() : "-1");
                _file.WriteAttributeString("SystemTimestampMS_SDK", EyeTracker.currentSystemTimestampMS_Backup.ToString());
                _file.WriteAttributeString("DeviceTimestampMS_Sync", _eyeTracker ? _eyeTracker.currentDeviceTimestampMS.ToString() : "-1");
                _file.WriteAttributeString("Message", msg);

                _file.WriteEndElement();
            }
        }
        #endregion

        /// <summary>
        /// Instance of <see cref="ScreenBasedSaveData"/> for easy access.
        /// Assigned in Awake() so use earliest in Start().
        /// </summary>
        public static ScreenBasedSaveData Instance { get; private set; }

        [SerializeField]
        [Tooltip("If true, data is saved.")]
        private bool _saveData;

        [SerializeField]
        [Tooltip("If true, Unity3D-converted data is saved.")]
        private bool _saveUnityData = true;

        [SerializeField]
        [Tooltip("If true, raw gaze data is saved.")]
        private bool _saveRawData = true;

        [SerializeField]
        [Tooltip("Folder in the application root directory where data is saved.")]
        private string _folder = "Data";

        [SerializeField]
        [Tooltip("This key will start or stop saving data.")]
        private KeyCode _toggleSaveData = KeyCode.None;

        /// <summary>
        /// If true, data is saved.
        /// </summary>
        public bool SaveData
        {
            get
            {
                return _saveData;
            }

            set
            {
                _saveData = value;
            }
        }

        private EyeTracker _eyeTracker;
        private XmlWriterSettings _fileSettings;
        private XmlWriter _file;
        private object _file_lock = new object();

        public void Setup(EyeTracker eyeTracker)
        {
            Instance = this;
            _eyeTracker = eyeTracker;
        }

        public void DoUpdate()
        {
            if (!isRecording)
                return;

            lock (_file_lock)
                if (_file == null)
                    return;

            if (!_saveUnityData && !_saveRawData)
            {
                // No one wants to save anyway.
                return;
            }

            var data = _eyeTracker.NextData;
            while (data != default(IGazeData))
            {
                WriteGazeData(data);
                data = _eyeTracker.NextData;
            }
        }

        private void Update()
        {
            return;

            if (Input.GetKeyDown(_toggleSaveData))
            {
                SaveData = !SaveData;
            }

            if (!_saveData)
            {
                // Closes _file and sets it to null.
                CloseDataFile();
                
                return;
            }

            // Opens data file. It becomes non-null.
            OpenDataFile();          

        }

        private void OnDestroy()
        {
            CloseDataFile();
        }

        public void OpenDataFile()
        {
            lock (_file_lock)
            {
                if (_file != null)
                {
                    Debug.Log("Already saving data.");
                    return;
                }

                _fileSettings = new XmlWriterSettings();
                _fileSettings.Indent = true;

                latestActiveFilePath = GetFilePath();
                _file = XmlWriter.Create(latestActiveFilePath, _fileSettings);
                _file.WriteStartDocument();
                _file.WriteStartElement("Data");
            }
        }

        string latestActiveFilePath;
        public bool isRecording;

        private string GetFilePath()
        {
            string folder = Application.dataPath + "/StreamingAssets/_TOBII_TEMP/";
            if (!System.IO.Directory.Exists(folder))
                System.IO.Directory.CreateDirectory(folder);

            return folder + GetFileName();
        }

        /// <summary>
        /// [SOS] use <see cref=" GetFilePath"/> instead
        /// </summary>
        private string GetFileName()
        {
            if (overrideFileName != null && overrideFileName != "")
                return overrideFileName;

            if (!System.IO.Directory.Exists(_folder))
            {
                System.IO.Directory.CreateDirectory(_folder);
            }

            var fileName = string.Format("data_{0}.xml", System.DateTime.Now.ToString("yyyyMMddTHHmmss"));

            return System.IO.Path.Combine(_folder, fileName);
        }

        public void CloseDataFile()
        {
            lock (_file_lock)
            {
                if (_file == null)
                {
#if UNITY_EDITOR
                    Debug.Log("No ongoing recording.");
#endif
                    return;
                }

                _file.WriteEndElement();
                _file.WriteEndDocument();
                _file.Flush();
                _file.Close();
                _file = null;
                _fileSettings = null;
            }
        }

        private void WriteGazeData(IGazeData gazeData)
        {
            lock (_file_lock)
            {
                _file.WriteStartElement("GazeData");

                if (_saveUnityData)
                {
                    _file.WriteAttributeString("SystemTimeStampMS_SDK", gazeData.GetTimestampMS().ToString());
                    _file.WriteEye(gazeData.Left, "Left");
                    _file.WriteEye(gazeData.Right, "Right");
                    _file.WriteRay(gazeData.CombinedGazeRayScreen, gazeData.CombinedGazeRayScreenValid, "CombinedGazeRayScreen");
                }

                if (_saveRawData)
                {
                    _file.WriteRawGaze(gazeData.OriginalGaze);
                }

                _file.WriteEndElement();
            }
        }
    }
}