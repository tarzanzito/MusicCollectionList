

using Serilog;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MusicCollectionContext
{
    public static class Utils
    {
        public static string AppendDirectorySeparator(string path)
        {
            if (!System.IO.Path.EndsInDirectorySeparator(path))
                return path + System.IO.Path.PathSeparator;
            
            return path;
        }

        public static Stopwatch GetNewStopwatch()
        {
            return new Stopwatch();
        }

        public static void Startwatch(Stopwatch stopwatch, string source, string msg)
        {
            Log.Information($"Stopwatch: [{source}].[{msg}] - Started:");
            stopwatch.Restart();

        }

        public static void Stopwatch(Stopwatch stopwatch, string source, string msg)
        {
            stopwatch.Stop();
            Log.Information($"Stopwatch: [{source}].[{msg}] - Finished: Elapsed time:[{stopwatch.ElapsedMilliseconds}]");
        }
    }
}
