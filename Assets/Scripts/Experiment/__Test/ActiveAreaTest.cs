using UnityEngine;

namespace ExperimentLibrary.Test
{
    public class ActiveAreaTest : MonoBehaviour
    {
        // Update is called once per frame
        void Update()
        {
            Rect activeArea = ExperimentLibraryManager.Config.Experiment.stimulus.background.GetActiveAreaWorld(Camera.main);
            bool isInside = ExperimentLibraryManager.Config.Experiment.stimulus.background.IsInsideActiveArea(Camera.main, transform);

            Debug.Log(string.Format("{0} -> {1} ({2})", transform.position, isInside, activeArea), gameObject);
        }
    }
}