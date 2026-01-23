using System;

namespace FishUI
{
	/// <summary>
	/// Font metrics information returned by the graphics backend.
	/// </summary>
	public struct FishUIFontMetrics
	{
		/// <summary>
		/// The height of a line of text in pixels.
		/// </summary>
		public float LineHeight;

		/// <summary>
		/// Distance from the baseline to the top of the tallest character.
		/// </summary>
		public float Ascent;

		/// <summary>
		/// Distance from the baseline to the bottom of the lowest descender.
		/// </summary>
		public float Descent;

		/// <summary>
		/// The baseline position relative to the top of the line.
		/// </summary>
		public float Baseline;

		/// <summary>
		/// Average character width (useful for monospaced fonts).
		/// </summary>
		public float AverageCharWidth;

		/// <summary>
		/// Maximum character width.
		/// </summary>
		public float MaxCharWidth;

		/// <summary>
		/// Creates font metrics with the specified values.
		/// </summary>
		public FishUIFontMetrics(float lineHeight, float ascent, float descent, float baseline, float avgCharWidth = 0, float maxCharWidth = 0)
		{
			LineHeight = lineHeight;
			Ascent = ascent;
			Descent = descent;
			Baseline = baseline;
			AverageCharWidth = avgCharWidth;
			MaxCharWidth = maxCharWidth;
		}
	}
}
