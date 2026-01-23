using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace FishUISample
{
    /// <summary>
    /// Provides screen capture functionality. Windows-only implementation.
    /// </summary>
    static class ScreenCapture
    {
        /// <summary>
        /// Returns true if screen capture is supported on the current platform.
        /// </summary>
        public static bool IsSupported => OperatingSystem.IsWindows();

        [DllImport("user32")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetDesktopWindow();

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32")]
        private static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

        [SupportedOSPlatform("windows")]
        public static Image CaptureDesktop()
        {
            return CaptureWindow(GetDesktopWindow());
        }

        [SupportedOSPlatform("windows")]
        public static Bitmap CaptureActiveWindow()
        {
            return CaptureWindow(GetForegroundWindow());
        }

        [SupportedOSPlatform("windows")]
        public static Bitmap CaptureWindow(IntPtr handle)
        {
            Rect rect = new Rect();
            GetWindowRect(handle, ref rect);
            Rectangle bounds = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);

            var result = new Bitmap(bounds.Width, bounds.Height);
            using (var graphics = Graphics.FromImage(result))
            {
                graphics.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
            }

            return result;
        }

        /// <summary>
        /// Captures the active window and saves it to a file. Safe to call on any platform.
        /// </summary>
        /// <param name="filePath">The file path to save the screenshot to.</param>
        /// <returns>True if the screenshot was saved successfully, false if not supported.</returns>
        public static bool TryCaptureActiveWindow(string filePath)
        {
            if (!IsSupported)
            {
                Console.WriteLine("Screenshot not supported on this platform.");
                return false;
            }

            return TryCaptureActiveWindowInternal(filePath);
        }

        [SupportedOSPlatform("windows")]
        private static bool TryCaptureActiveWindowInternal(string filePath)
        {
            try
            {
                using var bitmap = CaptureActiveWindow();
                bitmap.Save(filePath);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Screenshot failed: {ex.Message}");
                return false;
            }
        }
    }
}
