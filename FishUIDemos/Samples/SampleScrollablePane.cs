using FishUI;
using FishUI.Controls;
using System;
using System.Numerics;

namespace FishUIDemos
{
	/// <summary>
	/// Demonstrates the ScrollablePane control with automatic scrollbars.
	/// Shows a resizable window containing a scrollable pane filled with buttons.
	/// </summary>
	public class SampleScrollablePane : ISample
	{
		FishUI.FishUI FUI;

		public string Name => "ScrollablePane";

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
			// === Title ===
			Label titleLabel = new Label("ScrollablePane Demo");
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

			// === Description ===
			Label descLabel = new Label("Resize the window to see the scrollbars appear/disappear automatically.");
			descLabel.Position = new Vector2(20, 55);
			descLabel.Size = new Vector2(600, 20);
			descLabel.Alignment = Align.Left;
			FUI.AddControl(descLabel);

		// === Resizable Window with ScrollablePane ===
			Window scrollWindow = new Window("Scrollable Content");
			scrollWindow.Position = new Vector2(20, 90);
			scrollWindow.Size = new Vector2(300, 400);
			scrollWindow.IsResizable = true;
			scrollWindow.MinSize = new Vector2(150, 150);
			scrollWindow.ShowCloseButton = false;
			FUI.AddControl(scrollWindow);

			// Create ScrollablePane to fill the window content area
			// Window content area = Size - titlebar (24) - resize handle margin (20) on sides and bottom
			// Position is relative to content panel which starts at (ResizeHandleSize, TitlebarHeight)
			ScrollablePane scrollPane = new ScrollablePane();
			scrollPane.Position = new Vector2(0, 0);
			scrollPane.Size = scrollWindow.GetContentSize();
			scrollPane.Anchor = FishUIAnchor.All;
			scrollWindow.AddChild(scrollPane);

			// Add 20 buttons stacked vertically
			float buttonHeight = 35f;
			float buttonSpacing = 5f;
			float buttonWidth = 250f;
			float margin = 10f;

			for (int i = 0; i < 20; i++)
			{
				Button btn = new Button();
				btn.Text = $"Button {i + 1}";
				btn.Position = new Vector2(margin, margin + i * (buttonHeight + buttonSpacing));
				btn.Size = new Vector2(buttonWidth, buttonHeight);
				btn.TooltipText = $"This is button number {i + 1}";

				int buttonIndex = i + 1;
				btn.OnButtonPressed += (sender, mbtn, pos) =>
				{
					Console.WriteLine($"Button {buttonIndex} clicked!");
				};

				scrollPane.AddChild(btn);
			}

			// === Second Example: Horizontal Scrolling ===
			Window horizWindow = new Window("Horizontal Scroll");
			horizWindow.Position = new Vector2(350, 90);
			horizWindow.Size = new Vector2(400, 150);
			horizWindow.IsResizable = true;
			horizWindow.MinSize = new Vector2(200, 120);
			horizWindow.ShowCloseButton = false;
			FUI.AddControl(horizWindow);

			ScrollablePane horizPane = new ScrollablePane();
			horizPane.Position = new Vector2(0, 0);
			horizPane.Size = horizWindow.GetContentSize();
			horizPane.Anchor = FishUIAnchor.All;
			horizPane.ShowVerticalScrollBar = false;
			horizWindow.AddChild(horizPane);

			// Add buttons horizontally
			float hButtonWidth = 100f;
			float hButtonHeight = 80f;

			for (int i = 0; i < 10; i++)
			{
				Button btn = new Button();
				btn.Text = $"H-{i + 1}";
				btn.Position = new Vector2(margin + i * (hButtonWidth + buttonSpacing), margin);
				btn.Size = new Vector2(hButtonWidth, hButtonHeight);
				btn.TooltipText = $"Horizontal button {i + 1}";
				horizPane.AddChild(btn);
			}

			// === Third Example: Both Scrollbars ===
			Window gridWindow = new Window("Grid (Both Scrollbars)");
			gridWindow.Position = new Vector2(350, 260);
			gridWindow.Size = new Vector2(300, 230);
			gridWindow.IsResizable = true;
			gridWindow.MinSize = new Vector2(150, 150);
			gridWindow.ShowCloseButton = false;
			FUI.AddControl(gridWindow);

			ScrollablePane gridPane = new ScrollablePane();
			gridPane.Position = new Vector2(0, 0);
			gridPane.Size = gridWindow.GetContentSize();
			gridPane.Anchor = FishUIAnchor.All;
			gridWindow.AddChild(gridPane);

			// Add a grid of buttons
			int cols = 5;
			int rows = 5;
			float gridBtnSize = 80f;

			for (int row = 0; row < rows; row++)
			{
				for (int col = 0; col < cols; col++)
				{
					Button btn = new Button();
					btn.Text = $"{row},{col}";
					btn.Position = new Vector2(
						margin + col * (gridBtnSize + buttonSpacing),
						margin + row * (gridBtnSize + buttonSpacing)
					);
					btn.Size = new Vector2(gridBtnSize, gridBtnSize);
					btn.TooltipText = $"Grid button at row {row}, column {col}";
					gridPane.AddChild(btn);
				}
			}

			// === Instructions ===
			Label instructLabel = new Label("Try resizing each window to see scrollbars appear when content exceeds visible area.");
			instructLabel.Position = new Vector2(20, 510);
			instructLabel.Size = new Vector2(700, 20);
			instructLabel.Alignment = Align.Left;
			FUI.AddControl(instructLabel);
		}

		public void Update(float Dt)
		{
		}
	}
}

