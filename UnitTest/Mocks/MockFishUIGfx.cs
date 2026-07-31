using System.Numerics;
using FishUI;

namespace UnitTest.Mocks
{
	/// <summary>
	/// Mock graphics backend for unit testing FishUI without a real rendering backend.
	/// </summary>
	public class MockFishUIGfx : IFishUIGfx
	{
		public int WindowWidth { get; set; } = 800;
		public int WindowHeight { get; set; } = 600;

		// Tracking for verification
		public List<string> DrawCalls { get; } = new();
		public bool WasInitialized { get; private set; }
		public int BeginDrawingCount { get; private set; }
		public int EndDrawingCount { get; private set; }

		public void Init() => WasInitialized = true;
		public void BeginDrawing(float Dt) => BeginDrawingCount++;
		public void EndDrawing() => EndDrawingCount++;

		public int GetWindowWidth() => WindowWidth;
		public int GetWindowHeight() => WindowHeight;
		public void FocusWindow() { }

		public void BeginScissor(Vector2 Pos, Vector2 Size) => DrawCalls.Add($"BeginScissor({Pos}, {Size})");
		public void EndScissor() => DrawCalls.Add("EndScissor");
		public void PushScissor(Vector2 Pos, Vector2 Size) => DrawCalls.Add($"PushScissor({Pos}, {Size})");
		public void PopScissor() => DrawCalls.Add("PopScissor");

		public FontRef LoadFont(string FileName, float Size, float Spacing, FishColor Color) => new FontRef();
		public FontRef LoadFont(string FileName, float Size, float Spacing, FishColor Color, FontStyle Style) => new FontRef();
		public ImageRef LoadImage(string FileName) => new ImageRef { Width = 32, Height = 32 };
		public ImageRef LoadImage(string FileName, int X, int Y, int W, int H) => new ImageRef { Width = W, Height = H };
		public ImageRef LoadImage(ImageRef Orig, int X, int Y, int W, int H) => new ImageRef { Width = W, Height = H };

		public FishColor GetImageColor(ImageRef Img, Vector2 Pos) => FishColor.White;
		public Vector2 MeasureText(FontRef Fn, string Text) => new Vector2(Text?.Length * 8 ?? 0, 16);
		public FishUIFontMetrics GetFontMetrics(FontRef Fn) => new FishUIFontMetrics { LineHeight = 16, Ascent = 12, Descent = 4, Baseline = 12 };

		public void DrawLine(Vector2 Pos1, Vector2 Pos2, float Thick, FishColor Clr) => DrawCalls.Add($"DrawLine({Pos1}, {Pos2})");
		public void DrawRectangle(Vector2 Position, Vector2 Size, FishColor Color) => DrawCalls.Add($"DrawRectangle({Position}, {Size})");
		public void DrawRectangleOutline(Vector2 Position, Vector2 Size, FishColor Color) => DrawCalls.Add($"DrawRectangleOutline({Position}, {Size})");

		public void DrawImage(ImageRef Img, Vector2 Pos, float Rot, float Scale, FishColor Color) => DrawCalls.Add($"DrawImage({Pos})");
		public void DrawImage(ImageRef Img, Vector2 Pos, Vector2 Size, float Rot, float Scale, FishColor Color) => DrawCalls.Add($"DrawImage({Pos}, {Size})");
		public void DrawNPatch(NPatch NP, Vector2 Pos, Vector2 Size, FishColor Color) => DrawCalls.Add($"DrawNPatch({Pos}, {Size})");
		public void DrawNPatch(NPatch NP, Vector2 Pos, Vector2 Size, FishColor Color, float Rotation) => DrawCalls.Add($"DrawNPatch({Pos}, {Size}, rot={Rotation})");

		public void DrawText(FontRef Fn, string Text, Vector2 Pos) => DrawCalls.Add($"DrawText(\"{Text}\", {Pos})");
		public void DrawTextColor(FontRef Fn, string Text, Vector2 Pos, FishColor Color) => DrawCalls.Add($"DrawTextColor(\"{Text}\", {Pos})");
		public void DrawTextColorScale(FontRef Fn, string Text, Vector2 Pos, FishColor Color, float Scale) => DrawCalls.Add($"DrawTextColorScale(\"{Text}\", {Pos}, scale={Scale})");

		public void SetImageFilter(ImageRef Img, bool pixelated) { }
		public void DrawCircle(Vector2 Center, float Radius, FishColor Color) => DrawCalls.Add($"DrawCircle({Center}, r={Radius})");
		public void DrawCircleOutline(Vector2 Center, float Radius, FishColor Color, float Thickness = 1f) => DrawCalls.Add($"DrawCircleOutline({Center}, r={Radius})");

		public void Reset()
		{
			DrawCalls.Clear();
			BeginDrawingCount = 0;
			EndDrawingCount = 0;
		}
	}
}
