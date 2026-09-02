using System;

namespace Game.Core
{
    // [SOS] Flags must follow binary numbering to function
    [Flags]
    public enum ObjectColorType
    {
        Gray = 0,
        Orange = 1,
        Blue = 2,
        OrangeHigh = 4,
        BlueHigh = 8,
        GrayOrange = 16,
        GrayBlue = 32,
    }

    public enum WorldType { Blue = 0, Orange = 1 }
    public enum GameType { None = -1, Runner_2D_Blue = 0, Runner_2D_Orange = 1, Duet = 2 }

    public static class Utilities
    {
        public static WorldType Invert_TS(this WorldType type)
        {
            return (WorldType)(1 - (int)type);
        }
    }
}