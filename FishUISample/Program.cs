using FishUI;
using FishUI.Controls;
using FishUIDemos;
using FishUISample.Samples;
using Raylib_cs;
using System.Diagnostics;
using System.Numerics;
using RaylibGfx = RaylibFishGfx.RaylibFishGfx;
using RaylibInput = RaylibFishGfx.RaylibInput;

namespace FishUISample
{
    internal class Program
    {
        static bool ScreenshotMade = false;
        static int Ctr = 10;

        static string CapturePath(string fileName)
        {
            string root = Environment.GetEnvironmentVariable("FISHUI_CAPTURE_ROOT");
            if (!string.IsNullOrWhiteSpace(root) && !Path.IsPathFullyQualified(root))
                throw new ArgumentException("FISHUI_CAPTURE_ROOT must be an absolute path.");
            return Path.Combine(root ?? "../../../../screenshots", fileName);
        }


        /// <summary>
        /// Takes a screenshot and saves it to the screenshots folder.
        /// </summary>
        static void TakeScreenshot(string Title, RaylibGfx Gfx)
        {
            if (!ScreenCapture.IsSupported)
            {
                Console.WriteLine("Screenshot not supported on this platform.");
                return;
            }

            DateTime Now = DateTime.Now;
            string FName = CapturePath($"{Title}_{Now:ddMMyyyy_HHmmss}.png");

            ScreenCapture.Queue(FName);
        }

        // Automatically take a screenshot after a set number of frames (Windows only)
        static void AutoScreenshot(RaylibGfx Gfx)
        {
            if (!ScreenCapture.IsSupported)
                return;

            if (Ctr > 0)
            {
                Ctr--;
            }
            else if (!ScreenshotMade)
            {
                ScreenshotMade = true;

                DateTime Now = DateTime.Now;
                string FName = CapturePath($"ss_{Now:ddMMyyyy_HHmmss}.png");

                ScreenCapture.Queue(FName);
            }
        }

        static Vector2 RotatePoint(Vector2 point, float angle)
        {
            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);

            return new Vector2(point.X * cos - point.Y * sin, point.X * sin + point.Y * cos);
        }

        static void ConfigureDiagnostics(FishUI.FishUI ui)
        {
            ui.Diagnostics.Enabled = true;
            ui.Diagnostics.HotkeyEnabled = true;
            ui.Diagnostics.RollingEventHistoryEnabled = true;
            ui.Diagnostics.RollingEventHistoryDuration = TimeSpan.FromSeconds(10);
            ui.Diagnostics.MaximumRollingHistoryEvents = 20000;
            ui.Diagnostics.PrivacyPolicy.AllowFramebufferCapture = true;
            ui.Diagnostics.ResetEventRecorder();
            ui.Diagnostics.AutoExportAsync = (snapshot, cancellationToken) => Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                string path = Path.Combine(AppContext.BaseDirectory, "diagnostics", snapshot.DefaultExportName);
                snapshot.SaveDirectory(path, overwrite: false);
                Console.WriteLine("Diagnostic bundle saved: " + path);
            }, cancellationToken);
            ui.Diagnostics.ExportFailed += (_, eventArgs) =>
                Console.WriteLine("Diagnostic export failed: " + eventArgs.Exception?.Message);
        }

        static bool HasArgument(string[] args, string name)
        {
            for (int i = 0; i < args.Length; i++)
                if (args[i].Equals(name, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        static int ReadIntArgument(string[] args, string name)
        {
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i].Equals(name, StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(args[i + 1], out int value)) return Math.Max(0, value);
            return 0;
        }

        static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);

            // Auto-discover all ISample implementations using reflection
            ISample[] Samples = SampleDiscovery.DiscoverSamples();

            Console.WriteLine($"Discovered {Samples.Length} samples:");
            for (int i = 0; i < Samples.Length; i++)
            {
                Console.WriteLine($"  {i}: {Samples[i].Name}");
            }
            Console.WriteLine();

            // Loop back to chooser when window closes
            while (true)
            {
                int automatedFrameLimit = ReadIntArgument(args, "--frames");
                bool backendOwnsFrame = HasArgument(args, "--fishui-begin-drawing");
                bool captureDiagnostics = HasArgument(args, "--capture-diagnostics");
                // Sample chooser - select which sample to run
                // GUI-based sample chooser - uses FishUI to select which sample to run
                SampleChooser chooser = new SampleChooser(Samples);
                ISample Cur = chooser.ShowAndChoose(args);

                // Clear any previous command-line sample selection after first run
                // so subsequent iterations show the chooser menu
                args = Array.Empty<string>();

                // If chooser was closed without selection, exit the application
                if (Cur == null)
                {
                    Console.WriteLine("Sample chooser closed. Exiting application.");
                    break;
                }

                FishUISettings UISettings = new FishUISettings();
                UISettings.UIScale = 1.0f;

                RaylibGfx Gfx = new RaylibGfx(1280, 720, "FishUI - " + Cur.Name);
                Gfx.UseBeginDrawing = backendOwnsFrame;
                IFishUIInput Input = new RaylibInput();
                IFishUIEvents Events = new EvtHandler();

                // Set up screenshot action for the sample
                Cur.TakeScreenshot = (Title) => TakeScreenshot(Title, Gfx);

                FishUI.FishUI FUI = Cur.CreateUI(UISettings, Gfx, Input, Events);
                ConfigureDiagnostics(FUI);
                Cur.Init();
                Task<FishUIDebugSnapshot> automatedCapture = captureDiagnostics
                    ? FUI.Diagnostics.CaptureAsync(new FishUIDebugSnapshotOptions
                    {
                        IncludeScreenshot = true,
                        IncludeAnnotatedOverlay = true,
                        IncludeRecentEvents = true
                    }, FishUIDebugCaptureReason.ManualApi)
                    : null;

                Stopwatch SWatch = Stopwatch.StartNew();
                Stopwatch RuntimeWatch = Stopwatch.StartNew();

                float angle = 0;
                float radius = 480;
                Vector2 center = new Vector2(Gfx.GetWindowWidth() / 2f, Gfx.GetWindowHeight() / 2f);

                Texture2D Tex = Raylib.LoadTexture("data/images/win95.png");

                int renderedFrames = 0;
                while (!Raylib.WindowShouldClose() &&
                    (automatedFrameLimit == 0 || renderedFrames < automatedFrameLimit))
                {
                    float Dt = SWatch.ElapsedMilliseconds / 1000.0f;
                    SWatch.Restart();

                    if (Raylib.IsWindowResized())
                    {
                        FUI.Resized(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
                    }


                    angle += Raylib.GetFrameTime() * 2.0f; // radians per second

                    // Compute triangle vertices
                    Vector2 v1 = RotatePoint(new Vector2(0, -radius), angle) + center;
                    Vector2 v2 = RotatePoint(new Vector2(-radius, radius), angle) + center;
                    Vector2 v3 = RotatePoint(new Vector2(radius, radius), angle) + center;

                    v1 = Vector2.Normalize(v1);
                    v2 = Vector2.Normalize(v2);
                    v3 = Vector2.Normalize(v3);

                    if (!backendOwnsFrame)
                    {
                        Raylib.BeginDrawing();
                        Raylib.ClearBackground(Color.SkyBlue);

                        Vector2 ImgSz = new Vector2(Tex.Width, Tex.Height);
                        Raylib.DrawTexturePro(Tex, new Rectangle(Vector2.Zero, ImgSz), new Rectangle(center, ImgSz), ImgSz / 2, angle, Color.White);
                        Raylib.DrawRectangleLinesEx(new Rectangle(center - ImgSz / 2, ImgSz), 2, Color.White);
                    }


                    // Call sample-specific update logic
                    Cur.Update(Dt);

                    FUI.Tick(Dt, (float)RuntimeWatch.Elapsed.TotalSeconds);

                    if (!backendOwnsFrame)
                    {
                        Raylib.DrawFPS(Gfx.GetWindowWidth() - 150, 25);
                        ScreenCapture.FlushPending();
                        Raylib.EndDrawing();
                    }
                    renderedFrames++;
                }

                Raylib.UnloadTexture(Tex);
                if (automatedCapture != null)
                {
                    FishUIDebugSnapshot snapshot = automatedCapture.GetAwaiter().GetResult();
                    Console.WriteLine($"Automated diagnostic capture: {snapshot.CaptureStatus}; " +
                        $"screenshot={snapshot.Artifacts["screenshot"].Status}; " +
                        $"overlay={snapshot.Artifacts["overlay"].Status}");
                }
                FUI.Dispose();
                FUI.Diagnostics.WaitForPendingExportsAsync().GetAwaiter().GetResult();
                Gfx.Dispose();
                Raylib.CloseWindow();

                Console.WriteLine();
                Console.WriteLine("Window closed. Returning to sample chooser...");
                Console.WriteLine();
                if (automatedFrameLimit > 0)
                    break;
            }
        }
    }
}
