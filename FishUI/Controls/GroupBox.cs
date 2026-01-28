using System;
using System.Numerics;

namespace FishUI.Controls
{
	/// <summary>
	/// A labeled container control that groups related controls with a border and title.
	/// </summary>
	public class GroupBox : Control
	{
		/// <summary>
		/// The text displayed as the group box title.
		/// </summary>
		public string Text { get; set; } = "Group";

		/// <summary>
		/// Padding inside the group box content area.
		/// </summary>
		public float ContentPadding { get; set; } = 8;

		/// <summary>
		/// Height of the title area.
		/// </summary>
		public float TitleHeight { get; set; } = 16;

		/// <summary>
		/// Horizontal offset for the title text from the left edge.
		/// </summary>
		public float TitleOffset { get; set; } = 10;

		/// <summary>
		/// The color of the border.
		/// </summary>
		public FishColor BorderColor { get; set; } = new FishColor(180, 180, 180);

		public GroupBox()
		{
			Size = new Vector2(200, 150);
		}

		public GroupBox(string text) : this()
		{
			Text = text;
		}

		/// <summary>
		/// Gets the position of the content area (accounting for title and padding).
		/// </summary>
		public Vector2 GetContentPosition()
		{
			return GetAbsolutePosition() + new Vector2(ContentPadding, TitleHeight + ContentPadding);
		}

		/// <summary>
		/// Gets the size of the content area.
		/// </summary>
		public Vector2 GetContentSize()
		{
			return new Vector2(
				Size.X - ContentPadding * 2,
				Size.Y - TitleHeight - ContentPadding * 2
			);
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			Vector2 absPos = GetAbsolutePosition();
			Vector2 absSize = GetAbsoluteSize();

			NPatch borderImg = UI.Settings.ImgGroupBoxNormal;

			// Calculate title text size for gap in border
			Vector2 textSize = Vector2.Zero;
			if (!string.IsNullOrEmpty(Text) && UI.Settings.FontDefault != null)
			{
				textSize = UI.Graphics.MeasureText(UI.Settings.FontDefault, Text);
			}

			// The border should start slightly below the top to leave room for title
			float borderTopY = absPos.Y + TitleHeight / 2;
			Vector2 borderPos = new Vector2(absPos.X, borderTopY);
			Vector2 borderSize = new Vector2(absSize.X, absSize.Y - TitleHeight / 2);

			if (borderImg != null)
			{
				UI.Graphics.DrawNPatch(borderImg, borderPos, borderSize, Color);
			}
			else
			{
				// Fallback: draw a simple border rectangle
				// Draw bottom, left, right borders
				UI.Graphics.DrawRectangleOutline(borderPos, borderSize, BorderColor);
			}

			// Draw the title text with a background to "cut" the border
			if (!string.IsNullOrEmpty(Text))
			{
				float textX = absPos.X + TitleOffset;
				float textY = absPos.Y;

				// Draw background behind text to hide the border
				float bgPadding = 4;
				Vector2 bgPos = new Vector2(textX - bgPadding, textY);
				Vector2 bgSize = new Vector2(textSize.X + bgPadding * 2, textSize.Y);

				// Use parent or control background color
				FishColor bgColor = new FishColor(240, 240, 240); // Default light gray
				UI.Graphics.DrawRectangle(bgPos, bgSize, bgColor);

				// Draw the text
				UI.Graphics.DrawText(UI.Settings.FontDefault, Text, new Vector2(textX, textY));
			}
		}

		/// <summary>
		/// Draws the group box in editor mode with container area visualization.
		/// </summary>
		public override void DrawControlEditor(FishUI UI, float Dt, float Time, Vector2 canvasOffset)
		{
			// Draw normal control
			DrawControl(UI, Dt, Time);

			// Draw content area outline to show where children can be placed
			Vector2 contentPos = GetContentPosition();
			Vector2 contentSize = GetContentSize();
			FishColor containerColor = new FishColor(100, 150, 255, 150);
			UI.Graphics.DrawRectangleOutline(contentPos, contentSize, containerColor);

			// Draw anchor visualization
			DrawAnchorVisualization(UI);
		}
	}
}
