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

			FishUITheme theme = UISettings.LoadTheme(ThemePreferences.LoadThemePath(), applyImmediately: true);

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

			// ============ ROW 1: ImageBox Scaling/Filter Modes ============

			Label imageBoxLabel = new Label("ImageBox Scaling & Filter Modes");
			imageBoxLabel.Position = new Vector2(20, 60);
			imageBoxLabel.Size = new Vector2(250, 20);
			imageBoxLabel.Alignment = Align.Left;
			FUI.AddControl(imageBoxLabel);

			// Load pixel art image for better filter mode demonstration
			ImageRef pixelImage = FUI.Graphics.LoadImage("data/images/win95_pixel.png");

			// None - original size, centered
			Label noneLabel = new Label("None (Original)");
			noneLabel.Position = new Vector2(20, 85);
			noneLabel.Size = new Vector2(100, 16);
			noneLabel.Alignment = Align.Left;
			FUI.AddControl(noneLabel);

			ImageBox imgNone = new ImageBox(pixelImage);
			imgNone.Position = new Vector2(20, 105);
			imgNone.Size = new Vector2(100, 100);
			imgNone.ScaleMode = ImageScaleMode.None;
			imgNone.TooltipText = "ScaleMode.None - Original size, centered";
			FUI.AddControl(imgNone);

			// Stretch - fills bounds
			Label stretchLabel = new Label("Stretch");
			stretchLabel.Position = new Vector2(130, 85);
			stretchLabel.Size = new Vector2(100, 16);
			stretchLabel.Alignment = Align.Left;
			FUI.AddControl(stretchLabel);

			ImageBox imgStretch = new ImageBox(pixelImage);
			imgStretch.Position = new Vector2(130, 105);
			imgStretch.Size = new Vector2(100, 100);
			imgStretch.ScaleMode = ImageScaleMode.Stretch;
			imgStretch.TooltipText = "ScaleMode.Stretch - Fills bounds";
			FUI.AddControl(imgStretch);

			// Smooth filter mode
			Label smoothLabel = new Label("Smooth Filter");
			smoothLabel.Position = new Vector2(240, 85);
			smoothLabel.Size = new Vector2(100, 16);
			smoothLabel.Alignment = Align.Left;
			FUI.AddControl(smoothLabel);

			ImageBox imgSmooth = new ImageBox(pixelImage);
			imgSmooth.Position = new Vector2(240, 105);
			imgSmooth.Size = new Vector2(100, 100);
			imgSmooth.ScaleMode = ImageScaleMode.Stretch;
			imgSmooth.FilterMode = ImageFilterMode.Smooth;
			imgSmooth.TooltipText = "FilterMode.Smooth - Bilinear filtering";
			FUI.AddControl(imgSmooth);

			// Pixelated filter mode (best for pixel art)
			Label pixelLabel = new Label("Pixelated Filter");
			pixelLabel.Position = new Vector2(350, 85);
			pixelLabel.Size = new Vector2(100, 16);
			pixelLabel.Alignment = Align.Left;
			FUI.AddControl(pixelLabel);

			ImageBox imgPixelated = new ImageBox(pixelImage);
			imgPixelated.Position = new Vector2(350, 105);
			imgPixelated.Size = new Vector2(100, 100);
			imgPixelated.ScaleMode = ImageScaleMode.Stretch;
			imgPixelated.FilterMode = ImageFilterMode.Pixelated;
			imgPixelated.TooltipText = "FilterMode.Pixelated - Nearest-neighbor (pixel art)";
			FUI.AddControl(imgPixelated);

			// Clickable ImageBox
			Label clickLabel = new Label("Clickable");
			clickLabel.Position = new Vector2(460, 85);
			clickLabel.Size = new Vector2(100, 16);
			clickLabel.Alignment = Align.Left;
			FUI.AddControl(clickLabel);

			Label clickCountLabel = new Label("Clicks: 0");
			clickCountLabel.Position = new Vector2(570, 130);
			clickCountLabel.Size = new Vector2(80, 20);
			clickCountLabel.Alignment = Align.Left;
			FUI.AddControl(clickCountLabel);

			int clickCount = 0;
			ImageBox imgClickable = new ImageBox(pixelImage);
			imgClickable.Position = new Vector2(460, 105);
			imgClickable.Size = new Vector2(100, 100);
			imgClickable.ScaleMode = ImageScaleMode.Stretch;
			imgClickable.FilterMode = ImageFilterMode.Pixelated;
			imgClickable.TooltipText = "Click me!";
			imgClickable.OnClick += (sender, btn, pos) =>
			{
				clickCount++;
				clickCountLabel.Text = $"Clicks: {clickCount}";
			};
			FUI.AddControl(imgClickable);

			// ============ ROW 2: AnimatedImageBox ============

			Label animLabel = new Label("AnimatedImageBox");
			animLabel.Position = new Vector2(20, 220);
			animLabel.Size = new Vector2(150, 20);
			animLabel.Alignment = Align.Left;
			FUI.AddControl(animLabel);

			// Load stargate animation frames
			AnimatedImageBox stargateAnim = new AnimatedImageBox();
			stargateAnim.Position = new Vector2(20, 245);
			stargateAnim.Size = new Vector2(120, 120);
			stargateAnim.ScaleMode = ImageScaleMode.Fit;
			stargateAnim.FrameRate = 10f;
			stargateAnim.Loop = true;
			stargateAnim.TooltipText = "Stargate animation (10 FPS, looping)";

			for (int i = 0; i <= 27; i++)
			{
				string framePath = $"data/anim_images/stargate/frame_{i:D2}_delay-0.1s.png";
				ImageRef frame = FUI.Graphics.LoadImage(framePath);
				if (frame != null)
					stargateAnim.AddFrame(frame);
			}
			FUI.AddControl(stargateAnim);

			// Animation controls
			Button animPlayPause = new Button();
			animPlayPause.Text = "Pause";
			animPlayPause.Position = new Vector2(150, 245);
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

			Button animStop = new Button();
			animStop.Text = "Stop";
			animStop.Position = new Vector2(150, 275);
			animStop.Size = new Vector2(70, 25);
			animStop.OnButtonPressed += (btn, mbtn, pos) =>
			{
				stargateAnim.Stop();
				animPlayPause.Text = "Play";
			};
			FUI.AddControl(animStop);

			Label fpsLabel = new Label("FPS:");
			fpsLabel.Position = new Vector2(150, 310);
			fpsLabel.Size = new Vector2(30, 16);
			fpsLabel.Alignment = Align.Left;
			FUI.AddControl(fpsLabel);

			Slider fpsSlider = new Slider();
			fpsSlider.Position = new Vector2(180, 310);
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

			CheckBox pingPongCheck = new CheckBox("Ping-Pong");
			pingPongCheck.Position = new Vector2(150, 340);
			pingPongCheck.Size = new Vector2(15, 15);
			pingPongCheck.IsChecked = false;
			FUI.AddControl(pingPongCheck);

			Button pingPongBtn = new Button();
			pingPongBtn.Text = "Toggle";
			pingPongBtn.Position = new Vector2(240, 338);
			pingPongBtn.Size = new Vector2(50, 20);
			pingPongBtn.OnButtonPressed += (btn, mbtn, pos) =>
			{
				stargateAnim.PingPong = !stargateAnim.PingPong;
				pingPongCheck.IsChecked = stargateAnim.PingPong;
			};
			FUI.AddControl(pingPongBtn);

			// ============ Animation Viewer Window ============

			Window videoWindow = new Window();
			videoWindow.Title = "Animation Viewer";
			videoWindow.Position = new Vector2(350, 220);
			videoWindow.Size = new Vector2(280, 180);
			videoWindow.ShowCloseButton = true;
			videoWindow.OnClosed += (wnd) => { wnd.Visible = false; };
			FUI.AddControl(videoWindow);

			AnimatedImageBox videoAnim = new AnimatedImageBox();
			videoAnim.Position = new Vector2(5, 5);
			videoAnim.Size = new Vector2(270, 145);
			videoAnim.Anchor = FishUIAnchor.All;
			videoAnim.ScaleMode = ImageScaleMode.Fit;
			videoAnim.FrameRate = 10f;
			videoAnim.Loop = true;

			for (int j = 0; j <= 27; j++)
			{
				string vframePath = $"data/anim_images/stargate/frame_{j:D2}_delay-0.1s.png";
				ImageRef vframe = FUI.Graphics.LoadImage(vframePath);
				if (vframe != null)
					videoAnim.AddFrame(vframe);
			}
			videoWindow.AddChild(videoAnim);

			Button showVideoBtn = new Button();
			showVideoBtn.Text = "Show Viewer";
			showVideoBtn.Position = new Vector2(20, 375);
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

