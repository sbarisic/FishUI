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
			FishUISettings UISettings = new FishUISettings();
			IFishUIGfx Gfx = new RaylibGfx(800, 600, "FishUI");
			IFishUIInput Input = new RaylibInput();
			IFishUIEvents Events = new EvtHandler();

			FUI = new FishUI.FishUI(UISettings, Gfx, Input, Events);
			FUI.Init();
			MakeGUISample();

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
			Textbox Lbl = new Textbox("The quick");
			Lbl.Position = new Vector2(100, 400);
			Lbl.ZDepth = 2;
			FUI.AddControl(Lbl);

			ListBox Lb = new ListBox();
			Lb.Position = new Vector2(320, 350);
			Lb.ZDepth = 2;
			Lb.ID = "listbox1";
			FUI.AddControl(Lb);

			for (int i = 0; i < 10; i++)
			{
				Lb.Items.Add("Item " + i);
			}

			ScrollBarV Sbv = new ScrollBarV();
			Sbv.Position = new Vector2(480, 340);
			Sbv.ZDepth = 2;
			FUI.AddControl(Sbv);

			Button Btn0 = new Button();
			Btn0.ID = "visible";
			Btn0.Text = "Make Visible";
			Btn0.Position = new Vector2(430, 100);
			Btn0.Size = new Vector2(150, 50);
			Btn0.ZDepth = 2;
			FUI.AddControl(Btn0);

			Button Btn1 = new Button();
			Btn1.ID = "savelayout";
			Btn1.Text = "Save Layout";
			Btn1.Position = new Vector2(430, 160);
			Btn1.Size = new Vector2(150, 50);
			Btn1.ZDepth = 2;
			FUI.AddControl(Btn1);

			Button Btn2 = new Button();
			Btn2.ID = "loadlayout";
			Btn2.Text = "Load Layout";
			Btn2.Position = new Vector2(430, 220);
			Btn2.Size = new Vector2(150, 50);
			Btn2.ZDepth = 2;
			FUI.AddControl(Btn2);

			// Panel ------------------
			Panel Pnl = new Panel();
			Pnl.ID = "panel1";
			Pnl.Position = new Vector2(10, 10);
			Pnl.Size = new Vector2(400, 350);
			Pnl.ZDepth = 1;
			Pnl.Draggable = true;
			Pnl.IsTransparent = true;
			FUI.AddControl(Pnl);

			Button Btn = new Button();
			Btn.ID = "invisible";
			Btn.Text = "Make Invisible";
			//Btn.Position = new Vector2(100, 100);
			Btn.Position = new FishUIPosition(PositionMode.Docked, DockMode.Horizontal, new Vector4(10, 0, 10, 0), new Vector2(100, 100));
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
