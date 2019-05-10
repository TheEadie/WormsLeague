using System;
using System.Collections.Generic;

namespace WormsRandomizer.Flags
{
    internal static class ParseUtils
    {
        public static void MatchAny(string arg, string prefix, IEnumerable<string> options, Action<string> onMatch)
        {
            foreach (var option in options)
            {
                if (string.Equals(arg, prefix + option.Replace(" ",""), StringComparison.OrdinalIgnoreCase))
                {
                    onMatch(option);
                }
            }
        }

        public static void MatchBool(string arg, string match, Action<bool> onMatch)
        {
            if (string.Equals(arg, "+" + match, StringComparison.OrdinalIgnoreCase))
            {
                onMatch(true);
            }
            else if (string.Equals(arg, "-" + match, StringComparison.OrdinalIgnoreCase))
            {
                onMatch(false);
            }
        }

        public static void MatchAnyBool(string arg, string prefix, IEnumerable<string> options, Action<string, bool> onMatch)
        {
            foreach (var option in options)
            {
                MatchBool(arg, prefix + option, b => onMatch(option, b));
            }
        }

        public static void MatchNumber(string arg, string prefix, Action<int> onMatch)
        {
            if (!arg.ToUpper().StartsWith(prefix.ToUpper()))
            {
                return;
            }

            var numberPart = arg.Substring(prefix.Length);
            if (int.TryParse(numberPart, out var result))
            {
                onMatch(result);
            }
        }

        public static string DescribeAny(string flag, string description)
        {
            return $"{flag}: {description}";
        }

        public static string DescribeBool(string flag, string description, bool defaultValue)
        {
            var defaultValueString = defaultValue ? "+" : "-";
            return $"[+/-]{flag}: {description} (default: {defaultValueString})";
        }

        public static string DescribeNumber(string flag, string description, int defaultValue)
        {
            return $"{flag}[N]: {description} (default: {defaultValue})";
        }
    }
}