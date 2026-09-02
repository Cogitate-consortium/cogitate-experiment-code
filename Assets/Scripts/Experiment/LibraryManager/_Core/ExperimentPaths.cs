namespace Experiment.Library.Core
{
    /// <summary>
    /// [SOS] <see cref="Initialize(string)"/> first
    /// </summary>
    public class ExperimentPaths
    {
        public static string contentDirectory;

        public static void Initialize(string contentDirectory)
        {
            ExperimentPaths.contentDirectory = contentDirectory;
        }

        public static string getAudioFolderPath()
        {
            return contentDirectory + "Audio/";
        }

        public static string stimuliDirectoryObjects
        {
            get { return contentDirectory + "Stimuli/Objects/"; }
        }

        public static string stimuliDirectoryFaces
        {
            get { return contentDirectory + "Stimuli/Faces/"; }
        }

        public static string stimuliDirectoryBlobFaces
        {
            get { return contentDirectory + "Stimuli/Blobs/Faces"; }
        }

        public static string stimuliDirectoryBlobsObjects
        {
            get { return contentDirectory + "Stimuli/Blobs/Objects"; }
        }

        public static string screenshotDirectory
        {
            get { return contentDirectory + "MainMenu/"; }
        }

        public static string GetTutorialInfoImagesDirectory(string systemSuffix)
        {
            return contentDirectory + "TutorialInfoImages/" + systemSuffix;
        }

        /*
        public static string replayFilePath
        {
            get { return logsDirectory + "replay.json"; }
        }

        public static string serialDumpLogFilePath
        {
            get { return string.Format("{0}{1}/SerialPortDump/dump.txt", logsDirectory, (ExperimentManagerSession.probeSummary == null) ? "" : ExperimentManagerSession.probeSummary.subjectId); }
        }

        public static string analysisDirectory
        {
            get { return string.Format("{0}{1}/Analysis/", logsDirectory, ExperimentManagerSession.probeSummary.subjectId); }
        }

        public static string audioDirectory
        {
            get { return contentDirectory + "Audio/"; }
        }
        */
    }
}