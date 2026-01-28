using System;
using System.IO;
using System.Numerics;

namespace FishUI.Controls
{
	/// <summary>
	/// Mode for the file picker dialog.
	/// </summary>
	public enum FilePickerMode
	{
		/// <summary>
		/// Open an existing file.
		/// </summary>
		Open,

		/// <summary>
		/// Save to a file (new or existing).
		/// </summary>
		Save
	}

	/// <summary>
	/// A file picker dialog for selecting files to open or save.
	/// Uses FishUI controls for a consistent look and dependency-free implementation.
	/// </summary>
	public class FilePickerDialog : Window
	{
		/// <summary>
		/// Event raised when a file is selected and OK is clicked.
		/// </summary>
		public event Action<FilePickerDialog, string> OnFileConfirmed;

		/// <summary>
		/// Event raised when the dialog is cancelled.
		/// </summary>
		public event Action<FilePickerDialog> OnDialogCancelled;

		/// <summary>
		/// Gets or sets the current directory being browsed.
		/// </summary>
		public string CurrentDirectory
		{
			get => _currentDirectory;
			set
			{
				_currentDirectory = value;
				RefreshFileList();
				if (_pathTextbox != null)
					_pathTextbox.Text = value;
			}
		}
		private string _currentDirectory;

		/// <summary>
		/// Gets or sets the selected file name (without path).
		/// </summary>
		public string FileName
		{
			get => _fileNameTextbox?.Text ?? "";
			set
			{
				if (_fileNameTextbox != null)
					_fileNameTextbox.Text = value;
			}
		}

		/// <summary>
		/// Gets the full path of the selected file.
		/// </summary>
		public string SelectedPath => _fileSystem?.CombinePath(CurrentDirectory, FileName) ?? "";

		/// <summary>
		/// Gets or sets the file filter pattern (e.g., "*.yaml", "*.txt").
		/// </summary>
		public string Filter { get; set; } = "*";

		/// <summary>
		/// Gets the dialog mode (Open or Save).
		/// </summary>
		public FilePickerMode Mode { get; private set; }

		private ListBox _fileListBox;
		private Textbox _pathTextbox;
		private Textbox _fileNameTextbox;
		private Button _okButton;
		private Button _cancelButton;
		private Button _upButton;
		private Button _goButton;
		private Label _filterLabel;
		private IFishUIFileSystem _fileSystem;

		/// <summary>
		/// Creates a new file picker dialog.
		/// </summary>
		/// <param name="mode">The dialog mode (Open or Save).</param>
		/// <param name="fileSystem">The file system interface to use.</param>
		/// <param name="initialDirectory">The initial directory to browse.</param>
		/// <param name="filter">The file filter pattern (e.g., "*.yaml").</param>
		public FilePickerDialog(FilePickerMode mode, IFishUIFileSystem fileSystem, string initialDirectory = null, string filter = "*")
			: base()
		{
			Mode = mode;
			_fileSystem = fileSystem ?? new DefaultFishUIFileSystem();
			Filter = filter;

			Title = mode == FilePickerMode.Open ? "Open File" : "Save File";
			Size = new Vector2(500, 400);
			UpdateInternalSizes(); // Ensure content panel is sized correctly before adding children
			IsResizable = true;
			ShowCloseButton = true;
			MinSize = new Vector2(400, 300);

			OnClosed += (w) => OnDialogCancelled?.Invoke(this);

			CreateControls();

			// Set initial directory
			if (string.IsNullOrEmpty(initialDirectory))
			{
				initialDirectory = _fileSystem.GetFullPath(".");
			}
			CurrentDirectory = initialDirectory;
		}

		private void CreateControls()
		{
			// Get content area size (excludes titlebar and borders)
			Vector2 contentSize = GetContentSize();

			float padding = 8;
			float buttonWidth = 80;
			float buttonHeight = 28;
			float textboxHeight = 26;
			float labelHeight = 20;

			// Path label and textbox
			Label pathLabel = new Label("Location:");
			pathLabel.Position = new Vector2(padding, padding);
			pathLabel.Size = new Vector2(60, labelHeight);
			AddChild(pathLabel);

			_upButton = new Button();
			_upButton.Text = "";
			_upButton.IconPath = "data/silk_icons/arrow_up.png";
			_upButton.Position = new Vector2(padding + 65, padding - 2);
			_upButton.Size = new Vector2(28, textboxHeight);
			_upButton.TooltipText = "Go to parent directory";
			_upButton.OnButtonPressed += (s, b, p) => NavigateUp();
			AddChild(_upButton);

			_pathTextbox = new Textbox();
			_pathTextbox.Position = new Vector2(padding + 98, padding - 2);
			_pathTextbox.Size = new Vector2(contentSize.X - padding * 2 - 98 - 50, textboxHeight);
			_pathTextbox.Anchor = FishUIAnchor.Top | FishUIAnchor.Left | FishUIAnchor.Right;
			AddChild(_pathTextbox);

			_goButton = new Button();
			_goButton.Text = "Go";
			_goButton.Position = new Vector2(contentSize.X - padding - 45, padding - 2);
			_goButton.Size = new Vector2(40, textboxHeight);
			_goButton.Anchor = FishUIAnchor.Top | FishUIAnchor.Right;
			_goButton.OnButtonPressed += (s, b, p) => NavigateToPath(_pathTextbox.Text);
			AddChild(_goButton);

			// File list
			_fileListBox = new ListBox();
			_fileListBox.Position = new Vector2(padding, padding + textboxHeight + 10);
			_fileListBox.Size = new Vector2(contentSize.X - padding * 2, contentSize.Y - 130);
			_fileListBox.Anchor = FishUIAnchor.All;
			_fileListBox.OnItemSelected += HandleFileSelected;
			AddChild(_fileListBox);

			// File name label and textbox
			float bottomY = contentSize.Y - 70;
			Label fileLabel = new Label("File name:");
			fileLabel.Position = new Vector2(padding, bottomY);
			fileLabel.Size = new Vector2(70, labelHeight);
			fileLabel.Anchor = FishUIAnchor.Bottom | FishUIAnchor.Left;
			AddChild(fileLabel);

			_fileNameTextbox = new Textbox();
			_fileNameTextbox.Position = new Vector2(padding + 75, bottomY - 3);
			_fileNameTextbox.Size = new Vector2(contentSize.X - padding * 2 - 75, textboxHeight);
			_fileNameTextbox.Anchor = FishUIAnchor.Bottom | FishUIAnchor.Left | FishUIAnchor.Right;
			AddChild(_fileNameTextbox);

			// Filter label
			_filterLabel = new Label($"Filter: {Filter}");
			_filterLabel.Position = new Vector2(padding, bottomY + 30);
			_filterLabel.Size = new Vector2(150, labelHeight);
			_filterLabel.Anchor = FishUIAnchor.Bottom | FishUIAnchor.Left;
			AddChild(_filterLabel);

			// OK and Cancel buttons
			float buttonsY = bottomY + 28;
			_cancelButton = new Button();
			_cancelButton.Text = "Cancel";
			_cancelButton.Position = new Vector2(contentSize.X - padding - buttonWidth, buttonsY);
			_cancelButton.Size = new Vector2(buttonWidth, buttonHeight);
			_cancelButton.Anchor = FishUIAnchor.Bottom | FishUIAnchor.Right;
			_cancelButton.OnButtonPressed += (s, b, p) => Cancel();
			AddChild(_cancelButton);

			_okButton = new Button();
			_okButton.Text = Mode == FilePickerMode.Open ? "Open" : "Save";
			_okButton.Position = new Vector2(contentSize.X - padding - buttonWidth * 2 - 10, buttonsY);
			_okButton.Size = new Vector2(buttonWidth, buttonHeight);
			_okButton.Anchor = FishUIAnchor.Bottom | FishUIAnchor.Right;
			_okButton.OnButtonPressed += (s, b, p) => Confirm();
			AddChild(_okButton);
		}

		private void RefreshFileList()
		{
			if (_fileListBox == null || _fileSystem == null)
				return;

			_fileListBox.Items.Clear();

			// Add directories
			var directories = _fileSystem.GetDirectories(_currentDirectory);
			foreach (var dir in directories)
			{
				string name = _fileSystem.GetFileName(dir);
				if (!string.IsNullOrEmpty(name))
				{
					var item = new ListBoxItem($"[DIR] {name}");
					item.UserData = dir;
					_fileListBox.AddItem(item);
				}
			}

			// Add files matching filter
			var files = _fileSystem.GetFiles(_currentDirectory, Filter);
			foreach (var file in files)
			{
				string name = _fileSystem.GetFileName(file);
				if (!string.IsNullOrEmpty(name))
				{
					var item = new ListBoxItem(name);
					item.UserData = file;
					_fileListBox.AddItem(item);
				}
			}
		}

		private void HandleFileSelected(ListBox listBox, int index, ListBoxItem item)
		{
			if (item?.UserData is string path)
			{
				if (_fileSystem.IsDirectory(path))
				{
					// Double-click behavior: navigate into directory
					// For now, single click on directory navigates
					CurrentDirectory = path;
				}
				else
				{
					FileName = _fileSystem.GetFileName(path);
				}
			}
		}

		private void NavigateUp()
		{
			var parent = _fileSystem.GetParentDirectory(_currentDirectory);
			if (!string.IsNullOrEmpty(parent))
			{
				CurrentDirectory = parent;
			}
		}

		private void NavigateToPath(string path)
		{
			if (_fileSystem.IsDirectory(path))
			{
				CurrentDirectory = path;
			}
		}

		private void Confirm()
		{
			string fileName = FileName?.Trim();
			if (string.IsNullOrEmpty(fileName))
				return;

			string fullPath = _fileSystem.CombinePath(_currentDirectory, fileName);

			// For Open mode, check if file exists
			if (Mode == FilePickerMode.Open && !_fileSystem.Exists(fullPath))
			{
				// Could show an error message, for now just return
				return;
			}

			Visible = false;
			OnFileConfirmed?.Invoke(this, fullPath);
		}

		private void Cancel()
		{
			Visible = false;
			OnDialogCancelled?.Invoke(this);
		}

		/// <summary>
		/// Shows the dialog centered on screen.
		/// </summary>
		/// <param name="ui">The FishUI instance.</param>
		public void Show(FishUI ui)
		{
			// Get screen dimensions from graphics backend
			int screenWidth = ui.Graphics.GetWindowWidth();
			int screenHeight = ui.Graphics.GetWindowHeight();

			// Center on screen
			Position = new Vector2(
				(screenWidth - Size.X) / 2,
				(screenHeight - Size.Y) / 2
			);
			Visible = true;
			IsActive = true;

			ui.AddControl(this);
		}
	}
}
