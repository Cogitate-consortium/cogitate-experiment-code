using System;
using System.Collections;
using TGP.Helpers;
using UnityEngine;

namespace Helpers.Engine.Core
{
    /// <summary>
    /// [SOS] Make sure this has highest script priority
    /// [SOS] Make sure to call <see cref="Initialize"/> exactly once before using
    /// </summary>
    public class HelperMB : MonoBehaviour
    {
        private static HelperMB instance;

        public static void Initialize()
        {
            if (instance != null)
            {
                // Debug_Helper.LogWarning(typeof(HelperMB), "Already Init");
                return;
            }

            instance = new GameObject(typeof(HelperMB).FullName).AddComponent<HelperMB>();
        }

        public static Coroutine _StartCoroutine(IEnumerator enumerator)
        {
            return instance?.StartCoroutine(enumerator);
        }

        public static void _StopCoroutine(Coroutine coroutine)
        {
            instance?.StopCoroutine(coroutine);
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (instance != this)
            {
                Debug.LogWarning("Shouldn't have two");
                return;
            }

            instance = null;
        }

        public static EventHandler<float> onUpdate;
        public static EventHandler<float> onFixedUpdate;
        public static EventHandler onGUI;

        private void Update()
        {
            onUpdate?.Invoke(this, Time.deltaTime);
        }

        private void FixedUpdate()
        {
            onFixedUpdate?.Invoke(this, Time.fixedDeltaTime);
        }

        private void OnGUI()
        {
            onGUI?.Invoke(this, null);
        }
    }
}