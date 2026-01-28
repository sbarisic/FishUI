using System;
using System.Numerics;

namespace FishUI.Controls
{
	/// <summary>
	/// Event arguments for window close events.
	/// </summary>
	public class WindowCloseEventArgs : EventArgs
	{
		/// <summary>
		/// Set to true to cancel the close operation.
		/// </summary>
		public bool Cancel { get; set; } = false;
	}

	/// <summary>
	/// A window/dialog control with a titlebar, body, and optional close button.
	/// Supports dragging, modal mode, and resizing.
	/// </summary>
	public class Window : Control
	{
		/// <summary>
		/// The title displayed in the window titlebar.
		/// </summary>
		public string Title
		{
			get => _titlebar?.Title ?? _title;
			set
			{
				_title = value;
				if (_titlebar != null)
					_titlebar.Title = value;
			}
		}
		private string _title = "Window";

		/// <summary>
		/// Whether the window is currently active (focused).
		/// </summary>
		public bool IsActive
		{
			get => _isActive;
			set
			{
				_isActive = value;
				if (_titlebar != null)
					_titlebar.IsActive = value;
			}
		}
		private bool _isActive = true;

		/// <summary>
		/// Whether to show the close button in the titlebar.
		/// </summary>
		public bool ShowCloseButton
		{
			get => _titlebar?.ShowCloseButton ?? _showCloseButton;
			set
			{
				_showCloseButton = value;
				if (_titlebar != null)
					_titlebar.ShowCloseButton = value;
			}
		}
		private bool _showCloseButton = true;

		/// <summary>
		/// Whether the close button is enabled (clickable).
		/// When false, the close button is visible but non-interactive (grayed out).
		/// Note: Setting IsModal to true will automatically set this to false.
		/// </summary>
		public bool CloseButtonEnabled
		{
			get => _titlebar?.CloseButtonEnabled ?? _closeButtonEnabled;
			set
			{
				_closeButtonEnabled = value;
				if (_titlebar != null)
					_titlebar.CloseButtonEnabled = value;
			}
		}
		private bool _closeButtonEnabled = true;

		/// <summary>
		/// Whether this window is modal (blocks input to other controls).
		/// When set to true, CloseButtonEnabled is automatically set to false.
		/// </summary>
		public bool IsModal
		{
			get => _isModal;
			set
			{
				_isModal = value;
				if (value)
				{
					CloseButtonEnabled = false;
				}
			}
		}
		private bool _isModal = false;

		/// <summary>
		/// Whether this window can be resized by dragging its edges.
		/// </summary>
		public bool IsResizable { get; set; } = true;

		/// <summary>
		/// Whether to draw a shadow behind the window.
		/// </summary>
		public bool ShowShadow { get; set; } = true;

		/// <summary>
		/// Offset of the shadow from the window (in pixels).
		/// </summary>
		public Vector2 ShadowOffset { get; set; } = new Vector2(8, 8);

		/// <summary>
		/// Extra size added to the shadow beyond the window bounds (in pixels).
		/// </summary>
		public float ShadowSize { get; set; } = 8;

		/// <summary>
		/// Minimum size the window can be resized to.
		/// </summary>
		public override Vector2 MinSize { get; set; } = new Vector2(100, 60);

		/// <summary>
		/// Maximum size the window can be resized to. Set to Vector2.Zero for no limit.
		/// </summary>
		public override Vector2 MaxSize { get; set; } = Vector2.Zero;

		/// <summary>
		/// Height of the titlebar in pixels.
		/// </summary>
		public float TitlebarHeight { get; set; } = 24;

		/// <summary>
		/// Height of the bottom border in pixels.
		/// </summary>
		public float BottomBorderHeight { get; set; } = 4;

		/// <summary>
		/// Width of the side borders in pixels.
		/// </summary>
		public float SideBorderWidth { get; set; } = 4;

		/// <summary>
		/// The resize handle size in pixels (for edge detection).
		/// </summary>
		public float ResizeHandleSize { get; set; } = 6;

		/// <summary>
		/// Event raised when the window is about to close. Can be cancelled.
		/// </summary>
		public event EventHandler<WindowCloseEventArgs> OnClosing;

		/// <summary>
		/// Event raised after the window has closed.
		/// </summary>
		public event Action<Window> OnClosed;

		/// <summary>
		/// Event raised when the window is resized.
		/// </summary>
		public event Action<Window, Vector2> OnResized;

		private Titlebar _titlebar;
		private Panel _contentPanel;
		private ResizeDirection _resizeDirection = ResizeDirection.None;
		private bool _isResizing = false;

		[Flags]
		private enum ResizeDirection
		{
			None = 0,
			Left = 1,
			Right = 2,
			Top = 4,
			Bottom = 8,
			TopLeft = Top | Left,
			TopRight = Top | Right,
			BottomLeft = Bottom | Left,
			BottomRight = Bottom | Right
		}

		public Window()
		{
			Size = new Vector2(300, 200);
			Draggable = false; // Dragging is handled by the titlebar
			CreateInternalControls();
		}

		public Window(string title) : this()
		{
			Title = title;
		}

		public Window(string title, Vector2 size) : this(title)
		{
			Size = size;
			UpdateInternalSizes();
		}

		private void CreateInternalControls()
		{
			// Create titlebar
			_titlebar = new Titlebar(_title)
			{
				Position = new Vector2(0, 0),
				Size = new Vector2(Size.X, TitlebarHeight),
				ShowCloseButton = _showCloseButton,
				CloseButtonEnabled = _closeButtonEnabled,
				IsActive = _isActive
			};

			_titlebar.OnCloseClicked += (tb) => Close();
			_titlebar.OnTitlebarDragged += (tb, delta) =>
			{
				Position += delta;
			};

			base.AddChild(_titlebar);

			// Create content panel for child controls
			// Use ResizeHandleSize for margins so resize handles are outside client area
			float sideMargin = Math.Max(SideBorderWidth, ResizeHandleSize);
			float bottomMargin = Math.Max(BottomBorderHeight, ResizeHandleSize);
			_contentPanel = new Panel
			{
				Position = new Vector2(sideMargin, TitlebarHeight),
				Size = new Vector2(Size.X - sideMargin * 2, Size.Y - TitlebarHeight - bottomMargin),
				IsTransparent = true
			};

			base.AddChild(_contentPanel);
		}


		/// <summary>
		/// Updates the internal titlebar and content panel sizes based on the current window size.
		/// Call this after changing the window size to ensure internal controls are properly sized.
		/// </summary>
		protected void UpdateInternalSizes()
		{
			if (_titlebar != null)
				_titlebar.Size = new Vector2(Size.X, TitlebarHeight);

			if (_contentPanel != null)
			{
				// Use ResizeHandleSize for margins so resize handles are outside client area
				float sideMargin = Math.Max(SideBorderWidth, ResizeHandleSize);
				float bottomMargin = Math.Max(BottomBorderHeight, ResizeHandleSize);
				_contentPanel.Position = new Vector2(sideMargin, TitlebarHeight);
				_contentPanel.Size = new Vector2(Size.X - sideMargin * 2, Size.Y - TitlebarHeight - bottomMargin);
			}
		}

		/// <summary>
		/// Adds a child control to the window's content area.
		/// </summary>
		public new void AddChild(Control Child)
		{
			// Ensure content panel has correct size before adding child
			// This is needed when Window.Size is set after construction but before AddChild
			UpdateInternalSizes();
			_contentPanel.AddChild(Child);
		}

		/// <summary>
		/// Removes a child control from the window's content area.
		/// </summary>
		public new void RemoveChild(Control Child)
		{
			_contentPanel.RemoveChild(Child);
		}

		/// <summary>
		/// Gets the content area of the window (excluding titlebar and resize handles).
		/// </summary>
		public Vector2 GetContentPosition()
		{
			float sideMargin = Math.Max(SideBorderWidth, ResizeHandleSize);
			return GetAbsolutePosition() + new Vector2(sideMargin, TitlebarHeight);
		}

		/// <summary>
		/// Gets the size of the content area.
		/// </summary>
		public Vector2 GetContentSize()
		{
			float sideMargin = Math.Max(SideBorderWidth, ResizeHandleSize);
			float bottomMargin = Math.Max(BottomBorderHeight, ResizeHandleSize);
			return new Vector2(Size.X - sideMargin * 2, Size.Y - TitlebarHeight - bottomMargin);
		}

		/// <summary>
		/// Attempts to close the window. The OnClosing event can cancel the close operation.
		/// </summary>
		public void Close()
		{
			var args = new WindowCloseEventArgs();
			OnClosing?.Invoke(this, args);

			if (!args.Cancel)
			{
				Visible = false;
				OnClosed?.Invoke(this);

				// Clear modal if this was the modal window
				if (FishUI?.ModalControl == this)
					FishUI.SetModalControl(null);
			}
		}

		/// <summary>
		/// Shows the window and brings it to front.
		/// If IsModal is true, this window will block input to other controls.
		/// </summary>
		public void Show()
		{
			Visible = true;
			IsActive = true;
			BringToFront();

			// Set as modal if needed
			if (IsModal && FishUI != null)
			{
				FishUI.SetModalControl(this);
			}
		}

		/// <summary>
		/// Shows this window as a modal dialog, blocking input to other controls.
		/// </summary>
		public void ShowModal()
		{
			IsModal = true;
			Show();
		}

		/// <summary>
		/// Centers the window on the screen.
		/// </summary>
		public void CenterOnScreen()
		{
			if (FishUI != null)
			{
				Position.X = (FishUI.Width - Size.X) / 2;
				Position.Y = (FishUI.Height - Size.Y) / 2;
			}
		}

		private ResizeDirection GetResizeDirection(Vector2 point)
		{
			if (!IsResizable)
				return ResizeDirection.None;

			Vector2 absPos = GetAbsolutePosition();
			Vector2 absSize = GetAbsoluteSize();

			bool left = point.X >= absPos.X && point.X < absPos.X + ResizeHandleSize;
			bool right = point.X > absPos.X + absSize.X - ResizeHandleSize && point.X <= absPos.X + absSize.X;
			bool top = point.Y >= absPos.Y && point.Y < absPos.Y + ResizeHandleSize;
			bool bottom = point.Y > absPos.Y + absSize.Y - ResizeHandleSize && point.Y <= absPos.Y + absSize.Y;

			ResizeDirection dir = ResizeDirection.None;
			if (left) dir |= ResizeDirection.Left;
			if (right) dir |= ResizeDirection.Right;
			if (top) dir |= ResizeDirection.Top;
			if (bottom) dir |= ResizeDirection.Bottom;

			return dir;
		}

		public override void HandleMousePress(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMousePress(UI, InState, Btn, Pos);

			if (Btn == FishMouseButton.Left)
			{
				_resizeDirection = GetResizeDirection(Pos);
				if (_resizeDirection != ResizeDirection.None)
				{
					_isResizing = true;
				}
			}
		}

		public override void HandleMouseRelease(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMouseRelease(UI, InState, Btn, Pos);
			_isResizing = false;
			_resizeDirection = ResizeDirection.None;
		}

		public override void HandleDrag(FishUI UI, Vector2 StartPos, Vector2 EndPos, FishInputState InState)
		{
			if (_isResizing && _resizeDirection != ResizeDirection.None)
			{
				Vector2 delta = InState.MouseDelta;
				Vector2 newSize = Size;
				Vector2 newPos = new Vector2(Position.X, Position.Y);

				if (_resizeDirection.HasFlag(ResizeDirection.Right))
				{
					newSize.X += delta.X;
				}
				if (_resizeDirection.HasFlag(ResizeDirection.Bottom))
				{
					newSize.Y += delta.Y;
				}
				if (_resizeDirection.HasFlag(ResizeDirection.Left))
				{
					newSize.X -= delta.X;
					newPos.X += delta.X;
				}
				if (_resizeDirection.HasFlag(ResizeDirection.Top))
				{
					newSize.Y -= delta.Y;
					newPos.Y += delta.Y;
				}

				// Clamp to min/max size
				if (newSize.X < MinSize.X)
				{
					if (_resizeDirection.HasFlag(ResizeDirection.Left))
						newPos.X -= MinSize.X - newSize.X;
					newSize.X = MinSize.X;
				}
				if (newSize.Y < MinSize.Y)
				{
					if (_resizeDirection.HasFlag(ResizeDirection.Top))
						newPos.Y -= MinSize.Y - newSize.Y;
					newSize.Y = MinSize.Y;
				}

				if (MaxSize.X > 0 && newSize.X > MaxSize.X) newSize.X = MaxSize.X;
				if (MaxSize.Y > 0 && newSize.Y > MaxSize.Y) newSize.Y = MaxSize.Y;

				Size = newSize;
				Position.X = newPos.X;
				Position.Y = newPos.Y;

				// Update internal control sizes
				UpdateInternalSizes();

				OnResized?.Invoke(this, Size);
			}
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			Vector2 absPos = GetAbsolutePosition();
			Vector2 absSize = GetAbsoluteSize();

			// Update internal sizes in case window was resized
			UpdateInternalSizes();
			if (_titlebar != null)
				_titlebar.IsActive = IsActive;

			// Draw shadow behind the window
			if (ShowShadow && UI.Settings.ImgShadow != null)
			{
				Vector2 shadowPos = absPos + ShadowOffset - new Vector2(ShadowSize, ShadowSize);
				Vector2 shadowSize = absSize + new Vector2(ShadowSize * 2, ShadowSize * 2);
				UI.Graphics.DrawNPatch(UI.Settings.ImgShadow, shadowPos, shadowSize, FishColor.White);
			}

			// Draw window body (middle section)
			NPatch middleImg = IsActive
				? UI.Settings.ImgWindowMiddleNormal
				: UI.Settings.ImgWindowMiddleInactive;

			Vector2 bodyPos = new Vector2(absPos.X, absPos.Y + TitlebarHeight);
			Vector2 bodySize = new Vector2(absSize.X, absSize.Y - TitlebarHeight - BottomBorderHeight);

			if (middleImg != null)
			{
				UI.Graphics.DrawNPatch(middleImg, bodyPos, bodySize, Color);
			}
			else
			{
				// Fallback
				UI.Graphics.DrawRectangle(bodyPos, bodySize, new FishColor(240, 240, 240));
			}

			// Draw bottom border
			NPatch bottomImg = IsActive
				? UI.Settings.ImgWindowBottomNormal
				: UI.Settings.ImgWindowBottomInactive;

			Vector2 bottomPos = new Vector2(absPos.X, absPos.Y + absSize.Y - BottomBorderHeight);
			Vector2 bottomSize = new Vector2(absSize.X, BottomBorderHeight);

			if (bottomImg != null)
			{
				UI.Graphics.DrawNPatch(bottomImg, bottomPos, bottomSize, Color);
			}
			else
			{
				// Fallback
				UI.Graphics.DrawRectangle(bottomPos, bottomSize, new FishColor(100, 100, 100));
			}
		}

		/// <summary>
		/// Called after deserialization to reinitialize internal references.
		/// </summary>
		public override void OnDeserialized(FishUI UI)
		{
			// Find the titlebar and content panel in children
			_titlebar = null;
			_contentPanel = null;

			foreach (var child in Children)
			{
				if (child is Titlebar tb && _titlebar == null)
				{
					_titlebar = tb;
					// Rewire event handlers
					_titlebar.OnCloseClicked += (t) => Close();
					_titlebar.OnTitlebarDragged += (t, delta) => { Position += delta; };
				}
				else if (child is Panel p && _contentPanel == null)
				{
					_contentPanel = p;
				}
			}

			// If not found (shouldn't happen), recreate them
			if (_titlebar == null || _contentPanel == null)
			{
				CreateInternalControls();
			}

			// Call base to handle children recursively
			base.OnDeserialized(UI);
		}
	}
}
