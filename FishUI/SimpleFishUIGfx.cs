using System;
using System.Collections.Generic;
using System.Numerics;

namespace FishUI
{
	/// <summary>
	/// Simplified abstract base class for graphics backends. Provides default implementations
	/// for many methods by composing them from primitive operations.
	/// </summary>
	/// <remarks>
	/// Override only what you need. Minimum required overrides for a functional backend:
	/// - Init(), GetWindowWidth(), GetWindowHeight()
	/// - LoadImage(string), LoadFont(string, float, float, FishColor)
	/// - DrawLine(), DrawRectangle(), DrawImage() (basic overload)
	/// - DrawTextColorScale(), MeasureText()
	/// - BeginScissor(), EndScissor()
	/// 
	/// Optional overrides for better performance:
	/// - DrawRectangleOutline() - defaults to 4 DrawLine calls
	/// - DrawCircle(), DrawCircleOutline() - defaults to polygon approximation
	/// - DrawNPatch() - defaults to 9 DrawImageRegion calls
	/// - PushScissor(), PopScissor() - defaults to stack-based BeginScissor/EndScissor
	/// </remarks>
	public abstract class SimpleFishUIGfx : IFishUIGfx
	{
		#region Scissor Stack

		private readonly Stack<(Vector2 Pos, Vector2 Size)> _scissorStack = new();

		#endregion

		#region Abstract Methods (Must Override)

		/// <summary>
		/// Initializes the graphics backend.
		/// </summary>
		public abstract void Init();

		/// <summary>
		/// Gets the window width in pixels.
		/// </summary>
		public abstract int GetWindowWidth();

		/// <summary>
		/// Gets the window height in pixels.
		/// </summary>
		public abstract int GetWindowHeight();

		#endregion

		#region Frame Lifecycle

		/// <summary>
		/// Called at the start of each frame. Override if your backend needs begin/end frame calls.
		/// </summary>
		public virtual void BeginDrawing(float Dt)
		{
			// Does nothing by default, some graphics systems do not need it
		}

		/// <summary>
		/// Called at the end of each frame. Override if your backend needs begin/end frame calls.
		/// </summary>
		public virtual void EndDrawing()
		{
			// Does nothing by default
		}

		#endregion

		#region Scissor Clipping

		/// <summary>
		/// Sets the scissor clipping region. Must be overridden.
		/// </summary>
		public virtual void BeginScissor(Vector2 Pos, Vector2 Size)
		{
			throw new NotImplementedException("BeginScissor must be implemented by the graphics backend.");
		}

		/// <summary>
		/// Ends scissor clipping. Must be overridden.
		/// </summary>
		public virtual void EndScissor()
		{
			throw new NotImplementedException("EndScissor must be implemented by the graphics backend.");
		}

		/// <summary>
		/// Pushes a new scissor region onto the stack. Default implementation uses BeginScissor/EndScissor.
		/// </summary>
		public virtual void PushScissor(Vector2 Pos, Vector2 Size)
		{
			// If there's an existing scissor, intersect with it
			if (_scissorStack.Count > 0)
			{
				var current = _scissorStack.Peek();
				(Pos, Size) = IntersectRectangles(current.Pos, current.Size, Pos, Size);
			}

			_scissorStack.Push((Pos, Size));
			BeginScissor(Pos, Size);
		}

		/// <summary>
		/// Pops the top scissor region from the stack. Default implementation uses BeginScissor/EndScissor.
		/// </summary>
		public virtual void PopScissor()
		{
			if (_scissorStack.Count > 0)
			{
				_scissorStack.Pop();
			}

			if (_scissorStack.Count > 0)
			{
				var current = _scissorStack.Peek();
				BeginScissor(current.Pos, current.Size);
			}
			else
			{
				EndScissor();
			}
		}

		/// <summary>
		/// Intersects two rectangles and returns the overlapping region.
		/// </summary>
		private static (Vector2 Pos, Vector2 Size) IntersectRectangles(Vector2 pos1, Vector2 size1, Vector2 pos2, Vector2 size2)
		{
			float x1 = Math.Max(pos1.X, pos2.X);
			float y1 = Math.Max(pos1.Y, pos2.Y);
			float x2 = Math.Min(pos1.X + size1.X, pos2.X + size2.X);
			float y2 = Math.Min(pos1.Y + size1.Y, pos2.Y + size2.Y);

			float width = Math.Max(0, x2 - x1);
			float height = Math.Max(0, y2 - y1);

			return (new Vector2(x1, y1), new Vector2(width, height));
		}

		#endregion

		#region Resource Loading

		/// <summary>
		/// Loads a font from a file. Must be overridden for text rendering.
		/// </summary>
		public virtual FontRef LoadFont(string FileName, float Size, float Spacing, FishColor Color)
		{
			throw new NotImplementedException("LoadFont must be implemented by the graphics backend.");
		}

		/// <summary>
		/// Loads a font with style. Default implementation ignores style and calls base LoadFont.
		/// </summary>
		public virtual FontRef LoadFont(string FileName, float Size, float Spacing, FishColor Color, FontStyle Style)
		{
			// Default: ignore style, just load the font
			return LoadFont(FileName, Size, Spacing, Color);
		}

		/// <summary>
		/// Loads an image from a file. Must be overridden.
		/// </summary>
		public virtual ImageRef LoadImage(string FileName)
		{
			throw new NotImplementedException("LoadImage must be implemented by the graphics backend.");
		}

		/// <summary>
		/// Loads a sub-region of an image. Default implementation loads full image then creates atlas region.
		/// </summary>
		public virtual ImageRef LoadImage(string FileName, int X, int Y, int W, int H)
		{
			var fullImage = LoadImage(FileName);
			return ImageRef.FromAtlasRegion(fullImage, X, Y, W, H);
		}

		/// <summary>
		/// Creates a sub-region from an existing image. Default implementation uses ImageRef.FromAtlasRegion.
		/// </summary>
		public virtual ImageRef LoadImage(ImageRef Orig, int X, int Y, int W, int H)
		{
			return ImageRef.FromAtlasRegion(Orig, X, Y, W, H);
		}

		/// <summary>
		/// Gets the color of a pixel in an image. Override for pixel-level image operations.
		/// </summary>
		public virtual FishColor GetImageColor(ImageRef Img, Vector2 Pos)
		{
			throw new NotImplementedException("GetImageColor must be implemented by the graphics backend.");
		}

		/// <summary>
		/// Sets the texture filter mode. Override if your backend supports texture filtering.
		/// </summary>
		public virtual void SetImageFilter(ImageRef Img, bool pixelated)
		{
			// Does nothing by default - not all backends support texture filtering
		}

		#endregion

		#region Text Rendering

		/// <summary>
		/// Measures text size. Must be overridden for text rendering.
		/// </summary>
		public virtual Vector2 MeasureText(FontRef Fn, string Text)
		{
			throw new NotImplementedException("MeasureText must be implemented by the graphics backend.");
		}

		/// <summary>
		/// Gets font metrics. Default implementation estimates from MeasureText.
		/// </summary>
		public virtual FishUIFontMetrics GetFontMetrics(FontRef Fn)
		{
			// Estimate based on measuring a tall character
			var size = MeasureText(Fn, "Hg");
			return new FishUIFontMetrics
			{
				LineHeight = size.Y,
				Ascent = size.Y * 0.8f,
				Descent = size.Y * 0.2f,
				Baseline = size.Y * 0.8f
			};
		}

		/// <summary>
		/// Draws text with color and scale. Must be overridden for text rendering.
		/// </summary>
		public virtual void DrawTextColorScale(FontRef Fn, string Text, Vector2 Pos, FishColor Color, float Scale)
		{
			throw new NotImplementedException("DrawTextColorScale must be implemented by the graphics backend.");
		}

		/// <summary>
		/// Draws text with color. Default implementation calls DrawTextColorScale with scale 1.0.
		/// </summary>
		public virtual void DrawTextColor(FontRef Fn, string Text, Vector2 Pos, FishColor Color)
		{
			DrawTextColorScale(Fn, Text, Pos, Color, 1.0f);
		}

		/// <summary>
		/// Draws text with font's default color. Default implementation calls DrawTextColor.
		/// </summary>
		public virtual void DrawText(FontRef Fn, string Text, Vector2 Pos)
		{
			DrawTextColor(Fn, Text, Pos, Fn.Color);
		}

		#endregion

		#region Primitive Drawing

		/// <summary>
		/// Draws a line. Must be overridden.
		/// </summary>
		public virtual void DrawLine(Vector2 Pos1, Vector2 Pos2, float Thick, FishColor Clr)
		{
			throw new NotImplementedException("DrawLine must be implemented by the graphics backend.");
		}

		/// <summary>
		/// Draws a filled rectangle. Must be overridden.
		/// </summary>
		public virtual void DrawRectangle(Vector2 Position, Vector2 Size, FishColor Color)
		{
			throw new NotImplementedException("DrawRectangle must be implemented by the graphics backend.");
		}

		/// <summary>
		/// Draws a rectangle outline. Default implementation uses 4 DrawLine calls.
		/// </summary>
		public virtual void DrawRectangleOutline(Vector2 Position, Vector2 Size, FishColor Color)
		{
			float thickness = 1f;
			Vector2 topLeft = Position;
			Vector2 topRight = new Vector2(Position.X + Size.X, Position.Y);
			Vector2 bottomLeft = new Vector2(Position.X, Position.Y + Size.Y);
			Vector2 bottomRight = Position + Size;

			DrawLine(topLeft, topRight, thickness, Color);      // Top
			DrawLine(topRight, bottomRight, thickness, Color);  // Right
			DrawLine(bottomRight, bottomLeft, thickness, Color); // Bottom
			DrawLine(bottomLeft, topLeft, thickness, Color);    // Left
		}

		/// <summary>
		/// Draws a filled circle. Default implementation uses polygon approximation.
		/// </summary>
		public virtual void DrawCircle(Vector2 Center, float Radius, FishColor Color)
		{
			// Approximate with triangles - draw as a filled polygon
			// For a simple implementation, we draw lines from center to circumference
			int segments = Math.Max(12, (int)(Radius * 0.5f));
			float angleStep = MathF.PI * 2 / segments;

			for (int i = 0; i < segments; i++)
			{
				float angle1 = i * angleStep;
				float angle2 = (i + 1) * angleStep;

				Vector2 p1 = Center + new Vector2(MathF.Cos(angle1), MathF.Sin(angle1)) * Radius;
				Vector2 p2 = Center + new Vector2(MathF.Cos(angle2), MathF.Sin(angle2)) * Radius;

				// Draw as thick lines from center - this is an approximation
				// For true filled circle, override with native implementation
				DrawLine(Center, p1, Radius, Color);
			}
		}

		/// <summary>
		/// Draws a circle outline. Default implementation uses polygon approximation with lines.
		/// </summary>
		public virtual void DrawCircleOutline(Vector2 Center, float Radius, FishColor Color, float Thickness = 1)
		{
			int segments = Math.Max(16, (int)(Radius * 0.8f));
			float angleStep = MathF.PI * 2 / segments;

			for (int i = 0; i < segments; i++)
			{
				float angle1 = i * angleStep;
				float angle2 = (i + 1) * angleStep;

				Vector2 p1 = Center + new Vector2(MathF.Cos(angle1), MathF.Sin(angle1)) * Radius;
				Vector2 p2 = Center + new Vector2(MathF.Cos(angle2), MathF.Sin(angle2)) * Radius;

				DrawLine(p1, p2, Thickness, Color);
			}
		}

		#endregion

		#region Image Drawing

		/// <summary>
		/// Draws an image at position with rotation and scale. Must be overridden.
		/// </summary>
		public virtual void DrawImage(ImageRef Img, Vector2 Pos, float Rot, float Scale, FishColor Color)
		{
			throw new NotImplementedException("DrawImage must be implemented by the graphics backend.");
		}

		/// <summary>
		/// Draws an image scaled to fit a specific size. Default implementation calculates scale from size.
		/// </summary>
		public virtual void DrawImage(ImageRef Img, Vector2 Pos, Vector2 Size, float Rot, float Scale, FishColor Color)
		{
			// Calculate scale to fit the target size
			float scaleX = Size.X / Img.Width;
			float scaleY = Size.Y / Img.Height;
			float combinedScale = Math.Min(scaleX, scaleY) * Scale;

			DrawImage(Img, Pos, Rot, combinedScale, Color);
		}

		/// <summary>
		/// Draws a region of an image to a destination rectangle. Used internally for NPatch.
		/// Override for better performance with native source rectangle support.
		/// </summary>
		protected virtual void DrawImageRegion(ImageRef Img, Vector2 srcPos, Vector2 srcSize, Vector2 destPos, Vector2 destSize, FishColor Color)
		{
			// Default implementation: create a temporary atlas region and draw it
			// This is inefficient - override with native source rectangle drawing for better performance
			var region = ImageRef.FromAtlasRegion(Img, (int)srcPos.X, (int)srcPos.Y, (int)srcSize.X, (int)srcSize.Y);
			DrawImage(region, destPos, destSize, 0, 1, Color);
		}

		#endregion

		#region 9-Patch Drawing

		/// <summary>
		/// Draws a 9-patch (9-slice) image. Default implementation draws 9 regions using DrawImageRegion.
		/// </summary>
		public virtual void DrawNPatch(NPatch NP, Vector2 Pos, Vector2 Size, FishColor Color)
		{
			DrawNPatch(NP, Pos, Size, Color, 0);
		}

		/// <summary>
		/// Draws a 9-patch with rotation. Default implementation draws 9 regions.
		/// Note: Rotation is not supported in the default implementation.
		/// </summary>
		public virtual void DrawNPatch(NPatch NP, Vector2 Pos, Vector2 Size, FishColor Color, float Rotation)
		{
			// Note: Rotation is ignored in this default implementation
			// Override with native NPatch support for rotation

			float srcLeft = NP.Left;
			float srcRight = NP.Right;
			float srcTop = NP.Top;
			float srcBottom = NP.Bottom;

			float srcX = NP.ImagePos.X;
			float srcY = NP.ImagePos.Y;
			float srcW = NP.ImageSize.X;
			float srcH = NP.ImageSize.Y;

			float srcCenterW = srcW - srcLeft - srcRight;
			float srcCenterH = srcH - srcTop - srcBottom;

			float destCenterW = Size.X - srcLeft - srcRight;
			float destCenterH = Size.Y - srcTop - srcBottom;

			// Ensure minimum sizes
			if (destCenterW < 0) destCenterW = 0;
			if (destCenterH < 0) destCenterH = 0;

			// Source rectangles (9 regions)
			// Row 1: Top-Left, Top-Center, Top-Right
			// Row 2: Middle-Left, Center, Middle-Right
			// Row 3: Bottom-Left, Bottom-Center, Bottom-Right

			// Top-Left corner
			if (srcLeft > 0 && srcTop > 0)
			{
				DrawImageRegion(NP.Image,
					new Vector2(srcX, srcY),
					new Vector2(srcLeft, srcTop),
					Pos,
					new Vector2(srcLeft, srcTop),
					Color);
			}

			// Top-Center (stretches horizontally)
			if (srcCenterW > 0 && srcTop > 0)
			{
				DrawImageRegion(NP.Image,
					new Vector2(srcX + srcLeft, srcY),
					new Vector2(srcCenterW, srcTop),
					new Vector2(Pos.X + srcLeft, Pos.Y),
					new Vector2(destCenterW, srcTop),
					Color);
			}

			// Top-Right corner
			if (srcRight > 0 && srcTop > 0)
			{
				DrawImageRegion(NP.Image,
					new Vector2(srcX + srcW - srcRight, srcY),
					new Vector2(srcRight, srcTop),
					new Vector2(Pos.X + srcLeft + destCenterW, Pos.Y),
					new Vector2(srcRight, srcTop),
					Color);
			}

			// Middle-Left (stretches vertically)
			if (srcLeft > 0 && srcCenterH > 0)
			{
				DrawImageRegion(NP.Image,
					new Vector2(srcX, srcY + srcTop),
					new Vector2(srcLeft, srcCenterH),
					new Vector2(Pos.X, Pos.Y + srcTop),
					new Vector2(srcLeft, destCenterH),
					Color);
			}

			// Center (stretches both ways)
			if (srcCenterW > 0 && srcCenterH > 0)
			{
				DrawImageRegion(NP.Image,
					new Vector2(srcX + srcLeft, srcY + srcTop),
					new Vector2(srcCenterW, srcCenterH),
					new Vector2(Pos.X + srcLeft, Pos.Y + srcTop),
					new Vector2(destCenterW, destCenterH),
					Color);
			}

			// Middle-Right (stretches vertically)
			if (srcRight > 0 && srcCenterH > 0)
			{
				DrawImageRegion(NP.Image,
					new Vector2(srcX + srcW - srcRight, srcY + srcTop),
					new Vector2(srcRight, srcCenterH),
					new Vector2(Pos.X + srcLeft + destCenterW, Pos.Y + srcTop),
					new Vector2(srcRight, destCenterH),
					Color);
			}

			// Bottom-Left corner
			if (srcLeft > 0 && srcBottom > 0)
			{
				DrawImageRegion(NP.Image,
					new Vector2(srcX, srcY + srcH - srcBottom),
					new Vector2(srcLeft, srcBottom),
					new Vector2(Pos.X, Pos.Y + srcTop + destCenterH),
					new Vector2(srcLeft, srcBottom),
					Color);
			}

			// Bottom-Center (stretches horizontally)
			if (srcCenterW > 0 && srcBottom > 0)
			{
				DrawImageRegion(NP.Image,
					new Vector2(srcX + srcLeft, srcY + srcH - srcBottom),
					new Vector2(srcCenterW, srcBottom),
					new Vector2(Pos.X + srcLeft, Pos.Y + srcTop + destCenterH),
					new Vector2(destCenterW, srcBottom),
					Color);
			}

			// Bottom-Right corner
			if (srcRight > 0 && srcBottom > 0)
			{
				DrawImageRegion(NP.Image,
					new Vector2(srcX + srcW - srcRight, srcY + srcH - srcBottom),
					new Vector2(srcRight, srcBottom),
					new Vector2(Pos.X + srcLeft + destCenterW, Pos.Y + srcTop + destCenterH),
					new Vector2(srcRight, srcBottom),
					Color);
			}
		}

		#endregion
	}
}
