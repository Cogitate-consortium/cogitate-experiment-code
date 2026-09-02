using Helpers.Engine.Core;
using System;
using System.Collections;
using UnityEngine;

namespace Helpers.Engine
{
    /// <summary>
    /// [SOS] Make sure to call <see cref="Initialize"/> before using
    /// </summary>
    public static class EngineWrapper
    {
        public static bool Debug_IsDebugBuild { get; private set; }

        public static Coroutine StartCoroutine(IEnumerator enumerator)
        {
            return HelperMB._StartCoroutine(enumerator);
        }

        public static void StopCoroutine(Coroutine coroutine)
        {
            HelperMB._StopCoroutine(coroutine);
        }

        public static event EventHandler<float> onUpdate;
        public static event EventHandler<float> onFixedUpdate;
        public static event EventHandler onGUI;

        public static void Initialize()
        {
            HelperMB.Initialize();

            HelperMB.onUpdate -= HelperMB_onUpdate;
            HelperMB.onUpdate += HelperMB_onUpdate;
            HelperMB.onFixedUpdate -= HelperMB_onFixedUpdate;
            HelperMB.onFixedUpdate += HelperMB_onFixedUpdate;
            HelperMB.onGUI -= HelperMB_onGUI;
            HelperMB.onGUI += HelperMB_onGUI;

            Debug_IsDebugBuild = Debug.isDebugBuild;
        }

        public static T FindObjectOfType<T>() where T : UnityEngine.Object
        {
            return UnityEngine.Object.FindObjectOfType<T>();
        }

        public static void Destroy(Component component)
        {
            UnityEngine.Object.Destroy(component);
        }

        public static void Destroy(GameObject gameObject)
        {
            UnityEngine.Object.Destroy(gameObject);
        }

        private static void HelperMB_onUpdate(object sender, float e)
        {
            onUpdate?.Invoke(sender, e);
        }

        private static void HelperMB_onFixedUpdate(object sender, float e)
        {
            onFixedUpdate?.Invoke(sender, e);
        }

        private static void HelperMB_onGUI(object sender, EventArgs e)
        {
            onGUI?.Invoke(sender, e);
        }
    }
}