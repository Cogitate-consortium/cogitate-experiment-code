using Helpers.Assets;
using Helpers.Engine;
using System;
using System.Collections;
using TGP.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace Peripherals.Photodiode
{
    /// <summary>
    /// [SOS] Run <see cref="Initialize(Config)"/> first
    /// </summary>
    public class PhotoDiodeDebugger : MonoBehaviour
    {
        public static EventHandler<StatusUpdateArgs> onStatusUpdate;
        private static Config config;
        private static PhotoDiodeDebugger instance = null;

        public static void Initialize(Config config)
        {
            PhotoDiodeDebugger.config = config;
            if (instance != null) return;
            instance = ResourceHelper.InstantiateResource<GameObject>("PhotoDiodeDebugger").GetComponent<PhotoDiodeDebugger>();
            instance._Initialize();

            DontDestroyOnLoad(instance);
        }

        private static Color activeColor
        {
            get
            {
                float brightness = config.photoDiodeBrightness;
                return new Color(brightness, brightness, brightness, 1.0f);
            }
        }

        public static void DeInitialize()
        {
        }

        public static bool TryFire(int numBangs, object reason)
        {
            if (instance == null)
            {
                Debug_Helper.LogWarning(typeof(PhotoDiodeDebugger),
                    "Already running! Aborting!");
                return false;
            }

            return instance._TryFire(numBangs, reason);
        }

        private void _Initialize()
        {
            name = "PhotoDiadiodeDebbuger";
            //(photoDiodeImage.transform as RectTransform).sizeDelta = new Vector2(config.photoDiodeSizeCM, config.photoDiodeSizeCM);
            Transform pdi_transform = photoDiodeImage.transform;
            RectTransform pdi_recttransform = pdi_transform as RectTransform;
            pdi_recttransform.sizeDelta = new Vector2(config.photoDiodeSizeCM, config.photoDiodeSizeCM);

            // Offseting the rectangle
            float signX = config.photoDiodeLeft ? 1 : -1;
            pdi_recttransform.anchoredPosition += new Vector2(signX * config.photoDiodeOffsetX * Screen.width / 1920, config.photoDiodeOffsetY*Screen.height /1280);

            leftPhotodiodeImage.gameObject.SetActive(config.photoDiodeLeft);
            rightPhotodiodeImage.gameObject.SetActive(!config.photoDiodeLeft);
        }

        private Coroutine fireCR = null;

        private bool _TryFire(int numBangs, object reason)
        {
            if (fireCR != null)
            {
                this.LogWarning("Already running! Aborting!");
                return false;
            }

            fireCR = StartCoroutine(_FireIE(numBangs, reason));

            return true;
        }

        private IEnumerator _FireIE(int numBangs, object reason)
        {
            int lastOperation_FrameCycleID = -1;
            for (int i = 0; i < numBangs; i++)
            {
                if (TimeWrapper.currentFrameCycleID == lastOperation_FrameCycleID)
                {
                    this.LogError("Tried to toggle OFF in the same frame we toggled on");
                    yield return null;
                }

                // On
                //  Debug.Log("PDD ON CYCLE ID " + TimeWrapper.currentFrameCycleID + " , " + TimeWrapper.currentTimestampMS);
                _Toggle(true, reason, i == 0);

                lastOperation_FrameCycleID = TimeWrapper.currentFrameCycleID;

                // Wait
                yield return new WaitForSeconds(config.photoDiodeDuration);

                if (TimeWrapper.currentFrameCycleID == lastOperation_FrameCycleID)
                {
                    this.LogError("Tried to toggle OFF in the same frame we toggled on");
                    yield return null;
                }

                // Off
                // Debug.Log("PDD OFF CYCLE ID " + TimeWrapper.currentFrameCycleID + " , " + TimeWrapper.currentTimestampMS);
                _Toggle(false, reason, false);

                // Wait
                yield return new WaitForSeconds(config.photoDiodeDelayBetweenBangs);
            }

            fireCR = null;
        }

        private void _Toggle(bool on, object reason, bool isFirst)
        {
            photoDiodeImage.color = (on) ? activeColor : Color.black;
            // Debug.LogError(reason + " -> " + on);

            SendStatusUpdate(new StatusUpdateArgs(on, reason, isFirst));
        }

        private static void SendStatusUpdate(StatusUpdateArgs statusUpdate)
        {
            onStatusUpdate?.Invoke(null, statusUpdate);
        }

        private void OnDestroy()
        {
            instance = null;
        }

        private Image photoDiodeImage { get { return (config.photoDiodeLeft) ? leftPhotodiodeImage : rightPhotodiodeImage; } }

        public Image rightPhotodiodeImage;
        public Image leftPhotodiodeImage;

        [Serializable]
        public class Config
        {
            public bool debug = false;

            public float photoDiodeDuration { get { return photoDiodeDurationMS / 1000f; } }
            public float photoDiodeDelayBetweenBangs { get { return photoDiodeDelayBetweenBangsMS / 1000f; } }

            public int photoDiodeDurationMS = 50;
            public int photoDiodeDelayBetweenBangsMS = 50;

            public float photoDiodeBrightness = 1f;
            public float photoDiodeSizeCM = 1f;
            [SerializeField] private string photoDiodeOffset_comment = "Linear offset, somewhat resolution independent | max_photoDiodeOffsetX = 49, max_photoDiodeOffsetY = 32 ";
            public float photoDiodeOffsetX = 0f;
            public float photoDiodeOffsetY = 0f;
            public bool photoDiodeLeft = false;
        }

        public class StatusUpdateArgs : EventArgs
        {
            public bool isOn;
            public object reason;
            public bool isFirst;

            public StatusUpdateArgs(bool isOn, object reason, bool isFirst)
            {
                this.isOn = isOn;
                this.reason = reason;
                this.isFirst = isFirst;
            }
        }
    }
}