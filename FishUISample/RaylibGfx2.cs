using FishUI;
using Raylib_cs;
using System;
using System.Numerics;

namespace FishUISample
{
	/// <summary>
	/// Minimal Raylib graphics backend using SimpleFishUIGfx.
	/// Only overrides the minimum required methods - other features use default implementations.
	/// </summary>
	class RaylibGfx : SimpleFishUIGfx
	{
		int W;
		int H;
		string Title;

		public bool UseBeginDrawing { get; set; } = true;

		public RaylibGfx(int W, int H, string Title)
		{
			this.W = W;
			this.H = H;
			this.Title = Title;
		}

		#region Required: Initialization and Window

		public override void Init()
		{
			Raylib.SetTraceLogLevel(TraceLogLevel.None);
			Raylib.SetWindowState(ConfigFlags.HighDpiWindow);
			Raylib.SetWindowState(ConfigFlags.Msaa4xHint);
			Raylib.SetWindowState(ConfigFlags.ResizableWindow);
			Raylib.InitWindow(W, H, Title);
			Raylib.SetTargetFPS(Raylib.GetMonitorRefreshRate(0));
		}

		public override int GetWindowWidth() => Raylib.GetScreenWidth();

		public override int GetWindowHeight() => Raylib.GetScreenHeight();

		#endregion

		#region Required: Resource Loading

		public override void FocusWindow()
		{
			Raylib.SetWindowFocused();
		}

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

		public override FontRef LoadFont(string FileName, float Size, float Spacing, FishColor Color)
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
				LineHeight = font.BaseSize,
				IsMonospaced = Math.Abs(wWidth.X - iWidth.X) < 0.5f
			};
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

		#endregion

		#region Required: Image Drawing

		public override void DrawImage(ImageRef Img, Vector2 Pos, float Rot, float Scale, FishColor Color)
		{
			Texture2D tex = (Texture2D)Img.Userdata;
			Raylib.DrawTextureEx(tex, Pos, Rot, Scale, new Color(Color.R, Color.G, Color.B, Color.A));
		}

		// Override for proper sized image drawing (SimpleFishUIGfx default doesn't handle aspect ratio correctly for UI)
		public override void DrawImage(ImageRef Img, Vector2 Pos, Vector2 Size, float Rot, float Scale, FishColor Color)
		{
			Texture2D tex = (Texture2D)Img.Userdata;
			Rectangle src = new Rectangle(0, 0, tex.Width, tex.Height);
			Rectangle dest = new Rectangle(Pos, Size * Scale);
			Raylib.DrawTexturePro(tex, src, dest, Vector2.Zero, Rot, new Color(Color.R, Color.G, Color.B, Color.A));
		}

		#endregion

		#region Required: Text Rendering

		public override Vector2 MeasureText(FontRef Fn, string Text)
		{
			Font font = (Font)Fn.Userdata;
			return Raylib.MeasureTextEx(font, Text, Fn.Size, Fn.Spacing);
		}

		public override void DrawTextColorScale(FontRef Fn, string Text, Vector2 Pos, FishColor Color, float Scale)
		{
			Font font = (Font)Fn.Userdata;
			float fontSize = Fn.Size * Scale;
			float spacing = Fn.Spacing * Scale;
			Raylib.DrawTextEx(font, Text, Round(Pos), fontSize, spacing, new Color(Color.R, Color.G, Color.B, Color.A));
		}

		#endregion

		#region Optional Overrides for Better Performance

		// Override frame lifecycle for Raylib's begin/end drawing
		public override void BeginDrawing(float Dt)
		{
			if (UseBeginDrawing)
			{
				Raylib.BeginDrawing();
				Raylib.ClearBackground(new Color(240, 240, 240, 255));
			}
			Raylib.BeginBlendMode(BlendMode.Alpha);
		}

		public override void EndDrawing()
		{
			Raylib.EndBlendMode();
			if (UseBeginDrawing)
			{
				Raylib.EndDrawing();
			}
		}

		// Override for native rectangle outline (better than 4 lines)
		public override void DrawRectangleOutline(Vector2 Position, Vector2 Size, FishColor Color)
		{
			Raylib.DrawRectangleLinesEx(new Rectangle(Position, Size), 1, new Color(Color.R, Color.G, Color.B, Color.A));
		}

		// Override for native circle drawing
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

		// Override for native NPatch support (much better than 9 separate draws)
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

		// Override for texture filtering support
		public override void SetImageFilter(ImageRef Img, bool pixelated)
		{
			if (Img?.Userdata == null) return;
			Texture2D tex = (Texture2D)Img.Userdata;
			Raylib.SetTextureFilter(tex, pixelated ? TextureFilter.Point : TextureFilter.Trilinear);
		}

		// Override for pixel color reading
		public override FishColor GetImageColor(ImageRef Img, Vector2 Pos)
		{
			Color c = Raylib.GetImageColor((Image)Img.Userdata2, (int)Pos.X, (int)Pos.Y);
			return new FishColor(c.R, c.G, c.B, c.A);
		}

		#endregion

		#region Helpers

		private static Vector2 Round(Vector2 v) => new Vector2((int)Math.Round(v.X), (int)Math.Round(v.Y));

		#endregion
	}
}
