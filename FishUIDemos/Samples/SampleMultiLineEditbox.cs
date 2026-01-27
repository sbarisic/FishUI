using FishUI;
using FishUI.Controls;
using System;
using System.Numerics;

namespace FishUIDemos
{
	/// <summary>
	/// Demonstrates the MultiLineEditbox control for multi-line text editing.
	/// </summary>
	public class SampleMultiLineEditbox : ISample
	{
		FishUI.FishUI FUI;
		MultiLineEditbox mainEditor;
		MultiLineEditbox readOnlyEditor;
		Label lineCountLabel;
		Label cursorPosLabel;

		public string Name => "MultiLineEditbox";

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
			Label titleLabel = new Label("MultiLineEditbox Control Demo");
			titleLabel.Position = new Vector2(20, 20);
			titleLabel.Size = new Vector2(350, 30);
			titleLabel.Alignment = Align.Left;
			FUI.AddControl(titleLabel);

			// Screenshot button
			ImageRef iconCamera = FUI.Graphics.LoadImage("data/silk_icons/camera.png");
			Button screenshotBtn = new Button();
			screenshotBtn.Icon = iconCamera;
			screenshotBtn.Position = new Vector2(380, 20);
			screenshotBtn.Size = new Vector2(30, 30);
			screenshotBtn.IsImageButton = true;
			screenshotBtn.TooltipText = "Take a screenshot";
			screenshotBtn.OnButtonPressed += (btn, mbtn, pos) => TakeScreenshot?.Invoke(Name);
			FUI.AddControl(screenshotBtn);

			// === Editable Text Editor ===
			Label editableLabel = new Label("Editable Text Editor:");
			editableLabel.Position = new Vector2(20, 60);
			editableLabel.Size = new Vector2(200, 20);
			editableLabel.Alignment = Align.Left;
			FUI.AddControl(editableLabel);

			mainEditor = new MultiLineEditbox();
			mainEditor.Position = new Vector2(20, 85);
			mainEditor.Size = new Vector2(380, 200);
			mainEditor.ShowLineNumbers = true;
			mainEditor.Text = "Welcome to MultiLineEditbox!\n\nThis is a multi-line text editor.\nYou can:\n- Type text\n- Press Enter for new lines\n- Use arrow keys to navigate\n- Home/End to go to line start/end\n- Page Up/Down to scroll\n- Mouse click to position cursor\n- Mouse wheel to scroll\n\nTry editing this text!";
			mainEditor.OnTextChanged += (sender, text) => UpdateStats();
			FUI.AddControl(mainEditor);

			// === Stats ===
			lineCountLabel = new Label("Lines: 0");
			lineCountLabel.Position = new Vector2(20, 295);
			lineCountLabel.Size = new Vector2(150, 20);
			lineCountLabel.Alignment = Align.Left;
			FUI.AddControl(lineCountLabel);

			cursorPosLabel = new Label("Cursor: 0, 0");
			cursorPosLabel.Position = new Vector2(180, 295);
			cursorPosLabel.Size = new Vector2(150, 20);
			cursorPosLabel.Alignment = Align.Left;
			FUI.AddControl(cursorPosLabel);

			// === Buttons ===
			Button clearBtn = new Button();
			clearBtn.Text = "Clear";
			clearBtn.Position = new Vector2(20, 320);
			clearBtn.Size = new Vector2(80, 30);
			clearBtn.OnButtonPressed += (btn, mbtn, pos) =>
			{
				mainEditor.Clear();
			};
			FUI.AddControl(clearBtn);

			Button appendBtn = new Button();
			appendBtn.Text = "Append";
			appendBtn.Position = new Vector2(110, 320);
			appendBtn.Size = new Vector2(80, 30);
			appendBtn.OnButtonPressed += (btn, mbtn, pos) =>
			{
				mainEditor.AppendText($"\nAppended at {DateTime.Now:HH:mm:ss}");
				mainEditor.ScrollToEnd();
			};
			FUI.AddControl(appendBtn);

			Button scrollEndBtn = new Button();
			scrollEndBtn.Text = "Go to End";
			scrollEndBtn.Position = new Vector2(200, 320);
			scrollEndBtn.Size = new Vector2(90, 30);
			scrollEndBtn.OnButtonPressed += (btn, mbtn, pos) =>
			{
				mainEditor.ScrollToEnd();
			};
			FUI.AddControl(scrollEndBtn);

			Button scrollStartBtn = new Button();
			scrollStartBtn.Text = "Go to Start";
			scrollStartBtn.Position = new Vector2(300, 320);
			scrollStartBtn.Size = new Vector2(100, 30);
			scrollStartBtn.OnButtonPressed += (btn, mbtn, pos) =>
			{
				mainEditor.ScrollToStart();
			};
			FUI.AddControl(scrollStartBtn);

			// === Read-Only Editor ===
			Label readOnlyLabel = new Label("Read-Only Log Viewer:");
			readOnlyLabel.Position = new Vector2(420, 60);
			readOnlyLabel.Size = new Vector2(200, 20);
			readOnlyLabel.Alignment = Align.Left;
			FUI.AddControl(readOnlyLabel);

			readOnlyEditor = new MultiLineEditbox();
			readOnlyEditor.Position = new Vector2(420, 85);
			readOnlyEditor.Size = new Vector2(360, 200);
			readOnlyEditor.ReadOnly = true;
			readOnlyEditor.ShowLineNumbers = false;
			readOnlyEditor.Text = "[INFO] Application started\n[INFO] Loading resources...\n[INFO] Resources loaded successfully\n[DEBUG] Initializing UI components\n[INFO] Ready";
			FUI.AddControl(readOnlyEditor);

			Button addLogBtn = new Button();
			addLogBtn.Text = "Add Log Entry";
			addLogBtn.Position = new Vector2(420, 295);
			addLogBtn.Size = new Vector2(120, 30);
			addLogBtn.OnButtonPressed += (btn, mbtn, pos) =>
			{
				string[] levels = { "INFO", "DEBUG", "WARN", "ERROR" };
				string[] messages = {
					"User action detected",
					"Processing request",
					"Cache hit",
					"Connection established",
					"Data synchronized",
					"Background task completed"
				};
				Random rnd = new Random();
				string level = levels[rnd.Next(levels.Length)];
				string msg = messages[rnd.Next(messages.Length)];
				readOnlyEditor.AppendText($"\n[{level}] {msg}");
				readOnlyEditor.ScrollToEnd();
			};
			FUI.AddControl(addLogBtn);

			Button clearLogBtn = new Button();
			clearLogBtn.Text = "Clear Log";
			clearLogBtn.Position = new Vector2(550, 295);
			clearLogBtn.Size = new Vector2(100, 30);
			clearLogBtn.OnButtonPressed += (btn, mbtn, pos) =>
			{
				readOnlyEditor.Clear();
				readOnlyEditor.AppendText("[INFO] Log cleared");
			};
			FUI.AddControl(clearLogBtn);

			// === Simple Editor (no line numbers) ===
			Label simpleLabel = new Label("Simple Editor (no line numbers):");
			simpleLabel.Position = new Vector2(420, 340);
			simpleLabel.Size = new Vector2(250, 20);
			simpleLabel.Alignment = Align.Left;
			FUI.AddControl(simpleLabel);

			MultiLineEditbox simpleEditor = new MultiLineEditbox();
			simpleEditor.Position = new Vector2(420, 365);
			simpleEditor.Size = new Vector2(360, 100);
			simpleEditor.ShowLineNumbers = false;
			simpleEditor.Placeholder = "Enter your notes here...";
			FUI.AddControl(simpleEditor);

			UpdateStats();
		}

		private void UpdateStats()
		{
			lineCountLabel.Text = $"Lines: {mainEditor.LineCount}";
			cursorPosLabel.Text = $"Cursor: {mainEditor.CursorRow + 1}, {mainEditor.CursorColumn + 1}";
		}

		public void Update(float Dt)
		{
			// Update cursor position display
			cursorPosLabel.Text = $"Cursor: {mainEditor.CursorRow + 1}, {mainEditor.CursorColumn + 1}";
		}

		public void Draw(float Dt, float Time)
		{
		}

		public void Dispose()
		{
		}
	}
}

