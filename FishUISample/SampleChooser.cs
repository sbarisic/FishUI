using FishUI;
using FishUI.Controls;
using Raylib_cs;
using System;
using System.Diagnostics;
using System.Numerics;
using FishUIDemos;

namespace FishUISample.Samples
{
	/// <summary>
	/// GUI-based sample chooser that displays available samples as clickable buttons.
	/// Self-dogfooding: uses FishUI to demonstrate FishUI.
	/// </summary>
	internal class SampleChooser
	{
		private ISample[] _samples;
		private ISample _selectedSample;
		private bool _selectionMade;
		private FishUI.FishUI _fui;

		public SampleChooser(ISample[] samples)
		{
			_samples = samples;
		}

		/// <summary>
		/// Shows the GUI chooser and returns the selected sample.
		/// Returns null if the window was closed without selection.
		/// </summary>
		public ISample ShowAndChoose(string[] args)
		{
			// Check for command-line argument: --sample N
			for (int i = 0; i < args.Length - 1; i++)
			{
				if (args[i].Equals("--sample", StringComparison.OrdinalIgnoreCase) ||
					args[i].Equals("-s", StringComparison.OrdinalIgnoreCase))
				{
					if (int.TryParse(args[i + 1], out int sampleIndex) &&
						sampleIndex >= 0 && sampleIndex < _samples.Length)
					{
						Console.WriteLine($"Starting sample {sampleIndex}: {_samples[sampleIndex].Name}");
						return _samples[sampleIndex];
					}
				}
			}

			// Create GUI chooser window
			FishUISettings uiSettings = new FishUISettings();
			RaylibGfx gfx = new RaylibGfx(800, 600, "FishUI - Sample Chooser");
			IFishUIInput input = new RaylibInput();
			IFishUIEvents events = new EvtHandler();

			_fui = new FishUI.FishUI(uiSettings, gfx, input, events);
			_fui.Init();

			// Load theme
			uiSettings.LoadTheme("data/themes/gwen.yaml", applyImmediately: true);

			// Create UI
			CreateChooserUI();

			// Run chooser loop
			_selectionMade = false;
			_selectedSample = null;

			Stopwatch swatch = Stopwatch.StartNew();
			Stopwatch runtimeWatch = Stopwatch.StartNew();

			while (!Raylib.WindowShouldClose() && !_selectionMade)
			{
				float dt = swatch.ElapsedMilliseconds / 1000.0f;
				swatch.Restart();

				if (Raylib.IsWindowResized())
				{
					_fui.Resized(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
				}

				_fui.Tick(dt, (float)runtimeWatch.Elapsed.TotalSeconds);
			}

			Raylib.CloseWindow();

			if (_selectionMade && _selectedSample != null)
			{
				Console.WriteLine($"Selected sample: {_selectedSample.Name}");
				return _selectedSample;
			}

			// Window was closed without selection
			return null;
		}

		private void CreateChooserUI()
		{
			// Title
			Label titleLabel = new Label("FishUI Sample Chooser");
			titleLabel.Position = new Vector2(20, 20);
			titleLabel.Size = new Vector2(760, 40);
			titleLabel.Alignment = Align.Center;
			_fui.AddControl(titleLabel);

			// Subtitle
			Label subtitleLabel = new Label("Select a sample to run:");
			subtitleLabel.Position = new Vector2(20, 60);
			subtitleLabel.Size = new Vector2(760, 24);
			subtitleLabel.Alignment = Align.Center;
			_fui.AddControl(subtitleLabel);

			// Sample buttons in a grid layout
			const int buttonsPerRow = 2;
			const float buttonWidth = 350;
			const float buttonHeight = 40;
			const float buttonPadding = 20;
			const float startY = 100;
			const float startX = 40;

			for (int i = 0; i < _samples.Length; i++)
			{
				int row = i / buttonsPerRow;
				int col = i % buttonsPerRow;

				float x = startX + col * (buttonWidth + buttonPadding);
				float y = startY + row * (buttonHeight + buttonPadding);

				Button sampleBtn = new Button();
				sampleBtn.Text = $"{i + 1}. {_samples[i].Name}";
				sampleBtn.Position = new Vector2(x, y);
				sampleBtn.Size = new Vector2(buttonWidth, buttonHeight);

				int sampleIndex = i; // Capture for closure
				sampleBtn.OnButtonPressed += (btn, mbtn, pos) =>
				{
					_selectedSample = _samples[sampleIndex];
					_selectionMade = true;
				};

				_fui.AddControl(sampleBtn);
			}

			// Footer with instructions
			float footerY = startY + ((_samples.Length + buttonsPerRow - 1) / buttonsPerRow) * (buttonHeight + buttonPadding) + 20;

			Label footerLabel = new Label("Click a button or close this window to exit");
			footerLabel.Position = new Vector2(20, footerY);
			footerLabel.Size = new Vector2(760, 24);
			footerLabel.Alignment = Align.Center;
			_fui.AddControl(footerLabel);

			// Version/info panel
			Panel infoPanel = new Panel();
			infoPanel.Position = new Vector2(20, footerY + 40);
			infoPanel.Size = new Vector2(760, 60);
			_fui.AddControl(infoPanel);

			Label infoLabel = new Label("FishUI - GUI Library for .NET\nSelf-dogfooding: This chooser is built with FishUI!");
			infoLabel.Position = new Vector2(10, 10);
			infoLabel.Size = new Vector2(740, 40);
			infoLabel.Alignment = Align.Center;
			infoPanel.AddChild(infoLabel);
		}
	}
}
