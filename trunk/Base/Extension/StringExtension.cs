using System;
using System.Collections.Generic;
using System.Linq;

namespace Base.Extension.StringExtension
{
    /// <summary>
    /// Summary description for StringExtension
    /// </summary>
    public static class StringExtension
    {
        public static string ReverseStr(this string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        /// <summary>
        /// Compares 2 IList of string
        /// Returns true if Lists have the same items with the same order
        /// </summary>
        /// <param name="firstList"></param>
        /// <param name="secondList"></param>
        /// <returns></returns>
        public static bool CompareIListString(this IList<string> firstList, IList<string> secondList)
        {
            if (firstList.Count != secondList.Count) return false;
            for (int i = 0; i < firstList.Count; i++)
                if (firstList[i] != secondList[i]) return false;
            return true;
        }


        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        public static string ToCommaString(this string[] arrayString)
        {
            string retStr = "";
            foreach (string str in arrayString)
            {
                retStr += str + ",";
            }
            retStr = retStr.TrimEnd(',');
            return retStr;
        }

        public static string ApplyYeKeReverse(this string input)
        {
            if (input.Contains("ي") || input.Contains("ك"))
            {
                return string.IsNullOrEmpty(input) ? input : input.Replace("ي", "ی").Replace("ك", "ک");
            }
            if (input.Contains("ی") || input.Contains("ک"))
            {
                return string.IsNullOrEmpty(input) ? input : input.Replace("ی", "ي").Replace("ک", "ك");
            }
            return "";

        }

        public static string ApplyYeKe(this string input)
        {
            if (input.Contains("ي") || input.Contains("ك"))
            {
                return string.IsNullOrEmpty(input) ? input : input.Replace("ي", "ی").Replace("ك", "ک");
            }
            return input;

        }


        public static string NamedStringFormat(this string aString, object replacements)
        {
            Dictionary<string, object> keyValues = Base.TypeConverter.AnonymousToDictionary(replacements);
            foreach (var item in keyValues)
            {
                aString = aString.Replace("{" + item.Key + "}", (item.Value ?? "").ToString());
            }
            return aString;

        }

        public static string NamedStringFormatSQL(this string aString, object replacements)
        {
            Dictionary<string, object> keyValues = Base.TypeConverter.AnonymousToDictionary(replacements);
            foreach (var item in keyValues)
            {
                string val = item.Value == null ? "null" : item.Value.ToString();
                aString = aString.Replace("@" + item.Key, val);
            }
            return aString;

        }
    }
}