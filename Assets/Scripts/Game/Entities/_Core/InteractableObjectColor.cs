using Game.Core;
using UnityEngine;

namespace Game.Entities.Core
{
    /// <summary>
    /// Used by both <see cref="Player"/> (DUET) and <see cref="Interactables"/>
    /// </summary>
    public class InteractableObjectColor : MonoBehaviour
    {
        public ObjectColorType objectColor;
    }
}