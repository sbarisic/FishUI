using FishUI;
using FishUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FishUIDemos
{
	/// <summary>
	/// Demonstrates runtime theme switching between gwen.yaml and gwen2.yaml themes.
	/// </summary>
	public class SampleThemeSwitcher : ISample
	{
		FishUI.FishUI FUI;
		FishUISettings UISettings;
		Label CurrentThemeLabel;

		/// <summary>
		/// Display name of the sample.
		/// </summary>
		public string Name => "ThemeSwitcher";

		/// <summary>
		/// Action to take a screenshot, set by Program.cs.
		/// </summary>
		public TakeScreenshotFunc TakeScreenshot { get; set; }

		public FishUI.FishUI CreateUI(FishUISettings UISettings, IFishUIGfx Gfx, IFishUIInput Input, IFishUIEvents Events)
		{
			this.UISettings = UISettings;
			FUI = new FishUI.FishUI(UISettings, Gfx, Input, Events);
			FUI.Init();

			// Load initial theme
			FishUITheme theme = UISettings.LoadTheme(ThemePreferences.LoadThemePath(), applyImmediately: true);
			UISettings.OnThemeChanged += OnThemeChanged;

			return FUI;
		}

		public void Init()
		{
			// Panel container
			Panel panel = new Panel();
			panel.Position = new Vector2(50, 50);
			panel.Size = new Vector2(400, 300);
			FUI.AddControl(panel);

			// Title label
			Label titleLabel = new Label("Theme Switcher Demo");
			titleLabel.Position = new Vector2(20, 20);
			panel.AddChild(titleLabel);

			// Current theme label
			CurrentThemeLabel = new Label("Current Theme: gwen.yaml");
			CurrentThemeLabel.Position = new Vector2(20, 60);
			panel.AddChild(CurrentThemeLabel);

			// Theme 1 button (gwen.yaml)
			Button btnTheme1 = new Button();
			btnTheme1.Text = "Apply GWEN Theme";
			btnTheme1.Position = new Vector2(20, 100);
			btnTheme1.Size = new Vector2(160, 32);
			btnTheme1.OnButtonPressed += (ctrl, btn, pos) => SwitchTheme("data/themes/gwen.yaml");
			panel.AddChild(btnTheme1);

			// Theme 2 button (gwen2.yaml)
			Button btnTheme2 = new Button();
			btnTheme2.Text = "Apply GWEN2 Theme";
			btnTheme2.Position = new Vector2(200, 100);
			btnTheme2.Size = new Vector2(160, 32);
			btnTheme2.OnButtonPressed += (ctrl, btn, pos) => SwitchTheme("data/themes/gwen2.yaml");
			panel.AddChild(btnTheme2);

			// Demo controls to showcase theme changes
			Label demoLabel = new Label("Demo Controls:");
			demoLabel.Position = new Vector2(20, 150);
			panel.AddChild(demoLabel);

			CheckBox checkBox = new CheckBox("Sample Checkbox");
			checkBox.Position = new Vector2(20, 180);
			panel.AddChild(checkBox);

			Slider slider = new Slider();
			slider.Position = new Vector2(20, 220);
			slider.Size = new Vector2(200, 20);
			slider.Value = 0.5f;
			panel.AddChild(slider);

			ProgressBar progressBar = new ProgressBar();
			progressBar.Position = new Vector2(20, 250);
			progressBar.Size = new Vector2(200, 20);
			progressBar.Value = 0.7f;
			panel.AddChild(progressBar);

			// Screenshot button
			ImageRef iconCamera = FUI.Graphics.LoadImage("data/silk_icons/camera.png");
			Button screenshotBtn = new Button();
			screenshotBtn.Text = "Screenshot";
			screenshotBtn.Icon = iconCamera;
			screenshotBtn.IconPosition = IconPosition.Left;
			screenshotBtn.Position = new Vector2(250, 250);
			screenshotBtn.Size = new Vector2(120, 32);
			screenshotBtn.TooltipText = "Take a screenshot";
			screenshotBtn.OnButtonPressed += (btn, mbtn, pos) => TakeScreenshot?.Invoke(Name);
			panel.AddChild(screenshotBtn);
		}

		private void SwitchTheme(string themePath)
		{
			FishUITheme theme = UISettings.LoadTheme(themePath, applyImmediately: true);
		}

		private void OnThemeChanged(FishUITheme theme)
		{
			if (CurrentThemeLabel != null)
			{
				CurrentThemeLabel.Text = $"Current Theme: {theme.Name}";
			}
		}
	}
}

