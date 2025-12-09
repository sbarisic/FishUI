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

			Raylib.SetWindowState(ConfigFlags.HighDpiWindow);
			Raylib.SetWindowState(ConfigFlags.Msaa4xHint);

			Raylib.InitWindow(W, H, "Fishmachine");
			int TargetFPS = Raylib.GetMonitorRefreshRate(0);
			Raylib.SetTargetFPS(TargetFPS);


			FishUI.FishUI FUI = new FishUI.FishUI(new RaylibGfx(), new RaylibInput(), W, H);
			FUI.Init();




			Label Lbl = new Label(18, "The quick brown fox jumps over the lazy dog");
			Lbl.Position = new Vector2(100, 300);
			Lbl.Size = Lbl.MeasureText(FUI);
			Lbl.ZDepth = 1;
			FUI.Controls.Add(Lbl);

			Panel Pnl = new Panel();
			Pnl.Position = new Vector2(10, 10);
			Pnl.Size = new Vector2(400, 350);
			FUI.Controls.Add(Pnl);

			Button Btn = new Button();
			Btn.Position = new Vector2(100, 100);
			Btn.Size = new Vector2(150, 50);
			Pnl.AddChild(Btn);

			CheckBox CBox = new CheckBox();
			CBox.Position = new Vector2(0, 0);
			CBox.Size = new Vector2(15, 15);
			Pnl.AddChild(CBox);

			RadioButton RBut = new RadioButton();
			RBut.Position = new Vector2(0, 15);
			RBut.Size = new Vector2(15, 15);
			Pnl.AddChild(RBut);

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
