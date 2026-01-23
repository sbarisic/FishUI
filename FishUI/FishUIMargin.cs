using System;

namespace FishUI
{
	/// <summary>
	/// Represents margin or padding values for a control (top, right, bottom, left).
	/// </summary>
	public struct FishUIMargin
	{
		/// <summary>
		/// A margin with all values set to zero.
		/// </summary>
		public static readonly FishUIMargin Zero = new FishUIMargin(0, 0, 0, 0);

		/// <summary>
		/// Top margin/padding in pixels.
		/// </summary>
		public float Top;

		/// <summary>
		/// Right margin/padding in pixels.
		/// </summary>
		public float Right;

		/// <summary>
		/// Bottom margin/padding in pixels.
		/// </summary>
		public float Bottom;

		/// <summary>
		/// Left margin/padding in pixels.
		/// </summary>
		public float Left;

		/// <summary>
		/// Creates a margin with the same value for all sides.
		/// </summary>
		/// <param name="all">Value for all sides.</param>
		public FishUIMargin(float all)
		{
			Top = Right = Bottom = Left = all;
		}

		/// <summary>
		/// Creates a margin with vertical and horizontal values.
		/// </summary>
		/// <param name="vertical">Value for top and bottom.</param>
		/// <param name="horizontal">Value for left and right.</param>
		public FishUIMargin(float vertical, float horizontal)
		{
			Top = Bottom = vertical;
			Left = Right = horizontal;
		}

		/// <summary>
		/// Creates a margin with individual values for each side.
		/// </summary>
		/// <param name="top">Top margin.</param>
		/// <param name="right">Right margin.</param>
		/// <param name="bottom">Bottom margin.</param>
		/// <param name="left">Left margin.</param>
		public FishUIMargin(float top, float right, float bottom, float left)
		{
			Top = top;
			Right = right;
			Bottom = bottom;
			Left = left;
		}

		/// <summary>
		/// Gets the total horizontal space (left + right).
		/// </summary>
		public float Horizontal => Left + Right;

		/// <summary>
		/// Gets the total vertical space (top + bottom).
		/// </summary>
		public float Vertical => Top + Bottom;

		/// <summary>
		/// Returns true if all values are zero.
		/// </summary>
		public bool IsZero => Top == 0 && Right == 0 && Bottom == 0 && Left == 0;
	}
}
