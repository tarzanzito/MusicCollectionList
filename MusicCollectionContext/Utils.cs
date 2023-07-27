

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
    }
}
