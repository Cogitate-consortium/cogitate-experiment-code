// NS_REMOVE
using Experiment.Library.Core;
using ExperimentLibrary;
using Experiment.Managers;
using Helpers.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using TGP.Helpers;
using UnityEngine;

namespace Experiment.Stimulus
{
    public class StimulusManager_AssetsManager
    {
        /// <summary>
        /// Both inputs will be cleared and then AddRange()
        /// </summary>
        /// <param name="blobImagesFaces"></param>
        /// <param name="blobImagesObjects"></param>
        public static void LoadBlobs(List<Sprite> blobImagesFaces, List<Sprite> blobImagesObjects)
        {
            blobImagesFaces.Clear();
            blobImagesObjects.Clear();

            StreamingAssetsManager.GetAllSprites(ExperimentPaths.stimuliDirectoryBlobFaces, images =>
            {
                if (images.Count == 0)
                    Debug_Helper.LogError(typeof(ExperimentLibraryManager), "Trying to load ditractor face, but there are not any at path:" + ExperimentPaths.stimuliDirectoryBlobFaces);
                blobImagesFaces.AddRange(images);
            });
            StreamingAssetsManager.GetAllSprites(ExperimentPaths.stimuliDirectoryBlobsObjects, images =>
            {
                if (images.Count == 0)
                    Debug_Helper.LogError(typeof(ExperimentLibraryManager), "Trying to load ditractor objects, but there are not any at path:" + ExperimentPaths.stimuliDirectoryBlobsObjects);
                blobImagesObjects.AddRange(images);
            });
        }

        public static void Initialize(
            List<Sprite> stimuliTexturesFaces, List<Sprite> stimuliTexturesObjects, 
            Dictionary<string, int> stimulusNameToID, Action<bool> callback)
        {
            Debug_Helper.Log(typeof(StimulusManager), "RESET");
            stimuliTexturesFaces.Clear();
            stimuliTexturesObjects.Clear();
            stimulusNameToID.Clear();


            Action<bool> OnSpritesLoaded = partialAssetLoadSuccess =>
            {
                if (!partialAssetLoadSuccess)
                {
                    callback(false);
                    return;
                }

                // TODO Needs more robustness (ie. number of attempted loads vs actual loads)
                // Make sure we have the needed Sprites
                if (stimuliTexturesFaces.Count == 0 ||
                    stimuliTexturesObjects.Count == 0)
                    return;

                callback(true);
            };

            // Load face stimuli
            StreamingAssetsManager.GetAllSprites(ExperimentPaths.stimuliDirectoryFaces, (sprites) =>
            {
                stimuliTexturesFaces.AddRange(sprites.OrderBy(a => a.name));

                // Log their names
                string msg = "Loaded Faces (ID_0-Based, Stim Name) :: ";
                for (int i = 0; i < stimuliTexturesFaces.Count; i++)
                {
                    string name = stimuliTexturesFaces[i].name;
                    stimulusNameToID.AddOrUpdate(name, i);
                    msg += "({0}, {1})"._Format(stimulusNameToID[name], name);
                }

                ExperimentManagerSession.LogSessionInfo(msg);

                Debug_Helper.Log(typeof(StimulusManager), "LOADED {0} FACES"._Format(stimuliTexturesFaces.Count));

                // Create collage
                if (ExperimentLibraryManager.Config.Experiment.stimulus.createPreExposureSlide_Faces)
                    CollageSpritesAndExportPNG(stimuliTexturesFaces,
                        ExperimentLibraryManager.Config.Experiment.stimulus.preExposureConfig_Faces,
                        ExperimentPaths.GetTutorialInfoImagesDirectory(ExperimentManagerSession.module.ToString()));

                OnSpritesLoaded(true);
            });

            // Load object stimuli
            StreamingAssetsManager.GetAllSprites(ExperimentPaths.stimuliDirectoryObjects, (sprites) =>
            {
                stimuliTexturesObjects.AddRange(sprites.OrderBy(a => a.name));

                // Log their names
                string msg = "Loaded Objects (ID_0-Based, Stim Name) :: ";
                for (int i = 0; i < stimuliTexturesObjects.Count; i++)
                {
                    string name = stimuliTexturesObjects[i].name;
                    stimulusNameToID.AddOrUpdate(name, i);
                    msg += "({0}, {1})"._Format(stimulusNameToID[name], name);
                }

                ExperimentManagerSession.LogSessionInfo(msg);

                Debug_Helper.Log(typeof(StimulusManager), "LOADED {0} OBJECTS"._Format(stimuliTexturesObjects.Count));

                // Create collage
                if (ExperimentLibraryManager.Config.Experiment.stimulus.createPreExposureSlide_Objects)
                    CollageSpritesAndExportPNG(stimuliTexturesObjects,
                        ExperimentLibraryManager.Config.Experiment.stimulus.preExposureConfig_Objects,
                        ExperimentPaths.GetTutorialInfoImagesDirectory(ExperimentManagerSession.module.ToString()));

                OnSpritesLoaded(true);
            });
        }

        private static void CollageSpritesAndExportPNG(List<Sprite> sprites, PreExposureSlidesConfig preExposureConfig, string folderPath)
        {
            List<Sprite> spritesToWrite = new List<Sprite>();
            for (int i = 0; i < sprites.Count; i++)
                if (!preExposureConfig.excludeNames.Contains(sprites[i].name))
                    spritesToWrite.Add(sprites[i]);

            if (preExposureConfig.shuffle)
                spritesToWrite = spritesToWrite.Shuffle().ToList();
            CollageSpritesAndExportPNG(spritesToWrite, preExposureConfig.numColumns, preExposureConfig.numRows,
                preExposureConfig.paddingHorizontal, preExposureConfig.paddingVertical, folderPath + "/" + preExposureConfig.fileName + ".png");
        }

        private static void CollageSpritesAndExportPNG(List<Sprite> sprites, int numSpritesPerRow, int numRows, float paddingHorizontal_PercentileOfWidth, float paddingVertical_PercentileOfHeight, string filepath)
        {
            if (sprites.Count == 0) return;

            // Constrain number of rows
            // numRows = Mathf.CeilToInt(Mathf.Min(numRows, sprites.Count / numSpritesPerRow * 1f));

            // Assume all sprites equal
            Vector2Int sizeSprite = new Vector2Int(sprites[0].texture.width, sprites[0].texture.height);

            int pH = Mathf.RoundToInt(paddingHorizontal_PercentileOfWidth * sizeSprite.x);
            int pV = Mathf.RoundToInt(paddingVertical_PercentileOfHeight * sizeSprite.y);

            Vector2Int sizeFinal = new Vector2Int(
                numSpritesPerRow * sizeSprite.x + (numSpritesPerRow - 1) * pH,
                numRows * sizeSprite.y + (numRows - 1) * pV);

            int w = sizeSprite.x;
            int h = sizeSprite.y;
            int W = sizeFinal.x;
            int H = sizeFinal.y;
            // Get the pixels of each sprite
            Color[] pixelsFinal = new Color[sizeFinal.x * sizeFinal.y];

            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numSpritesPerRow; j++)
                {
                    int spriteIdxWithinSprites = i * numSpritesPerRow + j;
                    if (spriteIdxWithinSprites >= sprites.Count) break;
                    Texture2D temp = sprites[spriteIdxWithinSprites].texture;
                    Color[] pixelsSprite = temp.GetPixels();

                    // Find the pixel's index
                    for (int y = 0; y < temp.height; y++)
                    {
                        for (int x = 0; x < temp.width; x++)
                        {
                            int pixelIdxWithinSprite = y * w + x;
                            int pixelIdxWithinFinal = i * W * (h + pV) + y * W + j * (w + pH) + x;
                            pixelsFinal[pixelIdxWithinFinal] = pixelsSprite[pixelIdxWithinSprite];
                        }
                    }
                }
            }

            IO_Helper.WriteToPNG(pixelsFinal, sizeFinal, filepath);
            Debug_Helper.Log(typeof(StimulusManager), "CREATED PNG COLLAGE @{0}"._Format(filepath));
        }
    }

    [Serializable]
    public class PreExposureSlidesConfig
    {
        public List<string> excludeNames = new List<string>() { };
        public int numColumns = 5;
        public int numRows = 4;
        public string fileName = "";
        public bool shuffle = true;
        public float paddingHorizontal = 0.1f;
        public float paddingVertical = 0.1f;

        public PreExposureSlidesConfig()
        {
        }

        public PreExposureSlidesConfig(string fileName)
        {
            this.fileName = fileName;
        }
    }

}