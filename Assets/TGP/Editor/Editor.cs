using System.IO;
using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

namespace TGP
{
    namespace Helpers
    {
        public static class Editor_Helper
        {
            [MenuItem("Window/TGP Helpers/[TEST]")]
            public static void Test()
            {
                Utility_Helper.PingObject<Texture>("Assets/AvatarManagement/Content/Barbarian/Barbarian 6/Shared/Texture/collar_A.tga");
            }

            /*
            // taken from: http://answers.unity3d.com/questions/282959/set-inspector-lock-by-code.html
            [MenuItem("Tools/Toggle Inspector Lock #w")] // Shift + W
            static void ToggleInspectorLock()
            {
                ActiveEditorTracker.sharedTracker.isLocked = !ActiveEditorTracker.sharedTracker.isLocked;
                ActiveEditorTracker.sharedTracker.ForceRebuild();
            }
            
            private static bool FAST_MOVE = true;

            [MenuItem("Tools/Toggle Inspector Lock #s")] // Ctrl + L
            static void MoveToShared()
            {
                if (!FAST_MOVE) return;
                MoveTo("_Shared");
                PingNextItemBRT();
            }

            [MenuItem("Tools/Toggle Inspector Lock #c")] // Ctrl + L
            static void MoveToClearence()
            {
                if (!FAST_MOVE) return;
                MoveTo("Clearence");
                PingNextItemBRT();
            }

            [MenuItem("Tools/Toggle Inspector Lock #g")] // Ctrl + L
            static void MoveToGraveyard()
            {
                if (!FAST_MOVE) return;
                MoveTo("Graveyard");
                PingNextItemBRT();
            }

            [MenuItem("Tools/Toggle Inspector Lock #l")] // Ctrl + L
            static void MoveToLobby()
            {
                if (!FAST_MOVE) return;
                MoveTo("Lobby");
                PingNextItemBRT();
            }

            [MenuItem("Tools/Toggle Inspector Lock #v")] // Ctrl + L
            static void MoveToVillage()
            {
                if (!FAST_MOVE) return;
                MoveTo("Village");
                PingNextItemBRT();
            }

            [MenuItem("Tools/Toggle Inspector Lock %z")] // Ctrl + Z
            static void UnMove()
            {
                if (!FAST_MOVE) return;

                string ret = AssetDatabase.MoveAsset(lastTo, lastFrom);
                if (ret == "")
                {
                    Debug.Log("Material asset moved to {0}"._Format(lastFrom));
                    // AssetDatabase.Refresh();
                }
                else
                    Debug.Log(ret);

                PingLastItemBRT();
            }

            static void PingLastItemBRT()
            {
                if (!FAST_MOVE) return;

                AssetList.PingLastItem();
            }

            static void PingNextItemBRT()
            {
                if (!FAST_MOVE) return;

                AssetList.PingNextItem();
            }

            private static string basePrefabPath = "Assets/_GameAreas/Environments/{0}/{1}/{2}.{3}";
            private static string lastFrom = "";
            private static string lastTo = "";

            private static void MoveTo(string area)
            {
                // Grab the last 
                string path = AssetDatabase.GetAssetPath(Selection.activeObject);

                string ending = path.Split('.').GetLast();

                string folder = "";
                if (ending == "prefab")
                    folder = "Prefabs";
                else if (ending == "mat")
                    folder = "Materials";
                else if (ending == "png" || ending == "jpg" || ending == "tga")
                    folder = "Textures";
                else if (ending == "fbx")
                    folder = "Meshes";

                string destination = basePrefabPath._Format(area, folder, Selection.activeObject.name, ending);

                string ret = AssetDatabase.MoveAsset(path, destination);
                if (ret == "")
                {
                    lastFrom = path;
                    lastTo = destination;
                    Debug.Log("Material asset moved to {0}"._Format(destination));
                    // AssetDatabase.Refresh();
                }
                else
                    Debug.Log(ret);
            }
            */

            [MenuItem("Window/TGP Helpers/Export Package")]
            public static void ExportPackage()
            {
                string[] projectContent = new string[] { "Assets/TGP" };
                AssetDatabase.ExportPackage(projectContent, "_TGP.Helpers.unitypackage", ExportPackageOptions.Interactive | ExportPackageOptions.Recurse);
                Debug_Helper.Log(typeof(Editor_Helper), "Helpers Exported");
            }

            [MenuItem("uGUI/All Anchors to Corners")]
            static void AllAnchorsToCorners()
            {
                if (!EditorUtility.DisplayDialog("Are you sure you want to move all anchors to corners?",
                    "This will change all the UI elements in the scene", "Move", "Do NOT move"))
                {
                    Debug_Helper.Log(typeof(Editor), "Aborted all anchors to corners");
                    return;
                }

                foreach (RectTransform rT in Utility_Helper.GetComponentsInScene<RectTransform>())
                    rT.AnchorToCorners();
            }

            [MenuItem("uGUI/Anchors to Corners %[")]
            static void AnchorsToCorners()
            {
                RectTransform t = Selection.activeTransform as RectTransform;
                t.AnchorToCorners();
            }

            // [MenuItem("uGUI/Corners to Anchors %]")]
            static void CornersToAnchors()
            {
                RectTransform t = Selection.activeTransform as RectTransform;

                if (t == null) return;

                t.offsetMin = t.offsetMax = new Vector2(0, 0);
            }

            /*
            [MenuItem("Editor Tools/Export Package with Project Settings")]
            public static void ExportPackage()
            {
                string[] projectContent = new string[] { "Assets/Gameplay", "ProjectSettings/InputManager.asset" };
                AssetDatabase.ExportPackage(projectContent, "_Gameplay.unitypackage", ExportPackageOptions.Interactive | ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);
                Debug_Helper.Log(typeof(Editor_Helper), "Project Exported");
            }
            */
        }
    }
}
