using Experiment;
using TGP.Helpers;
using UnityEngine;

namespace ExperimentLibrary.Test
{
    /// <summary>
    /// [RENAME] To something with TESTING
    /// </summary>
    public class EditorOnlyUtilities : MonoBehaviour
    {
        // Check bool? == true, means if there is no instance this won't go through
        public static bool? mainMenu_AutoCompleteDetails { get { return instance?._mainMenu_AutoCompleteDetails; } }
        [SerializeField] private bool _mainMenu_AutoCompleteDetails = false;

        // Check bool? == true, means if there is no instance this won't go through
        public static ModuleType? moduleTypeOverride { get { return instance?._moduleTypeOverride; } }
        [SerializeField] private ModuleType _moduleTypeOverride = ModuleType.None;

        // Check bool? == true, means if there is no instance this won't go through
        public static bool? serialPort_ProcessKeyboardInput { get { return instance?._serialPort_ProcessKeyboardInput; } }
        [SerializeField] private bool _serialPort_ProcessKeyboardInput = false;

        // Check bool? == true, means if there is no instance this won't go through
        public static Vector3Int? sequences_massGenerationFromTo { get { return instance?._sequences_doMassGeneration == true ? instance?._sequences_massGenerationFromTo : null; } }
        [SerializeField] private bool _sequences_doMassGeneration = false;
        [SerializeField] private Vector3Int _sequences_massGenerationFromTo = new Vector3Int(0, 32, -1);

        // Check bool? == true, means if there is no instance this won't go through
        public static bool? applicationLibrary_SaveConfigOnStart { get { return instance?._applicationLibrary_SaveConfigOnStart; } set { if (instance) instance._applicationLibrary_SaveConfigOnStart = value == true; } }
        [SerializeField] private bool _applicationLibrary_SaveConfigOnStart = false;

        private static EditorOnlyUtilities instance = null;

        [SerializeField] private bool _soundSystem_DEBUG_LISTENER = false;
        float[] data = new float[1000];
        private void FixedUpdate()
        {
            TryDebugListener();
        }
        private void TryDebugListener()
        {
            if (!_soundSystem_DEBUG_LISTENER) return;

            AudioListener.GetOutputData(data, 0);
            float l = data.GetSqrtOfSumOfSquares();
            AudioListener.GetOutputData(data, 1);
            float r = data.GetSqrtOfSumOfSquares();
            if (l > 0)
                Debug.LogError("L: " + l + "\nR: " + r);
            else
                Debug.Log("L: " + l + "\nR: " + r);
        }

        private void Awake()
        {
            // When outside the editor, we want instance = null, s.t. all the bool? will never be true
#if !UNITY_EDITOR
        Destroy(this);
#else
            if (instance) Destroy(this);
            instance = this;
            DontDestroyOnLoad(gameObject);
#endif
        }
    }
}