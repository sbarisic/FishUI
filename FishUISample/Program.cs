using FishUI;
using FishUI.Controls;
using Raylib_cs;
using System.Diagnostics;
using System.Numerics;

namespace FishUISample
{
	internal class Program
	{
		static FishUI.FishUI FUI;

		static void Main(string[] args)
		{
			int W = 800;
			int H = 600;

			Raylib.SetTraceLogLevel(TraceLogLevel.None);

			Raylib.SetWindowState(ConfigFlags.HighDpiWindow);
			Raylib.SetWindowState(ConfigFlags.Msaa4xHint);
			Raylib.SetWindowState(ConfigFlags.ResizableWindow);
			//Raylib.SetWindowState(ConfigFlags.UndecoratedWindow);

			Raylib.InitWindow(W, H, "Fishmachine");
			int TargetFPS = Raylib.GetMonitorRefreshRate(0);
			Raylib.SetTargetFPS(TargetFPS);


			FUI = new FishUI.FishUI(new RaylibGfx(), new RaylibInput(), new EvtHandler());
			FUI.Init();

			bool LoadLayout = !true;

			if (File.Exists("layout.yaml") && LoadLayout)
			{
				LayoutFormat.Deserialize(FUI, File.ReadAllText("layout.yaml"));
			}
			else
			{
				MakeGUISample();

				string Dat = LayoutFormat.Serialize(FUI);
				File.WriteAllText("layout.yaml", Dat);
				Console.WriteLine("Wrote to: {0}", Path.GetFullPath("layout.yaml"));
			}

			Stopwatch SWatch = Stopwatch.StartNew();
			Stopwatch RuntimeWatch = Stopwatch.StartNew();

			while (!Raylib.WindowShouldClose())
			{
				float Dt = SWatch.ElapsedMilliseconds / 1000.0f;
				SWatch.Restart();

				if (Raylib.IsWindowResized())
				{
					FUI.Resized();
				}

				FUI.Tick(Dt, (float)RuntimeWatch.Elapsed.TotalSeconds);
			}

			Raylib.CloseWindow();
		}

		static void MakeGUISample()
		{
			Textbox Lbl = new Textbox(18, "The quick");
			Lbl.Position = new Vector2(100, 400);
			Lbl.Size = new Vector2(200, 30); //Lbl.MeasureText(FUI);
			Lbl.ZDepth = 1;
			FUI.Controls.Add(Lbl);

			Panel Pnl = new Panel();
			Pnl.ID = "panel1";
			Pnl.Position = new Vector2(10, 10);
			Pnl.Size = new Vector2(400, 350);
			Pnl.ZDepth = 2;
			FUI.Controls.Add(Pnl);

			Button Btn0 = new Button();
			Btn0.ID = "visible";
			Btn0.Text = "Make Visible";
			Btn0.Position = new Vector2(430, 100);
			Btn0.Size = new Vector2(150, 50);
			FUI.Controls.Add(Btn0);

			Button Btn = new Button();
			Btn.ID = "invisible";
			Btn.Text = "Make Invisible";
			Btn.Position = new Vector2(100, 100);
			Btn.Size = new Vector2(150, 50);
			Pnl.AddChild(Btn);

			CheckBox CBox = new CheckBox("Checkbox");
			CBox.Position = new Vector2(5, 10);
			CBox.Size = new Vector2(15, 15);
			Pnl.AddChild(CBox);

			RadioButton RBut = new RadioButton("Radio button");
			RBut.Position = new Vector2(5, 40);
			RBut.Size = new Vector2(15, 15);
			Pnl.AddChild(RBut);
		}
	}
}
