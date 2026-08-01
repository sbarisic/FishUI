using System;

namespace FishUI
{
    [Flags]
    public enum FontStyle
    {
        Regular = 0,
        Bold = 1,
        Italic = 2,
        BoldItalic = Bold | Italic
    }

    /// <summary>Immutable metadata handle for a font owned by a graphics backend.</summary>
    public sealed class FontRef
    {
        public string Path { get; }
        public object Userdata { get; }
        public float Size { get; }
        public float Spacing { get; }
        public FishColor Color { get; }
        public FontStyle Style { get; }
        public bool IsMonospaced { get; }
        public float LineHeight { get; }
        public bool IsBold => (Style & FontStyle.Bold) != 0;
        public bool IsItalic => (Style & FontStyle.Italic) != 0;

        public FontRef(string path = null, object userdata = null, float size = 0, float spacing = 0,
            FishColor color = default, FontStyle style = FontStyle.Regular, bool isMonospaced = false,
            float lineHeight = 0)
        {
            Path = path;
            Userdata = userdata;
            Size = size;
            Spacing = spacing;
            Color = color;
            Style = style;
            IsMonospaced = isMonospaced;
            LineHeight = lineHeight;
        }
    }
}
