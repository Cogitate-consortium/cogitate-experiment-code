using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace Helpers.Misc.Test
{
    public class HeatmapGenerator_Test : MonoBehaviour
    {
        [Header(".Net4 Supports Parallel.For")]
        public bool useParallelFor = true;

        [Header("Heatmap Settings")]
        public Color baseColor = Color.blue;
        public Color heatColor = Color.red;
        public int heatSize = 100;
        public string filePath = "C:/heatmap.png";
        public Vector2Int screenSize = new Vector2Int(1920, 1080);

        [Header("Debug")]
        public RawImage debugImage;

        [Header("Random Generator")]
        public int randomGeneratePoints = 1000;
        public bool doGenerateRandom = false;

        [Header("Generate from file (Gazes)")]
        public string fileToLoad_Gazes = "/../Logs/heatmap.json";
        public bool doGenerateFromFile_Gazes = false;

        [Header("Generate from file (Sacades)")]
        public string fileToLoad_Sacades = "/../Logs/heatmap.json";
        public bool doGenerateFromFile_Sacades = false;

        private void Start()
        {
            //CreateRandomGeneratedHeatmap();
        }

        private void Update()
        {
            if (doGenerateRandom)
            {
                doGenerateRandom = false;
                CreateRandomGeneratedHeatmap();
            }

            if (doGenerateFromFile_Gazes)
            {
                doGenerateFromFile_Gazes = false;
                CreateHeatmapFromFile_Gazes();
            }

            if (doGenerateFromFile_Sacades)
            {
                doGenerateFromFile_Sacades = false;
                CreateHeatmapFromFile_Sacades();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                CreateRandomGeneratedHeatmap();
            }
        }

        private void CreateRandomGeneratedHeatmap()
        {
            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i < randomGeneratePoints; i++)
            {
                points.Add(new Vector2(Random.Range(0, 1920), Random.Range(0, 1080)));
            }
            HeatmapGenerator.GenerateAndLogHeatmap(screenSize, points, baseColor, heatColor, heatSize, filePath, useParallelFor, (tex) =>
            {
                debugImage.texture = tex;
            });

        }

        private void CreateHeatmapFromFile_Gazes()
        {
            string data = System.IO.File.ReadAllText(Application.dataPath + fileToLoad_Gazes);
            JsonArray<Vector2> jsonArray = JsonUtility.FromJson<JsonArray<Vector2>>(data);
            HeatmapGenerator.GenerateAndLogHeatmap(screenSize, jsonArray.array, baseColor, heatColor, heatSize, filePath, useParallelFor, (tex) =>
            {
                debugImage.texture = tex;
            });
        }

        private void CreateHeatmapFromFile_Sacades()
        {
            string data = System.IO.File.ReadAllText(Application.dataPath + fileToLoad_Sacades);
            JsonArray<Vector4> jsonArray = JsonUtility.FromJson<JsonArray<Vector4>>(data);
            HeatmapGenerator.GenerateAndLogHeatmap(screenSize, jsonArray.array, baseColor, heatColor, heatSize, filePath, useParallelFor, (tex) =>
            {
                debugImage.texture = tex;
            });
        }
    }
}