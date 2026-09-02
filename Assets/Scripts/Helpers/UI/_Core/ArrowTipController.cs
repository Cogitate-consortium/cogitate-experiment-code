using System.Collections.Generic;
using UnityEngine;

namespace Helpers.UI.Core
{
    public class ArrowTipController : MonoBehaviour
    {
#if UNITY_EDITOR
        // Debug
        public List<Transform> targetObjects;
        public LineRenderer axisRenderer;
#endif

        public SpriteRenderer arrowTip;
        public float radius = 1f;
        public float lerp = 0;
        public int index = 0;

        public void PointTo(int index, Transform targetObject)
        {
            arrowTip.gameObject.SetActive(true);

            this.index = index;

            float angle = 0;
            if (index == 0)
                angle = -45;
            else if (index == 1)
                angle = 45;
            else if (index == 2)
                angle = 135;
            else if (index == 3)
                angle = 225;

            // Circular Position/Direction
            Vector3 circularPosition = AngleToCircleUnit(angle).normalized * radius;
            Vector3 _circularRotation = new Vector3(0, 0, -angle);
            Quaternion circularRotation = Quaternion.Euler(_circularRotation);


            // Target object Position/Direction
            Vector3 targetDirection = (targetObject.position - this.transform.position).normalized;
            targetDirection.z = 0;
            Vector3 targetPosition = targetDirection * radius;

            Vector3 _targetRotation = new Vector3(0, 0, 0);
            _targetRotation.z = Vector3.Angle(targetDirection, Vector3.up);
            float dot = Vector3.Dot(targetDirection, Vector3.up);
            _targetRotation.z += (Vector3.Dot(targetDirection, Vector3.right) < 0f) ? 0 : -120;

            _targetRotation.z = Vector3.Angle(targetDirection, Vector3.up);
            if (targetDirection.x > 0)
                _targetRotation.z *= -1;

            Quaternion targetRotation = Quaternion.Euler(_targetRotation);
            //Debug.Log(string.Format("Circular:{0} | Target:{1}", AngleToCircleUnit(angle).normalized.magnitude, targetObject.position - this.transform.position).normalized.magnitude));
            //Debug.Log(string.Format("Circular:{0} | Target:{1}", circularPosition.magnitude, targetPosition.magnitude));


            Vector3 finalPosition = Vector3.Lerp(circularPosition, targetPosition, lerp);
            Quaternion finalRotation = Quaternion.Lerp(circularRotation, targetRotation, lerp);

#if UNITY_EDITOR
            if (axisRenderer != null)
            {
                axisRenderer.positionCount = 2;
                axisRenderer.SetPosition(0, Vector3.zero);
                axisRenderer.SetPosition(1, (finalPosition - Vector3.zero).normalized * 100f);
            }
#endif

            //Debug.Log(Vector3.Distance(this.transform.position, finalPosition));
            arrowTip.transform.localPosition = finalPosition;
            arrowTip.transform.localRotation = finalRotation;
        }

        private void PointTipTo(float angle, out Vector3 position)
        {
            position = AngleToCircleUnit(angle) * radius;
        }

        public void Hide()
        {
            arrowTip.gameObject.SetActive(false);
        }

        private static Vector2 AngleToCircleUnit(float angle)
        {
            float rad = Mathf.Deg2Rad * angle;
            Vector2 pos = Vector2.zero;
            pos.x = Mathf.Sin(rad);
            pos.y = Mathf.Cos(rad);
            return pos;
        }

#if UNITY_EDITOR

        // Editor Debug
        [ContextMenu("Point to object")]
        public void PointToObject()
        {
            //int index = 0;
            Vector3 direction = (targetObjects[index].position - this.transform.position).normalized;
            Vector3 pos = direction * radius;

            Vector3 rotation = new Vector3(0, 0, 0);
            rotation.z = Vector3.Angle(direction, Vector3.up);
            arrowTip.transform.localPosition = pos;
            arrowTip.transform.localRotation = Quaternion.Euler(rotation);
        }

        [ContextMenu("Adjust Arrow Tip")]
        public void PointTo()
        {
            PointTo(index, targetObjects[index]);
        }

#endif
    }
}