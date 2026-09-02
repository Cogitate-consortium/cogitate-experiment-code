using System;
using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;

namespace Experiment.Stimulus
{
    public class StimulusManager_QueueReaderManager
    {
        public static void TrySetupQueues(IList<string> queueContents,
            List<Sprite> stimuliTexturesFaces, List<Sprite> stimuliTexturesObjects,
            List<List<SpriteLocation>> orderedStimuli_GameWorlds,
            List<List<SpriteLocation>> orderedStimuli_Localizers,
            Action<bool> callback)
        {
            Debug_Helper.Log(typeof(StimulusManager), "SETTING UP QUEUES");

            orderedStimuli_GameWorlds.Clear();
            orderedStimuli_Localizers.Clear();

            string lastGameWorldID_0Based = "";
            string lastLocalizerID_0Based = "";

            foreach (string queueElement in queueContents)
            {
                string[] fields = queueElement.Split(';');

                string worldID_0Based = fields[0];
                string levelID_0Based = fields[1];
                StimulusType type = fields[2].ToEnum<StimulusType>();
                Direction_2D_Diagonal direction = fields[4].ToEnum<Direction_2D_Diagonal>();

                Sprite sprite;
                string spriteName;

                if (type == StimulusType.None)
                {
                    sprite = null;
                    spriteName = StimulusManager.EMPTY_STIM_NAME;
                }
                else
                {
                    int typeID = fields[3].ToInt();
                    sprite = (type == StimulusType.Face ? stimuliTexturesFaces : stimuliTexturesObjects)[typeID - 1];
                    spriteName = sprite.name;
                }

                SpriteLocation spriteLocation = new SpriteLocation(sprite, direction, spriteName, type);

                if (worldID_0Based.Contains("L") || worldID_0Based.Contains("R"))
                {
                    if (levelID_0Based != lastLocalizerID_0Based)
                    {
                        orderedStimuli_Localizers.Add(new List<SpriteLocation>());
                        lastLocalizerID_0Based = levelID_0Based;
                    }

                    orderedStimuli_Localizers.GetLast().Add(spriteLocation);
                }
                else
                {
                    if (worldID_0Based != lastGameWorldID_0Based)
                    {
                        orderedStimuli_GameWorlds.Add(new List<SpriteLocation>());
                        lastGameWorldID_0Based = worldID_0Based;
                    }

                    orderedStimuli_GameWorlds.GetLast().Add(spriteLocation);
                }
            }
            callback(true);
        }
    }
}