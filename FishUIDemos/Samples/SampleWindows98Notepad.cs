using FishUI;
using FishUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace FishUIDemos
{
	/// <summary>
	/// A functional, self-contained recreation of the Windows 98 Notepad experience.
	/// Documents are stored in a private in-memory file system and never touch the host disk.
	/// </summary>
	public class SampleWindows98Notepad : ISample
	{
		private const string UntitledName = "Untitled";

		private FishUI.FishUI _fui;
		private IFishUIInput _input;
		private Window _notepadWindow;
		private MultiLineEditbox _editor;
		private readonly DemoNotepadFileSystem _fileSystem = new DemoNotepadFileSystem();
		private string _currentPath;
		private string _lastDirectory = DemoNotepadFileSystem.DocumentsDirectory;
		private bool _dirty;
		private bool _suppressDirtyTracking;
		private string _findText = "";
		private bool _matchCase;
		private Action _pendingAction;

		public string Name => "Windows 98 Notepad";

		public TakeScreenshotFunc TakeScreenshot { get; set; }

		public FishUI.FishUI CreateUI(FishUISettings UISettings, IFishUIGfx Gfx, IFishUIInput Input, IFishUIEvents Events)
		{
			_input = Input;
			_fui = new FishUI.FishUI(UISettings, Gfx, Input, Events);
			_fui.Init();
			_fui.Resized(Gfx.GetWindowWidth(), Gfx.GetWindowHeight());

			// This sample intentionally uses the classic GWEN skin regardless of the chooser preference.
			UISettings.LoadTheme("data/themes/gwen.yaml", applyImmediately: true);
			return _fui;
		}

		public void Init()
		{
			CreateNotepadWindow();
			RegisterHotkeys();
			_fui.FocusControl(_editor);
		}

		private void CreateNotepadWindow()
		{
			Vector2 windowSize = new Vector2(820, 560);
			_notepadWindow = new Window(GetWindowTitle(), windowSize)
			{
				Position = new Vector2(
					Math.Max(20, (_fui.Width - windowSize.X) / 2),
					Math.Max(20, (_fui.Height - windowSize.Y) / 2)),
				MinSize = new Vector2(420, 280),
				IsResizable = true,
				ShowCloseButton = true,
				ShowShadow = true
			};
			_notepadWindow.OnClosing += HandleNotepadClosing;
			_fui.AddControl(_notepadWindow);

			Vector2 contentSize = _notepadWindow.GetContentSize();
			MenuBar menuBar = new MenuBar
			{
				Position = Vector2.Zero,
				Size = new Vector2(contentSize.X, 24),
				Anchor = FishUIAnchor.Top | FishUIAnchor.Left | FishUIAnchor.Right,
				BarHeight = 24
			};
			_notepadWindow.AddChild(menuBar);

			_editor = new MultiLineEditbox
			{
				Position = new Vector2(0, 24),
				Size = new Vector2(contentSize.X, contentSize.Y - 24),
				Anchor = FishUIAnchor.All,
				TextPadding = 3,
				ShowLineNumbers = false,
				ShowScrollBar = true,
				ShowHorizontalScrollBar = true,
				WordWrap = false,
				BackgroundColor = FishColor.White,
				TextColor = FishColor.Black
			};
			_editor.OnTextChanged += (_, _) =>
			{
				if (!_suppressDirtyTracking)
					_dirty = true;
			};
			_notepadWindow.AddChild(_editor);

			BuildMenus(menuBar);
		}

		private void BuildMenus(MenuBar menuBar)
		{
			MenuBarItem fileMenu = menuBar.AddMenu("File");
			MenuItem newItem = fileMenu.AddItem("New");
			newItem.ShortcutText = "Ctrl+N";
			newItem.OnClicked += _ => RequestNewDocument();

			MenuItem openItem = fileMenu.AddItem("Open...");
			openItem.ShortcutText = "Ctrl+O";
			openItem.OnClicked += _ => ShowOpenDialog();

			MenuItem saveItem = fileMenu.AddItem("Save");
			saveItem.ShortcutText = "Ctrl+S";
			saveItem.OnClicked += _ => SaveDocument();

			MenuItem saveAsItem = fileMenu.AddItem("Save As...");
			saveAsItem.OnClicked += _ => ShowSaveAsDialog();

			fileMenu.AddSeparator();
			fileMenu.AddItem("Page Setup...").Disabled = true;
			fileMenu.AddItem("Print...").Disabled = true;
			fileMenu.AddSeparator();

			MenuItem exitItem = fileMenu.AddItem("Exit");
			exitItem.OnClicked += _ => _notepadWindow.Close();

			MenuBarItem editMenu = menuBar.AddMenu("Edit");
			editMenu.AddItem("Undo").Disabled = true;
			editMenu.AddSeparator();

			MenuItem cutItem = editMenu.AddItem("Cut");
			cutItem.ShortcutText = "Ctrl+X";
			cutItem.OnClicked += _ => CutSelection();

			MenuItem copyItem = editMenu.AddItem("Copy");
			copyItem.ShortcutText = "Ctrl+C";
			copyItem.OnClicked += _ => CopySelection();

			MenuItem pasteItem = editMenu.AddItem("Paste");
			pasteItem.ShortcutText = "Ctrl+V";
			pasteItem.OnClicked += _ => PasteClipboard();

			MenuItem deleteItem = editMenu.AddItem("Delete");
			deleteItem.ShortcutText = "Del";
			deleteItem.OnClicked += _ => DeleteSelection();

			editMenu.AddSeparator();
			MenuItem selectAllItem = editMenu.AddItem("Select All");
			selectAllItem.ShortcutText = "Ctrl+A";
			selectAllItem.OnClicked += _ =>
			{
				_editor.SelectAll();
				_fui.FocusControl(_editor);
			};

			MenuItem timeDateItem = editMenu.AddItem("Time/Date");
			timeDateItem.ShortcutText = "F5";
			timeDateItem.OnClicked += _ => InsertTimeAndDate();

			editMenu.AddSeparator();
			MenuItem wordWrapItem = editMenu.AddCheckItem("Word Wrap", false);
			wordWrapItem.OnClicked += item =>
			{
				_editor.WordWrap = item.IsChecked;
				_editor.ScrollOffsetPixels = 0;
				_editor.HorizontalScrollOffsetPixels = 0;
				_fui.FocusControl(_editor);
			};

			editMenu.OnOpened += _ =>
			{
				bool hasSelection = _editor.HasSelection;
				cutItem.Disabled = !hasSelection;
				copyItem.Disabled = !hasSelection;
				deleteItem.Disabled = !hasSelection;
			};

			MenuBarItem searchMenu = menuBar.AddMenu("Search");
			MenuItem findItem = searchMenu.AddItem("Find...");
			findItem.ShortcutText = "Ctrl+F";
			findItem.OnClicked += _ => ShowFindDialog();

			MenuItem findNextItem = searchMenu.AddItem("Find Next");
			findNextItem.ShortcutText = "F3";
			findNextItem.OnClicked += _ => FindNext();

			MenuBarItem helpMenu = menuBar.AddMenu("Help");
			helpMenu.AddItem("Help Topics").Disabled = true;
			helpMenu.AddSeparator();
			MenuItem aboutItem = helpMenu.AddItem("About Notepad");
			aboutItem.OnClicked += _ => ShowAboutDialog();
		}

		private void RegisterHotkeys()
		{
			RegisterHotkey(FishKey.N, FishKeyModifiers.Control, RequestNewDocument, "notepad.new");
			RegisterHotkey(FishKey.O, FishKeyModifiers.Control, ShowOpenDialog, "notepad.open");
			RegisterHotkey(FishKey.S, FishKeyModifiers.Control, () => SaveDocument(), "notepad.save");
			RegisterHotkey(FishKey.F, FishKeyModifiers.Control, ShowFindDialog, "notepad.find");
			RegisterHotkey(FishKey.F3, FishKeyModifiers.None, FindNext, "notepad.find-next");
			RegisterHotkey(FishKey.F5, FishKeyModifiers.None, InsertTimeAndDate, "notepad.time-date");
		}

		private void RegisterHotkey(FishKey key, FishKeyModifiers modifiers, Action action, string id)
		{
			_fui.Hotkeys.Register(key, modifiers, _ =>
			{
				if (_fui.ModalControl == null && _notepadWindow.Visible)
					action();
			}, id);
		}

		private void RequestNewDocument()
		{
			RequestDestructiveAction(() => SetDocument(null, ""));
		}

		private void HandleNotepadClosing(object sender, WindowCloseEventArgs args)
		{
			if (!_dirty)
				return;

			args.Cancel = true;
			RequestDestructiveAction(() =>
			{
				_dirty = false;
				_notepadWindow.Close();
			});
		}

		private void RequestDestructiveAction(Action action)
		{
			if (!_dirty)
			{
				action();
				return;
			}

			_pendingAction = action;
			string documentName = _currentPath == null ? UntitledName : _fileSystem.GetFileName(_currentPath);
			ShowChoiceDialog(
				"Notepad",
				$"The text in the {documentName} file has changed.\nDo you want to save the changes?",
				("Yes", () => SaveDocument(RunPendingAction, CancelPendingAction)),
				("No", RunPendingAction),
				("Cancel", CancelPendingAction));
		}

		private void RunPendingAction()
		{
			Action action = _pendingAction;
			_pendingAction = null;
			action?.Invoke();
		}

		private void CancelPendingAction()
		{
			_pendingAction = null;
			_fui.FocusControl(_editor);
		}

		private void ShowOpenDialog()
		{
			FilePickerDialog dialog = new FilePickerDialog(
				FilePickerMode.Open,
				_fileSystem,
				_lastDirectory,
				"*.txt")
			{
				Title = "Open",
				IsModal = true
			};

			dialog.OnFileConfirmed += (_, path) =>
			{
				_lastDirectory = _fileSystem.GetDirectoryName(path) ?? _lastDirectory;
				RequestDestructiveAction(() => LoadDocument(path));
			};
			dialog.OnDialogCancelled += _ => _fui.FocusControl(_editor);
			ShowFilePicker(dialog);
		}

		private void SaveDocument(Action afterSave = null, Action onCancel = null)
		{
			if (_currentPath == null)
			{
				ShowSaveAsDialog(afterSave, onCancel);
				return;
			}

			_fileSystem.WriteAllText(_currentPath, _editor.Text);
			_dirty = false;
			UpdateWindowTitle();
			afterSave?.Invoke();
		}

		private void ShowSaveAsDialog(Action afterSave = null, Action onCancel = null)
		{
			FilePickerDialog dialog = new FilePickerDialog(
				FilePickerMode.Save,
				_fileSystem,
				_lastDirectory,
				"*.txt")
			{
				Title = "Save As",
				FileName = _currentPath == null ? "Untitled.txt" : _fileSystem.GetFileName(_currentPath),
				IsModal = true
			};

			dialog.OnFileConfirmed += (_, selectedPath) =>
			{
				string path = EnsureTextExtension(selectedPath);
				_lastDirectory = _fileSystem.GetDirectoryName(path) ?? _lastDirectory;

				if (_fileSystem.Exists(path) && !string.Equals(path, _currentPath, StringComparison.OrdinalIgnoreCase))
				{
					ShowChoiceDialog(
						"Save As",
						$"{_fileSystem.GetFileName(path)} already exists.\nDo you want to replace it?",
						("Yes", () => CompleteSaveAs(path, afterSave)),
						("No", () => ShowSaveAsDialog(afterSave, onCancel)));
					return;
				}

				CompleteSaveAs(path, afterSave);
			};
			dialog.OnDialogCancelled += _ =>
			{
				onCancel?.Invoke();
				_fui.FocusControl(_editor);
			};
			ShowFilePicker(dialog);
		}

		private void CompleteSaveAs(string path, Action afterSave)
		{
			_currentPath = path;
			_fileSystem.WriteAllText(path, _editor.Text);
			_dirty = false;
			UpdateWindowTitle();
			afterSave?.Invoke();
		}

		private void ShowFilePicker(FilePickerDialog dialog)
		{
			dialog.Show(_fui);
			_fui.SetModalControl(dialog);
		}

		private void LoadDocument(string path)
		{
			SetDocument(path, _fileSystem.ReadAllText(path));
		}

		private void SetDocument(string path, string text)
		{
			_suppressDirtyTracking = true;
			_editor.Text = text ?? "";
			_editor.CursorRow = 0;
			_editor.CursorColumn = 0;
			_editor.ClearSelection();
			_editor.ScrollToStart();
			_suppressDirtyTracking = false;
			_currentPath = path;
			_dirty = false;
			UpdateWindowTitle();
			_fui.FocusControl(_editor);
		}

		private void UpdateWindowTitle()
		{
			_notepadWindow.Title = GetWindowTitle();
		}

		private string GetWindowTitle()
		{
			string documentName = _currentPath == null ? UntitledName : _fileSystem.GetFileName(_currentPath);
			return $"{documentName} - Notepad";
		}

		private static string EnsureTextExtension(string path)
		{
			string fileName = path.Replace('/', '\\');
			int separator = fileName.LastIndexOf('\\');
			int period = fileName.LastIndexOf('.');
			return period <= separator ? path + ".txt" : path;
		}

		private void CopySelection()
		{
			string text = _editor.Copy();
			if (!string.IsNullOrEmpty(text))
				_input.SetClipboardText(text);
			_fui.FocusControl(_editor);
		}

		private void CutSelection()
		{
			string text = _editor.Cut();
			if (!string.IsNullOrEmpty(text))
				_input.SetClipboardText(text);
			_fui.FocusControl(_editor);
		}

		private void PasteClipboard()
		{
			_editor.Paste(_input.GetClipboardText() ?? "");
			_fui.FocusControl(_editor);
		}

		private void DeleteSelection()
		{
			if (_editor.HasSelection)
				_editor.Cut();
			_fui.FocusControl(_editor);
		}

		private void InsertTimeAndDate()
		{
			DateTime now = DateTime.Now;
			_editor.InsertText($"{now:t} {now:d}");
			_fui.FocusControl(_editor);
		}

		private void ShowFindDialog()
		{
			Window dialog = CreateModalWindow("Find", new Vector2(430, 165));

			Label findLabel = new Label("Find what:")
			{
				Position = new Vector2(12, 14),
				Size = new Vector2(85, 24),
				Alignment = Align.Left
			};
			dialog.AddChild(findLabel);

			Textbox findBox = new Textbox(_findText)
			{
				Position = new Vector2(100, 12),
				Size = new Vector2(210, 25)
			};
			dialog.AddChild(findBox);

			CheckBox matchCase = new CheckBox("Match case")
			{
				Position = new Vector2(100, 55),
				Size = new Vector2(16, 16),
				IsChecked = _matchCase
			};
			dialog.AddChild(matchCase);

			Button findNext = new Button
			{
				Text = "Find Next",
				Position = new Vector2(325, 12),
				Size = new Vector2(90, 28)
			};
			findNext.OnButtonPressed += (_, _, _) =>
			{
				_findText = findBox.Text;
				_matchCase = matchCase.IsChecked;
				dialog.Close();
				FindNext();
			};
			dialog.AddChild(findNext);

			Button cancel = new Button
			{
				Text = "Cancel",
				Position = new Vector2(325, 50),
				Size = new Vector2(90, 28)
			};
			cancel.OnButtonPressed += (_, _, _) =>
			{
				dialog.Close();
				_fui.FocusControl(_editor);
			};
			dialog.AddChild(cancel);

			ShowModalWindow(dialog);
			_fui.FocusControl(findBox);
		}

		private void FindNext()
		{
			if (string.IsNullOrEmpty(_findText))
			{
				ShowFindDialog();
				return;
			}

			string text = _editor.Text;
			int startOffset = GetDocumentOffset(_editor.CursorRow, _editor.CursorColumn);
			if (_editor.HasSelection)
			{
				var (_, end) = _editor.GetSelectionRange();
				startOffset = GetDocumentOffset(end.Row, end.Col);
			}

			StringComparison comparison = _matchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
			int match = text.IndexOf(_findText, Math.Clamp(startOffset, 0, text.Length), comparison);
			if (match < 0)
			{
				ShowMessageDialog("Notepad", $"Cannot find \"{_findText}\"");
				return;
			}

			(int startRow, int startColumn) = GetRowAndColumn(match);
			(int endRow, int endColumn) = GetRowAndColumn(match + _findText.Length);
			_editor.SelectionStartRow = startRow;
			_editor.SelectionStartColumn = startColumn;
			_editor.SelectionEndRow = endRow;
			_editor.SelectionEndColumn = endColumn;
			_editor.CursorRow = endRow;
			_editor.CursorColumn = endColumn;
			_fui.FocusControl(_editor);
		}

		private int GetDocumentOffset(int row, int column)
		{
			int offset = 0;
			for (int i = 0; i < row && i < _editor.Lines.Count; i++)
				offset += _editor.Lines[i].Length + 1;
			return offset + column;
		}

		private (int Row, int Column) GetRowAndColumn(int offset)
		{
			int remaining = Math.Clamp(offset, 0, _editor.Text.Length);
			for (int row = 0; row < _editor.Lines.Count; row++)
			{
				int length = _editor.Lines[row].Length;
				if (remaining <= length)
					return (row, remaining);
				remaining -= length + 1;
			}

			int lastRow = Math.Max(0, _editor.Lines.Count - 1);
			return (lastRow, _editor.Lines[lastRow].Length);
		}

		private void ShowAboutDialog()
		{
			Window dialog = CreateModalWindow("About Notepad", new Vector2(420, 210));

			ImageRef icon = _fui.Graphics.LoadImage("data/images/help_win95.png");
			ImageBox image = new ImageBox(icon)
			{
				Position = new Vector2(18, 22),
				Size = new Vector2(64, 64),
				ScaleMode = ImageScaleMode.Fit,
				FilterMode = ImageFilterMode.Pixelated
			};
			dialog.AddChild(image);

			Label title = new Label("Microsoft Notepad")
			{
				Position = new Vector2(100, 22),
				Size = new Vector2(280, 24),
				Alignment = Align.Left
			};
			dialog.AddChild(title);

			Label description = new Label("Windows 98-style sample for FishUI\nDocuments are stored in memory only.")
			{
				Position = new Vector2(100, 52),
				Size = new Vector2(290, 55),
				Alignment = Align.Left
			};
			dialog.AddChild(description);

			Button ok = new Button
			{
				Text = "OK",
				Position = new Vector2(310, 125),
				Size = new Vector2(80, 28)
			};
			ok.OnButtonPressed += (_, _, _) =>
			{
				dialog.Close();
				_fui.FocusControl(_editor);
			};
			dialog.AddChild(ok);

			ShowModalWindow(dialog);
		}

		private void ShowMessageDialog(string title, string message)
		{
			ShowChoiceDialog(title, message, ("OK", () => _fui.FocusControl(_editor)));
		}

		private void ShowChoiceDialog(string title, string message, params (string Text, Action Action)[] choices)
		{
			Window dialog = CreateModalWindow(title, new Vector2(460, 175));

			Label label = new Label(message)
			{
				Position = new Vector2(18, 18),
				Size = new Vector2(420, 55),
				Alignment = Align.Left
			};
			dialog.AddChild(label);

			const float buttonWidth = 90;
			const float gap = 10;
			float totalWidth = choices.Length * buttonWidth + Math.Max(0, choices.Length - 1) * gap;
			float startX = 460 - 12 - totalWidth;

			for (int i = 0; i < choices.Length; i++)
			{
				(string buttonText, Action action) = choices[i];
				Button button = new Button
				{
					Text = buttonText,
					Position = new Vector2(startX + i * (buttonWidth + gap), 92),
					Size = new Vector2(buttonWidth, 28)
				};
				button.OnButtonPressed += (_, _, _) =>
				{
					dialog.Close();
					action?.Invoke();
				};
				dialog.AddChild(button);
			}

			ShowModalWindow(dialog);
		}

		private Window CreateModalWindow(string title, Vector2 size)
		{
			return new Window(title, size)
			{
				IsResizable = false,
				IsModal = true,
				AlwaysOnTop = true,
				ShowShadow = true
			};
		}

		private void ShowModalWindow(Window dialog)
		{
			_fui.AddControl(dialog);
			dialog.CenterOnScreen();
			dialog.ShowModal();
		}

		public void Update(float dt)
		{
		}

		private sealed class DemoNotepadFileSystem : IFishUIFileSystem
		{
			public const string RootDirectory = "C:\\";
			public const string DocumentsDirectory = "C:\\My Documents";

			private readonly Dictionary<string, string> _files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
				[$"{DocumentsDirectory}\\WELCOME.TXT"] =
					"Welcome to the FishUI Windows 98 Notepad sample!\n\n" +
					"This document lives entirely in memory. Try editing it, using Find, and saving a copy.",
				[$"{DocumentsDirectory}\\NOTES.TXT"] =
					"FishUI Notepad notes:\n- File dialogs use a virtual drive\n- Clipboard commands use the host clipboard\n- Word Wrap is implemented by MultiLineEditbox"
			};

			public bool Exists(string path) => _files.ContainsKey(Normalize(path));

			public string ReadAllText(string path)
			{
				return _files.TryGetValue(Normalize(path), out string value) ? value : "";
			}

			public void WriteAllText(string path, string contents)
			{
				_files[Normalize(path)] = contents ?? "";
			}

			public string GetFullPath(string path)
			{
				if (string.IsNullOrWhiteSpace(path) || path == ".")
					return DocumentsDirectory;
				return Normalize(path);
			}

			public string GetDirectoryName(string path)
			{
				string normalized = Normalize(path);
				if (string.Equals(normalized, RootDirectory, StringComparison.OrdinalIgnoreCase))
					return null;

				int separator = normalized.LastIndexOf('\\');
				if (separator <= 2)
					return RootDirectory;
				return normalized.Substring(0, separator);
			}

			public string CombinePath(string path1, string path2)
			{
				if (!string.IsNullOrEmpty(path2) && path2.Length >= 2 && path2[1] == ':')
					return Normalize(path2);
				return Normalize($"{path1?.TrimEnd('\\', '/') ?? RootDirectory}\\{path2?.TrimStart('\\', '/') ?? ""}");
			}

			public string GetFileName(string path)
			{
				string normalized = Normalize(path);
				int separator = normalized.LastIndexOf('\\');
				return separator >= 0 ? normalized.Substring(separator + 1) : normalized;
			}

			public string[] GetDirectories(string path)
			{
				string normalized = Normalize(path);
				if (string.Equals(normalized, RootDirectory, StringComparison.OrdinalIgnoreCase))
					return new[] { DocumentsDirectory };
				return Array.Empty<string>();
			}

			public string[] GetFiles(string path, string searchPattern = "*")
			{
				string directory = Normalize(path);
				bool textOnly = string.Equals(searchPattern, "*.txt", StringComparison.OrdinalIgnoreCase);
				return _files.Keys
					.Where(file => string.Equals(GetDirectoryName(file), directory, StringComparison.OrdinalIgnoreCase))
					.Where(file => !textOnly || file.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
					.OrderBy(file => file, StringComparer.OrdinalIgnoreCase)
					.ToArray();
			}

			public bool IsDirectory(string path)
			{
				string normalized = Normalize(path);
				return string.Equals(normalized, RootDirectory, StringComparison.OrdinalIgnoreCase) ||
					string.Equals(normalized, DocumentsDirectory, StringComparison.OrdinalIgnoreCase);
			}

			public string GetParentDirectory(string path)
			{
				string normalized = Normalize(path);
				if (string.Equals(normalized, DocumentsDirectory, StringComparison.OrdinalIgnoreCase))
					return RootDirectory;
				if (string.Equals(normalized, RootDirectory, StringComparison.OrdinalIgnoreCase))
					return null;
				return GetDirectoryName(normalized);
			}

			private static string Normalize(string path)
			{
				if (string.IsNullOrWhiteSpace(path))
					return RootDirectory;

				string normalized = path.Trim().Replace('/', '\\');
				while (normalized.Contains("\\\\"))
					normalized = normalized.Replace("\\\\", "\\");

				if (string.Equals(normalized, "C:", StringComparison.OrdinalIgnoreCase) ||
					string.Equals(normalized, RootDirectory, StringComparison.OrdinalIgnoreCase))
					return RootDirectory;

				return normalized.TrimEnd('\\');
			}
		}
	}
}
