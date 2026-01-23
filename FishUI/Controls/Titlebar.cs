using System;
using System.Numerics;

namespace FishUI.Controls
{
	/// <summary>
	/// A standalone titlebar component that can be used for windows and dialogs.
	/// Displays a title text and can optionally show a close button.
	/// </summary>
	public class Titlebar : Control
	{
		/// <summary>
		/// The text displayed in the titlebar.
		/// </summary>
		public string Title { get; set; } = "Window";

		/// <summary>
		/// Whether the titlebar shows as active (focused) or inactive.
		/// </summary>
		public bool IsActive { get; set; } = true;

		/// <summary>
		/// Whether to show the close button.
		/// </summary>
		public bool ShowCloseButton { get; set; } = true;

		/// <summary>
		/// Event raised when the close button is clicked.
		/// </summary>
		public event Action<Titlebar> OnCloseClicked;

		/// <summary>
		/// Event raised when the titlebar is dragged.
		/// </summary>
		public event Action<Titlebar, Vector2> OnTitlebarDragged;

		private bool _closeButtonHovered = false;
		private bool _closeButtonPressed = false;
		private const int CloseButtonSize = 24;
		private const int CloseButtonMargin = 2;

		public Titlebar()
		{
			Draggable = true;
			Size = new Vector2(200, 24);
		}

		public Titlebar(string title) : this()
		{
			Title = title;
		}

		private Vector2 GetCloseButtonPosition()
		{
			Vector2 absPos = GetAbsolutePosition();
			Vector2 absSize = GetAbsoluteSize();
			return new Vector2(absPos.X + absSize.X - CloseButtonSize - CloseButtonMargin, absPos.Y + (absSize.Y - CloseButtonSize) / 2);
		}

		private bool IsPointInCloseButton(Vector2 point)
		{
			if (!ShowCloseButton)
				return false;

			Vector2 closePos = GetCloseButtonPosition();
			return Utils.IsInside(closePos, new Vector2(CloseButtonSize, CloseButtonSize), point);
		}

		public override void HandleMouseMove(FishUI UI, FishInputState InState, Vector2 Pos)
		{
			base.HandleMouseMove(UI, InState, Pos);
			_closeButtonHovered = IsPointInCloseButton(Pos);
		}

		public override void HandleMouseLeave(FishUI UI, FishInputState InState)
		{
			base.HandleMouseLeave(UI, InState);
			_closeButtonHovered = false;
			_closeButtonPressed = false;
		}

		public override void HandleMousePress(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMousePress(UI, InState, Btn, Pos);
			if (Btn == FishMouseButton.Left && IsPointInCloseButton(Pos))
			{
				_closeButtonPressed = true;
			}
		}

		public override void HandleMouseRelease(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMouseRelease(UI, InState, Btn, Pos);
			_closeButtonPressed = false;
		}

		public override void HandleMouseClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMouseClick(UI, InState, Btn, Pos);

			if (Btn == FishMouseButton.Left && IsPointInCloseButton(Pos))
			{
				OnCloseClicked?.Invoke(this);
			}
		}

		public override void HandleDrag(FishUI UI, Vector2 StartPos, Vector2 EndPos, FishInputState InState)
		{
			// Don't drag if clicking the close button
			if (IsPointInCloseButton(StartPos))
				return;

			// Instead of moving the titlebar itself, invoke the drag event
			OnTitlebarDragged?.Invoke(this, InState.MouseDelta);

			// If no handler is attached, use default draggable behavior
			if (OnTitlebarDragged == null && Draggable)
			{
				Position += InState.MouseDelta;
			}
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			Vector2 absPos = GetAbsolutePosition();
			Vector2 absSize = GetAbsoluteSize();

			// Draw titlebar background
			NPatch titlebarImg = IsActive 
				? UI.Settings.ImgWindowHeadNormal 
				: UI.Settings.ImgWindowHeadInactive;

			if (titlebarImg != null)
			{
				UI.Graphics.DrawNPatch(titlebarImg, absPos, absSize, Color);
			}
			else
			{
				// Fallback: draw a simple rectangle
				UI.Graphics.DrawRectangle(absPos, absSize, IsActive ? new FishColor(60, 60, 60) : new FishColor(80, 80, 80));
			}

			// Draw title text
			if (!string.IsNullOrEmpty(Title))
			{
				Vector2 textSize = UI.Graphics.MeasureText(UI.Settings.FontDefault, Title);
				float textX = absPos.X + 8;
				float textY = absPos.Y + (absSize.Y - textSize.Y) / 2;
				UI.Graphics.DrawText(UI.Settings.FontDefault, Title, new Vector2(textX, textY));
			}

			// Draw close button
			if (ShowCloseButton)
			{
				Vector2 closePos = GetCloseButtonPosition();
				NPatch closeImg;

				if (Disabled)
					closeImg = UI.Settings.ImgWindowCloseDisabled;
				else if (_closeButtonPressed)
					closeImg = UI.Settings.ImgWindowClosePressed;
				else if (_closeButtonHovered)
					closeImg = UI.Settings.ImgWindowCloseHover;
				else
					closeImg = UI.Settings.ImgWindowCloseNormal;

				if (closeImg != null)
				{
					UI.Graphics.DrawNPatch(closeImg, closePos, new Vector2(CloseButtonSize, CloseButtonSize), Color);
				}
				else
				{
					// Fallback: draw a simple X
					FishColor xColor = _closeButtonHovered ? new FishColor(255, 100, 100) : new FishColor(200, 200, 200);
					UI.Graphics.DrawRectangle(closePos, new Vector2(CloseButtonSize, CloseButtonSize), xColor);
				}
			}
		}
	}
}
