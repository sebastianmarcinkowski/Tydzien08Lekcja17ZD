using System;
using System.Text.RegularExpressions;

namespace EmailSender.Extensions
{
    public static class StringExtensions
    {
        public static string StripHtml(this string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }
    }
}
