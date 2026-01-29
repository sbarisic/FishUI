using FishUI;
using Raylib_cs;
using System;
using System.Numerics;

namespace RaylibFishGfx
{
	/// <summary>
	/// Raylib graphics backend for FishUI. Implements all IFishUIGfx methods using Raylib-cs.
	/// </summary>
	/// <remarks>
	/// This is a complete, production-ready Raylib backend that demonstrates how to implement
	/// SimpleFishUIGfx with all optional overrides for maximum performance.
	/// </remarks>
	public class RaylibFishGfx : SimpleFishUIGfx
	{
		private readonly int _initialWidth;
		private readonly int _initialHeight;
		private readonly string _title;

		/// <summary>
		/// When true (default), BeginDrawing/EndDrawing will call Raylib.BeginDrawing/EndDrawing.
		/// Set to false when integrating with an existing game loop that manages its own frame lifecycle.
		/// </summary>
		public bool UseBeginDrawing { get; set; } = true;

		/// <summary>
		/// Creates a new Raylib graphics backend.
		/// </summary>
		/// <param name="width">Initial window width.</param>
		/// <param name="height">Initial window height.</param>
		/// <param name="title">Window title.</param>
		public RaylibFishGfx(int width, int height, string title)
		{
			_initialWidth = width;
			_initialHeight = height;
			_title = title;
		}

		#region Initialization and Window

		/// <inheritdoc/>
		public override void Init()
		{
			Raylib.SetTraceLogLevel(TraceLogLevel.None);
			Raylib.SetWindowState(ConfigFlags.HighDpiWindow);
			Raylib.SetWindowState(ConfigFlags.Msaa4xHint);
			Raylib.SetWindowState(ConfigFlags.ResizableWindow);
			Raylib.InitWindow(_initialWidth, _initialHeight, _title);
			Raylib.SetTargetFPS(Raylib.GetMonitorRefreshRate(0));
		}

		/// <inheritdoc/>
		public override int GetWindowWidth() => Raylib.GetScreenWidth();

		/// <inheritdoc/>
		public override int GetWindowHeight() => Raylib.GetScreenHeight();

		/// <inheritdoc/>
		public override void FocusWindow() => Raylib.SetWindowFocused();

		#endregion

		#region Resource Loading

		public override ImageRef LoadImage(string FileName)
		{
			Texture2D tex = Raylib.LoadTexture(FileName);
			Raylib.SetTextureFilter(tex, TextureFilter.Trilinear);
			Image img = Raylib.LoadImage(FileName);

			return new ImageRef
			{
				Path = FileName,
				Width = tex.Width,
				Height = tex.Height,
				Userdata = tex,
				Userdata2 = img
			};
		}

		public override ImageRef LoadImage(string FileName, int X, int Y, int W, int H)
		{
			Image img = Raylib.LoadImage(FileName);
			Raylib.ImageCrop(ref img, new Rectangle(X, Y, W, H));
			Texture2D tex = Raylib.LoadTextureFromImage(img);
			Raylib.SetTextureFilter(tex, TextureFilter.Trilinear);

			return new ImageRef
			{
				Path = FileName,
				Width = tex.Width,
				Height = tex.Height,
				Userdata = tex,
				Userdata2 = img
			};
		}

		public override ImageRef LoadImage(ImageRef Orig, int X, int Y, int W, int H)
		{
			Image img = Raylib.ImageFromImage((Image)Orig.Userdata2, new Rectangle(X, Y, W, H));
			Texture2D tex = Raylib.LoadTextureFromImage(img);
			Raylib.SetTextureFilter(tex, TextureFilter.Trilinear);

			return new ImageRef
			{
				Path = Orig.Path,
				Width = tex.Width,
				Height = tex.Height,
				Userdata = tex,
				Userdata2 = img
			};
		}

		public override FontRef LoadFont(string FileName, float Size, float Spacing, FishColor Color)
		{
			return LoadFont(FileName, Size, Spacing, Color, FontStyle.Regular);
		}

		public override FontRef LoadFont(string FileName, float Size, float Spacing, FishColor Color, FontStyle Style)
		{
			Font font = Raylib.LoadFontEx(FileName, (int)Size, null, 250);

			// Check if monospaced
			Vector2 wWidth = Raylib.MeasureTextEx(font, "W", Size, Spacing);
			Vector2 iWidth = Raylib.MeasureTextEx(font, "i", Size, Spacing);

			return new FontRef
			{
				Path = FileName,
				Userdata = font,
				Spacing = Spacing,
				Size = Size,
				Color = Color,
				Style = Style,
				LineHeight = font.BaseSize,
				IsMonospaced = Math.Abs(wWidth.X - iWidth.X) < 0.5f
			};
		}

		public override FishUIFontMetrics GetFontMetrics(FontRef Fn)
		{
			Font font = (Font)Fn.Userdata;
			float lineHeight = font.BaseSize;
			float ascent = lineHeight * 0.8f;
			float descent = lineHeight * 0.2f;
			float baseline = ascent;

			Vector2 avgSize = Raylib.MeasureTextEx(font, "x", Fn.Size, Fn.Spacing);
			Vector2 maxSize = Raylib.MeasureTextEx(font, "W", Fn.Size, Fn.Spacing);

			return new FishUIFontMetrics(lineHeight, ascent, descent, baseline, avgSize.X, maxSize.X);
		}

		#endregion

		#region Required: Scissor Clipping

		public override void BeginScissor(Vector2 Pos, Vector2 Size)
		{
			Raylib.BeginScissorMode((int)Pos.X, (int)Pos.Y, (int)Size.X, (int)Size.Y);
		}

		public override void EndScissor()
		{
			Raylib.EndScissorMode();
		}

		#endregion

		#region Required: Primitive Drawing

		public override void DrawLine(Vector2 Pos1, Vector2 Pos2, float Thick, FishColor Clr)
		{
			Raylib.DrawLineEx(Pos1, Pos2, Thick, new Color(Clr.R, Clr.G, Clr.B, Clr.A));
		}

		public override void DrawRectangle(Vector2 Position, Vector2 Size, FishColor Color)
		{
			Raylib.DrawRectangleV(Position, Size, new Color(Color.R, Color.G, Color.B, Color.A));
		}

		public override void DrawRectangleOutline(Vector2 Position, Vector2 Size, FishColor Color)
		{
			Raylib.DrawRectangleLinesEx(new Rectangle(Position, Size), 1, new Color(Color.R, Color.G, Color.B, Color.A));
		}

		public override void DrawCircle(Vector2 Center, float Radius, FishColor Color)
		{
			Raylib.DrawCircleV(Center, Radius, new Color(Color.R, Color.G, Color.B, Color.A));
		}

		public override void DrawCircleOutline(Vector2 Center, float Radius, FishColor Color, float Thickness = 1)
		{
			Color c = new Color(Color.R, Color.G, Color.B, Color.A);
			for (float r = Radius - Thickness / 2; r <= Radius + Thickness / 2; r += 0.5f)
			{
				Raylib.DrawCircleLinesV(Center, r, c);
			}
		}

		#endregion

		#region Required: Image Drawing

		public override void DrawImage(ImageRef Img, Vector2 Pos, float Rot, float Scale, FishColor Color)
		{
			Texture2D tex = (Texture2D)Img.Userdata;
			Raylib.DrawTextureEx(tex, Pos, Rot, Scale, new Color(Color.R, Color.G, Color.B, Color.A));
		}

		public override void DrawImage(ImageRef Img, Vector2 Pos, Vector2 Size, float Rot, float Scale, FishColor Color)
		{
			Texture2D tex = (Texture2D)Img.Userdata;
			Rectangle src = new Rectangle(0, 0, tex.Width, tex.Height);
			Rectangle dest = new Rectangle(Pos, Size * Scale);
			Raylib.DrawTexturePro(tex, src, dest, Vector2.Zero, Rot, new Color(Color.R, Color.G, Color.B, Color.A));
		}

		public override void DrawNPatch(NPatch NP, Vector2 Pos, Vector2 Size, FishColor Color)
		{
			DrawNPatch(NP, Pos, Size, Color, 0);
		}

		public override void DrawNPatch(NPatch NP, Vector2 Pos, Vector2 Size, FishColor Color, float Rotation)
		{
			Texture2D tex = (Texture2D)NP.Image.Userdata;
			NPatchInfo info = new NPatchInfo
			{
				Left = NP.Left,
				Right = NP.Right,
				Top = NP.Top,
				Bottom = NP.Bottom,
				Source = new Rectangle(NP.ImagePos, NP.ImageSize),
				Layout = NPatchLayout.NinePatch
			};

			Vector2 origin = Rotation != 0 ? Size / 2 : Vector2.Zero;
			Vector2 drawPos = Rotation != 0 ? Pos + origin : Pos;

			Raylib.DrawTextureNPatch(tex, info, new Rectangle(Round(drawPos), Round(Size)), Round(origin), Rotation,
				new Color(Color.R, Color.G, Color.B, Color.A));
		}

		public override void SetImageFilter(ImageRef Img, bool pixelated)
		{
			if (Img?.Userdata == null) return;
			Texture2D tex = (Texture2D)Img.Userdata;
			Raylib.SetTextureFilter(tex, pixelated ? TextureFilter.Point : TextureFilter.Trilinear);
		}

		public override FishColor GetImageColor(ImageRef Img, Vector2 Pos)
		{
			Color c = Raylib.GetImageColor((Image)Img.Userdata2, (int)Pos.X, (int)Pos.Y);
			return new FishColor(c.R, c.G, c.B, c.A);
		}

		/// <summary>
		/// Draws a region of an image to a destination rectangle using Raylib's DrawTexturePro.
		/// </summary>
		protected override void DrawImageRegion(ImageRef Img, Vector2 srcPos, Vector2 srcSize, Vector2 destPos, Vector2 destSize, FishColor Color)
		{
			Texture2D tex = (Texture2D)Img.Userdata;
			Rectangle src = new Rectangle(srcPos, srcSize);
			Rectangle dest = new Rectangle(destPos, destSize);
			Raylib.DrawTexturePro(tex, src, dest, Vector2.Zero, 0, ToColor(Color));
		}

		#endregion

		#region Text Rendering

		public override Vector2 MeasureText(FontRef Fn, string Text)
		{
			Font font = (Font)Fn.Userdata;
			return Raylib.MeasureTextEx(font, Text, Fn.Size, Fn.Spacing);
		}

		public override void DrawText(FontRef Fn, string Text, Vector2 Pos)
		{
			Font font = (Font)Fn.Userdata;
			Raylib.DrawTextEx(font, Text, Round(Pos), Fn.Size, Fn.Spacing,
				new Color(Fn.Color.R, Fn.Color.G, Fn.Color.B, Fn.Color.A));
		}

		public override void DrawTextColor(FontRef Fn, string Text, Vector2 Pos, FishColor Color)
		{
			Font font = (Font)Fn.Userdata;
			Raylib.DrawTextEx(font, Text, Round(Pos), Fn.Size, Fn.Spacing,
				new Color(Color.R, Color.G, Color.B, Color.A));
		}

		public override void DrawTextColorScale(FontRef Fn, string Text, Vector2 Pos, FishColor Color, float Scale)
		{
			Font font = (Font)Fn.Userdata;
			float fontSize = Fn.Size * Scale;
			float spacing = Fn.Spacing * Scale;
			Raylib.DrawTextEx(font, Text, Round(Pos), fontSize, spacing,
				new Color(Color.R, Color.G, Color.B, Color.A));
		}

		#endregion

		#region Frame Lifecycle

		/// <inheritdoc/>
		public override void BeginDrawing(float Dt)
		{
			if (UseBeginDrawing)
			{
				Raylib.BeginDrawing();
				Raylib.ClearBackground(new Color(240, 240, 240, 255));
			}
			Raylib.BeginBlendMode(BlendMode.Alpha);
		}

		/// <inheritdoc/>
		public override void EndDrawing()
		{
			Raylib.EndBlendMode();
			if (UseBeginDrawing)
			{
				Raylib.EndDrawing();
			}
		}

		#endregion

		#region Helpers

		/// <summary>
		/// Converts a FishColor to a Raylib Color.
		/// </summary>
		private static Color ToColor(FishColor c) => new Color(c.R, c.G, c.B, c.A);

		/// <summary>
		/// Rounds a Vector2 to integer pixel coordinates.
		/// </summary>
		private static Vector2 Round(Vector2 v) => new Vector2((int)Math.Round(v.X), (int)Math.Round(v.Y));

		#endregion
	}
}
