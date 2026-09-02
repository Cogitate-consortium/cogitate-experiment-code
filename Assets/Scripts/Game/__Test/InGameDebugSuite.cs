using UnityEngine;

namespace Game.Test
{
    public class InGameDebugSuite : MonoBehaviour
    {
        public bool useDebugSuite = false;

        private void Awake()
        {
#if UNITY_EDITOR
            if (useDebugSuite)
            {
                Debug.Log("In-Game Debug Suite enabled");
                return;
            }
#endif
            Destroy(gameObject);
        }
        // Update is called once per frame
        void Update()
        {

        }
    }
}