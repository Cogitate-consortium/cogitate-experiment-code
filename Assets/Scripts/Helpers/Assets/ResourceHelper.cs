using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TGP.Helpers;

namespace Helpers.Assets
{
    public static class ResourceHelper
    {
        private static readonly Dictionary<string, Object> resources = new Dictionary<string, Object>();

        public static T InstantiateResource<T>(string path) where T : Object
        {
            T resource = LoadResource<T>(path);
            if (resource == null)
            {
                Debug_Helper.LogWarning(typeof(ResourceHelper), "Can't instantiate null resource! ({0})"._Format(path));
                return default(T);
            }

            return Object.Instantiate(resource) as T;
        }

        public static T LoadResource<T>(string path) where T : Object
        {
            if (!resources.ContainsKey(path))
                resources.Add(path, null);
            if (resources[path] == null)
                resources[path] = Resources.Load(path);

            if (resources[path] == null)
            {
                Debug_Helper.LogWarning(typeof(ResourceHelper), "No resource found at {0}"._Format(path));
                return default(T);
            }

            return resources[path] as T;
        }

        public static T[] LoadAllResourcesUnder<T>(string folderPath) where T : Object
        {
            List<T> loadedResources = new List<T>();

            foreach (T resource in Resources.LoadAll<T>(folderPath))
            {
                string path = "{0}/{1}"._Format(folderPath, resource.name);

                if (!resources.ContainsKey(path))
                    resources.Add(path, null);
                if (resources[path] == null)
                    resources[path] = resource;

                loadedResources.Add(resource);
            }

            return loadedResources.ToArray();
        }
    }
}