using UnityEngine;

namespace Helpers.Themes
{
    [CreateAssetMenu(fileName = "NewTheme", menuName = "Theme")]
    public class ThemePalette : ScriptableObject
    {
        public Color DarkTone_1;
        public Color DarkTone_2;
        public Color MidTone;
        public Color LightTone_1;
        public Color LightTone_2;
    }
}