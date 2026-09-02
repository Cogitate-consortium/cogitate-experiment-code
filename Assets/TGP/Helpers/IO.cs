// NS_REMOVE !!
using Peripherals.Logging.Core;
// NS_REMOVE !!
using Helpers.Misc;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TGP
{
    namespace Helpers
    {
        public static class IO_Helper
        {
            private static string _DROPBOX_PATH = "";
            public static string DROPBOX_PATH
            {
                get
                {
                    if (_DROPBOX_PATH != "")
                        return _DROPBOX_PATH;

                    string infoPath = @"Dropbox\info.json";

                    string jsonPath = Path.Combine(Environment.GetEnvironmentVariable("LocalAppData"), infoPath);

                    if (!File.Exists(jsonPath)) jsonPath = Path.Combine(Environment.GetEnvironmentVariable("AppData"), infoPath);

                    if (!File.Exists(jsonPath)) { Debug_Helper.Log(typeof(IO_Helper), "Dropbox could not be found!", LogType.Exception); return ""; }

                    _DROPBOX_PATH = File.ReadAllText(jsonPath).Split('\"')[5].Replace(@"\\", "/");

                    Debug_Helper.Log(typeof(IO_Helper), "Dropbox path :: {0}"._Format(_DROPBOX_PATH));

                    return _DROPBOX_PATH;
                }
            }

            public static Dictionary<string, string> PrepFileCollection(string folder, string subFolderIdxSeparator, bool appendFilePrefix, bool debug, params string[] subFolders)
            {
                Dictionary<string, string> fileCollection = new Dictionary<string, string>();
                if (folder.EndsWith("/"))
                    folder = folder.RemoveLast();
                foreach (string subFolder in subFolders)
                {
                    string subFolderPath = "{0}/{1}"._Format(folder, subFolder);

                    if (!Directory.Exists(subFolderPath))
                    {
                        if (debug)
                            Debug_Helper.Log(typeof(IO_Helper), "Directory doesn't exist :: {0}"._Format(subFolderPath), LogType.Warning);
                        continue;
                    }

                    int i = 0;
                    foreach (string fileName in Directory.GetFiles(subFolderPath))
                    {
                        string newKey = "{0}{1}{2}"._Format(subFolder, subFolderIdxSeparator, i);
                        fileCollection.Add(newKey, "{0}{1}"._Format(appendFilePrefix ? "file://" : "", fileName));
                        i++;
                    }
                }

                return fileCollection;
            }

            public static Texture2D LoadFromPath(this Texture2D Texture, string Path)
            {
                if (File.Exists(Path))
                {
                    byte[] rawData = File.ReadAllBytes(Path);

                    //Put loaded bytes to Texture2D 
                    Texture.LoadRawTextureData(rawData);
                    Texture.Apply();
                }
                return Texture;
            }

            public static void Reset<T>(T defaultValue)
            {

                string serializedInfo = defaultValue.XmlSerializeToString();
                PlayerPrefs.SetString("SaveInfo", serializedInfo);

            }

            public static T LoadFromPrefs<T>(T defaultValue)
            {
                // First time
                if (!PlayerPrefs.HasKey("SaveInfo"))
                {
                    PlayerPrefs.SetString("SaveInfo", defaultValue.XmlSerializeToString());
                    return defaultValue;
                }

                string serializedInfo = PlayerPrefs.GetString("SaveInfo");

                return serializedInfo.XmlDeserializeFromString<T>();
            }

            public static void SaveToPrefs<T>(this T saveInfo)
            {
                string serializedInfo = saveInfo.XmlSerializeToString();
                PlayerPrefs.SetString("SaveInfo", serializedInfo);
            }

            public static string XmlSerializeToString(this object objectInstance)
            {
                var serializer = new XmlSerializer(objectInstance.GetType());
                var sb = new StringBuilder();

                using (TextWriter writer = new StringWriter(sb))
                {
                    serializer.Serialize(writer, objectInstance);
                }

                return sb.ToString();
            }

            public static void WriteToPNG(Color[] texPixels, Vector2Int size, string filepath)
            {
                Texture2D texture = new Texture2D(size.x, size.y);
                texture.SetPixels(texPixels);
                texture.Apply();
                WriteToPNG(texture, filepath);
            }

            public static void WriteToPNG(Texture2D texture, string filepath)
            {
                try
                {
                    // Save it to file
                    FileWrapper.WriteToFile(filepath, texture.EncodeToPNG());
                }
                catch (Exception ex)
                {
                    Debug_Helper.LogException(typeof(HeatmapGenerator), ex);
                }

            }
            /*
             * 
        // Parse once to deduce dimensions
        int[] rowWidths = new int[numRows];
        int[] columnHeights = new int[numColumns];
        Color[][][][] pixels = new Color[numRows][][][];

        for (int i = 0; i < numRows; i++)
        {
            pixels[i] = new Color[numColumns][][];
            for (int j = 0; j < numColumns; j++)
            {
                int idx = j * numRows + i;
                if (idx >= sprites.Count)
                {
                    Debug_Helper.LogWarning(typeof(StimulusManager), "Reached end of Sprite list at element {0} ({1}, {2})"._Format(idx, i, j));
                    break;
                }

                Texture2D temp = sprites[i].texture;

                // Iterate the sprite's pixels, get the first row
                int width = temp.width;
                int height = temp.height;

                rowWidths[i] += width;
                columnHeights[j] += height;

                pixels[i][j] = new Color[height][];

                // For each row
                for (int p_y = 0; p_y < height; p_y++)
                {
                    // Create the respective pixel entry
                    pixels[i][j][p_y] = new Color[width];

                    // Get all the column
                    for (int p_x = 0; p_x < width; p_x++)
                        pixels[i][j][p_y][p_x] = temp.GetPixel(p_x, p_y);
                }
            }
        }

        // Figure out final dimensions
        int maxRowWidth = rowWidths.Max();
        int maxHeight = columnHeights.Max();
        Vector2Int size = new Vector2Int(maxRowWidth, maxHeight);

        Color[] pixelsFinal = new Color[maxRowWidth * maxHeight];

        // Iterate each row of Sprites (1 row = many sprites)
        for (int i = 0; i < pixels.Length; i ++)
        {
            int offsetRow = maxRowWidth * i;

            // Iterate the columns (1 column = 1 Sprite) of that row
            for (int j = 0; j < pixels[i].Length; j++)
            {
                int offsetColumn = maxHeight;
                for (int p_x = 0)
            }
            
        }
*/
            public static Texture2D ReadPNGFromFile(string fullPath)
            {
                if (!File.Exists(fullPath)) return null;

                byte[] rawData = File.ReadAllBytes(fullPath);
                if (rawData == null) return null;

                // Tex size does NOT matter - will be replaced in the next line
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(rawData);
                return tex;
            }

            public static T XmlDeserializeFromString<T>(this string objectData)
            {
                return (T)XmlDeserializeFromString(objectData, typeof(T));
            }

            public static object XmlDeserializeFromString(this string objectData, Type type)
            {
                var serializer = new XmlSerializer(type);
                object result = null;

                using (TextReader reader = new StringReader(objectData))
                {
                    try
                    {
                        result = serializer.Deserialize(reader);
                    }
                    catch (Exception e)
                    {
                        Debug_Helper.Log(typeof(IO_Helper), e.ToString(), LogType.Warning);
                    }
                }

                return result;
            }

            public static void XmlSaveToAssets<T>(this T item, string folder, string fileName, bool overwrite) where T : class
            {
                if (item == null) return;

                if (!fileName.ContainsInvariant(".xml"))
                    fileName += ".xml";
                if (folder[folder.Length - 1] != '/')
                    folder += "/";
                string filePath = Application.dataPath + "/" + folder + fileName;

                item.XmlSaveToFile(filePath, overwrite);

#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
            }

            public static void XmlSaveToFile<T>(this T item, string fullPath, bool overwrite) where T : class
            {
                if (item == null) return;

                string data = item.XmlSerializeToString();

                if (File.Exists(fullPath))
                {
                    if (!overwrite)
                    {
                        Debug_Helper.Log(typeof(IO_Helper), "File already existed and not directed to overwrite! {0}".
                            _Format(fullPath), LogType.Exception);
                        return;
                    }
                }

                File.WriteAllText(fullPath, data);
            }

            public static T XmlLoadFromFile<T>(string filePath) where T : class
            {
                if (!filePath.ContainsInvariant(".xml"))
                    filePath += ".xml";

                // Check if file exists
                if (!File.Exists(filePath))
                    return null;

                // Read raw file text
                string _xml = File.ReadAllText(filePath);

                // XML Deserialize text to FeedbackAnalysis
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                StringReader reader = new StringReader(_xml.ToString());

                T loadedObject = serializer.Deserialize(reader) as T;
                reader.Close();

                return loadedObject;
            }

            public static void SaveData(this string data, string fullFilePath)
            {
                // Write the string array to a new file named "WriteLines.txt".
                using (StreamWriter outputFile = new StreamWriter(fullFilePath))
                {
                    outputFile.WriteLine(data);
                }
            }

            public static void SaveDataToAssets(this string data, string assetsFilePath)
            {
                data.SaveData(Application.dataPath + "/" + assetsFilePath);
            }

            public static T XmlLoadFromAssets<T>(string filePath) where T : class
            {
                return XmlLoadFromFile<T>("{0}/{1}"._Format(Application.dataPath, filePath));
            }
        }
    }
}
