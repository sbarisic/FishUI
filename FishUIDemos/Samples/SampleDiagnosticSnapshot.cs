using System.Numerics;
using FishUI;
using FishUI.Controls;

namespace FishUIDemos
{
	/// <summary>
	/// Demonstrates an in-memory diagnostic capture with the multiline Notepad editing flow.
	/// </summary>
	public sealed class SampleDiagnosticSnapshot : ISample
	{
		private FishUI.FishUI _ui;
		private Label _status;

		public string Name => "Diagnostic Snapshot";
		public TakeScreenshotFunc TakeScreenshot { get; set; }

		public FishUI.FishUI CreateUI(FishUISettings settings, IFishUIGfx graphics, IFishUIInput input, IFishUIEvents events)
		{
			_ui = new FishUI.FishUI(settings, graphics, input, events);
			_ui.Init();
			settings.LoadTheme(ThemePreferences.LoadThemePath(), applyImmediately: true);
			_ui.Diagnostics.Enabled = true;
			_ui.Diagnostics.CaptureCompleted += HandleCaptureCompleted;
			return _ui;
		}

		public void Init()
		{
			var editor = new MultiLineEditbox
			{
				ID = "diagnosticEditor",
				Position = new Vector2(20, 50),
				Size = new Vector2(620, 350),
				ShowLineNumbers = true,
				WordWrap = false,
				Text = "FishUI diagnostic snapshot sample\n\nType or scroll here, then press Capture next draw.\nCtrl+Shift+F12 also queues a capture."
			};
			_ui.AddControl(editor);

			var capture = new Button
			{
				ID = "captureDiagnostics",
				Text = "Capture next draw",
				Position = new Vector2(20, 15),
				Size = new Vector2(170, 28)
			};
			capture.OnButtonPressed += (_, _, _) =>
			{
				_status.Text = "Capture queued...";
				_ = FishUIDiagnostics.CaptureAsync(_ui, new FishUIDebugSnapshotOptions
				{
					IncludeScreenshot = false,
					IncludeAnnotatedOverlay = false
				});
			};
			_ui.AddControl(capture);

			_status = new Label("No capture yet")
			{
				ID = "diagnosticStatus",
				Position = new Vector2(205, 19),
				Size = new Vector2(435, 22),
				Alignment = Align.Left
			};
			_ui.AddControl(_status);
		}

		private void HandleCaptureCompleted(object sender, FishUICaptureCompletedEventArgs args)
		{
			_status.Text = $"Session {args.UiSessionId:N}, capture {args.CaptureId}, request {args.RequestId}: {args.Snapshot.CaptureStatus}";
		}
	}
}
