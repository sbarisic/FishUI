using System.Numerics;
using FishUI;
using FishUI.Controls;

namespace UnitTest;

public class DropDownOverlayTests
{
	private static void UpdateInput(FishUITestFixture fixture, ref float elapsedTime)
	{
		const float deltaTime = 0.016f;
		elapsedTime += deltaTime;
		fixture.UI.TickUpdate(deltaTime, elapsedTime);
		fixture.Input.EndFrame();
	}

	[Fact]
	public void OpenListReceivesInputBeforeOverlappedLaterSibling()
	{
		using var fixture = new FishUITestFixture();
		float elapsedTime = 0;
		var root = new Panel { Size = new Vector2(300, 200) };
		var tabContent = new Panel { Size = new Vector2(300, 200), IsTransparent = true };
		var dropDown = new DropDown
		{
			Position = new Vector2(20, 20),
			Size = new Vector2(120, 24),
			CustomItemHeight = 18,
		};
		dropDown.AddItem("First");
		dropDown.AddItem("Second");
		tabContent.AddChild(dropDown);
		root.AddChild(tabContent);

		bool underlyingClicked = false;
		var underlyingButton = new Button
		{
			Position = new Vector2(20, 39),
			Size = new Vector2(120, 60),
		};
		underlyingButton.OnButtonPressed += (_, _, _) => underlyingClicked = true;
		root.AddChild(underlyingButton);
		fixture.UI.AddControl(root);

		dropDown.Open();
		fixture.Input.SimulateMouseMove(new Vector2(50, 50));
		UpdateInput(fixture, ref elapsedTime);
		fixture.Input.SimulateMouseMove(new Vector2(51, 50));
		UpdateInput(fixture, ref elapsedTime);
		fixture.Input.SimulateMouseDown(FishMouseButton.Left);
		UpdateInput(fixture, ref elapsedTime);
		fixture.Input.SimulateMouseUp(FishMouseButton.Left);
		UpdateInput(fixture, ref elapsedTime);

		Assert.Equal(0, dropDown.SelectedIndex);
		Assert.False(dropDown.IsOpen);
		Assert.False(underlyingClicked);
	}

	[Fact]
	public void OpenListClosesWhenAnAncestorBecomesHidden()
	{
		using var fixture = new FishUITestFixture();
		float elapsedTime = 0;
		var window = new Window { Size = new Vector2(300, 200) };
		var dropDown = new DropDown
		{
			Position = new Vector2(20, 20),
			Size = new Vector2(120, 24),
			CustomItemHeight = 18,
		};
		dropDown.AddItem("First");
		window.AddChild(dropDown);
		fixture.UI.AddControl(window);

		dropDown.Open();
		window.Visible = false;
		UpdateInput(fixture, ref elapsedTime);

		Assert.False(dropDown.IsOpen);
	}
}
