using System.Numerics;
using FishUI;
using FishUI.Controls;

namespace UnitTest
{
	/// <summary>
	/// Tests for control hierarchy (add/remove children, parent relationships).
	/// </summary>
	public class ControlHierarchyTests
	{
		[Fact]
		public void AddChild_SetsParentReference()
		{
			using var fixture = new FishUITestFixture();

			var panel = new Panel { Size = new Vector2(200, 200) };
			var button = new Button { Size = new Vector2(100, 30) };

			fixture.UI.AddControl(panel);
			panel.AddChild(button);

			Assert.Single(panel.Children);
			Assert.Contains(button, panel.Children);
		}

		[Fact]
		public void AddChild_AssignsIncrementalZDepth()
		{
			using var fixture = new FishUITestFixture();

			var panel = new Panel { Size = new Vector2(200, 200) };
			var button1 = new Button { Size = new Vector2(100, 30) };
			var button2 = new Button { Size = new Vector2(100, 30) };
			var button3 = new Button { Size = new Vector2(100, 30) };

			fixture.UI.AddControl(panel);
			panel.AddChild(button1);
			panel.AddChild(button2);
			panel.AddChild(button3);

			Assert.Equal(0, button1.ZDepth);
			Assert.Equal(1, button2.ZDepth);
			Assert.Equal(2, button3.ZDepth);
		}

		[Fact]
		public void RemoveChild_RemovesFromChildrenList()
		{
			using var fixture = new FishUITestFixture();

			var panel = new Panel { Size = new Vector2(200, 200) };
			var button = new Button { Size = new Vector2(100, 30) };

			fixture.UI.AddControl(panel);
			panel.AddChild(button);
			panel.RemoveChild(button);

			Assert.Empty(panel.Children);
		}

		[Fact]
		public void Unparent_RemovesControlFromParent()
		{
			using var fixture = new FishUITestFixture();

			var panel = new Panel { Size = new Vector2(200, 200) };
			var button = new Button { Size = new Vector2(100, 30) };

			fixture.UI.AddControl(panel);
			panel.AddChild(button);
			button.Unparent();

			Assert.Empty(panel.Children);
		}

		[Fact]
		public void FindChildByType_ReturnsCorrectChild()
		{
			using var fixture = new FishUITestFixture();

			var panel = new Panel { Size = new Vector2(200, 200) };
			var button = new Button { Size = new Vector2(100, 30) };
			var label = new Label { Text = "Test" };

			fixture.UI.AddControl(panel);
			panel.AddChild(button);
			panel.AddChild(label);

			var foundLabel = panel.FindChildByType<Label>();
			var foundButton = panel.FindChildByType<Button>();

			Assert.Same(label, foundLabel);
			Assert.Same(button, foundButton);
		}

		[Fact]
		public void FindChildByType_ReturnsNullWhenNotFound()
		{
			using var fixture = new FishUITestFixture();

			var panel = new Panel { Size = new Vector2(200, 200) };
			var button = new Button { Size = new Vector2(100, 30) };

			fixture.UI.AddControl(panel);
			panel.AddChild(button);

			var foundCheckBox = panel.FindChildByType<CheckBox>();

			Assert.Null(foundCheckBox);
		}

		[Fact]
		public void GetAllChildren_ReturnsAllChildren()
		{
			using var fixture = new FishUITestFixture();

			var panel = new Panel { Size = new Vector2(200, 200) };
			var button1 = new Button { Size = new Vector2(100, 30) };
			var button2 = new Button { Size = new Vector2(100, 30) };

			fixture.UI.AddControl(panel);
			panel.AddChild(button1);
			panel.AddChild(button2);

			var children = panel.GetAllChildren();

			Assert.Equal(2, children.Length);
			Assert.Contains(button1, children);
			Assert.Contains(button2, children);
		}

		[Fact]
		public void AddControl_ToUI_SetsInternalReference()
		{
			using var fixture = new FishUITestFixture();

			var button = new Button { Size = new Vector2(100, 30) };

			fixture.UI.AddControl(button);

			// Button should be registered with UI
			var allControls = fixture.UI.GetAllControls();
			Assert.Contains(button, allControls);
		}

		[Fact]
		public void RemoveControl_FromUI_RemovesFromList()
		{
			using var fixture = new FishUITestFixture();

			var button = new Button { Size = new Vector2(100, 30) };

			fixture.UI.AddControl(button);
			fixture.UI.RemoveControl(button);

			var allControls = fixture.UI.GetAllControls();
			Assert.DoesNotContain(button, allControls);
		}

		[Fact]
		public void NestedChildren_AreAccessibleRecursively()
		{
			using var fixture = new FishUITestFixture();

			var panel = new Panel { Size = new Vector2(200, 200) };
			var childPanel = new Panel { Size = new Vector2(150, 150) };
			var button = new Button { Size = new Vector2(100, 30) };

			fixture.UI.AddControl(panel);
			panel.AddChild(childPanel);
			childPanel.AddChild(button);

			// Child panel should contain button
			Assert.Contains(button, childPanel.Children);

			// FindChildByType only searches direct children (not recursive)
			var foundButtonInPanel = panel.FindChildByType<Button>();
			Assert.Null(foundButtonInPanel); // Button is in childPanel, not panel

			// But the nested panel is found
			var foundPanel = panel.FindChildByType<Panel>();
			Assert.Same(childPanel, foundPanel);

			// And button is found in childPanel
			var foundButtonInChildPanel = childPanel.FindChildByType<Button>();
			Assert.Same(button, foundButtonInChildPanel);
		}
	}
}
