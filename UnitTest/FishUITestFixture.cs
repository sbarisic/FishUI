using FishUI;
using UnitTest.Mocks;

namespace UnitTest
{
	/// <summary>
	/// Test fixture that provides a configured FishUI instance with mock backends.
	/// </summary>
	public class FishUITestFixture : IDisposable
	{
		public FishUI.FishUI UI { get; }
		public FishUISettings Settings { get; }
		public MockFishUIGfx Graphics { get; }
		public MockFishUIInput Input { get; }
		public MockFishUIEvents Events { get; }
		public MockFishUIFileSystem FileSystem { get; }

		public FishUITestFixture(int width = 800, int height = 600)
		{
			Graphics = new MockFishUIGfx { WindowWidth = width, WindowHeight = height };
			Input = new MockFishUIInput();
			Events = new MockFishUIEvents();
			FileSystem = new MockFishUIFileSystem();
			Settings = new FishUISettings();

			UI = new FishUI.FishUI(Settings, Graphics, Input, Events, FileSystem);
			UI.Width = width;
			UI.Height = height;
		}

		private float _elapsedTime = 0f;

		/// <summary>
		/// Simulates a single frame update and draw (uses the public Tick method).
		/// </summary>
		public void Update(float dt = 0.016f)
		{
			_elapsedTime += dt;
			UI.Tick(dt, _elapsedTime);
			Input.EndFrame();
		}

		/// <summary>
		/// Simulates drawing a frame. Note: Draw is integrated into Tick, this is a no-op.
		/// </summary>
		public void Draw(float dt = 0.016f)
		{
			// Drawing is already handled by Tick
		}

		/// <summary>
		/// Reset all mock state for a clean test.
		/// </summary>
		public void Reset()
		{
			Graphics.Reset();
			Input.Reset();
			Events.Reset();
			FileSystem.Reset();
		}

		public void Dispose()
		{
			// Cleanup if needed
		}
	}
}
