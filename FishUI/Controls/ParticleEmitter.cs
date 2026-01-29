using System;
using System.Collections.Generic;
using System.Numerics;

namespace FishUI.Controls
{
	/// <summary>
	/// Blend mode for particle rendering.
	/// </summary>
	public enum ParticleBlendMode
	{
		/// <summary>Normal alpha blending.</summary>
		Alpha,
		/// <summary>Additive blending for glow effects.</summary>
		Additive,
		/// <summary>Multiply blending for darkening effects.</summary>
		Multiply
	}

	/// <summary>
	/// Shape of the emission area.
	/// </summary>
	public enum EmitterShape
	{
		/// <summary>Emit from a single point.</summary>
		Point,
		/// <summary>Emit from within a rectangle.</summary>
		Rectangle,
		/// <summary>Emit from within a circle.</summary>
		Circle,
		/// <summary>Emit from the edge of a circle.</summary>
		CircleEdge
	}

	/// <summary>
	/// Represents a single particle in the system.
	/// </summary>
	public class Particle
	{
		/// <summary>Current position relative to emitter.</summary>
		public Vector2 Position;

		/// <summary>Current velocity in pixels per second.</summary>
		public Vector2 Velocity;

		/// <summary>Acceleration applied each frame.</summary>
		public Vector2 Acceleration;

		/// <summary>Current rotation in radians.</summary>
		public float Rotation;

		/// <summary>Rotation speed in radians per second.</summary>
		public float RotationSpeed;

		/// <summary>Current scale factor.</summary>
		public float Scale;

		/// <summary>Scale change per second.</summary>
		public float ScaleSpeed;

		/// <summary>Current color and alpha.</summary>
		public FishColor Color;

		/// <summary>Starting color for interpolation.</summary>
		public FishColor StartColor;

		/// <summary>Ending color for interpolation.</summary>
		public FishColor EndColor;

		/// <summary>Time remaining in seconds.</summary>
		public float Lifetime;

		/// <summary>Total lifetime for normalization.</summary>
		public float MaxLifetime;

		/// <summary>Whether this particle is active.</summary>
		public bool IsAlive => Lifetime > 0;

		/// <summary>Normalized age (0 = just born, 1 = about to die).</summary>
		public float NormalizedAge => MaxLifetime > 0 ? 1f - (Lifetime / MaxLifetime) : 1f;
	}

	/// <summary>
	/// Configuration for particle emission.
	/// </summary>
	public class ParticleConfig
	{
		/// <summary>Minimum initial velocity.</summary>
		public Vector2 VelocityMin { get; set; } = new Vector2(-50, -100);

		/// <summary>Maximum initial velocity.</summary>
		public Vector2 VelocityMax { get; set; } = new Vector2(50, -50);

		/// <summary>Constant acceleration (e.g., gravity).</summary>
		public Vector2 Acceleration { get; set; } = new Vector2(0, 100);

		/// <summary>Minimum particle lifetime in seconds.</summary>
		public float LifetimeMin { get; set; } = 0.5f;

		/// <summary>Maximum particle lifetime in seconds.</summary>
		public float LifetimeMax { get; set; } = 2f;

		/// <summary>Minimum initial scale.</summary>
		public float ScaleMin { get; set; } = 0.5f;

		/// <summary>Maximum initial scale.</summary>
		public float ScaleMax { get; set; } = 1.5f;

		/// <summary>Scale change per second (negative = shrink).</summary>
		public float ScaleSpeed { get; set; } = -0.3f;

		/// <summary>Minimum rotation speed in radians per second.</summary>
		public float RotationSpeedMin { get; set; } = -2f;

		/// <summary>Maximum rotation speed in radians per second.</summary>
		public float RotationSpeedMax { get; set; } = 2f;

		/// <summary>Starting color.</summary>
		public FishColor StartColor { get; set; } = new FishColor(255, 255, 255, 255);

		/// <summary>Ending color (interpolated over lifetime).</summary>
		public FishColor EndColor { get; set; } = new FishColor(255, 255, 255, 0);

		/// <summary>Easing function for color interpolation.</summary>
		public Easing ColorEasing { get; set; } = Easing.Linear;

		/// <summary>Creates a default particle configuration.</summary>
		public static ParticleConfig Default => new ParticleConfig();

		/// <summary>Creates a fire-like particle configuration.</summary>
		public static ParticleConfig Fire => new ParticleConfig
		{
			VelocityMin = new Vector2(-20, -80),
			VelocityMax = new Vector2(20, -40),
			Acceleration = new Vector2(0, -20),
			LifetimeMin = 0.3f,
			LifetimeMax = 0.8f,
			ScaleMin = 0.3f,
			ScaleMax = 0.8f,
			ScaleSpeed = -0.5f,
			StartColor = new FishColor(255, 200, 50, 255),
			EndColor = new FishColor(255, 50, 0, 0),
			ColorEasing = Easing.EaseOutQuad
		};

		/// <summary>Creates a sparkle/confetti particle configuration.</summary>
		public static ParticleConfig Sparkle => new ParticleConfig
		{
			VelocityMin = new Vector2(-100, -150),
			VelocityMax = new Vector2(100, -50),
			Acceleration = new Vector2(0, 200),
			LifetimeMin = 1f,
			LifetimeMax = 2f,
			ScaleMin = 0.2f,
			ScaleMax = 0.5f,
			ScaleSpeed = 0f,
			RotationSpeedMin = -5f,
			RotationSpeedMax = 5f,
			StartColor = new FishColor(255, 255, 100, 255),
			EndColor = new FishColor(255, 255, 100, 0)
		};

		/// <summary>Creates a smoke particle configuration.</summary>
		public static ParticleConfig Smoke => new ParticleConfig
		{
			VelocityMin = new Vector2(-15, -30),
			VelocityMax = new Vector2(15, -15),
			Acceleration = new Vector2(0, -5),
			LifetimeMin = 1f,
			LifetimeMax = 3f,
			ScaleMin = 0.5f,
			ScaleMax = 1f,
			ScaleSpeed = 0.5f,
			RotationSpeedMin = -0.5f,
			RotationSpeedMax = 0.5f,
			StartColor = new FishColor(100, 100, 100, 150),
			EndColor = new FishColor(50, 50, 50, 0),
			ColorEasing = Easing.EaseInQuad
		};

		/// <summary>Creates an explosion burst configuration.</summary>
		public static ParticleConfig Explosion => new ParticleConfig
		{
			VelocityMin = new Vector2(-200, -200),
			VelocityMax = new Vector2(200, 200),
			Acceleration = new Vector2(0, 50),
			LifetimeMin = 0.3f,
			LifetimeMax = 0.6f,
			ScaleMin = 0.5f,
			ScaleMax = 1.5f,
			ScaleSpeed = -1f,
			StartColor = new FishColor(255, 200, 100, 255),
			EndColor = new FishColor(255, 50, 0, 0),
			ColorEasing = Easing.EaseOutQuad
		};
	}

	/// <summary>
	/// A particle emitter control that spawns and manages animated particles.
	/// Supports image-based particles, color interpolation, and various emission shapes.
	/// </summary>
	public class ParticleEmitter : Control
	{
		private readonly List<Particle> _particles = new();
		private readonly Random _random = new();
		private float _emitAccumulator;

		/// <summary>
		/// Gets or sets the particle configuration.
		/// </summary>
		public ParticleConfig Config { get; set; } = new ParticleConfig();

		/// <summary>
		/// Gets or sets the optional particle image. If null, draws colored rectangles.
		/// </summary>
		public ImageRef ParticleImage { get; set; }

		/// <summary>
		/// Gets or sets the size of particles when no image is used.
		/// </summary>
		public Vector2 ParticleSize { get; set; } = new Vector2(8, 8);

		/// <summary>
		/// Gets or sets the blend mode for particle rendering.
		/// </summary>
		public ParticleBlendMode BlendMode { get; set; } = ParticleBlendMode.Alpha;

		/// <summary>
		/// Gets or sets the emission shape.
		/// </summary>
		public EmitterShape Shape { get; set; } = EmitterShape.Point;

		/// <summary>
		/// Gets or sets the emission rate (particles per second). Set to 0 for manual burst emission.
		/// </summary>
		public float EmissionRate { get; set; } = 10f;

		/// <summary>
		/// Gets or sets whether the emitter is actively emitting particles.
		/// </summary>
		public bool IsEmitting { get; set; } = true;

		/// <summary>
		/// Gets or sets the maximum number of particles. Older particles are removed when exceeded.
		/// </summary>
		public int MaxParticles { get; set; } = 500;

		/// <summary>
		/// Gets the current number of active particles.
		/// </summary>
		public int ParticleCount => _particles.Count;

		/// <summary>
		/// Creates a new particle emitter.
		/// </summary>
		public ParticleEmitter()
		{
			Size = new Vector2(100, 100);
		}

		/// <summary>
		/// Emits a burst of particles.
		/// </summary>
		/// <param name="count">Number of particles to emit.</param>
		public void Burst(int count)
		{
			for (int i = 0; i < count; i++)
			{
				EmitParticle();
			}
		}

		/// <summary>
		/// Clears all active particles.
		/// </summary>
		public void Clear()
		{
			_particles.Clear();
		}

		/// <summary>
		/// Emits a single particle at the specified position (relative to emitter).
		/// </summary>
		public void EmitAt(Vector2 localPosition)
		{
			if (_particles.Count >= MaxParticles)
			{
				// Remove oldest particle
				_particles.RemoveAt(0);
			}

			var particle = CreateParticle();
			particle.Position = localPosition;
			_particles.Add(particle);
		}

		private void EmitParticle()
		{
			if (_particles.Count >= MaxParticles)
			{
				_particles.RemoveAt(0);
			}

			var particle = CreateParticle();
			particle.Position = GetEmitPosition();
			_particles.Add(particle);
		}

		private Particle CreateParticle()
		{
			var config = Config;
			float lifetime = RandomRange(config.LifetimeMin, config.LifetimeMax);

			return new Particle
			{
				Velocity = new Vector2(
					RandomRange(config.VelocityMin.X, config.VelocityMax.X),
					RandomRange(config.VelocityMin.Y, config.VelocityMax.Y)
				),
				Acceleration = config.Acceleration,
				Rotation = RandomRange(0, MathF.PI * 2),
				RotationSpeed = RandomRange(config.RotationSpeedMin, config.RotationSpeedMax),
				Scale = RandomRange(config.ScaleMin, config.ScaleMax),
				ScaleSpeed = config.ScaleSpeed,
				StartColor = config.StartColor,
				EndColor = config.EndColor,
				Color = config.StartColor,
				Lifetime = lifetime,
				MaxLifetime = lifetime
			};
		}

		private Vector2 GetEmitPosition()
		{
			Vector2 center = Size / 2;

			return Shape switch
			{
				EmitterShape.Point => center,
				EmitterShape.Rectangle => new Vector2(
					RandomRange(0, Size.X),
					RandomRange(0, Size.Y)
				),
				EmitterShape.Circle => GetRandomPointInCircle(center, MathF.Min(Size.X, Size.Y) / 2),
				EmitterShape.CircleEdge => GetRandomPointOnCircle(center, MathF.Min(Size.X, Size.Y) / 2),
				_ => center
			};
		}

		private Vector2 GetRandomPointInCircle(Vector2 center, float radius)
		{
			float angle = RandomRange(0, MathF.PI * 2);
			float r = MathF.Sqrt((float)_random.NextDouble()) * radius;
			return center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * r;
		}

		private Vector2 GetRandomPointOnCircle(Vector2 center, float radius)
		{
			float angle = RandomRange(0, MathF.PI * 2);
			return center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
		}

		private float RandomRange(float min, float max)
		{
			return min + (float)_random.NextDouble() * (max - min);
		}

		/// <inheritdoc/>
		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			// Update emission
			if (IsEmitting && EmissionRate > 0)
			{
				_emitAccumulator += Dt;
				float emitInterval = 1f / EmissionRate;

				while (_emitAccumulator >= emitInterval)
				{
					_emitAccumulator -= emitInterval;
					EmitParticle();
				}
			}

			// Update and render particles
			Vector2 basePos = GetAbsolutePosition();

			for (int i = _particles.Count - 1; i >= 0; i--)
			{
				var p = _particles[i];

				// Update physics
				p.Velocity += p.Acceleration * Dt;
				p.Position += p.Velocity * Dt;
				p.Rotation += p.RotationSpeed * Dt;
				p.Scale += p.ScaleSpeed * Dt;
				if (p.Scale < 0) p.Scale = 0;

				// Update lifetime
				p.Lifetime -= Dt;

				if (!p.IsAlive)
				{
					_particles.RemoveAt(i);
					continue;
				}

				// Interpolate color
				float t = EasingFunctions.Apply(Config.ColorEasing, p.NormalizedAge);
				p.Color = LerpColor(p.StartColor, p.EndColor, t);

				// Render particle
				Vector2 drawPos = basePos + p.Position;
				RenderParticle(UI, p, drawPos);
			}
		}

		private void RenderParticle(FishUI UI, Particle p, Vector2 position)
		{
			if (ParticleImage != null)
			{
				// Draw image particle
				Vector2 imgSize = new Vector2(ParticleImage.Width, ParticleImage.Height);
				Vector2 scaledSize = imgSize * p.Scale;
				Vector2 centeredPos = position - scaledSize / 2;

				UI.Graphics.DrawImage(ParticleImage, centeredPos, scaledSize, p.Rotation, 1f, p.Color);
			}
			else
			{
				// Draw rectangle particle
				Vector2 scaledSize = ParticleSize * p.Scale;
				Vector2 centeredPos = position - scaledSize / 2;

				// For simplicity, draw as rectangle (rotation not applied for rectangles)
				UI.Graphics.DrawRectangle(centeredPos, scaledSize, p.Color);
			}
		}

		private static FishColor LerpColor(FishColor a, FishColor b, float t)
		{
			return new FishColor(
				(byte)(a.R + (b.R - a.R) * t),
				(byte)(a.G + (b.G - a.G) * t),
				(byte)(a.B + (b.B - a.B) * t),
				(byte)(a.A + (b.A - a.A) * t)
			);
		}
	}
}
