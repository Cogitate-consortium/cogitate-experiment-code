// NS_REMOVE
// using ApplicationUtilities;

using System;
using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;

#if UNITY_EDITOR_OSX
#elif UNITY_STANDALONE_OSXs
#elif PLATFORM_STANDALONE_OSX
#else
using UnityEyeLink;
#endif

namespace Peripherals.EyeTracking.EyeLink
{
    /// <summary>
    /// [CHECK]
    /// </summary>
    public class EyeTrackerManager_EyeLink : EyeTrackerManager_Base
    {
        private Config config;

        protected override string GetName()
        {
            return "EYE_TRACKER_EYELINK";
        }

        protected override void ExperimentOnset()
        {
            // string debugCommands = "";

            foreach (Config.Command command in config.GetAllCommands())
                if (command.enabled)
                {
                    string cmd = command.GetCommand();
                    // debugCommands += cmd + "\n";
                    RequestSendCommand(cmd);
                }

            // % Setup the parser used for data extraction!
            if (config.sendExtraCommands && !config.extraCommands.IsNullOrEmpty())
                foreach (string extraCommand in config.extraCommands)
                {
                    if (extraCommand.IsNullOrEmpty()) continue;
                    // debugCommands += extraCommand + "\n";
                    RequestSendCommand(extraCommand);
                }

            // this.LogWarning(debugCommands);
        }

        protected override bool GetUseSimulation()
        {
            return config.useSimulation;
        }


#if UNITY_EDITOR_OSX
    private void SendCommand(string v) { }
    private bool CreateEDF(string newFileName) { return false; }
    private void CloseEDF() { }
    private void TransferEDF(string fullPath) { }
    private void RunCalibration() { }
    private void TryConnect() { }
    public void WriteToEDF(params string[] msgs) { }
    private void ToggleRecording(bool v) { }
    public static bool HasData(Vector2 gaze) { return false; }
    public void RequestWriteToEDF(params string[] msgs) { }
#elif UNITY_STANDALONE_OSXs
    private void SendCommand(string v) { }
    private bool CreateEDF(string newFileName) { return false; }
    private void CloseEDF() { }
    private void TransferEDF(string fullPath) { }
    private void RunCalibration() { }
    private void TryConnect() { }
    public void WriteToEDF(params string[] msgs) { }
    public static bool HasData(Vector2 gaze) { return false; }
    private void ToggleRecording(bool v) { }
    public void RequestWriteToEDF(params string[] msgs) { }
#elif PLATFORM_STANDALONE_OSX
    private void SendCommand(string v) { }
    private bool CreateEDF(string newFileName) { return false; }
    private void CloseEDF() { }
    private void TransferEDF(string fullPath) { }
    private void RunCalibration() { }
    private void TryConnect() { }
    public void WriteToEDF(params string[] msgs) { }
    public static bool HasData(Vector2 gaze) { return false; }
    private void ToggleRecording(bool v) { }
    public void RequestWriteToEDF(params string[] msgs) { }
#else
        private Core core;

        protected override void OnAwake()
        {
        }

        protected override void OnUpdate(float dT)
        {
        }

        // [TODO?]
        protected override void Toggle(bool on)
        {
        }

        protected override void OnInitialized(EyeTrackerManager_Base.ImplementationConfig config)
        {
            base.OnInitialized(config);

            this.config = config as Config;

            if (this.config == null)
            {
                (this).LogError("Invalid Config provided ; needs to be of type {0} was of type {1}"._Format(
                    typeof(Config), typeof(EyeTrackerManager_Base.ImplementationConfig)));
            }

            core = new Core();
        }

        protected override bool Core_TryConnect()
        {
            return core.Connect();
        }

        protected override bool Core_TryConnect_Simulation()
        {
            return core.SimulateConnect();
        }

        protected override void Core_CloseSystem()
        {
            if (config.sendCloseEyelinkSystemCommandAtExeShutdown)
                RequestSendCommand("close_eyelink_system");
        }

        protected override double Core_GetTrackerTimeMS()
        {
            return core.GetTrackerTimeUsec() / 1000;
        }

        protected override double Core_GetTrackerTimeMS_Fallback()
        {
            return core.GetTrackerTime();
        }

        protected override double Core_GetCurrentTimeMS_Debug()
        {
            return core.GetCurrentTime();
        }

        protected override void Core_WriteMessage_TS(string msg)
        {
            core.WriteMessage(msg);
        }

        protected override void Core_DoCommandRequested(string command)
        {
            core.SendCommand(command);
        }

        protected override void Core_DoRunCalibration(IList<Vector2> calibrationTargets, Action<ProgressReport> progressReportCB)
        {
            progressReportCB?.Invoke(new ProgressReport(false, 0.0f));
            core.Calibration();
            // progressReportCB?.Invoke(new ProgressReport(true, 1.0f));
        }

        protected override string GetTrackingFileNameSuffix()
        {
            return "edf";
        }

        protected override void Core_StartRecording(string filename)
        {
            core.StartRecording(filename);
        }

        protected override void Core_StopRecording()
        {
            core.StopRecoreing();
        }
        protected override void Core_CloseFile()
        {
            core.CloseFile();
        }

        protected override void Core_GetFile(string fullPath)
        {
            core.GetFile(fullPath);
        }

        readonly List<object> core_Events = new List<object>();

        private void GetAllEvents()
        {
            List<object> newEvents = new List<object>();

            while (true)
            {
                object e = core.GetEvent();

                // First null we get, we stop
                if (e.IsNull())
                    break;

                newEvents.Add(e);
            }

            lock (core_Events)
                core_Events.AddRange(newEvents);
        }

        protected override List<Sacada> Core_GetSacades()
        {
            return Core_GetEvent<Core.Sacada, Sacada>(core => core.ToBase());
        }

        protected override List<Blink> Core_GetBlinks()
        {
            return Core_GetEvent<Core.Blink, Blink>(core => core.ToBase());
        }

        protected List<G> Core_GetEvent<T, G>(Func<T, G> convert) where T : class
        {
            if (core == null) return null;

            List<G> result = new List<G>();

            GetAllEvents(); // See if there's any new events to get

            List<object> processedEvents = new List<object>();
            lock (core_Events)
            {
                foreach (object e in core_Events)
                {
                    // [HACK 200527] this "burns" events that are not sacadas ; but we dont care
                    // If we got an event, but it's not a sacada, look deeper
                    if (!(e is T))
                        continue;

                    // We found our sacada
                    T s = e as T;

                    result.Add(convert(s));
                    processedEvents.Add(e);
                }

                core_Events.RemoveRange(processedEvents);
            }

            return result;
        }

        protected override Sample Core_GetSample()
        {
            Core.Sample coreSample = core?.GetSample();

            return coreSample.ToBase(config.validDataThreshold);
        }

        protected override bool Core_IsNull()
        {
            return core.IsNull();
        }

        protected override bool Core_IsConnected()
        {
            return core.IsConnected;
        }

        protected override void Core_CreateTrackingFile(string filePath)
        {
            core.StartFile(filePath);

            // EyeLink sampling rate
            if (config.GetSampleRate().HasValue)
                RequestWriteToTracker_NotTS("EYELINK_SAMPLE_RATE " + config.GetSampleRate()); // [TODO] :: Get sample rate otherwise
            else
                RequestWriteToTracker_NotTS("EYELINK_SAMPLE_RATE -1");
        }


        // [NOTE_0]
        /*
    public Text timeDebugText0;
    public Text timeDebugText1;
    public Text timeDebugText2;
    public Text timeDebugText3;
    public Text timeDebugText4;


        timeDebugText0.text = "Current " + core.GetCurrentTime().ToString();
        timeDebugText1.text = "Tracker " + core.GetTrackerTime().ToString();
        timeDebugText2.text = "TrackerOffset " + core.GetTrackerTimeOffset().ToString();
        timeDebugText3.text = "TrackerUsec " + core.GetTrackerTimeUsec().ToString();
        timeDebugText4.text = "TrackerUsecOffset " + core.GetTrackerTimeUsecOffset().ToString();

        SubjectPerformanceReport.LogLevelData("EyeLinkManagerTime", "GetCurrentTime :: " + timeDebugText0.text);
        SubjectPerformanceReport.LogLevelData("EyeLinkManagerTime", "GetTrackerTime :: " + timeDebugText1.text);
        SubjectPerformanceReport.LogLevelData("EyeLinkManagerTime", "GetTrackerTimeOffset :: " + timeDebugText2.text);
        SubjectPerformanceReport.LogLevelData("EyeLinkManagerTime", "GetTrackerTimeUsec :: " + timeDebugText3.text);
        SubjectPerformanceReport.LogLevelData("EyeLinkManagerTime", "GetTrackerTimeUsecOffset :: " + timeDebugText4.text);
        */
#endif

        [Serializable]
        public new class Config : ImplementationConfig
        {
            public bool useSimulation = false;
            public Vector2Int validDataThreshold = new Vector2Int(-10000, 10000);

            public SampleRateCommand.SampleRate? GetSampleRate() { return sampleRate.GetSampleRate(); }

            public List<Command> GetAllCommands()
            {
                return new List<Command>()
                {
                    pixelCoordinates,
                    calibrationAreaProportion,
                    validationAreaProportion,
                    binocularEnabled,
                    enableAutomaticCalibration,
                    calibrationType,
                    parserConfigurationCommand,
                    parserOverride_saccadeVelocityThreshold,
                    parserOverride_saccadeAccelerationThreshold,
                    parserOverride_saccadeMotionThreshold,
                    parserOverride_saccadePursuitFixup,
                    parserOverride_fixationUpdateInterval,
                    parserOverride_fixationUpdateAccumulate,
                    fileEventFilter,
                    fileSampleData,
                    linkEventFilter,
                    linkSampleData,
                    sampleRate,
                    useEllipseFitter,
                    elclTtPower,
                    screenPhysicalCoordinates,
                    screenDistance,
                    remoteCameraPosition
                };
            }

            [SerializeField] private PixelCoordinatesCommand pixelCoordinates = new PixelCoordinatesCommand(true);
            [SerializeField] private CalibrationAreaProportionCommand calibrationAreaProportion = new CalibrationAreaProportionCommand(true);
            [SerializeField] private ValidationAreaProportionCommand validationAreaProportion = new ValidationAreaProportionCommand(true);
            [SerializeField] private BinocularEnabledCommand binocularEnabled = new BinocularEnabledCommand(true);
            [SerializeField] private EnableAutomaticCalibrationCommand enableAutomaticCalibration = new EnableAutomaticCalibrationCommand(true);
            [SerializeField] private CalibrationTypeCommand calibrationType = new CalibrationTypeCommand(true);
            [SerializeField] private SelectParserConfigurationCommand parserConfigurationCommand = new SelectParserConfigurationCommand(true);
            [SerializeField] private SaccadeVelocityThresholdCommand parserOverride_saccadeVelocityThreshold = new SaccadeVelocityThresholdCommand(false);
            [SerializeField] private SaccadeAccelerationThresholdCommand parserOverride_saccadeAccelerationThreshold = new SaccadeAccelerationThresholdCommand(false);
            [SerializeField] private SaccadeMotionThresholdCommand parserOverride_saccadeMotionThreshold = new SaccadeMotionThresholdCommand(false);
            [SerializeField] private SaccadePursuitFixupCommand parserOverride_saccadePursuitFixup = new SaccadePursuitFixupCommand(false);
            [SerializeField] private FixationUpdateIntervalCommand parserOverride_fixationUpdateInterval = new FixationUpdateIntervalCommand(false);
            [SerializeField] private FixationUpdateAccumulateCommand parserOverride_fixationUpdateAccumulate = new FixationUpdateAccumulateCommand(false);
            [SerializeField] private FileEventFilterCommand fileEventFilter = new FileEventFilterCommand(true);
            [SerializeField] private FileSampleDataCommand fileSampleData = new FileSampleDataCommand(true);
            [SerializeField] private LinkEventFilterCommand linkEventFilter = new LinkEventFilterCommand(true);
            [SerializeField] private LinkSampleDataCommand linkSampleData = new LinkSampleDataCommand(true);
            [SerializeField] private SampleRateCommand sampleRate = new SampleRateCommand(true);
            [SerializeField] private UseEllipseFitterCommand useEllipseFitter = new UseEllipseFitterCommand(true);
            [SerializeField] private ElclTtPowerCommand elclTtPower = new ElclTtPowerCommand(true);
            [SerializeField] private ScreenPhysicalCoordinatesCommand screenPhysicalCoordinates = new ScreenPhysicalCoordinatesCommand(true);
            [SerializeField] private ScreenDistanceCommand screenDistance = new ScreenDistanceCommand(true);
            [SerializeField] private RemoteCameraPositionCommand remoteCameraPosition = new RemoteCameraPositionCommand(true);

            public string extraCommands_Comment = "These are sent 'as-is' to the EyeLink. Use carefully. Empty commands are ignored.";
            public bool sendExtraCommands = false;
            public string[] extraCommands = { "", "" };
            public string sendCloseEyelinkSystemCommandAtExeShutdown_Comment = "Sends the close_eyelink_system command to eyelink when the game exits for whatever reason";
            public bool sendCloseEyelinkSystemCommandAtExeShutdown = true;

            [Serializable]
            public class Command
            {
                public bool enabled;
                [SerializeField] private string comment;

                public Command(bool enabled, string comment)
                {
                    this.enabled = enabled;
                    this.comment = comment;
                }

                public string GetCommand() { return "{0} = {1}"._Format(commandBody, GetArgumentsFormatted()); }
                private string GetArgumentsFormatted()
                {
                    string s = "";
                    foreach (object o in GetArguments())
                        s += o + " ";

                    return s.Substring(0, s.Length - 1); // drop the last " "
                }
                protected virtual List<object> GetArguments() { return null; }
                protected virtual string commandBody { get; }
            }

            [Serializable]
            public class PixelCoordinatesCommand : Command
            {
                [SerializeField] private Vector2 bottomLeft01 = new Vector2(0, 0);
                [SerializeField] private Vector2 topRight01 = new Vector2(1, 1);

                public PixelCoordinatesCommand(bool enabled)
                    : base(enabled, "screen_pixel_coords = 0 0 1 1 | defined in the 01 space, x gets multiplied by Screen.width, y by Screen.height")
                { }

                protected override string commandBody => "screen_pixel_coords";

                protected override List<object> GetArguments()
                {
                    return new List<object>() {
                        bottomLeft01.x * Screen.width, bottomLeft01.y * Screen.height,
                        topRight01.x * Screen.width - 1, topRight01.y * Screen.height - 1 };
                }
            }

            [Serializable]
            public class CalibrationAreaProportionCommand : Command
            {
                [SerializeField] private Vector2 limits01 = new Vector2(0.88f, 0.83f);

                public CalibrationAreaProportionCommand(bool enabled)
                    : base(enabled, "'calibration_area_proportion = 0.88 0.83' % default: 0.88 0.83")
                { }

                protected override string commandBody => "calibration_area_proportion";

                protected override List<object> GetArguments()
                {
                    return new List<object>() { limits01.x, limits01.y };
                }
            }

            [Serializable]
            public class ValidationAreaProportionCommand : Command
            {
                [SerializeField] private Vector2 limits01 = new Vector2(0.88f, 0.83f);

                public ValidationAreaProportionCommand(bool enabled)
                    : base(enabled, "'validation_area_proportion = 0.88 0.83' % default: 0.88 0.83")
                { }

                protected override string commandBody => "validation_area_proportion";

                protected override List<object> GetArguments()
                {
                    return new List<object>() { limits01.x, limits01.y };
                }
            }

            [Serializable]
            public class BinocularEnabledCommand : Command
            {
                [SerializeField] private bool binocularEnabled = false;

                public BinocularEnabledCommand(bool enabled)
                    : base(enabled, "'binocular_enabled = true / false")
                { }

                protected override string commandBody => "binocular_enabled";

                protected override List<object> GetArguments()
                {
                    return new List<object>() { binocularEnabled ? "YES" : "NO" };
                }
            }

            [Serializable]
            public class EnableAutomaticCalibrationCommand : Command
            {
                [SerializeField] private bool automaticCalibrationEnabled = true;

                public EnableAutomaticCalibrationCommand(bool enabled)
                    : base(enabled, "'enable_automatic_calibration = YES' % OR NO")
                { }

                protected override string commandBody => "enable_automatic_calibration";

                protected override List<object> GetArguments()
                {
                    return new List<object>() { automaticCalibrationEnabled ? "YES" : "NO" };
                }
            }

            [Serializable]
            public class CalibrationTypeCommand : Command
            {
                [SerializeField] private CalibrationType calibrationType = CalibrationType.HV9;

                protected override string commandBody => "calibration_type";

                public CalibrationTypeCommand(bool enabled)
                    : base(enabled, "'calibration_type = HV13' % OR HV5 OR HV9, number of points for calibration")
                { }

                protected override List<object> GetArguments()
                {
                    return new List<object>() { calibrationType };
                }

                public enum CalibrationType { HV5 = 5, HV9 = 9, HV13 = 13 }
            }

            [Serializable]
            public class SaccadeVelocityThresholdCommand : Command
            {
                [SerializeField] private int threshold = 22;

                public SaccadeVelocityThresholdCommand(bool enabled)
                    : base(enabled, "'saccade_velocity_threshold = 22’ % Setting the velocity threshold for saccade")
                { }

                protected override string commandBody => "saccade_velocity_threshold";

                protected override List<object> GetArguments()
                {
                    return new List<object>() { threshold };
                }
            }

            [Serializable]
            public class SelectParserConfigurationCommand : Command
            {
                [SerializeField] private SelectParserConfiguration parserConfiguration = SelectParserConfiguration.Standard;

                protected override string commandBody => "select_parser_configuration";

                public SelectParserConfigurationCommand(bool enabled)
                    : base(enabled, "'select_parser_configuration = <set> | <set>: 0 for standard, 1 for high sensitivity saccade detector configuration |")
                { }

                protected override List<object> GetArguments()
                {
                    return new List<object>() { (int)parserConfiguration };
                }

                public enum SelectParserConfiguration { Standard = 0, Sensitive = 1 }
            }

            [Serializable]
            public class SaccadeAccelerationThresholdCommand : Command
            {
                [SerializeField] private int threshold = 3800;

                public SaccadeAccelerationThresholdCommand(bool enabled)
                    : base(enabled, "'saccade_acceleration_threshold = 3800 | Setting the acceleration threshold for saccade |")
                { }

                protected override string commandBody => "saccade_acceleration_threshold";

                protected override List<object> GetArguments()
                {
                    return new List<object>() { threshold };
                }
            }

            [Serializable]
            public class SaccadeMotionThresholdCommand : Command
            {
                [SerializeField] private float threshold = 0.0f;

                public SaccadeMotionThresholdCommand(bool enabled)
                    : base(enabled, "'saccade_motion_threshold=0.0 | Setting the motion threshold for saccade |")
                { }

                protected override string commandBody => "saccade_motion_threshold";

                protected override List<object> GetArguments()
                {
                    return new List<object>() { threshold };
                }
            }

            [Serializable]
            public class SaccadePursuitFixupCommand : Command
            {
                [SerializeField] private int fixup = 60;

                public SaccadePursuitFixupCommand(bool enabled)
                    : base(enabled, "'saccade_pursuit_fixup=60 | Setting the pursuit dixup for saccade |")
                { }

                protected override string commandBody => "saccade_pursuit_fixup";

                protected override List<object> GetArguments()
                {
                    return new List<object>() { fixup };
                }
            }

            [Serializable]
            public class FixationUpdateIntervalCommand : Command
            {
                [SerializeField] private int interval = 0;

                public FixationUpdateIntervalCommand(bool enabled)
                    : base(enabled, "'fixation_update_interval=0 | Setting the fixation update interval |")
                { }

                protected override string commandBody => "fixation_update_interval";

                protected override List<object> GetArguments()
                {
                    return new List<object>() { interval };
                }
            }

            [Serializable]
            public class FixationUpdateAccumulateCommand : Command
            {
                [SerializeField] private int accumulate = 0;

                public FixationUpdateAccumulateCommand(bool enabled)
                    : base(enabled, "'fixation_update_accumulate=0 | Setting the fixation update accumulate |")
                { }

                protected override string commandBody => "fixation_update_accumulate";

                protected override List<object> GetArguments()
                {
                    return new List<object>() { accumulate };
                }
            }

            [Serializable]
            public class FileEventFilterCommand : Command
            {
                [SerializeField] private bool leftEnabled = true;
                [SerializeField] private bool rightEnabled = true;
                [SerializeField] private bool fixationEnabled = true;
                [SerializeField] private bool saccadeEnabled = true;
                [SerializeField] private bool blinkEnabled = true;
                [SerializeField] private bool messageEnabled = true;
                [SerializeField] private bool buttonEnabled = true;
                [SerializeField] private bool inputEnabled = true;

                public FileEventFilterCommand(bool enabled)
                    : base(enabled, "'file_event_filter = LEFT,RIGHT,FIXATION,SACCADE,BLINK,MESSAGE,BUTTON,INPUT'")
                { }

                protected override string commandBody => "file_event_filter";

                protected override List<object> GetArguments()
                {
                    string separator = ",";
                    string arguments = "";

                    if (leftEnabled) arguments += "LEFT" + separator;
                    if (rightEnabled) arguments += "RIGHT" + separator;
                    if (fixationEnabled) arguments += "FIXATION" + separator;
                    if (saccadeEnabled) arguments += "SACCADE" + separator;
                    if (blinkEnabled) arguments += "BLINK" + separator;
                    if (messageEnabled) arguments += "MESSAGE" + separator;
                    if (buttonEnabled) arguments += "BUTTON" + separator;
                    if (inputEnabled) arguments += "INPUT" + separator;

                    if (arguments.Length >= separator.Length)
                        arguments = arguments.Substring(0, arguments.Length - separator.Length); // Remove last separator

                    return new List<object>() { arguments };
                }
            }

            [Serializable]
            public class FileSampleDataCommand : Command
            {
                [SerializeField] private bool leftEnabled = true;
                [SerializeField] private bool rightEnabled = true;
                [SerializeField] private bool gazeEnabled = true;
                [SerializeField] private bool hrefEnabled = true;
                [SerializeField] private bool rawEnabled = true;
                [SerializeField] private bool areaEnabled = true;
                [SerializeField] private bool htargetEnabled = true;
                [SerializeField] private bool gazeresEnabled = true;
                [SerializeField] private bool buttonEnabled = true;
                [SerializeField] private bool statusEnabled = true;
                [SerializeField] private bool inputEnabled = true;

                public FileSampleDataCommand(bool enabled)
                    : base(enabled, "‘file_sample_data = LEFT,RIGHT,GAZE,HREF,RAW,AREA,HTARGET,GAZERES,BUTTON,STATUS,INPUT'")
                { }

                protected override string commandBody => "file_sample_data";

                protected override List<object> GetArguments()
                {
                    string separator = ",";
                    string arguments = "";

                    if (leftEnabled) arguments += "LEFT" + separator;
                    if (rightEnabled) arguments += "RIGHT" + separator;
                    if (gazeEnabled) arguments += "GAZE" + separator;
                    if (hrefEnabled) arguments += "HREF" + separator;
                    if (rawEnabled) arguments += "RAW" + separator;
                    if (areaEnabled) arguments += "AREA" + separator;
                    if (htargetEnabled) arguments += "HTARGET" + separator;
                    if (gazeresEnabled) arguments += "GAZERES" + separator;
                    if (buttonEnabled) arguments += "BUTTON" + separator;
                    if (statusEnabled) arguments += "STATUS" + separator;
                    if (inputEnabled) arguments += "INPUT" + separator;

                    if (arguments.Length >= separator.Length)
                        arguments = arguments.Substring(0, arguments.Length - separator.Length); // Remove last separator

                    return new List<object>() { arguments };
                }
            }

            [Serializable]
            public class LinkSampleDataCommand : Command
            {
                [SerializeField] private bool leftEnabled = true;
                [SerializeField] private bool rightEnabled = true;
                [SerializeField] private bool gazeEnabled = true;
                [SerializeField] private bool gazeresEnabled = true;
                [SerializeField] private bool areaEnabled = true;
                [SerializeField] private bool htargetEnabled = true;
                [SerializeField] private bool statusEnabled = true;
                [SerializeField] private bool inputEnabled = true;

                public LinkSampleDataCommand(bool enabled)
                    : base(enabled, "link_sample_data = LEFT,RIGHT,GAZE,GAZERES,AREA,HTARGET,STATUS,INPUT")
                { }

                protected override string commandBody => "link_sample_data";

                protected override List<object> GetArguments()
                {
                    string separator = ",";
                    string arguments = "";

                    if (leftEnabled) arguments += "LEFT" + separator;
                    if (rightEnabled) arguments += "RIGHT" + separator;
                    if (gazeEnabled) arguments += "GAZE" + separator;
                    if (gazeresEnabled) arguments += "GAZERES" + separator;
                    if (areaEnabled) arguments += "AREA" + separator;
                    if (htargetEnabled) arguments += "HTARGET" + separator;
                    if (statusEnabled) arguments += "STATUS" + separator;
                    if (inputEnabled) arguments += "INPUT" + separator;

                    if (arguments.Length >= separator.Length)
                        arguments = arguments.Substring(0, arguments.Length - separator.Length); // Remove last separator

                    return new List<object>() { arguments };
                }
            }
            [Serializable]
            public class LinkEventFilterCommand : Command
            {
                [SerializeField] private bool leftEnabled = true;
                [SerializeField] private bool rightEnabled = true;
                [SerializeField] private bool fixationEnabled = true;
                [SerializeField] private bool saccadeEnabled = true;
                [SerializeField] private bool blinkEnabled = true;
                [SerializeField] private bool messageEnabled = true;
                [SerializeField] private bool buttonEnabled = true;
                [SerializeField] private bool fixupdateEnabled = true;
                [SerializeField] private bool inputEnabled = true;

                public LinkEventFilterCommand(bool enabled)
                    : base(enabled, "link_event_filter = LEFT,RIGHT,FIXATION,SACCADE,BLINK,MESSAGE,BUTTON,FIXUPDATE,INPUT'")
                { }

                protected override string commandBody => "link_event_filter";

                protected override List<object> GetArguments()
                {
                    string separator = ",";
                    string arguments = "";

                    if (leftEnabled) arguments += "LEFT" + separator;
                    if (rightEnabled) arguments += "RIGHT" + separator;
                    if (fixationEnabled) arguments += "FIXATION" + separator;
                    if (saccadeEnabled) arguments += "SACCADE" + separator;
                    if (blinkEnabled) arguments += "BLINK" + separator;
                    if (messageEnabled) arguments += "MESSAGE" + separator;
                    if (buttonEnabled) arguments += "BUTTON" + separator;
                    if (fixupdateEnabled) arguments += "FIXUPDATE" + separator;
                    if (inputEnabled) arguments += "INPUT" + separator;

                    if (arguments.Length >= separator.Length)
                        arguments = arguments.Substring(0, arguments.Length - separator.Length); // Remove last separator

                    return new List<object>() { arguments };
                }
            }
            [Serializable]
            public class SampleRateCommand : Command
            {
                [SerializeField] private SampleRate sampleRate = SampleRate.SR500;

                protected override string commandBody => "sample_rate";

                public SampleRateCommand(bool enabled)
                    : base(enabled, "'sample_rate = 500 % OR 1000 | Setting the sample rate |")
                { }

                protected override List<object> GetArguments()
                {
                    return new List<object>() { (int)sampleRate };
                }

                public SampleRate? GetSampleRate()
                {
                    return enabled ? (SampleRate?) sampleRate : null;
                }

                public enum SampleRate { SR500 = 500, SR1000 = 1000 }
            }

            [Serializable]
            public class UseEllipseFitterCommand : Command
            {
                [SerializeField] private bool useEllipseFitter = false;

                public UseEllipseFitterCommand(bool enabled)
                    : base(enabled, "'use_ellipse_fitter = true / false'")
                { }

                protected override string commandBody => "use_ellipse_fitter";

                protected override List<object> GetArguments()
                {
                    return new List<object>() { useEllipseFitter ? "YES" : "NO" };
                }
            }

            [Serializable]
            public class ElclTtPowerCommand : Command
            {
                [SerializeField] private ElclTtPower elclTtPowerPercentage = ElclTtPower.Three_Quarters;

                public ElclTtPowerCommand(bool enabled)
                    : base(enabled, "'elcl_tt_power = 2' % 1 is 100% illumination, 2 is 75% and 3 50%")
                { }

                protected override string commandBody => "elcl_tt_power";

                protected override List<object> GetArguments()
                {
                    return new List<object>() { (int)elclTtPowerPercentage };
                }

                public enum ElclTtPower { Full = 1, Three_Quarters = 2, Half = 3 }
            }

            [Serializable]
            public class ScreenPhysicalCoordinatesCommand : Command
            {
                [SerializeField] private int left = -265;
                [SerializeField] private int top = 150;
                [SerializeField] private int right = 265;
                [SerializeField] private int bottom = -150;

                public ScreenPhysicalCoordinatesCommand(bool enabled)
                    : base(enabled, "'screen_phys_coords = left, top, right, bottom' | to set the screen size in mm, MEASURED FROM THE CENTER OF THE SCREEN")
                { }

                protected override string commandBody => "screen_phys_coords";

                protected override List<object> GetArguments()
                {
                    return new List<object> { left, top, right, bottom };
                }
            }

            [Serializable]
            public class ScreenDistanceCommand : Command
            {
                [SerializeField] private float top = 736.6f;
                [SerializeField] private float bottom = 744.2f;

                public ScreenDistanceCommand(bool enabled)
                    : base(enabled, "'screen_distance= top, bottom % Distance between participants eyes and top and bottom of screen IN MM")
                { }

                protected override string commandBody => "screen_distance";

                protected override List<object> GetArguments()
                {
                    return new List<object>() { top, bottom };
                }
            }

            [Serializable]
            public class RemoteCameraPositionCommand : Command
            {
                private int int1 = -10;
                private int int2 = 17;
                private int int3 = 80;
                private int int4 = 60;
                [SerializeField] private int int5 = -90;

                public RemoteCameraPositionCommand(bool enabled)
                    : base(enabled, "‘remote_camera_position = -10 17 80 60 –90’) // int5 corresponds to the last argument (-90) | to change the first 4 arguments use the ExtraCommands feature | % Distance between the screen and the tracker IN MM, important for remote mode only")
                { }

                protected override string commandBody => "remote_camera_position";

                protected override List<object> GetArguments()
                {
                    return new List<object>() { int1, int2, int3, int4, int5 };
                }
            }
        }
    }
}

namespace Peripherals.EyeTracking.EyeLink
{
    public static class EyeTrackerManager_EyeLink_Helper
    {
        public static bool IsGazeValid(this Core.XY gaze, Vector2 threshold)
        {
            if (!gaze.X.IsBetween(threshold)) return false;
            if (!gaze.Y.IsBetween(threshold)) return false;

            return true;
        }

        public static string FromBase(this IList<Vector2> calibrationData_Base_Normalized)
        {
            string calibrationData_EyeLink = "";

            // "960,600 960,400";
            foreach (Vector2 cD_B_Normalized in calibrationData_Base_Normalized)
            {
                Vector2Int cD_B_Scren = new Vector2Int(
                    Mathf.RoundToInt(cD_B_Normalized.x * Screen.width),
                    Mathf.RoundToInt(cD_B_Normalized.y * Screen.height));
                calibrationData_EyeLink += "{0},{1} "._Format(cD_B_Scren.x, cD_B_Scren.y);
            }

            calibrationData_EyeLink = calibrationData_EyeLink.RemoveLast(1);

            return calibrationData_EyeLink;
        }

        public static Sample ToBase(this Core.Sample sample_EyeLink, Vector2 validDataThreshold)
        {
            Sample sample_Base = new Sample();

            if (sample_EyeLink.left.IsGazeValid(validDataThreshold))
                sample_Base.leftEyePosition = new Vector2(
                    sample_EyeLink.left.X, Screen.height - sample_EyeLink.left.Y);
            else
                sample_Base.leftEyePosition = null;

            if (sample_EyeLink.right.IsGazeValid(validDataThreshold))
                sample_Base.rightEyePosition = new Vector2(
                    sample_EyeLink.right.X, Screen.height - sample_EyeLink.right.Y);
            else
                sample_Base.rightEyePosition = null;

            sample_Base.timestampMS = sample_EyeLink.time;
              
            return sample_Base;
        }

        public static Blink ToBase(this Core.Blink blink_EyeLink)
        {
            Blink blink_Base = new Blink();

            blink_Base.eye = blink_EyeLink.eye.ToBase();
            blink_Base.startTimestampMS = blink_EyeLink.startTime;
            blink_Base.endTimestampMS = blink_EyeLink.endTime;

            return blink_Base;
        }

        public static Sacada ToBase(this Core.Sacada sacada_EyeLink)
        {
            Sacada sacada_Base = new Sacada();

            sacada_Base.start = sacada_EyeLink.Start.ToBase();
            sacada_Base.end = sacada_EyeLink.End.ToBase();

            return sacada_Base;
        }

        public static TimestampedPosition ToBase(this Core.XYT tsPos_EyeLink)
        {
            TimestampedPosition tsPos_Base = new TimestampedPosition();

            tsPos_Base.position.x = tsPos_EyeLink.X;
            tsPos_Base.position.y = Screen.height - tsPos_EyeLink.Y;
            tsPos_Base.timestampMS = tsPos_EyeLink.time;

            return tsPos_Base;
        }

        public static Vector2 ToBase(this Core.XY pos_EyeLink)
        {
            Vector2 pos_Base = Vector2.zero;

            pos_Base.x = pos_EyeLink.X;
            pos_Base.y = Screen.height - pos_EyeLink.Y;

            return pos_Base;
        }

        public static Eye ToBase(this Core.EYE eye_EyeLink)
        {
            switch (eye_EyeLink)
            {
                case Core.EYE.NONE:
                    return Eye.None;
                case Core.EYE.LEFT:
                    return Eye.Left;
                case Core.EYE.RIGHT:
                    return Eye.Right;
                case Core.EYE.BINOCULAR:
                    return Eye.Binocular;
            }

            return Eye.None;
        }
    }
}