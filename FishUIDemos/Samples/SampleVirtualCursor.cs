using FishUI;
using FishUI.Controls;
using System;
using System.Numerics;

namespace FishUIDemos
{
	/// <summary>
	/// Demonstrates the virtual cursor/mouse functionality for keyboard/gamepad UI navigation.
	/// Use Arrow keys to move cursor, Space/Enter for left click, Right Shift for right click.
	/// </summary>
	public class SampleVirtualCursor : ISample
	{
		FishUI.FishUI FUI;
		IFishUIInput InputRef;
		Label statusLabel;
		Label positionLabel;
		CheckBox enabledCheckbox;
		Slider speedSlider;

		/// <summary>
		/// Display name of the sample.
		/// </summary>
		public string Name => "VirtualCursor";

	/// <summary>
		/// Action to take a screenshot, set by Program.cs.
		/// </summary>
		public TakeScreenshotFunc TakeScreenshot { get; set; }

		public FishUI.FishUI CreateUI(FishUISettings UISettings, IFishUIGfx Gfx, IFishUIInput Input, IFishUIEvents Events)
		{
			FUI = new FishUI.FishUI(UISettings, Gfx, Input, Events);
			FUI.Init();
			InputRef = Input;

			// Load theme
			FishUITheme theme = UISettings.LoadTheme(ThemePreferences.LoadThemePath(), applyImmediately: true);

			// Initialize virtual mouse at center of screen
			FUI.VirtualMouse.Initialize(FUI.Width, FUI.Height);
			FUI.VirtualMouse.Enabled = true;
			FUI.VirtualMouse.DrawCursor = true;
			FUI.VirtualMouse.CursorColor = new FishColor(255, 200, 50, 255);
			FUI.VirtualMouse.Speed = 400f;

			return FUI;
		}

		public void Init()
		{
			// Main panel
			Panel mainPanel = new Panel();
			mainPanel.Position = new Vector2(50, 50);
			mainPanel.Size = new Vector2(400, 430);
			FUI.AddControl(mainPanel);

			// Title
			Label titleLabel = new Label("Virtual Cursor Demo");
			titleLabel.Position = new Vector2(20, 15);
			titleLabel.Size = new Vector2(300, 20);
			titleLabel.Alignment = Align.Left;
			mainPanel.AddChild(titleLabel);

			// Instructions
			StaticText instructions = new StaticText(
				"Controls:\n" +
				"• Arrow Keys: Move cursor\n" +
				"• Space/Enter: Left click\n" +
				"• Right Shift: Right click\n" +
				"Uncheck checkbox to use hybrid mode (follows real mouse)"
			);
			instructions.Position = new Vector2(20, 45);
			instructions.Size = new Vector2(360, 90);
			instructions.HorizontalAlignment = Align.Left;
			instructions.VerticalAlignment = VerticalAlign.Top;
			instructions.ShowBackground = true;
			instructions.BackgroundColor = new FishColor(40, 40, 60, 180);
			mainPanel.AddChild(instructions);

			// Enabled checkbox - controls keyboard input vs hybrid mode
			enabledCheckbox = new CheckBox("Keyboard Control (uncheck for Hybrid)");
			enabledCheckbox.Position = new Vector2(20, 150);
			enabledCheckbox.Size = new Vector2(15, 15);
			enabledCheckbox.IsChecked = true;
			mainPanel.AddChild(enabledCheckbox);

			// Speed slider
			Label speedLabel = new Label("Cursor Speed:");
			speedLabel.Position = new Vector2(20, 185);
			speedLabel.Size = new Vector2(100, 20);
			speedLabel.Alignment = Align.Left;
			mainPanel.AddChild(speedLabel);

			speedSlider = new Slider();
			speedSlider.Position = new Vector2(130, 185);
			speedSlider.Size = new Vector2(200, 24);
			speedSlider.MinValue = 100;
			speedSlider.MaxValue = 800;
			speedSlider.Value = 400;
			speedSlider.ShowValueLabel = true;
		speedSlider.OnValueChanged += (slider, val) =>
			{
				FUI.VirtualMouse.Speed = val;
			};
			mainPanel.AddChild(speedSlider);

			// Cursor image selection
			Label cursorLabel = new Label("Cursor Image:");
			cursorLabel.Position = new Vector2(20, 215);
			cursorLabel.Size = new Vector2(100, 20);
			cursorLabel.Alignment = Align.Left;
			mainPanel.AddChild(cursorLabel);

			// Load cursor images
			string[] cursorFiles = new string[]
			{
				"(Default)",
				"arrow.png",
				"Beam.png",
				"Cursor_3.png",
				"Cursor_4.png",
				"Cursor_5.png",
				"Cursor_6.png",
				"help_win95.png"
			};

			DropDown cursorDropdown = new DropDown();
			cursorDropdown.Position = new Vector2(130, 212);
			cursorDropdown.Size = new Vector2(200, 24);
			foreach (var cursor in cursorFiles)
			{
				cursorDropdown.AddItem(new DropDownItem(cursor));
			}
			cursorDropdown.SelectIndex(0);
			cursorDropdown.OnItemSelected += (dd, item) =>
			{
				if (item.Text == "(Default)")
				{
					// Use default drawn cursor
					FUI.VirtualMouse.CursorImage = null;
				}
				else
				{
					// Load cursor image
					string path = $"data/images/cursors/{item.Text}";
					ImageRef cursorImg = FUI.Graphics.LoadImage(path);
					FUI.VirtualMouse.CursorImage = cursorImg;
				}
			};
			mainPanel.AddChild(cursorDropdown);

			// Position label
			positionLabel = new Label("Position: (0, 0)");
			positionLabel.Position = new Vector2(20, 250);
			positionLabel.Size = new Vector2(300, 20);
			positionLabel.Alignment = Align.Left;
			mainPanel.AddChild(positionLabel);

			// Status label
			statusLabel = new Label("Status: Virtual cursor enabled");
			statusLabel.Position = new Vector2(20, 275);
			statusLabel.Size = new Vector2(300, 20);
			statusLabel.Alignment = Align.Left;
			mainPanel.AddChild(statusLabel);

			// Test buttons
			Label testLabel = new Label("Test Buttons (click with virtual cursor):");
			testLabel.Position = new Vector2(20, 310);
			testLabel.Size = new Vector2(300, 20);
			testLabel.Alignment = Align.Left;
			mainPanel.AddChild(testLabel);

			Button btn1 = new Button();
			btn1.Text = "Button 1";
			btn1.Position = new Vector2(20, 335);
			btn1.Size = new Vector2(100, 35);
			btn1.OnButtonPressed += (ctrl, btn, pos) => statusLabel.Text = "Status: Button 1 clicked!";
			mainPanel.AddChild(btn1);

			Button btn2 = new Button();
			btn2.Text = "Button 2";
			btn2.Position = new Vector2(140, 335);
			btn2.Size = new Vector2(100, 35);
			btn2.OnButtonPressed += (ctrl, btn, pos) => statusLabel.Text = "Status: Button 2 clicked!";
			mainPanel.AddChild(btn2);

			Button btn3 = new Button();
			btn3.Text = "Button 3";
			btn3.Position = new Vector2(260, 335);
			btn3.Size = new Vector2(100, 35);
			btn3.OnButtonPressed += (ctrl, btn, pos) => statusLabel.Text = "Status: Button 3 clicked!";
			mainPanel.AddChild(btn3);

			// Checkbox test
			CheckBox testCheckbox = new CheckBox("Checkbox Test");
			testCheckbox.Position = new Vector2(20, 380);
			testCheckbox.Size = new Vector2(15, 15);
			mainPanel.AddChild(testCheckbox);

			// Screenshot button
			ImageRef iconCamera = FUI.Graphics.LoadImage("data/silk_icons/camera.png");
			Button screenshotBtn = new Button();
			screenshotBtn.Position = new Vector2(340, 10);
			screenshotBtn.Size = new Vector2(24, 24);
			screenshotBtn.Icon = iconCamera;
			screenshotBtn.IsImageButton = true;
			screenshotBtn.TooltipText = "Take screenshot";
			screenshotBtn.OnButtonPressed += (ctrl, btn, pos) => TakeScreenshot?.Invoke(Name);
			mainPanel.AddChild(screenshotBtn);

			// Second panel for more interaction targets
			Panel targetPanel = new Panel();
			targetPanel.Position = new Vector2(500, 50);
			targetPanel.Size = new Vector2(300, 200);
			targetPanel.Variant = PanelVariant.Dark;
			FUI.AddControl(targetPanel);

			Label targetTitle = new Label("More Targets");
			targetTitle.Position = new Vector2(20, 15);
			targetTitle.Size = new Vector2(200, 20);
			targetTitle.Alignment = Align.Left;
			targetPanel.AddChild(targetTitle);

			// Slider target
			Slider targetSlider = new Slider();
			targetSlider.Position = new Vector2(20, 50);
			targetSlider.Size = new Vector2(200, 24);
			targetSlider.MinValue = 0;
			targetSlider.MaxValue = 100;
			targetSlider.Value = 50;
			targetSlider.ShowValueLabel = true;
			targetSlider.OnValueChanged += (slider, val) =>
			{
				statusLabel.Text = $"Status: Slider value = {val:F0}";
			};
			targetPanel.AddChild(targetSlider);

			// NumericUpDown target
			NumericUpDown targetNumeric = new NumericUpDown(25, 0, 100, 5);
			targetNumeric.Position = new Vector2(20, 90);
			targetNumeric.Size = new Vector2(120, 24);
			targetNumeric.OnValueChanged += (num, val) =>
			{
				statusLabel.Text = $"Status: NumericUpDown value = {val}";
			};
			targetPanel.AddChild(targetNumeric);

			// ToggleSwitch target
			ToggleSwitch targetToggle = new ToggleSwitch();
			targetToggle.Position = new Vector2(20, 130);
			targetToggle.OnToggleChanged += (toggle, isOn) =>
			{
				statusLabel.Text = $"Status: Toggle is {(isOn ? "ON" : "OFF")}";
			};
			targetPanel.AddChild(targetToggle);

			Label toggleLabel = new Label("Toggle Switch");
			toggleLabel.Position = new Vector2(75, 130);
			toggleLabel.Size = new Vector2(150, 24);
			toggleLabel.Alignment = Align.Left;
			targetPanel.AddChild(toggleLabel);
		}

		/// <summary>
		/// Called every frame. Handles hybrid mode - when checkbox is unchecked, 
		/// sync virtual cursor with real mouse position and clicks.
		/// </summary>
		public void Update(float dt)
		{
			// Update position label
			positionLabel.Text = $"Position: ({FUI.VirtualMouse.Position.X:F0}, {FUI.VirtualMouse.Position.Y:F0})";

			// Hybrid mode: when keyboard control is unchecked, follow real mouse position and clicks
			if (!enabledCheckbox.IsChecked)
			{
				// Disable keyboard input handling in virtual mouse
				FUI.VirtualMouse.UseKeyboardInput = false;
				// Sync virtual cursor with real mouse position and button states
				FUI.VirtualMouse.SyncWithRealMouse(InputRef);
				FUI.VirtualMouse.SyncButtonsWithRealMouse(InputRef);
				statusLabel.Text = "Status: Hybrid mode (following real mouse + clicks)";
			}
			else
			{
				// Enable keyboard input handling
				FUI.VirtualMouse.UseKeyboardInput = true;
			}
		}

		private void UpdateStatus()
		{
			if (FUI.VirtualMouse.Enabled)
				statusLabel.Text = "Status: Virtual cursor enabled";
			else
				statusLabel.Text = "Status: Virtual cursor disabled (use real mouse)";
		}
	}
}

