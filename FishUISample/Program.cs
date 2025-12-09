using FishUI;
using FishUI.Controls;
using Raylib_cs;
using System.Diagnostics;
using System.Numerics;

namespace FishUISample
{
	internal class Program
	{
		static void Main(string[] args)
		{
			int W = 800;
			int H = 600;

			Raylib.SetTraceLogLevel(TraceLogLevel.None);
			Raylib.InitWindow(W, H, "Fishmachine");
			Raylib.SetTargetFPS(60);

			FishUI.FishUI FUI = new FishUI.FishUI(new RaylibGfx(), new RaylibInput(), W, H);
			FUI.Init();


			Button Btn = new Button();
			Btn.Position = new Vector2(100, 100);
			Btn.Size = new Vector2(100, 60);
			FUI.Controls.Add(Btn);

			Stopwatch SWatch = Stopwatch.StartNew();
			Stopwatch RuntimeWatch = Stopwatch.StartNew();

			while (!Raylib.WindowShouldClose())
			{
				float Dt = SWatch.ElapsedMilliseconds / 1000.0f;
				SWatch.Restart();

				FUI.Tick(Dt, (float)RuntimeWatch.Elapsed.TotalSeconds);
			}

			Raylib.CloseWindow();
		}
	}
}
