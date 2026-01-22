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
            FishUISettings UISettings = new FishUISettings();
            IFishUIGfx Gfx = new RaylibGfx(1920, 1080, "FishUI");
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
                //AutoScreenshot(Gfx as RaylibGfx);
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
                Lb.AddItem("Item " + i);
            }

            //Lb.AutoResizeHeight();

            DropDown DD = new DropDown();
            DD.Position = new Vector2(550, 350);
            DD.ZDepth = 2;
            DD.ID = "dropdown1";
            FUI.AddControl(DD);

            for (int i = 0; i < 10; i++)
            {
                DD.AddItem("Option " + i);
            }

            ScrollBarV Sbv = new ScrollBarV();
            Sbv.Position = new Vector2(480, 340);
            Sbv.ZDepth = 2;
            FUI.AddControl(Sbv);

            ScrollBarH Sbh = new ScrollBarH();
            Sbh.Position = new Vector2(260, 540);
            Sbh.ZDepth = 2;
            FUI.AddControl(Sbh);

            // Determinate progress bar
            ProgressBar progressBar = new ProgressBar();
            progressBar.Position = new Vector2(260, 580);
            progressBar.Size = new Vector2(200, 20);
            progressBar.Value = 0.75f; // 75% complete
            FUI.AddControl(progressBar);

            // Indeterminate/marquee progress bar
            ProgressBar loadingBar = new ProgressBar();
            loadingBar.Position = new Vector2(260, 620);
            loadingBar.IsIndeterminate = true;
            FUI.AddControl(loadingBar);

            // Toggle switch
            ToggleSwitch toggle1 = new ToggleSwitch();
            toggle1.Position = new Vector2(260, 660);
            toggle1.Size = new Vector2(50, 24);
            toggle1.IsOn = false;
            FUI.AddControl(toggle1);

            // Toggle switch with labels
            ToggleSwitch toggle2 = new ToggleSwitch();
            toggle2.Position = new Vector2(320, 660);
            toggle2.Size = new Vector2(70, 24);
            toggle2.IsOn = true;
            toggle2.ShowLabels = true;
            FUI.AddControl(toggle2);

            // Horizontal slider
            Slider slider1 = new Slider();
            slider1.Position = new Vector2(260, 700);
            slider1.Size = new Vector2(200, 24);
            slider1.MinValue = 0;
            slider1.MaxValue = 100;
            slider1.Value = 50;
            slider1.ShowValueLabel = true;
            FUI.AddControl(slider1);

            // Vertical slider
            Slider slider2 = new Slider();
            slider2.Position = new Vector2(500, 540);
            slider2.Size = new Vector2(24, 150);
            slider2.Orientation = SliderOrientation.Vertical;
            slider2.MinValue = 0;
            slider2.MaxValue = 100;
            slider2.Step = 10; // Snap to 10s
            slider2.Value = 30;
            FUI.AddControl(slider2);


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
            Btn.Position = new FishUIPosition(PositionMode.Docked, DockMode.Horizontal, new Vector4(15, 0, 15, 0), new Vector2(100, 100));
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
