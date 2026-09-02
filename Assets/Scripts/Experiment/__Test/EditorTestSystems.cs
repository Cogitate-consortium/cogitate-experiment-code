// NS_REMOVE
using Experiment.Library.Core;

// NS_DEBATABLE | What is it testing?
using Helpers.Assets;
// NS_DEBATABLE | What is it testing?
using Peripherals.Logging;

using System.Collections.Generic;
using UnityEngine;

namespace ExperimentLibrary.Test
{
    public class EditorTestSystems : MonoBehaviour
    {
#if UNITY_EDITOR
        [Header("Check Update() function within script")]
        public string hooray = "hooray!";

        [Header("Debug values")]
        public List<Sprite> sprites;

        private void Start()
        {
            if (!ExperimentLibraryManager.isInitialized)
            {
                Debug.LogError("Need ApplicationLibrary in order to test", this.transform);
                this.gameObject.SetActive(false);
                return;
            }
        }

        // Update is called once per frame
        void Update()
        {
            // Test FileLogger
            if (Input.GetKeyDown(KeyCode.Q))
            {
                LogWrapper.WriteToLogsAsync("Test", "Test logging system");
            }

            // Test SoundSystem
            if (Input.GetKeyDown(KeyCode.W))
            {
                // SoundSystem.PlayAudio(ApplicationLibrary.Config.Audio.Music);
            }

            // Test ProbeSystem
            if (Input.GetKeyDown(KeyCode.E))
            {
                /*
                ProbeSystem.ShowProbe(ApplicationLibrary.Config.Probes.Questions[0], (index, question) =>
                {
                    //Debug.Log("User unswered:" + index);
                });
                */
            }

            // Test Fetching many files from directory
            if (Input.GetKeyDown(KeyCode.R))
            {
                StreamingAssetsManager.GetAllSprites(ExperimentPaths.stimuliDirectoryObjects, (sprites) =>
                {
                    this.sprites = sprites;
                    Debug.Log(sprites.Count);
                });
            }
        }

#endif

    }
}