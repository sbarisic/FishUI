using FishUI;
using FishUI.Controls;
using FishUISample.Samples;
using Raylib_cs;
using System.Diagnostics;
using System.Numerics;

namespace FishUISample
{
	internal class Program
	{
		static bool ScreenshotMade = false;
		//static int Ctr = 100;
		static int Ctr = 10;

		// TODO: Add platform support, on windows take screenshot, on other platforms do nothing (to make the compile warnings go away)
		// Automatically take a screenshot after a set number of frames
		static void AutoScreenshot(RaylibGfx Gfx)
		{
			if (Ctr > 0)
			{
				Ctr--;
			}
			else if (!ScreenshotMade)
			{
				ScreenshotMade = true;

				DateTime Now = DateTime.Now;
				string FName = $"../../../../screenshots/ss_{Now.ToString("ddMMyyyy_HHmmss")}.png";

				Gfx.FocusWindow();
				Thread.Sleep(200);

				ScreenCapture.CaptureActiveWindow().Save(FName);

				Thread.Sleep(200);
				Environment.Exit(0);

				//Raylib.TakeScreenshot(FName);
				//Console.WriteLine("Made screenshot " + FName);
			}
		}

		static void Main(string[] args)
		{
			ISample[] Samples = new ISample[] { new SampleDefault(), new SampleThemeSwitcher(), new SampleGameMenu() };

			FishUISettings UISettings = new FishUISettings();
			IFishUIGfx Gfx = new RaylibGfx(1920, 1080, "FishUI");
			IFishUIInput Input = new RaylibInput();
			IFishUIEvents Events = new EvtHandler();

			ISample Cur = Samples[0];
			FishUI.FishUI FUI = Cur.CreateUI(UISettings, Gfx, Input, Events);
			Cur.Init();

			Stopwatch SWatch = Stopwatch.StartNew();
			Stopwatch RuntimeWatch = Stopwatch.StartNew();

			while (!Raylib.WindowShouldClose())
			{
				float Dt = SWatch.ElapsedMilliseconds / 1000.0f;
				SWatch.Restart();

				if (Raylib.IsWindowResized())
				{
					FUI.Resized(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
				}

				FUI.Tick(Dt, (float)RuntimeWatch.Elapsed.TotalSeconds);
				//AutoScreenshot(Gfx as RaylibGfx);
			}

			Raylib.CloseWindow();
		}
	}
}
