// NS_REMOVE | Config
using ExperimentLibrary;

using UnityEngine;
using UnityEngine.UI;
using Game.Core; // Just uses classes

namespace Helpers.UI.Core
{
    public class PathColor : MonoBehaviour
    {
        public Color blueColor;
        public Color orangeColor;
        public Image image;
        public WorldType world;

        // Start is called before the first frame update
        void Start()
        {
            world = (ExperimentLibraryManager.Config.PlayerProgression.levelsLibrary.invertBlueOrangeWorlds) ? 1 - world : world;
            image.color = (world == 0) ? blueColor : orangeColor;
        }

    }
}