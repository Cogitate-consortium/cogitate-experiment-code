using UnityEngine;

namespace Helpers.Misc
{
    /// {CHECK}
    /// <summary>
    /// http://elvers.us/perception/visualAngle/ to verify
    /// </summary>
    public static class VisualAngleMath
    {
        public static float CalculateVisualAngle(float size, float distance)
        {
            return 2 * Mathf.Atan(size / (2 * distance)) * Mathf.Rad2Deg;
        }

        public static float CalculateSize(float visualAngle, float distance)
        {
            return 2 * distance * Mathf.Tan((visualAngle * Mathf.Deg2Rad) / 2);
        }

        public static float CalculateDistance(float visualAngle, float size)
        {
            return (size / 2) / Mathf.Tan((visualAngle * Mathf.Deg2Rad) / 2);
        }
    }
}