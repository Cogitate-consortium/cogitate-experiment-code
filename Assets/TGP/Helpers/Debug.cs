#region Using
// NS_REMOVE !!
using ExperimentLibrary;
using Helpers.Engine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#endregion

namespace TGP
{
    namespace Helpers
    {
        public static class Debug_Helper
        {
            public static bool doLogs = false;
            private static readonly bool log_ShowUI = false;
            public static bool isDebugBuild { get { return EngineWrapper.Debug_IsDebugBuild; } }

            public static void Log_ComplaintNoChild<T>(this T caller, Type childType) where T : class
            {
                caller.Log("Expected a {0} as child!"._Format(childType), LogType.Warning);
            }

            public static void Log<T>(this T caller, object msg, float showOnGUI_ForSeconds = 0) where T : class
            {
                caller.Log(msg, LogType.Log, showOnGUI_ForSeconds);
            }

            public static void LogError<T>(this T caller, object msg, float showOnGUI_ForSeconds = 0) where T : class
            {
                caller.Log(msg, LogType.Error, showOnGUI_ForSeconds);
            }

            public static void LogWarning<T>(this T caller, object msg, float showOnGUI_ForSeconds = 0) where T : class
            {
                caller.Log(msg, LogType.Warning, showOnGUI_ForSeconds);
            }

            public static void LogException<T>(this T caller, Exception exception, float showOnGUI_ForSeconds = 0) where T : class
            {
                caller.Log(exception, LogType.Exception, showOnGUI_ForSeconds);
            }

            public static void LogAssertion<T>(this T caller, object msg, float showOnGUI_ForSeconds = 0) where T : class
            {
                caller.Log(msg, LogType.Assert, showOnGUI_ForSeconds);
            }

            public static void Log<T>(this T caller, object msg, LogType logType, float showOnGUI_ForSeconds = 0) where T : class
            {
                if (!isDebugBuild) return;

                if (!doLogs && logType == LogType.Log) return;

                string output = "[" + (TimeWrapper.currentTimestampMS / 1000f).ToString("#.00") + " (<b>" + caller + "</b>)]: " + msg;
                if (logType == LogType.Log)
                    UnityEngine.Debug.Log(output, caller as UnityEngine.Object);
                else if (logType == LogType.Warning)
                    UnityEngine.Debug.LogWarning(output, caller as UnityEngine.Object);
                else if (logType == LogType.Error)
                    UnityEngine.Debug.LogError(output, caller as UnityEngine.Object);
                else if (logType == LogType.Exception)
                    UnityEngine.Debug.LogException(new Exception(output), caller as UnityEngine.Object);
                else if (logType == LogType.Assert)
                    UnityEngine.Debug.LogAssertion(output, caller as UnityEngine.Object);

                if (log_ShowUI)
                    OnGUI_AddMessage(output, showOnGUI_ForSeconds);
            }

            public static void OnGUI_AddMessage(object message, float time = Mathf.Infinity, bool showInReleaseMode = false)
            {
                if (!showInReleaseMode && !isDebugBuild) return;

                if (Debug_MB.instance)
                    Debug_MB.instance.OnGUI_AddMessage(message, time);
            }

            public static void OnGUI_Clear()
            {
                if (Debug_MB.instance)
                    Debug_MB.instance.OnGUI_Clear();
            }

            public static void DrawPoint(Vector3 point, float size, Color color, float duration)
            {
                // 3 axes => point visualization
                DrawRay(point - Vector3.up * size, point + Vector3.up * 2 * size, color, duration);
                DrawRay(point - Vector3.right * size, point + Vector3.right * 2 * size, color, duration);
                DrawRay(point - Vector3.forward * size, point + Vector3.forward * 2 * size, color, duration);
            }

            public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration)
            {
                UnityEngine.Debug.DrawRay(start, dir, color, duration);
            }

            /// <summary>
            /// [SOS] In debug builds it uses (<see cref="CustomToString{T}(T)"/> which has try-catches)
            /// </summary>
            public static string ToReadableString<T, G>(this Dictionary<T, G> dictionary, string title = "")
            {
                if (title == "")
                    title = "Dictionary contents";
                string s = title + ":\r\n";
                for (int i = 0; i < title.Length + 2; i++)
                    s += "-";
                foreach (KeyValuePair<T, G> kVP in dictionary)
                    s += "\r\n\r\n[" + kVP.Key.CustomToString() + "]\r\n" + kVP.Value.CustomToString();
                // s += "\n" + kVP.Key.CustomToString() + "\t" + kVP.Value.CustomToString();

                return s;
            }

            /// <summary>
            /// [SOS] In debug builds it uses (<see cref="CustomToString{T}(T)"/> which has try-catches)
            /// </summary>
            public static string ToReadableString<T>(this Queue<T> queue)
            {
                string s = "Queue contents:\r\n-----------------";

                int idx = 0;
                // [SOS] Don't deque the original queue
                Queue copy = new Queue(queue);

                // Add as many zeros as needed to have everything aligned
                int copyCountDigits = copy.Count.GetNumDigits();

                foreach (T t in queue)
                {
                    // Add as many zeros as needed to have everything aligned
                    string zeros = idx.AddLeadingSymbols(copyCountDigits);

                    s += "\r\n[" + zeros + "]: " + copy.Dequeue().ToString();

                    idx++;
                }

                return s;
            }

            /// <summary>
            /// [SOS] In debug builds it uses (<see cref="CustomToString{T}(T)"/> which has try-catches)
            /// </summary>
            public static string ToReadableString<T>(this IList<T> list, string title = "IList contents", bool requireThreadSafe = false)
            {
                // Add as many zeros as needed to have everything aligned
                int listCountDigits = list.Count.GetNumDigits();

                string s = "{0}:\r\n-----------------"._Format(title);
                for (int idx = 0; idx < list.Count; idx++)
                {
                    string zeros = idx.AddLeadingSymbols(listCountDigits);

                    s += "\r\n[" + zeros + "]: " + (requireThreadSafe ? list[idx].ToString() : list[idx].CustomToString());
                }
                return s;
            }
            /*
            /// <summary>
            /// [SOS] In debug builds it uses (<see cref="CustomToString{T}(T)"/> which has try-catches)
            /// </summary>
            public static string ToReadableString<T>(this T[][] array2D)
            {
                string s = "2D-Array contents:\n-----------------";
                int lengthI = array2D.GetLength(0);
                int lengthJ = array2D.GetLength(1);

                for (int i = 0; i < lengthI; i++)
                {
                    // Add as many zeros as needed to have everything aligned
                    string zeros_I = i.AddLeadingSymbols(lengthI);
                    for (int j = 0; j < lengthJ; j++)
                    {
                        // Add as many zeros as needed to have everything aligned
                        string zeros_J = j.AddLeadingSymbols(lengthJ);

                        s += "\n[{0}][{1}]: {2}"._Format(zeros_I, zeros_J, array2D[i][j].CustomToString());
                    }
                }
                return s;
            }
            */
            public static string ToStringExtended(this float f, string format, string one, string plural)
            {
                if (f == 1)
                    return f.ToString(format) + " " + one;
                else
                    return f.ToString(format) + " " + plural;
            }

            /// <summary>
            /// [SOS] Outside debug builds, it reverts to <see cref="ToString"/> (In debug builds, calls multiple <see cref="CustomToStruct{T, G}(T, out G)"/>, each with a try-catch)
            /// </summary>
            private static string CustomToString<T>(this T t)
            {
                if (!Debug_Helper.isDebugBuild)
                    return t.ToString();

                // Override for Raycast
                RaycastHit rH = default(RaycastHit);
                if (t.CustomToStruct(out rH))
                {
                    if (rH.IsNull())
                        return "{0} (Empty)"._Format(typeof(RaycastHit));
                    string format = "#.0000";
                    return "{0} ({1}, {2}, {3})"._Format(typeof(RaycastHit), rH.collider, rH.distance.ToString(format), rH.point.ToString(format));
                }

                return t.ToString();
            }

            /// <summary>
            /// [SOS] Use only for debugging (has a try-catch)
            /// </summary>
            public static bool CustomToStruct<T, G>(this T o, out G s)
            {
                s = default(G);
                try
                {
                    s = (G)(object)o;
                }
                catch (Exception e) { return e == null; }

                return true;
            }
        }

        public class Debug_MB : Singleton<Debug_MB>
        {
            protected override void Initialize()
            {
                base.Initialize();

                messages = new List<TimedMessage>();
            }

            protected override void DeInitialize()
            {
                OnGUI_Clear();
            }

            public void OnGUI_AddMessage(object message, float time = Mathf.Infinity)
            {
                if (messages == null)
                    messages = new List<TimedMessage>();

                string message_Text = "";

                if (message != null)
                    message_Text = message.ToString();

                if (message_Text.CompareTo("") == 0 || time == 0) return;

                TimedMessage tM = new TimedMessage();
                tM = new TimedMessage(instance, message_Text, a => { messages.Remove(tM); tM = null; }, time);
                messages.Add(tM);
            }

            public void OnGUI_Clear()
            {
                if (messages != null)
                    messages.Clear();
            }

            List<TimedMessage> messages = new List<TimedMessage>();

            private void OnGUI()
            {
                GUILayout.BeginArea(new Rect(30, 30, Screen.width, Screen.height));
                foreach (TimedMessage tM in messages)
                    if (tM != null && tM.isValid)
                        GUILayout.Label(tM.message);

                GUILayout.EndArea();
            }
        }

        public class TimedMessage
        {
            public TimedMessage() { }
            public TimedMessage(MonoBehaviour caller, string message, Action<bool> callback, float timeRemaining = Mathf.Infinity)
            {
                this.message = message;
                this.timeRemaining = timeRemaining;
                caller.StartCoroutine(Countdown(callback));
            }
            ~TimedMessage() { message = null; timeRemaining = 0; }

            public float timeRemaining = Mathf.Infinity;
            public string message;

            public IEnumerator Countdown(Action<bool> callback)
            {
                while (timeRemaining > 0)
                {
                    timeRemaining -= TimeWrapper.deltaTime_SinceLastUpdate_NotTS;
                    yield return new WaitForEndOfFrame();
                }

                message = null;
                timeRemaining = 0;
                callback(true);
            }

            public bool isValid
            {
                get
                {
                    return message != null && message.CompareTo("") != 0 && timeRemaining > 0;
                }
            }
        }
    }
}
