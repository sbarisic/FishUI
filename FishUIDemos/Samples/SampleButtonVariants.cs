using FishUI;
using FishUI.Controls;
using System;
using System.Numerics;

namespace FishUIDemos
{
	/// <summary>
	/// Demonstrates button variants: Toggle, Repeat, ImageButton, Icon positions.
	/// </summary>
	public class SampleButtonVariants : ISample
	{
		FishUI.FishUI FUI;

		public string Name => "Button Variants";

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
			// Load icons
			ImageRef iconCamera = FUI.Graphics.LoadImage("data/silk_icons/camera.png");
			ImageRef iconStar = FUI.Graphics.LoadImage("data/silk_icons/star.png");
			ImageRef iconHeart = FUI.Graphics.LoadImage("data/silk_icons/heart.png");
			ImageRef iconCog = FUI.Graphics.LoadImage("data/silk_icons/cog.png");
			ImageRef iconSave = FUI.Graphics.LoadImage("data/silk_icons/disk.png");
			ImageRef iconFolder = FUI.Graphics.LoadImage("data/silk_icons/folder.png");
			ImageRef iconHelp = FUI.Graphics.LoadImage("data/silk_icons/help.png");

			// === Title ===
			Label titleLabel = new Label("Button Variants Demo");
			titleLabel.Position = new Vector2(20, 20);
			titleLabel.Size = new Vector2(300, 30);
			titleLabel.Alignment = Align.Left;
			FUI.AddControl(titleLabel);

			// Screenshot button
			Button screenshotBtn = new Button();
			screenshotBtn.Icon = iconCamera;
			screenshotBtn.Position = new Vector2(330, 20);
			screenshotBtn.Size = new Vector2(30, 30);
			screenshotBtn.IsImageButton = true;
			screenshotBtn.TooltipText = "Take a screenshot";
			screenshotBtn.OnButtonPressed += (btn, mbtn, pos) => TakeScreenshot?.Invoke(GetType().Name);
			FUI.AddControl(screenshotBtn);

			// === Standard Button ===
			Label standardLabel = new Label("Standard Button");
			standardLabel.Position = new Vector2(20, 70);
			standardLabel.Alignment = Align.Left;
			FUI.AddControl(standardLabel);

			Button standardBtn = new Button();
			standardBtn.Text = "Click Me";
			standardBtn.Position = new Vector2(20, 95);
			standardBtn.Size = new Vector2(120, 40);
			standardBtn.OnButtonPressed += (btn, mbtn, pos) => Console.WriteLine("Standard button clicked");
			FUI.AddControl(standardBtn);

			// === Toggle Button ===
			Label toggleLabel = new Label("Toggle Button");
			toggleLabel.Position = new Vector2(160, 70);
			toggleLabel.Alignment = Align.Left;
			FUI.AddControl(toggleLabel);

			Button toggleBtn = new Button();
			toggleBtn.Text = "Toggle Me";
			toggleBtn.Position = new Vector2(160, 95);
			toggleBtn.Size = new Vector2(120, 40);
			toggleBtn.IsToggleButton = true;
			toggleBtn.TooltipText = "Click to toggle on/off";
			toggleBtn.OnToggled += (btn, toggled) => Console.WriteLine($"Toggle: {toggled}");
			FUI.AddControl(toggleBtn);

			// === Repeat Button ===
			Label repeatLabel = new Label("Repeat Button (hold)");
			repeatLabel.Position = new Vector2(300, 70);
			repeatLabel.Alignment = Align.Left;
			FUI.AddControl(repeatLabel);

			int repeatCounter = 0;
			Label counterLabel = new Label("Count: 0");
			counterLabel.Position = new Vector2(300, 145);
			counterLabel.Size = new Vector2(100, 20);
			counterLabel.Alignment = Align.Left;
			FUI.AddControl(counterLabel);

			Button repeatBtn = new Button();
			repeatBtn.Text = "+ Hold";
			repeatBtn.Position = new Vector2(300, 95);
			repeatBtn.Size = new Vector2(100, 40);
			repeatBtn.IsRepeatButton = true;
			repeatBtn.RepeatDelay = 0.3f;
			repeatBtn.RepeatInterval = 0.05f;
			repeatBtn.TooltipText = "Hold to increment continuously";
			repeatBtn.OnButtonPressed += (btn, mbtn, pos) =>
			{
				repeatCounter++;
				counterLabel.Text = $"Count: {repeatCounter}";
			};
			FUI.AddControl(repeatBtn);

			Button resetBtn = new Button();
			resetBtn.Text = "Reset";
			resetBtn.Position = new Vector2(410, 95);
			resetBtn.Size = new Vector2(60, 40);
			resetBtn.OnButtonPressed += (btn, mbtn, pos) =>
			{
				repeatCounter = 0;
				counterLabel.Text = "Count: 0";
			};
			FUI.AddControl(resetBtn);

			// === Icon Buttons with Text ===
			Label iconTextLabel = new Label("Icon + Text Buttons");
			iconTextLabel.Position = new Vector2(20, 180);
			iconTextLabel.Alignment = Align.Left;
			FUI.AddControl(iconTextLabel);

			Button iconLeftBtn = new Button();
			iconLeftBtn.Text = "Save";
			iconLeftBtn.Icon = iconSave;
			iconLeftBtn.IconPosition = IconPosition.Left;
			iconLeftBtn.Position = new Vector2(20, 205);
			iconLeftBtn.Size = new Vector2(100, 35);
			iconLeftBtn.TooltipText = "Icon on left";
			FUI.AddControl(iconLeftBtn);

			Button iconRightBtn = new Button();
			iconRightBtn.Text = "Open";
			iconRightBtn.Icon = iconFolder;
			iconRightBtn.IconPosition = IconPosition.Right;
			iconRightBtn.Position = new Vector2(130, 205);
			iconRightBtn.Size = new Vector2(100, 35);
			iconRightBtn.TooltipText = "Icon on right";
			FUI.AddControl(iconRightBtn);

			Button iconTopBtn = new Button();
			iconTopBtn.Text = "Help";
			iconTopBtn.Icon = iconHelp;
			iconTopBtn.IconPosition = IconPosition.Top;
			iconTopBtn.Position = new Vector2(240, 195);
			iconTopBtn.Size = new Vector2(60, 55);
			iconTopBtn.TooltipText = "Icon on top";
			FUI.AddControl(iconTopBtn);

			Button iconBottomBtn = new Button();
			iconBottomBtn.Text = "Settings";
			iconBottomBtn.Icon = iconCog;
			iconBottomBtn.IconPosition = IconPosition.Bottom;
			iconBottomBtn.Position = new Vector2(310, 195);
			iconBottomBtn.Size = new Vector2(80, 55);
			iconBottomBtn.TooltipText = "Icon on bottom";
			FUI.AddControl(iconBottomBtn);

			// === ImageButton (Icon Only) ===
			Label imageButtonLabel = new Label("ImageButton (icon-only, toolbar style)");
			imageButtonLabel.Position = new Vector2(20, 270);
			imageButtonLabel.Alignment = Align.Left;
			FUI.AddControl(imageButtonLabel);

			Panel toolbarPanel = new Panel();
			toolbarPanel.Position = new Vector2(20, 295);
			toolbarPanel.Size = new Vector2(200, 40);
			toolbarPanel.Variant = PanelVariant.Dark;
			FUI.AddControl(toolbarPanel);

			Button imgBtn1 = new Button();
			imgBtn1.Icon = iconSave;
			imgBtn1.Position = new Vector2(5, 5);
			imgBtn1.Size = new Vector2(28, 28);
			imgBtn1.IsImageButton = true;
			imgBtn1.TooltipText = "Save";
			toolbarPanel.AddChild(imgBtn1);

			Button imgBtn2 = new Button();
			imgBtn2.Icon = iconFolder;
			imgBtn2.Position = new Vector2(38, 5);
			imgBtn2.Size = new Vector2(28, 28);
			imgBtn2.IsImageButton = true;
			imgBtn2.TooltipText = "Open folder";
			toolbarPanel.AddChild(imgBtn2);

			Button imgBtn3 = new Button();
			imgBtn3.Icon = iconStar;
			imgBtn3.Position = new Vector2(71, 5);
			imgBtn3.Size = new Vector2(28, 28);
			imgBtn3.IsImageButton = true;
			imgBtn3.IsToggleButton = true;
			imgBtn3.TooltipText = "Favorite (toggle)";
			toolbarPanel.AddChild(imgBtn3);

			Button imgBtn4 = new Button();
			imgBtn4.Icon = iconHeart;
			imgBtn4.Position = new Vector2(104, 5);
			imgBtn4.Size = new Vector2(28, 28);
			imgBtn4.IsImageButton = true;
			imgBtn4.IsToggleButton = true;
			imgBtn4.TooltipText = "Like (toggle)";
			toolbarPanel.AddChild(imgBtn4);

			Button imgBtn5 = new Button();
			imgBtn5.Icon = iconCog;
			imgBtn5.Position = new Vector2(137, 5);
			imgBtn5.Size = new Vector2(28, 28);
			imgBtn5.IsImageButton = true;
			imgBtn5.TooltipText = "Settings";
			toolbarPanel.AddChild(imgBtn5);

			// === Disabled Buttons ===
			Label disabledLabel = new Label("Disabled State");
			disabledLabel.Position = new Vector2(20, 350);
			disabledLabel.Alignment = Align.Left;
			FUI.AddControl(disabledLabel);

			Button disabledBtn = new Button();
			disabledBtn.Text = "Disabled";
			disabledBtn.Position = new Vector2(20, 375);
			disabledBtn.Size = new Vector2(100, 35);
			disabledBtn.Disabled = true;
			FUI.AddControl(disabledBtn);

			Button disabledIconBtn = new Button();
			disabledIconBtn.Icon = iconSave;
			disabledIconBtn.Position = new Vector2(130, 375);
			disabledIconBtn.Size = new Vector2(28, 28);
			disabledIconBtn.IsImageButton = true;
			disabledIconBtn.Disabled = true;
			disabledIconBtn.TooltipText = "Disabled ImageButton";
			FUI.AddControl(disabledIconBtn);

			// === Combined: Toggle + Repeat + Icon ===
			Label combinedLabel = new Label("Combined Features");
			combinedLabel.Position = new Vector2(20, 420);
			combinedLabel.Alignment = Align.Left;
			FUI.AddControl(combinedLabel);

			Button combinedBtn = new Button();
			combinedBtn.Text = "Toggle + Icon";
			combinedBtn.Icon = iconHeart;
			combinedBtn.IconPosition = IconPosition.Left;
			combinedBtn.Position = new Vector2(20, 445);
			combinedBtn.Size = new Vector2(140, 40);
			combinedBtn.IsToggleButton = true;
			combinedBtn.TooltipText = "Toggle button with icon";
			combinedBtn.OnToggled += (btn, toggled) => Console.WriteLine($"Combined toggle: {toggled}");
			FUI.AddControl(combinedBtn);
		}
	}
}

