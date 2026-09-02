using System;
using System.Collections.Generic;
using UnityEngine;

namespace Peripherals.UserInput.Core
{
    public static class ExtendedKeyCodeHelper
    {
        public static string CustomToString(this IList<KeyCode> keyCodes)
        {
            string s = "";
            string separator = " / ";
            foreach (KeyCode kC in keyCodes)
                s += kC + separator;
            return s.Substring(0, s.Length - separator.Length);// drop last separator
        }
    }

    [System.Serializable]
    public struct ExtendedKeyCode
    {
        /// <summary>
        /// Returns a COPY of the internal key codes of this struct. To change those, use the constructor
        /// </summary>
        public List<KeyCode> keyCodes { get { return new List<KeyCode>(_keyCodes); } }
        [SerializeField] private List<KeyCode> _keyCodes;

        /// <summary>
        /// NS_RENAME to full description
        /// [SOS] JSON variable only. Use <see cref="ToString"/> instead.
        /// </summary>
        public string responseBoxDescription;

        public ExtendedKeyCode(params KeyCode[] keyCodes)
        {
            _keyCodes = new List<KeyCode>(keyCodes);
            responseBoxDescription = "";
        }

        public ExtendedKeyCode(IList<KeyCode> keyCodes)
        {
            _keyCodes = new List<KeyCode>(keyCodes);
            responseBoxDescription = "";
        }

        /// <summary>
        /// [SOS] Should only be used by static accessors of <see cref="InputManager"/> like <see cref="ApplicationLibrary.Config.moveLeft_String"/>
        /// </summary>
        /// <param name="fullText"></param>
        /// <returns></returns>
        public string GetDescription(bool fullText)
        {
            return fullText ? responseBoxDescription : keyCodes.CustomToString().Replace("Alpha", "");
        }

        public override bool Equals(object obj)
        {
            return keyCodes.Equals(obj);
        }

        public override int GetHashCode()
        {
            return keyCodes.GetHashCode();
        }

        public static bool operator ==(ExtendedKeyCode lhs, KeyCode rhs) { return lhs._keyCodes.Contains(rhs); }
        public static bool operator !=(ExtendedKeyCode lhs, KeyCode rhs) { return !(lhs == rhs); }

        public static bool operator ==(KeyCode lhs, ExtendedKeyCode rhs) { return rhs == lhs; }
        public static bool operator !=(KeyCode lhs, ExtendedKeyCode rhs) { return !(lhs == rhs); }

        public static bool operator ==(ExtendedKeyCode lhs, ExtendedKeyCode rhs)
        {
            foreach (KeyCode lhs_Kc in lhs.keyCodes)
                if (!rhs._keyCodes.Contains(lhs_Kc))
                    return false;

            foreach (KeyCode rhs_Kc in rhs.keyCodes)
                if (!lhs._keyCodes.Contains(rhs_Kc))
                    return false;

            return true;
        }

        public static bool operator !=(ExtendedKeyCode lhs, ExtendedKeyCode rhs) { return !(lhs == rhs); }

        public static implicit operator ExtendedKeyCode(KeyCode kC) => new ExtendedKeyCode(kC);
        // public static implicit operator KeyCode(ExtendedKeyCode eKC) => eKC.keyCodes; (no longer applies to the arrays)

        public static implicit operator ExtendedKeyCode(List<KeyCode> kC) => new ExtendedKeyCode(kC);
        public static implicit operator List<KeyCode>(ExtendedKeyCode eKC) => eKC.keyCodes;

        public static implicit operator ExtendedKeyCode(KeyCode[] kC) => new ExtendedKeyCode(kC);
        public static implicit operator KeyCode[](ExtendedKeyCode eKC) => eKC.keyCodes.ToArray();

        /// <summary>
        /// [SOS] Use <see cref="GetDescription(bool)"/> instead
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Debug.isDebugBuild)
                Debug.LogError("Should not be called! Use GetDescription(bool)");
            return base.ToString();
        }
    }
}