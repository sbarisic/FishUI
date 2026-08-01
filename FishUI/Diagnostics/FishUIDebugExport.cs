using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FishUI
{
    internal static class FishUIDiagnosticJson
    {
        internal static JsonSerializerOptions Options { get; } = Create();
        private static JsonSerializerOptions Create()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            return options;
        }
    }

    internal static class FishUIDebugExport
    {
        internal static void SaveDirectory(FishUIDebugSnapshot snapshot, string path, bool overwrite)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("An export path is required.", nameof(path));
            string full = Path.GetFullPath(path);
            if (Directory.Exists(full) && !overwrite) throw new IOException($"Directory already exists: {full}");
            string parent = Path.GetDirectoryName(full) ?? Directory.GetCurrentDirectory();
            Directory.CreateDirectory(parent);
            string temporary = Path.Combine(parent, "." + Path.GetFileName(full) + ".tmp-" + Guid.NewGuid().ToString("N"));
            try
            {
                Directory.CreateDirectory(temporary);
                WriteFiles(snapshot, temporary);
                if (Directory.Exists(full)) Directory.Delete(full, true);
                Directory.Move(temporary, full);
            }
            catch
            {
                if (Directory.Exists(temporary)) Directory.Delete(temporary, true);
                throw;
            }
        }

        internal static void SaveZip(FishUIDebugSnapshot snapshot, string path, bool overwrite)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            string full = Path.GetFullPath(path);
            if (File.Exists(full) && !overwrite) throw new IOException($"File already exists: {full}");
            string parent = Path.GetDirectoryName(full) ?? Directory.GetCurrentDirectory();
            Directory.CreateDirectory(parent);
            string temporary = Path.Combine(parent, "." + Path.GetFileName(full) + ".tmp-" + Guid.NewGuid().ToString("N"));
            try
            {
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                using (var zip = new ZipArchive(stream, ZipArchiveMode.Create))
                {
                    WriteEntry(zip, "snapshot.json", JsonSerializer.SerializeToUtf8Bytes(snapshot, FishUIDiagnosticJson.Options));
                    if (snapshot.IncludesRecentEvents) WriteEntry(zip, "recent-events.json", JsonSerializer.SerializeToUtf8Bytes(new { paths = snapshot.Paths, events = snapshot.RecentEvents }, FishUIDiagnosticJson.Options));
                    if (snapshot.IncludesInteractionSummary) WriteEntry(zip, "interaction-summary.txt", Encoding.UTF8.GetBytes(snapshot.InteractionSummary ?? string.Empty));
                    if (snapshot.ScreenshotPng != null) WriteEntry(zip, "screenshot.png", snapshot.ScreenshotPng);
                    if (snapshot.OverlayPng != null) WriteEntry(zip, "overlay.png", snapshot.OverlayPng);
                }
                if (File.Exists(full)) File.Delete(full);
                File.Move(temporary, full);
            }
            catch
            {
                if (File.Exists(temporary)) File.Delete(temporary);
                throw;
            }
        }

        private static void WriteFiles(FishUIDebugSnapshot snapshot, string directory)
        {
            File.WriteAllBytes(Path.Combine(directory, "snapshot.json"), JsonSerializer.SerializeToUtf8Bytes(snapshot, FishUIDiagnosticJson.Options));
            if (snapshot.IncludesRecentEvents)
                File.WriteAllBytes(Path.Combine(directory, "recent-events.json"), JsonSerializer.SerializeToUtf8Bytes(new { paths = snapshot.Paths, events = snapshot.RecentEvents }, FishUIDiagnosticJson.Options));
            if (snapshot.IncludesInteractionSummary) File.WriteAllText(Path.Combine(directory, "interaction-summary.txt"), snapshot.InteractionSummary ?? string.Empty, Encoding.UTF8);
            if (snapshot.ScreenshotPng != null) File.WriteAllBytes(Path.Combine(directory, "screenshot.png"), snapshot.ScreenshotPng);
            if (snapshot.OverlayPng != null) File.WriteAllBytes(Path.Combine(directory, "overlay.png"), snapshot.OverlayPng);
        }

        private static void WriteEntry(ZipArchive zip, string name, byte[] data)
        {
            var entry = zip.CreateEntry(name, CompressionLevel.Optimal);
            using Stream target = entry.Open();
            target.Write(data, 0, data.Length);
        }
    }

    internal static class FishUIInteractionSummary
    {
        internal static string Create(IReadOnlyList<FishUIDiagnosticEvent> events,
            FishUIDebugCaptureReason reason, double requestedHistorySeconds,
            double actualHistorySeconds, bool truncatedByCapacity)
        {
            var text = new StringBuilder();
            text.Append("Captured by ").Append(reason == FishUIDebugCaptureReason.Hotkey
                ? "Ctrl+Shift+F12" : reason.ToString()).AppendLine(".");
            text.Append("Requested ").Append(requestedHistorySeconds.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture))
                .Append(" seconds of pre-trigger history; included ")
                .Append(actualHistorySeconds.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)).AppendLine(" seconds.");
            text.Append("Recorded ").Append(events?.Count ?? 0).AppendLine(" projected diagnostic events.");
            text.AppendLine(truncatedByCapacity
                ? "The rolling history was truncated by the event-capacity limit."
                : "The rolling history was not truncated by capacity.");
            if (events == null || events.Count == 0) return text.ToString();
            text.AppendLine();
            int omittedMoves = 0;
            for (int eventIndex = 0; eventIndex < events.Count; eventIndex++)
            {
                FishUIDiagnosticEvent record = events[eventIndex];
                if (record.Category == FishUIDiagnosticEventCategory.RawInput && record.Type == FishUIDiagnosticEventType.PointerState) continue;
                if (record.Type == FishUIDiagnosticEventType.MouseMoved)
                {
                    omittedMoves += Math.Max(1, record.Pointer?.SampleCount ?? 1);
                    continue;
                }
                if (record.Type == FishUIDiagnosticEventType.DragUpdated)
                {
                    int lastIndex = eventIndex;
                    int consumedThrough = eventIndex;
                    int samples = Math.Max(1, record.Pointer?.SampleCount ?? 1);
                    for (int scanIndex = eventIndex + 1; scanIndex < events.Count; scanIndex++)
                    {
                        FishUIDiagnosticEvent candidate = events[scanIndex];
                        if (candidate.Category == FishUIDiagnosticEventCategory.RawInput &&
                            candidate.Type == FishUIDiagnosticEventType.PointerState)
                        {
                            consumedThrough = scanIndex;
                            continue;
                        }
                        if (candidate.Type == FishUIDiagnosticEventType.MouseMoved)
                        {
                            omittedMoves += Math.Max(1, candidate.Pointer?.SampleCount ?? 1);
                            consumedThrough = scanIndex;
                            continue;
                        }
                        if (candidate.Type != FishUIDiagnosticEventType.DragUpdated ||
                            candidate.InteractionId != record.InteractionId ||
                            candidate.ControlId != record.ControlId)
                            break;
                        lastIndex = scanIndex;
                        consumedThrough = scanIndex;
                        samples += Math.Max(1, candidate.Pointer?.SampleCount ?? 1);
                    }
                    FishUIDiagnosticEvent last = events[lastIndex];
                    FishUIDebugPoint start = record.Pointer?.StartPositionPixels ?? record.Pointer?.PreviousPositionPixels;
                    FishUIDebugPoint end = last.Pointer?.PositionPixels;
                    FishUIDebugPoint delta = last.Pointer?.TotalDeltaPixels;
                    text.Append('#').Append(record.Sequence).Append("-").Append(last.Sequence)
                        .Append(" DragUpdated");
                    if (record.ControlId.HasValue) text.Append(" control ").Append(record.ControlId.Value);
                    text.Append(": duration=")
                        .Append(Math.Max(0, last.TimeSeconds - record.TimeSeconds).ToString("0.###", System.Globalization.CultureInfo.InvariantCulture))
                        .Append("s samples=").Append(samples);
                    if (start != null) text.Append(" start=(").Append(start.X).Append(',').Append(start.Y).Append(')');
                    if (end != null) text.Append(" end=(").Append(end.X).Append(',').Append(end.Y).Append(')');
                    if (delta != null) text.Append(" totalDelta=(").Append(delta.X).Append(',').Append(delta.Y).Append(')');
                    text.AppendLine();
                    eventIndex = consumedThrough;
                    continue;
                }
                text.Append('#').Append(record.Sequence).Append(' ').Append(record.Type);
                if (record.ControlId.HasValue) text.Append(" control ").Append(record.ControlId.Value);
                if (!string.IsNullOrEmpty(record.Message)) text.Append(": ").Append(record.Message);
                text.AppendLine();
            }
            if (omittedMoves > 0)
                text.Append("Omitted ").Append(omittedMoves).AppendLine(" routine mouse-movement samples; they remain in recent-events.json.");
            return text.ToString();
        }
    }

    internal static class FishUIDebugImage
    {
        private static readonly uint[] CrcTable = CreateCrcTable();
        private static readonly byte[] FontColumns =
        {
            0x00,0x00,0x00,0x00,0x00, 0x00,0x00,0x5f,0x00,0x00, 0x00,0x07,0x00,0x07,0x00, 0x14,0x7f,0x14,0x7f,0x14,
            0x24,0x2a,0x7f,0x2a,0x12, 0x23,0x13,0x08,0x64,0x62, 0x36,0x49,0x55,0x22,0x50, 0x00,0x05,0x03,0x00,0x00,
            0x00,0x1c,0x22,0x41,0x00, 0x00,0x41,0x22,0x1c,0x00, 0x14,0x08,0x3e,0x08,0x14, 0x08,0x08,0x3e,0x08,0x08,
            0x00,0x50,0x30,0x00,0x00, 0x08,0x08,0x08,0x08,0x08, 0x00,0x60,0x60,0x00,0x00, 0x20,0x10,0x08,0x04,0x02,
            0x3e,0x51,0x49,0x45,0x3e, 0x00,0x42,0x7f,0x40,0x00, 0x42,0x61,0x51,0x49,0x46, 0x21,0x41,0x45,0x4b,0x31,
            0x18,0x14,0x12,0x7f,0x10, 0x27,0x45,0x45,0x45,0x39, 0x3c,0x4a,0x49,0x49,0x30, 0x01,0x71,0x09,0x05,0x03,
            0x36,0x49,0x49,0x49,0x36, 0x06,0x49,0x49,0x29,0x1e, 0x00,0x36,0x36,0x00,0x00, 0x00,0x56,0x36,0x00,0x00,
            0x08,0x14,0x22,0x41,0x00, 0x14,0x14,0x14,0x14,0x14, 0x00,0x41,0x22,0x14,0x08, 0x02,0x01,0x51,0x09,0x06,
            0x32,0x49,0x79,0x41,0x3e, 0x7e,0x11,0x11,0x11,0x7e, 0x7f,0x49,0x49,0x49,0x36, 0x3e,0x41,0x41,0x41,0x22,
            0x7f,0x41,0x41,0x22,0x1c, 0x7f,0x49,0x49,0x49,0x41, 0x7f,0x09,0x09,0x09,0x01, 0x3e,0x41,0x49,0x49,0x7a,
            0x7f,0x08,0x08,0x08,0x7f, 0x00,0x41,0x7f,0x41,0x00, 0x20,0x40,0x41,0x3f,0x01, 0x7f,0x08,0x14,0x22,0x41,
            0x7f,0x40,0x40,0x40,0x40, 0x7f,0x02,0x0c,0x02,0x7f, 0x7f,0x04,0x08,0x10,0x7f, 0x3e,0x41,0x41,0x41,0x3e,
            0x7f,0x09,0x09,0x09,0x06, 0x3e,0x41,0x51,0x21,0x5e, 0x7f,0x09,0x19,0x29,0x46, 0x46,0x49,0x49,0x49,0x31,
            0x01,0x01,0x7f,0x01,0x01, 0x3f,0x40,0x40,0x40,0x3f, 0x1f,0x20,0x40,0x20,0x1f, 0x3f,0x40,0x38,0x40,0x3f,
            0x63,0x14,0x08,0x14,0x63, 0x07,0x08,0x70,0x08,0x07, 0x61,0x51,0x49,0x45,0x43, 0x00,0x7f,0x41,0x41,0x00,
            0x02,0x04,0x08,0x10,0x20, 0x00,0x41,0x41,0x7f,0x00, 0x04,0x02,0x01,0x02,0x04, 0x40,0x40,0x40,0x40,0x40,
            0x00,0x01,0x02,0x04,0x00, 0x20,0x54,0x54,0x54,0x78, 0x7f,0x48,0x44,0x44,0x38, 0x38,0x44,0x44,0x44,0x20,
            0x38,0x44,0x44,0x48,0x7f, 0x38,0x54,0x54,0x54,0x18, 0x08,0x7e,0x09,0x01,0x02, 0x0c,0x52,0x52,0x52,0x3e,
            0x7f,0x08,0x04,0x04,0x78, 0x00,0x44,0x7d,0x40,0x00, 0x20,0x40,0x44,0x3d,0x00, 0x7f,0x10,0x28,0x44,0x00,
            0x00,0x41,0x7f,0x40,0x00, 0x7c,0x04,0x18,0x04,0x78, 0x7c,0x08,0x04,0x04,0x78, 0x38,0x44,0x44,0x44,0x38,
            0x7c,0x14,0x14,0x14,0x08, 0x08,0x14,0x14,0x18,0x7c, 0x7c,0x08,0x04,0x04,0x08, 0x48,0x54,0x54,0x54,0x20,
            0x04,0x3f,0x44,0x40,0x20, 0x3c,0x40,0x40,0x20,0x7c, 0x1c,0x20,0x40,0x20,0x1c, 0x3c,0x40,0x30,0x40,0x3c,
            0x44,0x28,0x10,0x28,0x44, 0x0c,0x50,0x50,0x50,0x3c, 0x44,0x64,0x54,0x4c,0x44, 0x00,0x08,0x36,0x41,0x00,
            0x00,0x00,0x7f,0x00,0x00, 0x00,0x41,0x36,0x08,0x00, 0x10,0x08,0x08,0x10,0x08
        };

        internal static byte[] Normalize(FishUIFramebuffer source, int maximumWidth, int maximumHeight, long maximumBytes)
        {
            if (source.Width <= 0 || source.Height <= 0) throw new InvalidDataException("Framebuffer dimensions must be positive.");
            if (source.Width > maximumWidth || source.Height > maximumHeight) throw new InvalidDataException("Framebuffer dimensions exceed configured limits.");
            int rowBytes = checked(source.Width * 4);
            if (source.RowStrideBytes < rowBytes) throw new InvalidDataException("Framebuffer row stride is smaller than width * 4.");
            long required = checked((long)source.RowStrideBytes * source.Height);
            long decoded = checked((long)rowBytes * source.Height);
            if (required > source.Rgba32.Length) throw new InvalidDataException("Framebuffer memory is truncated.");
            if (required > maximumBytes) throw new InvalidDataException("Framebuffer source byte count exceeds the configured limit.");
            if (decoded > maximumBytes) throw new InvalidDataException("Framebuffer decoded byte count exceeds the configured limit.");
            byte[] result = new byte[checked((int)decoded)];
            ReadOnlySpan<byte> input = source.Rgba32.Span;
            for (int y = 0; y < source.Height; y++)
            {
                int sourceY = source.Origin == FishUIPixelOrigin.TopLeft ? y : source.Height - 1 - y;
                input.Slice(sourceY * source.RowStrideBytes, rowBytes).CopyTo(result.AsSpan(y * rowBytes, rowBytes));
            }
            if (source.PremultipliedAlpha)
            {
                for (int i = 0; i < result.Length; i += 4)
                {
                    int alpha = result[i + 3];
                    if (alpha == 0) { result[i] = result[i + 1] = result[i + 2] = 0; continue; }
                    result[i] = (byte)Math.Min(255, result[i] * 255 / alpha);
                    result[i + 1] = (byte)Math.Min(255, result[i + 1] * 255 / alpha);
                    result[i + 2] = (byte)Math.Min(255, result[i + 2] * 255 / alpha);
                }
            }
            return result;
        }

        internal static byte[] EncodePng(int width, int height, byte[] rgba)
        {
            using var output = new MemoryStream();
            output.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
            byte[] header = new byte[13];
            WriteInt(header, 0, width); WriteInt(header, 4, height); header[8] = 8; header[9] = 6;
            WriteChunk(output, "IHDR", header);
            using var raw = new MemoryStream();
            int rowBytes = checked(width * 4);
            for (int y = 0; y < height; y++) { raw.WriteByte(0); raw.Write(rgba, y * rowBytes, rowBytes); }
            byte[] rawBytes = raw.ToArray();
            using var compressed = new MemoryStream();
            compressed.WriteByte(0x78); compressed.WriteByte(0x9C);
            using (var deflate = new DeflateStream(compressed, CompressionLevel.Optimal, true)) deflate.Write(rawBytes, 0, rawBytes.Length);
            uint adler = Adler32(rawBytes); compressed.WriteByte((byte)(adler >> 24)); compressed.WriteByte((byte)(adler >> 16)); compressed.WriteByte((byte)(adler >> 8)); compressed.WriteByte((byte)adler);
            WriteChunk(output, "IDAT", compressed.ToArray()); WriteChunk(output, "IEND", Array.Empty<byte>());
            return output.ToArray();
        }

        internal static void DrawOverlay(byte[] rgba, int width, int height,
            int coordinateWidth, int coordinateHeight,
            IEnumerable<FishUIControlSnapshot> controls, IEnumerable<FishUIDiagnosticWarning> warnings)
        {
            if (coordinateWidth <= 0 || coordinateHeight <= 0)
                throw new InvalidDataException("Overlay coordinate dimensions must be positive.");
            float scaleX = width / (float)coordinateWidth;
            float scaleY = height / (float)coordinateHeight;
            var warned = new HashSet<long>(warnings.Where(w => w.ControlId.HasValue).Select(w => w.ControlId.Value));
            foreach (var control in controls)
            {
                if (control.RemovedDuringDraw) continue;
                var rect = control.Geometry?.AbsoluteBoundsPixels; if (rect == null) continue;
                byte r = control.State.HasFocus ? (byte)0 : control.State.Hovered ? (byte)255 : (byte)0;
                byte g = control.State.HasFocus ? (byte)120 : control.State.Hovered ? (byte)180 : (byte)220;
                byte b = control.State.HasFocus ? (byte)255 : (byte)0;
                DrawRect(rgba, width, height, rect, scaleX, scaleY, r, g, b, 255);
                int labelX = (int)MathF.Round(rect.X * scaleX) + 2;
                int labelY = (int)MathF.Round(rect.Y * scaleY) + 2;
                DrawText(rgba, width, height, labelX, labelY, "#" + control.ControlId, r, g, b);
                if (warned.Contains(control.ControlId)) DrawText(rgba, width, height, labelX, labelY + 8, "!", 255, 0, 0);
            }
        }

        private static void DrawRect(byte[] pixels, int width, int height, FishUIDebugRect rect,
            float scaleX, float scaleY, byte r, byte g, byte b, byte a)
        {
            int x0 = (int)MathF.Round(rect.X * scaleX);
            int y0 = (int)MathF.Round(rect.Y * scaleY);
            int x1 = (int)MathF.Round((rect.X + rect.Width) * scaleX);
            int y1 = (int)MathF.Round((rect.Y + rect.Height) * scaleY);
            for (int x = x0; x <= x1; x++) { Set(pixels, width, height, x, y0, r, g, b, a); Set(pixels, width, height, x, y1, r, g, b, a); }
            for (int y = y0; y <= y1; y++) { Set(pixels, width, height, x0, y, r, g, b, a); Set(pixels, width, height, x1, y, r, g, b, a); }
        }

        private static void DrawText(byte[] pixels, int width, int height, int x, int y, string text, byte r, byte g, byte b)
        {
            foreach (char input in text)
            {
                char c = input >= 32 && input <= 126 ? input : '?';
                int glyphOffset = (c - 32) * 5;
                for (int gx = 0; gx < 5; gx++)
                    for (int gy = 0; gy < 7; gy++)
                        if ((FontColumns[glyphOffset + gx] & (1 << gy)) != 0)
                            Set(pixels, width, height, x + gx, y + gy, r, g, b, 255);
                x += 6;
            }
        }

        private static void Set(byte[] pixels, int width, int height, int x, int y, byte r, byte g, byte b, byte a)
        {
            if ((uint)x >= (uint)width || (uint)y >= (uint)height) return; int i = (y * width + x) * 4;
            pixels[i] = r; pixels[i + 1] = g; pixels[i + 2] = b; pixels[i + 3] = a;
        }

        private static void WriteChunk(Stream output, string type, byte[] data)
        {
            byte[] typeBytes = Encoding.ASCII.GetBytes(type); byte[] length = new byte[4]; WriteInt(length, 0, data.Length); output.Write(length); output.Write(typeBytes); output.Write(data);
            uint crc = 0xffffffff; foreach (byte value in typeBytes) crc = CrcTable[(crc ^ value) & 255] ^ (crc >> 8); foreach (byte value in data) crc = CrcTable[(crc ^ value) & 255] ^ (crc >> 8); crc ^= 0xffffffff;
            output.WriteByte((byte)(crc >> 24)); output.WriteByte((byte)(crc >> 16)); output.WriteByte((byte)(crc >> 8)); output.WriteByte((byte)crc);
        }

        private static void WriteInt(byte[] target, int offset, int value) { target[offset] = (byte)(value >> 24); target[offset + 1] = (byte)(value >> 16); target[offset + 2] = (byte)(value >> 8); target[offset + 3] = (byte)value; }
        private static uint Adler32(byte[] data) { uint a = 1, b = 0; foreach (byte value in data) { a = (a + value) % 65521; b = (b + a) % 65521; } return (b << 16) | a; }
        private static uint[] CreateCrcTable() { var table = new uint[256]; for (uint n = 0; n < 256; n++) { uint c = n; for (int k = 0; k < 8; k++) c = (c & 1) != 0 ? 0xedb88320 ^ (c >> 1) : c >> 1; table[n] = c; } return table; }
    }
}
