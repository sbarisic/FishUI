using FishUI;
using FishUI.Controls;
using System;
using System.Numerics;

namespace FishUIDemos
{
	/// <summary>
	/// Sample demonstrating the ParticleEmitter control with various effects.
	/// </summary>
	public class SampleParticles : ISample
	{
		private FishUI.FishUI FUI;
		private ParticleEmitter _fireEmitter;
		private ParticleEmitter _sparkleEmitter;
		private ParticleEmitter _smokeEmitter;
		private Label _particleCountLabel;

		public string Name => "Particle System";

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
			// Title
			var titleLabel = new Label
			{
				Position = new Vector2(20, 20),
				Size = new Vector2(600, 30),
				Text = "Particle System Demo - Click buttons to trigger effects"
			};
			FUI.AddControl(titleLabel);

			// Particle count display
			_particleCountLabel = new Label
			{
				Position = new Vector2(20, 50),
				Size = new Vector2(300, 24),
				Text = "Active particles: 0"
			};
			FUI.AddControl(_particleCountLabel);

			// Fire emitter
			_fireEmitter = new ParticleEmitter
			{
				Position = new Vector2(150, 400),
				Size = new Vector2(100, 20),
				Config = ParticleConfig.Fire,
				Shape = EmitterShape.Rectangle,
				EmissionRate = 30,
				IsEmitting = false,
				ParticleSize = new Vector2(6, 6)
			};
			FUI.AddControl(_fireEmitter);

			var fireLabel = new Label
			{
				Position = new Vector2(130, 430),
				Size = new Vector2(140, 24),
				Text = "Fire Effect"
			};
			FUI.AddControl(fireLabel);

			var fireButton = new Button
			{
				Position = new Vector2(130, 460),
				Size = new Vector2(140, 30),
				Text = "Toggle Fire"
			};
			fireButton.OnButtonPressed += (btn, mbtn, pos) =>
			{
				_fireEmitter.IsEmitting = !_fireEmitter.IsEmitting;
				btn.Text = _fireEmitter.IsEmitting ? "Stop Fire" : "Toggle Fire";
			};
			FUI.AddControl(fireButton);

			// Sparkle/Confetti emitter
			_sparkleEmitter = new ParticleEmitter
			{
				Position = new Vector2(400, 300),
				Size = new Vector2(200, 50),
				Config = ParticleConfig.Sparkle,
				Shape = EmitterShape.Rectangle,
				EmissionRate = 0, // Manual burst only
				IsEmitting = false,
				ParticleSize = new Vector2(4, 4)
			};
			FUI.AddControl(_sparkleEmitter);

			var sparkleLabel = new Label
			{
				Position = new Vector2(430, 430),
				Size = new Vector2(140, 24),
				Text = "Confetti Burst"
			};
			FUI.AddControl(sparkleLabel);

			var sparkleButton = new Button
			{
				Position = new Vector2(430, 460),
				Size = new Vector2(140, 30),
				Text = "Burst!"
			};
			sparkleButton.OnButtonPressed += (btn, mbtn, pos) =>
			{
				_sparkleEmitter.Burst(50);
			};
			FUI.AddControl(sparkleButton);

			// Smoke emitter
			_smokeEmitter = new ParticleEmitter
			{
				Position = new Vector2(700, 380),
				Size = new Vector2(40, 40),
				Config = ParticleConfig.Smoke,
				Shape = EmitterShape.Circle,
				EmissionRate = 5,
				IsEmitting = false,
				ParticleSize = new Vector2(20, 20)
			};
			FUI.AddControl(_smokeEmitter);

			var smokeLabel = new Label
			{
				Position = new Vector2(680, 430),
				Size = new Vector2(140, 24),
				Text = "Smoke Effect"
			};
			FUI.AddControl(smokeLabel);

			var smokeButton = new Button
			{
				Position = new Vector2(680, 460),
				Size = new Vector2(140, 30),
				Text = "Toggle Smoke"
			};
			smokeButton.OnButtonPressed += (btn, mbtn, pos) =>
			{
				_smokeEmitter.IsEmitting = !_smokeEmitter.IsEmitting;
				btn.Text = _smokeEmitter.IsEmitting ? "Stop Smoke" : "Toggle Smoke";
			};
			FUI.AddControl(smokeButton);

			// Explosion button (uses a temporary emitter pattern)
			var explosionButton = new Button
			{
				Position = new Vector2(400, 520),
				Size = new Vector2(200, 40),
				Text = "💥 EXPLOSION!"
			};
			explosionButton.OnButtonPressed += (btn, mbtn, pos) =>
			{
				// Create explosion at center of screen
				var explosion = new ParticleEmitter
				{
					Position = new Vector2(450, 250),
					Size = new Vector2(10, 10),
					Config = ParticleConfig.Explosion,
					Shape = EmitterShape.Point,
					EmissionRate = 0,
					ParticleSize = new Vector2(8, 8)
				};
				FUI.AddControl(explosion);
				explosion.Burst(100);

				// Note: In a real app, you'd want to remove this emitter after particles die
			};
			FUI.AddControl(explosionButton);

			// Custom config section
			var customPanel = new Panel
			{
				Position = new Vector2(850, 100),
				Size = new Vector2(250, 400)
			};
			FUI.AddControl(customPanel);

			var customTitle = new Label
			{
				Position = new Vector2(10, 10),
				Size = new Vector2(230, 24),
				Text = "Custom Emitter"
			};
			customPanel.AddChild(customTitle);

			var customEmitter = new ParticleEmitter
			{
				Position = new Vector2(75, 150),
				Size = new Vector2(100, 100),
				Config = new ParticleConfig
				{
					VelocityMin = new Vector2(-80, -80),
					VelocityMax = new Vector2(80, 80),
					Acceleration = Vector2.Zero,
					LifetimeMin = 1f,
					LifetimeMax = 2f,
					ScaleMin = 0.3f,
					ScaleMax = 0.8f,
					StartColor = new FishColor(100, 150, 255, 255),
					EndColor = new FishColor(255, 100, 200, 0),
					ColorEasing = Easing.EaseInOutQuad
				},
				Shape = EmitterShape.CircleEdge,
				EmissionRate = 15,
				IsEmitting = true,
				ParticleSize = new Vector2(5, 5)
			};
			customPanel.AddChild(customEmitter);

			var rateLabel = new Label
			{
				Position = new Vector2(10, 280),
				Size = new Vector2(100, 24),
				Text = "Rate: 15/s"
			};
			customPanel.AddChild(rateLabel);

			var rateSlider = new Slider
			{
				Position = new Vector2(10, 310),
				Size = new Vector2(230, 24),
				MinValue = 0,
				MaxValue = 100,
				Value = 15
			};
			rateSlider.OnValueChanged += (slider, val) =>
			{
				customEmitter.EmissionRate = val;
				rateLabel.Text = $"Rate: {val:0}/s";
			};
			customPanel.AddChild(rateSlider);

			var shapeLabel = new Label
			{
				Position = new Vector2(10, 340),
				Size = new Vector2(100, 24),
				Text = "Shape:"
			};
			customPanel.AddChild(shapeLabel);

			var shapeDropdown = new DropDown
			{
				Position = new Vector2(70, 340),
				Size = new Vector2(170, 24)
			};
			shapeDropdown.Items.Add(new DropDownItem("Point"));
			shapeDropdown.Items.Add(new DropDownItem("Rectangle"));
			shapeDropdown.Items.Add(new DropDownItem("Circle"));
			shapeDropdown.Items.Add(new DropDownItem("Circle Edge"));
			shapeDropdown.SelectIndex(3);
			shapeDropdown.OnItemSelected += (dropdown, item) =>
			{
				int idx = shapeDropdown.Items.FindIndex(i => i == item);
				customEmitter.Shape = (EmitterShape)idx;
			};
			customPanel.AddChild(shapeDropdown);

			// Screenshot button
			var screenshotBtn = new Button
			{
				Position = new Vector2(20, 520),
				Size = new Vector2(100, 30),
				Text = "Screenshot"
			};
			screenshotBtn.OnButtonPressed += (btn, mbtn, pos) => TakeScreenshot?.Invoke(GetType().Name);
			FUI.AddControl(screenshotBtn);
		}

		public void Update(float Dt)
		{
			// Update particle count label
			int totalParticles = _fireEmitter.ParticleCount + _sparkleEmitter.ParticleCount + _smokeEmitter.ParticleCount;
			_particleCountLabel.Text = $"Active particles: {totalParticles}";
		}
	}
}
