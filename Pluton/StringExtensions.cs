using System;

namespace Pluton
{
    public static class StringExtensions
    {
        public static string BoldText(this string self)
        {
            return String.Format("<b>self</b>", self);
        }

        public static string ColorText(this string self, string color)
        {
            return String.Format("<color=#{0}>{1}</color>", color, self);
        }

        public static string ItalicText(this string self)
        {
            return String.Format("<i>self</i>", self);
        }

        public static string SetSize(this string self, int size)
        {
            return String.Format("<size={0}>{1}</size>", size, self);
        }

        public static string SetSize(this string self, string size)
        {
            return String.Format("<size={0}>{1}</size>", size, self);
        }

        public static string QuoteSafe(this string self)
        {
            return global::UnityEngine.StringExtensions.QuoteSafe(self);
        }
    }
}

