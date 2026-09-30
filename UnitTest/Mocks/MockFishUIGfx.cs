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
        public virtual void BeginDrawing(float Dt) => BeginDrawingCount++;
        public virtual void EndDrawing() => EndDrawingCount++;

        public int GetWindowWidth() => WindowWidth;
        public int GetWindowHeight() => WindowHeight;
        public void FocusWindow() { }

        public void BeginScissor(Vector2 Pos, Vector2 Size) => DrawCalls.Add($"BeginScissor({Pos}, {Size})");
        public void EndScissor() => DrawCalls.Add("EndScissor");
        public void PushScissor(Vector2 Pos, Vector2 Size) => DrawCalls.Add($"PushScissor({Pos}, {Size})");
        public void PopScissor() => DrawCalls.Add("PopScissor");

        public FontRef LoadFont(string FileName, float Size, float Spacing, FishColor Color) => new FontRef(FileName, size: Size, spacing: Spacing, color: Color);
        public FontRef LoadFont(string FileName, float Size, float Spacing, FishColor Color, FontStyle Style) => new FontRef(FileName, size: Size, spacing: Spacing, color: Color, style: Style);
        public Func<string, Exception?>? ImageLoadFailure { get; set; }
        public ImageRef LoadImage(string FileName)
        {
            if (ImageLoadFailure?.Invoke(FileName) is Exception error) throw error;
            return new ImageRef(FileName, 32, 32);
        }
        public ImageRef LoadImage(string FileName, int X, int Y, int W, int H) => new ImageRef(FileName, W, H);
        public ImageRef LoadImage(ImageRef Orig, int X, int Y, int W, int H) => new ImageRef(Orig.Path, W, H);

        public FishColor GetImageColor(ImageRef Img, Vector2 Pos) => FishColor.White;
        public bool TryMeasureTextAdvances(FontRef font, string text, Span<float> advances, Span<float> leading)
        {
            for (int i = 0; i <= text.Length; i++) { advances[i] = i * 8; leading[i] = 0; }
            return true;
        }
        public Vector2 MeasureText(FontRef Fn, string Text) => new Vector2(Text?.Length * 8 ?? 0, 16);
        public FishUIFontMetrics GetFontMetrics(FontRef Fn) => new FishUIFontMetrics { LineHeight = 16, Ascent = 12, Descent = 4, Baseline = 12 };

        public void DrawLine(Vector2 Pos1, Vector2 Pos2, float Thick, FishColor Clr) => RecordDraw($"DrawLine({Pos1}, {Pos2})", Clr);
        public void DrawRectangle(Vector2 Position, Vector2 Size, FishColor Color) => RecordDraw($"DrawRectangle({Position}, {Size})", Color);
        public void DrawRectangleOutline(Vector2 Position, Vector2 Size, FishColor Color) => RecordDraw($"DrawRectangleOutline({Position}, {Size})", Color);

        public void DrawImage(ImageRef Img, Vector2 Pos, float Rot, float Scale, FishColor Color) => RecordDraw($"DrawImage({Pos})", Color);
        public void DrawImage(ImageRef Img, Vector2 Pos, Vector2 Size, float Rot, float Scale, FishColor Color) => RecordDraw($"DrawImage({Pos}, {Size})", Color);
        public void DrawNPatch(NPatch NP, Vector2 Pos, Vector2 Size, FishColor Color) => RecordDraw($"DrawNPatch({Pos}, {Size})", Color);
        public void DrawNPatch(NPatch NP, Vector2 Pos, Vector2 Size, FishColor Color, float Rotation) => RecordDraw($"DrawNPatch({Pos}, {Size}, rot={Rotation})", Color);

        public void DrawText(FontRef Fn, string Text, Vector2 Pos) => RecordDraw($"DrawText(\"{Text}\", {Pos})", Fn?.Color ?? FishColor.White);
        public void DrawTextColor(FontRef Fn, string Text, Vector2 Pos, FishColor Color) => RecordDraw($"DrawTextColor(\"{Text}\", {Pos})", Color);
        public void DrawTextColorScale(FontRef Fn, string Text, Vector2 Pos, FishColor Color, float Scale) => RecordDraw($"DrawTextColorScale(\"{Text}\", {Pos}, scale={Scale})", Color);

        public void SetImageFilter(ImageRef Img, bool pixelated) { }
        public void DrawCircle(Vector2 Center, float Radius, FishColor Color) => RecordDraw($"DrawCircle({Center}, r={Radius})", Color);
        public void DrawCircleOutline(Vector2 Center, float Radius, FishColor Color, float Thickness = 1f) => RecordDraw($"DrawCircleOutline({Center}, r={Radius})", Color);

        public List<FishColor> DrawColors { get; } = new();
        private void RecordDraw(string call, FishColor color) { DrawCalls.Add(call); DrawColors.Add(color); }

        public void Reset()
        {
            DrawCalls.Clear();
            DrawColors.Clear();
            BeginDrawingCount = 0;
            EndDrawingCount = 0;
        }
    }
}
