using FishUI;
using FishUI.Controls;
using Raylib_cs;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.ConstrainedExecution;

namespace FishUIEditor
{
	internal class Program
	{
		static FishUI.FishUI FUI;

		static void Main(string[] args)
		{
			FishUISettings UISettings = new FishUISettings();
			UISettings.UIScale = 1.0f;

			RaylibGfx Gfx = new RaylibGfx(1920, 1080, "FishUI Editor");
			IFishUIInput Input = new RaylibInput();
			IFishUIEvents Events = new EvtHandler();

			FUI = new FishUI.FishUI(UISettings, Gfx, Input, Events);
			FUI.Init();

			UISettings.LoadTheme("data/themes/gwen.yaml", applyImmediately: true);

			CreateGUI();

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
				//Cur.Update(Dt);

				FUI.Tick(Dt, (float)RuntimeWatch.Elapsed.TotalSeconds);
				//AutoScreenshot(Gfx as RaylibGfx);
			}

			Raylib.CloseWindow();
		}

		static void CreateGUI()
		{
			// Title
			Label titleLabel = new Label("FishUI Sample Chooser");
			titleLabel.Position = new Vector2(20, 20);
			titleLabel.Size = new Vector2(760, 40);
			titleLabel.Alignment = Align.Center;
			FUI.AddControl(titleLabel);
		}
	}
}
