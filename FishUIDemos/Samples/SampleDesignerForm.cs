using FishUI;
using FishUI.Controls;
using FishUIDemos.Forms;
using System;
using System.Numerics;

namespace FishUIDemos
{
	/// <summary>
	/// Sample demonstrating the use of IFishUIForm designer-generated forms.
	/// Shows how to create forms using the FishUI Layout Editor and load them at runtime.
	/// </summary>
	public class SampleDesignerForm : ISample
	{
		private FishUI.FishUI FUI;
		private IFishUIForm _form;

		public string Name => "Designer Form";

		public TakeScreenshotFunc TakeScreenshot { get; set; }

		public FishUI.FishUI CreateUI(FishUISettings UISettings, IFishUIGfx Gfx, IFishUIInput Input, IFishUIEvents Events)
		{
			FUI = new FishUI.FishUI(UISettings, Gfx, Input, Events);
			FUI.Init();

			UISettings.DebugEnabled = false;
			FishUITheme theme = UISettings.LoadTheme(ThemePreferences.LoadThemePath(), applyImmediately: true);

			return FUI;
		}

		public void Init()
		{
			// Add a label explaining the demo
			var infoLabel = new Label();
			infoLabel.Position = new Vector2(10, 10);
			infoLabel.Size = new Vector2(700, 48);
			infoLabel.Text = "This form was created using the FishUI Layout Editor's 'Export as C#...' feature.\n" +
			                 "The Designer.cs file contains auto-generated control creation code.";
			FUI.AddControl(infoLabel);

			// Create an instance of the designer-generated form
			_form = new MyApp.Forms.MainForm();

			// Load controls from the designer
			_form.LoadControls(FUI);

			// Wire up event handlers from the user code
			//_form.SetupEventHandlers();

			// Call OnLoaded (can be overridden if needed)
			_form.OnLoaded();

			// Add screenshot button
			ImageRef iconCamera = FUI.Graphics.LoadImage("data/silk_icons/camera.png");
			Button screenshotBtn = new Button();
			screenshotBtn.Icon = iconCamera;
			screenshotBtn.Position = new Vector2(720, 10);
			screenshotBtn.Size = new Vector2(30, 30);
			screenshotBtn.IsImageButton = true;
			screenshotBtn.TooltipText = "Take a screenshot";
			screenshotBtn.OnButtonPressed += (btn, mbtn, pos) => TakeScreenshot?.Invoke(GetType().Name);
			FUI.AddControl(screenshotBtn);
		}
	}
}
