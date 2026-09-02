// NS_DEBATABLE
using Helpers.Assets;

using System.Collections.Generic;

namespace Helpers.Themes
{
    public class ThemeLibrary
    {
        private const string THEMES_PATH = "Themes";

        public List<ThemePalette> themes = new List<ThemePalette>();

        public ThemePalette defaultTheme;

        public ThemeLibrary()
        {
            ThemePalette[] _themes = ResourceHelper.LoadAllResourcesUnder<ThemePalette>(THEMES_PATH);
            themes.AddRange(_themes);
            defaultTheme = themes[0];
        }
    }
}