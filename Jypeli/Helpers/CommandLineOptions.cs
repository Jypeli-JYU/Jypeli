using System;
using System.Linq;

namespace Jypeli
{
    internal static class CommandLineOptions
    {
        static CommandLineOptions()
        {
            string[] args = Environment.GetCommandLineArgs();
            Save = Parse<bool>(args, "save", bool.TryParse);
            FramesToRun = Parse<int>(args, "framesToRun", int.TryParse);
            Headless = Parse<bool>(args, "headless", bool.TryParse);
            SkipFrames = Parse<int>(args, "skipFrames", int.TryParse);
            SaveToStdout = Parse<bool>(args, "saveToStdout", bool.TryParse);
        }

        public static bool? Save { get; }
        public static int? FramesToRun { get; }
        public static bool? Headless { get; }
        public static int? SkipFrames { get; }
        public static bool? SaveToStdout { get; }

        private static T? Parse<T>(string[] args, string value, TryParseDelegate<T> parser) where T : struct
        {
            string argName = $"--{value}";
            if (!args.Contains(argName))
                return null;
            if (parser(args[Array.IndexOf(args, argName) + 1], out T result))
                return result;
            throw new ArgumentException("Invalid value for --save");
        }

        private delegate bool TryParseDelegate<T>(string value, out T result);
    }
}
