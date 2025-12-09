using FishUI;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
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

		public void Init()
		{
		}

		public void BeginDrawing(float Dt)
		{
			Raylib.BeginDrawing();
			Raylib.ClearBackground(new Color(240, 240, 240));
		}

		public void EndDrawing()
		{
			Raylib.EndDrawing();
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
			bool HasScissor = false;

			if (ScissorStack.Count == 1)
			{
				HasScissor = true;
				CurScissor = ScissorStack.Peek();
			}
			else if (ScissorStack.Count > 1)
			{
				HasScissor = true;
				Scissor Tp = ScissorStack.Peek();
				if (Utils.Union(CurScissor.Pos, CurScissor.Size, Tp.Pos, Tp.Size, out Vector2 NewPos, out Vector2 NewSize))
				{
					CurScissor = new Scissor(NewPos, NewSize);
				}
			}

			if (HasScissor)
				BeginScissor(CurScissor.Pos, CurScissor.Size);
		}

		public void PopScissor()
		{
			ScissorStack.Pop();

			if (ScissorStack.Count == 0)
				EndScissor();
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
			Texture2D Tex = (Texture2D)NP.Image.Userdata;
			Color C = new Color(Color.R, Color.G, Color.B, Color.A);
			NPatchInfo Info = new NPatchInfo();

			Info.Left = NP.Left;
			Info.Right = NP.Right;
			Info.Top = NP.Top;
			Info.Bottom = NP.Bottom;
			Info.Source = new Rectangle(NP.ImagePos, NP.ImageSize);

			Raylib.DrawTextureNPatch(Tex, Info, new Rectangle(Pos, Size), Vector2.Zero, 0, C);
		}
	}
}
