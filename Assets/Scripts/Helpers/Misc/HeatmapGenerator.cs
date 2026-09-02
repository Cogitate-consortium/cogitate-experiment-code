using System;
using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;
using Helpers.Async;
using Peripherals.Logging;
using Helpers.Engine;

namespace Helpers.Misc
{
    public class HeatmapGenerator
    {
        /// <summary>
        /// Create a heatmap based on input points and save it to file
        /// </summary>
        /// <param name="size">Size of points contained in (Screen size)</param>
        /// <param name="points">List of points to visualize</param>
        /// <param name="baseColor">Background heatmap color</param>
        /// <param name="heatColor">Overlay heatmap color</param>
        /// <param name="heatSize">Size in pixels for heat point</param>
        /// <param name="fileName">Filepath for output image - if BLANK it will just create the texture, not write it</param>
        /// <param name="useParallelFor">.Net4 Supports Parallel.For to speed up the process</param>
        public static void GenerateAndLogHeatmap(Vector2Int size, List<Vector2> points, Color baseColor, Color heatColor, int heatSize, string fileName, bool useParallelFor, Action<Texture2D> callback)
        {
            float timeStarted = TimeWrapper.realtimeSinceStartup_NotTS;

            bool isFinished = false;

            AsyncThread.RequestRunOnNewThread(() =>
            {
                try
                {
                // Create an empty map
                Color[][] mapPixels = GenerateBaseMap(size.x, size.y, baseColor);

                // Find max quantized density in screen
                float maxDensity = GetMaxDensity(size, points, heatSize);
                    float opacity = 1f / maxDensity;
                // Min 1%
                opacity = Mathf.Max(0.01f, opacity);

                // Test point
                //PaintOnPoint(in mapPixels, new Vector2(size.x / 2, size.y / 2), heatSize, 0.5f);

                // Pass map as reference
                if (!useParallelFor)
                    {
                        for (int i = 0; i < points.Count; i++)
                        {
                            PaintOnPoint(in mapPixels, points[i], heatColor, heatSize, opacity);
                        }
                    }
                    else
                    {
                        System.Threading.Tasks.Parallel.For(0, points.Count, (index) =>
                        {
                            PaintOnPoint(in mapPixels, points[index], heatColor, heatSize, opacity);
                        });
                    }

                // Convert 2D pixels to single array
                Color[] texPixels = Convert2DArrayTo1D(mapPixels);

                    AsyncThread.RunOnMainThread_ASAP_TS(() =>
                    {
                    // Create new Texture to convert it to an image
                    Texture2D texture = new Texture2D(size.x, size.y);
                        texture.SetPixels(texPixels);
                        texture.Apply();

                        try
                        {
                        // Save it to file (if we want to)
                        if (!fileName.IsNullOrEmpty())
                                LogWrapper.WriteToLogs(fileName, texture.EncodeToPNG());
                        }
                        catch (Exception ex)
                        {
                            Debug_Helper.LogException(typeof(HeatmapGenerator), ex);
                        }
                        finally
                        {
                        // Show/Debug map
                        //debugImage.texture = texture;

                        // [SOS] Do this after the try catch block
                        callback(texture);
                            isFinished = true;

                            Debug_Helper.LogWarning(typeof(HeatmapGenerator), string.Format("Generating heatmap took:{0}s", (TimeWrapper.realtimeSinceStartup_NotTS - timeStarted)));

                        }
                    });
                }
                catch (Exception ex)
                {
                    if (!isFinished)
                        callback(null);
                    isFinished = true;
                    Debug_Helper.LogException(typeof(HeatmapGenerator), ex);
                }
            });
        }


        /// <summary>
        /// Create a heatmap based on input points and save it to file
        /// </summary>
        /// <param name="size">Size of points contained in (Screen size)</param>
        /// <param name="points">List of points to visualize</param>
        /// <param name="baseColor">Background heatmap color</param>
        /// <param name="heatColor">Overlay heatmap color</param>
        /// <param name="heatSize">Size in pixels for heat point</param>
        /// <param name="fileName">Filepath for output image</param>
        /// <param name="useParallelFor">.Net4 Supports Parallel.For to speed up the process</param>
        public static void GenerateAndLogHeatmap(Vector2Int size, List<Vector4> points, Color baseColor, Color heatColor, int heatSize, string fileName, bool useParallelFor, Action<Texture2D> callback)
        {
            float timeStarted = TimeWrapper.realtimeSinceStartup_NotTS;
            bool isFinished = false;

            AsyncThread.RequestRunOnNewThread(() =>
            {
                try
                {
                // Create an empty map
                Color[][] mapPixels = GenerateBaseMap(size.x, size.y, baseColor);

                // Find max quantized density in screen
                float maxDensity = GetMaxDensity(size, points, heatSize);
                    float opacity = 1f / maxDensity;
                // Min 1%
                opacity = 1;// Mathf.Max(0.01f, opacity);

                // Test point
                //PaintOnPoint(in mapPixels, new Vector2(size.x / 2, size.y / 2), heatSize, 0.5f);

                // Pass map as reference
                if (!useParallelFor)
                    {
                        for (int i = 0; i < points.Count; i++)
                        {
                            PaintLine(in mapPixels, points[i], heatColor, heatSize, opacity);
                        }
                    }
                    else
                    {
                        System.Threading.Tasks.Parallel.For(0, points.Count, (index) =>
                        {
                            PaintLine(in mapPixels, points[index], heatColor, heatSize, opacity);
                        });
                    }

                // Convert 2D pixels to single array
                Color[] texPixels = Convert2DArrayTo1D(mapPixels);

                    AsyncThread.RunOnMainThread_ASAP_TS(() =>
                    {
                    // Create new Texture to convert it to an image
                    Texture2D texture = new Texture2D(size.x, size.y);
                        texture.SetPixels(texPixels);
                        texture.Apply();

                        try
                        {
                        // Save it to file
                        if (!fileName.IsNullOrEmpty())
                                LogWrapper.WriteToLogs(fileName, texture.EncodeToPNG());
                        }
                        catch (Exception ex)
                        {
                            Debug_Helper.LogException(typeof(HeatmapGenerator), ex);
                        }
                        finally
                        {
                        // Show/Debug map
                        //debugImage.texture = texture;

                        // [SOS] Do this after the try catch block
                        callback(texture);
                            isFinished = true;

                            Debug_Helper.LogWarning(typeof(HeatmapGenerator), string.Format("Generating heatmap took:{0}s", (TimeWrapper.realtimeSinceStartup_NotTS - timeStarted)));

                        }

                    // Show/Debug map
                    //debugImage.texture = texture;

                    Debug_Helper.Log(typeof(HeatmapGenerator), string.Format("Generating heatmap took:{0}s", (TimeWrapper.realtimeSinceStartup_NotTS - timeStarted)));
                    });
                }
                catch (Exception ex)
                {
                    if (!isFinished)
                        callback(null);
                    isFinished = true;

                    Debug_Helper.LogException(typeof(HeatmapGenerator), ex);
                }
            });
        }

        /// <summary>
        /// Quantize space and get max number of points (density)
        /// </summary>
        public static float GetMaxDensity(Vector2Int size, List<Vector4> points, int quantizeSize)
        {
            List<Vector2> _points = new List<Vector2>();
            foreach (Vector4 p in points)
            {
                _points.Add(new Vector2(p.x, p.y));
                _points.Add(new Vector2(p.z, p.w));
            }

            return GetMaxDensity(size, _points, quantizeSize);
        }

        /// <summary>
        /// Quantize space and get max number of points (density)
        /// </summary>
        public static float GetMaxDensity(Vector2Int size, List<Vector2> points, int quantizeSize)
        {
            // Quantize screen into blocks
            // Add one more block to fill the remainder of division
            int numX = size.x / quantizeSize + 1;
            int numY = size.y / quantizeSize + 1;

            int[][] densityBlocks = new int[numX][];
            // Initialize array
            for (int x = 0; x < numX; x++)
                densityBlocks[x] = new int[numY];

            // Find all points in which block they belong
            for (int i = 0; i < points.Count; i++)
            {
                int blockX = (int)points[i].x / quantizeSize;
                int blockY = (int)points[i].y / quantizeSize;
                // Out of screen coords
                if (blockX < 0 || blockY < 0 || blockX >= numX || blockY >= numY) continue;
                // Increase block density
                densityBlocks[blockX][blockY]++;
            }

            // Find max density
            int maxDensity = 0;
            for (int x = 0; x < numX; x++)
            {
                for (int y = 0; y < numY; y++)
                {
                    maxDensity = Mathf.Max(maxDensity, densityBlocks[x][y]);
                }
            }

            return maxDensity;
        }

        /// <summary>
        /// Paint Sphere on pixels. Given a point a size and opacity.
        /// </summary>
        public static void PaintOnPoint(in Color[][] pixels, Vector2 point, Color overlayColor, int size = 100, float opacity = 0.5f)
        {
            // Cache values
            float distanceWeight = 1f;
            Color mapColor;
            Color blendColor;

            // Get offset from center
            int offset = size / 2;
            for (int x = (int)point.x - offset; x < point.x + offset; x++)
            {
                // Protect borders
                if (x < 0 || x >= pixels.Length) continue;

                for (int y = (int)point.y - offset; y < point.y + offset; y++)
                {
                    // Protect borders
                    if (x < 0 || x >= pixels.Length) continue;
                    if (y < 0 || y >= pixels[x].Length) continue;

                    // Make it circle - Radial distance
                    float distance = Vector2.Distance(point, new Vector2(x, y));
                    if (distance > size / 2f) continue;

                    // Calculate paint weights based on distance
                    // Linear blending (0-1) from center towards out

                    distanceWeight = distance / (size / 2f);
                    // Revert it - Max weight on center
                    distanceWeight = 1f - distanceWeight;

                    // Get current Color
                    mapColor = pixels[x][y];
                    // Blend heat color
                    blendColor = Color.Lerp(mapColor, overlayColor, distanceWeight * opacity);
                    // Set new Color
                    pixels[x][y] = blendColor;
                }
            }
        }

        public static void PaintLine(in Color[][] pixels, Vector4 sacada, Color overlayColor, int width = 100, float opacity = 0.5f)
        {
            PaintLine(in pixels, (int)sacada.x, (int)sacada.y, (int)sacada.z, (int)sacada.w, overlayColor, width, opacity);
        }

        public static void PaintLine(in Color[][] pixels, Vector2Int start, Vector2Int end, Color overlayColor, int width = 100, float opacity = 0.5f)
        {
            PaintLine(in pixels, start.x, start.y, end.x, end.y, overlayColor, width, opacity);
        }



        /// <summary>
        /// Paint line between pixels
        /// https://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm
        /// 
        /// Potential improvement : https://en.wikipedia.org/wiki/Xiaolin_Wu%27s_line_algorithm
        /// </summary>
        public static void PaintLine(in Color[][] pixels, int x0, int y0, int x1, int y1, Color overlayColor, int width = 100, float opacity = 0.5f)
        {
            if (Mathf.Abs(y1 - y0) < Mathf.Abs(x1 - x0))
            {
                if (x0 > x1)
                    PaintLine_Low(pixels, x1, y1, x0, y0, overlayColor, width, opacity);
                else
                    PaintLine_Low(pixels, x0, y0, x1, y1, overlayColor, width, opacity);
            }
            else
            {
                if (y0 > y1)
                    PaintLine_High(pixels, x1, y1, x0, y0, overlayColor, width, opacity);
                else
                    PaintLine_High(pixels, x0, y0, x1, y1, overlayColor, width, opacity);
            }
        }

        private static void PaintLine_Low(in Color[][] pixels, int x0, int y0, int x1, int y1, Color overlayColor, int width = 100, float opacity = 0.5f)
        {
            // Cache values
            Color mapColor;
            Color blendColor;

            // Get offset from center
            int offset = width / 2;
            int dX = x1 - x0;
            int dY = y1 - y0;
            int yi = 1;
            if (dY < 0)
            {
                yi = -1;
                dY = -dY;
            }
            int D = 2 * dY - dX;
            int y = y0;

            for (int x = x0; x <= x1; x++)
            {
                // Protect borders
                if (x < 0 || x >= pixels.Length) continue;
                if (y < 0 || y >= pixels[x].Length) continue;

                // Get current Color
                mapColor = pixels[x][y];
                // Blend heat color
                blendColor = Color.Lerp(mapColor, overlayColor, opacity);
                // Set new Color
                pixels[x][y] = blendColor;

                if (D > 0)
                {
                    y = y + yi;
                    D = D - 2 * dX;
                }

                D = D + 2 * dY;
            }
        }

        /// <summary>
        /// Paint line between pixels
        /// https://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm
        /// 
        /// Potential improvement : https://en.wikipedia.org/wiki/Xiaolin_Wu%27s_line_algorithm
        /// </summary>
        private static void PaintLine_High(in Color[][] pixels, int x0, int y0, int x1, int y1, Color overlayColor, int width = 100, float opacity = 0.5f)
        {
            // Cache values
            Color mapColor;
            Color blendColor;

            // Get offset from center
            int offset = width / 2;
            int dX = x1 - x0;
            int dY = y1 - y0;
            int xi = 1;
            if (dX < 0)
            {
                xi = -1;
                dX = -dX;
            }
            int D = 2 * dX - dY;
            int x = x0;

            for (int y = y0; y <= y1; y++)
            {
                // Protect borders
                if (x < 0 || x >= pixels.Length) continue;
                if (y < 0 || y >= pixels[x].Length) continue;

                // Get current Color
                mapColor = pixels[x][y];
                // Blend heat color
                blendColor = Color.Lerp(mapColor, overlayColor, opacity);
                // Set new Color
                pixels[x][y] = blendColor;

                if (D > 0)
                {
                    x = x + xi;
                    D = D - 2 * dY;
                }

                D = D + 2 * dX;
            }
        }

        /// <summary>
        /// Generate pixel map based on dimensions
        /// </summary>
        public static Color[][] GenerateBaseMap(int width, int height, Color color)
        {
            // Create 2D pixel map
            Color[][] map = new Color[width][];
            for (int x = 0; x < width; x++)
            {
                map[x] = new Color[height];

                // Paint each pixel
                for (int y = 0; y < height; y++)
                {
                    map[x][y] = color;
                }
            }

            return map;
        }

        /// <summary>
        /// Convert 2D pixel array to 1D
        /// </summary>
        public static Color[] Convert2DArrayTo1D(Color[][] pixels)
        {
            int width = pixels.Length;
            int height = pixels[0].Length;
            Color[] colors = new Color[width * height];
            for (int x = 0; x < pixels.Length; x++)
            {
                for (int y = 0; y < pixels[x].Length; y++)
                {
                    colors[(width * y) + x] = pixels[x][y];
                }
            }
            return colors;
        }

        [Serializable]
        public class Config
        {
            public string colorBackgroundFile = Color.clear.ToHex();
            public string colorGazesFile = Color.red.ToHex();
            public string colorSacadasFile = Color.blue.ToHex();
            public string colorBackgroundUI = Color.white.ToHex();
            public string colorGazesUI = Color.red.ToHex();
            public string colorSacadasUI = Color.blue.ToHex();

        }
    }
}