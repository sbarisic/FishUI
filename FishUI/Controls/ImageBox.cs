using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	/// <summary>
	/// Scaling mode for image display.
	/// </summary>
	public enum ImageScaleMode
	{
		/// <summary>
		/// No scaling - image is drawn at its original size.
		/// </summary>
		None,

		/// <summary>
		/// Stretch the image to fill the control bounds (may distort aspect ratio).
		/// </summary>
		Stretch,

		/// <summary>
		/// Scale the image to fit within the control while maintaining aspect ratio.
		/// </summary>
		Fit,

		/// <summary>
		/// Scale the image to fill the control while maintaining aspect ratio (may crop).
		/// </summary>
		Fill
	}

	/// <summary>
	/// A control that displays an image with configurable scaling modes.
	/// </summary>
	public class ImageBox : Control
	{
		/// <summary>
		/// The image to display.
		/// </summary>
		[YamlIgnore]
		public ImageRef Image { get; set; }

		/// <summary>
		/// How the image should be scaled within the control bounds.
		/// </summary>
		public ImageScaleMode ScaleMode { get; set; } = ImageScaleMode.Stretch;

		/// <summary>
		/// Event fired when the ImageBox is clicked.
		/// </summary>
		public event Action<ImageBox, FishMouseButton, Vector2> OnClick;

		public ImageBox()
		{
			Size = new Vector2(100, 100);
		}

		public ImageBox(ImageRef image) : this()
		{
			Image = image;
			if (image != null)
			{
				Size = new Vector2(image.Width, image.Height);
			}
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			if (Image == null)
				return;

			Vector2 pos = GetAbsolutePosition();
			Vector2 size = GetAbsoluteSize();
			FishColor drawColor = EffectiveColor;

			switch (ScaleMode)
			{
				case ImageScaleMode.None:
					// Draw at original size, centered
					Vector2 offset = (size - new Vector2(Image.Width, Image.Height)) / 2;
					UI.Graphics.DrawImage(Image, pos + offset, 0f, 1f, drawColor);
					break;

				case ImageScaleMode.Stretch:
					// Stretch to fill the control bounds
					UI.Graphics.DrawImage(Image, pos, size, 0f, 1f, drawColor);
					break;

				case ImageScaleMode.Fit:
					// Scale to fit while maintaining aspect ratio
					{
						float imgAspect = (float)Image.Width / Image.Height;
						float ctrlAspect = size.X / size.Y;
						Vector2 drawSize;
						if (imgAspect > ctrlAspect)
						{
							// Image is wider - fit to width
							drawSize = new Vector2(size.X, size.X / imgAspect);
						}
						else
						{
							// Image is taller - fit to height
							drawSize = new Vector2(size.Y * imgAspect, size.Y);
						}
						Vector2 drawOffset = (size - drawSize) / 2;
						UI.Graphics.DrawImage(Image, pos + drawOffset, drawSize, 0f, 1f, drawColor);
					}
					break;

				case ImageScaleMode.Fill:
					// Scale to fill while maintaining aspect ratio (may crop)
					{
						float imgAspect = (float)Image.Width / Image.Height;
						float ctrlAspect = size.X / size.Y;
						Vector2 drawSize;
						if (imgAspect < ctrlAspect)
						{
							// Image is taller - fit to width (crop height)
							drawSize = new Vector2(size.X, size.X / imgAspect);
						}
						else
						{
							// Image is wider - fit to height (crop width)
							drawSize = new Vector2(size.Y * imgAspect, size.Y);
						}
						Vector2 drawOffset = (size - drawSize) / 2;

						// Use scissor to clip the overflow
						UI.Graphics.PushScissor(pos, size);
						UI.Graphics.DrawImage(Image, pos + drawOffset, drawSize, 0f, 1f, drawColor);
						UI.Graphics.PopScissor();
					}
					break;
			}
		}

		public override void HandleMouseClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMouseClick(UI, InState, Btn, Pos);

			OnClick?.Invoke(this, Btn, Pos);
		}
	}
}
