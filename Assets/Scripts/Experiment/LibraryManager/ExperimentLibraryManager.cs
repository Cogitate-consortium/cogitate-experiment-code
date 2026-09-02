// NS_REMOVE | Not the Core
using Peripherals.Logging.Core;

// NS_REMOVE | Better Segment
using ExperimentLibrary.Test;

using TGP.Helpers;
using UnityEngine;
using Experiment.Library.Core;
using Helpers.Engine;

namespace ExperimentLibrary
{
    /// <summary>
    /// [SOS] <see cref="Initialize"/> first
    /// TODO Cleanup make non static
    /// </summary>
    [ExecuteInEditMode]
    public class ExperimentLibraryManager
    {
        public static bool isInitialized { get { return instance != null; } }

        public static void Initialize()
        {
            if (isInitialized)
            {
                Debug_Helper.LogWarning(typeof(ExperimentLibraryManager), "Already initialized!");
                return;
            }

            ExperimentPaths.Initialize(dataRootDirectory + "Content/");

            instance = new ExperimentLibraryManager();
        }


        #region UI

        [Header("I/O")]
        public bool DoSaveToFile = false;
        public bool DoLoadFromFile = false;

        [Header("Editor")]
        public ExperimentConfig config;

        #endregion


        #region Internal Management
        private static ExperimentLibraryManager instance;

        public static ExperimentConfig Config { get { return instance.config; } }

        public static bool TryLoadFile(out string error)
        {
            try
            {
                string jsonFile = System.IO.File.ReadAllText(defaultConfigFilePath);
                var _config = JsonUtility.FromJson<ExperimentConfig>(jsonFile);
                error = "";
                return true;
            }
            catch (System.Exception ex)
            {
                error = ex.Message.ToString();
                return false;
            }
        }

        public ExperimentLibraryManager()
        {
            // SOS Do first !
            _LoadFromFile();

            // Check for logical contraints
            CheckForConstraints();

            // Assign directory here, so we can pull it from other Thread (Async logging)
            // Else error 'get_dataPath can only be called from the main thread.'
#if UNITY_EDITOR
            logsDirectory = Application.dataPath + "/../Logs/";
#else
            logsDirectory = dataRootDirectory + "Logs/";
#endif

            if (EditorOnlyUtilities.applicationLibrary_SaveConfigOnStart == true)
            {
                Debug.Log("[Editor Only] Auto-saving config to file");
                _SaveConfigToFile();
                EditorOnlyUtilities.applicationLibrary_SaveConfigOnStart = false;
            }

            EngineWrapper.onUpdate += OnUpdate;
        }

        private void OnUpdate(object sender, float e)
        {
            if (DoSaveToFile)
            {
                DoSaveToFile = false;
                _SaveConfigToFile();
            }
            if (DoLoadFromFile)
            {
                DoLoadFromFile = false;
                _LoadFromFile();
            }
        }

        // private ThemeLibrary themeLibrary;

        // Add here any logical contraints
        private void CheckForConstraints()
        {
            foreach (string errorMessage in config.CheckForConstraints())
                Debug_Helper.LogError(typeof(ExperimentLibraryManager), errorMessage);
        }

        public static void SaveConfigToFile()
        {
            instance?._SaveConfigToFile();
        }

        private void _SaveConfigToFile()
        {
            FileWrapper.WriteToFile_TS(defaultConfigFilePath, JsonUtility.ToJson(config, true));
            Debug_Helper.Log(typeof(ExperimentLibraryManager), "Config file updated in " + defaultConfigFilePath);
        }

        public static void LoadFromFile(string overrideFileToLoadFrom = "")
        {
            instance?._LoadFromFile(overrideFileToLoadFrom);
        }

        private void _LoadFromFile(string overrideFileToLoadFrom = "")
        {
            string fileToLoadFrom = overrideFileToLoadFrom.IsNullOrEmpty() ?
                defaultConfigFilePath : overrideFileToLoadFrom;

            string jsonFile = System.IO.File.ReadAllText(fileToLoadFrom);
            config = JsonUtility.FromJson<ExperimentConfig>(jsonFile);
            config.Initialize();
            Debug_Helper.Log(typeof(ExperimentLibraryManager), "Config file loaded from " + fileToLoadFrom);
        }
        #endregion


        #region Paths

        // Assign directory from GameObject, so we can pull it from other Thread (Async logging)
        // Else error 'get_dataPath can only be called from the main thread.'
        /// <summary>
        /// Ends in "/"
        /// </summary>
        public static string logsDirectory { get; private set; }

        public static string dataRootDirectory
        {
            get { return Application.streamingAssetsPath + "/"; }
        }

        public static string configDirectory
        {
            get { return dataRootDirectory + "Config/"; }
        }

        public static string defaultConfigFilePath
        {
            get { return GetConfigFilePath(configDirectory); }
        }

        public static string GetConfigFilePath(string configDirectory)
        {
            return configDirectory + "config.json";
        }

        #endregion


        #region Deprecated
        /*
        public static void HelperDelay(float delay, System.Action callback)
        {
            instance.StartCoroutine(HelperDelayIE(delay, callback));
        }

        private static IEnumerator HelperDelayIE(float delay, System.Action callback)
        {
            yield return new WaitForSeconds(delay);
            callback();
        }
        */

        /*
        public static ThemeLibrary ThemeLibrary
        {
            get
            {
                if (instance == null) return null;
                if (instance.themeLibrary == null)
                    instance.themeLibrary = new ThemeLibrary();
                return instance.themeLibrary;
            }
        }

        public static ThemePalette DefaultTheme
        {
            get
            {
                return ThemeLibrary.defaultTheme;
            }
        }
        */
        #endregion
    }
}