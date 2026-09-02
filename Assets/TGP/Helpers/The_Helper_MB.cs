// NS_ABSORB
using Helpers.Engine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TGP
{
    namespace Helpers
    {
        /// <summary>
        /// Helper for Coroutines -> Expose them neatly above in Utility_Helper:
        /// 
        /// Coroutine (in Utility_Helper_MB): 
        /*
                public IEnumerator InterpolateScale(GameObject gO, float scaleTo, float timeTo, bool andBack, Action<bool> callback = null)
                {
                    bool result = false;

                    // .. Do Stuff..

                    if(callback != null)
                        callback(result);
                }
        */
        /// 
        /// Exposed (in Utility_Helper): 
        /*
                // callback is called when the function ends
                public static void InterpolateScale(this GameObject gO, float scaleTo, float timeTo, bool andBack, Action<bool> callback = null)
                { 
                    Utility_Helper_MB.instance.StartCoroutine(Utility_Helper_MB.instance.InterpolateScale(gO, scaleTo, timeTo, andBack, callback));
                } 
        */
        /// Call (from other scripts):
        /*
                // Simple Call (no Callback)
                gO.InterpolateScale(1f, 1, false);

                // With defined Callback
                gO.InterpolateScale(1f, 1, false, OnDone);

                OnDone (bool success)
                {
                    this.Log(success? "Success" : "Fail");
                }

                // Alternative (Shorthand):
                gO.InterpolateScale(1f, 1, false,
                    success => { this.Log(success? "Success" : "Fail"); }
                );
        */
        /// </summary>
        public class Utility_Helper_MB : Singleton<Utility_Helper_MB>
        {
            private static List<UnityEngine.Object> LOCKED_OBJECTS;

            public static EventHandler<EventArgs> onUpdate { get { return instance._onUpdate; } set { instance._onUpdate = value; } }
            private EventHandler<EventArgs> _onUpdate;

            protected override void Initialize()
            {
                base.Initialize();

                LOCKED_OBJECTS = new List<UnityEngine.Object>();
            }

            protected override void DeInitialize()
            {
                if (LOCKED_OBJECTS != null)
                    LOCKED_OBJECTS.Clear();
            }

            void Update()
            {
                if (onUpdate != null)
                    onUpdate(this, new EventArgs());
            }

            public IEnumerator WaitUntil(Func<bool> predicate, Action<bool> callback, float timeout = -1)
            {
                float timeStarted = TimeWrapper.time_NotTS;
                yield return new WaitUntil(() => predicate() || (TimeWrapper.time_NotTS - timeStarted > timeout && timeout >= 0));
                if (callback != null)
                    callback(predicate());
            }

            // ------ INTERNET CHECKINGonst
            // Improvements: Enum for return value, list of callbacks so that when we do check, we notify all of them
            static bool CheckingConnection = false;
            public IEnumerator CheckInternetConnection(Action<bool> callback, string urlToCheck = "http://www.google.com")
            {
                if (CheckingConnection)
                {
                    if (callback != null)
                        callback(false);
                    this.Log("CheckInternetConnection -> Already Checking");
                    yield break;
                }
                CheckingConnection = true;

                WWW www = new WWW(urlToCheck);

                float t = 0;
                while (!www.isDone)
                {
                    t += TimeWrapper.deltaTime_SinceLastUpdate_NotTS;
                    if (t > 15)
                    {
                        if (callback != null)
                            callback(false);
                        this.Log("CheckInternetConnection -> Timed Out");
                        CheckingConnection = false;
                        yield break;
                    }
                    yield return new WaitForEndOfFrame();
                }
                if (!www.error.IsNullOrEmpty())
                {
                    if (callback != null)
                        callback(false);
                    this.Log("CheckInternetConnection -> Error\n" + www.error);
                }
                else
                {
                    if (callback != null)
                        callback(true);
                    this.Log("CheckInternetConnection -> Success");
                }
                CheckingConnection = false;
            }

            // ------ TIMERS
            public IEnumerator StartTimer(float dT, Action<bool> callback, int timesToCall = 1)
            {
                // To
                bool loopEndlessly = (timesToCall <= 0);
                while (loopEndlessly || timesToCall > 0)
                {
                    float startTime = TimeWrapper.time_NotTS;
                    // Allowing only position delta time(+0.01)
                    while (TimeWrapper.time_NotTS - startTime - TimeWrapper.deltaTime_SinceLastUpdate_NotTS / 2 < dT) yield return null;

                    // Allowing average precision time (-0.01, +0.01)
                    // yield return new WaitForSeconds(dT);

                    if (callback != null)
                        callback(true);
                    timesToCall--;
                }
            }

            // ---- LERPERS
            public IEnumerator InterpolateVolume(AudioSource aS, float volumeTo, float timeTo, bool andBack, ConflictResolutionStrategy conflictResolutionStrategy, Action<bool> callback = null)
            {
                if (conflictResolutionStrategy == ConflictResolutionStrategy.Wait)
                    while (!OBJECT_TRY_LOCK(aS)) yield return new WaitForEndOfFrame();
                else if (conflictResolutionStrategy == ConflictResolutionStrategy.Abort)
                {
                    if (!OBJECT_TRY_LOCK(aS)) yield break;
                }
                else if (conflictResolutionStrategy == ConflictResolutionStrategy.Interrupt)
                {
                    OBJECT_UNLOCK(aS);
                    yield return new WaitForEndOfFrame();
                    while (!OBJECT_TRY_LOCK(aS)) yield return new WaitForEndOfFrame();
                }
                if (aS)
                {

                    float baseVolume = aS.volume;

                    // To
                    yield return StartCoroutine(InterpolateVolume(aS, volumeTo, timeTo));

                    // And Back
                    if (andBack && OBJECT_IS_LOCKED(aS))
                        yield return StartCoroutine(InterpolateVolume(aS, baseVolume, timeTo));
                }

                OBJECT_UNLOCK(aS);

                if (callback != null)
                    callback(OBJECT_IS_LOCKED(aS));
            }

            public IEnumerator SmoothLookAt(Transform T, Vector3 pos, Vector3 up, float timeTo, ConflictResolutionStrategy conflictResolutionStrategy, Action<bool> callback = null)
            {
                if (conflictResolutionStrategy == ConflictResolutionStrategy.Wait)
                    while (!OBJECT_TRY_LOCK(T)) yield return new WaitForEndOfFrame();
                else if (conflictResolutionStrategy == ConflictResolutionStrategy.Abort)
                {
                    if (!OBJECT_TRY_LOCK(T)) yield break;
                }
                else if (conflictResolutionStrategy == ConflictResolutionStrategy.Interrupt)
                {
                    OBJECT_UNLOCK(T);
                    yield return new WaitForEndOfFrame();
                    while (!OBJECT_TRY_LOCK(T)) yield return new WaitForEndOfFrame();
                }

                if (T)
                {
                    if ((pos - T.position).magnitude < 0.005f)
                        yield break;

                    Quaternion wantedRotation = Quaternion.LookRotation(pos - T.position, up);

                    for (float t = 0; t < timeTo; t += TimeWrapper.deltaTime_SinceLastUpdate_NotTS)
                    {
                        if (!OBJECT_IS_LOCKED(T)) break;
                        if (T)
                        {
                            Quaternion rot = Quaternion.Slerp(T.rotation, wantedRotation, (t / timeTo).Clamped01());
                            // this.Log(val);
                            T.rotation = rot;
                        }
                        yield return new WaitForEndOfFrame();
                    }

                    if (T) T.rotation = wantedRotation;
                }

                OBJECT_UNLOCK(T);

                if (callback != null)
                    callback(OBJECT_IS_LOCKED(T));
            }

            public IEnumerator InterpolateScale(GameObject gO, Vector3 scaleTo, float timeTo, bool andBack, ConflictResolutionStrategy conflictResolutionStrategy, Action<bool> callback = null)
            {
                if (conflictResolutionStrategy == ConflictResolutionStrategy.Wait)
                    while (!OBJECT_TRY_LOCK(gO)) yield return new WaitForEndOfFrame();
                else if (conflictResolutionStrategy == ConflictResolutionStrategy.Abort)
                {
                    if (!OBJECT_TRY_LOCK(gO)) yield break;
                }
                else if (conflictResolutionStrategy == ConflictResolutionStrategy.Interrupt)
                {
                    OBJECT_UNLOCK(gO);
                    yield return new WaitForEndOfFrame();
                    while (!OBJECT_TRY_LOCK(gO)) yield return new WaitForEndOfFrame();
                }

                if (gO)
                {
                    Vector3 baseScale = gO.transform.localScale;

                    // To
                    yield return StartCoroutine(InterpolateScale(gO, scaleTo, timeTo));

                    // And Back
                    if (andBack && OBJECT_IS_LOCKED(gO))
                        yield return StartCoroutine(InterpolateScale(gO, baseScale, timeTo));
                }

                OBJECT_UNLOCK(gO);

                if (callback != null)
                    callback(OBJECT_IS_LOCKED(gO));

            }

            private IEnumerator InterpolateScale(GameObject gO, Vector3 scaleTo, float time)
            {
                if (!gO)
                    yield break;

                Vector3 scaleFrom = gO.transform.localScale;
                for (float t = 0; t < time; t += TimeWrapper.deltaTime_SinceLastUpdate_NotTS)
                {
                    // We lost the Lock
                    if (!OBJECT_IS_LOCKED(gO)) yield break;

                    Vector3 val = Vector3.Lerp(scaleFrom, scaleTo, (t / time).Clamped01());
                    // this.Log(val);
                    if (gO)
                        gO.transform.localScale = val;


                    yield return new WaitForEndOfFrame();
                }
                if (gO)
                    gO.transform.localScale = scaleTo;
            }

            private IEnumerator InterpolateVolume(AudioSource aS, float volumeTo, float time)
            {
                float volumeFrom = aS.volume;

                for (float t = 0; t < time; t += TimeWrapper.deltaTime_SinceLastUpdate_NotTS)
                {
                    // We lost the Lock
                    if (!OBJECT_IS_LOCKED(aS)) yield break;

                    float val = volumeFrom + (volumeTo - volumeFrom) * (t / time).Clamped01();
                    // this.Log(val);
                    if (aS)
                        aS.volume = val;

                    yield return new WaitForEndOfFrame();
                }
                if (aS)
                    aS.volume = volumeTo;
            }

            private static bool OBJECT_IS_LOCKED(UnityEngine.Object obj)
            {
                if (LOCKED_OBJECTS == null)
                    LOCKED_OBJECTS = new List<UnityEngine.Object>();
                return LOCKED_OBJECTS.Contains(obj);
            }

            private static bool OBJECT_TRY_LOCK(UnityEngine.Object obj)
            {
                if (LOCKED_OBJECTS == null)
                    LOCKED_OBJECTS = new List<UnityEngine.Object>();

                if (OBJECT_IS_LOCKED(obj))
                {
                    // instance.Log(obj + " was locked.", LogType.Warning);
                    return false;
                }

                LOCKED_OBJECTS.Add(obj);
                return true;
            }

            private static void OBJECT_UNLOCK(UnityEngine.Object obj)
            {
                if (LOCKED_OBJECTS == null)
                    LOCKED_OBJECTS = new List<UnityEngine.Object>();

                if (OBJECT_IS_LOCKED(obj))
                    LOCKED_OBJECTS.Remove(obj);
            }

            // ---- AUDIO LOADING
            public void LoadAudio(string url, Action<bool, AudioClip> callback, bool debug)
            {
                StartCoroutine(_LoadAudio(url, callback, debug));
            }

            public void LoadAudio(Dictionary<string, string> keyUrls, Action<bool, Dictionary<string, AudioClip>> callback, bool debug)
            {
                StartCoroutine(_LoadAudio(keyUrls, callback, debug));
            }

            IEnumerator _LoadAudio(Dictionary<string, string> keyUrls, Action<bool, Dictionary<string, AudioClip>> callback, bool debug = false, float timeout = 10)
            {
                Dictionary<string, AudioClip> cB_Dic = new Dictionary<string, AudioClip>();
                this.Log("Loading the following audios:\n{0}"._Format(keyUrls.ToReadableString()));
                bool allGood = true;
                foreach (KeyValuePair<string, string> kVP in keyUrls)
                {
                    StartCoroutine(_LoadAudio(kVP.Value, (s, a) =>
                    {
                        cB_Dic.Add(kVP.Key, a);
                        allGood &= s;
                    }, debug));
                }

                for (float t = 0; t <= timeout; t += TimeWrapper.fixedDeltaTime_NotTS)
                {
                    if (cB_Dic.Count >= keyUrls.Count)
                    {
                        if (callback != null)
                            callback(allGood, cB_Dic);
                        yield break;
                    }
                    yield return new WaitForFixedUpdate();
                }

                if (callback != null)
                    callback(false, cB_Dic);
            }

            IEnumerator _LoadAudio(string url, Action<bool, AudioClip> callback, bool debug)
            {
                if (debug)
                    this.Log("Loading from {0}"._Format(url));
                WWW www = new WWW(url);

                yield return www;

                if (!www.error.IsNullOrEmpty())
                {
                    if (debug)
                        this.Log(www.error, LogType.Error);
                    if (callback != null)
                        callback(false, null);
                    yield break;
                }

                AudioClip aC = www.GetAudioClip();
                if (aC == null)
                {
                    if (callback != null)
                        callback(false, null);
                }

                aC.name = Path.GetFileName(url);

                if (callback != null)
                    callback(true, aC);
            }
        }

        public enum ConflictResolutionStrategy { Wait = 0, Interrupt, Abort }
    }
}
