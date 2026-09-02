using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace TGP
{
    namespace Helpers
    {
        public static class String_Helper
        {
            public static string SecondsToDuration(this int seconds)
            {
                TimeSpan span = TimeSpan.FromSeconds(seconds);

                bool showDays = span.Duration().Days > 0;
                bool showHours = showDays || span.Duration().Hours > 0;
                bool showMinutes = showHours || span.Duration().Minutes > 0;
                bool showSeconds = showMinutes || span.Duration().Seconds > 0;

                // Hide seconds if we have days (wanna show up to three fields)
                showSeconds &= !showDays;

                string formatted = string.Format("{0}{1}{2}{3}",
                    showDays    ? string.Format("{0:0} day{1}, ", span.Days, span.Days == 1 ? String.Empty : "s") : string.Empty,
                    showHours   ? string.Format("{0:0} hour{1}, ", span.Hours, span.Hours == 1 ? String.Empty : "s") : string.Empty,
                    showMinutes ? string.Format("{0:0} minute{1}, ", span.Minutes, span.Minutes == 1 ? String.Empty : "s") : string.Empty,
                    showSeconds ? string.Format("{0:0} second{1}", span.Seconds, span.Seconds == 1 ? String.Empty : "s") : string.Empty);

                if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

                if (string.IsNullOrEmpty(formatted)) formatted = "-"; // "0 seconds";

                return formatted;
            }

            public static string ToReadableString(this KeyCode keyCode)
            {
                string str = keyCode.ToString();
                str = str.Replace("Alpha", "");
                return str;
            }

            public static string RemoveTabs(this string str, bool alsoReduceMultiSpaces = true)
            {
                const string reduceMultiSpace = @"[ ]{2,}";
                return Regex.Replace(str.Replace("\t", " "), reduceMultiSpace, " ");
            }

            /// <summary>
            /// Assumes a default words per minute reading of 100 (suitable for children)
            /// </summary>
            public static float GetAverageReadingTime(this string msg)
            {
                // Even "!" needs a second to notice and process
                float minReactionTime = 1f;

                float averageWordsPerMinute = 100;
                float averageSecondsPerWord = 1 / (averageWordsPerMinute / 60);
                float averageWordLength = 6;

                float totalTime = minReactionTime;
                foreach (string word in msg.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries))
                {
                    // Add another word
                    totalTime += averageSecondsPerWord * 0.7f + 0.3f * word.Length / averageWordLength;
                }

                return totalTime;
            }
            
            public static string AddSpacesBeforeCapitals(this string input)
            {
                return input.AddSubstringBeforeCapitals(" ");
            }

            public static string AddSubstringBeforeCapitals(this string input, string substring)
            {
                return Regex.Replace(input, @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", substring + "$0");
            }

            public static string AddApostropheBeforeS(this string input)
            {
                return Regex.Replace(input, @"s ", "'s ");
            }
            
            public static string RemoveSpaces(this string input)
            {
                // From http://net-informations.com/q/faq/remove.html
                return Regex.Replace(input, @"\s", "");
            }

            public static string ToColoredHex(this Color c)
            {
                return c.ToHex().Colorize(c);
            }

            public static string Colorize(this string s, Color c)
            {
                return "<color=" + c.ToHex() + ">" + s + "</color>";
            }

            public static bool ContainsInvariant(this string s, string subString)
            {
                return s.ToLowerInvariant().Contains(subString.ToLowerInvariant());
            }

            public static bool EqualsInvariant(this string s, string otherString)
            {
                return s.ToLower() == otherString.ToLower();
            }

            public static string RemoveLast(this string str, int lastHowMany = 1)
            {
                if (string.IsNullOrEmpty(str))
                    return str;
                else if (str.Length <= lastHowMany)
                    return "";
                else
                    return str.Substring(0, str.Length - lastHowMany);
            }

            public static string PrependHTTP(this string s)
            {
                if (!s.ContainsInvariant("http") ||
                    !s.ContainsInvariant("://"))
                    s = "http://{0}"._Format(s);

                return s;
            }

            public static string ToBold(this string s)
            {
                return "<b>{0}</b>"._Format(s);
            }

            public static string _Format(this string s, params object[] args)
            {
                return string.Format(s, args);
            }

            public static string _FormatBold(this string s, params object[] args)
            {
                return string.Format(s.Replace("{", "<b>{").Replace("}", "}</b>"), args);
            }

            public static string ToStringPrecise(this Vector2 vector)
            {
                return string.Format("({0}, {1})", vector.x.ToString("0.00000000"), vector.y.ToString("0.00000000"));
            }

            public static string AddLeadingSymbols(this int number, int wantedDigits, char c = '_')
            {
                // Add as many zeros as needed to have everything aligned
                string zeros = "";
                int currentDigits = number.GetNumDigits();
                int leadingZeros = wantedDigits - currentDigits;

                for (int i = 0; i < leadingZeros; i++)
                    zeros += c;

                zeros += number;

                return zeros;
            }

            public static int CountOccurences(this string haystack, string needle)
            {
                // https://stackoverflow.com/a/542001/4786381
                return (haystack.Length - haystack.Replace(needle, "").Length) / needle.Length;
            }

            public static string CapitalizeFirst(this string s)
            {
                if (s.IsNullOrEmpty())
                {
                    Debug_Helper.Log(typeof(String_Helper), "Empty / null string provided");
                    return s;
                }

                // Not much to do
                if (s.Length == 0)
                    return s.ToUpper();

                return s[0].ToString().ToUpper() + s.Substring(1, s.Length - 1).ToLower();
            }

            public static string[] Split(this string s, bool removeEmptyEntries, params string[] splitters)
            {
                return s.Split(splitters, removeEmptyEntries ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
            }

            public static bool Contains(this IList<string> array, string value)
            {
                foreach (string s in array)
                    if (s == value)
                        return true;

                return false;
            }

            public static bool ContainsInvariant(this IList<string> array, string value)
            {
                foreach (string s in array)
                    if (s.ContainsInvariant(value))
                        return true;

                return false;
            }

            public static bool IsValidEmail(this string s)
            {
                return new Regex(@"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*"
                                            + "@"
                                            + @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$").Match(s).Success;
            }
        }
    }
}
