using FishUI;
using FishUI.Controls;
using System;
using System.Numerics;

namespace FishUIDemos
{
	/// <summary>
	/// Demonstrates the ToastNotification system with different message types
	/// and customization options.
	/// </summary>
	public class SampleToastNotification : ISample
	{
		FishUI.FishUI FUI;
		ToastNotification _toastSystem;
		int _counter = 0;

		public string Name => "Toast Notifications";

		public TakeScreenshotFunc TakeScreenshot { get; set; }

		public FishUI.FishUI CreateUI(FishUISettings UISettings, IFishUIGfx Gfx, IFishUIInput Input, IFishUIEvents Events)
		{
			FUI = new FishUI.FishUI(UISettings, Gfx, Input, Events);
			FUI.Init();

			FishUITheme theme = UISettings.LoadTheme(ThemePreferences.LoadThemePath(), applyImmediately: true);

			return FUI;
		}

		public void Init()
		{
			// Add the toast notification system (should be added once, rendered on top)
			_toastSystem = new ToastNotification();
			_toastSystem.MaxToasts = 5;
			_toastSystem.DefaultDuration = 4f;
			FUI.AddControl(_toastSystem);

			// === Title ===
			Label titleLabel = new Label("Toast Notification Demo");
			titleLabel.Position = new Vector2(20, 20);
			titleLabel.Size = new Vector2(300, 30);
			titleLabel.Alignment = Align.Left;
			FUI.AddControl(titleLabel);

			// Screenshot button
			ImageRef iconCamera = FUI.Graphics.LoadImage("data/silk_icons/camera.png");
			Button screenshotBtn = new Button();
			screenshotBtn.Icon = iconCamera;
			screenshotBtn.Position = new Vector2(330, 20);
			screenshotBtn.Size = new Vector2(30, 30);
			screenshotBtn.IsImageButton = true;
			screenshotBtn.TooltipText = "Take a screenshot";
			screenshotBtn.OnButtonPressed += (btn, mbtn, pos) => TakeScreenshot?.Invoke(Name);
			FUI.AddControl(screenshotBtn);

			// ============ Toast Type Buttons ============

			Label typeLabel = new Label("Show Toast by Type:");
			typeLabel.Position = new Vector2(20, 60);
			typeLabel.Size = new Vector2(200, 20);
			typeLabel.Alignment = Align.Left;
			FUI.AddControl(typeLabel);

			// Info button
			Button infoBtn = new Button();
			infoBtn.Text = "Info";
			infoBtn.Position = new Vector2(20, 90);
			infoBtn.Size = new Vector2(100, 32);
			infoBtn.OnButtonPressed += (b, m, p) =>
			{
				_counter++;
				_toastSystem.ShowInfo($"Information message #{_counter}");
			};
			FUI.AddControl(infoBtn);

			// Success button
			Button successBtn = new Button();
			successBtn.Text = "Success";
			successBtn.Position = new Vector2(130, 90);
			successBtn.Size = new Vector2(100, 32);
			successBtn.OnButtonPressed += (b, m, p) =>
			{
				_counter++;
				_toastSystem.ShowSuccess($"Operation completed successfully! #{_counter}");
			};
			FUI.AddControl(successBtn);

			// Warning button
			Button warningBtn = new Button();
			warningBtn.Text = "Warning";
			warningBtn.Position = new Vector2(240, 90);
			warningBtn.Size = new Vector2(100, 32);
			warningBtn.OnButtonPressed += (b, m, p) =>
			{
				_counter++;
				_toastSystem.ShowWarning($"Warning: Please check settings #{_counter}");
			};
			FUI.AddControl(warningBtn);

			// Error button
			Button errorBtn = new Button();
			errorBtn.Text = "Error";
			errorBtn.Position = new Vector2(350, 90);
			errorBtn.Size = new Vector2(100, 32);
			errorBtn.OnButtonPressed += (b, m, p) =>
			{
				_counter++;
				_toastSystem.ShowError($"Error: Something went wrong! #{_counter}");
			};
			FUI.AddControl(errorBtn);

			// ============ Toast with Title ============

			Label titleToastLabel = new Label("Show Toast with Title:");
			titleToastLabel.Position = new Vector2(20, 140);
			titleToastLabel.Size = new Vector2(200, 20);
			titleToastLabel.Alignment = Align.Left;
			FUI.AddControl(titleToastLabel);

			Button titledInfoBtn = new Button();
			titledInfoBtn.Text = "Info + Title";
			titledInfoBtn.Position = new Vector2(20, 170);
			titledInfoBtn.Size = new Vector2(110, 32);
			titledInfoBtn.OnButtonPressed += (b, m, p) =>
			{
				_toastSystem.Show("System Update", "A new version is available.", ToastType.Info);
			};
			FUI.AddControl(titledInfoBtn);

			Button titledSuccessBtn = new Button();
			titledSuccessBtn.Text = "Success + Title";
			titledSuccessBtn.Position = new Vector2(140, 170);
			titledSuccessBtn.Size = new Vector2(120, 32);
			titledSuccessBtn.OnButtonPressed += (b, m, p) =>
			{
				_toastSystem.Show("File Saved", "Document saved successfully.", ToastType.Success);
			};
			FUI.AddControl(titledSuccessBtn);

			Button titledErrorBtn = new Button();
			titledErrorBtn.Text = "Error + Title";
			titledErrorBtn.Position = new Vector2(270, 170);
			titledErrorBtn.Size = new Vector2(110, 32);
			titledErrorBtn.OnButtonPressed += (b, m, p) =>
			{
				_toastSystem.Show("Connection Failed", "Unable to reach server.", ToastType.Error);
			};
			FUI.AddControl(titledErrorBtn);

			// ============ Custom Duration ============

			Label durationLabel = new Label("Custom Duration:");
			durationLabel.Position = new Vector2(20, 220);
			durationLabel.Size = new Vector2(150, 20);
			durationLabel.Alignment = Align.Left;
			FUI.AddControl(durationLabel);

			Button shortBtn = new Button();
			shortBtn.Text = "1 Second";
			shortBtn.Position = new Vector2(20, 250);
			shortBtn.Size = new Vector2(100, 32);
			shortBtn.OnButtonPressed += (b, m, p) =>
			{
				_toastSystem.Show("Quick notification!", ToastType.Info, 1f);
			};
			FUI.AddControl(shortBtn);

			Button longBtn = new Button();
			longBtn.Text = "10 Seconds";
			longBtn.Position = new Vector2(130, 250);
			longBtn.Size = new Vector2(100, 32);
			longBtn.OnButtonPressed += (b, m, p) =>
			{
				_toastSystem.Show("This will stay for 10 seconds", ToastType.Warning, 10f);
			};
			FUI.AddControl(longBtn);

			// ============ Flood Test ============

			Label floodLabel = new Label("Stress Test:");
			floodLabel.Position = new Vector2(20, 300);
			floodLabel.Size = new Vector2(150, 20);
			floodLabel.Alignment = Align.Left;
			FUI.AddControl(floodLabel);

			Button floodBtn = new Button();
			floodBtn.Text = "Add 10 Toasts";
			floodBtn.Position = new Vector2(20, 330);
			floodBtn.Size = new Vector2(120, 32);
			floodBtn.OnButtonPressed += (b, m, p) =>
			{
				for (int i = 0; i < 10; i++)
				{
					_counter++;
					ToastType type = (ToastType)(i % 4);
					_toastSystem.Show($"Toast message #{_counter}", type);
				}
			};
			FUI.AddControl(floodBtn);

			Button clearBtn = new Button();
			clearBtn.Text = "Clear All";
			clearBtn.Position = new Vector2(150, 330);
			clearBtn.Size = new Vector2(100, 32);
			clearBtn.OnButtonPressed += (b, m, p) =>
			{
				_toastSystem.ClearAll();
			};
			FUI.AddControl(clearBtn);

			// ============ Active Count Display ============

			Label activeLabel = new Label("Active toasts: 0");
			activeLabel.Position = new Vector2(20, 380);
			activeLabel.Size = new Vector2(200, 20);
			activeLabel.Alignment = Align.Left;
			activeLabel.ID = "activeCountLabel";
			FUI.AddControl(activeLabel);

			// ============ Info ============

			Label infoLabel = new Label("Toast notifications appear in the top-right corner and auto-dismiss after timeout.");
			infoLabel.Position = new Vector2(20, 420);
			infoLabel.Size = new Vector2(500, 20);
			infoLabel.Alignment = Align.Left;
			FUI.AddControl(infoLabel);
		}

		public void Update(float dt)
		{
			// Update active count label
			var label = FUI.FindControlByID<Label>("activeCountLabel");
			if (label != null)
			{
				label.Text = $"Active toasts: {_toastSystem.ActiveCount}";
			}
		}
	}
}
