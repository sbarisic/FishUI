using System.Numerics;
using FishUI;
using FishUI.Controls;

namespace UnitTest
{
	/// <summary>
	/// Tests for control positioning and layout.
	/// </summary>
	public class ControlPositionTests
	{
		[Fact]
		public void Position_Absolute_SetsFixedPosition()
		{
			using var fixture = new FishUITestFixture();

			var button = new Button
			{
				Size = new Vector2(100, 30),
				Position = new FishUIPosition(PositionMode.Absolute, new Vector2(50, 100))
			};

			fixture.UI.AddControl(button);
			fixture.Update();

			var globalPos = button.GetAbsolutePosition();
			Assert.Equal(50, globalPos.X);
			Assert.Equal(100, globalPos.Y);
		}

		[Fact]
		public void Position_Relative_SetsRelativePosition()
		{
			using var fixture = new FishUITestFixture();

			var panel = new Panel { Size = new Vector2(200, 200) };
			var button = new Button
			{
				Size = new Vector2(50, 30),
				Position = new FishUIPosition(PositionMode.Relative, new Vector2(50, 50))
			};

			fixture.UI.AddControl(panel);
			panel.AddChild(button);
			fixture.Update();

			// Button position is relative to parent
			var absPos = button.GetAbsolutePosition();
			Assert.Equal(50, absPos.X);
			Assert.Equal(50, absPos.Y);
		}

		[Fact]
		public void Position_Docked_Fill_FillsParent()
		{
			using var fixture = new FishUITestFixture();

			var panel = new Panel { Size = new Vector2(200, 200) };
			var childPanel = new Panel
			{
				Position = new FishUIPosition(PositionMode.Docked, DockMode.Fill, Vector4.Zero, Vector2.Zero)
			};

			fixture.UI.AddControl(panel);
			panel.AddChild(childPanel);
			fixture.Update();

			// Child should fill parent
			var childSize = childPanel.GetAbsoluteSize();
			Assert.Equal(panel.Size.X, childSize.X);
			Assert.Equal(panel.Size.Y, childSize.Y);
		}

		[Fact]
		public void ChildPosition_IsRelativeToParent()
		{
			using var fixture = new FishUITestFixture();

			var panel = new Panel
			{
				Size = new Vector2(200, 200),
				Position = new FishUIPosition(PositionMode.Relative, new Vector2(100, 100))
			};
			var button = new Button
			{
				Size = new Vector2(50, 30),
				Position = new FishUIPosition(PositionMode.Relative, new Vector2(20, 20))
			};

			fixture.UI.AddControl(panel);
			panel.AddChild(button);
			fixture.Update();

			// Button's global position should be panel position + local position
			var globalPos = button.GetAbsolutePosition();
			Assert.Equal(120, globalPos.X); // 100 + 20
			Assert.Equal(120, globalPos.Y); // 100 + 20
		}

		[Fact]
		public void FishUIMargin_AllSame_SetsAllSides()
		{
			var margin = new FishUIMargin(10);

			Assert.Equal(10, margin.Left);
			Assert.Equal(10, margin.Top);
			Assert.Equal(10, margin.Right);
			Assert.Equal(10, margin.Bottom);
		}

		[Fact]
		public void FishUIMargin_HorizontalVertical_SetsCorrectly()
		{
			var margin = new FishUIMargin(10, 5); // vertical, horizontal

			Assert.Equal(5, margin.Left);
			Assert.Equal(10, margin.Top);
			Assert.Equal(5, margin.Right);
			Assert.Equal(10, margin.Bottom);
		}

		[Fact]
		public void FishUIMargin_AllDifferent_SetsCorrectly()
		{
			var margin = new FishUIMargin(2, 3, 4, 1); // top, right, bottom, left

			Assert.Equal(1, margin.Left);
			Assert.Equal(2, margin.Top);
			Assert.Equal(3, margin.Right);
			Assert.Equal(4, margin.Bottom);
		}
	}
}
