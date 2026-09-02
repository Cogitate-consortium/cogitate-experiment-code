// NS_DEBATABLE
using Helpers.Assets;
// NS_DEBATABLE
using Helpers.Async;
// NS_DEBATABLE
using Peripherals.UserInput;

using Helpers.Engine;
using System;
using System.Collections;
using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Managers.LevelManagers
{
    /// <summary>
    /// [CLEANUP]
    /// </summary>
    public class LevelMasterManager_Game_Tutorial_I_UI : MonoBehaviour
    {
        public class PresentationImageInfo
        {
            public Sprite sprite;
            public float zoom;

            public PresentationImageInfo(Sprite sprite, float zoom)
            {
                this.sprite = sprite;
                this.zoom = zoom;
            }
        }

        public event EventHandler<EventArgs> onEndOfGame;

        public static LevelMasterManager_Game_Tutorial_I_UI instance;

        [Header("Editor")]
        public Button endButton;
        public Button nextButton;
        public Button previousButton;
        public Image presentationImage;
        private bool isReady = false;

        public List<PresentationImageInfo> images = new List<PresentationImageInfo>();
        private int presentationIndex = 0;
        private Coroutine autoProceedCR = null;

        const string RESOURCE_NAME = "LevelMasterManager_Game_Tutorial_I_UI";
        private RuntimeConfig runtimeConfig;

        public static void Initialize(RuntimeConfig runtimeConfig)
        {
            if (instance == null)
            {
                instance = Resources_Helper.ResourceInstantiate<LevelMasterManager_Game_Tutorial_I_UI>(RESOURCE_NAME);
                instance.name = RESOURCE_NAME;

                instance._Initialize(runtimeConfig);

            }
            else
            {
                // instance.isReady = false; ??
                /*
                 * 200620 - doesnt seem needed
                if (GameObject.Find("EventSystem") == null)
                {
                    Debug_Helper.LogError(typeof(ProbeSystem), "ProbeSystem need UI 'EventSystem' GameObject in order to work");
                    return;
                }
                */
            }
        }

        private void _Initialize(RuntimeConfig runtimeConfig)
        {
            this.runtimeConfig = runtimeConfig;

            if (runtimeConfig.autoProceed)
            {
                StopAutoProceed();
                StartAutoProceed();
            }

            endButton.onClick.AddListener(EndButton_OnClick);
            nextButton.onClick.AddListener(NextButton_OnClick);
            previousButton.onClick.AddListener(PreviousButton_OnClick);

            StreamingAssetsManager.GetAllSprites(runtimeConfig.tutorialImagesPath, (_imageList) =>
            {
                images = new List<PresentationImageInfo>();

                foreach (Sprite s in _imageList)
                {
                    bool ignoreZoom = runtimeConfig.ignoreZoomFileNames.Contains(s.name);

                    images.Add(new PresentationImageInfo(s, ignoreZoom ? 1 : runtimeConfig.zoomFactor));
                }
                images.Sort(delegate (PresentationImageInfo c1, PresentationImageInfo c2) { return c1.sprite.name.CompareTo(c2.sprite.name); });
                SetPresentationIndex(0);
                isReady = true;
            });
            InputManager.onKeyUp_TS += InputManager_onKeyUp_TS;
        }

        public static void DeInitialize()
        {
            if (instance == null) return;

            instance.StopAutoProceed();
        }

        private void InputManager_onKeyUp_TS(object sender, InputManager.HighAccuracyEventArgs e)
        {
            // We don't need anything related to the Presentation to be that accurate
            AsyncThread.RunOnMainThread_ASAP_TS(() =>
            {
                if (!isReady)
                {
                    this.LogWarning("Not ready!");
                    return;
                }

                KeyCode key = e.key;

                if (runtimeConfig.keyCodes_Backward.Contains(key))
                    SetPresentationIndex(presentationIndex - 1);
                else if (runtimeConfig.keyCodes_Forward.Contains(key))
                {
                    if (presentationIndex == images.Count - 1)
                        onEndOfGame?.Invoke(this, new EventArgs());
                    else
                        SetPresentationIndex(presentationIndex + 1);
                }
            });
        }

        private void OnDestroy()
        {
            InputManager.onKeyUp_TS -= InputManager_onKeyUp_TS;
        }

        private void SetPresentationIndex(int index)
        {
            if (index < 0)
                index = 0;
            if (index >= images.Count)
                index = images.Count - 1;
            presentationIndex = index;
            presentationImage.sprite = images[presentationIndex].sprite;
            presentationImage.transform.localScale = Vector3.one * images[presentationIndex].zoom;

            bool isEnd = presentationIndex == images.Count - 1;
            endButton.gameObject.SetActive(isEnd);
            nextButton.gameObject.SetActive(!isEnd);
        }

        private void PreviousButton_OnClick()
        {
            SetPresentationIndex(presentationIndex - 1);
        }

        private void NextButton_OnClick()
        {
            SetPresentationIndex(presentationIndex + 1);
        }

        private void EndButton_OnClick()
        {
            onEndOfGame?.Invoke(this, new EventArgs());
        }

        private void StartAutoProceed()
        {
            autoProceedCR = StartCoroutine(AutoProceedInstructions(0.35f));
        }

        private void StopAutoProceed()
        {
            if (autoProceedCR != null)
            {
                StopCoroutine(autoProceedCR);
                autoProceedCR = null;
            }
        }

        private IEnumerator AutoProceedInstructions(float timeBetweenSlides)
        {
            yield return new WaitUntil(() => isReady);

            while (true)
            {
                InputManager_onKeyUp_TS("fake", new InputManager.HighAccuracyEventArgs(
                    // Any will do
                    runtimeConfig.keyCodes_Forward.GetRandom(), TimeWrapper.GetCurrentTimestamp_TS()));

                yield return new WaitForSeconds(timeBetweenSlides);
                yield return null;
            }
        }

        public class RuntimeConfig
        {
            public string tutorialImagesPath;
            public bool autoProceed;
            public List<KeyCode> keyCodes_Forward;
            public List<KeyCode> keyCodes_Backward;
            public List<string> ignoreZoomFileNames;
            public float zoomFactor;

            public RuntimeConfig(string tutorialImagesPath, bool autoProceed, IList<KeyCode> keyCodes_Forward, IList<KeyCode> keyCodes_Backward, List<string> ignoreZoomFileNames, float zoomFactor)
            {
                this.tutorialImagesPath = tutorialImagesPath;
                this.autoProceed = autoProceed;
                this.keyCodes_Forward = new List<KeyCode>(keyCodes_Forward);
                this.keyCodes_Backward = new List<KeyCode>(keyCodes_Backward);
                this.ignoreZoomFileNames = ignoreZoomFileNames;
                this.zoomFactor = zoomFactor;
            }
        }
    }
}