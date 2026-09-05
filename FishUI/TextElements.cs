using System;
using System.Buffers;
using System.Globalization;
using System.Text;

namespace FishUI;

/// <summary>UTF-16 offsets constrained to Unicode grapheme boundaries.</summary>
public static class TextElements
{
    private static int ElementLength(string text, int offset)
    {
#if NETSTANDARD2_1
        return StringInfo.GetNextTextElement(text, offset).Length;
#else
        return StringInfo.GetNextTextElementLength(text.AsSpan(offset));
#endif
    }

    public static int Floor(string text, int offset)
    {
        offset = Math.Clamp(offset, 0, text.Length);
        int current = 0;
        while (current < offset)
        {
            int next = current + ElementLength(text, current);
            if (next > offset) break;
            current = next;
        }
        return current;
    }

    public static int Next(string text, int offset)
    {
        int current = Floor(text, offset);
        return current == text.Length ? current : current + ElementLength(text, current);
    }

    public static int Ceiling(string text, int offset) => Floor(text, offset) == offset ? offset : Next(text, offset);
    public static int Previous(string text, int offset) => Floor(text, Math.Max(0, offset - 1));
    public static string Truncate(string text, int maximum) => text[..Floor(text, maximum)];

    public static string Normalize(string text)
    {
        text ??= "";
        int offset = 0;
        while (offset < text.Length)
        {
            if (Rune.DecodeFromUtf16(text.AsSpan(offset), out _, out int consumed) != OperationStatus.Done)
            {
                StringBuilder result = new(text.Length);
                Span<char> encoded = stackalloc char[2];
                foreach (Rune rune in text.EnumerateRunes())
                    result.Append(encoded[..rune.EncodeToUtf16(encoded)]);
                return result.ToString();
            }
            offset += consumed;
        }
        return text;
    }
}
