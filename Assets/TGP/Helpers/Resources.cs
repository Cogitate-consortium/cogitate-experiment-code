using System;
using System.Collections.Generic;
using UnityEngine;

namespace TGP
{
    namespace Helpers
    {
        public static class Resources_Helper
        {
            #region Game Objects
            public static GameObject PrefabInstantiateAsChild(this GameObject gO, GameObject prefab, bool isUI = false)
            {
                if (prefab == null)
                {
                    Debug_Helper.Log(typeof(Resources_Helper), "Provided prefab was null!", LogType.Exception);
                    return null;
                }
                GameObject child = UnityEngine.Object.Instantiate(prefab,
                    gO.transform.position, gO.transform.rotation) as GameObject;
                child.transform.SetParent(gO.transform, !isUI);
                if (isUI)
                {
                    child.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    child.GetComponent<RectTransform>().localPosition = Vector3.zero;
                }
                child.name = "{0} (Clone)"._Format(prefab.name);
                return child;
            }

            public static GameObject ResourceInstantiateAsChild_GameObject(this GameObject gO, string filePath, bool isUI = false)
            {
                GameObject child = ResourceInstantiate_GameObject(filePath, gO.transform.position, gO.transform.rotation);
                child.transform.SetParent(gO.transform, !isUI);
                if (isUI)
                    child.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                return child;
            }

            public static GameObject ResourceInstantiate_GameObject(string filePath)
            {
                return ResourceInstantiate_GameObject(filePath, Vector3.zero, Quaternion.identity);
            }

            public static GameObject ResourceInstantiate_GameObject(string filePath, Vector3 position, Quaternion rotation)
            {
                GameObject prefab = Resources.Load<GameObject>(filePath);
                if (!prefab)
                {
                    if (Debug.isDebugBuild)
                        Debug.LogError("Could not instantiate " + filePath);
                    return null;
                }
                GameObject gO = GameObject.Instantiate(prefab, position, rotation) as GameObject;
                gO.name = filePath;
                return gO;
            }
            #endregion


            #region Generic
            public static T[] ResourceLoadAll_Under<T>(string folderPath) where T : UnityEngine.Object
            {
                // Grab the exercise's audio clip
                List<T> resources = new List<T>();

                // We want the same cues for left and right exercises
                foreach (object o in Resources.LoadAll(folderPath))
                {
                    T nextResource = o as T;
                    if (nextResource == null) continue;
                    resources.Add(nextResource);
                }

                return resources.ToArray();
            }

            public static T[] ResourceLoadAll<T>(string mainName, int initIndex = 0, int maxIndex = 100) where T : UnityEngine.Object
            {
                List<T> resources = new List<T>();
                int i = initIndex;
                while (true && i < maxIndex)
                {
                    T resource = ResourceLoad<T>(mainName + i, false);
                    i++;
                    if (!resource)
                        break;
                    resources.Add(resource);
                }
                return resources.ToArray();
            }

            /// <summary>
            /// Loads all resources under the base name named ie. Dust0, Dust1, .. until it doesn't find one to load. Returns the loaded objects
            /// </summary> 
            public static List<T> ResourceLoadNumbered<T>(string baseName, int initNumber = 0, int maxNumber = -1) where T : UnityEngine.Object
            {
                List<T> resources = new List<T>();

                int i = 0;

                while (true && (maxNumber < 0 || i <= maxNumber))
                {
                    T nextResource = ResourceLoad<T>(baseName + i.ToString());
                    if (nextResource == null)
                        break;
                    resources.Add(nextResource);
                    i++;
                }

                return resources;
            }


            public static T ResourceLoad<T>(string filePath, bool debug = true) where T : UnityEngine.Object
            {
                T resource = Resources.Load<T>(filePath) as T;
                if (resource == null && debug)
                    Debug_Helper.Log(typeof(Resources_Helper), "Couldn't find a resource of type {0} at {1}"._Format(typeof(T), filePath), LogType.Exception);
                return resource;
            }

            public static T ResourceInstantiate<T>(string filePath) where T : UnityEngine.Object
            {
                return UnityEngine.Object.Instantiate(ResourceLoad<T>(filePath)) as T;
            }
            #endregion
        }
    }
}
