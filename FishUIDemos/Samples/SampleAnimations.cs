using FishUI;
using FishUI.Controls;
using System;
using System.Numerics;

namespace FishUIDemos
{
	/// <summary>
	/// Demonstrates the animation system with various transition effects.
	/// </summary>
	public class SampleAnimations : ISample
	{
		FishUI.FishUI FUI;

		public string Name => "Animations";

		public TakeScreenshotFunc TakeScreenshot { get; set; }

		// Controls that will be animated
		Button fadeButton;
		Button slideButton;
		Button scaleButton;
		Panel animatedPanel;
		ProgressBar animatedProgress;

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
			Label titleLabel = new Label("Animation System Demo");
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

			// === Fade Animations ===
			Label fadeLabel = new Label("Fade Animations");
			fadeLabel.Position = new Vector2(20, 60);
			fadeLabel.Alignment = Align.Left;
			FUI.AddControl(fadeLabel);

			fadeButton = new Button();
			fadeButton.Text = "Fade Me!";
			fadeButton.Position = new Vector2(20, 85);
			fadeButton.Size = new Vector2(100, 30);
			FUI.AddControl(fadeButton);

			Button fadeInBtn = new Button();
			fadeInBtn.Text = "Fade In";
			fadeInBtn.Position = new Vector2(130, 85);
			fadeInBtn.Size = new Vector2(70, 30);
			fadeInBtn.OnButtonPressed += (btn, mbtn, pos) =>
			{
				fadeButton.FadeIn(0.5f, Easing.EaseOutQuad);
			};
			FUI.AddControl(fadeInBtn);

			Button fadeOutBtn = new Button();
			fadeOutBtn.Text = "Fade Out";
			fadeOutBtn.Position = new Vector2(205, 85);
			fadeOutBtn.Size = new Vector2(70, 30);
			fadeOutBtn.OnButtonPressed += (btn, mbtn, pos) =>
			{
				fadeButton.FadeOut(0.5f, Easing.EaseInQuad);
			};
			FUI.AddControl(fadeOutBtn);

			// === Slide Animations ===
			Label slideLabel = new Label("Slide Animations");
			slideLabel.Position = new Vector2(20, 130);
			slideLabel.Alignment = Align.Left;
			FUI.AddControl(slideLabel);

			slideButton = new Button();
			slideButton.Text = "Slide Me!";
			slideButton.Position = new Vector2(20, 155);
			slideButton.Size = new Vector2(100, 30);
			FUI.AddControl(slideButton);

			Button slideLeftBtn = new Button();
			slideLeftBtn.Text = "? Left";
			slideLeftBtn.Position = new Vector2(130, 155);
			slideLeftBtn.Size = new Vector2(60, 30);
			slideLeftBtn.OnButtonPressed += (btn, mbtn, pos) =>
			{
				slideButton.SlideIn(new Vector2(-100, 0), 0.4f, Easing.EaseOutBack);
			};
			FUI.AddControl(slideLeftBtn);

			Button slideRightBtn = new Button();
			slideRightBtn.Text = "Right ?";
			slideRightBtn.Position = new Vector2(195, 155);
			slideRightBtn.Size = new Vector2(60, 30);
			slideRightBtn.OnButtonPressed += (btn, mbtn, pos) =>
			{
				slideButton.SlideIn(new Vector2(100, 0), 0.4f, Easing.EaseOutBack);
			};
			FUI.AddControl(slideRightBtn);

			Button slideUpBtn = new Button();
			slideUpBtn.Text = "? Up";
			slideUpBtn.Position = new Vector2(260, 155);
			slideUpBtn.Size = new Vector2(50, 30);
			slideUpBtn.OnButtonPressed += (btn, mbtn, pos) =>
			{
				slideButton.SlideIn(new Vector2(0, -50), 0.4f, Easing.EaseOutBounce);
			};
			FUI.AddControl(slideUpBtn);

			// === Scale/Bounce Animations ===
			Label scaleLabel = new Label("Scale Animations");
			scaleLabel.Position = new Vector2(20, 200);
			scaleLabel.Alignment = Align.Left;
			FUI.AddControl(scaleLabel);

			scaleButton = new Button();
			scaleButton.Text = "Bounce Me!";
			scaleButton.Position = new Vector2(20, 225);
			scaleButton.Size = new Vector2(100, 30);
			FUI.AddControl(scaleButton);

			Button bounceBtn = new Button();
			bounceBtn.Text = "Bounce";
			bounceBtn.Position = new Vector2(130, 225);
			bounceBtn.Size = new Vector2(70, 30);
			bounceBtn.OnButtonPressed += (btn, mbtn, pos) =>
			{
				scaleButton.ScaleBounce(1.2f, 0.3f);
			};
			FUI.AddControl(bounceBtn);

			Button growBtn = new Button();
			growBtn.Text = "Grow";
			growBtn.Position = new Vector2(205, 225);
			growBtn.Size = new Vector2(60, 30);
			growBtn.OnButtonPressed += (btn, mbtn, pos) =>
			{
				scaleButton.AnimateSize(new Vector2(150, 40), 0.3f, Easing.EaseOutElastic);
			};
			FUI.AddControl(growBtn);

			Button shrinkBtn = new Button();
			shrinkBtn.Text = "Shrink";
			shrinkBtn.Position = new Vector2(270, 225);
			shrinkBtn.Size = new Vector2(60, 30);
			shrinkBtn.OnButtonPressed += (btn, mbtn, pos) =>
			{
				scaleButton.AnimateSize(new Vector2(100, 30), 0.3f, Easing.EaseOutQuad);
			};
			FUI.AddControl(shrinkBtn);

			// === Easing Comparison ===
			Label easingLabel = new Label("Easing Functions");
			easingLabel.Position = new Vector2(20, 280);
			easingLabel.Alignment = Align.Left;
			FUI.AddControl(easingLabel);

			// Create panels to demonstrate different easing functions
			string[] easingNames = { "Linear", "EaseOut", "EaseOutBack", "EaseOutElastic", "EaseOutBounce" };
			Easing[] easings = { Easing.Linear, Easing.EaseOut, Easing.EaseOutBack, Easing.EaseOutElastic, Easing.EaseOutBounce };

			for (int i = 0; i < easingNames.Length; i++)
			{
				Label easingNameLabel = new Label(easingNames[i]);
				easingNameLabel.Position = new Vector2(20, 305 + i * 35);
				easingNameLabel.Size = new Vector2(100, 20);
				easingNameLabel.Alignment = Align.Left;
				FUI.AddControl(easingNameLabel);

				Panel easingPanel = new Panel();
				easingPanel.Position = new Vector2(130, 305 + i * 35);
				easingPanel.Size = new Vector2(30, 25);
				easingPanel.Variant = PanelVariant.Highlight;
				FUI.AddControl(easingPanel);

				// Store for animation
				int index = i;
				Easing easing = easings[i];
			}

			Button animateEasingsBtn = new Button();
			animateEasingsBtn.Text = "Animate All Easings";
			animateEasingsBtn.Position = new Vector2(20, 485);
			animateEasingsBtn.Size = new Vector2(150, 30);
			animateEasingsBtn.OnButtonPressed += (btn, mbtn, pos) =>
			{
				AnimateEasingPanels();
			};
			FUI.AddControl(animateEasingsBtn);

			// === Panel Animation ===
			Label panelLabel = new Label("Panel Animation");
			panelLabel.Position = new Vector2(350, 60);
			panelLabel.Alignment = Align.Left;
			FUI.AddControl(panelLabel);

			animatedPanel = new Panel();
			animatedPanel.Position = new Vector2(350, 85);
			animatedPanel.Size = new Vector2(150, 100);
			animatedPanel.Variant = PanelVariant.Bright;
			FUI.AddControl(animatedPanel);

			Label panelContent = new Label("Animated Panel");
			panelContent.Position = new Vector2(10, 40);
			panelContent.Alignment = Align.Left;
			animatedPanel.AddChild(panelContent);

			Button movePanelBtn = new Button();
			movePanelBtn.Text = "Move Panel";
			movePanelBtn.Position = new Vector2(350, 195);
			movePanelBtn.Size = new Vector2(100, 30);
			movePanelBtn.OnButtonPressed += (btn, mbtn, pos) =>
			{
				var currentPos = new Vector2(animatedPanel.Position.X, animatedPanel.Position.Y);
				var targetPos = currentPos.X > 400 ? new Vector2(350, 85) : new Vector2(450, 120);
				animatedPanel.AnimatePosition(targetPos, 0.5f, Easing.EaseOutBack);
			};
			FUI.AddControl(movePanelBtn);

			// === Progress Bar Animation ===
			Label progressLabel = new Label("Progress Animation");
			progressLabel.Position = new Vector2(350, 240);
			progressLabel.Alignment = Align.Left;
			FUI.AddControl(progressLabel);

			animatedProgress = new ProgressBar();
			animatedProgress.Position = new Vector2(350, 265);
			animatedProgress.Size = new Vector2(200, 25);
			animatedProgress.Value = 0.3f;
			FUI.AddControl(animatedProgress);

			Button animateProgressBtn = new Button();
			animateProgressBtn.Text = "Animate Progress";
			animateProgressBtn.Position = new Vector2(350, 300);
			animateProgressBtn.Size = new Vector2(120, 30);
			animateProgressBtn.OnButtonPressed += (btn, mbtn, pos) =>
			{
				float targetProgress = animatedProgress.Value < 0.5f ? 1f : 0f;
				FishUITween.Float(FUI.Animations, animatedProgress, "Progress",
					animatedProgress.Value, targetProgress, 1f, Easing.EaseInOutQuad,
					v => animatedProgress.Value = v);
			};
			FUI.AddControl(animateProgressBtn);

			// === Animation Info ===
			Label infoLabel = new Label("Active Animations: 0");
			infoLabel.Position = new Vector2(350, 350);
			infoLabel.Size = new Vector2(200, 20);
			infoLabel.Alignment = Align.Left;
			infoLabel.ID = "animationCountLabel";
			FUI.AddControl(infoLabel);
		}

		private void AnimateEasingPanels()
		{
			// Find all highlight panels and animate them
			var controls = FUI.GetAllControls();
			int panelIndex = 0;
			Easing[] easings = { Easing.Linear, Easing.EaseOut, Easing.EaseOutBack, Easing.EaseOutElastic, Easing.EaseOutBounce };

			foreach (var control in controls)
			{
				if (control is Panel panel && panel.Variant == PanelVariant.Highlight)
				{
					if (panelIndex < easings.Length)
					{
						var startX = 130f;
						var endX = 280f;
						var currentX = panel.Position.X;
						var targetX = currentX < 200 ? endX : startX;

						panel.AnimatePosition(new Vector2(targetX, panel.Position.Y), 1f, easings[panelIndex]);
						panelIndex++;
					}
				}
			}
		}
	}
}

