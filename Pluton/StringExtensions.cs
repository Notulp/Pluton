using System;

namespace Pluton
{
    public static class StringExtensions
    {
        public static string ColorText(this string self, string color)
        {
            return String.Format("<color=#{0}>{1}</color>", color, self);
        }

        public static string QuoteSafe(this string self)
        {
            return global::UnityEngine.StringExtensions.QuoteSafe(self);
        }
    }
}

