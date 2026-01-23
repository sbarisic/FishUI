using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	/// <summary>
	/// Panel visual variant types matching GWEN theme variants.
	/// </summary>
	public enum PanelVariant
	{
		Normal,
		Bright,
		Dark,
		Highlight
	}

	/// <summary>
	/// Border style options for panels.
	/// </summary>
	public enum BorderStyle
	{
		None,
		Solid,
		Inset,
		Outset
	}

	public class Panel : Control
	{
		/// <summary>
		/// Whether the panel background is transparent (not drawn).
		/// </summary>
		[YamlMember]
		public bool IsTransparent { get; set; } = false;

		/// <summary>
		/// Visual variant of the panel (Normal, Bright, Dark, Highlight).
		/// </summary>
		[YamlMember]
		public PanelVariant Variant { get; set; } = PanelVariant.Normal;

		/// <summary>
		/// Border style for the panel.
		/// </summary>
		[YamlMember]
		public BorderStyle BorderStyle { get; set; } = BorderStyle.None;

		/// <summary>
		/// Border color when BorderStyle is not None.
		/// </summary>
		[YamlMember]
		public FishColor BorderColor { get; set; } = new FishColor(180, 180, 180, 255);

		/// <summary>
		/// Border thickness in pixels.
		/// </summary>
		[YamlMember]
		public float BorderThickness { get; set; } = 1f;

		public Panel()
		{
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			//base.Draw(UI, Dt, Time);

			if (IsTransparent)
			{
				// Still draw border if requested even when transparent
				DrawBorder(UI);
				return;
			}

			// Select panel image based on variant
			NPatch Cur = Variant switch
			{
				PanelVariant.Bright => UI.Settings.ImgPanelBright ?? UI.Settings.ImgPanel,
				PanelVariant.Dark => UI.Settings.ImgPanelDark ?? UI.Settings.ImgPanel,
				PanelVariant.Highlight => UI.Settings.ImgPanelHighlight ?? UI.Settings.ImgPanel,
				_ => UI.Settings.ImgPanel
			};

			if (Disabled)
				Cur = UI.Settings.ImgPanelDisabled;

		UI.Graphics.DrawNPatch(Cur, GetAbsolutePosition(), GetAbsoluteSize(), EffectiveColor);

			DrawBorder(UI);

			//DrawChildren(UI, Dt, Time);
		}

		/// <summary>
		/// Draws the panel border based on BorderStyle.
		/// </summary>
		private void DrawBorder(FishUI UI)
		{
			if (BorderStyle == BorderStyle.None || BorderThickness <= 0)
				return;

			Vector2 pos = GetAbsolutePosition();
			Vector2 size = GetAbsoluteSize();

			FishColor topLeft = BorderColor;
			FishColor bottomRight = BorderColor;

			// Adjust colors for inset/outset 3D effect
			if (BorderStyle == BorderStyle.Inset)
			{
				topLeft = new FishColor(
					(byte)Math.Max(0, BorderColor.R - 60),
					(byte)Math.Max(0, BorderColor.G - 60),
					(byte)Math.Max(0, BorderColor.B - 60),
					BorderColor.A);
				bottomRight = new FishColor(
					(byte)Math.Min(255, BorderColor.R + 60),
					(byte)Math.Min(255, BorderColor.G + 60),
					(byte)Math.Min(255, BorderColor.B + 60),
					BorderColor.A);
			}
			else if (BorderStyle == BorderStyle.Outset)
			{
				topLeft = new FishColor(
					(byte)Math.Min(255, BorderColor.R + 60),
					(byte)Math.Min(255, BorderColor.G + 60),
					(byte)Math.Min(255, BorderColor.B + 60),
					BorderColor.A);
				bottomRight = new FishColor(
					(byte)Math.Max(0, BorderColor.R - 60),
					(byte)Math.Max(0, BorderColor.G - 60),
					(byte)Math.Max(0, BorderColor.B - 60),
					BorderColor.A);
			}

			// Draw top border
			UI.Graphics.DrawLine(pos, new Vector2(pos.X + size.X, pos.Y), BorderThickness, topLeft);
			// Draw left border
			UI.Graphics.DrawLine(pos, new Vector2(pos.X, pos.Y + size.Y), BorderThickness, topLeft);
			// Draw bottom border
			UI.Graphics.DrawLine(new Vector2(pos.X, pos.Y + size.Y), new Vector2(pos.X + size.X, pos.Y + size.Y), BorderThickness, bottomRight);
			// Draw right border
			UI.Graphics.DrawLine(new Vector2(pos.X + size.X, pos.Y), new Vector2(pos.X + size.X, pos.Y + size.Y), BorderThickness, bottomRight);
		}
	}
}
