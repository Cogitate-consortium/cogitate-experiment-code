using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using System.IO;
using System.Text.RegularExpressions;
using Helpers.Engine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TGP
{
    namespace Helpers
    {
        public static class Utility_Helper
        {
            #region Scene
            public static Transform FindChildRecursively(this Transform t, string childName, bool includeInactive)
            {
                if (t == null) return null;
                Transform[] temp = t.GetChildrenContaining(childName, true, true, includeInactive);

                if (temp == null)
                    return null;
                if (temp.Length == 0)
                    return null;
                return temp[0];
            }

            public static List<Transform> GetOrderedChildTree(this Transform T, bool downwards = true)
            {
                List<Transform> tree = new List<Transform>();

                for (int i = 0; i < T.childCount; i++)
                {
                    tree.Add(T.GetChild(i));
                    tree.AddRange(T.GetChild(i).GetOrderedChildTree(true));
                }

                if (!downwards)
                    tree.Reverse();

                return tree;
            }

            public static Transform[] GetChildrenContaining(this Transform T, string subString, bool matchCase, bool wholeWordsOnly, bool includeInactive = true)
            {
                if (T == null) return null;

                List<Transform> list = new List<Transform>();

                foreach (Transform t in T.GetComponentsInChildren<Transform>(includeInactive))
                {
                    /// so "heirs" in inactive "bloodlines" will be selected. Not very intuitive.
                    if (!t.gameObject.activeInHierarchy && !includeInactive) continue;

                    if (matchCase && !t.name.Contains(subString))
                        continue;

                    if (!matchCase && !t.name.ContainsInvariant(subString))
                        continue;

                    if (wholeWordsOnly && (t.name.Length != subString.Length))
                        continue;

                    list.Add(t);
                }

                return list.ToArray();
            }

            public static GameObject[] GetChildrenGameObjects(this GameObject obj, bool includeInactive = false)
            {
                List<GameObject> list = new List<GameObject>();

                for (int i = 0; i < obj.transform.childCount; i++)
                    if (obj.transform.GetChild(i) != obj.transform)
                        list.Add(obj.transform.GetChild(i).gameObject);
                return list.ToArray();
            }

            public static GameObject[] GetChildrenGameObjects_Recursively(this GameObject obj, bool includeInactive = false)
            {
                List<GameObject> list = new List<GameObject>();

                foreach (Transform t in obj.GetComponentsInChildren<Transform>(includeInactive))
                    if (t != obj.transform)
                        list.Add(t.gameObject);
                return list.ToArray();
            }

            /// <summary>
            /// Returns a valid position within the mesh, where you can place the collider, 
            /// so it doesn't collide with anything in the layermask, 
            /// ignoring those with tags in ignoreTags.
            /// </summary>  
            public static Vector3 GetValidPositionInsideConvex(this MeshFilter container, Collider colliderToPlace, LayerMask layerMask, params string[] ignoreTags)
            {
                // Get all colliders that might cause trouble
                List<Collider> opposingColliders = GetComponentsInScene<Collider>().ToList();

                // Filter out by tag
                opposingColliders.FilterByTags(KeepOrRemove.Remove, ignoreTags);

                // If our container has a collider attached to it, also ignore
                Collider self = container.GetComponent<Collider>();
                if (self)
                    opposingColliders.Remove(self);

                // Remove the collider we're trying to place
                opposingColliders.Remove(colliderToPlace);

                // Filter out those who aren't in the area or in the layer mask or which are triggers
                for (int i = 0; i < opposingColliders.Count; i++)
                {
                    if (!layerMask.Contains(opposingColliders[i].gameObject.layer) || opposingColliders[i].isTrigger)
                    {
                        opposingColliders.RemoveAt(i);
                        i--;
                    }
                }

                // Pick a random position in the container 
                Vector3 currentGuess = Vector3.zero;

                // Create a dummy collider at the size of our collider
                GameObject dummyGO = new GameObject();
                colliderToPlace.CopyComponentToGameObject(dummyGO);
                dummyGO.transform.localScale = colliderToPlace.transform.lossyScale;
                Collider dummy = dummyGO.GetComponent<Collider>();

                bool done = false;
                int count = 0;
                while (!done && count++ < 50)
                {
                    // Pick a new spot
                    currentGuess = container.transform.TransformPoint(container.mesh.GetRandomPointInsideConvex());

                    // Check if placing the collider at that point would collide with anything
                    dummy.transform.position = currentGuess;
                    bool debug = false;
#if UNITY_EDITOR
                    debug = true;
#endif
                    if (!dummy.IntersectsAny(opposingColliders, debug))
                        done = true;
                }
                UnityEngine.Object.DestroyImmediate(dummyGO);
                return currentGuess;
            }

            internal static void DestroyOnLoad(GameObject gameObject)
            {
                SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
            }

            /// <summary>
            /// Finds a single Component of type T in the scene - else returns null & Error
            /// </summary>
            public static T GetComponentInScene<T>(bool debug = false) where T : Component
            {
                T myObject = UnityEngine.Object.FindObjectOfType<T>();

                if (debug)
                {
                    if (myObject == null)
                        Debug_Helper.Log(typeof(Utility_Helper), "Make sure there is a " + typeof(T).FullName + " in the scene..!", LogType.Warning);
                    else
                        Debug_Helper.Log(typeof(Utility_Helper), "Found a " + typeof(T).FullName + " attached to " + myObject.name);
                }

                return myObject;
            }

            /// <summary>
            /// Finds all Components of type T in the scene - else returns empty & Error
            /// </summary>
            public static T[] GetComponentsInScene<T>(bool debug = false) where T : Component
            {
                T[] myObjects = UnityEngine.Object.FindObjectsOfType<T>();

                if (debug)
                {
                    if (myObjects.Length == 0)
                        Debug_Helper.Log(typeof(Utility_Helper), "Couldn't find any " + typeof(T).FullName + " components in the scene..!", LogType.Warning);
                    else
                        Debug_Helper.Log(typeof(Utility_Helper), "Found " + myObjects.Length + " " + typeof(T).FullName + (myObjects.Length != 1 ? " component" : " components") + " attached to various gameObjects");
                }

                return myObjects;
            }

            public static IList<T> FilterByTag<T>(this IList<T> container, KeepOrRemove keepOrDestroyThoseInTag, string tag) where T : Component
            {
                return container.FilterByTags(keepOrDestroyThoseInTag, tag);
            }

            public static IList<T> FilterByTags<T>(this IList<T> container, KeepOrRemove keepOrDestroyThoseInTags, params string[] tags) where T : Component
            {
                if (container.IsReadOnly)
                    container = container.FromReadOnly();

                List<T> originalContainer = new List<T>(container);

                if (tags.Length > 0)
                    foreach (T obj in originalContainer)
                        if ((!tags.Contains(obj.tag) && keepOrDestroyThoseInTags == KeepOrRemove.Keep)
                            || (tags.Contains(obj.tag) && keepOrDestroyThoseInTags == KeepOrRemove.Remove))
                            container.Remove(obj);

                return container;
            }

            public static bool Contains(this LayerMask layerMask, string layerName)
            {
                return (layerMask == (layerMask | 1 << LayerMask.NameToLayer(layerName)));
            }

            public static bool Contains(this LayerMask layerMask, int layer)
            {
                return (layerMask == (layerMask | 1 << layer));
            }

            public static Collider[] GetAllColliders(this IList<RaycastHit> hitInfos, bool includeTriggers)
            {
                List<Collider> colliders = new List<Collider>();

                foreach (RaycastHit rH in hitInfos)
                    if (!rH.IsNull() && (includeTriggers || !rH.collider.isTrigger))
                        colliders.Add(rH.collider);

                return colliders.ToArray();
            }

            private static Vector2 screenCenterOffsetFrom_05_05 = new Vector2(0, 0);
            private static Vector2 screenScaleWithRespectToFull = new Vector2(1, 1);

            /// <summary>
            /// Affects <see cref="GetRelativeScreenPosition(Vector3, Camera)"/>
            /// </summary>
            public static void SetScreenOffsetAndScale(Vector2 screenCenterOffsetFrom_05_05, Vector2 screenScaleWithRespectToFull)
            {
                Utility_Helper.screenCenterOffsetFrom_05_05 = screenCenterOffsetFrom_05_05;
                Utility_Helper.screenScaleWithRespectToFull = screenScaleWithRespectToFull;

                Debug_Helper.LogWarning(typeof(Utility_Helper), "Set Screen Offset to {0} and Scale to {1}"._Format(screenCenterOffsetFrom_05_05, screenScaleWithRespectToFull));
            }

            public static Vector2 GetScreenOffset()
            {
                return screenCenterOffsetFrom_05_05;
            }

            public static Vector2 GetScreenScale()
            {
                return screenScaleWithRespectToFull;
            }

            // To avoid GC
            private static Vector2 _relativeScreenPosition;

            /// <summary>
            /// [SOS] Affected by <see cref="SetScreenOffsetAndScale(Vector2, Vector2)"/>
            /// </summary>
            public static Vector2 GetRelativeScreenPosition(Vector3 objectPosition, Camera camera = null)
            {
                if (Camera.main == null) return Vector2.zero;
                _relativeScreenPosition = Camera.main.WorldToScreenPoint(objectPosition);
                _relativeScreenPosition.x /= (float)Screen.width;
                _relativeScreenPosition.y /= (float)Screen.height;
                
                // That's the relative screen position with center at 0.5, 0.5 and assuming full screen

                // Shift it by its offset
                _relativeScreenPosition -= screenCenterOffsetFrom_05_05;

                // Scale it (by the default center)
                _relativeScreenPosition -= Vector2.one * 0.5f;
                _relativeScreenPosition /= screenScaleWithRespectToFull;
                _relativeScreenPosition += Vector2.one * 0.5f;

                return _relativeScreenPosition;
            }

            #endregion


            #region Transforms

            public static Rect GetAreaFullScreen()
            {
                return Screen.safeArea;
            }

            public static bool IsInsideArea(Transform transform, Rect area)
            {
                Vector3 worldPosition = transform ? transform.position : Vector3.negativeInfinity;

                // [TODO] Expand to RECT checking (if any part inside)
                bool isInside = area.Contains(worldPosition);

                // Debug.Log(worldPosition + " -> " + isInside, transform);

                return isInside;
            }

            /// <summary>
            /// Sets the local scale of the transform so that its lossy scale is as requested.
            /// </summary> 
            public static void SetLossyScale(this Transform t, Vector3 lossyScale)
            {
                Vector3 scaleToSet = lossyScale;
                if (t.parent != null)
                    scaleToSet = lossyScale.DivideBy(t.parent.lossyScale);
                if (!scaleToSet.IsNaN())
                    t.localScale = scaleToSet;
            }

            public static bool CopyPoseTo(this Transform from, Transform to)
            {
                Transform[] fromChildren = from.GetComponentsInChildren<Transform>();
                Transform[] toChildren = to.GetComponentsInChildren<Transform>();

                if (fromChildren.Length != toChildren.Length)
                    return false;

                for (int i = 0; i < fromChildren.Length; i++)
                {
                    toChildren[i].localPosition = fromChildren[i].localPosition;
                    toChildren[i].localRotation = fromChildren[i].localRotation;
                    toChildren[i].localScale = fromChildren[i].localScale;
                }

                return true;
            }

            public static void SmoothLookAt(this Transform T, Vector3 target, float damping)
            {
                Vector3 diff = (target - T.position);
                if (diff.magnitude == 0)
                    return;
                Quaternion rotation = Quaternion.LookRotation(target - T.position);
                T.rotation = Quaternion.Slerp(T.rotation, rotation, TimeWrapper.deltaTime_SinceLastUpdate_NotTS * damping);
            }

            public static void Reset(this Transform t)
            {
                RectTransform rT = t.GetComponent<RectTransform>();
                if (rT)
                {
                    rT.anchorMin = Vector2.zero;
                    rT.anchorMax = Vector2.one;
                    rT.sizeDelta = Vector2.zero;
                }
                else
                    t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
            }

            public static void ReparentAndReset<T>(this T t, Transform newParent, bool isUI = false) where T : MonoBehaviour
            {
                t.transform.ReparentAndReset(newParent, !isUI);
            }

            public static void ReparentAndReset(this GameObject gO, Transform newParent, bool isUI = false)
            {
                gO.transform.ReparentAndReset(newParent, !isUI);
            }

            public static void ReparentAndReset(this Transform t, Transform newParent, bool isUI = false)
            {
                t.Reparent(newParent, isUI);
                t.Reset();
            }

            public static void Genocide<T>(this T t) where T : Component
            {
                if (!t) return;
                for (int i = 0; i < t.transform.childCount; i++)
                    UnityEngine.Object.Destroy(t.transform.GetChild(i).gameObject);
            }

            public static void Reparent(this Transform t, Transform newParent, bool isUI = false)
            {
                bool worldPosStays = !isUI;
                if (t != null)
                    t.transform.SetParent(newParent, worldPosStays);
            }
            #endregion


            #region Components
            public static T TryGetComponent<T>(this GameObject gO) where T : Component
            {
                if (!gO) return null;
                return gO.GetComponent<T>();
            }

            public static T AddComponentIfNotExists<T>(this GameObject gO) where T : Component
            {
                T existingT = gO.GetComponent<T>();
                if (existingT)
                    return existingT;
                return gO.AddComponent<T>();
            }

            public static MonoBehaviour GetMonoBehaviour<I>(this I i) where I : class
            {
                return i.TryGetAs<I, MonoBehaviour>();
            }

            public static I GetInterface<I>(this Component c)
            {
                return c.GetComponent<I>();
            }

            public static float RandomRange(MinMax minMax)
            {
                return RandomRange(minMax.min, minMax.max);
            }

            public static I GetInterface<I>(this GameObject g)
            {
                return g.GetComponent<I>();
            }

            public static I GetInterface<I>(this object o)
            {
                if (o.IsNull()) return default(I);
                MonoBehaviour mB = o.TryGetAs<object, MonoBehaviour>();
                if (mB == null) return default(I);
                return mB.GetInterface<I>();
            }

            public static C GetComponent<C>(this object o) where C : Component
            {
                MonoBehaviour mB = o.GetMonoBehaviour();
                if (mB == null) return null;
                return mB.GetComponent<C>();
            }

            private static G TryGetAs<T, G>(this T t, bool debug = false) where T : class where G : class
            {
                G g = t as G;
                if (g == null)
                {
                    // Try Component
                    if (debug)
                        t.Log("Could not cast from " + typeof(T).Name + " to " + typeof(G).Name + ".", LogType.Warning);
                    return null;
                }

                return g;
            }

            public static T[] GetCompNoRoot<T>(this GameObject obj) where T : Component
            {
                List<T> tList = new List<T>();
                foreach (Transform child in obj.transform.root)
                {
                    T[] scripts = child.GetComponentsInChildren<T>();
                    if (scripts != null)
                    {
                        foreach (T sc in scripts)
                            tList.Add(sc);
                    }
                }
                return tList.ToArray();
            }

            public static void CopyTo<T>(this T source, T target)
            {
                var type = typeof(T);
                foreach (PropertyInfo typeProperty in type.GetProperties())
                {
                    MethodInfo propertyGetter = typeProperty.GetGetMethod();
                    MethodInfo propertySetter = typeProperty.GetSetMethod();

                    // The Getter or Setter doesn't exist or isn't public, we can't do anything here
                    if (propertyGetter == null || propertySetter == null) continue;

                    PropertyInfo targetProperty = type.GetProperty(typeProperty.Name);
                    targetProperty.SetValue(target, typeProperty.GetValue(source, null), null);
                }
                foreach (FieldInfo typeField in type.GetFields())
                {
                    FieldInfo targetField = type.GetField(typeField.Name);
                    targetField.SetValue(target, typeField.GetValue(source));
                }
            }

            public static void CopyComponentToGameObject<T>(this T other, GameObject gO) where T : Component
            {
                try
                {
                    if (typeof(T) == typeof(Collider))
                    {
                        Type colliderType = other.GetType();
                        gO.AddComponent(colliderType);
                        other.CopyComponentTo(gO.GetComponent(colliderType));
                    }
                    else
                    {
#if !UNITY_METRO
                        if (!typeof(T).IsAbstract)
#endif
                            other.CopyComponentTo(gO.AddComponentIfNotExists<T>());
                    }
                }
                catch (Exception e)
                {
                    if (Debug.isDebugBuild)
                        Debug.LogException(e, gO);
                    return;
                }
            }

            static T CopyComponentTo<T>(this T other, T comp) where T : Component
            {
                Type type = comp.GetType();
                if (type != other.GetType()) return null; // type mis-match

#pragma warning disable 0219
                BindingFlags flags = BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
#pragma warning restore 0219

#if !UNITY_METRO
                flags |= BindingFlags.Default;
#endif
                PropertyInfo[] pinfos;
#if !UNITY_METRO || UNITY_EDITOR
                pinfos = type.GetProperties(flags);
#else
        pinfos = type.GetRuntimeProperties().
                         Where(p=> p.GetMethod != null && p.SetMethod != null &&
                                     !p.SetMethod.IsStatic && !p.GetMethod.IsStatic).ToArray();
#endif
                foreach (var pinfo in pinfos)
                {
                    if (pinfo.CanWrite)
                    {
                        try
                        {
                            pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                        }
                        catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                    }
                }
                FieldInfo[] finfos;
#if !UNITY_METRO || UNITY_EDITOR
                finfos = type.GetFields(flags);
#else
        finfos = type.GetRuntimeFields().ToArray();
#endif
                foreach (var finfo in finfos)
                {
                    finfo.SetValue(comp, finfo.GetValue(other));
                }
                return comp as T;
            }

            /// <summary>
            /// Call with both type parameters specified
            /// ie foreach (Renderer r in myRigidbody.GetComponentsInChildren_ExcludeSelf<Rigidbody, Renderer>())
            /// </summary>
            public static T GetComponentInChildren_ExcludeSelf<G, T>(this G g, bool includeInactive = true) where G : Component where T : Component
            {
                foreach (T t in g.GetComponentsInChildren<T>(includeInactive))
                {
                    if (t.gameObject == g.gameObject) continue;
                    return t;
                }
                return default(T);
            }
            /// <summary>
            /// Call with both type parameters specified
            /// ie foreach (Renderer r in myRigidbody.GetComponentsInChildren_ExcludeSelf<Rigidbody, Renderer>())
            /// </summary>
            public static T[] GetComponentsInChildren_ExcludeSelf<G, T>(this G g, bool includeInactive = true) where G : Component where T : Component
            {
                List<T> objects = new List<T>();
                foreach (T t in g.GetComponentsInChildren<T>(includeInactive))
                {
                    if (t.gameObject == g.gameObject) continue;
                    objects.Add(t);
                }
                return objects.ToArray();
            }
            #endregion


            #region Enums
            public static T EnumGetRandom<T>()
            {
                var v = Enum.GetValues(typeof(T));
                int randomIndex = RandomRange(0, v.Length);
                return (T)v.GetValue(randomIndex);
            }

            public static bool EnumContains<T>(string name)
            {
                T t = default(T);
                return EnumContains(name, out t);
            }

            public static bool EnumContains<T>(string name, out T t)
            {
                foreach (T _t in EnumGetValues<T>())
                    if (_t.ToString() == name)
                    {
                        t = _t;
                        return true;
                    }

                t = default(T);

                return false;
            }

            public static List<T> EnumGetValues<T>()
            {
                List<T> values = new List<T>();
                var v = Enum.GetValues(typeof(T));
                for (int i = 0; i < v.Length; i++)
                    values.Add((T)v.GetValue(i));
                return values;
            }

            public static List<string> EnumGetValuesAsStrings<T>()
            {
                List<string> names = new List<string>();
                foreach (T t in EnumGetValues<T>())
                    names.Add(t.ToString());
                return names;
            }

            public static int EnumCount<T>()
            {
                var v = Enum.GetValues(typeof(T));
                return v.Length;
            }

            public static T ToEnum<T>(this int value, T defaultValue = default(T))
            {
                if (!Enum.IsDefined(typeof(T), value))
                    return defaultValue;

                return (T)(object)value;
            }

            #endregion


            #region Arrays, Lists & Dictionaries
            public static void ClearAndDestroy<T>(this IList<T> list) where T : Component
            {
                foreach (T t in list)
                    if (t)
                        GameObject.Destroy(t.gameObject);
                list.Clear();
            }

            public static void ClearAndDestroy<T, G>(this Dictionary<T, G> dictionary) where G : Component
            {
                foreach (G g in dictionary.Values)
                    if (g)
                        GameObject.Destroy(g.gameObject);
                dictionary.Clear();
            }

            public static void ClearAndDestroy(this IList<GameObject> list)
            {
                foreach (GameObject t in list)
                    if (t)
                        GameObject.Destroy(t.gameObject);
                list.Clear();
            }

            public static void ClearAndDestroy<T>(this Dictionary<T, GameObject> dictionary)
            {
                foreach (GameObject g in dictionary.Values)
                    if (g)
                        GameObject.Destroy(g.gameObject);
                dictionary.Clear();
            }

            public static bool Or(this IList<bool> values)
            {
                bool or = false;
                foreach (bool v in values)
                {
                    if (v)
                    {
                        or = true;
                        break;
                    }
                }
                return or;
            }

            public static bool And(this IList<bool> values)
            {
                bool and = true;
                foreach (bool v in values)
                {
                    if (!v)
                    {
                        and = false;
                        break;
                    }
                }
                return and;
            }

            public static T GetRandom<T>(this IList<T> array)
            {
                if (array == null) return default(T);
                if (array.Count == 0) return default(T);
                int randomValue = RandomRange(0, int.MaxValue);
                int randomIndex = randomValue % array.Count;
                return array[randomIndex];
            }

            public static T GetRandomWeighted<T>(this Dictionary<T, float> dictionary)
            {
                if (dictionary == null) return default(T);
                if (dictionary.Count == 0) return default(T);

                float sum = dictionary.Values.Sum();
                if (sum == 0) return new List<T>(dictionary.Keys).GetRandom();

                float randomValue = RandomRange(0, sum);

                float count = 0;
                foreach (KeyValuePair<T, float> kVP in dictionary)
                {
                    float newValue = kVP.Value; // / sum;
                    count += newValue;

                    if (randomValue < count)
                        return kVP.Key;
                }

                Debug_Helper.LogWarning(typeof(Utility_Helper), "Something weird happened");
                return default(T);
            }

            private static System.Random RANDOM;

            public static bool RandomBool()
            {
                return RandomRange(0f, 1f) >= 0.5f;
            }

            /// <summary>
            /// Returns a random float number between and min [inclusive] and max [inclusive].
            /// </summary>
            public static float RandomRange(float min, float max)
            {
                InitializeRandSeedIfNecessary();

                return ((float)RANDOM.NextDouble()).Retargeted(0, 1, min, max);
                // return UnityEngine.Random.Range(min, max);
            }

            /// <summary>
            /// Returns a random integer number between and min [inclusive] and max [exclusive].
            /// </summary>
            public static int RandomRange(int min, int max, bool include = false)
            {
                InitializeRandSeedIfNecessary();
                return RANDOM.Next(min, include ? max + 1 : max);
                // return UnityEngine.Random.Range(min, max);
            }

            private static bool randInitialized = false;
            private static void InitializeRandSeedIfNecessary()
            {
                if (randInitialized) return;

                int seed = Environment.TickCount;

                // -- UNITY --
                UnityEngine.Random.InitState(seed);

                // -- SYSTEM --
                RANDOM = new System.Random(seed);

                randInitialized = true;
            }

            public static bool TryAdd<T>(this List<T> list, T element)
            {
                if (list == null) return false;
                if (list.Contains(element)) return false;
                list.Add(element);
                return true;
            }

            public static bool TryAdd<T, G>(this Dictionary<T, G> dictionary, T key, G value)
            {
                if (dictionary == null) return false;
                if (dictionary.ContainsKey(key)) return false;
                dictionary.Add(key, value);
                return true;
            }

            public static void AddOrUpdate<T, G>(this Dictionary<T, G> dictionary, T key, G value)
            {
                if (dictionary == null) return;
                if (key == null) return;
                if (!dictionary.ContainsKey(key))
                    dictionary.Add(key, value);
                else
                    dictionary[key] = value;
            }

            public static G TryGet<T, G>(this Dictionary<T, G> dictionary, T key, G defaultValue = default(G))
            {
                if (dictionary == null) return defaultValue;
                if (!dictionary.ContainsKey(key)) return defaultValue;
                return dictionary[key];
            }

            public static bool TrySet<T, G>(this Dictionary<T, G> dictionary, T key, G value, bool forceAdd = false)
            {
                if (dictionary == null) return false;
                if (!dictionary.ContainsKey(key))
                {
                    if (!forceAdd) return false;
                    return dictionary.TryAdd(key, value);
                }
                dictionary[key] = value;
                return true;
            }

            public static bool TryRemove<T>(this List<T> list, T element)
            {
                if (list == null) return false;
                if (!list.Contains(element)) return false;
                list.Remove(element);
                return true;
            }

            public static bool TryRemove<T, G>(this Dictionary<T, G> dictionary, T key)
            {
                if (dictionary == null) return false;
                if (!dictionary.ContainsKey(key)) return false;
                dictionary.Remove(key);
                return true;
            }

            public static IList<T> Shuffle<T>(this IList<T> list)
            {
                int n = list.Count;
                while (n > 1)
                {
                    n--;
                    int k = Utility_Helper.RandomRange(0, int.MaxValue) % (n + 1);
                    T value = list[k];
                    list[k] = list[n];
                    list[n] = value;
                }
                return list;
            }

            public static Queue<T> Shuffle<T>(this Queue<T> queue)
            {
                return new Queue<T>(queue.ToList().Shuffle());
            }

            public static int GetMaxIndex<T>(this IList<T> list) where T : IComparable
            {
                int indexMax
                   = !list.Any() ? -1 :
                   list
                   .Select((value, index) => new { Value = value, Index = index })
                   .Aggregate((a, b) => (a.Value.CompareTo(b.Value) > 0) ? a : b)
                   .Index;

                return indexMax;
            }

            // DOESN'T SORT
            /*
            public static Dictionary<T, G> Sort<T, G>(this Dictionary<T, G> dictionary, Order order = Order.Descending) where G : IComparable
            {
                var myList = dictionary.ToList();

                myList.Sort((pair1, pair2) => (order == Order.Descending ? -1 : 1) * pair1.Value.CompareTo(pair2.Value));

                return myList.ToDictionary(kVP => kVP.Key, kVP => kVP.Value);
            }
            */

            /// <summary>
            /// Returns the modul of the index (always in the list)
            /// </summary> 
            public static T GetMod<T>(this IList<T> iList, int id)
            {
                if (iList == null || iList.Count == 0)
                    return default(T);
                return iList[id % iList.Count];
            }

            /// <summary>
            /// Clamps the id within the bounds of the list
            /// </summary> 
            public static T GetSafe<T>(this IList<T> iList, int id)
            {
                if (iList == null || iList.Count == 0)
                    return default(T);
                return iList[Mathf.Clamp(id, 0, iList.Count - 1)];
            }


            /// <summary>
            /// Returns the last element of the list
            /// </summary> 
            public static T GetFirst<T>(this IList<T> iList)
            {
                if (iList == null || iList.Count == 0)
                    return default(T);
                return iList[0];
            }

            /// <summary>
            /// Returns the last element of the list
            /// </summary> 
            public static T GetLast<T>(this IList<T> iList)
            {
                if (iList == null || iList.Count == 0)
                    return default(T);
                return iList[iList.Count - 1];
            }

            /// <summary>
            /// Sets the list's capacity, resizing if necessary
            /// </summary> 
            public static void SetCapacity<T>(this List<T> list, int newCapacity)
            {
                // Check for resize
                if (newCapacity < list.Capacity && newCapacity < list.Count)
                    list.RemoveRange(list.Capacity - 1, newCapacity - list.Capacity);

                list.Capacity = newCapacity;
            }

            /// <summary>
            /// Uses the List's capacity to simulate a queue. Useful if you want all the goodies of a list.
            /// </summary> 
            public static void Enqueue<T>(this List<T> list, T value)
            {
                if (list.Count == list.Capacity)
                    list.Dequeue();
                list.Add(value);
            }

            /// <summary>
            /// Uses the List's capacity to simulate a queue. Useful if you want all the goodies of a list.
            /// </summary> 
            public static T Dequeue<T>(this List<T> list, bool FIFO = true)
            {
                if (list.Count == 0)
                    return default(T);

                T objToReturn = default(T);

                if (FIFO)
                {
                    objToReturn = list[0];
                    list.RemoveAt(0);
                }
                else
                {
                    objToReturn = list.GetLast();
                    list.RemoveAt(list.Count - 1);
                }

                return objToReturn;
            }

            public static void RemoveAt<T>(this T[] array, int value)
            {
                List<T> newArray = new List<T>();
                for (int i = 0; i < array.Length; i++)
                    if (i != value)
                        newArray.Add(array[i]);
                array = newArray.ToArray();
            }

            public static T[] Add<T>(this T[] array, T value)
            {
                List<T> newArray = new List<T>();
                for (int i = 0; i < array.Length; i++)
                    newArray.Add(array[i]);
                newArray.Add(value);
                return newArray.ToArray();
            }

            public static IList<T> FromReadOnly<T>(this IList<T> readOnlyContainer)
            {
                if (!readOnlyContainer.IsReadOnly)
                    return readOnlyContainer;

                IList<T> newContainer = new List<T>();
                foreach (T t in readOnlyContainer)
                    newContainer.Add(t);

                return newContainer;
            }

            public static IList<T> RemoveNull<T>(this IList<T> container)
            {
                List<int> nullIndices = new List<int>();

                for (int i = 0; i < container.Count; i++)
                    if (container[i].IsNull())
                        nullIndices.Add(i);

                if (nullIndices.Count > 0)
                    Debug_Helper.Log(typeof(Utility_Helper), nullIndices.ToReadableString());

                return container.RemoveIDs(nullIndices);
            }

            /*

            static bool IsNull<T>(this T obj)
            {
                bool isNull = obj == null || (obj is UnityEngine.Object && ((obj as UnityEngine.Object) == null));
                return isNull;
            }
            */
            public static IEnumerable<T> DistinctBy<T, TIdentity>(this IEnumerable<T> source, Func<T, TIdentity> identitySelector)
            {
                return source.Distinct(By(identitySelector));
            }

            public static IEqualityComparer<TSource> By<TSource, TIdentity>(Func<TSource, TIdentity> identitySelector)
            {
                return new DelegateComparer<TSource, TIdentity>(identitySelector);
            }

            private class DelegateComparer<T, TIdentity> : IEqualityComparer<T>
            {
                private readonly Func<T, TIdentity> identitySelector;

                public DelegateComparer(Func<T, TIdentity> identitySelector)
                {
                    this.identitySelector = identitySelector;
                }

                public bool Equals(T x, T y)
                {
                    return Equals(identitySelector(x), identitySelector(y));
                }

                public int GetHashCode(T obj)
                {
                    return identitySelector(obj).GetHashCode();
                }
            }

            public static IList<T> RemoveDuplicates<T>(this IList<T> container, IEqualityComparer<T> comparer = null)
            {
                container = (comparer == null ? container.Distinct() : container.Distinct(comparer)).ToList();
                return container;
            }

            public static IList<T> RemoveDuplicates<T, TIdentity>(this IList<T> container, Func<T, TIdentity> comparer = null)
            {
                container = (comparer == null ? container.Distinct() : container.DistinctBy(comparer)).ToList();
                return container;
            }

            public static IList<T> RemoveRange<T>(this IList<T> container, IList<T> rangeToRemove)
            {
                if (container.IsReadOnly)
                {
                    List<T> newContainer = new List<T>();
                    foreach (T t in container)
                        newContainer.Add(t);

                    return newContainer.RemoveRange(rangeToRemove);
                }

                foreach (T t in rangeToRemove)
                    container.Remove(t);

                return container;
            }

            /// <summary>
            /// x => x.sortingField
            /// </summary>
            public static List<T> CustomOrderBy<T, TKey>(this IList<T> container, Func<T, TKey> keySelector, Order order = Order.Ascending)
            {
                if (container == null) return null;
                if (order == Order.Descending)
                    return container.OrderByDescending(keySelector).ToList();
                return container.OrderBy(keySelector).ToList();
            }

            /// <summary>
            /// x => x.sortingField
            /// </summary>
            public static int CustomFindIndex<T>(this IList<T> container, T key)
            {
                if (container == null) return -1;
                return container.CustomToList().FindIndex(x => x.Equals(key));
            }

            public static IList<T> RemoveIDs<T>(this IList<T> container, IList<int> idsToRemove)
            {
                if (container.IsReadOnly)
                {
                    List<T> newContainer = new List<T>();
                    foreach (T t in container)
                        newContainer.Add(t);

                    return newContainer.RemoveIDs(idsToRemove);
                }

                for (int i = idsToRemove.Count - 1; i >= 0; i--)
                    container.RemoveAt(i);

                return container;
            }

            public static List<T> Aggregate<T>(this IList<T> list, IEnumerable<T> listB)
            {
                List<T> l = new List<T>(list);
                l.AddRange(listB);
                return l;
            }

            public static List<T> Degregate<T>(this IList<T> list, IList<T> listB)
            {
                List<T> l = new List<T>(list);
                l.RemoveRange(listB);
                return l;
            }

            public static bool Contains<T>(this T[] array, T element)
            {
                return array.ToList().Contains(element);
            }
            #endregion


            #region Conversions

            private static Regex _vector2ParseRegex = new Regex(@"[-+]?([0-9]*\.[0-9]+|[0-9]+)");
            private static MatchCollection _vector2ParseResults;

            public static Vector2 Vector2Parse(string data)
            {
                data = data.Replace("(", "").Replace(")", "").Replace(" ", "");
                string[]  _vector2ParseSplitString = data.Split(',');

                if(_vector2ParseSplitString.Length != 2)
                    return Vector2.one * -1;

                Vector2 _vector2ParseXY = new Vector2();
                _vector2ParseXY.x = Utility_Helper.ToFloat_FromCSV(_vector2ParseSplitString[0]);
                _vector2ParseXY.y = Utility_Helper.ToFloat_FromCSV(_vector2ParseSplitString[1]);
                return _vector2ParseXY;
            }

            // Using Regex for finding Vector2
            // Replaced by above method for performance issues
            public static Vector2 _Vector2Parse(string data)
            {
                _vector2ParseResults = _vector2ParseRegex.Matches(data);
                if (_vector2ParseResults.Count < 2 || !_vector2ParseResults[0].Success || !_vector2ParseResults[1].Success)
                {
                    //Debug.Log("Incorrect match for data:" + data);
                    return Vector2.one * -1;
                }

                Vector2 _vector2ParseXY = new Vector2();
                _vector2ParseXY.x = Utility_Helper.ToFloat_FromCSV(_vector2ParseResults[0].Value);
                _vector2ParseXY.y = Utility_Helper.ToFloat_FromCSV(_vector2ParseResults[1].Value);
                return _vector2ParseXY;
            }

            public static T ToEnum<T>(this string name, bool debug = true)
            {
                T e = default(T);
                ToEnum(name, out e, debug);
                return e;
            }

            public static bool ToEnum<T>(this string name, out T res, bool debug = true)
            {   
                try
                {
                    res = (T)Enum.Parse(typeof(T), name);
                    return true;
                }
                catch (Exception e)
                {
                    if (debug)
                        Debug_Helper.Log(typeof(Utility_Helper), e.ToString(), LogType.Exception);
                    res = default(T);
                    return false;
                }
            }

            public static int ToInt(this string name, int defaultValue = -1)
            {
                int val = defaultValue;
                if (int.TryParse(name, out val))
                    return val;
                else
                    return defaultValue;
            }

            public static long ToLong(this string name, long defaultValue = -1)
            {
                long val = defaultValue;
                if (long.TryParse(name, out val))
                    return val;
                else
                    return defaultValue;
            }

            public static float ToFloat(this string name, float defaultValue = -1)
            {
                float val = defaultValue;
                if (float.TryParse(name, out val))
                    return val;
                else
                    return defaultValue;
            }

            public static float ToFloat_FromCSV(string s)
            {
                // [HACK]
                double d = ToDouble_FromCSV(s);
                return (float)d;
                // return float.Parse(s.Replace(',', '.'));
            }

            // [200722] Making sure CSV is read properly ; ran into issues with various culture settings
            // Not elegant, not fast, but safe, gets the job done and isn't called anywhere where performance really matters
            public static double ToDouble_FromCSV(string s, double defaultValue = -1)
            {
                double returnValue = defaultValue;

                if (!s.Contains("."))
                    returnValue = s.ToLong();
                else
                {
                    string[] halves = s.Split('.');

                    long mainHalf = halves[0].ToLong();

                    int countOfTrailingZeroes = 0;
                    for (int i = halves[1].Length - 1; i >= 0; i--)
                        if (halves[1][i] == '0')
                            countOfTrailingZeroes++;
                        else
                            break;

                    string trimmedSecondHalf = halves[1].RemoveLast(countOfTrailingZeroes);

                    long secondHalf = trimmedSecondHalf.ToLong(0);
                    double divisor = Math.Pow(10, trimmedSecondHalf.Length);

                    returnValue = mainHalf;

                    if (s[0] == '-') // To handle cases like -0.321 where -0 would be cast as 0
                        returnValue -= secondHalf / divisor;
                    else
                        returnValue += secondHalf / divisor;
                }

                // if (returnValue.ToString() != s) Debug.LogError("S :: {0}\nR :: {1}"._Format(s, returnValue));

                return returnValue;
            }

            public static int GetNumDigits(double value)
            {
                if (value == 0)
                    return 1;

                return 1 + (int)Math.Floor(Math.Log10(Math.Abs(value)));
            }

            public static double ToDouble(this string s, double defaultValue = -1)
            {
                double val = defaultValue;
                if (double.TryParse(s, out val))
                    return val;
                else
                    return defaultValue;
            }

            public static TimeSpan SecondsToTimeSpan(this float seconds)
            {
                if (seconds < 0) seconds = 0;
                return TimeSpan.FromSeconds(seconds);
            }

            public static TimeSpan ToTimeSpan(this string s)
            {
                TimeSpan tS = new TimeSpan();
                if (TimeSpan.TryParse(s, out tS))
                    return tS;
                return new TimeSpan();
            }

            public static string ToHex(this Color color)
            {
                Color32 _color = (Color32)color;
                string hex = _color.r.ToString("X2") + _color.g.ToString("X2") + _color.b.ToString("X2") + _color.a.ToString("X2");
                return "#" + hex.ToLower();
            }

            public static Color ToColor(this string hex)
            {
                hex = hex.Replace("#", "");
                byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                byte a = 255;
                if (hex.Length >= 8)
                    a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
                return new Color32(r, g, b, a);
            }

            public static Color UpdateAlpha(this Color color, float alpha)
            {
                return new Color(color.r, color.g, color.b, alpha);
            }

            public static void CopyTo<T>(this IList<T> from, IList<T> to)
            {
                if (to == null)
                {
                    Debug_Helper.Log(typeof(Utility_Helper), "Null reference exception", LogType.Exception);
                    return;
                }

                to.Clear();

                foreach (T t in from)
                    to.Add(t);
            }

            public static string ToRoman(this int number)
            {
                if ((number < 0) || (number > 3999)) throw new ArgumentOutOfRangeException("insert value betwheen 1 and 3999");
                if (number < 1) return string.Empty;
                if (number >= 1000) return "M" + ToRoman(number - 1000);
                if (number >= 900) return "CM" + ToRoman(number - 900); //EDIT: i've typed 400 instead 900
                if (number >= 500) return "D" + ToRoman(number - 500);
                if (number >= 400) return "CD" + ToRoman(number - 400);
                if (number >= 100) return "C" + ToRoman(number - 100);
                if (number >= 90) return "XC" + ToRoman(number - 90);
                if (number >= 50) return "L" + ToRoman(number - 50);
                if (number >= 40) return "XL" + ToRoman(number - 40);
                if (number >= 10) return "X" + ToRoman(number - 10);
                if (number >= 9) return "IX" + ToRoman(number - 9);
                if (number >= 5) return "V" + ToRoman(number - 5);
                if (number >= 4) return "IV" + ToRoman(number - 4);
                if (number >= 1) return "I" + ToRoman(number - 1);
                throw new ArgumentOutOfRangeException("something bad happened");
            }

            public static string ToRank(this int number, bool zeroFirst)
            {
                if (zeroFirst)
                    number++;

                int d = number % 10;

                string suffix = "";

                switch (d)
                {
                    case 1:
                        suffix = "st";
                        break;

                    case 2:
                        suffix = "nd";
                        break;

                    case 3:
                        suffix = "rd";
                        break;

                    default:
                        suffix = "th";
                        break;
                }

                return "{0}{1}"._Format(number, suffix);
            }

            public static List<T> ToList<T>(this T[] array)
            {
                return new List<T>(array);
            }

            public static List<T> CustomToList<T>(this IList<T> iList)
            {
                return iList.ToList();
            }

            public static float ToFloat(this Direction_1D d1D)
            {
                if (d1D == Direction_1D.Left)
                    return -1;
                else
                    return 1;
            }

            public static Vector2 ToVector2(this Direction_2D d2D)
            {
                if (d2D == Direction_2D.Left)
                    return Vector2.left;
                else if (d2D == Direction_2D.Right)
                    return Vector2.right;
                else if (d2D == Direction_2D.Up)
                    return Vector2.up;
                else
                    return Vector2.down;
            }

            public static Vector2 ToVector2(this Direction_2D_Diagonal d2D_Diagonal)
            {
                if (d2D_Diagonal == Direction_2D_Diagonal.TopLeft)
                    return new Vector2(-1, 1).normalized;
                else if (d2D_Diagonal == Direction_2D_Diagonal.TopRight)
                    return new Vector2(1, 1).normalized;
                else if (d2D_Diagonal == Direction_2D_Diagonal.BottomLeft)
                    return new Vector2(-1, -1).normalized;
                else
                    return new Vector2(1, -1).normalized;
            }

            public static Vector3 ToVector3(this Direction_3D d3D)
            {
                if (d3D == Direction_3D.Left)
                    return Vector3.left;
                else if (d3D == Direction_3D.Right)
                    return Vector3.right;
                else if (d3D == Direction_3D.Up)
                    return Vector3.up;
                else if (d3D == Direction_3D.Down)
                    return Vector3.down;
                else if (d3D == Direction_3D.Forward)
                    return Vector3.forward;
                else
                    return Vector3.back;
            }

            public static Direction_3D ToClosestD3D(this Vector3 v3)
            {
                Dictionary<Direction_3D, float> dots = new Dictionary<Direction_3D, float>();
                foreach (Direction_3D d3D in EnumGetValues<Direction_3D>())
                    dots.Add(d3D, Vector3.Angle(v3, d3D.ToVector3()));

                return dots.OrderBy(a => a.Value).ToList()[0].Key;
            }

            public static Direction_2D ToClosestD2D(this Vector2 v2)
            {
                Dictionary<Direction_2D, float> dots = new Dictionary<Direction_2D, float>();
                foreach (Direction_2D d2D in EnumGetValues<Direction_2D>())
                    dots.Add(d2D, Vector2.Angle(v2, d2D.ToVector2()));

                return dots.OrderBy(a => a.Value).ToList()[0].Key;
            }

            public static Direction_2D_Diagonal ToClosestD2D_Diagonal(this Vector2 v2)
            {
                Dictionary<Direction_2D_Diagonal, float> dots = new Dictionary<Direction_2D_Diagonal, float>();
                foreach (Direction_2D_Diagonal d2D_Diag in EnumGetValues<Direction_2D_Diagonal>())
                    dots.Add(d2D_Diag, Vector2.Angle(v2, d2D_Diag.ToVector2()));

                return dots.OrderBy(a => a.Value).ToList()[0].Key;
            }

            public static string PercentileToPercent(this float f, string format = "#")
            {
                string preText = "";

                if (float.IsNaN(f))
                    return "n/a";

                if (f < 0.01f)
                    preText = "0";

                return preText + (f * 100).ToString(format) + "%";
            }

            public static string BoolToOnOff(this bool v)
            {
                return "<b>" + (v ? "on" : "off") + "</b>";
            }

            public static float ToKMH(this float speedMS)
            {
                return speedMS * 3600 / 1000;
            }

            public static float TotalSeconds(this DateTime dT)
            {
                return (float)dT.Subtract(Convert.ToDateTime("00:00:00")).TotalSeconds;
            }

            public static float ArcLengthToDegrees(this float length, float radius)
            {
                // All of it is 2πr and it correspnds to 360 degrees
                // So l/2πr <-> degrees/360
                // degrees = l / 2πr * 360
                return length / (2 * Mathf.PI * radius) * 360;
            }
            #endregion


            #region Checks & Actuators
            public static bool IsOverUIElement()
            {
                return EventSystem.current.IsPointerOverGameObject();
            }

            public static bool IsNull<T>(this T obj)
            {
                return EqualityComparer<T>.Default.Equals(obj, default(T));
            }

            public static bool IsNullOrEmpty(this string s)
            {
                return s == null || s == "";
            }

            public static bool IsNullOrEmpty<T>(this IList<T> array)
            {
                return array.IsNull() || array.Count == 0;
            }

            public static bool IsActive(this CanvasGroup cG)
            {
                return (cG.alpha > 0 && cG.interactable);
            }

            public static bool IsConvex(this Collider c)
            {
                return !(c as MeshCollider) || (c as MeshCollider).convex;
            }

            public static void SetConvex(this Collider c, bool on)
            {
                MeshCollider mC = c as MeshCollider;
                if (!mC) return;
                mC.convex = on;
            }

            public static void Toggle(this GameObject gO)
            {
                gO.SetActive(!gO.activeSelf);
            }

            /*
            public static void Toggle_On_Play_Toggle_Off(this ParticleSystem pS)
            {
                if (pS.isPlaying) pS.Stop();
                pS.Simulate(0.0f, true, true);
                pS.Toggle(true);
                pS.Play();
                StartTimer(pS.duration, success => { if (pS) { pS.Toggle(false); pS.Stop(); } });
            }

            public static void Toggle(this ParticleSystem pS, bool on)
            {
                foreach (ParticleSystem child in pS.GetComponentsInChildren<ParticleSystem>())
                {
                    ParticleSystem.EmissionModule pEM = child.emission;
                    pEM.enabled = on;
                    child.GetComponent<ParticleSystemRenderer>().enabled = on;
                    ParticleSystem.SubEmittersModule sE = child.subEmitters;
                    if (sE.birth0)
                        sE.birth0.Toggle(on);
                    if (sE.birth1)
                        sE.birth1.Toggle(on);
                    if (sE.collision0)
                        sE.collision0.Toggle(on);
                    if (sE.collision1)
                        sE.collision1.Toggle(on);
                    if (sE.death0)
                        sE.death0.Toggle(on);
                    if (sE.death1)
                        sE.death1.Toggle(on);
                }
            }
            */

            public static void ClearSelectedElement()
            {
                if (EventSystem.current == null) return;
                EventSystem.current.SetSelectedGameObject(null);
            }

            public static void SetLayer(this GameObject gO, int layer, bool includeChildren)
            {
                gO.layer = layer;
                if (!includeChildren) return;
                foreach (Transform t in gO.GetComponentsInChildren<Transform>())
                    t.gameObject.layer = layer;
            }
            #endregion


            #region Visibility
            /// <summary>
            /// Counts the bounding box corners of the given RectTransform that are visible from the given Camera in screen space.
            /// </summary>
            /// <returns>The amount of bounding box corners that are visible from the Camera.</returns>
            /// <param name="rectTransform">Rect transform.</param>
            /// <param name="camera">Camera.</param>
            private static int CountCornersVisibleFrom(this RectTransform rectTransform, Camera camera = null)
            {
                if (camera == null) camera = Camera.main;
                if (camera == null) camera = GetComponentInScene<Camera>();
                if (camera == null) { Debug_Helper.Log(typeof(Utility_Helper), "No valid camera found"); return -1; }

                Rect screenBounds = new Rect(0f, 0f, Screen.width, Screen.height); // Screen space bounds (assumes camera renders across the entire screen)
                Vector3[] objectCorners = new Vector3[4];
                rectTransform.GetWorldCorners(objectCorners);

                int visibleCorners = 0;
                Vector3 tempScreenSpaceCorner; // Cached
                for (var i = 0; i < objectCorners.Length; i++) // For each corner in rectTransform
                {
                    tempScreenSpaceCorner = camera.WorldToScreenPoint(objectCorners[i]); // Transform world space position of corner to screen space
                    if (screenBounds.Contains(tempScreenSpaceCorner)) // If the corner is inside the screen
                    {
                        visibleCorners++;
                    }
                }
                return visibleCorners;
            }

            /// <summary>
            /// Determines if this RectTransform is fully visible from the specified camera.
            /// Works by checking if each bounding box corner of this RectTransform is inside the cameras screen space view frustrum.
            /// </summary>
            /// <returns><c>true</c> if is fully visible from the specified camera; otherwise, <c>false</c>.</returns>
            /// <param name="rectTransform">Rect transform.</param>
            /// <param name="camera">Camera.</param>
            public static bool IsFullyVisibleFrom(this RectTransform rectTransform, Camera camera = null)
            {
                return CountCornersVisibleFrom(rectTransform, camera) == 4; // True if all 4 corners are visible
            }

            /// <summary>
            /// Determines if this RectTransform is at least partially visible from the specified camera.
            /// Works by checking if any bounding box corner of this RectTransform is inside the cameras screen space view frustrum.
            /// </summary>
            /// <returns><c>true</c> if is at least partially visible from the specified camera; otherwise, <c>false</c>.</returns>
            /// <param name="rectTransform">Rect transform.</param>
            /// <param name="camera">Camera.</param>
            public static bool IsVisibleFrom(this RectTransform rectTransform, Camera camera = null)
            {
                return CountCornersVisibleFrom(rectTransform, camera) > 0; // True if any corners are visible
            }

            public static bool TryGetBounds<T>(this T thing, out Bounds bounds)
            {
                bounds = new Bounds();

                // Is that thing attached to a gameObject? If so it must be a component
                Transform t = thing.GetComponent<Transform>();
                if (!t)
                {
                    GameObject gO = thing as GameObject;
                    if (!gO) return false;
                    t = gO.transform;
                }

                if (!t) return false;

                // Attempt collider bounds
                Collider c = t.GetComponent<Collider>();
                if (c)
                {
                    bounds = c.bounds;
                    return true;
                }

                // Attempt Renderer bounds
                Renderer r = t.GetComponent<Renderer>();
                if (r)
                {
                    bounds = r.bounds;
                    return true;
                }

                // Attempt UI bounds
                RectTransform rT = t.GetComponent<RectTransform>();
                if (rT)
                {
                    bounds = new Bounds(rT.rect.center, rT.rect.size);
                    return true;
                }

                // Return the scale
                bounds = new Bounds(t.position, t.lossyScale);
                return true;
            }
            #endregion


            #region Colors & Materials

            public static void SetEmissionColor(this Material m, Color c, float emissionLevel = 1)
            {
                m.SetColor("_EmissionColor", c * Mathf.LinearToGammaSpace(emissionLevel));
            }

            private static readonly Dictionary<ParticleSystem.MainModule, Vector3> initAlphas = new Dictionary<ParticleSystem.MainModule, Vector3>();
            public static void SetVisibility(this ParticleSystem pS, float alpha, bool basedOnInitAlpha = true)
            {
                ParticleSystem.MainModule main = pS.main;
                ParticleSystem.MinMaxGradient startColor = main.startColor;
                if (!initAlphas.ContainsKey(main))
                    initAlphas.Add(main, new Vector3(startColor.color.a, startColor.colorMin.a, startColor.colorMax.a));

                Vector3 alphaToApply = Vector3.one * alpha;
                if (basedOnInitAlpha)
                {
                    alphaToApply.MultiplyBy(initAlphas[main]);
                }

                startColor.color = startColor.color.Change(ColorProperty.a, alphaToApply.x);
                startColor.colorMin = startColor.colorMin.Change(ColorProperty.a, alphaToApply.y);
                startColor.colorMax = startColor.colorMax.Change(ColorProperty.a, alphaToApply.z);

                main.startColor = startColor;
            }

            public static void SetVisibility(this Renderer r, float alpha, float outlineMultiplier = 1)
            {
                alpha = alpha.Clamped01();

                if (alpha == 1)
                    r.TurnVisible(outlineMultiplier);
                else
                    r.TurnInvisible(alpha, outlineMultiplier);
            }

            private static void TurnInvisible(this Renderer r, float alpha, float outlineMultiplier = 1)
            {
                foreach (Material m in r.materials)
                    m.TurnInvisible(alpha, outlineMultiplier);
            }

            private static void TurnVisible(this Renderer r, float outlineMultiplier = 1)
            {
                foreach (Material m in r.materials)
                    m.TurnVisible(outlineMultiplier);
            }

            private static void TurnInvisible(this Material m, float alpha, float outlineMultiplier)
            {
                m.ChangeColorProperty(ColorProperty.a, alpha, outlineMultiplier);
                m.DisableKeyword("_EMISSION");
                m.ChangeMode(BlendMode.Fade);
            }

            private static void TurnVisible(this Material m, float outlineMultiplier = 1)
            {
                m.ChangeColorProperty(ColorProperty.a, 1, outlineMultiplier);

                if (m.HasProperty("_EmissionMap") && m.GetTexture("_EmissionMap") && !m.shaderKeywords.Contains("_EMISSION"))
                    m.EnableKeyword("_EMISSION");

                m.ChangeMode(BlendMode.Opaque);
            }

            public static void ChangeMode(this Material material, BlendMode blendMode)
            {
                switch (blendMode)
                {
                    case BlendMode.Opaque:
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                        material.SetInt("_ZWrite", 1);
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.DisableKeyword("_ALPHABLEND_ON");
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = -1;
                        break;
                    case BlendMode.Cutout:
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                        material.SetInt("_ZWrite", 1);
                        material.EnableKeyword("_ALPHATEST_ON");
                        material.DisableKeyword("_ALPHABLEND_ON");
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = 2450;
                        break;
                    case BlendMode.Fade:
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        material.SetInt("_ZWrite", 0);
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.EnableKeyword("_ALPHABLEND_ON");
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = 3000;
                        break;
                    case BlendMode.Transparent:
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        material.SetInt("_ZWrite", 0);
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.DisableKeyword("_ALPHABLEND_ON");
                        material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = 3000;
                        break;
                }
            }

            public static Color ColorifyHDR(this Color hdrColor, Color wantedColor)
            {
                float wantedBrightness = hdrColor.grayscale;
                Color colorifiedHDR = wantedColor.AdjustBrightness(wantedBrightness);
                return colorifiedHDR;
            }

            public static Color AdjustBrightness(this Color color, float newValue)
            {
                float currentBrightness = color.grayscale;
                if (currentBrightness == 0)
                    return new Color(newValue, newValue, newValue, color.a);
                // this.Log(string.Format("Brightness: {0} to {1}", currentBrightness, newValue));
                return new Color(
                    color.r / currentBrightness * newValue,
                    color.g / currentBrightness * newValue,
                    color.b / currentBrightness * newValue, color.a);
            }

            public static void ChangeColorProperty(this Material material, ColorProperty property, float newValue, float outlineMultiplier = 1)
            {
                if (material.HasProperty("_Color"))
                    material.SetColor("_Color", material.GetColor("_Color").Change(property, newValue));

                if (material.HasProperty("_TintColor"))
                    material.SetColor("_TintColor", material.GetColor("_TintColor").Change(property, newValue));

                if (material.HasProperty("_OutlineColor"))
                    material.SetColor("_OutlineColor", material.GetColor("_OutlineColor").Change(property, newValue * outlineMultiplier));
            }

            public static Color Change(this Color color, ColorProperty property, float newValue)
            {
                newValue = Mathf.Clamp(newValue, 0, 1);

                switch (property)
                {
                    case ColorProperty.r:
                        return new Color(newValue, color.g, color.b, color.a);
                    case ColorProperty.g:
                        return new Color(color.r, newValue, color.b, color.a);
                    case ColorProperty.b:
                        return new Color(color.r, color.g, newValue, color.a);
                    case ColorProperty.a:
                        return new Color(color.r, color.g, color.b, newValue);

                }
                return color;
            }

            public static Color InterpolateColor(Color colorA, Color colorB, float t)
            {
                t = Mathf.Clamp01(t);

                Color c = new Color();

                c.r = colorA.r * (1 - t) + colorB.r * t;
                c.g = colorA.g * (1 - t) + colorB.g * t;
                c.b = colorA.b * (1 - t) + colorB.b * t;
                c.a = colorA.a * (1 - t) + colorB.a * t;

                return c;
            }
            #endregion


            #region Audio
            private static float[] listenerOutputData;

            public static float GetCurrentVolume(Direction_1D leftRight)
            {
                if (listenerOutputData == null)
                    listenerOutputData = new float[4096];

                float result = 0;

                AudioListener.GetOutputData(listenerOutputData, (int)leftRight);
                result = listenerOutputData.GetSqrtOfSumOfSquares();
                return result;
            }

            public static void PlayRandom(this AudioSource source, AudioClip[] clips, bool interrupt, float volumeScale = 1)
            {
                if (clips == null)
                    return;

                AudioClip clip = clips.GetRandom();
                if (interrupt)
                    source.Stop();

                source.PlayOneShot(clip);
            }
            #endregion


            #region Coroutines
            public static Coroutine CheckInternetConnection(Action<bool> callback, string urlToCheck = "http://www.google.com")
            {
                if (Utility_Helper_MB.instance)
                    return Utility_Helper_MB.instance.StartCoroutine(Utility_Helper_MB.instance.
                        CheckInternetConnection(callback, urlToCheck));
                return null;
            }

            public static Coroutine SmoothLookAt(this Transform t, Vector3 pos, Vector3 up, float timeTo, Action<bool> callback = null)
            {
                if (Utility_Helper_MB.instance)
                    return Utility_Helper_MB.instance.StartCoroutine(Utility_Helper_MB.instance.
                        SmoothLookAt(t, pos, up, timeTo, ConflictResolutionStrategy.Interrupt, callback));
                return null;
            }

            /// <summary>
            /// [SOS] Local scale!
            /// </summary>
            /// <param name="speedInitMiddleNormalized">This is scaled by <paramref name="middle"/></param>
            /// <param name="speedMiddleFinalNormalized">This is scaled by <paramref name="middle"/></param>
            public static void Flash<T>(this T component, Vector3 baseScale, float init, float middle, float final, float speedInitMiddleNormalized, float speedMiddleFinalNormalized, Action<bool> callback = null) where T : Component
            {
                if (component == null)
                {
                    if (callback != null)
                        callback(false);
                    return;
                }

                component.transform.localScale = baseScale * init;

                float timeExplode = Mathf.Abs(middle - init) / (speedInitMiddleNormalized * middle);
                float timeImplode = Mathf.Abs(middle - final) / (speedMiddleFinalNormalized * middle);

                component.InterpolateScale(baseScale * middle, timeExplode, false, a =>
                {
                    component.InterpolateScale(baseScale * final, timeImplode, false, callback);
                });
            }

            /// <summary>
            /// [SOS] Local scale!
            /// </summary>
            public static Coroutine InterpolateScale<T>(this T component, Vector3 scaleTo, float timeTo, bool andBack, Action<bool> callback = null) where T : Component
            {
                if (component == null) return null;
                if (Utility_Helper_MB.instance)
                    return Utility_Helper_MB.instance.StartCoroutine(Utility_Helper_MB.instance.
                        InterpolateScale(component.gameObject, scaleTo, timeTo, andBack, ConflictResolutionStrategy.Interrupt, callback));
                return null;
            }

            public static Coroutine InterpolateVolume(this AudioSource aS, float volumeTo, float timeTo, bool andBack, Action<bool> callback = null)
            {
                if (Utility_Helper_MB.instance)
                    return Utility_Helper_MB.instance.StartCoroutine(Utility_Helper_MB.instance.
                        InterpolateVolume(aS, volumeTo, timeTo, andBack, ConflictResolutionStrategy.Interrupt, callback));
                return null;
            }

            public static Coroutine WaitUntil(Func<bool> predicate, Action<bool> callback, float timeout = -1)
            {
                if (!Utility_Helper_MB.instance) return null;

                return Utility_Helper_MB.instance.StartCoroutine(
                    Utility_Helper_MB.instance.WaitUntil(predicate, callback, timeout));
            }

            /// <summary>
            /// Pass in timesToCall negative to call endlessly
            /// success => { if(!this) return; this.Log(success? "Success" : "Fail"); 
            /// </summary>
            public static Coroutine StartTimer(float dT, Action<bool> callback, int timesToCall = 1)
            {
                if (dT <= 0)
                {
                    for (int i = 0; i < timesToCall; i++)
                        if (callback != null)
                            callback(true);
                    return null;
                }
                if (Utility_Helper_MB.instance)
                    return Utility_Helper_MB.instance.StartCoroutine(Utility_Helper_MB.instance.
                        StartTimer(dT, callback, timesToCall));
                return null;
            }

            public static void StopCoroutine(this Coroutine c)
            {
                if (!Utility_Helper_MB.instance || c == null) return;
                Utility_Helper_MB.instance.StopCoroutine(c);
            }

            public static void LoadAudio(string url, Action<bool, AudioClip> callback, bool debug = false)
            {
                if (Utility_Helper_MB.instance)
                    Utility_Helper_MB.instance.LoadAudio(url, callback, debug);
            }

            public static void LoadAudio(Dictionary<string, string> keyUrls, Action<bool, Dictionary<string, AudioClip>> callback, bool debug = false)
            {
                if (Utility_Helper_MB.instance)
                    Utility_Helper_MB.instance.LoadAudio(keyUrls, callback, debug);
            }
            #endregion


            #region Editor
            /// <summary>
            /// [SOS] Only functions in Editor
            /// </summary>
            public static string GetPathToSelected()
            {
                string path = "";

#if UNITY_EDITOR

                foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
                {
                    path = AssetDatabase.GetAssetPath(obj);

                    if (!File.Exists(path)) continue;

                    // This if you want the parent folder
                    // path = Path.GetDirectoryName(path);

                    break;
                }
#endif

                return path;
            }

            /// <summary>
            /// [SOS] Only functions in Editor
            /// </summary>
            public static void PingObject<T>(string path)
            {
                System.Type type = typeof(T);
                UnityEngine.Object obj = GetObjectAtPath(path, type);
                if (obj == null)
                {
                    Debug_Helper.Log(typeof(Utility_Helper), "No object of type {0} at {1}"._Format(type, path), LogType.Error);
                    return;
                }
                PingObject(obj);
            }

            /// <summary>
            /// [SOS] Only functions in Editor
            /// </summary>
            public static void PingObject(UnityEngine.Object obj)
            {
#if UNITY_EDITOR
                if (obj == null)
                {
                    Debug_Helper.Log(typeof(Utility_Helper), "Object was null", LogType.Error);
                    return;
                }
                EditorGUIUtility.PingObject(obj);
                obj.Log("Ping!");
#endif
            }

            /// <summary>
            /// [SOS] Only functions in Editor
            /// </summary>
            private static UnityEngine.Object GetObjectAtPath(string path, System.Type type)
            {
#if UNITY_EDITOR
                return AssetDatabase.LoadAssetAtPath(path, type);
#else
                return null;
#endif
            }


#if UNITY_EDITOR
            public static Vector2 EditorWindowSize = Vector2.zero;

            // C#
            public static Vector2 GetEditorWindowSize()
            {
                if (EditorWindowSize != Vector2.zero)
                    return EditorWindowSize;
                System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
                System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                System.Object Res = GetSizeOfMainGameView.Invoke(null, null);
                EditorWindowSize = (Vector2)Res;
                return EditorWindowSize;
            }
#endif

            #endregion

            
            // To avoid GC
            private static Vector2 _lastObjectScreenPos;

            /// <summary>
            /// [SOS] Affected by <see cref="SetScreenOffsetAndScale(Vector2, Vector2)"/>
            /// </summary>
            public static bool IsObjectInsideOfCameraVisibility(GameObject go, float marginPercentile)
            {
                _lastObjectScreenPos = GetRelativeScreenPosition(go.transform.position);
                bool isObjectInsideScreen = (_lastObjectScreenPos.x < 1f + marginPercentile && _lastObjectScreenPos.x > 0f - marginPercentile) &&
                                            (_lastObjectScreenPos.y < 1f + marginPercentile && _lastObjectScreenPos.y > 0f - marginPercentile);
                return isObjectInsideScreen;
            }

            private static Vector2 _screenSize = Vector2.zero;
            public static bool _IsObjectOutOfCameraVisibility(GameObject go, float marginPercentile)
            {
                if (Camera.main == null) return true;
                _lastObjectScreenPos = Camera.main.WorldToScreenPoint(go.transform.position);
                if (_screenSize == Vector2.zero)
                    _screenSize = new Vector2(Screen.width, Screen.height);
#if UNITY_EDITOR
                _screenSize = Utility_Helper.GetEditorWindowSize();
#endif
                bool isObjectOutOfScreen = ((_lastObjectScreenPos.x > _screenSize.x * (1f + marginPercentile) || _lastObjectScreenPos.x < -_screenSize.x * (0f + marginPercentile)) ||
                                        (_lastObjectScreenPos.y > _screenSize.y * (1f + marginPercentile) || _lastObjectScreenPos.y < -_screenSize.y * (0f + marginPercentile)));
                return isObjectOutOfScreen;
            }

            public static LeftRight ToLeftRight(this Direction_2D_Diagonal dir)
            {
                return dir == Direction_2D_Diagonal.BottomLeft || dir == Direction_2D_Diagonal.TopLeft ? LeftRight.Left : LeftRight.Right;
            }

            public static TopBottom ToTopBottom(this Direction_2D_Diagonal dir)
            {
                return dir == Direction_2D_Diagonal.BottomLeft || dir == Direction_2D_Diagonal.BottomRight ? TopBottom.Bottom : TopBottom.Top;
            }

            #region Other
            public static DateTime UtcNow
            {
                get
                {
                    return DateTime.UtcNow;
                }
            }

            public static DateTime GetAverage(this IList<DateTime> dateTimes)
            {
                int count = dateTimes.Count;
                double temp = 0D;
                for (int i = 0; i < count; i++)
                {
                    temp += dateTimes[i].Ticks / (double)count;
                }
                DateTime average = new DateTime((long)temp);

                return average;
            }

            public static DateTime GetMedian(this IList<DateTime> dateTimes)
            {
                List<DateTime> ordered = dateTimes.CustomOrderBy(x => x.Ticks);
                int count = dateTimes.Count;
                if (count % 2 == 0)
                {
                    List<DateTime> toAvg = new List<DateTime>();
                    toAvg.Add(ordered[count / 2]);
                    toAvg.Add(ordered[count / 2 - 1]);
                    return toAvg.GetAverage();
                }
                else
                    return ordered[(count - 1) / 2];
            }

            /*
            public static string UtcNow_FileNameSafe { get { return UtcNow_DB.Replace(':', '-'); } }
            public static string Now_FileNameSafe { get { return Now_DB.Replace(':', '-'); } }
            public static string UtcNow_DB { get { return UtcNow.ToString("yyyy-MM-dd HH:mm:ss"); } }
            public static string Now_DB { get { return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); } }
            public static string UtcNow_ddMMyy { get { return UtcNow.ToString("ddMMyy"); } }
            public static string Now_ddMMyy { get { return DateTime.Now.ToString("ddMMyy"); } }
            public static string UtcNow_MMddHHmm { get { return UtcNow.ToString("MMddHHmm"); } }
            public static string Now_yyMMddHHmmss { get { return DateTime.Now.ToString("MMddHHmm"); } }
            public static int UtcNow_TimestampSecBased { get { return UtcNow.ToTimestampSecBased(); } }
            */

            // MaxValue :: 59,999 (used for negmoding DT Differences)
            // (59 sec, 999 ms -> 60 sec - 1ms -> 60 * 1000 - 1)
            public static int ToTimestampSecBased(this DateTime dT)
            {
                return GetTimestampSecBased(dT.Second, dT.Millisecond);
            }

            public static int GetTimestampSecBased(int seconds, int milliseconds)
            {
                int timestamp = 0;

                timestamp += milliseconds;
                timestamp += seconds * 1000;

                return timestamp;
            }

            public static int TimestampSecBasedDiff(this int timestampEnd, int timestampBegin)
            {
                return (timestampEnd - timestampBegin).NegMod(59999);
            }

            public static Color GetRandomColor()
            {
                Vector3 v3 = Math_Helper.GetRandomVector3(0, float.MaxValue);
                return new Color(v3.x % 1, v3.y % 1, v3.z % 1);
            }

            public static void ChangeAnimClipOfState(this Animator m_animator, string animatorState, AnimationClip animationClip)
            {
                AnimatorOverrideController myOverrideController = new AnimatorOverrideController();
                myOverrideController.runtimeAnimatorController = m_animator.runtimeAnimatorController;

                myOverrideController[animatorState] = animationClip;
                // Put this line at the end because when you assign a controller on an Animator, unity rebind all the animated properties

                m_animator.runtimeAnimatorController = myOverrideController;
            }

            public static void CopyToClipboard(this string s)
            {
                TextEditor te = new TextEditor();
                te.text = s;
                te.SelectAll();
                te.Copy();
            }
            #endregion
        }

        public enum FilesFolders { Files, Folders, FilesAndFolders }

        public enum MinOrMax { Min = 0, Max }

        public enum SmoothType { None, EaseInOut, EaseIn, EaseOut, Exp, Sqrt }

        public enum LeftRight { Left = 0, Right }

        public enum TopBottom { Top = 0, Bottom }

        public enum Direction_1D { Left, Right }

        public enum Direction_2D { Left, Right, Up, Down }

        public enum Direction_2D_Diagonal { TopLeft = 0, TopRight, BottomRight, BottomLeft }

        public enum Direction_3D { Left, Right, Up, Down, Forward, Backward }

        public enum ColorProperty { r, g, b, a }

        public enum BlendMode { Opaque, Cutout, Fade, Transparent }

        public enum KeepOrRemove { Keep = 0, Remove }

        public enum Order { Ascending = 0, Descending }

        public enum ToggleMode { On, Off, Swap }

        public enum RequestType { Instant = 0, NextFrame = 1 }

        public enum ConsumeOrAcquire { Consume, Acquire }
    }
}
