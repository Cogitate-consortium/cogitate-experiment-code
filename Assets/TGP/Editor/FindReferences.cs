using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace TGP
{
    public static class FindReferences
    {
        [MenuItem("Tools/Toggle Inspector Lock %f")] // Ctrl + F
        public static void SetSearchFilter_CurrentItem_Name()
        {
            // Grab the selected item 
            UnityEngine.Object obj = Selection.activeObject;
            if (obj == null) return;

            SetSearchFilter(obj.name, HierarchyFilterMode.All);
        }

        [MenuItem("Tools/Toggle Inspector Lock #&f")] // Ctrl + Shift + F
        public static void SetSearchFilter_CurrentItem_References()
        {
            // Grab the selected item 
            UnityEngine.Object obj = Selection.activeObject;
            if (obj == null) return;

            string path = AssetDatabase.GetAssetPath(obj);

            // Trim the beginning, otherwise it doesn't work
            path = path.Remove(0, "Assets/".Length);

            string searchTerm = "ref: " + path;
            SetSearchFilter(searchTerm, HierarchyFilterMode.All);
        }

        // Original from https://stackoverflow.com/questions/29575964/setting-a-hierarchy-filter-via-script, switched to enums
        public static void SetSearchFilter(string filter, HierarchyFilterMode filterMode)
        {

            SearchableEditorWindow[] windows = (SearchableEditorWindow[])Resources.FindObjectsOfTypeAll(typeof(SearchableEditorWindow));
            SearchableEditorWindow hierarchy = null;
            foreach (SearchableEditorWindow window in windows)
            {

                if (window.GetType().ToString() == "UnityEditor.SceneHierarchyWindow")
                {

                    hierarchy = window;
                    break;
                }
            }

            if (hierarchy == null)
                return;

            MethodInfo setSearchType = typeof(SearchableEditorWindow).GetMethod("SetSearchFilter", BindingFlags.NonPublic | BindingFlags.Instance);
            object[] parameters = new object[] { filter, (int)filterMode, false };

            setSearchType.Invoke(hierarchy, parameters);
        }

        /*
        public const int FILTERMODE_ALL = 0;
        public const int FILTERMODE_NAME = 1;
        public const int FILTERMODE_TYPE = 2;
        */
        public enum HierarchyFilterMode { All = 0, Name = 1, Type = 2 }
    }
}