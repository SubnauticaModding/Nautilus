using System.Text.RegularExpressions;

namespace SMLHelper.V2.ExtensionMethods
{
    internal static class SystemExtensions
    {
        private static Regex xmlRegex = new Regex("<.*?>", RegexOptions.Compiled);
        public static string StripXML(this string source)
           => xmlRegex.Replace(source, string.Empty);
    }
}
