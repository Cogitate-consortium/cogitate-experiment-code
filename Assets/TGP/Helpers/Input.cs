using UnityEngine;

namespace TGP
{
    namespace Helpers
    {
        public static class Input_Helper
        {
            /// <summary>
            /// Feed it the stick (2D input) - it will break it down to speed and rot. Parameters refer to angles from the input's "UP" or "FWD".
            /// </summary>
            /// <param name="fwdSpeed">To affect the character's relative forward movement.</param>
            /// <param name="yRotation">To affect the character's Y rotation.</param>
            /// <param name="fullSpeedZone">Angles less than this will not have a speed reduction.</param>
            /// <param name="justRotationAngle">Approaching this angle, speed will reach 0.</param>
            /// <param name="fullBackMovementZone">Angles more than this will not suffer a speed reduction (negative speeds).</param>
            public static void BreakMovementInput_TopDown_FwdSpeedAndYRotation(this Vector2 input, out float fwdSpeed, out float yRotation,
                float fullSpeedZone = 70, float justRotationAngle = 125, float fullBackMovementZone = 160)
            {
                fwdSpeed = 0;
                yRotation = 0;

                // Break the input into cartessian (angle from "UP" and magnitude)
                float theta = Math_Helper.AngleSigned(Vector2.up, input);
                float r = input.magnitude;

                // Figure out the rotation
                if (Mathf.Abs(theta) < justRotationAngle)
                    yRotation = theta / justRotationAngle;
                else
                    yRotation = Mathf.Sign(theta) * (180 - Mathf.Abs(theta)) / (180 - justRotationAngle);

                // Figure out the speed based on the zone we're in
                if (Mathf.Abs(theta) < fullSpeedZone)
                    fwdSpeed = r;
                else if (Mathf.Abs(theta) < justRotationAngle)
                    fwdSpeed = r * (justRotationAngle - Mathf.Abs(theta)) / (justRotationAngle - fullSpeedZone);
                else if (Mathf.Abs(theta) < fullBackMovementZone)
                    fwdSpeed = Mathf.Sign(input.y) * r * (Mathf.Abs(theta) - justRotationAngle) / (180 - justRotationAngle);
                else
                    fwdSpeed = -r;
            }

            public static int GetNumericalChoiceUp()
            {
                if (Input.GetKeyUp(KeyCode.Alpha0)) return 0;
                else if (Input.GetKeyUp(KeyCode.Alpha1)) return 1;
                else if (Input.GetKeyUp(KeyCode.Alpha2)) return 2;
                else if (Input.GetKeyUp(KeyCode.Alpha3)) return 3;
                else if (Input.GetKeyUp(KeyCode.Alpha4)) return 4;
                else if (Input.GetKeyUp(KeyCode.Alpha5)) return 5;
                else if (Input.GetKeyUp(KeyCode.Alpha6)) return 6;
                else if (Input.GetKeyUp(KeyCode.Alpha7)) return 7;
                else if (Input.GetKeyUp(KeyCode.Alpha8)) return 8;
                else if (Input.GetKeyUp(KeyCode.Alpha9)) return 9;
                return int.MinValue;
            }

            public static int GetNumericalChoice()
            {
                if (Input.GetKey(KeyCode.Alpha0)) return 0;
                else if (Input.GetKey(KeyCode.Alpha1)) return 1;
                else if (Input.GetKey(KeyCode.Alpha2)) return 2;
                else if (Input.GetKey(KeyCode.Alpha3)) return 3;
                else if (Input.GetKey(KeyCode.Alpha4)) return 4;
                else if (Input.GetKey(KeyCode.Alpha5)) return 5;
                else if (Input.GetKey(KeyCode.Alpha6)) return 6;
                else if (Input.GetKey(KeyCode.Alpha7)) return 7;
                else if (Input.GetKey(KeyCode.Alpha8)) return 8;
                else if (Input.GetKey(KeyCode.Alpha9)) return 9;
                return int.MinValue;
            }

            public static int GetNumericalChoiceDown()
            {
                if (Input.GetKeyDown(KeyCode.Alpha0)) return 0;
                else if (Input.GetKeyDown(KeyCode.Alpha1)) return 1;
                else if (Input.GetKeyDown(KeyCode.Alpha2)) return 2;
                else if (Input.GetKeyDown(KeyCode.Alpha3)) return 3;
                else if (Input.GetKeyDown(KeyCode.Alpha4)) return 4;
                else if (Input.GetKeyDown(KeyCode.Alpha5)) return 5;
                else if (Input.GetKeyDown(KeyCode.Alpha6)) return 6;
                else if (Input.GetKeyDown(KeyCode.Alpha7)) return 7;
                else if (Input.GetKeyDown(KeyCode.Alpha8)) return 8;
                else if (Input.GetKeyDown(KeyCode.Alpha9)) return 9;
                return int.MinValue;
            }
        }
    }
}
