using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace FishUI
{
	/// <summary>
	/// Interface for the graphics backend. Implement this to use FishUI with different rendering libraries.
	/// </summary>
	public interface IFishUIGfx
	{
		/// <summary>
		/// Initializes the graphics backend.
		/// </summary>
		public void Init();

		/// <summary>
		/// Called at the start of each frame before drawing controls.
		/// </summary>
		/// <param name="Dt">Delta time since last frame.</param>
		public void BeginDrawing(float Dt);

		/// <summary>
		/// Called at the end of each frame after drawing all controls.
		/// </summary>
		public void EndDrawing();

		/// <summary>
		/// Sets the scissor clipping region.
		/// </summary>
		public void BeginScissor(Vector2 Pos, Vector2 Size);

		/// <summary>
		/// Pushes a new scissor region onto the scissor stack.
		/// </summary>
		public void PushScissor(Vector2 Pos, Vector2 Size);

		/// <summary>
		/// Pops the top scissor region from the stack.
		/// </summary>
		public void PopScissor();

		/// <summary>
		/// Ends scissor clipping.
		/// </summary>
		public void EndScissor();

		/// <summary>
		/// Gets the window width in pixels.
		/// </summary>
		public int GetWindowWidth();

		/// <summary>
		/// Gets the window height in pixels.
		/// </summary>
		public int GetWindowHeight();

		// Loading

		/// <summary>
		/// Loads a font from a file.
		/// </summary>
		public FontRef LoadFont(string FileName, float Size, float Spacing, FishColor Color);

		/// <summary>
		/// Loads a font from a file with a specific style.
		/// </summary>
		public FontRef LoadFont(string FileName, float Size, float Spacing, FishColor Color, FontStyle Style);

		/// <summary>
		/// Loads an image from a file.
		/// </summary>
		public ImageRef LoadImage(string FileName);

		/// <summary>
		/// Loads a sub-region of an image from a file.
		/// </summary>
		public ImageRef LoadImage(string FileName, int X, int Y, int W, int H);

		/// <summary>
		/// Creates a sub-region image from an existing image.
		/// </summary>
		public ImageRef LoadImage(ImageRef Orig, int X, int Y, int W, int H);

		/// <summary>
		/// Gets the color of a pixel in an image.
		/// </summary>
		public FishColor GetImageColor(ImageRef Img, Vector2 Pos);

		/// <summary>
		/// Measures the size of text when rendered with the specified font.
		/// </summary>
		public Vector2 MeasureText(FontRef Fn, string Text);

		/// <summary>
		/// Gets font metrics (line height, ascent, descent, baseline) for a font.
		/// </summary>
		public FishUIFontMetrics GetFontMetrics(FontRef Fn);

		// Drawing

		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		public void DrawLine(Vector2 Pos1, Vector2 Pos2, float Thick, FishColor Clr);

		/// <summary>
		/// Draws a filled rectangle.
		/// </summary>
		public void DrawRectangle(Vector2 Position, Vector2 Size, FishColor Color);

		/// <summary>
		/// Draws a rectangle outline.
		/// </summary>
		public void DrawRectangleOutline(Vector2 Position, Vector2 Size, FishColor Color);

		/// <summary>
		/// Draws an image at the specified position.
		/// </summary>
		public void DrawImage(ImageRef Img, Vector2 Pos, float Rot, float Scale, FishColor Color);

		/// <summary>
		/// Draws an image scaled to the specified size.
		/// </summary>
		public void DrawImage(ImageRef Img, Vector2 Pos, Vector2 Size, float Rot, float Scale, FishColor Color);

		/// <summary>
		/// Draws a 9-patch (9-slice) image.
		/// </summary>
		public void DrawNPatch(NPatch NP, Vector2 Pos, Vector2 Size, FishColor Color);

		/// <summary>
		/// Draws a 9-patch (9-slice) image with rotation.
		/// </summary>
		public void DrawNPatch(NPatch NP, Vector2 Pos, Vector2 Size, FishColor Color, float Rotation);

		/// <summary>
		/// Draws text at the specified position.
		/// </summary>
		public void DrawText(FontRef Fn, string Text, Vector2 Pos);

		/// <summary>
		/// Draws text with a custom color.
		/// </summary>
		public void DrawTextColor(FontRef Fn, string Text, Vector2 Pos, FishColor Color);

		/// <summary>
		/// Sets the texture filter mode for an image. Call before drawing to change filtering.
		/// </summary>
		/// <param name="Img">The image to set the filter for.</param>
		/// <param name="pixelated">True for nearest-neighbor (pixelated), false for smooth (bilinear/trilinear).</param>
		public void SetImageFilter(ImageRef Img, bool pixelated);

	}
}
