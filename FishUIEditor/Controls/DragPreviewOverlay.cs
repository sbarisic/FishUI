using FishUI;
using FishUI.Controls;
using System.Numerics;

namespace FishUIEditor.Controls
{
	/// <summary>
	/// A simple overlay control that displays a drag preview for toolbox items.
	/// </summary>
	public class DragPreviewOverlay : Control
	{
		/// <summary>
		/// The name of the control type being dragged.
		/// </summary>
		public string ControlTypeName { get; set; }

		public DragPreviewOverlay()
		{
			Size = new Vector2(100, 28);
		}

		public override void DrawControl(FishUI.FishUI UI, float Dt, float Time)
		{
			if (!Visible || string.IsNullOrEmpty(ControlTypeName))
				return;

			Vector2 pos = GetAbsolutePosition();
			Vector2 size = Size;

			// Draw semi-transparent background
			UI.Graphics.DrawRectangle(
				pos,
				size,
				new FishColor(100, 150, 200, 128));

			// Draw outline
			UI.Graphics.DrawRectangleOutline(
				pos,
				size,
				new FishColor(0, 120, 215, 255));

			// Draw control type label
			UI.Graphics.DrawTextColor(
				UI.Settings.FontDefault,
				ControlTypeName,
				pos + new Vector2(4, 4),
				new FishColor(255, 255, 255, 255));
		}

		public override bool IsPointInside(Vector2 Pos)
		{
			// Don't intercept any mouse input - this is just a visual overlay
			return false;
		}
	}
}
