using System;

namespace FishUI
{
    /// <summary>Immutable metadata handle for an image owned by a graphics backend.</summary>
    public sealed class ImageRef
    {
        public string Path { get; }
        public int Width { get; }
        public int Height { get; }
        public object Userdata { get; }
        public object Userdata2 { get; }
        public bool IsAtlasRegion { get; }
        public int SourceX { get; }
        public int SourceY { get; }
        public int SourceW { get; }
        public int SourceH { get; }
        public ImageRef AtlasParent { get; }

        public ImageRef(string path = null, int width = 0, int height = 0, object userdata = null, object userdata2 = null)
            : this(path, width, height, userdata, userdata2, false, 0, 0, width, height, null)
        {
        }

        private ImageRef(string path, int width, int height, object userdata, object userdata2,
            bool isAtlasRegion, int sourceX, int sourceY, int sourceW, int sourceH, ImageRef atlasParent)
        {
            Path = path;
            Width = width;
            Height = height;
            Userdata = userdata;
            Userdata2 = userdata2;
            IsAtlasRegion = isAtlasRegion;
            SourceX = sourceX;
            SourceY = sourceY;
            SourceW = sourceW;
            SourceH = sourceH;
            AtlasParent = atlasParent;
        }

        public static ImageRef FromAtlasRegion(ImageRef atlas, int x, int y, int width, int height)
        {
            if (atlas == null) throw new ArgumentNullException(nameof(atlas));
            return new ImageRef(atlas.Path, width, height, atlas.Userdata, atlas.Userdata2, true,
                x, y, width, height, atlas);
        }

        public static ImageRef FromAtlasRegion(ImageRef atlas, FishUIThemeRegion region)
        {
            if (region == null) throw new ArgumentNullException(nameof(region));
            return FromAtlasRegion(atlas, region.X, region.Y, region.Width, region.Height);
        }
    }
}
