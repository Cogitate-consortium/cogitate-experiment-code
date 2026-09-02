using Helpers.Engine;
using UnityEngine;

namespace Game.Entities.Interactables
{
    /// <summary>
    /// [DEPRECATED] ?
    /// </summary>
    public class RotateAround : MonoBehaviour
    {
        public Vector3 speed = new Vector3();

        // Update is called once per frame
        void Update()
        {
            Vector3 currentRotation = this.transform.localRotation.eulerAngles;
            currentRotation += speed * TimeWrapper.deltaTime_SinceLastUpdate_NotTS;
            transform.localRotation = Quaternion.Euler(currentRotation);
        }
    }
}