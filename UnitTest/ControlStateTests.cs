using System.Numerics;
using FishUI;
using FishUI.Controls;

namespace UnitTest
{
	/// <summary>
	/// Tests for control visibility and enabled state.
	/// </summary>
	public class ControlStateTests
	{
		[Fact]
		public void Visible_DefaultsToTrue()
		{
			var button = new Button { Size = new Vector2(100, 30) };

			Assert.True(button.Visible);
		}

		[Fact]
		public void Visible_CanBeSetToFalse()
		{
			var button = new Button { Size = new Vector2(100, 30) };
			button.Visible = false;

			Assert.False(button.Visible);
		}

		[Fact]
		public void Disabled_DefaultsToFalse()
		{
			var button = new Button { Size = new Vector2(100, 30) };

			Assert.False(button.Disabled);
		}

		[Fact]
		public void Disabled_CanBeSetToTrue()
		{
			var button = new Button { Size = new Vector2(100, 30) };
			button.Disabled = true;

			Assert.True(button.Disabled);
		}

		[Fact]
		public void ID_CanBeSetAndRetrieved()
		{
			var button = new Button { ID = "myButton", Size = new Vector2(100, 30) };

			Assert.Equal("myButton", button.ID);
		}

		[Fact]
		public void DesignerName_CanBeSetAndRetrieved()
		{
			var button = new Button { DesignerName = "btnSubmit", Size = new Vector2(100, 30) };

			Assert.Equal("btnSubmit", button.DesignerName);
		}

		[Fact]
		public void ZDepth_DefaultsToZero()
		{
			var button = new Button { Size = new Vector2(100, 30) };

			Assert.Equal(0, button.ZDepth);
		}

		[Fact]
		public void ZDepth_CanBeSet()
		{
			var button = new Button { Size = new Vector2(100, 30), ZDepth = 5 };

			Assert.Equal(5, button.ZDepth);
		}

		[Fact]
		public void AlwaysOnTop_DefaultsToFalse()
		{
			var button = new Button { Size = new Vector2(100, 30) };

			Assert.False(button.AlwaysOnTop);
		}

		[Fact]
		public void AlwaysOnTop_CanBeSetToTrue()
		{
			var button = new Button { Size = new Vector2(100, 30), AlwaysOnTop = true };

			Assert.True(button.AlwaysOnTop);
		}

		[Fact]
		public void Size_CanBeSetAndRetrieved()
		{
			var button = new Button { Size = new Vector2(120, 40) };

			Assert.Equal(new Vector2(120, 40), button.Size);
		}

		[Fact]
		public void Margin_CanBeSetAndRetrieved()
		{
			var button = new Button
			{
				Size = new Vector2(100, 30),
				Margin = new FishUIMargin(10, 15, 20, 5) // top, right, bottom, left
			};

			Assert.Equal(5, button.Margin.Left);
			Assert.Equal(10, button.Margin.Top);
			Assert.Equal(15, button.Margin.Right);
			Assert.Equal(20, button.Margin.Bottom);
		}

		[Fact]
		public void Padding_CanBeSetAndRetrieved()
		{
			var panel = new Panel
			{
				Size = new Vector2(200, 200),
				Padding = new FishUIMargin(8, 8, 8, 8)
			};

			Assert.Equal(8, panel.Padding.Left);
			Assert.Equal(8, panel.Padding.Top);
			Assert.Equal(8, panel.Padding.Right);
			Assert.Equal(8, panel.Padding.Bottom);
		}
	}
}
