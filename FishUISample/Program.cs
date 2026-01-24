using FishUI;
using FishUI.Controls;
using FishUISample.Samples;
using Raylib_cs;
using System.Diagnostics;
using System.Numerics;
using FishUIDemos;

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
			if (!ScreenCapture.IsSupported)
			{
				Console.WriteLine("Screenshot not supported on this platform.");
				return;
			}

			DateTime Now = DateTime.Now;
			string FName = $"../../../../screenshots/ss_{Now.ToString("ddMMyyyy_HHmmss")}.png";

			Gfx.FocusWindow();
			Thread.Sleep(200);

			if (ScreenCapture.TryCaptureActiveWindow(FName))
			{
				Console.WriteLine("Screenshot saved: " + FName);
			}
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
				string FName = $"../../../../screenshots/ss_{Now.ToString("ddMMyyyy_HHmmss")}.png";

				Gfx.FocusWindow();
				Thread.Sleep(200);

				ScreenCapture.TryCaptureActiveWindow(FName);

				Thread.Sleep(200);
				Environment.Exit(0);
			}
		}

		static void Main(string[] args)
		{
			ISample[] Samples = new ISample[]
			{
			new SampleDefault(),        // Windows, Dialogs, TabControl, TreeView, ContextMenu
			new SampleBasicControls(),  // Textbox, ListBox, DropDown, ScrollBars, ProgressBars, Sliders, etc.
			new SampleButtonVariants(), // Toggle, Repeat, ImageButton, Icon buttons
			new SampleLayoutSystem(),   // Margin, Padding, Anchors, StackLayout, Panel variants
			new SampleThemeSwitcher(),  // Runtime theme switching
			new SampleGameMenu(),       // Game main menu example
			new SampleVirtualCursor(),  // Virtual cursor/mouse for keyboard/gamepad navigation
			new SamplePropertyGrid(),   // PropertyGrid control demo
			new SampleMenuBar()         // MenuBar with dropdown menus
			};

			// Loop back to chooser when window closes
			while (true)
			{
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

					// Call sample-specific update logic
					Cur.Update(Dt);

					FUI.Tick(Dt, (float)RuntimeWatch.Elapsed.TotalSeconds);
					//AutoScreenshot(Gfx as RaylibGfx);
				}

				Raylib.CloseWindow();

				Console.WriteLine();
				Console.WriteLine("Window closed. Returning to sample chooser...");
				Console.WriteLine();
			}
		}
	}
}
