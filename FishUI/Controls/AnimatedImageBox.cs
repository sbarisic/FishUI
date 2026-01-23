using System;
using System.Collections.Generic;
using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	/// <summary>
	/// An image box control that displays animated frame sequences.
	/// </summary>
	public class AnimatedImageBox : Control
	{
		/// <summary>
		/// The frames of the animation as an array of images.
		/// </summary>
		[YamlIgnore]
		public List<ImageRef> Frames { get; set; } = new List<ImageRef>();

		/// <summary>
		/// The current frame index being displayed.
		/// </summary>
		[YamlIgnore]
		public int CurrentFrame
		{
			get => _currentFrame;
			set
			{
				if (Frames.Count == 0)
					_currentFrame = 0;
				else
					_currentFrame = Math.Clamp(value, 0, Frames.Count - 1);
			}
		}
		private int _currentFrame = 0;

		/// <summary>
		/// Frame rate in frames per second.
		/// </summary>
		[YamlMember]
		public float FrameRate
		{
			get => _frameRate;
			set => _frameRate = Math.Max(0.1f, value);
		}
		private float _frameRate = 10f;

		/// <summary>
		/// Whether the animation is currently playing.
		/// </summary>
		[YamlMember]
		public bool IsPlaying { get; set; } = true;

		/// <summary>
		/// Whether the animation should loop when it reaches the end.
		/// </summary>
		[YamlMember]
		public bool Loop { get; set; } = true;

		/// <summary>
		/// Whether to play the animation in reverse.
		/// </summary>
		[YamlMember]
		public bool Reverse { get; set; } = false;

		/// <summary>
		/// Whether to ping-pong (play forward then backward).
		/// </summary>
		[YamlMember]
		public bool PingPong { get; set; } = false;

		/// <summary>
		/// Scaling mode for how frames are displayed within the control bounds.
		/// </summary>
		[YamlMember]
		public ImageScaleMode ScaleMode { get; set; } = ImageScaleMode.Fit;

		/// <summary>
		/// Event fired when the animation completes (only when Loop is false).
		/// </summary>
		public event Action<AnimatedImageBox> OnAnimationComplete;

		/// <summary>
		/// Event fired when a frame changes.
		/// </summary>
		public event Action<AnimatedImageBox, int> OnFrameChanged;

		private float _frameTimer = 0f;
		private bool _pingPongForward = true;

		public AnimatedImageBox()
		{
			Size = new Vector2(64, 64);
		}

		/// <summary>
		/// Creates an AnimatedImageBox with the specified frames.
		/// </summary>
		public AnimatedImageBox(IEnumerable<ImageRef> frames) : this()
		{
			Frames.AddRange(frames);
			if (Frames.Count > 0)
			{
				Size = new Vector2(Frames[0].Width, Frames[0].Height);
			}
		}

		/// <summary>
		/// Adds a frame to the animation.
		/// </summary>
		public void AddFrame(ImageRef frame)
		{
			Frames.Add(frame);
		}

		/// <summary>
		/// Clears all frames from the animation.
		/// </summary>
		public void ClearFrames()
		{
			Frames.Clear();
			_currentFrame = 0;
		}

		/// <summary>
		/// Starts playing the animation.
		/// </summary>
		public void Play()
		{
			IsPlaying = true;
		}

		/// <summary>
		/// Pauses the animation at the current frame.
		/// </summary>
		public void Pause()
		{
			IsPlaying = false;
		}

		/// <summary>
		/// Stops the animation and resets to the first frame.
		/// </summary>
		public void Stop()
		{
			IsPlaying = false;
			_currentFrame = Reverse ? Frames.Count - 1 : 0;
			_frameTimer = 0f;
			_pingPongForward = true;
		}

		/// <summary>
		/// Advances to the next frame manually.
		/// </summary>
		public void NextFrame()
		{
			if (Frames.Count == 0) return;

			int oldFrame = _currentFrame;
			_currentFrame++;
			if (_currentFrame >= Frames.Count)
			{
				_currentFrame = Loop ? 0 : Frames.Count - 1;
			}

			if (oldFrame != _currentFrame)
				OnFrameChanged?.Invoke(this, _currentFrame);
		}

		/// <summary>
		/// Goes to the previous frame manually.
		/// </summary>
		public void PreviousFrame()
		{
			if (Frames.Count == 0) return;

			int oldFrame = _currentFrame;
			_currentFrame--;
			if (_currentFrame < 0)
			{
				_currentFrame = Loop ? Frames.Count - 1 : 0;
			}

			if (oldFrame != _currentFrame)
				OnFrameChanged?.Invoke(this, _currentFrame);
		}

		/// <summary>
		/// Jumps to a specific frame.
		/// </summary>
		public void GotoFrame(int frameIndex)
		{
			int oldFrame = _currentFrame;
			CurrentFrame = frameIndex;
			if (oldFrame != _currentFrame)
				OnFrameChanged?.Invoke(this, _currentFrame);
		}

		/// <summary>
		/// Gets the total number of frames in the animation.
		/// </summary>
		[YamlIgnore]
		public int FrameCount => Frames.Count;

		/// <summary>
		/// Gets the duration of the full animation in seconds.
		/// </summary>
		[YamlIgnore]
		public float Duration => Frames.Count / FrameRate;

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			// Update animation
			if (IsPlaying && Frames.Count > 1)
			{
				_frameTimer += Dt;
				float frameInterval = 1f / FrameRate;

				while (_frameTimer >= frameInterval)
				{
					_frameTimer -= frameInterval;
					AdvanceFrame();
				}
			}

			// Draw current frame
			if (Frames.Count > 0 && _currentFrame < Frames.Count)
			{
				ImageRef currentImage = Frames[_currentFrame];
				if (currentImage != null)
				{
					DrawFrame(UI, currentImage);
				}
			}
		}

		private void AdvanceFrame()
		{
			int oldFrame = _currentFrame;

			if (PingPong)
			{
				if (_pingPongForward)
				{
					_currentFrame++;
					if (_currentFrame >= Frames.Count - 1)
					{
						_currentFrame = Frames.Count - 1;
						_pingPongForward = false;
					}
				}
				else
				{
					_currentFrame--;
					if (_currentFrame <= 0)
					{
						_currentFrame = 0;
						_pingPongForward = true;
						if (!Loop)
						{
							IsPlaying = false;
							OnAnimationComplete?.Invoke(this);
						}
					}
				}
			}
			else if (Reverse)
			{
				_currentFrame--;
				if (_currentFrame < 0)
				{
					if (Loop)
					{
						_currentFrame = Frames.Count - 1;
					}
					else
					{
						_currentFrame = 0;
						IsPlaying = false;
						OnAnimationComplete?.Invoke(this);
					}
				}
			}
			else
			{
				_currentFrame++;
				if (_currentFrame >= Frames.Count)
				{
					if (Loop)
					{
						_currentFrame = 0;
					}
					else
					{
						_currentFrame = Frames.Count - 1;
						IsPlaying = false;
						OnAnimationComplete?.Invoke(this);
					}
				}
			}

			if (oldFrame != _currentFrame)
				OnFrameChanged?.Invoke(this, _currentFrame);
		}

		private void DrawFrame(FishUI UI, ImageRef image)
		{
			Vector2 pos = GetAbsolutePosition();
			Vector2 size = GetAbsoluteSize();
			FishColor drawColor = EffectiveColor;

			switch (ScaleMode)
			{
				case ImageScaleMode.None:
					// Draw at original size, centered
					Vector2 offset = (size - new Vector2(image.Width, image.Height)) / 2;
					UI.Graphics.DrawImage(image, pos + offset, 0f, 1f, drawColor);
					break;

				case ImageScaleMode.Stretch:
					// Stretch to fill the control bounds
					UI.Graphics.DrawImage(image, pos, size, 0f, 1f, drawColor);
					break;

				case ImageScaleMode.Fit:
					// Scale to fit while maintaining aspect ratio
					{
						float imgAspect = (float)image.Width / image.Height;
						float ctrlAspect = size.X / size.Y;
						Vector2 drawSize;
						if (imgAspect > ctrlAspect)
						{
							drawSize = new Vector2(size.X, size.X / imgAspect);
						}
						else
						{
							drawSize = new Vector2(size.Y * imgAspect, size.Y);
						}
						Vector2 drawOffset = (size - drawSize) / 2;
						UI.Graphics.DrawImage(image, pos + drawOffset, drawSize, 0f, 1f, drawColor);
					}
					break;

				case ImageScaleMode.Fill:
					// Scale to fill while maintaining aspect ratio (may crop)
					{
						float imgAspect = (float)image.Width / image.Height;
						float ctrlAspect = size.X / size.Y;
						Vector2 drawSize;
						if (imgAspect < ctrlAspect)
						{
							drawSize = new Vector2(size.X, size.X / imgAspect);
						}
						else
						{
							drawSize = new Vector2(size.Y * imgAspect, size.Y);
						}
						Vector2 drawOffset = (size - drawSize) / 2;

						UI.Graphics.PushScissor(pos, size);
						UI.Graphics.DrawImage(image, pos + drawOffset, drawSize, 0f, 1f, drawColor);
						UI.Graphics.PopScissor();
					}
					break;
			}
		}
	}
}
