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
	/// GUI-based sample chooser that displays available samples in a ListBox.
	/// Self-dogfooding: uses FishUI to demonstrate FishUI.
	/// </summary>
	internal class SampleChooser
	{
		private ISample[] _samples;
		private ISample _selectedSample;
		private bool _selectionMade;
		private FishUI.FishUI _fui;
		private ListBox _sampleListBox;

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
			Label subtitleLabel = new Label("Select a sample and click 'Launch':");
			subtitleLabel.Position = new Vector2(20, 60);
			subtitleLabel.Size = new Vector2(760, 24);
			subtitleLabel.Alignment = Align.Center;
			_fui.AddControl(subtitleLabel);

			// Sample ListBox
			_sampleListBox = new ListBox();
			_sampleListBox.Position = new Vector2(150, 100);
			_sampleListBox.Size = new Vector2(500, 350);
			_sampleListBox.AlternatingRowColors = true;
			_sampleListBox.EvenRowColor = new FishColor(200, 220, 255, 40);
			_sampleListBox.OddRowColor = new FishColor(255, 255, 255, 10);
			_sampleListBox.TooltipText = "Select a sample to launch";
			_fui.AddControl(_sampleListBox);

			// Add samples to ListBox
			for (int i = 0; i < _samples.Length; i++)
			{
				_sampleListBox.AddItem($"{i + 1}. {_samples[i].Name}");
			}

			// Select first item by default
			if (_samples.Length > 0)
				_sampleListBox.SelectIndex(0);

			// Launch button
			Button launchBtn = new Button();
			launchBtn.Text = "Launch Selected";
			launchBtn.Position = new Vector2(300, 470);
			launchBtn.Size = new Vector2(200, 40);
			launchBtn.TooltipText = "Launch the selected sample";
			launchBtn.OnButtonPressed += (btn, mbtn, pos) =>
			{
				int idx = _sampleListBox.SelectedIndex;
				if (idx >= 0 && idx < _samples.Length)
				{
					_selectedSample = _samples[idx];
					_selectionMade = true;
				}
			};
			_fui.AddControl(launchBtn);

			// Footer with instructions
			Label footerLabel = new Label("Select a sample from the list and click 'Launch' to run it");
			footerLabel.Position = new Vector2(20, 520);
			footerLabel.Size = new Vector2(760, 24);
			footerLabel.Alignment = Align.Center;
			_fui.AddControl(footerLabel);

			// Version/info panel
			Panel infoPanel = new Panel();
			infoPanel.Position = new Vector2(20, 550);
			infoPanel.Size = new Vector2(760, 40);
			_fui.AddControl(infoPanel);

			Label infoLabel = new Label("FishUI - GUI Library for .NET | Self-dogfooding: This chooser is built with FishUI!");
			infoLabel.Position = new Vector2(10, 10);
			infoLabel.Size = new Vector2(740, 20);
			infoLabel.Alignment = Align.Center;
			infoPanel.AddChild(infoLabel);
		}
	}
}
