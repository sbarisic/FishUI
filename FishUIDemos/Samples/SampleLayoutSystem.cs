using FishUI;
using FishUI.Controls;
using System;
using System.Numerics;

namespace FishUIDemos
{
	/// <summary>
	/// Demonstrates layout system features: Margin, Padding, Anchors, StackLayout, FlowLayout, GridLayout.
	/// Uses Window controls as containers to show real-world usage.
	/// </summary>
	public class SampleLayoutSystem : ISample
	{
		FishUI.FishUI FUI;

		public string Name => "LayoutSystem";

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
			Label titleLabel = new Label("Layout System Demo");
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

			// === Panel Variants ===
			Label panelLabel = new Label("Panel Variants");
			panelLabel.Position = new Vector2(20, 60);
			panelLabel.Alignment = Align.Left;
			FUI.AddControl(panelLabel);

			Panel panelNormal = new Panel();
			panelNormal.Position = new Vector2(20, 85);
			panelNormal.Size = new Vector2(80, 50);
			panelNormal.Variant = PanelVariant.Normal;
			FUI.AddControl(panelNormal);
			Label labelNormal = new Label("Normal");
			labelNormal.Position = new Vector2(5, 15);
			labelNormal.Alignment = Align.Left;
			panelNormal.AddChild(labelNormal);

			Panel panelBright = new Panel();
			panelBright.Position = new Vector2(110, 85);
			panelBright.Size = new Vector2(80, 50);
			panelBright.Variant = PanelVariant.Bright;
			FUI.AddControl(panelBright);
			Label labelBright = new Label("Bright");
			labelBright.Position = new Vector2(5, 15);
			labelBright.Alignment = Align.Left;
			panelBright.AddChild(labelBright);

			Panel panelDark = new Panel();
			panelDark.Position = new Vector2(200, 85);
			panelDark.Size = new Vector2(80, 50);
			panelDark.Variant = PanelVariant.Dark;
			FUI.AddControl(panelDark);
			Label labelDark = new Label("Dark");
			labelDark.Position = new Vector2(5, 15);
			labelDark.Alignment = Align.Left;
			panelDark.AddChild(labelDark);

			Panel panelHighlight = new Panel();
			panelHighlight.Position = new Vector2(290, 85);
			panelHighlight.Size = new Vector2(90, 50);
			panelHighlight.Variant = PanelVariant.Highlight;
			FUI.AddControl(panelHighlight);
			Label labelHighlight = new Label("Highlight");
			labelHighlight.Position = new Vector2(5, 15);
			labelHighlight.Alignment = Align.Left;
			panelHighlight.AddChild(labelHighlight);

			// === Border Styles ===
			Label borderLabel = new Label("Border Styles");
			borderLabel.Position = new Vector2(20, 145);
			borderLabel.Alignment = Align.Left;
			FUI.AddControl(borderLabel);

			Panel panelSolid = new Panel();
			panelSolid.Position = new Vector2(20, 170);
			panelSolid.Size = new Vector2(80, 50);
			panelSolid.IsTransparent = true;
			panelSolid.BorderStyle = BorderStyle.Solid;
			panelSolid.BorderColor = new FishColor(100, 100, 100, 255);
			panelSolid.BorderThickness = 2f;
			FUI.AddControl(panelSolid);
			Label labelSolid = new Label("Solid");
			labelSolid.Position = new Vector2(5, 15);
			labelSolid.Alignment = Align.Left;
			panelSolid.AddChild(labelSolid);

			Panel panelInset = new Panel();
			panelInset.Position = new Vector2(110, 170);
			panelInset.Size = new Vector2(80, 50);
			panelInset.IsTransparent = true;
			panelInset.BorderStyle = BorderStyle.Inset;
			panelInset.BorderColor = new FishColor(150, 150, 150, 255);
			panelInset.BorderThickness = 2f;
			FUI.AddControl(panelInset);
			Label labelInset = new Label("Inset");
			labelInset.Position = new Vector2(5, 15);
			labelInset.Alignment = Align.Left;
			panelInset.AddChild(labelInset);

			Panel panelOutset = new Panel();
			panelOutset.Position = new Vector2(200, 170);
			panelOutset.Size = new Vector2(80, 50);
			panelOutset.IsTransparent = true;
			panelOutset.BorderStyle = BorderStyle.Outset;
			panelOutset.BorderColor = new FishColor(150, 150, 150, 255);
			panelOutset.BorderThickness = 2f;
			FUI.AddControl(panelOutset);
			Label labelOutset = new Label("Outset");
			labelOutset.Position = new Vector2(5, 15);
			labelOutset.Alignment = Align.Left;
			panelOutset.AddChild(labelOutset);

			// === Margin & Padding ===
			Label marginPaddingLabel = new Label("Margin & Padding");
			marginPaddingLabel.Position = new Vector2(20, 235);
			marginPaddingLabel.Alignment = Align.Left;
			FUI.AddControl(marginPaddingLabel);

			Panel paddedPanel = new Panel();
			paddedPanel.Position = new Vector2(20, 260);
			paddedPanel.Size = new Vector2(160, 90);
			paddedPanel.Padding = new FishUIMargin(15);
			paddedPanel.Variant = PanelVariant.Bright;
			paddedPanel.TooltipText = "Panel with Padding=15";
			FUI.AddControl(paddedPanel);

			Button paddedBtn = new Button();
			paddedBtn.Text = "Child at (0,0)";
			paddedBtn.Position = new Vector2(0, 0);
			paddedBtn.Size = new Vector2(100, 25);
			paddedBtn.TooltipText = "Positioned at (0,0) but offset by parent padding";
			paddedPanel.AddChild(paddedBtn);

			Label paddingNote = new Label("Padding=15");
			paddingNote.Position = new Vector2(0, 35);
			paddingNote.Alignment = Align.Left;
			paddedPanel.AddChild(paddingNote);

			Panel marginPanel = new Panel();
			marginPanel.Position = new Vector2(200, 260);
			marginPanel.Size = new Vector2(160, 90);
			marginPanel.Variant = PanelVariant.Normal;
			marginPanel.TooltipText = "Panel with child that has Margin";
			FUI.AddControl(marginPanel);

			Button marginBtn = new Button();
			marginBtn.Text = "Margin=10";
			marginBtn.Position = new Vector2(0, 0);
			marginBtn.Size = new Vector2(100, 25);
			marginBtn.Margin = new FishUIMargin(10);
			marginBtn.TooltipText = "Button with Margin=10 all sides";
			marginPanel.AddChild(marginBtn);

			Label marginNote = new Label("Child Margin=10");
			marginNote.Position = new Vector2(10, 50);
			marginNote.Alignment = Align.Left;
			marginPanel.AddChild(marginNote);

			// === Anchor System ===
			Label anchorLabel = new Label("Anchor System (resize window to test)");
			anchorLabel.Position = new Vector2(20, 365);
			anchorLabel.Alignment = Align.Left;
			FUI.AddControl(anchorLabel);

			Window anchorWindow = new Window("Anchors Demo", new Vector2(260, 150));
			anchorWindow.Position = new Vector2(20, 390);
			anchorWindow.IsResizable = true;
			anchorWindow.ShowCloseButton = false;
			anchorWindow.MinSize = new Vector2(180, 100);
			FUI.AddControl(anchorWindow);

			Button anchorTL = new Button();
			anchorTL.Text = "TL";
			anchorTL.Position = new Vector2(5, 5);
			anchorTL.Size = new Vector2(45, 25);
			anchorTL.Anchor = FishUIAnchor.TopLeft;
			anchorTL.TooltipText = "TopLeft anchor";
			anchorWindow.AddChild(anchorTL);

			Button anchorTR = new Button();
			anchorTR.Text = "TR";
			anchorTR.Position = new Vector2(190, 5);
			anchorTR.Size = new Vector2(45, 25);
			anchorTR.Anchor = FishUIAnchor.TopRight;
			anchorTR.TooltipText = "TopRight anchor";
			anchorWindow.AddChild(anchorTR);

			Button anchorBL = new Button();
			anchorBL.Text = "BL";
			anchorBL.Position = new Vector2(5, 70);
			anchorBL.Size = new Vector2(45, 25);
			anchorBL.Anchor = FishUIAnchor.BottomLeft;
			anchorBL.TooltipText = "BottomLeft anchor";
			anchorWindow.AddChild(anchorBL);

			Button anchorBR = new Button();
			anchorBR.Text = "BR";
			anchorBR.Position = new Vector2(190, 70);
			anchorBR.Size = new Vector2(45, 25);
			anchorBR.Anchor = FishUIAnchor.BottomRight;
			anchorBR.TooltipText = "BottomRight anchor";
			anchorWindow.AddChild(anchorBR);

			Button anchorH = new Button();
			anchorH.Text = "Horizontal";
			anchorH.Position = new Vector2(55, 35);
			anchorH.Size = new Vector2(130, 25);
			anchorH.Anchor = FishUIAnchor.Horizontal;
			anchorH.TooltipText = "Horizontal anchor (stretches)";
			anchorWindow.AddChild(anchorH);

			// === StackLayout in Window ===
			Window stackWindow = new Window("StackLayout Demo", new Vector2(180, 320));
			stackWindow.Position = new Vector2(300, 235);
			stackWindow.IsResizable = true;
			stackWindow.ShowCloseButton = false;
			stackWindow.MinSize = new Vector2(150, 200);
			FUI.AddControl(stackWindow);

			// Vertical Stack
			StackLayout vStack = new StackLayout();
			vStack.Position = new Vector2(5, 5);
			vStack.Size = new Vector2(140, 130);
			vStack.Anchor = FishUIAnchor.Horizontal; // Stretch with window width
			vStack.Orientation = StackOrientation.Vertical;
			vStack.Spacing = 8;
			vStack.LayoutPadding = 10;
			vStack.IsTransparent = false;
			stackWindow.AddChild(vStack);

			Button vBtn1 = new Button();
			vBtn1.Text = "Stack 1";
			vBtn1.Size = new Vector2(110, 28);
			vStack.AddChild(vBtn1);

			Button vBtn2 = new Button();
			vBtn2.Text = "Stack 2";
			vBtn2.Size = new Vector2(110, 28);
			vStack.AddChild(vBtn2);

			CheckBox vCheck = new CheckBox("Check");
			vCheck.Size = new Vector2(15, 15);
			vStack.AddChild(vCheck);

			ToggleSwitch vToggle = new ToggleSwitch();
			vToggle.Size = new Vector2(50, 22);
			vStack.AddChild(vToggle);

			// Horizontal Stack (inside same window)
			Label hStackLabel = new Label("Horizontal:");
			hStackLabel.Position = new Vector2(5, 140);
			hStackLabel.Alignment = Align.Left;
			stackWindow.AddChild(hStackLabel);

			StackLayout hStack = new StackLayout();
			hStack.Position = new Vector2(5, 160);
			hStack.Size = new Vector2(160, 45);
			hStack.Anchor = FishUIAnchor.Horizontal; // Stretch with window width
			hStack.Orientation = StackOrientation.Horizontal;
			hStack.Spacing = 5;
			hStack.LayoutPadding = 5;
			hStack.IsTransparent = false;
			stackWindow.AddChild(hStack);

			for (char c = 'A'; c <= 'D'; c++)
			{
				Button hBtn = new Button();
				hBtn.Text = c.ToString();
				hBtn.Size = new Vector2(32, 32);
				hStack.AddChild(hBtn);
			}

			// Nested Stacks (inside same window)
			Label nestedLabel = new Label("Nested:");
			nestedLabel.Position = new Vector2(5, 210);
			nestedLabel.Alignment = Align.Left;
			stackWindow.AddChild(nestedLabel);

			StackLayout outerStack = new StackLayout();
			outerStack.Position = new Vector2(5, 230);
			outerStack.Size = new Vector2(160, 55);
			outerStack.Anchor = FishUIAnchor.Horizontal; // Stretch with window width
			outerStack.Orientation = StackOrientation.Horizontal;
			outerStack.Spacing = 5;
			outerStack.LayoutPadding = 4;
			outerStack.IsTransparent = false;
			stackWindow.AddChild(outerStack);

			for (int i = 1; i <= 3; i++)
			{
				StackLayout innerStack = new StackLayout();
				innerStack.Size = new Vector2(48, 45);
				innerStack.Orientation = StackOrientation.Vertical;
				innerStack.Spacing = 2;
				innerStack.LayoutPadding = 2;
				innerStack.IsTransparent = false;
				outerStack.AddChild(innerStack);

				for (int j = 1; j <= 2; j++)
				{
					Button innerBtn = new Button();
					innerBtn.Text = $"{i}{(char)('A' + j - 1)}";
					innerBtn.Size = new Vector2(40, 18);
					innerStack.AddChild(innerBtn);
				}
			}

			// === FlowLayout in Window ===
			Window flowWindow = new Window("FlowLayout Demo", new Vector2(200, 340));
			flowWindow.Position = new Vector2(500, 60);
			flowWindow.IsResizable = true;
			flowWindow.ShowCloseButton = false;
			flowWindow.MinSize = new Vector2(150, 200);
			FUI.AddControl(flowWindow);

			// Basic FlowLayout - LeftToRight with wrap
			Label flowLTRLabel = new Label("Left-to-Right:");
			flowLTRLabel.Position = new Vector2(5, 5);
			flowLTRLabel.Alignment = Align.Left;
			flowWindow.AddChild(flowLTRLabel);

			FlowLayout flowLTR = new FlowLayout();
			flowLTR.Position = new Vector2(5, 25);
			flowLTR.Size = new Vector2(175, 80);
			flowLTR.Anchor = FishUIAnchor.Horizontal; // Stretch with window width
			flowLTR.Direction = FlowDirection.LeftToRight;
			flowLTR.Wrap = FlowWrap.Wrap;
			flowLTR.Spacing = 4;
			flowLTR.WrapSpacing = 4;
			flowLTR.LayoutPadding = 6;
			flowLTR.IsTransparent = false;
			flowWindow.AddChild(flowLTR);

			for (int i = 1; i <= 8; i++)
			{
				Button flowBtn = new Button();
				flowBtn.Text = $"B{i}";
				flowBtn.Size = new Vector2(35, 24);
				flowLTR.AddChild(flowBtn);
			}

			// FlowLayout - RightToLeft
			Label flowRTLLabel = new Label("Right-to-Left:");
			flowRTLLabel.Position = new Vector2(5, 110);
			flowRTLLabel.Alignment = Align.Left;
			flowWindow.AddChild(flowRTLLabel);

			FlowLayout flowRTL = new FlowLayout();
			flowRTL.Position = new Vector2(5, 130);
			flowRTL.Size = new Vector2(175, 55);
			flowRTL.Anchor = FishUIAnchor.Horizontal; // Stretch with window width
			flowRTL.Direction = FlowDirection.RightToLeft;
			flowRTL.Wrap = FlowWrap.Wrap;
			flowRTL.Spacing = 4;
			flowRTL.WrapSpacing = 4;
			flowRTL.LayoutPadding = 6;
			flowRTL.IsTransparent = false;
			flowWindow.AddChild(flowRTL);

			for (char c = 'A'; c <= 'F'; c++)
			{
				Button rtlBtn = new Button();
				rtlBtn.Text = c.ToString();
				rtlBtn.Size = new Vector2(28, 22);
				flowRTL.AddChild(rtlBtn);
			}

			// FlowLayout - TopToBottom vertical flow
			Label flowTBLabel = new Label("Vertical Flow:");
			flowTBLabel.Position = new Vector2(5, 190);
			flowTBLabel.Alignment = Align.Left;
			flowWindow.AddChild(flowTBLabel);

			FlowLayout flowTB = new FlowLayout();
			flowTB.Position = new Vector2(5, 210);
			flowTB.Size = new Vector2(175, 70);
			flowTB.Anchor = FishUIAnchor.Horizontal; // Stretch with window width
			flowTB.Direction = FlowDirection.TopToBottom;
			flowTB.Wrap = FlowWrap.Wrap;
			flowTB.Spacing = 3;
			flowTB.WrapSpacing = 6;
			flowTB.LayoutPadding = 4;
			flowTB.IsTransparent = false;
			flowWindow.AddChild(flowTB);

			for (int i = 1; i <= 6; i++)
			{
				Button tbBtn = new Button();
				tbBtn.Text = $"V{i}";
				tbBtn.Size = new Vector2(42, 20);
				flowTB.AddChild(tbBtn);
			}

			// === GridLayout in Window ===
			Window gridWindow = new Window("GridLayout Demo", new Vector2(200, 340));
			gridWindow.Position = new Vector2(720, 60);
			gridWindow.IsResizable = true;
			gridWindow.ShowCloseButton = false;
			gridWindow.MinSize = new Vector2(150, 200);
			FUI.AddControl(gridWindow);

			// Basic GridLayout - 3x3 grid
			Label grid3x3Label = new Label("3x3 Grid:");
			grid3x3Label.Position = new Vector2(5, 5);
			grid3x3Label.Alignment = Align.Left;
			gridWindow.AddChild(grid3x3Label);

			GridLayout grid3x3 = new GridLayout();
			grid3x3.Position = new Vector2(5, 25);
			grid3x3.Size = new Vector2(175, 90);
			grid3x3.Anchor = FishUIAnchor.Horizontal; // Stretch with window width
			grid3x3.Columns = 3;
			grid3x3.HorizontalSpacing = 4;
			grid3x3.VerticalSpacing = 4;
			grid3x3.LayoutPadding = 5;
			grid3x3.StretchCells = true;
			grid3x3.IsTransparent = false;
			gridWindow.AddChild(grid3x3);

			for (int i = 1; i <= 9; i++)
			{
				Button gridBtn = new Button();
				gridBtn.Text = $"{i}";
				grid3x3.AddChild(gridBtn);
			}

			// GridLayout - 2 columns, auto rows
			Label grid2ColLabel = new Label("2 Columns:");
			grid2ColLabel.Position = new Vector2(5, 120);
			grid2ColLabel.Alignment = Align.Left;
			gridWindow.AddChild(grid2ColLabel);

			GridLayout grid2Col = new GridLayout();
			grid2Col.Position = new Vector2(5, 140);
			grid2Col.Size = new Vector2(175, 80);
			grid2Col.Anchor = FishUIAnchor.Horizontal; // Stretch with window width
			grid2Col.Columns = 2;
			grid2Col.HorizontalSpacing = 5;
			grid2Col.VerticalSpacing = 5;
			grid2Col.LayoutPadding = 4;
			grid2Col.StretchCells = true;
			grid2Col.IsTransparent = false;
			gridWindow.AddChild(grid2Col);

			string[] labels = { "OK", "Cancel", "Yes", "No" };
			foreach (var label in labels)
			{
				Button colBtn = new Button();
				colBtn.Text = label;
				grid2Col.AddChild(colBtn);
			}

			// GridLayout - 4 columns icon grid
			Label grid4ColLabel = new Label("4 Columns:");
			grid4ColLabel.Position = new Vector2(5, 225);
			grid4ColLabel.Alignment = Align.Left;
			gridWindow.AddChild(grid4ColLabel);

			GridLayout grid4Col = new GridLayout();
			grid4Col.Position = new Vector2(5, 245);
			grid4Col.Size = new Vector2(175, 55);
			grid4Col.Anchor = FishUIAnchor.Horizontal; // Stretch with window width
			grid4Col.Columns = 4;
			grid4Col.HorizontalSpacing = 3;
			grid4Col.VerticalSpacing = 3;
			grid4Col.LayoutPadding = 4;
			grid4Col.StretchCells = true;
			grid4Col.IsTransparent = false;
			gridWindow.AddChild(grid4Col);

			for (char c = 'A'; c <= 'H'; c++)
			{
				Button iconBtn = new Button();
				iconBtn.Text = c.ToString();
				grid4Col.AddChild(iconBtn);
			}

			// === Auto-Sizing ===
			Label autoSizeLabel = new Label("Auto-Sizing");
			autoSizeLabel.Position = new Vector2(300, 575);
			autoSizeLabel.Alignment = Align.Left;
			FUI.AddControl(autoSizeLabel);

			// Auto-size Buttons
			Button autoBtn1 = new Button();
			autoBtn1.Text = "OK";
			autoBtn1.Position = new Vector2(400, 570);
			autoBtn1.Size = new Vector2(50, 25);
			autoBtn1.AutoSize = AutoSizeMode.Both;
			FUI.AddControl(autoBtn1);

			Button autoBtn2 = new Button();
			autoBtn2.Text = "Cancel";
			autoBtn2.Position = new Vector2(460, 570);
			autoBtn2.Size = new Vector2(50, 25);
			autoBtn2.AutoSize = AutoSizeMode.Both;
			FUI.AddControl(autoBtn2);

			Button autoBtn3 = new Button();
			autoBtn3.Text = "Apply Changes";
			autoBtn3.Position = new Vector2(530, 570);
			autoBtn3.Size = new Vector2(50, 25);
			autoBtn3.AutoSize = AutoSizeMode.Both;
			FUI.AddControl(autoBtn3);

			Button constrainedBtn = new Button();
			constrainedBtn.Text = "Constrained (Min/Max)";
			constrainedBtn.Position = new Vector2(670, 570);
			constrainedBtn.Size = new Vector2(50, 25);
			constrainedBtn.AutoSize = AutoSizeMode.Width;
			constrainedBtn.MinSize = new Vector2(80, 0);
			constrainedBtn.MaxSize = new Vector2(150, 0);
			FUI.AddControl(constrainedBtn);
		}
	}
}

