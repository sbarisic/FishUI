using System;
using System.Collections.Generic;
using System.Numerics;

namespace FishUI
{
    public sealed class FishUIFramebuffer : IDisposable
    {
        private readonly Action _release;
        private bool _disposed;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int RowStrideBytes { get; private set; }
        public FishUIPixelOrigin Origin { get; private set; }
        public bool PremultipliedAlpha { get; private set; }
        public ReadOnlyMemory<byte> Rgba32 { get; private set; }

        public FishUIFramebuffer(int width, int height, int rowStrideBytes, FishUIPixelOrigin origin,
            bool premultipliedAlpha, ReadOnlyMemory<byte> rgba32, Action release = null)
        {
            Width = width; Height = height; RowStrideBytes = rowStrideBytes; Origin = origin;
            PremultipliedAlpha = premultipliedAlpha; Rgba32 = rgba32; _release = release;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _release?.Invoke();
        }
    }

    public interface IFishUIFramebufferProvider
    {
        bool TryCaptureFramebuffer(out FishUIFramebuffer framebuffer);
    }

    internal sealed class FishUIRenderRecorder
    {
        private struct ScissorState
        {
            internal FishUIDebugRect EffectiveClip;
            internal bool DirectScissor;
        }

        private readonly FishUIDiagnosticsSession _session;
        private readonly int _maximum;
        private readonly Stack<ScissorState> _savedClips = new Stack<ScissorState>();
        private FishUIDebugRect _effectiveClip;
        private FishUIDebugRect _clipBeforeDirect;
        private bool _directScissor;
        private long _sequence;

        internal List<FishUIGraphicsCall> Calls { get; } = new List<FishUIGraphicsCall>();
        internal List<FishUIDiagnosticWarning> Warnings { get; } = new List<FishUIDiagnosticWarning>();
        internal int TruncatedCount { get; private set; }
        internal Dictionary<FishUIGraphicsCallCategory, int> TruncatedByCategory { get; } = new Dictionary<FishUIGraphicsCallCategory, int>();

        internal FishUIRenderRecorder(FishUIDiagnosticsSession session, int maximum)
        {
            _session = session;
            _maximum = Math.Max(0, maximum);
        }

        internal void Record(FishUIGraphicsCallCategory category, string operation, FishUIDebugRect bounds = null,
            string asset = null, string text = null)
        {
            long sequence = ++_sequence;
            if (Calls.Count >= _maximum)
            {
                TruncatedCount++;
                TruncatedByCategory.TryGetValue(category, out int count);
                TruncatedByCategory[category] = count + 1;
                return;
            }
            var owner = _session.CurrentRenderOwner;
            var call = new FishUIGraphicsCall
            {
                Sequence = sequence,
                Frame = _session.Frame,
                Category = category,
                Operation = operation,
                ControlId = owner.ControlId,
                Owner = owner.Owner,
                Semantic = owner.Semantic,
                BoundsPixels = bounds,
                EffectiveClipPixels = Clone(_effectiveClip),
                Asset = _session.CollectCaptureText(asset),
                TextLength = text?.Length
            };
            if (text != null && _session.ShouldCollectTextPreview)
                call.TextPreview = _session.CollectCaptureText(text);
            Calls.Add(call);

            if (bounds != null && (!IsFinite(bounds) || bounds.Width < 0 || bounds.Height < 0))
                Warn("NON_FINITE_GEOMETRY", "Graphics call contains non-finite or negative geometry.", sequence, owner.ControlId);
        }

        internal void BeginScissor(FishUIDebugRect requested)
        {
            if (_directScissor)
                Warn("INVALID_SCISSOR_NESTING", "BeginScissor replaced an active direct scissor.", _sequence + 1, _session.CurrentRenderOwner.ControlId);
            else
                _clipBeforeDirect = Clone(_effectiveClip);
            _directScissor = true;
            _effectiveClip = requested;
            Record(FishUIGraphicsCallCategory.Scissor, "BeginScissor", requested);
        }

        internal FishUIDebugRect EndScissor()
        {
            if (!_directScissor)
            {
                Warn("UNMATCHED_END_SCISSOR", "EndScissor had no matching direct scissor.", _sequence + 1, _session.CurrentRenderOwner.ControlId);
                Record(FishUIGraphicsCallCategory.Scissor, "EndScissor");
                return Clone(_effectiveClip);
            }
            _directScissor = false;
            Record(FishUIGraphicsCallCategory.Scissor, "EndScissor");
            _effectiveClip = _clipBeforeDirect;
            _clipBeforeDirect = null;
            return Clone(_effectiveClip);
        }

        internal void PushScissor(FishUIDebugRect requested)
        {
            _savedClips.Push(new ScissorState { EffectiveClip = Clone(_effectiveClip), DirectScissor = _directScissor });
            _effectiveClip = FishUIDebugRect.Intersect(_effectiveClip, requested);
            Record(FishUIGraphicsCallCategory.Scissor, "PushScissor", requested);
        }

        internal void PopScissor()
        {
            if (_savedClips.Count == 0)
            {
                Warn("SCISSOR_STACK_UNDERFLOW", "PopScissor had no matching push.", _sequence + 1, _session.CurrentRenderOwner.ControlId);
                Record(FishUIGraphicsCallCategory.Scissor, "PopScissor");
                return;
            }
            Record(FishUIGraphicsCallCategory.Scissor, "PopScissor");
            ScissorState state = _savedClips.Pop();
            _effectiveClip = state.EffectiveClip;
            _directScissor = state.DirectScissor;
        }

        internal void Complete()
        {
            if (_savedClips.Count != 0 || _directScissor)
                Warn("UNBALANCED_SCISSOR_STACK", $"Frame ended with {_savedClips.Count} pushed clips and directScissor={_directScissor}.", null, null);
        }

        private void Warn(string code, string message, long? call, long? control)
        {
            Warnings.Add(new FishUIDiagnosticWarning
            {
                Severity = FishUIDiagnosticSeverity.Warning,
                Code = code,
                Message = message,
                UiSessionId = _session.UiSessionId,
                ControlId = control,
                GraphicsSequence = call
            });
        }

        private static bool IsFinite(FishUIDebugRect rect) =>
            float.IsFinite(rect.X) && float.IsFinite(rect.Y) && float.IsFinite(rect.Width) && float.IsFinite(rect.Height);
        private static FishUIDebugRect Clone(FishUIDebugRect rect) => rect == null ? null : new FishUIDebugRect(rect.X, rect.Y, rect.Width, rect.Height);
    }

    public sealed class RecordingFishUIGfx : IFishUIGfx, IFishUIFramebufferProvider
    {
        private readonly IFishUIGfx _inner;
        private readonly FishUIRenderRecorder _recorder;
        internal RecordingFishUIGfx(IFishUIGfx inner, FishUIRenderRecorder recorder) { _inner = inner; _recorder = recorder; }
        internal bool HasFramebufferProvider => _inner is IFishUIFramebufferProvider;
        public void Init() { _recorder.Record(FishUIGraphicsCallCategory.Resource, "Init"); _inner.Init(); }
        public void BeginDrawing(float Dt) { _recorder.Record(FishUIGraphicsCallCategory.GraphicsState, "BeginDrawing"); _inner.BeginDrawing(Dt); }
        public void EndDrawing() { _recorder.Record(FishUIGraphicsCallCategory.GraphicsState, "EndDrawing"); _inner.EndDrawing(); }
        public void BeginScissor(Vector2 Pos, Vector2 Size) { _recorder.BeginScissor(Rect(Pos, Size)); _inner.BeginScissor(Pos, Size); }
        public void EndScissor()
        {
            FishUIDebugRect restored = _recorder.EndScissor();
            _inner.EndScissor();
            if (restored != null)
                _inner.BeginScissor(new Vector2(restored.X, restored.Y), new Vector2(restored.Width, restored.Height));
        }
        public void PushScissor(Vector2 Pos, Vector2 Size) { _recorder.PushScissor(Rect(Pos, Size)); _inner.PushScissor(Pos, Size); }
        public void PopScissor() { _recorder.PopScissor(); _inner.PopScissor(); }
        public int GetWindowWidth() { _recorder.Record(FishUIGraphicsCallCategory.Measurement, "GetWindowWidth"); return _inner.GetWindowWidth(); }
        public int GetWindowHeight() { _recorder.Record(FishUIGraphicsCallCategory.Measurement, "GetWindowHeight"); return _inner.GetWindowHeight(); }
        public void FocusWindow() { _recorder.Record(FishUIGraphicsCallCategory.GraphicsState, "FocusWindow"); _inner.FocusWindow(); }
        public FontRef LoadFont(string FileName, float Size, float Spacing, FishColor Color) { _recorder.Record(FishUIGraphicsCallCategory.Resource, "LoadFont", asset: FileName); return _inner.LoadFont(FileName, Size, Spacing, Color); }
        public FontRef LoadFont(string FileName, float Size, float Spacing, FishColor Color, FontStyle Style) { _recorder.Record(FishUIGraphicsCallCategory.Resource, "LoadFont", asset: FileName); return _inner.LoadFont(FileName, Size, Spacing, Color, Style); }
        public ImageRef LoadImage(string FileName) { _recorder.Record(FishUIGraphicsCallCategory.Resource, "LoadImage", asset: FileName); return _inner.LoadImage(FileName); }
        public ImageRef LoadImage(string FileName, int X, int Y, int W, int H) { _recorder.Record(FishUIGraphicsCallCategory.Resource, "LoadImageRegion", new FishUIDebugRect(X, Y, W, H), FileName); return _inner.LoadImage(FileName, X, Y, W, H); }
        public ImageRef LoadImage(ImageRef Orig, int X, int Y, int W, int H) { _recorder.Record(FishUIGraphicsCallCategory.Resource, "LoadImageRegion", new FishUIDebugRect(X, Y, W, H), Orig?.Path); return _inner.LoadImage(Orig, X, Y, W, H); }
        public FishColor GetImageColor(ImageRef Img, Vector2 Pos) { _recorder.Record(FishUIGraphicsCallCategory.Measurement, "GetImageColor", new FishUIDebugRect(Pos.X, Pos.Y, 1, 1), Img?.Path); return _inner.GetImageColor(Img, Pos); }
        public bool TryMeasureTextAdvances(FontRef font, string text, Span<float> advances, Span<float> leading) => _inner.TryMeasureTextAdvances(font, text, advances, leading);
        public long GetTextMetricsVersion(FontRef font) => _inner.GetTextMetricsVersion(font);
        public Vector2 MeasureText(FontRef Fn, string Text) { _recorder.Record(FishUIGraphicsCallCategory.Measurement, "MeasureText", asset: Fn?.Path, text: Text); return _inner.MeasureText(Fn, Text); }
        public FishUIFontMetrics GetFontMetrics(FontRef Fn) { _recorder.Record(FishUIGraphicsCallCategory.Measurement, "GetFontMetrics", asset: Fn?.Path); return _inner.GetFontMetrics(Fn); }
        public void DrawLine(Vector2 Pos1, Vector2 Pos2, float Thick, FishColor Clr) { _recorder.Record(FishUIGraphicsCallCategory.Rendering, "DrawLine", Bounds(Pos1, Pos2, Thick)); _inner.DrawLine(Pos1, Pos2, Thick, Clr); }
        public void DrawRectangle(Vector2 Position, Vector2 Size, FishColor Color) { _recorder.Record(FishUIGraphicsCallCategory.Rendering, "DrawRectangle", Rect(Position, Size)); _inner.DrawRectangle(Position, Size, Color); }
        public void DrawRectangleOutline(Vector2 Position, Vector2 Size, FishColor Color) { _recorder.Record(FishUIGraphicsCallCategory.Rendering, "DrawRectangleOutline", Rect(Position, Size)); _inner.DrawRectangleOutline(Position, Size, Color); }
        public void DrawImage(ImageRef Img, Vector2 Pos, float Rot, float Scale, FishColor Color) { var size = new Vector2((Img?.Width ?? 0) * Scale, (Img?.Height ?? 0) * Scale); _recorder.Record(FishUIGraphicsCallCategory.Rendering, "DrawImage", Rect(Pos, size), Img?.Path); _inner.DrawImage(Img, Pos, Rot, Scale, Color); }
        public void DrawImage(ImageRef Img, Vector2 Pos, Vector2 Size, float Rot, float Scale, FishColor Color) { _recorder.Record(FishUIGraphicsCallCategory.Rendering, "DrawImage", Rect(Pos, Size), Img?.Path); _inner.DrawImage(Img, Pos, Size, Rot, Scale, Color); }
        public void DrawNPatch(NPatch NP, Vector2 Pos, Vector2 Size, FishColor Color) { _recorder.Record(FishUIGraphicsCallCategory.Rendering, "DrawNPatch", Rect(Pos, Size), NP?.Image?.Path); _inner.DrawNPatch(NP, Pos, Size, Color); }
        public void DrawNPatch(NPatch NP, Vector2 Pos, Vector2 Size, FishColor Color, float Rotation) { _recorder.Record(FishUIGraphicsCallCategory.Rendering, "DrawNPatch", Rect(Pos, Size), NP?.Image?.Path); _inner.DrawNPatch(NP, Pos, Size, Color, Rotation); }
        public void DrawText(FontRef Fn, string Text, Vector2 Pos) { _recorder.Record(FishUIGraphicsCallCategory.Rendering, "DrawText", new FishUIDebugRect(Pos.X, Pos.Y, 0, 0), Fn?.Path, Text); _inner.DrawText(Fn, Text, Pos); }
        public void DrawTextColor(FontRef Fn, string Text, Vector2 Pos, FishColor Color) { _recorder.Record(FishUIGraphicsCallCategory.Rendering, "DrawTextColor", new FishUIDebugRect(Pos.X, Pos.Y, 0, 0), Fn?.Path, Text); _inner.DrawTextColor(Fn, Text, Pos, Color); }
        public void DrawTextColorScale(FontRef Fn, string Text, Vector2 Pos, FishColor Color, float Scale) { _recorder.Record(FishUIGraphicsCallCategory.Rendering, "DrawTextColorScale", new FishUIDebugRect(Pos.X, Pos.Y, 0, 0), Fn?.Path, Text); _inner.DrawTextColorScale(Fn, Text, Pos, Color, Scale); }
        public void SetImageFilter(ImageRef Img, bool pixelated) { _recorder.Record(FishUIGraphicsCallCategory.GraphicsState, "SetImageFilter", asset: Img?.Path); _inner.SetImageFilter(Img, pixelated); }
        public void DrawCircle(Vector2 Center, float Radius, FishColor Color) { _recorder.Record(FishUIGraphicsCallCategory.Rendering, "DrawCircle", new FishUIDebugRect(Center.X - Radius, Center.Y - Radius, Radius * 2, Radius * 2)); _inner.DrawCircle(Center, Radius, Color); }
        public void DrawCircleOutline(Vector2 Center, float Radius, FishColor Color, float Thickness = 1f) { _recorder.Record(FishUIGraphicsCallCategory.Rendering, "DrawCircleOutline", new FishUIDebugRect(Center.X - Radius, Center.Y - Radius, Radius * 2, Radius * 2)); _inner.DrawCircleOutline(Center, Radius, Color, Thickness); }
        public bool TryCaptureFramebuffer(out FishUIFramebuffer framebuffer)
        {
            if (_inner is IFishUIFramebufferProvider provider) return provider.TryCaptureFramebuffer(out framebuffer);
            framebuffer = null; return false;
        }
        private static FishUIDebugRect Rect(Vector2 pos, Vector2 size) => new FishUIDebugRect(pos.X, pos.Y, size.X, size.Y);
        private static FishUIDebugRect Bounds(Vector2 a, Vector2 b, float thick) => new FishUIDebugRect(Math.Min(a.X, b.X) - thick / 2, Math.Min(a.Y, b.Y) - thick / 2, Math.Abs(a.X - b.X) + thick, Math.Abs(a.Y - b.Y) + thick);
    }
}
