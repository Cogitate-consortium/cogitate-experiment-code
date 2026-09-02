using Helpers.Engine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TGP
{
    namespace Helpers
    {
        public static class CSVSerialization
        {
            /*
            public static T FromCSV<T>(this string line, char separator, string nullFieldValue, bool hasHeader = true)
            {
                Type type = typeof(T);

                FieldInfo[] fields = type.GetFields();

                string[] data = line.Split(separator);

                if (data.Length < fields.Length)
                {
                    Debug.LogError(string.Format("There was an error for data {0}", line));
                    return default;
                }

                T obj = new T()
            }
            */

            public static string ToCsv<T>(this T obj, string separator, string nullFieldValue, bool printHeader = false)
            {
                Type t = typeof(T);

                FieldInfo[] fields = t.GetFields();

                System.Text.StringBuilder csvdata = new System.Text.StringBuilder();
                if (printHeader)
                {
                    string header = String.Join(separator, fields.Select(f => f.Name).ToArray());
                    csvdata.AppendLine(header);
                }

                csvdata.AppendLine(ToCsvFields(separator, nullFieldValue, fields, obj));

                return csvdata.ToString();
            }

            public static string ListToCsv<T>(this IEnumerable<T> list, string separator, string nullFieldValue, bool printHeader)
            {
                System.Text.StringBuilder csvdata = new System.Text.StringBuilder();
                int index = 0;

                foreach (var o in list)
                {
                    csvdata.Append(o.ToCsv(separator, nullFieldValue, printHeader && index == 0));
                    index++;
                }

                return csvdata.ToString();
            }

            public static string ToCsvFields<T>(string separator, string nullFieldValue, IList<FieldInfo> fields, T o)
            {
                System.Text.StringBuilder line = new System.Text.StringBuilder();

                foreach (var _f in fields)
                {
                    var f = _f;

                    if (line.Length > 0)
                        line.Append(separator);

                    // f isn't a field within T
                    if (f.ReflectedType != typeof(T))
                    {
                        // Is there a field within T that matches f?
                        List<FieldInfo> _fields = typeof(T).GetFields().ToList();
                        f = _fields.Find(a => a.Name == f.Name);
                    }

                    var x = f?.GetValue(o);

                    if (x != null)
                        line.Append(x.ToString());
                    else
                        line.Append(nullFieldValue);
                }

                return line.ToString();
            }
        }

        [System.Serializable]
        public struct EventInformation
        {
            public int id;

            public string eventType;

            public double delayFromPrevious;
            /// <summary>
            /// [SOS] For debug only
            /// </summary>
            public double delay_Intended;

            /// <summary>
            /// When is the cycle supposed to occur
            /// </summary>
            public double timestamp;
            /// <summary>
            /// [SOS] For debug only
            /// </summary>
            public double timestamp_Intended;

            public int triggeredByEventID;
            public int triggersEventID;
            public static readonly EventInformation DEFAULT = new EventInformation(0);

            public EventInformation(int sth)
            {
                id = -1;
                delayFromPrevious = -1;
                delay_Intended = -1;
                timestamp = -1;
                timestamp_Intended = -1;
                triggeredByEventID = -1;
                triggersEventID = -1;
                eventType = "";
            }

            public EventInformation(int id, double delay, double cumDelay) : this(0)
            {
                this.id = id;
                this.delayFromPrevious = delay;
                this.timestamp = cumDelay;
            }

            public EventInformation(int id, double delay, double delay_Intended, double cumDelay, double cumDelay_Intended, int lowerCycleID) : this(id, delay, delay_Intended)
            {
                this.timestamp = cumDelay;
                this.timestamp_Intended = cumDelay_Intended;
                this.triggeredByEventID = lowerCycleID;
            }

            public override string ToString()
            {
                if (triggeredByEventID < 0)
                    return "{0} :: {2}, {3}"._Format(id, triggeredByEventID, delayFromPrevious, timestamp);
                else
                    return "{0} ({1}) :: {2} ({3}), {4} ({5})"._Format(id, triggeredByEventID, delayFromPrevious, delay_Intended, timestamp, timestamp_Intended);
            }

            public void SetEventType(string eventType)
            {
                this.eventType = eventType;
            }

            public string ToStringDetailed()
            {
                return "{0};{1};{2};{3};{4};{5}\r\n"._Format(timestamp, eventType, delayFromPrevious, id, triggeredByEventID, triggersEventID);
            }

            internal static EventInformation FromCSV(string line)
            {
                EventInformation eventInformation = new EventInformation();
                string[] fields = line.Split(';');

                // timestamp	 eventType	 delayFromPrevious	 id	 triggeredByEventID	 triggersEventID
                eventInformation.timestamp = Utility_Helper.ToDouble_FromCSV(fields[0]);
                eventInformation.eventType = fields[1];
                eventInformation.delayFromPrevious = Utility_Helper.ToDouble_FromCSV(fields[2]);
                eventInformation.id = fields[3].ToInt();
                eventInformation.triggeredByEventID = fields[4].ToInt();
                eventInformation.triggersEventID = fields[5].ToInt();

                return eventInformation;
            }
        }

        [Serializable]
        public class DebugInfo
        {
            public string sender;
            public string message;

            public DebugInfo(string sender, string message)
            {
                this.sender = sender;
                this.message = message;
            }

            public override string ToString()
            {
                return "[{0}] {1}"._Format(sender, message);
            }
        }

        [Serializable]
        public class AudioClipVolume
        {
            public AudioClip audioClip = null;

            [Range(0, 1)]
            public float volumeMultiplier = 1;
        }

        public class FloatChangedArgs : EventArgs
        {
            public float newValue { get; private set; }
            public float diff { get; private set; }

            public FloatChangedArgs(float newValue, float oldValue)
            {
                this.newValue = newValue;
                diff = newValue - oldValue;
            }
        }

        public struct TimeStamped<T> : IEquatable<T> where T : struct
        {
            public TimeStamped(T obj) : this(obj, TimeWrapper.realtimeSinceStartup_NotTS) { }

            public TimeStamped(T obj, float referenceTime)
            {
                _value = obj;
                lastTimeUpdated = referenceTime;
            }

            public float GetTimeSinceLastUpdate(float referenceTime)
            {
                return referenceTime - lastTimeUpdated;
            }

            public float realTimeSinceLastUpdate { get { return GetTimeSinceLastUpdate(TimeWrapper.realtimeSinceStartup_NotTS); } }
            public T value { get { return _value; } set { _value = value; lastTimeUpdated = TimeWrapper.realtimeSinceStartup_NotTS; } }
            private T _value;
            private float lastTimeUpdated;

            // Assign from T to Timestamped variable
            public static implicit operator TimeStamped<T>(T obj)
            {
                return new TimeStamped<T>(obj);
            }

            // Assign from timestamped variable to T
            public static implicit operator T(TimeStamped<T> obj)
            {
                return obj.value;
            }
            public bool Equals(T other)
            {
                return other.Equals(value);
            }
        }

        public class StructEventArgs<T> : EventArgs, IEquatable<T> where T : struct
        {
            T value;
            public StructEventArgs(T value)
            {
                this.value = value;
            }

            public static implicit operator StructEventArgs<T>(T obj)
            {
                return new StructEventArgs<T>(obj);
            }

            // Assign from timestamped variable to T
            public static implicit operator T(StructEventArgs<T> obj)
            {
                return obj.value;
            }
            public bool Equals(T other)
            {
                return other.Equals(value);
            }
        }

        public class EventArgs<T> : EventArgs, IEquatable<T>
        {
            public T value;
            public EventArgs(T value)
            {
                this.value = value;
            }
            public static implicit operator EventArgs<T>(T obj)
            {
                return new EventArgs<T>(obj);
            }

            // Assign from timestamped variable to T
            public static implicit operator T(EventArgs<T> obj)
            {
                return obj.value;
            }
            public bool Equals(T other)
            {
                return other.Equals(value);
            }
        }

        /// <summary>
        /// Be aware this will not prevent a non singleton constructor
        ///   such as `T myT = new T();`
        /// To prevent that, add `protected T () {}` to your singleton class.
        /// 
        /// As a note, this is made as MonoBehaviour because we need Coroutines.
        /// </summary>
        public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
        {
            private static T _instance;
            // For Sanity check - should always be 1
            private static int members = 0;

            private static object _lock = new object();

            public static T ForceCreate()
            {
                return instance;
            }

            public static T instance
            {
                get
                {
                    if (applicationIsQuitting && Application.isPlaying)
                    {
#if UNITY_EDITOR
                        //   Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                        //      "' already destroyed on application quit." +
                        //       " Won't create again - returning null.");
#endif
                        return null;
                    }

                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            try
                            {
                                _instance = (T)FindObjectOfType(typeof(T));
                            }
                            catch(Exception ex)
                            {
                                Debug.LogError(string.Format("type:{0}\nerror:{1}", typeof(T).ToString(), ex.Message));
                            }

                            if (members > 1)
                            {
#if UNITY_EDITOR
                                Debug_Helper.Log(typeof(Singleton<T>), "[Singleton] Something went really wrong " +
                                    " - there should never be more than 1 singleton!" +
                                    " Reopenning the scene might fix it.", LogType.Error);
#endif
                                return _instance;
                            }

                            if (_instance == null)
                            {
                                GameObject singleton = new GameObject();
                                _instance = singleton.AddComponent<T>();
                                singleton.name = "(singleton) " + typeof(T).ToString();

                                // DontDestroyOnLoad(singleton);
#if UNITY_EDITOR
                                //                        if (Application.isPlaying)
                                //                            this.Log("[Singleton] An instance of " + typeof(T) +
                                //                                " is needed in the scene, so '" + singleton +
                                //                                "' was created with DontDestroyOnLoad.");
#endif
                            }
                            else
                            {
#if UNITY_EDITOR
                                //                        this.Log("[Singleton] Using instance already created: " +
                                //                            _instance.gameObject.name);
#endif
                            }
                        }

                        return _instance;
                    }
                }
            }

            private void Awake()
            {
                // Already have a member - who the fuck are you?
                if (members > 0)
                {
                    this.Log("Already have an instance of this singleton. Destroying.", LogType.Warning);
                    Destroy(gameObject);
                    return;
                }
                applicationIsQuitting = false;
                members++;
                Initialize();
            }

            /// <summary>
            /// Call the base method to NotDestroyOnLoad
            /// </summary>
            protected virtual void Initialize() {
                if(transform.parent == null)
                    DontDestroyOnLoad(gameObject);
            }
            protected virtual void DeInitialize() { }

            protected static bool applicationIsQuitting = false;
            /// <summary>
            /// When Unity quits, it destroys objects in a random order.
            /// In principle, a Singleton is only destroyed when application quits.
            /// If any script calls Instance after it have been destroyed, 
            ///   it will create a buggy ghost object that will stay on the Editor scene
            ///   even after stopping playing the Application. Really bad!
            /// So, this was made to be sure we're not creating that buggy ghost object.
            /// </summary>
            public void OnDestroy()
            {
                StopAllCoroutines();
                if (_instance == this) members--;
                if (members == 0)
                {
                    applicationIsQuitting = true;
                    _instance = null;
                }
                DeInitialize();
            }
        }

        public class TimeQueue<T>
        {
            public bool Contains(T item)
            {
                return countDown.ContainsKey(item);
            }

            private Dictionary<T, float> countDown;

            private EventHandler<EventArgs<T>> onObjectFinished;

            private TimeQueue() { }

            /// <summary>
            /// On finish countdown, object removed and sent as <b>args</b>, not sender.
            /// </summary>
            /// <param name="onObjectFinishedHandler"></param>
            public TimeQueue(EventHandler<EventArgs<T>> onObjectFinishedHandler)
            {
                Utility_Helper_MB.onUpdate += Update;
                onObjectFinished += onObjectFinishedHandler;
                countDown = new Dictionary<T, float>();
            }

            ~TimeQueue()
            {
                if (Utility_Helper_MB.onUpdate != null)
                    Utility_Helper_MB.onUpdate -= Update;
            }

            public List<T> GetAll()
            {
                return new List<T>(countDown.Keys);
            }

            public void AddOrUpdate(T t, float time)
            {
                countDown.TryAdd(t, time);
                countDown[t] = Mathf.Max(countDown[t], time);
            }

            public int Count
            {
                get
                {
                    int c = 0;
                    foreach (KeyValuePair<T, float> kVP in countDown)
                        if (kVP.Value >= 0)
                            c++;
                    return c;
                }
            }

            public void Remove(T t)
            {
                countDown[t] = 0;
            }

            public float GetTimeLeft(T t)
            {
                if (!countDown.ContainsKey(t)) return 0;
                return countDown[t];
            }

            public void SetAll(float newTime)
            {
                List<T> keys = new List<T>(countDown.Keys);

                foreach (T key in keys)
                {
                    countDown[key] = newTime;
                }
            }

            private void Update(object sender, EventArgs e)
            {
                List<T> keys = new List<T>(countDown.Keys);

                foreach (T key in keys)
                {
                    countDown[key] = Mathf.Max(0, b: countDown[key] - TimeWrapper.deltaTime_SinceLastUpdate_NotTS);
                    if (countDown[key] == 0)
                    {
                        countDown.Remove(key);
                        if (onObjectFinished != null)
                            onObjectFinished(this, new EventArgs<T>(key));
                    }
                }
            }

            public void Clear()
            {
                countDown.Clear();
            }
        }

        [Serializable]
        /// <summary>
        /// [SOS] When checking equality use .Equals() not ==
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        public class Tuple<T1, T2>
        {
            public T1 a;
            public T2 b;

            public Tuple() : this(default(T1), default(T2)) { }

            public Tuple(T1 a, T2 b)
            {
                this.a = a;
                this.b = b;
            }

            public override bool Equals(object obj)
            {
                Tuple<T1, T2> other = obj as Tuple<T1, T2>;

                if (other == null)
                {
                    return false;
                }

                return a.Equals(other.a) && b.Equals(other.b);
            }

            public override int GetHashCode()
            {
                int hCodeFirst = a.GetHashCode();
                int hCodeSecond = b.GetHashCode();

                return hCodeFirst * 47 + hCodeSecond;
            }

            public override string ToString()
            {
                return "({0}, {1})"._Format(a, b);
            }
        }

        /// <summary>
        /// [SOS] When checking equality use .Equals() not ==
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        public class Triplet<T1, T2, T3>
        {
            public T1 a;
            public T2 b;
            public T3 c;

            public Triplet() : this(default(T1), default(T2), default (T3)) { }

            public Triplet(T1 a, T2 b, T3 c)
            {
                this.a = a;
                this.b = b;
                this.c = c;
            }

            public override bool Equals(object obj)
            {
                Triplet<T1, T2, T3> other = obj as Triplet<T1, T2, T3>;

                if (other == null)
                {
                    return false;
                }

                return a.Equals(other.a) && b.Equals(other.b) && c.Equals(other.c);
            }

            public override int GetHashCode()
            {
                int hCodeA = a.GetHashCode();
                int hCodeB = b.GetHashCode();
                int hCodeC = c.GetHashCode();

                return hCodeA * 47 * 47 + hCodeB * 47 + hCodeC;
            }

            public override string ToString()
            {
                return "({0}, {1}, {2})"._Format(a, b, c);
            }
        }

        public class IEnumeratorSingleton
        {
            private IEnumerator iEnumerator;
            private Coroutine iEnumeratorInstance = null;
            private MonoBehaviour caller;

            private IEnumeratorSingleton() { }

            public IEnumeratorSingleton(MonoBehaviour caller, IEnumerator iEnumerator)
            {
                this.caller = caller;
                this.iEnumerator = iEnumerator;
            }

            public bool StartCoroutine(bool forceRestart = false)
            {
                if (!Check()) return false;

                if (iEnumeratorInstance != null && !forceRestart)
                {
                    this.Log("An instance of {0} is already running. Choose force restart if you want to override it."._Format(iEnumerator), LogType.Warning);
                    return false;
                }

                if (iEnumeratorInstance != null)
                    StopCoroutine();
                iEnumeratorInstance = caller.StartCoroutine(iEnumerator);
                this.Log("Started IEnumerator {0}!"._Format(iEnumerator));
                return true;
            }

            public bool StopCoroutine()
            {
                if (!Check()) return false;

                if (iEnumeratorInstance == null)
                {
                    this.Log("IEnumerator {0} has not been initialized."._Format(iEnumerator), LogType.Warning);
                    return false;
                }

                caller.StopCoroutine(iEnumeratorInstance);
                iEnumeratorInstance = null;
                this.Log("Stopped IEnumerator {0}!"._Format(iEnumerator));

                return true;
            }

            private bool Check()
            {
                if (caller == null)
                {
                    this.Log("There is no Caller set!!", LogType.Warning);
                    return false;
                }

                if (iEnumerator == null)
                {
                    this.Log("There is no IEnumerator set!!", LogType.Warning);
                    return false;
                }

                return true;
            }
        }

        [Serializable]
        /// <summary>
        /// Derive a new class from this base class before use:
        /// [Serializable] public class DictionaryOfStringAndInt : SerializableDictionary<string, int>
        /// {
        ///     public DictionaryOfStringAndInt() : base() { }
        ///     public DictionaryOfStringAndInt(Dictionary<string, int> obj) : base(obj) { }
        /// }
        /// </summary> 
        public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
        {
            [SerializeField]
            private int customCount = 0;

            [SerializeField]
            private List<TKey> keys = new List<TKey>();

            [SerializeField]
            private List<TValue> values = new List<TValue>();

            public SerializableDictionary() { }
            public SerializableDictionary(Dictionary<TKey, TValue> obj)
            {
                keys = new List<TKey>();
                values = new List<TValue>();

                foreach (KeyValuePair<TKey, TValue> kVP in obj)
                {
                    keys.Add(kVP.Key);
                    values.Add(kVP.Value);
                }
                OnAfterDeserialize();
            }

            // save the dictionary to lists
            public void OnBeforeSerialize()
            {
                keys.Clear();
                values.Clear();
                foreach (KeyValuePair<TKey, TValue> pair in this)
                {
                    keys.Add(pair.Key);
                    values.Add(pair.Value);
                }
            }

            // load dictionary from lists
            public void OnAfterDeserialize()
            {
                this.Clear();

                if (customCount > 0)
                {
                    keys.Add(default(TKey));
                    customCount = 0;
                }

                if (keys.Count > values.Count)
                {
                    for (int i = 0; i < keys.Count - values.Count; i++)
                        values.Add(default(TValue));
                }
                if (keys.Count != values.Count)
                {
                    throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable.", keys.Count, values.Count));

                }

                for (int i = 0; i < keys.Count; i++)
                    this.Add(keys[i], values[i]);
            }
        }
        
        [Serializable]
        public struct HSBColor
        {
            public float h;
            public float s;
            public float b;
            public float a;

            public HSBColor(float h, float s, float b, float a)
            {
                this.h = h;
                this.s = s;
                this.b = b;
                this.a = a;
            }

            public HSBColor(float h, float s, float b)
            {
                this.h = h;
                this.s = s;
                this.b = b;
                this.a = 1f;
            }

            public HSBColor(Color col)
            {
                HSBColor temp = FromColor(col);
                h = temp.h;
                s = temp.s;
                b = temp.b;
                a = temp.a;
            }

            public static HSBColor FromColor(Color color)
            {
                HSBColor ret = new HSBColor(0f, 0f, 0f, color.a);

                float r = color.r;
                float g = color.g;
                float b = color.b;

                float max = Mathf.Max(r, Mathf.Max(g, b));

                if (max <= 0)
                {
                    return ret;
                }

                float min = Mathf.Min(r, Mathf.Min(g, b));
                float dif = max - min;

                if (max > min)
                {
                    if (g == max)
                    {
                        ret.h = (b - r) / dif * 60f + 120f;
                    }
                    else if (b == max)
                    {
                        ret.h = (r - g) / dif * 60f + 240f;
                    }
                    else if (b > g)
                    {
                        ret.h = (g - b) / dif * 60f + 360f;
                    }
                    else
                    {
                        ret.h = (g - b) / dif * 60f;
                    }
                    if (ret.h < 0)
                    {
                        ret.h = ret.h + 360f;
                    }
                }
                else
                {
                    ret.h = 0;
                }

                ret.h *= 1f / 360f;
                ret.s = (dif / max) * 1f;
                ret.b = max;

                return ret;
            }

            public static Color ToColor(HSBColor hsbColor)
            {
                float r = hsbColor.b;
                float g = hsbColor.b;
                float b = hsbColor.b;
                if (hsbColor.s != 0)
                {
                    float max = hsbColor.b;
                    float dif = hsbColor.b * hsbColor.s;
                    float min = hsbColor.b - dif;

                    float h = hsbColor.h * 360f;

                    if (h < 60f)
                    {
                        r = max;
                        g = h * dif / 60f + min;
                        b = min;
                    }
                    else if (h < 120f)
                    {
                        r = -(h - 120f) * dif / 60f + min;
                        g = max;
                        b = min;
                    }
                    else if (h < 180f)
                    {
                        r = min;
                        g = max;
                        b = (h - 120f) * dif / 60f + min;
                    }
                    else if (h < 240f)
                    {
                        r = min;
                        g = -(h - 240f) * dif / 60f + min;
                        b = max;
                    }
                    else if (h < 300f)
                    {
                        r = (h - 240f) * dif / 60f + min;
                        g = min;
                        b = max;
                    }
                    else if (h <= 360f)
                    {
                        r = max;
                        g = min;
                        b = -(h - 360f) * dif / 60 + min;
                    }
                    else
                    {
                        r = 0;
                        g = 0;
                        b = 0;
                    }
                }

                return new Color(r, g, b, hsbColor.a);
            }

            public Color ToColor()
            {
                return ToColor(this);
            }

            public override string ToString()
            {
                return "H:" + h + " S:" + s + " B:" + b;
            }

            public static HSBColor Lerp(HSBColor a, HSBColor b, float t)
            {
                float h, s;

                //check special case black (color.b==0): interpolate neither hue nor saturation!
                //check special case grey (color.s==0): don't interpolate hue!
                if (a.b == 0)
                {
                    h = b.h;
                    s = b.s;
                }
                else if (b.b == 0)
                {
                    h = a.h;
                    s = a.s;
                }
                else
                {
                    if (a.s == 0)
                    {
                        h = b.h;
                    }
                    else if (b.s == 0)
                    {
                        h = a.h;
                    }
                    else
                    {
                        // works around bug with LerpAngle
                        float angle = Mathf.LerpAngle(a.h * 360f, b.h * 360f, t);
                        while (angle < 0f)
                            angle += 360f;
                        while (angle > 360f)
                            angle -= 360f;
                        h = angle / 360f;
                    }
                    s = Mathf.Lerp(a.s, b.s, t);
                }
                return new HSBColor(h, s, Mathf.Lerp(a.b, b.b, t), Mathf.Lerp(a.a, b.a, t));
            }

            public static void Test()
            {
                HSBColor color;

                color = new HSBColor(Color.red);
                Debug_Helper.Log(typeof(HSBColor), "red: " + color);

                color = new HSBColor(Color.green);
                Debug_Helper.Log(typeof(HSBColor), "green: " + color);

                color = new HSBColor(Color.blue);
                Debug_Helper.Log(typeof(HSBColor), "blue: " + color);

                color = new HSBColor(Color.grey);
                Debug_Helper.Log(typeof(HSBColor), "grey: " + color);

                color = new HSBColor(Color.white);
                Debug_Helper.Log(typeof(HSBColor), "white: " + color);

                color = new HSBColor(new Color(0.4f, 1f, 0.84f, 1f));
                Debug_Helper.Log(typeof(HSBColor), "0.4, 1f, 0.84: " + color);

                Debug_Helper.Log(typeof(HSBColor), "164,82,84   .... 0.643137f, 0.321568f, 0.329411f  :" + ToColor(new HSBColor(new Color(0.643137f, 0.321568f, 0.329411f))));
            }
        }

        public class TriggerCheckBase : MonoBehaviour
        {
            public EventHandler R_OnBecameInvisible, R_OnBecameVisible;

            public EventHandler P_OnMouseUpAsButton;

            private void OnMouseUpAsButton()
            {
                if (P_OnMouseUpAsButton != null)
                    P_OnMouseUpAsButton(this, null);
            }

            private void OnBecameInvisible()
            {
                if (R_OnBecameInvisible != null)
                    R_OnBecameInvisible(this, null);
            }

            private void OnBecameVisible()
            {
                if (R_OnBecameVisible != null)
                    R_OnBecameVisible(this, null);
            }
        }

        public class TriggerCheck : TriggerCheckBase
        {
            public delegate void OnTriggerChangeHandler(TriggerCheck self, Collider other);

            public OnTriggerChangeHandler TC_OnTriggerEnter, TC_OnTriggerExit, TC_OnTriggerStay;

            public delegate void OnCollisionChangeHandler(TriggerCheck self, Collision other);

            public OnCollisionChangeHandler TC_OnCollisionEnter, TC_OnCollisionExit, TC_OnCollisionStay;

            void OnTriggerEnter(Collider other)
            {
                if (TC_OnTriggerEnter != null)
                    TC_OnTriggerEnter(this, other);
            }

            void OnTriggerExit(Collider other)
            {
                if (TC_OnTriggerExit != null)
                    TC_OnTriggerExit(this, other);
            }

            void OnTriggerStay(Collider other)
            {
                if (TC_OnTriggerStay != null)
                    TC_OnTriggerStay(this, other);
            }

            void OnCollisionEnter(Collision other)
            {
                if (TC_OnCollisionEnter != null)
                    TC_OnCollisionEnter(this, other);
            }

            void OnCollisionExit(Collision other)
            {
                if (TC_OnCollisionExit != null)
                    TC_OnCollisionExit(this, other);
            }

            void OnCollisionStay(Collision other)
            {
                if (TC_OnCollisionStay != null)
                    TC_OnCollisionStay(this, other);
            }
        }

        public class TriggerCheck2D : TriggerCheckBase
        {
            public delegate void OnTriggerChangeHandler(TriggerCheck2D self, Collider2D other);

            public OnTriggerChangeHandler TC_OnTriggerEnter, TC_OnTriggerExit, TC_OnTriggerStay;

            public delegate void OnCollisionChangeHandler(TriggerCheck2D self, Collision2D other);

            public OnCollisionChangeHandler TC_OnCollisionEnter, TC_OnCollisionExit, TC_OnCollisionStay;

            void OnTriggerEnter2D(Collider2D other)
            {
                if (TC_OnTriggerEnter != null)
                    TC_OnTriggerEnter(this, other);
            }

            void OnTriggerExit2D(Collider2D other)
            {
                if (TC_OnTriggerExit != null)
                    TC_OnTriggerExit(this, other);
            }

            void OnTriggerStay2D(Collider2D other)
            {
                if (TC_OnTriggerStay != null)
                    TC_OnTriggerStay(this, other);
            }

            void OnCollisionEnter2D(Collision2D other)
            {
                if (TC_OnCollisionEnter != null)
                    TC_OnCollisionEnter(this, other);
            }

            void OnCollisionExit2D(Collision2D other)
            {
                if (TC_OnCollisionExit != null)
                    TC_OnCollisionExit(this, other);
            }

            void OnCollisionStay2D(Collision2D other)
            {
                if (TC_OnCollisionStay != null)
                    TC_OnCollisionStay(this, other);
            }
        }

        public class UICheck : MonoBehaviour, ISelectHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
        {
            public EventHandler<EventArgs<BaseEventData>> onSelect;
            public EventHandler<EventArgs<PointerEventData>> onClick;
            public EventHandler<EventArgs<PointerEventData>> onEnter;
            public EventHandler<EventArgs<PointerEventData>> onExit;

            public void OnPointerClick(PointerEventData eventData)
            {
                if (onClick != null)
                    onClick(this, eventData);
            }

            public void OnPointerEnter(PointerEventData eventData)
            {
                if (onEnter != null)
                    onEnter(this, eventData);
            }

            public void OnPointerExit(PointerEventData eventData)
            {
                if (onExit != null)
                    onExit(this, eventData);
            }

            public void OnSelect(BaseEventData eventData)
            {
                if (onSelect != null)
                    onSelect(this, eventData);
            }
        }

        public struct Phase
        {
            private int MAX_PHASES;

            /// <summary>
            /// Do [NOT] use ; this is for storing info only and needs to be PUBLIC. Use <see cref="currentPhase"/> instead.
            /// </summary>
            public int _currentPhase;
            public int currentPhase
            {
                get { return _currentPhase; }
                set
                {
                    if (value >= MAX_PHASES)
                    {
                        Debug_Helper.Log(typeof(Phase), "Invalid change {0} -> {1} (new phase > max :: {2})"._FormatBold(_currentPhase, value, MAX_PHASES), LogType.Warning);
                        return;
                    }

                    Debug_Helper.Log(typeof(Phase), "Phase change {0} -> {1}"._FormatBold(_currentPhase, value), LogType.Log);

                    _currentPhase = value;
                }
            }

            public Phase(int MAX_PHASES)
            {
                _currentPhase = 0;
                this.MAX_PHASES = MAX_PHASES;
            }

            public bool TryIncrementPhase(bool force = false, int increment = 1)
            {
                return TrySetPhase(currentPhase + increment, force);
            }

            public bool TryResetPhase(bool force = false)
            {
                return TrySetPhase(0, force);
            }

            public bool TrySetPhase(int value, bool force = false)
            {
                if (currentPhase == value && !force)
                {
                    Debug_Helper.Log(typeof(Phase), "Invalid change {0} -> {1} (same phase)"._FormatBold(currentPhase, value), LogType.Warning);
                    return false;
                }

                currentPhase = value;

                return true;
            }


            /*
             public class Job : ThreadedJob
             {
                 public Vector3[] InData;  // arbitary job data
                 public Vector3[] OutData; // arbitary job data

                 protected override void ThreadFunction()
                 { 
                 }
                 protected override void OnFinished()
                 { 
                 }

             }
              Job myJob;
             void Start ()
             {
                 myJob = new Job(inData);
                 myJob.Start(); // Don't touch any data in the job class after you called Start until IsDone is true.
             }
             void Update()
             {
                 if (myJob != null)
                 {
                     if (myJob.Update())
                     {
                         // Alternative to the OnFinished callback
                         myJob = null;
                     }
                 }
             }
              yield return StartCoroutine(myJob.WaitFor());
             */

            /* Load / Save:
            public static class SaveManager {

                public static SaveInfo saveInfo = new SaveInfo();

                public static void Reset()
                {
                    Utility_Helper.Reset<SaveInfo>(new SaveInfo());
                }

                public static void Load()
                {
                    saveInfo = Utility_Helper.Load<SaveInfo>(new SaveInfo());
                }

                public static void Save()
                {
                    saveInfo.Save();
                } 
            }
            */
        }

        [System.Serializable]
        public class JsonArray<T> where T : struct
        {
            public List<T> array = new List<T>();
            public JsonArray() { }

            public JsonArray(List<T> array)
            {
                this.array = array;
            }
        }

        public class ProgressReport
        {
            public bool isDone;
            public float value01;
            public string message;

            public ProgressReport(bool done) :
                this(done, done ? 1 : 0, "")
            { }

            public ProgressReport(bool done, float progress01) :
                this(done, progress01, "")
            { }

            public ProgressReport(bool done, float progress01, string message)
            {
                this.isDone = done;
                this.value01 = progress01;
                this.message = message;
            }
        }
    }
}