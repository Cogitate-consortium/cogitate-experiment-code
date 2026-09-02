// NS_REMOVE | Shouldn't access Core directly
using Game.Core;

using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// [DEPRECATE]
    /// [RENAME] at the very least if we keep it. no idea what this does
    /// </summary>
    public class ScoreItemDuetPrefab : MonoBehaviour
    {
        public GameObject blueOrb;
        public GameObject orangeOrb;

        public ObjectColorType unlockedTypes;

        public bool HasColorType(ObjectColorType objectColorType)
        {
            return unlockedTypes.Contains(objectColorType);
        }

        public bool IsComplete()
        {
            return HasColorType(ObjectColorType.BlueHigh) && HasColorType(ObjectColorType.OrangeHigh);
        }

        public void SetEmpty()
        {
            blueOrb.SetActive(false);
            orangeOrb.SetActive(false);
            unlockedTypes = ObjectColorType.Gray;
        }

        public void Unlock(ObjectColorType objectColorType)
        {
            if (unlockedTypes == ObjectColorType.Gray)
                unlockedTypes = objectColorType;
            else if (!unlockedTypes.Contains(objectColorType))
                unlockedTypes |= objectColorType;

            if (objectColorType == ObjectColorType.BlueHigh)
                blueOrb.SetActive(true);
            else if (objectColorType == ObjectColorType.OrangeHigh)
                orangeOrb.SetActive(true);
        }

    }
}