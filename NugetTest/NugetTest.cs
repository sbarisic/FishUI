using FishUI;
using FishUI.Controls;
using Raylib_cs;
using System.Numerics;
using RaylibGfx = RaylibFishGfx.RaylibFishGfx;
using RaylibInput = RaylibFishGfx.RaylibInput;

namespace NugetTest
{
	internal class NugetTest
	{
		static void Main(string[] args)
		{
			// Create UI settings
			FishUISettings settings = new FishUISettings();
			settings.UIScale = 1.0f;

			// Create Raylib window and graphics backend
			RaylibGfx gfx = new RaylibGfx(800, 600, "FishUI NuGet Demo");
			gfx.UseBeginDrawing = false;

			// Create input handler
			IFishUIInput input = new RaylibInput();

			// Create event handler (can be null for simple demos)
			IFishUIEvents events = new SimpleEventHandler();

			// Create FishUI instance
			FishUI.FishUI fui = new FishUI.FishUI(settings, gfx, input, events);
			fui.Init();

			// Load theme (required for proper fonts and control rendering)
			// data/themes/themes/ is correct!!
			settings.LoadTheme("data/themes/themes/gwen.yaml", applyImmediately: true);

			// Create some UI controls
			CreateDemoUI(fui);

			// Main loop
			while (!Raylib.WindowShouldClose())
			{
				float dt = Raylib.GetFrameTime();

				// Handle window resize
				if (Raylib.IsWindowResized())
				{
					fui.Resized(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
				}

				Raylib.BeginDrawing();
				Raylib.ClearBackground(Color.DarkGray);

				// Update and render UI
				fui.Tick(dt, (float)Raylib.GetTime());

				Raylib.DrawFPS(10, 10);
				Raylib.EndDrawing();
			}

			Raylib.CloseWindow();
		}

		static void CreateDemoUI(FishUI.FishUI fui)
		{
			// Title label
			Label titleLabel = new Label("FishUI NuGet Demo");
			titleLabel.Position = new Vector2(20, 40);
			titleLabel.Size = new Vector2(300, 30);
			titleLabel.Alignment = Align.Left;
			fui.AddControl(titleLabel);

			// Panel to group controls
			Panel panel = new Panel();
			panel.Position = new Vector2(20, 80);
			panel.Size = new Vector2(350, 400);
			fui.AddControl(panel);

			// Panel title label
			Label panelTitle = new Label("Demo Controls");
			panelTitle.Position = new Vector2(10, 5);
			panelTitle.Size = new Vector2(200, 20);
			panelTitle.Alignment = Align.Left;
			panel.AddChild(panelTitle);

			// Text input
			Label textLabel = new Label("Text Input:");
			textLabel.Position = new Vector2(10, 30);
			textLabel.Size = new Vector2(100, 20);
			textLabel.Alignment = Align.Left;
			panel.AddChild(textLabel);

			Textbox textbox = new Textbox("Type here...");
			textbox.Position = new Vector2(10, 55);
			textbox.Size = new Vector2(200, 30);
			panel.AddChild(textbox);

			// Button
			Button button = new Button();
			button.Text = "Click Me!";
			button.Position = new Vector2(220, 55);
			button.Size = new Vector2(100, 30);
			button.OnButtonPressed += (btn, mouseBtn, pos) =>
			{
				Console.WriteLine("Button clicked!");
				textbox.Text = "Button was clicked!";
			};
			panel.AddChild(button);

			// Checkbox
			CheckBox checkBox = new CheckBox("Enable Feature");
			checkBox.Position = new Vector2(10, 100);
			checkBox.Size = new Vector2(20, 20);
			checkBox.OnCheckedChanged += (cb, isChecked) =>
			{
				Console.WriteLine($"Checkbox: {isChecked}");
			};
			panel.AddChild(checkBox);

			// Slider with label
			Label sliderLabel = new Label("Slider Value: 50");
			sliderLabel.Position = new Vector2(10, 140);
			sliderLabel.Size = new Vector2(200, 20);
			sliderLabel.Alignment = Align.Left;
			panel.AddChild(sliderLabel);

			Slider slider = new Slider();
			slider.Position = new Vector2(10, 165);
			slider.Size = new Vector2(200, 20);
			slider.MinValue = 0;
			slider.MaxValue = 100;
			slider.Value = 50;
			slider.OnValueChanged += (s, val) =>
			{
				sliderLabel.Text = $"Slider Value: {val:F0}";
			};
			panel.AddChild(slider);

			// Progress bar
			Label progressLabel = new Label("Progress Bar:");
			progressLabel.Position = new Vector2(10, 200);
			progressLabel.Size = new Vector2(100, 20);
			progressLabel.Alignment = Align.Left;
			panel.AddChild(progressLabel);

			ProgressBar progressBar = new ProgressBar();
			progressBar.Position = new Vector2(10, 225);
			progressBar.Size = new Vector2(200, 25);
			progressBar.Value = 0.7f;
			panel.AddChild(progressBar);

			// ListBox
			Label listLabel = new Label("List Box:");
			listLabel.Position = new Vector2(10, 260);
			listLabel.Size = new Vector2(100, 20);
			listLabel.Alignment = Align.Left;
			panel.AddChild(listLabel);

			ListBox listBox = new ListBox();
			listBox.Position = new Vector2(10, 285);
			listBox.Size = new Vector2(200, 100);
			listBox.AddItem("Item 1");
			listBox.AddItem("Item 2");
			listBox.AddItem("Item 3");
			listBox.AddItem("Item 4");
			listBox.AddItem("Item 5");
			listBox.OnItemSelected += (lb, idx, item) =>
			{
				Console.WriteLine($"Selected item index: {idx}");
			};
			panel.AddChild(listBox);

			// Second panel with more controls
			Panel panel2 = new Panel();
			panel2.Position = new Vector2(400, 80);
			panel2.Size = new Vector2(350, 200);
			fui.AddControl(panel2);

			// Panel 2 title label
			Label panel2Title = new Label("More Controls");
			panel2Title.Position = new Vector2(10, 5);
			panel2Title.Size = new Vector2(200, 20);
			panel2Title.Alignment = Align.Left;
			panel2.AddChild(panel2Title);

			// Radio buttons
			Label radioLabel = new Label("Choose Option:");
			radioLabel.Position = new Vector2(10, 30);
			radioLabel.Size = new Vector2(150, 20);
			radioLabel.Alignment = Align.Left;
			panel2.AddChild(radioLabel);

			RadioButton radio1 = new RadioButton("Option A");
			radio1.Position = new Vector2(10, 55);
			radio1.Size = new Vector2(20, 20);
			radio1.IsChecked = true;
			panel2.AddChild(radio1);

			RadioButton radio2 = new RadioButton("Option B");
			radio2.Position = new Vector2(10, 80);
			radio2.Size = new Vector2(20, 20);
			panel2.AddChild(radio2);

			RadioButton radio3 = new RadioButton("Option C");
			radio3.Position = new Vector2(10, 105);
			radio3.Size = new Vector2(20, 20);
			panel2.AddChild(radio3);

			// NumericUpDown
			Label numLabel = new Label("Numeric Input:");
			numLabel.Position = new Vector2(150, 30);
			numLabel.Size = new Vector2(120, 20);
			numLabel.Alignment = Align.Left;
			panel2.AddChild(numLabel);

			NumericUpDown numUpDown = new NumericUpDown(50, 0, 100, 5);
			numUpDown.Position = new Vector2(150, 55);
			numUpDown.Size = new Vector2(100, 28);
			numUpDown.OnValueChanged += (nud, val) =>
			{
				Console.WriteLine($"NumericUpDown: {val}");
			};
			panel2.AddChild(numUpDown);

			// Toggle switch
			Label toggleLabel = new Label("Toggle Switch:");
			toggleLabel.Position = new Vector2(150, 90);
			toggleLabel.Size = new Vector2(120, 20);
			toggleLabel.Alignment = Align.Left;
			panel2.AddChild(toggleLabel);

			ToggleSwitch toggle = new ToggleSwitch();
			toggle.Position = new Vector2(150, 115);
			toggle.Size = new Vector2(60, 28);
			toggle.OnToggleChanged += (ts, isOn) =>
			{
				Console.WriteLine($"Toggle: {isOn}");
			};
			panel2.AddChild(toggle);
		}
	}

	/// <summary>
	/// Simple event handler implementation for the demo.
	/// </summary>
	internal class SimpleEventHandler : IFishUIEvents
	{
		public void Broadcast(FishUI.FishUI FUI, Control Ctrl, string Name, object[] Args)
		{
			// Handle events if needed
			// Console.WriteLine($"Event: {Ctrl?.GetType().Name} - {Name}");
		}
	}
}
