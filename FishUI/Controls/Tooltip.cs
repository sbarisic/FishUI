using System;
using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	/// <summary>
	/// A tooltip control that displays text when hovering over a target control.
	/// Tooltips are managed by FishUI and rendered on top of all other controls.
	/// </summary>
	public class Tooltip : Control
	{
		/// <summary>
		/// The text to display in the tooltip.
		/// </summary>
		[YamlMember]
		public string Text { get; set; } = "";

		/// <summary>
		/// Delay in seconds before showing the tooltip after hover starts.
		/// </summary>
		[YamlMember]
		public float ShowDelay { get; set; } = 0.5f;

		/// <summary>
		/// Duration in seconds to show the tooltip (0 = indefinite until mouse leaves).
		/// </summary>
		[YamlMember]
		public float Duration { get; set; } = 0f;

		/// <summary>
		/// Padding around the text inside the tooltip.
		/// </summary>
		[YamlMember]
		public float TextPadding { get; set; } = 6f;

		/// <summary>
		/// Offset from the mouse cursor position.
		/// </summary>
		[YamlMember]
		public Vector2 CursorOffset { get; set; } = new Vector2(12, 16);

		/// <summary>
		/// The control this tooltip is attached to.
		/// </summary>
		[YamlIgnore]
		public Control TargetControl { get; set; }

		/// <summary>
		/// Whether the tooltip is currently visible.
		/// </summary>
		[YamlIgnore]
		public bool IsShowing { get; private set; } = false;

		private float _hoverTime = 0f;
		private float _showTime = 0f;
		private Vector2 _showPosition = Vector2.Zero;

		public Tooltip()
		{
			Visible = false;
			AlwaysOnTop = true;
		}

		public Tooltip(string text) : this()
		{
			Text = text;
		}

		/// <summary>
		/// Call this each frame from the UI system to update tooltip state.
		/// </summary>
		public void UpdateTooltip(FishUI UI, float dt, Vector2 mousePos, Control hoveredControl)
		{
			// Check if we're hovering over the target control
			bool isHoveringTarget = TargetControl != null && 
				(hoveredControl == TargetControl || IsDescendantOf(hoveredControl, TargetControl));

			if (isHoveringTarget)
			{
				_hoverTime += dt;

				if (!IsShowing && _hoverTime >= ShowDelay)
				{
					Show(mousePos);
				}
				else if (IsShowing && Duration > 0)
				{
					_showTime += dt;
					if (_showTime >= Duration)
					{
						Hide();
					}
				}
			}
			else
			{
				if (IsShowing)
				{
					Hide();
				}
				_hoverTime = 0f;
			}

			// Update position to follow mouse while showing
			if (IsShowing)
			{
				UpdatePosition(UI, mousePos);
			}
		}

		private bool IsDescendantOf(Control child, Control parent)
		{
			if (child == null || parent == null)
				return false;

			Control current = child;
			while (current != null)
			{
				if (current == parent)
					return true;
				current = current.GetParent();
			}
			return false;
		}

		/// <summary>
		/// Show the tooltip at the specified position.
		/// </summary>
	public void Show(Vector2 mousePos)
		{
			if (FishUI != null && FishUI.Settings.DebugLogTooltips)
			{
				FishUIDebug.Log($"[Tooltip.Show] Text='{Text}', Pos={mousePos + CursorOffset}");
			}
			IsShowing = true;
			Visible = true;
			_showTime = 0f;
			_showPosition = mousePos + CursorOffset;
			Position = _showPosition;
		}

		/// <summary>
		/// Hide the tooltip.
		/// </summary>
	public void Hide()
		{
			if (FishUI != null && FishUI.Settings.DebugLogTooltips)
			{
				FishUIDebug.Log($"[Tooltip.Hide] Was showing: {IsShowing}");
			}
			IsShowing = false;
			Visible = false;
			_showTime = 0f;
		}

		/// <summary>
		/// Update the tooltip position to follow the mouse cursor.
		/// </summary>
		public void UpdatePosition(FishUI UI, Vector2 mousePos)
		{
		Vector2 targetPos = mousePos + CursorOffset;

			// Clamp to screen bounds
			Vector2 size = GetAbsoluteSize();
			int screenWidth = UI.Width > 0 ? UI.Width : UI.Graphics.GetWindowWidth();
			int screenHeight = UI.Height > 0 ? UI.Height : UI.Graphics.GetWindowHeight();

			if (targetPos.X + size.X > screenWidth)
				targetPos.X = screenWidth - size.X;
			if (targetPos.Y + size.Y > screenHeight)
				targetPos.Y = screenHeight - size.Y;
			if (targetPos.X < 0)
				targetPos.X = 0;
			if (targetPos.Y < 0)
				targetPos.Y = 0;

			Position = targetPos;
		}

	public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			if (UI.Settings.DebugLogTooltips)
			{
				FishUIDebug.Log($"[Tooltip.DrawControl] IsShowing={IsShowing}, Text='{Text}'");
			}
			
			if (!IsShowing || string.IsNullOrEmpty(Text))
			{
				if (UI.Settings.DebugLogTooltips)
				{
					FishUIDebug.Log($"[Tooltip.DrawControl] Early return - IsShowing={IsShowing}, Text empty={string.IsNullOrEmpty(Text)}");
				}
				return;
			}

			Vector2 absPos = GetAbsolutePosition();

			// Measure text to determine tooltip size
			Vector2 textSize = UI.Graphics.MeasureText(UI.Settings.FontDefault, Text);
			Size = new Vector2(textSize.X + TextPadding * 2, textSize.Y + TextPadding * 2);
			Vector2 absSize = GetAbsoluteSize();

			// Draw background
			NPatch bgImg = UI.Settings.ImgTooltipNormal;
			if (bgImg != null)
			{
				UI.Graphics.DrawNPatch(bgImg, absPos, absSize, Color);
			}
			else
			{
			// Fallback: draw a simple rectangle
			UI.Graphics.DrawRectangle(absPos, absSize, new FishColor(40, 40, 40, 230));
			}

			// Draw text
			Vector2 textPos = absPos + new Vector2(TextPadding, TextPadding);
			UI.Graphics.DrawText(UI.Settings.FontDefault, Text, textPos);
		}
	}
}
