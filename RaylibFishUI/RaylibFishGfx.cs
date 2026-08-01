using FishUI;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace RaylibFishGfx
{
    /// <summary>
    /// Raylib graphics backend for FishUI. Implements all IFishUIGfx methods using Raylib-cs.
    /// </summary>
    /// <remarks>
    /// This is a complete, production-ready Raylib backend that demonstrates how to implement
    /// SimpleFishUIGfx with all optional overrides for maximum performance.
    /// </remarks>
    public class RaylibFishGfx : SimpleFishUIGfx, IFishUIFramebufferProvider, IDisposable
    {
        private readonly int _initialWidth;
        private readonly int _initialHeight;
        private readonly string _title;
        private readonly Dictionary<string, ImageRef> _imageCache = new Dictionary<string, ImageRef>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Font> _fontCache = new Dictionary<string, Font>(StringComparer.OrdinalIgnoreCase);
        private readonly List<Texture2D> _ownedTextures = new List<Texture2D>();
        private readonly List<Image> _ownedImages = new List<Image>();
        private readonly List<Font> _ownedFonts = new List<Font>();
        private bool _initialized;
        private bool _disposed;

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
            ThrowIfDisposed();
            if (_initialized) return;
            Raylib.SetTraceLogLevel(TraceLogLevel.None);
            Raylib.SetWindowState(ConfigFlags.HighDpiWindow);
            Raylib.SetWindowState(ConfigFlags.Msaa4xHint);
            Raylib.SetWindowState(ConfigFlags.ResizableWindow);
            Raylib.InitWindow(_initialWidth, _initialHeight, _title);
            Raylib.SetTargetFPS(Raylib.GetMonitorRefreshRate(0));
            _initialized = true;
        }

        /// <inheritdoc/>
        public override int GetWindowWidth() => Raylib.GetScreenWidth();

        /// <inheritdoc/>
        public override int GetWindowHeight() => Raylib.GetScreenHeight();

        /// <inheritdoc/>
        public override void FocusWindow() => Raylib.SetWindowFocused();

        #endregion

        #region Resource Loading

        /// <inheritdoc/>
        public override ImageRef LoadImage(string FileName)
        {
            return LoadImageCore(FileName, null);
        }

        /// <inheritdoc/>
        public override ImageRef LoadImage(string FileName, int X, int Y, int W, int H)
        {
            return LoadImageCore(FileName, new Rectangle(X, Y, W, H));
        }

        /// <inheritdoc/>
        public override unsafe ImageRef LoadImage(ImageRef Orig, int X, int Y, int W, int H)
        {
            ThrowIfDisposed();
            if (Orig == null) throw new ArgumentNullException(nameof(Orig));
            string path = NormalizePath(Orig.Path);
            string key = ImageKey(path, X, Y, W, H, Orig);
            if (_imageCache.TryGetValue(key, out ImageRef? cached)) return cached;
            Image image = default;
            Texture2D texture = default;
            try
            {
                image = Raylib.ImageFromImage((Image)Orig.Userdata2, new Rectangle(X, Y, W, H));
                if (image.Data == null || image.Width <= 0 || image.Height <= 0)
                    throw new InvalidOperationException("Raylib failed to create an image region.");
                texture = Raylib.LoadTextureFromImage(image);
                if (texture.Id == 0) throw new InvalidOperationException("Raylib failed to create a texture for an image region.");
                Raylib.SetTextureFilter(texture, TextureFilter.Trilinear);
                return TrackImage(key, path, image, texture);
            }
            catch
            {
                if (texture.Id != 0) Raylib.UnloadTexture(texture);
                if (image.Data != null) Raylib.UnloadImage(image);
                throw;
            }
        }

        /// <inheritdoc/>
        public override FontRef LoadFont(string FileName, float Size, float Spacing, FishColor Color)
        {
            return LoadFont(FileName, Size, Spacing, Color, FontStyle.Regular);
        }

        /// <inheritdoc/>
        public override FontRef LoadFont(string FileName, float Size, float Spacing, FishColor Color, FontStyle Style)
        {
            ThrowIfDisposed();
            string path = NormalizePath(FileName);
            string key = string.Join("|", path, Size.ToString("R", CultureInfo.InvariantCulture),
                Spacing.ToString("R", CultureInfo.InvariantCulture), ((int)Style).ToString(CultureInfo.InvariantCulture));
            if (!_fontCache.TryGetValue(key, out Font font))
            {
                font = Raylib.LoadFontEx(FileName, (int)Size, null, 250);
                if (font.Texture.Id == 0) throw new InvalidOperationException("Raylib failed to load a font.");
                _fontCache.Add(key, font);
                _ownedFonts.Add(font);
            }

            // Check if monospaced
            Vector2 wWidth = Raylib.MeasureTextEx(font, "W", Size, Spacing);
            Vector2 iWidth = Raylib.MeasureTextEx(font, "i", Size, Spacing);

            return new FontRef(path, font, Size, Spacing, Color, Style,
                Math.Abs(wWidth.X - iWidth.X) < 0.5f, font.BaseSize);
        }

        private unsafe ImageRef LoadImageCore(string fileName, Rectangle? crop)
        {
            ThrowIfDisposed();
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("An image path is required.", nameof(fileName));
            string path = NormalizePath(fileName);
            string key = crop.HasValue
                ? string.Join("|", path, crop.Value.X, crop.Value.Y, crop.Value.Width, crop.Value.Height)
                : path + "|full";
            if (_imageCache.TryGetValue(key, out ImageRef? cached)) return cached;

            Image image = default;
            Texture2D texture = default;
            try
            {
                image = Raylib.LoadImage(fileName);
                if (image.Data == null || image.Width <= 0 || image.Height <= 0)
                    throw new InvalidOperationException("Raylib failed to load an image.");
                if (crop.HasValue) Raylib.ImageCrop(ref image, crop.Value);
                if (image.Data == null || image.Width <= 0 || image.Height <= 0)
                    throw new InvalidOperationException("Raylib produced an invalid cropped image.");
                texture = Raylib.LoadTextureFromImage(image);
                if (texture.Id == 0) throw new InvalidOperationException("Raylib failed to create an image texture.");
                Raylib.SetTextureFilter(texture, TextureFilter.Trilinear);
                return TrackImage(key, path, image, texture);
            }
            catch
            {
                if (texture.Id != 0) Raylib.UnloadTexture(texture);
                if (image.Data != null) Raylib.UnloadImage(image);
                throw;
            }
        }

        private ImageRef TrackImage(string key, string path, Image image, Texture2D texture)
        {
            ImageRef result = new ImageRef(path, texture.Width, texture.Height, texture, image);
            _imageCache.Add(key, result);
            _ownedImages.Add(image);
            _ownedTextures.Add(texture);
            return result;
        }

        private static string ImageKey(string path, int x, int y, int width, int height, ImageRef original)
        {
            return string.Join("|", path, "region", RuntimeHelpers.GetHashCode(original), x, y, width, height);
        }

        private static string NormalizePath(string path) => string.IsNullOrWhiteSpace(path) ? string.Empty : Path.GetFullPath(path);

        /// <inheritdoc/>
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

        #region Scissor Clipping

        /// <inheritdoc/>
        public override void BeginScissor(Vector2 Pos, Vector2 Size)
        {
            Raylib.BeginScissorMode((int)Pos.X, (int)Pos.Y, (int)Size.X, (int)Size.Y);
        }

        /// <inheritdoc/>
        public override void EndScissor()
        {
            Raylib.EndScissorMode();
        }

        #endregion

        #region Primitive Drawing

        /// <inheritdoc/>
        public override void DrawLine(Vector2 Pos1, Vector2 Pos2, float Thick, FishColor Clr)
        {
            Raylib.DrawLineEx(Pos1, Pos2, Thick, new Color(Clr.R, Clr.G, Clr.B, Clr.A));
        }

        /// <inheritdoc/>
        public override void DrawRectangle(Vector2 Position, Vector2 Size, FishColor Color)
        {
            Raylib.DrawRectangleV(Position, Size, new Color(Color.R, Color.G, Color.B, Color.A));
        }

        /// <inheritdoc/>
        public override void DrawRectangleOutline(Vector2 Position, Vector2 Size, FishColor Color)
        {
            Raylib.DrawRectangleLinesEx(new Rectangle(Position, Size), 1, new Color(Color.R, Color.G, Color.B, Color.A));
        }

        /// <inheritdoc/>
        public override void DrawCircle(Vector2 Center, float Radius, FishColor Color)
        {
            Raylib.DrawCircleV(Center, Radius, new Color(Color.R, Color.G, Color.B, Color.A));
        }

        /// <inheritdoc/>
        public override void DrawCircleOutline(Vector2 Center, float Radius, FishColor Color, float Thickness = 1)
        {
            Color c = new Color(Color.R, Color.G, Color.B, Color.A);
            for (float r = Radius - Thickness / 2; r <= Radius + Thickness / 2; r += 0.5f)
            {
                Raylib.DrawCircleLinesV(Center, r, c);
            }
        }

        #endregion

        #region Image Drawing

        /// <inheritdoc/>
        public override void DrawImage(ImageRef Img, Vector2 Pos, float Rot, float Scale, FishColor Color)
        {
            Texture2D tex = (Texture2D)Img.Userdata;
            Raylib.DrawTextureEx(tex, Pos, Rot, Scale, new Color(Color.R, Color.G, Color.B, Color.A));
        }

        /// <inheritdoc/>
        public override void DrawImage(ImageRef Img, Vector2 Pos, Vector2 Size, float Rot, float Scale, FishColor Color)
        {
            Texture2D tex = (Texture2D)Img.Userdata;
            Rectangle src = new Rectangle(0, 0, tex.Width, tex.Height);
            Rectangle dest = new Rectangle(Pos, Size * Scale);
            Raylib.DrawTexturePro(tex, src, dest, Vector2.Zero, Rot, new Color(Color.R, Color.G, Color.B, Color.A));
        }

        /// <inheritdoc/>
        public override void DrawNPatch(NPatch NP, Vector2 Pos, Vector2 Size, FishColor Color)
        {
            DrawNPatch(NP, Pos, Size, Color, 0);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override void SetImageFilter(ImageRef Img, bool pixelated)
        {
            if (Img?.Userdata == null) return;
            Texture2D tex = (Texture2D)Img.Userdata;
            Raylib.SetTextureFilter(tex, pixelated ? TextureFilter.Point : TextureFilter.Trilinear);
        }

        /// <inheritdoc/>
        public override FishColor GetImageColor(ImageRef Img, Vector2 Pos)
        {
            Color c = Raylib.GetImageColor((Image)Img.Userdata2, (int)Pos.X, (int)Pos.Y);
            return new FishColor(c.R, c.G, c.B, c.A);
        }

        /// <inheritdoc/>
        protected override void DrawImageRegion(ImageRef Img, Vector2 srcPos, Vector2 srcSize, Vector2 destPos, Vector2 destSize, FishColor Color)
        {
            Texture2D tex = (Texture2D)Img.Userdata;
            Rectangle src = new Rectangle(srcPos, srcSize);
            Rectangle dest = new Rectangle(destPos, destSize);
            Raylib.DrawTexturePro(tex, src, dest, Vector2.Zero, 0, ToColor(Color));
        }

        #endregion

        #region Text Rendering

        /// <inheritdoc/>
        public override Vector2 MeasureText(FontRef Fn, string Text)
        {
            Font font = (Font)Fn.Userdata;
            return Raylib.MeasureTextEx(font, Text, Fn.Size, Fn.Spacing);
        }

        /// <inheritdoc/>
        public override void DrawText(FontRef Fn, string Text, Vector2 Pos)
        {
            Font font = (Font)Fn.Userdata;
            Raylib.DrawTextEx(font, Text, Round(Pos), Fn.Size, Fn.Spacing,
                new Color(Fn.Color.R, Fn.Color.G, Fn.Color.B, Fn.Color.A));
        }

        /// <inheritdoc/>
        public override void DrawTextColor(FontRef Fn, string Text, Vector2 Pos, FishColor Color)
        {
            Font font = (Font)Fn.Userdata;
            Raylib.DrawTextEx(font, Text, Round(Pos), Fn.Size, Fn.Spacing,
                new Color(Color.R, Color.G, Color.B, Color.A));
        }

        /// <inheritdoc/>
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

        /// <summary>
        /// Flushes Raylib's active batch and copies the current physical framebuffer without presenting it.
        /// </summary>
        public unsafe bool TryCaptureFramebuffer(out FishUIFramebuffer framebuffer)
        {
            framebuffer = null!;
            Rlgl.DrawRenderBatchActive();
            Image image = Raylib.LoadImageFromScreen();
            try
            {
                if (image.Data == null || image.Width <= 0 || image.Height <= 0)
                    return false;
                if (image.Format != PixelFormat.UncompressedR8G8B8A8)
                {
                    Raylib.ImageFormat(ref image, PixelFormat.UncompressedR8G8B8A8);
                    if (image.Data == null || image.Width <= 0 || image.Height <= 0 ||
                        image.Format != PixelFormat.UncompressedR8G8B8A8)
                        return false;
                }
                int rowStride = checked(image.Width * 4);
                int byteCount = checked(rowStride * image.Height);
                byte[] pixels = new byte[byteCount];
                new ReadOnlySpan<byte>(image.Data, byteCount).CopyTo(pixels);
                pixels = ExpandLogicalHighDpiViewport(pixels, image.Width, image.Height);
                framebuffer = new FishUIFramebuffer(image.Width, image.Height, rowStride,
                    FishUIPixelOrigin.TopLeft, false, pixels);
                return true;
            }
            finally
            {
                if (image.Data != null)
                    Raylib.UnloadImage(image);
            }
        }

        private static byte[] ExpandLogicalHighDpiViewport(byte[] pixels, int physicalWidth, int physicalHeight)
        {
            int logicalWidth = Raylib.GetScreenWidth();
            int logicalHeight = Raylib.GetScreenHeight();
            Vector2 dpiScale = Raylib.GetWindowScaleDPI();
            if (logicalWidth <= 0 || logicalHeight <= 0 || physicalWidth < logicalWidth || physicalHeight < logicalHeight ||
                physicalWidth == logicalWidth && physicalHeight == logicalHeight)
                return pixels;
            int expectedWidth = (int)MathF.Round(logicalWidth * dpiScale.X);
            int expectedHeight = (int)MathF.Round(logicalHeight * dpiScale.Y);
            if (expectedWidth != physicalWidth || expectedHeight != physicalHeight)
                return pixels;

            // Raylib 5.5 reads the DPI-sized buffer while the 2D viewport remains logical-sized.
            // After rlReadScreenPixels fixes the GL origin, that viewport is bottom-aligned.
            int sourceTop = physicalHeight - logicalHeight;
            byte[] expanded = new byte[pixels.Length];
            for (int y = 0; y < physicalHeight; y++)
            {
                int sourceY = sourceTop + Math.Min(logicalHeight - 1,
                    (int)((long)y * logicalHeight / physicalHeight));
                for (int x = 0; x < physicalWidth; x++)
                {
                    int sourceX = Math.Min(logicalWidth - 1,
                        (int)((long)x * logicalWidth / physicalWidth));
                    int sourceOffset = (sourceY * physicalWidth + sourceX) * 4;
                    int targetOffset = (y * physicalWidth + x) * 4;
                    expanded[targetOffset] = pixels[sourceOffset];
                    expanded[targetOffset + 1] = pixels[sourceOffset + 1];
                    expanded[targetOffset + 2] = pixels[sourceOffset + 2];
                    expanded[targetOffset + 3] = pixels[sourceOffset + 3];
                }
            }
            return expanded;
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

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RaylibFishGfx));
        }

        /// <summary>Unloads every Raylib resource owned by this backend exactly once.</summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            for (int i = _ownedFonts.Count - 1; i >= 0; i--) Raylib.UnloadFont(_ownedFonts[i]);
            for (int i = _ownedTextures.Count - 1; i >= 0; i--) Raylib.UnloadTexture(_ownedTextures[i]);
            for (int i = _ownedImages.Count - 1; i >= 0; i--) Raylib.UnloadImage(_ownedImages[i]);
            _ownedFonts.Clear();
            _ownedTextures.Clear();
            _ownedImages.Clear();
            _fontCache.Clear();
            _imageCache.Clear();
        }

        #endregion
    }
}
