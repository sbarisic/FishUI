using FishUI;
using FishUI.Controls;
using System;
using System.Numerics;

namespace FishUISample.Samples
{
	/// <summary>
	/// Demonstrates layout system features: Margin, Padding, Anchors, StackLayout, Panel variants.
	/// </summary>
	internal class SampleLayoutSystem : ISample
	{
		FishUI.FishUI FUI;

		public string Name => "Layout System";

		public Action TakeScreenshot { get; set; }

		public FishUI.FishUI CreateUI(FishUISettings UISettings, IFishUIGfx Gfx, IFishUIInput Input, IFishUIEvents Events)
		{
			FUI = new FishUI.FishUI(UISettings, Gfx, Input, Events);
			FUI.Init();

			FishUITheme theme = UISettings.LoadTheme("data/themes/gwen.yaml", applyImmediately: true);

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
			screenshotBtn.OnButtonPressed += (btn, mbtn, pos) => TakeScreenshot?.Invoke();
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
			Label anchorLabel = new Label("Anchor System (resize container to test)");
			anchorLabel.Position = new Vector2(20, 365);
			anchorLabel.Alignment = Align.Left;
			FUI.AddControl(anchorLabel);

			Panel anchorContainer = new Panel();
			anchorContainer.Position = new Vector2(20, 390);
			anchorContainer.Size = new Vector2(240, 120);
			anchorContainer.Variant = PanelVariant.Bright;
			anchorContainer.BorderStyle = BorderStyle.Solid;
			anchorContainer.BorderColor = new FishColor(100, 100, 150, 255);
			anchorContainer.BorderThickness = 1f;
			FUI.AddControl(anchorContainer);

			Button anchorTL = new Button();
			anchorTL.Text = "TL";
			anchorTL.Position = new Vector2(5, 5);
			anchorTL.Size = new Vector2(45, 25);
			anchorTL.Anchor = FishUIAnchor.TopLeft;
			anchorTL.TooltipText = "TopLeft anchor";
			anchorContainer.AddChild(anchorTL);

			Button anchorTR = new Button();
			anchorTR.Text = "TR";
			anchorTR.Position = new Vector2(190, 5);
			anchorTR.Size = new Vector2(45, 25);
			anchorTR.Anchor = FishUIAnchor.TopRight;
			anchorTR.TooltipText = "TopRight anchor";
			anchorContainer.AddChild(anchorTR);

			Button anchorBL = new Button();
			anchorBL.Text = "BL";
			anchorBL.Position = new Vector2(5, 90);
			anchorBL.Size = new Vector2(45, 25);
			anchorBL.Anchor = FishUIAnchor.BottomLeft;
			anchorBL.TooltipText = "BottomLeft anchor";
			anchorContainer.AddChild(anchorBL);

			Button anchorBR = new Button();
			anchorBR.Text = "BR";
			anchorBR.Position = new Vector2(190, 90);
			anchorBR.Size = new Vector2(45, 25);
			anchorBR.Anchor = FishUIAnchor.BottomRight;
			anchorBR.TooltipText = "BottomRight anchor";
			anchorContainer.AddChild(anchorBR);

			Button anchorH = new Button();
			anchorH.Text = "Horizontal";
			anchorH.Position = new Vector2(55, 45);
			anchorH.Size = new Vector2(130, 25);
			anchorH.Anchor = FishUIAnchor.Horizontal;
			anchorH.TooltipText = "Horizontal anchor (stretches)";
			anchorContainer.AddChild(anchorH);

			// === StackLayout ===
			Label stackLabel = new Label("StackLayout");
			stackLabel.Position = new Vector2(300, 235);
			stackLabel.Alignment = Align.Left;
			FUI.AddControl(stackLabel);

			// Vertical Stack
			StackLayout vStack = new StackLayout();
			vStack.Position = new Vector2(300, 260);
			vStack.Size = new Vector2(140, 150);
			vStack.Orientation = StackOrientation.Vertical;
			vStack.Spacing = 8;
			vStack.Padding = 10;
			vStack.IsTransparent = false;
			FUI.AddControl(vStack);

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

			// Horizontal Stack
			StackLayout hStack = new StackLayout();
			hStack.Position = new Vector2(300, 420);
			hStack.Size = new Vector2(220, 50);
			hStack.Orientation = StackOrientation.Horizontal;
			hStack.Spacing = 5;
			hStack.Padding = 5;
			hStack.IsTransparent = false;
			FUI.AddControl(hStack);

			for (char c = 'A'; c <= 'E'; c++)
			{
				Button hBtn = new Button();
				hBtn.Text = c.ToString();
				hBtn.Size = new Vector2(35, 35);
				hStack.AddChild(hBtn);
			}

			// Nested Stacks
			Label nestedLabel = new Label("Nested Stacks");
			nestedLabel.Position = new Vector2(300, 480);
			nestedLabel.Alignment = Align.Left;
			FUI.AddControl(nestedLabel);

			StackLayout outerStack = new StackLayout();
			outerStack.Position = new Vector2(300, 505);
			outerStack.Size = new Vector2(250, 100);
			outerStack.Orientation = StackOrientation.Horizontal;
			outerStack.Spacing = 10;
			outerStack.Padding = 8;
			outerStack.IsTransparent = false;
			FUI.AddControl(outerStack);

			for (int i = 1; i <= 3; i++)
			{
				StackLayout innerStack = new StackLayout();
				innerStack.Size = new Vector2(70, 80);
				innerStack.Orientation = StackOrientation.Vertical;
				innerStack.Spacing = 4;
				innerStack.Padding = 4;
				innerStack.IsTransparent = false;
				outerStack.AddChild(innerStack);

				for (int j = 1; j <= 3; j++)
				{
					Button innerBtn = new Button();
					innerBtn.Text = $"{i}{(char)('A' + j - 1)}";
					innerBtn.Size = new Vector2(55, 20);
					innerStack.AddChild(innerBtn);
				}
			}
		}
	}
}
