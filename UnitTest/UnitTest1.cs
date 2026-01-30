using System.Numerics;
using FishUI;
using FishUI.Controls;

namespace UnitTest
{
	/// <summary>
	/// Tests for FishUI core system initialization and basic functionality.
	/// </summary>
	public class FishUICoreTests
	{
		[Fact]
		public void FishUI_CanBeCreatedWithMocks()
		{
			using var fixture = new FishUITestFixture();

			Assert.NotNull(fixture.UI);
			Assert.NotNull(fixture.Graphics);
			Assert.NotNull(fixture.Input);
			Assert.NotNull(fixture.Events);
			Assert.NotNull(fixture.FileSystem);
		}

		[Fact]
		public void FishUI_HasCorrectDimensions()
		{
			using var fixture = new FishUITestFixture(1024, 768);

			Assert.Equal(1024, fixture.UI.Width);
			Assert.Equal(768, fixture.UI.Height);
		}

		[Fact]
		public void FishUI_GetAllControls_ReturnsEmptyWhenNoControls()
		{
			using var fixture = new FishUITestFixture();

			var controls = fixture.UI.GetAllControls();

			Assert.Empty(controls);
		}

		[Fact]
		public void FishUI_AddControl_AddsToControlsList()
		{
			using var fixture = new FishUITestFixture();

			var button = new Button { Size = new Vector2(100, 30) };
			fixture.UI.AddControl(button);

			var controls = fixture.UI.GetAllControls();

			Assert.Single(controls);
			Assert.Contains(button, controls);
		}

		[Fact]
		public void FishUI_Update_DoesNotThrow()
		{
			using var fixture = new FishUITestFixture();

			var button = new Button { Size = new Vector2(100, 30) };
			fixture.UI.AddControl(button);

			var exception = Record.Exception(() => fixture.Update());

			Assert.Null(exception);
		}

		[Fact]
		public void FishUI_Draw_DoesNotThrow()
		{
			using var fixture = new FishUITestFixture();

			var button = new Button { Size = new Vector2(100, 30) };
			fixture.UI.AddControl(button);

			fixture.Update();
			var exception = Record.Exception(() => fixture.Draw());

			Assert.Null(exception);
		}

		[Fact]
		public void FishUI_DoubleClickTime_HasDefault()
		{
			using var fixture = new FishUITestFixture();

			Assert.True(fixture.UI.DoubleClickTime > 0);
		}

		[Fact]
		public void FishUI_DoubleClickDistance_HasDefault()
		{
			using var fixture = new FishUITestFixture();

			Assert.True(fixture.UI.DoubleClickDistance > 0);
		}

		[Fact]
		public void FishUI_Hotkeys_IsNotNull()
		{
			using var fixture = new FishUITestFixture();

			Assert.NotNull(fixture.UI.Hotkeys);
		}

		[Fact]
		public void FishUI_VirtualMouse_IsNotNull()
		{
			using var fixture = new FishUITestFixture();

			Assert.NotNull(fixture.UI.VirtualMouse);
		}

		[Fact]
		public void FishUI_EventHandlers_IsNotNull()
		{
			using var fixture = new FishUITestFixture();

			Assert.NotNull(fixture.UI.EventHandlers);
		}

		[Fact]
		public void FishUI_Animations_IsNotNull()
		{
			using var fixture = new FishUITestFixture();

			Assert.NotNull(fixture.UI.Animations);
		}
	}
}
