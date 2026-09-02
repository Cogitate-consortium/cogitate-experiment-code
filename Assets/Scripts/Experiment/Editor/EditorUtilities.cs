using UnityEditor;
using UnityEditor.SceneManagement;

namespace Experiment.Editor
{
    public class EditorUtilities
    {
        [MenuItem("Seattle/Scenes/Preloader")]
        public static void LoadScenePreloader()
        {
            LoadScene("Preloader");
        }

        [MenuItem("Seattle/Scenes/Analyzer")]
        public static void LoadSceneAnalyzer()
        {
            LoadScene("FullLogAnalyzer");
        }

        private static void LoadScene(string sceneName)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.OpenScene("Assets/Scenes/" + sceneName + ".unity");
        }
    }
}