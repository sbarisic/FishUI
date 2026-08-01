using Raylib_cs;

namespace FishUISample
{
    /// <summary>Queues screenshots for capture while the Raylib frame is still open.</summary>
    internal static class ScreenCapture
    {
        private static string _pendingPath;

        public static bool IsSupported => true;

        public static bool Queue(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return false;
            _pendingPath = Path.GetFullPath(filePath);
            return true;
        }

        public static void FlushPending()
        {
            string path = _pendingPath;
            if (path == null) return;
            _pendingPath = null;
            try
            {
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
                Raylib.TakeScreenshot(path);
                Console.WriteLine("Screenshot saved: " + path);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Screenshot failed: " + exception.Message);
            }
        }
    }
}
