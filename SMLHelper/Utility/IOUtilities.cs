namespace SMLHelper.V2.Utility
{
    using System.IO;

    /// <summary>
    /// Utilities for files and paths
    /// </summary>
    public static class IOUtilities
    {
        /// <summary>Used to know if Debug logs are enabled.</summary>
        public static bool IsDebugLogsEnabled
        {
            get => Logger.EnableDebugging;
            private set { }
        }

        /// <summary>
        /// Works like <see cref="Path.Combine(string, string)"/>, but can have more than 2 paths
        /// </summary>
        /// <param name="one">The first part of the path.</param>
        /// <param name="two">The second part of the path.</param>
        /// <param name="rest">Optional: Additional path parts to concatenate at the end.</param>
        /// <returns>Returns combined path parts as a single path.</returns>
        public static string Combine(string one, string two, params string[] rest)
        {
            string path = Path.Combine(one, two);

            foreach (string str in rest)
                path = Path.Combine(path, str);

            return path;
        }
    }
}
