// https://stackoverflow.com/questions/3261451/using-a-bitmask-in-c-sharp
// The casts to object in the below code are an unfortunate necessity due to
// C#'s restriction against a where T : Enum constraint. (There are ways around
// this, but they're outside the scope of this simple illustration.)
using System.Collections.Generic;
using TGP.Helpers;

public static class FlagHelper
{
    public static bool Contains<T>(this T flags, T flag) where T : struct
    {
        int flagsValue = (int)(object)flags;
        int flagValue = (int)(object)flag;

        return (flagsValue & flagValue) != 0;
    }

    public static T Add<T>(this T flags, T flag) where T : struct
    {
        int flagsValue = (int)(object)flags;
        int flagValue = (int)(object)flag;

        flags = (T)(object)(flagsValue | flagValue);
        return flags;
    }

    public static T Remove<T>(this T flags, T flag) where T : struct
    {
        int flagsValue = (int)(object)flags;
        int flagValue = (int)(object)flag;

        flags = (T)(object)(flagsValue & (~flagValue));
        return flags;
    }

    public static T Random<T>(this T flags) where T :struct
    {
        List<T> flagArray = new List<T>();
        foreach(T _flag in TGP.Helpers.Utility_Helper.EnumGetValues<T>())
        {
            if (flags.Contains(_flag))
                flagArray.Add(_flag);
        }
        return flagArray.GetRandom();
    }
}
