// NS_REMOVE
// using Peripherals.Logging.Test.Core;
using System;
using System.Collections.Generic;
using System.IO;
using TGP.Helpers;

namespace Peripherals.Logging.Core
{
    public class FileWrapper
    {
        public static void WriteToFile_TS(string path, string contents)
        {
            CreateDirectory_TS(path);
            File.WriteAllText(path, contents);
        }

        public static void WriteToFile(string path, byte[] bytes)
        {
            CreateDirectory_TS(path);
            File.WriteAllBytes(path, bytes);
        }

        public static void CopyFile(string source, string destination, bool overwrite)
        {
            CreateDirectory_TS(destination);
            // Debug.LogError("Copying ({0} overwrite) {1} -> {2}"._Format(overwrite ? "with" : "without", source, destination));
            File.Copy(source, destination, overwrite);
        }

        internal static void MoveFile(string source, string destination, bool overwrite)
        {
            File.Copy(source, destination, overwrite);
            DeleteFile(source);
        }

        internal static void DeleteFile(string source)
        {
            File.Delete(source);
        }

        public static void CopyFolder(string sourceDirectory, string targetDirectory, bool overwrite)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget, overwrite);
        }

        private static void CopyAll(DirectoryInfo source, DirectoryInfo target, bool overwrite)
        {
            Directory.CreateDirectory(target.FullName);

            try
            {
                // Copy each file into the new directory.
                foreach (FileInfo fi in source.GetFiles())
                {
                    if (fi.Name.Contains(".meta")) continue; // Skip Meta Files
                                                             // Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                    fi.CopyTo(Path.Combine(target.FullName, fi.Name), overwrite);
                }

                // Copy each subdirectory using recursion.
                foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
                {
                    DirectoryInfo nextTargetSubDir =
                        target.CreateSubdirectory(diSourceSubDir.Name);
                    CopyAll(diSourceSubDir, nextTargetSubDir, overwrite);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
            }
        }


        public static string ReadFromFile(string source)
        {
            if (!File.Exists(source))
            {
                Debug_Helper.LogError(typeof(FileWrapper), "Missing file at :: {0}"._Format(source));
                return "";
            }

            return File.ReadAllText(source);
        }

        // [TODO] Brute-forcing writing isn't ideal
        // But can't check if in use https://stackoverflow.com/questions/876473/is-there-a-way-to-check-if-a-file-is-in-use
        // Can't really write async to same file https://stackoverflow.com/questions/3507770/write-to-a-file-from-multiple-threads-asynchronously-c-sharp
        public static void AppendToFile_TS(string path, string contents, int maxNumAttempts = 10)
        {
            CreateDirectory_TS(path);

            /*
            int dataToBeWritten = 0;
            if (LoggingAnalyzer.config_doAnalytics)
            {
                dataToBeWritten = String_Helper.CountOccurences(contents, Environment.NewLine);
                LoggingAnalyzer.logTotalData_ToBeWritten += dataToBeWritten;
                LoggingAnalyzer.lastBatchToBeWritten = contents;
            }
            */
            // StreamWriter file = new StreamWriter(path);

            if (maxNumAttempts == 1)
            {
                File.AppendAllText(path, contents);
                /*
                if (LoggingAnalyzer.config_doAnalytics)
                {
                    LoggingAnalyzer.logTotalData_Written += dataToBeWritten;
                    LoggingAnalyzer.lastBatchToBeWritten = "";
                    LoggingAnalyzer.lastLineWritten = contents.Split(true, Environment.NewLine).GetLast();
                }
                */
                return;
            }

            if (maxNumAttempts <= 0) maxNumAttempts = int.MaxValue; // Infinite

            bool error = false;
            int attempts = 0;

            for (attempts = 0; attempts < maxNumAttempts; attempts++)
            {
                error = false;

                try
                {
                    // file.WriteLine(contents);
                    File.AppendAllText(path, contents);
                    // attempts == 0 ? contents :  (contents + " WROTE_AFTER_ATTEMPTS " + attempts));
                }
                catch
                {
                    error = true;
                }

                if (!error) break;
            }

            if (!error)
            {
                /*
                if (LoggingAnalyzer.config_doAnalytics)
                {
                    LoggingAnalyzer.logTotalData_Written += dataToBeWritten;
                    LoggingAnalyzer.lastBatchToBeWritten = "";
                    LoggingAnalyzer.lastLineWritten = contents.Split(true, Environment.NewLine).GetLast();
                }
                */
            }
            else
            {
                /*
                if (LoggingAnalyzer.config_doAnalytics)
                {
                    LoggingAnalyzer.lastBatchToBeWritten += " WRITING_ERROR";
                }
                */
            }
        }

        /// <summary>
        /// Returns null if path doesn't exist
        /// </summary>
        public static List<string> GetFolderContents(string path, FilesFolders filesFolders, bool returnFullPaths)
        {
            if (!FolderExists(path)) return null;

            List<string> contents = new List<string>();

            if (filesFolders == FilesFolders.Folders || filesFolders == FilesFolders.FilesAndFolders)
                contents.AddRange(Directory.GetDirectories(path));

            if (filesFolders == FilesFolders.Files || filesFolders == FilesFolders.FilesAndFolders)
                foreach (string filePath in Directory.GetFiles(path))
                {
                    // Ignore META files
                    if (filePath.EndsWith(".meta"))
                        continue;
                    contents.Add(filePath);
                }

            if (returnFullPaths)
                return contents;

            // Otherwise, Trim
            List<string> contentNames = new List<string>();
            foreach (string s in contents)
                contentNames.Add(s.Remove(0, path.Length));

            return contentNames;
        }

        public static bool FolderExists(string path)
        {
            return Directory.Exists(path);
        }

        public static bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public static void CreateDirectory_TS(string path)
        {
            string stripPath = Path.GetDirectoryName(path);
            if (!FolderExists(stripPath))
                Directory.CreateDirectory(stripPath);
        }

        internal static List<string> ReadAllLines(string filePath)
        {
            if (!FileExists(filePath))
                return null;

            List<string> allLines = new List<string>();
            StreamReader reader = new StreamReader(filePath);
            string line;

            while ((line = reader.ReadLine()) != null)
                allLines.Add(line);

            return allLines;
        }
    }
}