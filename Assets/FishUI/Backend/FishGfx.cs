using FishUI;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UMatrix4x4 = UnityEngine.Matrix4x4;
using UVector2 = UnityEngine.Vector2;
using UVector3 = UnityEngine.Vector3;
using Vector2 = System.Numerics.Vector2;

public class FishGfx : IFishUIGfx
{
	private FishFS FS;
	private Texture2D whiteTexture;
	private Dictionary<int, Texture2D> loadedTextures = new Dictionary<int, Texture2D>();
	private Dictionary<int, Font> loadedFonts = new Dictionary<int, Font>();
	private Dictionary<int, FontData> fontDataMap = new Dictionary<int, FontData>();
	private int nextImageId = 1;
	private int nextFontId = 1;
	private readonly Stack<(Vector2 Pos, Vector2 Size)> scissorStack = new Stack<(Vector2 Pos, Vector2 Size)>();
	private int activeClipCount = 0;
	private Vector2 currentClipOffset = Vector2.Zero;

	private class FontData
	{
		public Font UnityFont;
		public float Size;
		public float Spacing;
		public FishColor Color;
		public FishUI.FontStyle Style;
		public GUIStyle CachedStyle;
	}

	public FishGfx(FishFS FS)
	{
		this.FS = FS;
		CreateWhiteTexture();
	}

	private void CreateWhiteTexture()
	{
		whiteTexture = new Texture2D(1, 1);
		whiteTexture.SetPixel(0, 0, UnityEngine.Color.white);
		whiteTexture.Apply();
	}

	public void Init()
	{
		// Initialization is done in constructor
	}

	public void BeginFrame()
	{
		activeClipCount = 0;
		scissorStack.Clear();
		currentClipOffset = Vector2.Zero;
	}

	public void EndFrame()
	{
		while (activeClipCount > 0)
		{
			GUI.EndClip();
			activeClipCount--;
		}
		scissorStack.Clear();
		currentClipOffset = Vector2.Zero;
	}

	public void BeginDrawing(float Dt)
	{
		// No-op for Unity IMGUI - drawing happens in OnGUI
	}

	public void EndDrawing()
	{
		// No-op for Unity IMGUI
	}

	public int GetWindowWidth()
	{
		return Screen.width;
	}

	public int GetWindowHeight()
	{
		return Screen.height;
	}

	public void FocusWindow()
	{
		// No-op - Unity handles window focus
	}

	// Scissor/Clipping
	public void BeginScissor(Vector2 Pos, Vector2 Size)
	{
		// End any existing clip before starting a new one
		if (activeClipCount > 0)
		{
			GUI.EndClip();
			activeClipCount--;
		}

		Rect clipRect = new Rect(Pos.X, Pos.Y, Size.X, Size.Y);
		GUI.BeginClip(clipRect);
		activeClipCount++;
		currentClipOffset = Pos;
	}

	public void EndScissor()
	{
		if (activeClipCount > 0)
		{
			GUI.EndClip();
			activeClipCount--;
			currentClipOffset = Vector2.Zero;
		}
	}

	public void PushScissor(Vector2 Pos, Vector2 Size)
	{
		// End current clip if any
		if (activeClipCount > 0)
		{
			GUI.EndClip();
			activeClipCount--;
		}

		// Intersect with existing scissor if any
		if (scissorStack.Count > 0)
		{
			var current = scissorStack.Peek();
			(Pos, Size) = IntersectRectangles(current.Pos, current.Size, Pos, Size);
		}

		scissorStack.Push((Pos, Size));

		Rect clipRect = new Rect(Pos.X, Pos.Y, Size.X, Size.Y);
		GUI.BeginClip(clipRect);
		activeClipCount++;
		currentClipOffset = Pos;
	}

	public void PopScissor()
	{
		// End current clip
		if (activeClipCount > 0)
		{
			GUI.EndClip();
			activeClipCount--;
		}

		if (scissorStack.Count > 0)
		{
			scissorStack.Pop();
		}

		// Restore previous clip if any
		if (scissorStack.Count > 0)
		{
			var current = scissorStack.Peek();
			Rect clipRect = new Rect(current.Pos.X, current.Pos.Y, current.Size.X, current.Size.Y);
			GUI.BeginClip(clipRect);
			activeClipCount++;
			currentClipOffset = current.Pos;
		}
		else
		{
			currentClipOffset = Vector2.Zero;
		}
	}

	private static (Vector2 Pos, Vector2 Size) IntersectRectangles(Vector2 pos1, Vector2 size1, Vector2 pos2, Vector2 size2)
	{
		float x1 = System.Math.Max(pos1.X, pos2.X);
		float y1 = System.Math.Max(pos1.Y, pos2.Y);
		float x2 = System.Math.Min(pos1.X + size1.X, pos2.X + size2.X);
		float y2 = System.Math.Min(pos1.Y + size1.Y, pos2.Y + size2.Y);

		float width = System.Math.Max(0, x2 - x1);
		float height = System.Math.Max(0, y2 - y1);

		return (new Vector2(x1, y1), new Vector2(width, height));
	}

	// Drawing Primitives
	public void DrawRectangle(Vector2 Position, Vector2 Size, FishColor Color)
	{
		Vector2 localPos = ToLocalCoords(Position);
		GUI.color = ToUnityColor(Color);
		GUI.DrawTexture(new Rect(localPos.X, localPos.Y, Size.X, Size.Y), whiteTexture);
		GUI.color = UnityEngine.Color.white;
	}

	public void DrawRectangleOutline(Vector2 Position, Vector2 Size, FishColor Color)
	{
		float thickness = 1f;
		// Top
		DrawRectangle(Position, new Vector2(Size.X, thickness), Color);
		// Bottom
		DrawRectangle(new Vector2(Position.X, Position.Y + Size.Y - thickness), new Vector2(Size.X, thickness), Color);
		// Left
		DrawRectangle(Position, new Vector2(thickness, Size.Y), Color);
		// Right
		DrawRectangle(new Vector2(Position.X + Size.X - thickness, Position.Y), new Vector2(thickness, Size.Y), Color);
	}

	public void DrawLine(Vector2 Pos1, Vector2 Pos2, float Thick, FishColor Clr)
	{
		Vector2 diff = Pos2 - Pos1;
		float length = diff.Length();
		if (length < 0.001f) return;

		Vector2 localPos1 = ToLocalCoords(Pos1);
		float angle = Mathf.Atan2(diff.Y, diff.X) * Mathf.Rad2Deg;

		UMatrix4x4 matrixBackup = GUI.matrix;
		GUIUtility.RotateAroundPivot(angle, new UVector2(localPos1.X, localPos1.Y));

		GUI.color = ToUnityColor(Clr);
		GUI.DrawTexture(new Rect(localPos1.X, localPos1.Y - Thick / 2f, length, Thick), whiteTexture);
		GUI.color = UnityEngine.Color.white;

		GUI.matrix = matrixBackup;
	}

	public void DrawCircle(Vector2 Center, float Radius, FishColor Color)
	{
		Vector2 localCenter = ToLocalCoords(Center);
		int segments = Mathf.Max(16, (int)(Radius * 0.5f));
		float angleStep = 360f / segments;

		GUI.color = ToUnityColor(Color);

		for (int i = 0; i < segments; i++)
		{
			float angle1 = i * angleStep * Mathf.Deg2Rad;
			float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;

			UVector2 p1 = new UVector2(localCenter.X, localCenter.Y);
			UVector2 p2 = new UVector2(localCenter.X + Mathf.Cos(angle1) * Radius, localCenter.Y + Mathf.Sin(angle1) * Radius);
			UVector2 p3 = new UVector2(localCenter.X + Mathf.Cos(angle2) * Radius, localCenter.Y + Mathf.Sin(angle2) * Radius);

			DrawTriangleLocal(p1, p2, p3);
		}

		GUI.color = UnityEngine.Color.white;
	}

	private void DrawTriangleLocal(UVector2 p1, UVector2 p2, UVector2 p3)
	{
		float minX = Mathf.Min(p1.x, Mathf.Min(p2.x, p3.x));
		float maxX = Mathf.Max(p1.x, Mathf.Max(p2.x, p3.x));
		float minY = Mathf.Min(p1.y, Mathf.Min(p2.y, p3.y));
		float maxY = Mathf.Max(p1.y, Mathf.Max(p2.y, p3.y));

		GUI.DrawTexture(new Rect(minX, minY, maxX - minX, maxY - minY), whiteTexture);
	}

	public void DrawCircleOutline(Vector2 Center, float Radius, FishColor Color, float Thickness = 1)
	{
		int segments = Mathf.Max(16, (int)(Radius * 0.5f));
		float angleStep = 360f / segments;

		for (int i = 0; i < segments; i++)
		{
			float angle1 = i * angleStep * Mathf.Deg2Rad;
			float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;

			Vector2 p1 = new Vector2(Center.X + Mathf.Cos(angle1) * Radius, Center.Y + Mathf.Sin(angle1) * Radius);
			Vector2 p2 = new Vector2(Center.X + Mathf.Cos(angle2) * Radius, Center.Y + Mathf.Sin(angle2) * Radius);

			DrawLine(p1, p2, Thickness, Color);
		}
	}

	private int GetImageId(ImageRef img)
	{
		if (img.Userdata is int id) return id;
		return 0;
	}

	private int GetFontId(FontRef fn)
	{
		if (fn.Userdata is int id) return id;
		return 0;
	}

	// Image Loading and Drawing
	public ImageRef LoadImage(string FileName)
	{
		string resolvedPath = FS.ResolvePath(FileName);
		Texture2D texture = null;

		// Try loading from Resources first (without extension)
		string resourcePath = FileName.Replace("\\", "/");
		if (resourcePath.EndsWith(".png") || resourcePath.EndsWith(".jpg") || resourcePath.EndsWith(".jpeg"))
		{
			resourcePath = resourcePath.Substring(0, resourcePath.LastIndexOf('.'));
		}
		texture = Resources.Load<Texture2D>(resourcePath);

		// If not found in Resources, try loading from file
		if (texture == null && System.IO.File.Exists(resolvedPath))
		{
			byte[] fileData = System.IO.File.ReadAllBytes(resolvedPath);
			texture = new Texture2D(2, 2);
			texture.LoadImage(fileData);
		}

		if (texture == null)
		{
			Debug.LogWarning($"[FishGfx] Failed to load image: {FileName}");
			return new ImageRef { Userdata = 0, Width = 0, Height = 0 };
		}

		int id = nextImageId++;
		loadedTextures[id] = texture;

		return new ImageRef { Userdata = id, Width = texture.width, Height = texture.height };
	}

	public ImageRef LoadImage(string FileName, int X, int Y, int W, int H)
	{
		ImageRef fullImage = LoadImage(FileName);
		if (GetImageId(fullImage) == 0) return fullImage;

		return ImageRef.FromAtlasRegion(fullImage, X, Y, W, H);
	}

	public ImageRef LoadImage(ImageRef Orig, int X, int Y, int W, int H)
	{
		return ImageRef.FromAtlasRegion(Orig, X, Y, W, H);
	}

	public void DrawImage(ImageRef Img, Vector2 Pos, float Rot, float Scale, FishColor Color)
	{
		DrawImage(Img, Pos, new Vector2(Img.Width * Scale, Img.Height * Scale), Rot, 1f, Color);
	}

	public void DrawImage(ImageRef Img, Vector2 Pos, Vector2 Size, float Rot, float Scale, FishColor Color)
	{
		// Handle atlas regions
		Texture2D texture;
		Rect texCoords = new Rect(0, 0, 1, 1);

		if (Img.IsAtlasRegion)
		{
			int parentId = GetImageId(Img.AtlasParent);
			if (!loadedTextures.TryGetValue(parentId, out texture)) return;

			// Calculate UV coordinates for the atlas region
			float u = (float)Img.SourceX / texture.width;
			float v = 1f - (float)(Img.SourceY + Img.SourceH) / texture.height;
			float uWidth = (float)Img.SourceW / texture.width;
			float vHeight = (float)Img.SourceH / texture.height;
			texCoords = new Rect(u, v, uWidth, vHeight);
		}
		else
		{
			int imgId = GetImageId(Img);
			if (!loadedTextures.TryGetValue(imgId, out texture)) return;
		}

		Vector2 localPos = ToLocalCoords(Pos);
		Vector2 scaledSize = Size * Scale;
		Rect drawRect = new Rect(localPos.X, localPos.Y, scaledSize.X, scaledSize.Y);

		UMatrix4x4 matrixBackup = GUI.matrix;

		if (Rot != 0)
		{
			UVector2 pivot = new UVector2(localPos.X + scaledSize.X / 2f, localPos.Y + scaledSize.Y / 2f);
			GUIUtility.RotateAroundPivot(Rot, pivot);
		}

		GUI.color = ToUnityColor(Color);

		if (Img.IsAtlasRegion)
		{
			GUI.DrawTextureWithTexCoords(drawRect, texture, texCoords);
		}
		else
		{
			GUI.DrawTexture(drawRect, texture);
		}

		GUI.color = UnityEngine.Color.white;
		GUI.matrix = matrixBackup;
	}

	public void SetImageFilter(ImageRef Img, bool pixelated)
	{
		Texture2D texture;
		if (Img.IsAtlasRegion)
		{
			int parentId = GetImageId(Img.AtlasParent);
			if (!loadedTextures.TryGetValue(parentId, out texture)) return;
		}
		else
		{
			int imgId = GetImageId(Img);
			if (!loadedTextures.TryGetValue(imgId, out texture)) return;
		}
		texture.filterMode = pixelated ? FilterMode.Point : FilterMode.Bilinear;
	}

	public FishColor GetImageColor(ImageRef Img, Vector2 Pos)
	{
		Texture2D texture;
		int offsetX = 0;
		int offsetY = 0;

		if (Img.IsAtlasRegion)
		{
			int parentId = GetImageId(Img.AtlasParent);
			if (!loadedTextures.TryGetValue(parentId, out texture))
			{
				return new FishColor(0, 0, 0, 0);
			}
			offsetX = Img.SourceX;
			offsetY = Img.SourceY;
		}
		else
		{
			int imgId = GetImageId(Img);
			if (!loadedTextures.TryGetValue(imgId, out texture))
			{
				return new FishColor(0, 0, 0, 0);
			}
		}

		int x = Mathf.Clamp((int)Pos.X + offsetX, 0, texture.width - 1);
		int y = Mathf.Clamp((int)Pos.Y + offsetY, 0, texture.height - 1);
		UnityEngine.Color pixel = texture.GetPixel(x, texture.height - 1 - y);

		return new FishColor((byte)(pixel.r * 255), (byte)(pixel.g * 255), (byte)(pixel.b * 255), (byte)(pixel.a * 255));
	}

	// NPatch Drawing
	public void DrawNPatch(NPatch NP, Vector2 Pos, Vector2 Size, FishColor Color)
	{
		DrawNPatch(NP, Pos, Size, Color, 0f);
	}

	public void DrawNPatch(NPatch NP, Vector2 Pos, Vector2 Size, FishColor Color, float Rotation)
	{
		// Note: Rotation is ignored in this implementation

		float srcLeft = NP.Left;
		float srcRight = NP.Right;
		float srcTop = NP.Top;
		float srcBottom = NP.Bottom;

		float srcX = NP.ImagePos.X;
		float srcY = NP.ImagePos.Y;
		float srcW = NP.ImageSize.X;
		float srcH = NP.ImageSize.Y;

		float srcCenterW = srcW - srcLeft - srcRight;
		float srcCenterH = srcH - srcTop - srcBottom;

		float destCenterW = Size.X - srcLeft - srcRight;
		float destCenterH = Size.Y - srcTop - srcBottom;

		// Ensure minimum sizes
		if (destCenterW < 0) destCenterW = 0;
		if (destCenterH < 0) destCenterH = 0;

		// Set color once for all 9 patches
		GUI.color = ToUnityColor(Color);

		// Top-Left corner
		if (srcLeft > 0 && srcTop > 0)
		{
			DrawImageRegionNoColor(NP.Image,
				new Vector2(srcX, srcY),
				new Vector2(srcLeft, srcTop),
				Pos,
				new Vector2(srcLeft, srcTop));
		}

		// Top-Center (stretches horizontally)
		if (srcCenterW > 0 && srcTop > 0)
		{
			DrawImageRegionNoColor(NP.Image,
				new Vector2(srcX + srcLeft, srcY),
				new Vector2(srcCenterW, srcTop),
				new Vector2(Pos.X + srcLeft, Pos.Y),
				new Vector2(destCenterW, srcTop));
		}

		// Top-Right corner
		if (srcRight > 0 && srcTop > 0)
		{
			DrawImageRegionNoColor(NP.Image,
				new Vector2(srcX + srcW - srcRight, srcY),
				new Vector2(srcRight, srcTop),
				new Vector2(Pos.X + srcLeft + destCenterW, Pos.Y),
				new Vector2(srcRight, srcTop));
		}

		// Middle-Left (stretches vertically)
		if (srcLeft > 0 && srcCenterH > 0)
		{
			DrawImageRegionNoColor(NP.Image,
				new Vector2(srcX, srcY + srcTop),
				new Vector2(srcLeft, srcCenterH),
				new Vector2(Pos.X, Pos.Y + srcTop),
				new Vector2(srcLeft, destCenterH));
		}

		// Center (stretches both ways)
		if (srcCenterW > 0 && srcCenterH > 0)
		{
			DrawImageRegionNoColor(NP.Image,
				new Vector2(srcX + srcLeft, srcY + srcTop),
				new Vector2(srcCenterW, srcCenterH),
				new Vector2(Pos.X + srcLeft, Pos.Y + srcTop),
				new Vector2(destCenterW, destCenterH));
		}

		// Middle-Right (stretches vertically)
		if (srcRight > 0 && srcCenterH > 0)
		{
			DrawImageRegionNoColor(NP.Image,
				new Vector2(srcX + srcW - srcRight, srcY + srcTop),
				new Vector2(srcRight, srcCenterH),
				new Vector2(Pos.X + srcLeft + destCenterW, Pos.Y + srcTop),
				new Vector2(srcRight, destCenterH));
		}

		// Bottom-Left corner
		if (srcLeft > 0 && srcBottom > 0)
		{
			DrawImageRegionNoColor(NP.Image,
				new Vector2(srcX, srcY + srcH - srcBottom),
				new Vector2(srcLeft, srcBottom),
				new Vector2(Pos.X, Pos.Y + srcTop + destCenterH),
				new Vector2(srcLeft, srcBottom));
		}

		// Bottom-Center (stretches horizontally)
		if (srcCenterW > 0 && srcBottom > 0)
		{
			DrawImageRegionNoColor(NP.Image,
				new Vector2(srcX + srcLeft, srcY + srcH - srcBottom),
				new Vector2(srcCenterW, srcBottom),
				new Vector2(Pos.X + srcLeft, Pos.Y + srcTop + destCenterH),
				new Vector2(destCenterW, srcBottom));
		}

		// Bottom-Right corner
		if (srcRight > 0 && srcBottom > 0)
		{
			DrawImageRegionNoColor(NP.Image,
				new Vector2(srcX + srcW - srcRight, srcY + srcH - srcBottom),
				new Vector2(srcRight, srcBottom),
				new Vector2(Pos.X + srcLeft + destCenterW, Pos.Y + srcTop + destCenterH),
				new Vector2(srcRight, srcBottom));
		}

		GUI.color = UnityEngine.Color.white;
	}

	private void DrawImageRegionNoColor(ImageRef Img, Vector2 srcPos, Vector2 srcSize, Vector2 destPos, Vector2 destSize)
	{
		// Get the texture - handle atlas regions
		Texture2D texture;
		int baseOffsetX = 0;
		int baseOffsetY = 0;

		if (Img.IsAtlasRegion)
		{
			int parentId = GetImageId(Img.AtlasParent);
			if (!loadedTextures.TryGetValue(parentId, out texture)) return;
			baseOffsetX = Img.SourceX;
			baseOffsetY = Img.SourceY;
		}
		else
		{
			int imgId = GetImageId(Img);
			if (!loadedTextures.TryGetValue(imgId, out texture)) return;
		}

		if (destSize.X <= 0 || destSize.Y <= 0) return;

		Vector2 localDestPos = ToLocalCoords(destPos);

		// Calculate UV coordinates with atlas offset
		float u = (baseOffsetX + srcPos.X) / texture.width;
		float v = 1f - (baseOffsetY + srcPos.Y + srcSize.Y) / texture.height;
		float uWidth = srcSize.X / texture.width;
		float vHeight = srcSize.Y / texture.height;

		Rect texCoords = new Rect(u, v, uWidth, vHeight);
		Rect destRect = new Rect(localDestPos.X, localDestPos.Y, destSize.X, destSize.Y);

		GUI.DrawTextureWithTexCoords(destRect, texture, texCoords);
	}

	private void DrawImageRegion(ImageRef Img, Vector2 srcPos, Vector2 srcSize, Vector2 destPos, Vector2 destSize, FishColor Color)
	{
		GUI.color = ToUnityColor(Color);
		DrawImageRegionNoColor(Img, srcPos, srcSize, destPos, destSize);
		GUI.color = UnityEngine.Color.white;
	}

	// Font Loading and Text Drawing
	public FontRef LoadFont(string FileName, float Size, float Spacing, FishColor Color)
	{
		return LoadFont(FileName, Size, Spacing, Color, FishUI.FontStyle.Regular);
	}

	public FontRef LoadFont(string FileName, float Size, float Spacing, FishColor Color, FishUI.FontStyle Style)
	{
		Font font = null;

		// Try loading from Resources
		string resourcePath = FileName.Replace("\\", "/");
		if (resourcePath.EndsWith(".ttf") || resourcePath.EndsWith(".otf"))
		{
			resourcePath = resourcePath.Substring(0, resourcePath.LastIndexOf('.'));
		}
		font = Resources.Load<Font>(resourcePath);

		// Fallback to Arial if not found
		if (font == null)
		{
			font = Font.CreateDynamicFontFromOSFont("Arial", (int)Size);
		}

		if (font == null)
		{
			Debug.LogWarning($"[FishGfx] Failed to load font: {FileName}, using default");
		}

		int id = nextFontId++;
		loadedFonts[id] = font;
		fontDataMap[id] = new FontData
		{
			UnityFont = font,
			Size = Size,
			Spacing = Spacing,
			Color = Color,
			Style = Style,
			CachedStyle = null  // Will be created on first use in OnGUI
		};

		return new FontRef
		{
			Path = FileName,
			Userdata = id,
			Size = Size,
			Spacing = Spacing,
			Color = Color
		};
	}

	public void DrawText(FontRef Fn, string Text, Vector2 Pos)
	{
		int fontId = GetFontId(Fn);
		if (!fontDataMap.TryGetValue(fontId, out FontData fontData)) return;
		DrawTextColorScale(Fn, Text, Pos, fontData.Color, 1f);
	}

	public void DrawTextColor(FontRef Fn, string Text, Vector2 Pos, FishColor Color)
	{
		DrawTextColorScale(Fn, Text, Pos, Color, 1f);
	}

	GUIStyle CreateLabelStyle(FontData fontData, float Scale)
	{
		GUIStyle style = new GUIStyle(GUI.skin.label);
		style.font = fontData.UnityFont;
		style.fontSize = (int)(fontData.Size * Scale);
		style.wordWrap = false;
		style.alignment = TextAnchor.UpperLeft;
		style.margin = new RectOffset(0, 0, 0, 0);
		style.padding = new RectOffset(0, 0, 0, 0);
		style.hover.textColor = ToUnityColor(FishColor.Black);

		return style;
	}

	public void DrawTextColorScale(FontRef Fn, string Text, Vector2 Pos, FishColor Color, float Scale)
	{
		int fontId = GetFontId(Fn);
		if (!fontDataMap.TryGetValue(fontId, out FontData fontData)) return;

		Vector2 localPos = ToLocalCoords(Pos);

		GUIStyle style = CreateLabelStyle(fontData, Scale);

		// Set text color directly without gamma conversion for text
		// (text rendering in IMGUI works differently)
		UnityEngine.Color textColor = new UnityEngine.Color(Color.R / 255f, Color.G / 255f, Color.B / 255f, Color.A / 255f);
		style.normal.textColor = textColor;

		switch (fontData.Style)
		{
			case FishUI.FontStyle.Bold:
				style.fontStyle = UnityEngine.FontStyle.Bold;
				break;
			case FishUI.FontStyle.Italic:
				style.fontStyle = UnityEngine.FontStyle.Italic;
				break;
			case FishUI.FontStyle.BoldItalic:
				style.fontStyle = UnityEngine.FontStyle.BoldAndItalic;
				break;
			default:
				style.fontStyle = UnityEngine.FontStyle.Normal;
				break;
		}

		Vector2 size = MeasureText(Fn, Text) * Scale;
		Rect textRect = new Rect(localPos.X, localPos.Y, size.X + 100, size.Y + 10);

		GUI.Label(textRect, Text, style);
	}

	public Vector2 MeasureText(FontRef Fn, string Text)
	{
		int fontId = GetFontId(Fn);
		if (!fontDataMap.TryGetValue(fontId, out FontData fontData))
		{
			return Vector2.Zero;
		}

		if (string.IsNullOrEmpty(Text))
		{
			return new Vector2(0, fontData.Size);
		}

		// Try to use cached style, or create one if we're in OnGUI context
		GUIStyle style = fontData.CachedStyle;

		if (style == null)
		{
			// Check if we're in OnGUI context
			try
			{
				style = CreateLabelStyle(fontData, 1);
				fontData.CachedStyle = style;
			}
			catch (System.ArgumentException)
			{
				// Not in OnGUI context - estimate based on font size
				float estimatedWidth = Text.Length * fontData.Size * 0.6f;
				float estimatedHeight = fontData.Size * 1.2f;
				return new Vector2(estimatedWidth + (Text.Length - 1) * fontData.Spacing, estimatedHeight);
			}
		}

		// Ensure font is set correctly (might have been created with different settings)
		if (style.font != fontData.UnityFont || style.fontSize != (int)fontData.Size)
		{
			style.font = fontData.UnityFont;
			style.fontSize = (int)fontData.Size;
		}

		GUIContent content = new GUIContent(Text);
		UVector2 size = style.CalcSize(content);

		// Add character spacing
		float extraWidth = (Text.Length - 1) * fontData.Spacing;

		return new Vector2(size.x + extraWidth, size.y);
	}

	public FishUIFontMetrics GetFontMetrics(FontRef Fn)
	{
		int fontId = GetFontId(Fn);
		if (!fontDataMap.TryGetValue(fontId, out FontData fontData))
		{
			return new FishUIFontMetrics(0, 0, 0, 0, 0, 0);
		}

		float fontSize = fontData.Size;
		float lineHeight = fontSize * 1.2f;
		float ascent = fontSize * 0.8f;
		float descent = fontSize * 0.2f;
		float baseline = ascent;
		float avgCharWidth = fontSize * 0.5f;
		float maxCharWidth = fontSize;

		return new FishUIFontMetrics(lineHeight, ascent, descent, baseline, avgCharWidth, maxCharWidth);
	}

	// Helper to convert global coordinates to local clip coordinates
	private Vector2 ToLocalCoords(Vector2 globalPos)
	{
		return globalPos - currentClipOffset;
	}

	private float ToLocalX(float globalX)
	{
		return globalX - currentClipOffset.X;
	}

	private float ToLocalY(float globalY)
	{
		return globalY - currentClipOffset.Y;
	}

	// Utility
	private UnityEngine.Color ToUnityColor(FishColor color)
	{
		float r = color.R / 255f;
		float g = color.G / 255f;
		float b = color.B / 255f;
		float a = color.A / 255f;

		// If in linear color space, convert from gamma (sRGB) to linear
		if (QualitySettings.activeColorSpace == ColorSpace.Linear)
		{
			r = Mathf.GammaToLinearSpace(r);
			g = Mathf.GammaToLinearSpace(g);
			b = Mathf.GammaToLinearSpace(b);
		}

		return new UnityEngine.Color(r, g, b, a);
	}
}