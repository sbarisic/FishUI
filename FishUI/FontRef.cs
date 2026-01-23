using System;
using System.Collections.Generic;
using System.Text;

namespace FishUI
{
	/// <summary>
	/// Font style options.
	/// </summary>
	[Flags]
	public enum FontStyle
	{
		/// <summary>Regular font style.</summary>
		Regular = 0,
		/// <summary>Bold font style.</summary>
		Bold = 1,
		/// <summary>Italic font style.</summary>
		Italic = 2,
		/// <summary>Bold and Italic combined.</summary>
		BoldItalic = Bold | Italic
	}

	/// <summary>
	/// Reference to a loaded font with size, spacing, and style information.
	/// </summary>
	public class FontRef
	{
		/// <summary>
		/// Path to the font file.
		/// </summary>
		public string Path;

		/// <summary>
		/// Backend-specific font data.
		/// </summary>
		public object Userdata;

		/// <summary>
		/// Font size in pixels.
		/// </summary>
		public float Size;

		/// <summary>
		/// Character spacing in pixels.
		/// </summary>
		public float Spacing;

		/// <summary>
		/// Default text color for this font.
		/// </summary>
		public FishColor Color;

		/// <summary>
		/// Font style (Regular, Bold, Italic, BoldItalic).
		/// </summary>
		public FontStyle Style { get; set; } = FontStyle.Regular;

		/// <summary>
		/// Whether this is a monospaced (fixed-width) font.
		/// </summary>
		public bool IsMonospaced { get; set; } = false;

		/// <summary>
		/// Line height for this font in pixels.
		/// Set by the graphics backend when loading the font.
		/// </summary>
		public float LineHeight { get; set; }

		/// <summary>
		/// Gets whether this font is bold.
		/// </summary>
		public bool IsBold => (Style & FontStyle.Bold) != 0;

		/// <summary>
		/// Gets whether this font is italic.
		/// </summary>
		public bool IsItalic => (Style & FontStyle.Italic) != 0;
	}
}
