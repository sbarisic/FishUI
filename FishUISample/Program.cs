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

		/// <summary>
		/// Takes a screenshot and saves it to the screenshots folder.
		/// </summary>
		static void TakeScreenshot(RaylibGfx Gfx)
		{
			DateTime Now = DateTime.Now;
			string FName = $"../../../../screenshots/ss_{Now.ToString("ddMMyyyy_HHmmss")}.png";

			Gfx.FocusWindow();
			Thread.Sleep(200);

			ScreenCapture.CaptureActiveWindow().Save(FName);
			Console.WriteLine("Screenshot saved: " + FName);
		}

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

			// Sample chooser - select which sample to run
			ISample Cur = ChooseSample(Samples, args);

			FishUISettings UISettings = new FishUISettings();
			RaylibGfx Gfx = new RaylibGfx(1920, 1080, "FishUI - " + Cur.Name);
			IFishUIInput Input = new RaylibInput();
			IFishUIEvents Events = new EvtHandler();

			// Set up screenshot action for the sample
			Cur.TakeScreenshot = () => TakeScreenshot(Gfx);

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

		/// <summary>
		/// Displays a console menu to choose which sample to run.
		/// Supports command-line argument to skip menu (e.g., --sample 0).
		/// </summary>
		static ISample ChooseSample(ISample[] samples, string[] args)
		{
			// Check for command-line argument: --sample N
			for (int i = 0; i < args.Length - 1; i++)
			{
				if (args[i].Equals("--sample", StringComparison.OrdinalIgnoreCase) ||
				    args[i].Equals("-s", StringComparison.OrdinalIgnoreCase))
				{
					if (int.TryParse(args[i + 1], out int sampleIndex) && 
					    sampleIndex >= 0 && sampleIndex < samples.Length)
					{
						Console.WriteLine($"Starting sample {sampleIndex}: {samples[sampleIndex].Name}");
						return samples[sampleIndex];
					}
				}
			}

			// Interactive console menu
			Console.WriteLine("??????????????????????????????????????????");
			Console.WriteLine("?         FishUI Sample Chooser          ?");
			Console.WriteLine("??????????????????????????????????????????");
			
			for (int i = 0; i < samples.Length; i++)
			{
				Console.WriteLine($"?  [{i}] {samples[i].Name,-32} ?");
			}
			
			Console.WriteLine("??????????????????????????????????????????");
			Console.WriteLine("?  Enter number or press Enter for [0]   ?");
			Console.WriteLine("??????????????????????????????????????????");
			Console.Write("> ");

			string input = Console.ReadLine() ?? "";
			
			if (string.IsNullOrWhiteSpace(input))
			{
				Console.WriteLine($"Starting default sample: {samples[0].Name}");
				return samples[0];
			}

			if (int.TryParse(input.Trim(), out int choice) && choice >= 0 && choice < samples.Length)
			{
				Console.WriteLine($"Starting sample: {samples[choice].Name}");
				return samples[choice];
			}

			Console.WriteLine($"Invalid choice, starting default sample: {samples[0].Name}");
			return samples[0];
		}
	}
}
