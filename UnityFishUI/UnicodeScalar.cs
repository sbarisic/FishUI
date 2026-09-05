global using Rune = FishUI.Compatibility.UnicodeScalar;

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;

namespace FishUI.Compatibility
{
    /// <summary>A Unicode scalar for the .NET Standard adapter, whose reference framework has no public Rune.</summary>
    public readonly struct UnicodeScalar
    {
        public int Value { get; }
        public int Utf16SequenceLength => Value > 0xffff ? 2 : 1;
        public static UnicodeScalar ReplacementChar => new UnicodeScalar(0xfffd);

        private UnicodeScalar(int value) => Value = value;

        public static bool TryCreate(int value, out UnicodeScalar result)
        {
            bool valid = (uint)value <= 0x10ffff && (value < 0xd800 || value > 0xdfff);
            result = valid ? new UnicodeScalar(value) : default;
            return valid;
        }

        public static OperationStatus DecodeFromUtf16(ReadOnlySpan<char> source, out UnicodeScalar result, out int consumed)
        {
            result = ReplacementChar;
            consumed = 0;
            if (source.IsEmpty) return OperationStatus.NeedMoreData;
            consumed = 1;
            if (!char.IsSurrogate(source[0]))
            {
                result = new UnicodeScalar(source[0]);
                return OperationStatus.Done;
            }
            if (!char.IsHighSurrogate(source[0])) return OperationStatus.InvalidData;
            if (source.Length == 1) return OperationStatus.NeedMoreData;
            if (!char.IsLowSurrogate(source[1])) return OperationStatus.InvalidData;
            result = new UnicodeScalar(char.ConvertToUtf32(source[0], source[1]));
            consumed = 2;
            return OperationStatus.Done;
        }

        public int EncodeToUtf16(Span<char> destination)
        {
            if (destination.Length < Utf16SequenceLength) throw new ArgumentException("Destination is too short.", nameof(destination));
            if (Value <= 0xffff) destination[0] = (char)Value;
            else
            {
                int pair = Value - 0x10000;
                destination[0] = (char)(0xd800 + (pair >> 10));
                destination[1] = (char)(0xdc00 + (pair & 0x3ff));
            }
            return Utf16SequenceLength;
        }

        public static UnicodeCategory GetUnicodeCategory(UnicodeScalar value) => CharUnicodeInfo.GetUnicodeCategory(value.ToString(), 0);
        public static bool IsControl(UnicodeScalar value) => GetUnicodeCategory(value) == UnicodeCategory.Control;
        public static UnicodeScalar ToLowerInvariant(UnicodeScalar value)
        {
            string lower = value.ToString().ToLowerInvariant();
            DecodeFromUtf16(lower.AsSpan(), out UnicodeScalar result, out _);
            return result;
        }
        public override string ToString() => char.ConvertFromUtf32(Value);
    }
}

namespace System.Text
{
    internal static class UnicodeScalarExtensions
    {
        public static IEnumerable<FishUI.Compatibility.UnicodeScalar> EnumerateRunes(this string text)
        {
            int offset = 0;
            while (offset < text.Length)
            {
                FishUI.Compatibility.UnicodeScalar.DecodeFromUtf16(text.AsSpan(offset), out var value, out int consumed);
                offset += consumed;
                yield return value;
            }
        }
    }
}
