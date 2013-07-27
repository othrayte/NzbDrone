using System.Text;
using System.Text.RegularExpressions;

namespace NzbDrone.Common
{
    public static class StringExtensions
    {
        public static string Inject(this string format, params object[] formattingArgs)
        {
            return string.Format(format, formattingArgs);
        }

        private static readonly Regex InvalidCharRegex = new Regex(@"[^a-z0-9\s-]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex InvalidSearchCharRegex = new Regex(@"[^a-z0-9\s-\.]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex CollapseSpace = new Regex(@"\s+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static string ToSlug(this string phrase)
        {
            phrase = phrase.RemoveAccent().ToLower();

            phrase = InvalidCharRegex.Replace(phrase, string.Empty);
            phrase = CollapseSpace.Replace(phrase, " ").Trim();
            phrase = phrase.Replace(" ", "-");

            return phrase;
        }

        public static string ToSearchTerm(this string phrase)
        {
            phrase = phrase.RemoveAccent().ToLower();

            phrase = phrase.Replace("&", "and");
            phrase = InvalidSearchCharRegex.Replace(phrase, string.Empty);
            phrase = CollapseSpace.Replace(phrase, " ").Trim();
            phrase = phrase.Replace(" ", "+");
            
            return phrase;
        }

        public static string RemoveAccent(this string txt)
        {
            var bytes = Encoding.GetEncoding("ascii").GetBytes(txt);
            return Encoding.ASCII.GetString(bytes);
        }
    }
}