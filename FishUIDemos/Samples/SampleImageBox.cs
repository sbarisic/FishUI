using FishUI;
using FishUI.Controls;
using System;
using System.Numerics;

namespace FishUIDemos
{
	/// <summary>
	/// Demonstrates ImageBox and AnimatedImageBox controls with various 
	/// scaling modes, filter modes, and animation features.
	/// </summary>
	public class SampleImageBox : ISample
	{
		FishUI.FishUI FUI;

		public string Name => "ImageBox";

		public TakeScreenshotFunc TakeScreenshot { get; set; }

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
			Label titleLabel = new Label("ImageBox Demo");
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

			// === ImageBox Scaling Modes ===
			Label imageBoxLabel = new Label("ImageBox Scaling Modes");
			imageBoxLabel.Position = new Vector2(20, 60);
			imageBoxLabel.Size = new Vector2(200, 20);
			imageBoxLabel.Alignment = Align.Left;
			FUI.AddControl(imageBoxLabel);

			// Load a sample image for demonstration
			ImageRef sampleImage = FUI.Graphics.LoadImage("data/images/win95_small.png");

			// None - original size, centered
			Label noneLabel = new Label("None");
			noneLabel.Position = new Vector2(20, 85);
			noneLabel.Size = new Vector2(80, 16);
			noneLabel.Alignment = Align.Left;
			FUI.AddControl(noneLabel);

			ImageBox imgNone = new ImageBox(sampleImage);
			imgNone.Position = new Vector2(20, 105);
			imgNone.Size = new Vector2(80, 80);
			imgNone.ScaleMode = ImageScaleMode.None;
			imgNone.TooltipText = "ScaleMode.None - Original size, centered";
			FUI.AddControl(imgNone);

			// Stretch - fills bounds, may distort
			Label stretchLabel = new Label("Stretch");
			stretchLabel.Position = new Vector2(110, 85);
			stretchLabel.Size = new Vector2(80, 16);
			stretchLabel.Alignment = Align.Left;
			FUI.AddControl(stretchLabel);

			ImageBox imgStretch = new ImageBox(sampleImage);
			imgStretch.Position = new Vector2(110, 105);
			imgStretch.Size = new Vector2(80, 80);
			imgStretch.ScaleMode = ImageScaleMode.Stretch;
			imgStretch.TooltipText = "ScaleMode.Stretch - Fills bounds (may distort)";
			FUI.AddControl(imgStretch);

			// Fit - maintains aspect ratio, fits within bounds (smooth)
			Label fitLabel = new Label("Smooth");
			fitLabel.Position = new Vector2(200, 85);
			fitLabel.Size = new Vector2(80, 16);
			fitLabel.Alignment = Align.Left;
			FUI.AddControl(fitLabel);

			ImageBox imgFit = new ImageBox(sampleImage);
			imgFit.Position = new Vector2(200, 105);
			imgFit.Size = new Vector2(80, 80);
			imgFit.ScaleMode = ImageScaleMode.Stretch;
			imgFit.FilterMode = ImageFilterMode.Smooth;
			imgFit.TooltipText = "FilterMode.Smooth - Bilinear filtering";
			FUI.AddControl(imgFit);

			// Fill - pixelated filter mode for pixel art
			Label fillLabel = new Label("Pixelated");
			fillLabel.Position = new Vector2(290, 85);
			fillLabel.Size = new Vector2(80, 16);
			fillLabel.Alignment = Align.Left;
			FUI.AddControl(fillLabel);

			ImageBox imgFill = new ImageBox(sampleImage);
			imgFill.Position = new Vector2(290, 105);
			imgFill.Size = new Vector2(80, 80);
			imgFill.ScaleMode = ImageScaleMode.Stretch;
			imgFill.FilterMode = ImageFilterMode.Pixelated;
			imgFill.TooltipText = "FilterMode.Pixelated - Nearest-neighbor (pixel art)";
			FUI.AddControl(imgFill);

			// Clickable ImageBox demo
			Label clickLabel = new Label("Clickable:");
			clickLabel.Position = new Vector2(380, 85);
			clickLabel.Size = new Vector2(80, 16);
			clickLabel.Alignment = Align.Left;
			FUI.AddControl(clickLabel);

			Label clickCountLabel = new Label("Clicks: 0");
			clickCountLabel.Position = new Vector2(470, 105);
			clickCountLabel.Size = new Vector2(80, 20);
			clickCountLabel.Alignment = Align.Left;
			FUI.AddControl(clickCountLabel);

			int clickCount = 0;
			ImageBox imgClickable = new ImageBox(sampleImage);
			imgClickable.Position = new Vector2(380, 105);
			imgClickable.Size = new Vector2(80, 80);
			imgClickable.ScaleMode = ImageScaleMode.Stretch;
			imgClickable.TooltipText = "Click me!";
			imgClickable.OnClick += (sender, btn, pos) =>
			{
				clickCount++;
				clickCountLabel.Text = $"Clicks: {clickCount}";
			};
			FUI.AddControl(imgClickable);

			// === AnimatedImageBox Section ===
			Label animLabel = new Label("AnimatedImageBox");
			animLabel.Position = new Vector2(20, 200);
			animLabel.Size = new Vector2(150, 20);
			animLabel.Alignment = Align.Left;
			FUI.AddControl(animLabel);

			// Load stargate animation frames
			AnimatedImageBox stargateAnim = new AnimatedImageBox();
			stargateAnim.Position = new Vector2(20, 225);
			stargateAnim.Size = new Vector2(100, 100);
			stargateAnim.ScaleMode = ImageScaleMode.Fit;
			stargateAnim.FrameRate = 10f; // 0.1s delay = 10 FPS
			stargateAnim.Loop = true;
			stargateAnim.TooltipText = "Stargate animation (10 FPS, looping)";

			// Load all 28 frames
			for (int i = 0; i <= 27; i++)
			{
				string framePath = $"data/anim_images/stargate/frame_{i:D2}_delay-0.1s.png";
				ImageRef frame = FUI.Graphics.LoadImage(framePath);
				if (frame != null)
					stargateAnim.AddFrame(frame);
			}
			FUI.AddControl(stargateAnim);

			// Play/Pause button
			Button animPlayPause = new Button();
			animPlayPause.Text = "Pause";
			animPlayPause.Position = new Vector2(130, 225);
			animPlayPause.Size = new Vector2(70, 25);
			animPlayPause.OnButtonPressed += (btn, mbtn, pos) =>
			{
				if (stargateAnim.IsPlaying)
				{
					stargateAnim.Pause();
					animPlayPause.Text = "Play";
				}
				else
				{
					stargateAnim.Play();
					animPlayPause.Text = "Pause";
				}
			};
			FUI.AddControl(animPlayPause);

			// Stop button
			Button animStop = new Button();
			animStop.Text = "Stop";
			animStop.Position = new Vector2(130, 255);
			animStop.Size = new Vector2(70, 25);
			animStop.OnButtonPressed += (btn, mbtn, pos) =>
			{
				stargateAnim.Stop();
				animPlayPause.Text = "Play";
			};
			FUI.AddControl(animStop);

			// Frame rate slider
			Label fpsLabel = new Label("FPS:");
			fpsLabel.Position = new Vector2(130, 285);
			fpsLabel.Size = new Vector2(30, 16);
			fpsLabel.Alignment = Align.Left;
			FUI.AddControl(fpsLabel);

			Slider fpsSlider = new Slider();
			fpsSlider.Position = new Vector2(160, 285);
			fpsSlider.Size = new Vector2(80, 20);
			fpsSlider.MinValue = 1;
			fpsSlider.MaxValue = 30;
			fpsSlider.Value = 10;
			fpsSlider.Step = 1;
			fpsSlider.ShowValueLabel = true;
			fpsSlider.TooltipText = "Adjust animation frame rate";
			fpsSlider.OnValueChanged += (slider, val) =>
			{
				stargateAnim.FrameRate = val;
			};
			FUI.AddControl(fpsSlider);

			// Ping-pong toggle
			CheckBox pingPongCheck = new CheckBox("Ping-Pong");
			pingPongCheck.Position = new Vector2(130, 315);
			pingPongCheck.Size = new Vector2(15, 15);
			pingPongCheck.IsChecked = false;
			FUI.AddControl(pingPongCheck);

			// Use a button to toggle ping-pong since CheckBox doesn't have event
			Button pingPongBtn = new Button();
			pingPongBtn.Text = "Toggle";
			pingPongBtn.Position = new Vector2(220, 310);
			pingPongBtn.Size = new Vector2(50, 20);
			pingPongBtn.OnButtonPressed += (btn, mbtn, pos) =>
			{
				stargateAnim.PingPong = !stargateAnim.PingPong;
				pingPongCheck.IsChecked = stargateAnim.PingPong;
			};
			FUI.AddControl(pingPongBtn);

			// === AnimatedImageBox in Resizable Window (Video Player Style) ===
			Window videoWindow = new Window();
			videoWindow.Title = "Animation Viewer";
			videoWindow.Position = new Vector2(300, 200);
			videoWindow.Size = new Vector2(300, 180);
			videoWindow.ShowCloseButton = true;
			videoWindow.OnClosed += (wnd) => { wnd.Visible = false; };
			FUI.AddControl(videoWindow);

			// Anchored AnimatedImageBox that resizes with window
			AnimatedImageBox videoAnim = new AnimatedImageBox();
			videoAnim.Position = new Vector2(5, 5);
			videoAnim.Size = new Vector2(290, 145);
			videoAnim.Anchor = FishUIAnchor.All; // Resize with window
			videoAnim.ScaleMode = ImageScaleMode.Fit;
			videoAnim.FrameRate = 10f;
			videoAnim.Loop = true;

			// Load stargate frames for video player demo
			for (int j = 0; j <= 27; j++)
			{
				string vframePath = $"data/anim_images/stargate/frame_{j:D2}_delay-0.1s.png";
				ImageRef vframe = FUI.Graphics.LoadImage(vframePath);
				if (vframe != null)
					videoAnim.AddFrame(vframe);
			}
			videoWindow.AddChild(videoAnim);

			// Button to show/hide video window
			Button showVideoBtn = new Button();
			showVideoBtn.Text = "Video Player";
			showVideoBtn.Position = new Vector2(20, 345);
			showVideoBtn.Size = new Vector2(100, 25);
			showVideoBtn.TooltipText = "Show resizable animation viewer window";
			showVideoBtn.OnButtonPressed += (btn, mbtn, pos) =>
			{
				videoWindow.Visible = true;
				videoWindow.BringToFront();
			};
			FUI.AddControl(showVideoBtn);
		}
	}
}
