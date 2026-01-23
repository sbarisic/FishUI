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
			ISample[] Samples = new[] { new SampleDefault() };

			ISample Cur = Samples[0];

			FishUI.FishUI FUI = Cur.CreateUI();
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
