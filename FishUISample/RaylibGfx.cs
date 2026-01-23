using FishUI;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace FishUISample
{
    class RaylibGfx : IFishUIGfx
    {
        class Scissor
        {
            public Vector2 Pos;
            public Vector2 Size;

            public Scissor(Vector2 Pos, Vector2 Size)
            {
                this.Pos = Pos;
                this.Size = Size;
            }
        }

        int W;
        int H;
        string Title;

        public RaylibGfx(int W, int H, string Title)
        {
            this.W = W;
            this.H = H;
            this.Title = Title;
        }

        public void FocusWindow()
        {
            Raylib.SetWindowFocused();
        }

        public void Init()
        {
            Raylib.SetTraceLogLevel(TraceLogLevel.None);

            Raylib.SetWindowState(ConfigFlags.HighDpiWindow);
            Raylib.SetWindowState(ConfigFlags.Msaa4xHint);
            Raylib.SetWindowState(ConfigFlags.ResizableWindow);
            //Raylib.SetWindowState(ConfigFlags.UndecoratedWindow);

            Raylib.InitWindow(W, H, "Fishmachine");
            int TargetFPS = Raylib.GetMonitorRefreshRate(0);
            Raylib.SetTargetFPS(TargetFPS);
        }

        public void BeginDrawing(float Dt)
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(240, 240, 240));

            Raylib.BeginBlendMode(BlendMode.Alpha);
        }

        public void EndDrawing()
        {
            Raylib.EndDrawing();
        }

        public void DrawLine(Vector2 Pos1, Vector2 Pos2, float Thick, FishColor Clr)
        {
            Raylib.DrawLineEx(Pos1, Pos2, Thick, new Color(Clr.R, Clr.G, Clr.B, Clr.A));
        }

        public void DrawRectangle(Vector2 Position, Vector2 Size, FishColor Color)
        {
            Raylib.DrawRectangleV(Position, Size, new Color(Color.R, Color.G, Color.B, Color.A));
        }

        public void DrawRectangleOutline(Vector2 Position, Vector2 Size, FishColor Color)
        {
            Color C = new Color(Color.R, Color.G, Color.B, Color.A);
            Raylib.DrawRectangleLinesEx(new Rectangle(Position, Size), 1, C);
        }

        public void BeginScissor(Vector2 Pos, Vector2 Size)
        {
            Raylib.BeginScissorMode((int)Pos.X, (int)Pos.Y, (int)Size.X, (int)Size.Y);
        }

        public void EndScissor()
        {
            Raylib.EndScissorMode();
        }

        Scissor CurScissor;
        Stack<Scissor> ScissorStack = new Stack<Scissor>();

        public void PushScissor(Vector2 Pos, Vector2 Size)
        {
            ScissorStack.Push(new Scissor(Pos, Size));
            ApplyScissorStack();
        }

        public void PopScissor()
        {
            ScissorStack.Pop();

            if (ScissorStack.Count == 0)
                EndScissor();
            else
                ApplyScissorStack();
        }

        private void ApplyScissorStack()
        {
            if (ScissorStack.Count == 0)
            {
                EndScissor();
                return;
            }

            // Calculate the intersection of all scissors in the stack
            var scissors = ScissorStack.ToArray();
            Vector2 resultPos = scissors[0].Pos;
            Vector2 resultSize = scissors[0].Size;

            for (int i = 1; i < scissors.Length; i++)
            {
                if (!Utils.Union(resultPos, resultSize, scissors[i].Pos, scissors[i].Size, out resultPos, out resultSize))
                {
                    // No intersection - use a zero-size scissor (nothing visible)
                    resultPos = Vector2.Zero;
                    resultSize = Vector2.Zero;
                    break;
                }
            }

            CurScissor = new Scissor(resultPos, resultSize);
            BeginScissor(CurScissor.Pos, CurScissor.Size);
        }

        Texture2D LoadTex(Image Img)
        {
            Texture2D Ret = Raylib.LoadTextureFromImage(Img);
            LoadTex(Ret);
            return Ret;
        }

        void LoadTex(Texture2D Ret)
        {
            Raylib.SetTextureFilter(Ret, TextureFilter.Trilinear);
        }

        ImageRef CreateImageRef(string FileName, Texture2D Img, Image Img2)
        {
            ImageRef IRef = new ImageRef();
            IRef.Path = FileName;
            IRef.Width = Img.Width;
            IRef.Height = Img.Height;
            IRef.Userdata = Img;
            IRef.Userdata2 = Img2;

            return IRef;
        }

        public int GetWindowWidth()
        {
            return Raylib.GetScreenWidth();
        }

        public int GetWindowHeight()
        {
            return Raylib.GetScreenHeight();
        }

        // Loading

        public FontRef LoadFont(string FileName, float Size, float Spacing, FishColor Color)
        {
            return LoadFont(FileName, Size, Spacing, Color, FontStyle.Regular);
        }

        public FontRef LoadFont(string FileName, float Size, float Spacing, FishColor Color, FontStyle Style)
        {
            Font F = Raylib.LoadFontEx(FileName, (int)Size, null, 250);

            FontRef FRef = new FontRef();
            FRef.Path = FileName;
            FRef.Userdata = F;
            FRef.Spacing = Spacing;
            FRef.Size = Size;
            FRef.Color = Color;
            FRef.Style = Style;
            FRef.LineHeight = F.BaseSize;

            // Check if font is monospaced by comparing widths of different characters
            Vector2 wWidth = Raylib.MeasureTextEx(F, "W", Size, Spacing);
            Vector2 iWidth = Raylib.MeasureTextEx(F, "i", Size, Spacing);
            FRef.IsMonospaced = Math.Abs(wWidth.X - iWidth.X) < 0.5f;

            return FRef;
        }

        public FishUIFontMetrics GetFontMetrics(FontRef Fn)
        {
            Font F = (Font)Fn.Userdata;
            
            // Raylib font metrics
            float lineHeight = F.BaseSize;
            float ascent = lineHeight * 0.8f; // Approximate - Raylib doesn't expose this directly
            float descent = lineHeight * 0.2f; // Approximate
            float baseline = ascent;

            // Measure average and max character widths
            Vector2 avgSize = Raylib.MeasureTextEx(F, "x", Fn.Size, Fn.Spacing);
            Vector2 maxSize = Raylib.MeasureTextEx(F, "W", Fn.Size, Fn.Spacing);

            return new FishUIFontMetrics(lineHeight, ascent, descent, baseline, avgSize.X, maxSize.X);
        }

        public ImageRef LoadImage(string FileName)
        {
            Texture2D Img = Raylib.LoadTexture(FileName);
            LoadTex(Img);
            Image Img2 = Raylib.LoadImage(FileName);

            return CreateImageRef(FileName, Img, Img2);
        }

        public ImageRef LoadImage(string FileName, int X, int Y, int W, int H)
        {
            Image Img2 = Raylib.LoadImage(FileName);
            Raylib.ImageCrop(ref Img2, new Rectangle(X, Y, W, H));

            Texture2D Img = LoadTex(Img2);
            return CreateImageRef(FileName, Img, Img2);
        }

        public ImageRef LoadImage(ImageRef Orig, int X, int Y, int W, int H)
        {
            Image Img2 = Raylib.ImageFromImage((Image)Orig.Userdata2, new Rectangle(X, Y, W, H));
            Texture2D Img = LoadTex(Img2);
            return CreateImageRef(Orig.Path, Img, Img2);
        }

        public FishColor GetImageColor(ImageRef Img, Vector2 Pos)
        {
            Color C = Raylib.GetImageColor((Image)Img.Userdata2, (int)Pos.X, (int)Pos.Y);
            return new FishColor(C.R, C.G, C.B, C.A);
        }

        // Drawing

        public void DrawImage(ImageRef Img, Vector2 Pos, float Rot, float Scale, FishColor Color)
        {
            Texture2D Tex = (Texture2D)Img.Userdata;
            Color C = new Color(Color.R, Color.G, Color.B, Color.A);
            Raylib.DrawTextureEx(Tex, Pos, Rot, Scale, C);
        }

        public void DrawImage(ImageRef Img, Vector2 Pos, Vector2 Size, float Rot, float Scale, FishColor Color)
        {
            Texture2D Tex = (Texture2D)Img.Userdata;
            Color C = new Color(Color.R, Color.G, Color.B, Color.A);
            //Raylib.DrawTextureEx(Tex, Pos, Rot, Scale, C);

            Raylib.DrawTexturePro(Tex, new Rectangle(0, 0, Tex.Width, Tex.Height), new Rectangle(Pos, Size * Scale), Vector2.Zero, Rot, C);

        }

        public void DrawNPatch(NPatch NP, Vector2 Pos, Vector2 Size, FishColor Color)
        {
            DrawNPatch(NP, Pos, Size, Color, 0);
        }

        public void DrawNPatch(NPatch NP, Vector2 Pos, Vector2 Size, FishColor Color, float Rotation)
        {
            Texture2D Tex = (Texture2D)NP.Image.Userdata;
            Color C = new Color(Color.R, Color.G, Color.B, Color.A);
            NPatchInfo Info = new NPatchInfo();

            Info.Left = NP.Left;
            Info.Right = NP.Right;
            Info.Top = NP.Top;
            Info.Bottom = NP.Bottom;
            Info.Source = new Rectangle(NP.ImagePos, NP.ImageSize);
            Info.Layout = NPatchLayout.NinePatch;

            // When rotation is applied, we need to offset by the center of the destination rectangle
            Vector2 origin = Rotation != 0 ? Size / 2 : Vector2.Zero;
            Vector2 drawPos = Rotation != 0 ? Pos + origin : Pos;

            Raylib.DrawTextureNPatch(Tex, Info, new Rectangle(drawPos.Round(), Size.Round()), origin.Round(), Rotation, C);
        }

        public void DrawText(FontRef Fn, string Text, Vector2 Pos)
        {
            Font F = (Font)Fn.Userdata;
            Raylib.DrawTextEx(F, Text, Pos, Fn.Size, Fn.Spacing, new Color(Fn.Color.R, Fn.Color.G, Fn.Color.B, Fn.Color.A));
        }

        public void DrawTextColor(FontRef Fn, string Text, Vector2 Pos, FishColor Color)
        {
            Font F = (Font)Fn.Userdata;
            Raylib.DrawTextEx(F, Text, Pos.Round(), Fn.Size, Fn.Spacing, new Color(Color.R, Color.G, Color.B, Color.A));
        }

        public Vector2 MeasureText(FontRef Fn, string Text)
        {
            Font F = (Font)Fn.Userdata;
            return Raylib.MeasureTextEx(F, Text, Fn.Size, Fn.Spacing);
        }
    }

    static class GfxUtils
    {
        public static Vector2 Round(this Vector2 V)
        {
            return new Vector2((int)Math.Round(V.X), (int)Math.Round(V.Y));
        }
    }
}
