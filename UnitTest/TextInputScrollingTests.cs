using System.Numerics;
using FishUI;
using FishUI.Controls;

namespace UnitTest
{
	public class TextInputScrollingTests
	{
		private static FishUITestFixture CreateFixture(float scale = 1f)
		{
			var fixture = new FishUITestFixture();
			fixture.Settings.UIScale = scale;
			fixture.Settings.FontDefault = new FontRef();
			fixture.Settings.FontTextboxDefault = new FontRef();
			return fixture;
		}

		private static MultiLineEditbox AddEditor(FishUITestFixture fixture, Vector2 size, string text)
		{
			var editor = new MultiLineEditbox
			{
				Size = size,
				TextPadding = 0,
				Text = text,
				WordWrap = false
			};
			fixture.UI.AddControl(editor);
			fixture.Update();
			return editor;
		}

		[Fact]
		public void MultiLine_ExactFit_DoesNotShowHorizontalScrollBar()
		{
			using var fixture = CreateFixture();
			MultiLineEditbox editor = AddEditor(fixture, new Vector2(80, 40), "0123456789");

			Assert.Null(editor.FindChildByType<ScrollBarH>());
			Assert.Equal(0, editor.HorizontalScrollOffsetPixels);
		}

		[Fact]
		public void MultiLine_HorizontalBarCanCauseVerticalOverflow()
		{
			using var fixture = CreateFixture();
			MultiLineEditbox editor = AddEditor(fixture, new Vector2(80, 24), "01234567890");

			Assert.True(editor.FindChildByType<ScrollBarH>()?.Visible);
			Assert.True(editor.FindChildByType<ScrollBarV>()?.Visible);
		}

		[Fact]
		public void MultiLine_VerticalBarCanCauseHorizontalOverflow()
		{
			using var fixture = CreateFixture();
			MultiLineEditbox editor = AddEditor(fixture, new Vector2(80, 48),
				"123456789\n1\n2\n3");

			Assert.True(editor.FindChildByType<ScrollBarV>()?.Visible);
			Assert.True(editor.FindChildByType<ScrollBarH>()?.Visible);
		}

		[Fact]
		public void MultiLine_HiddenHorizontalBar_StillFollowsProgrammaticCaret()
		{
			using var fixture = CreateFixture();
			var editor = new MultiLineEditbox
			{
				Size = new Vector2(80, 40),
				TextPadding = 0,
				ShowHorizontalScrollBar = false,
				Text = "01234567890123456789"
			};
			fixture.UI.AddControl(editor);
			editor.CursorColumn = editor.Lines[0].Length;

			fixture.Update();

			Assert.Null(editor.FindChildByType<ScrollBarH>());
			Assert.True(editor.HorizontalScrollOffsetPixels > 0);
		}

		[Fact]
		public void MultiLine_ManualScroll_IsPreservedUntilCaretMoves()
		{
			using var fixture = CreateFixture();
			MultiLineEditbox editor = AddEditor(fixture, new Vector2(80, 40), "01234567890123456789");
			editor.CursorColumn = editor.Lines[0].Length;
			fixture.Update();
			editor.HorizontalScrollOffsetPixels = 12;

			fixture.Update();

			Assert.Equal(12, editor.HorizontalScrollOffsetPixels);
			editor.CursorColumn = 0;
			fixture.Update();
			Assert.Equal(0, editor.HorizontalScrollOffsetPixels);
		}

		[Fact]
		public void MultiLine_WordWrapAndResizeRemoveObsoleteHorizontalScroll()
		{
			using var fixture = CreateFixture();
			MultiLineEditbox editor = AddEditor(fixture, new Vector2(80, 40), "01234567890123456789");
			editor.HorizontalScrollOffsetPixels = 20;

			editor.WordWrap = true;
			fixture.Update();

			Assert.Equal(0, editor.HorizontalScrollOffsetPixels);
			Assert.False(editor.FindChildByType<ScrollBarH>()?.Visible);

			editor.WordWrap = false;
			editor.Size = new Vector2(240, 40);
			fixture.Update();
			Assert.Equal(0, editor.HorizontalScrollOffsetPixels);
			Assert.False(editor.FindChildByType<ScrollBarH>()?.Visible);
		}

		[Fact]
		public void MultiLine_AnchoredParentResize_RebuildsViewportAndClipsText()
		{
			using var fixture = CreateFixture();
			var parent = new Panel { Size = new Vector2(160, 60) };
			var editor = new MultiLineEditbox
			{
				Size = parent.Size,
				Anchor = FishUIAnchor.All,
				TextPadding = 0,
				Text = "012345678901",
				WordWrap = false
			};
			parent.AddChild(editor);
			fixture.UI.AddControl(parent);
			fixture.Update();

			Assert.Null(editor.FindChildByType<ScrollBarH>());

			parent.Size = new Vector2(80, 60);
			fixture.Graphics.Reset();
			fixture.Update();

			Assert.Equal(80, editor.GetAbsoluteSize().X);
			Assert.True(editor.FindChildByType<ScrollBarH>()?.Visible);
			string[] scissors = fixture.Graphics.DrawCalls
				.Where(call => call.StartsWith("PushScissor"))
				.ToArray();
			Assert.True(scissors.Length >= 2);
			Assert.DoesNotContain(scissors, call => call.Contains("<160,"));
		}

		[Fact]
		public void MultiLine_ThumbRatioIncludesConditionalCaretMargin()
		{
			using var fixture = CreateFixture();
			MultiLineEditbox editor = AddEditor(fixture, new Vector2(80, 40), "01234567890123456789");
			ScrollBarH scrollBar = Assert.IsType<ScrollBarH>(editor.FindChildByType<ScrollBarH>());

			Assert.Equal(80f / 162f, scrollBar.ThumbWidth, 3);
		}

		[Fact]
		public void MultiLine_GutterStaysFixedWhileTextScrolls()
		{
			using var fixture = CreateFixture();
			var editor = new MultiLineEditbox
			{
				Size = new Vector2(100, 40),
				TextPadding = 0,
				ShowLineNumbers = true,
				LineNumberWidth = 16,
				Text = "01234567890123456789"
			};
			fixture.UI.AddControl(editor);
			fixture.Update();
			string gutterAtStart = fixture.Graphics.DrawCalls.Single(call => call.StartsWith("DrawTextColor(\"1\""));
			string textAtStart = fixture.Graphics.DrawCalls.Single(call => call.StartsWith("DrawTextColor(\"0123"));

			editor.HorizontalScrollOffsetPixels = 24;
			fixture.Graphics.Reset();
			fixture.Update();
			string gutterAfterScroll = fixture.Graphics.DrawCalls.Single(call => call.StartsWith("DrawTextColor(\"1\""));
			string textAfterScroll = fixture.Graphics.DrawCalls.Single(call => call.StartsWith("DrawTextColor(\"0123"));

			Assert.Equal(gutterAtStart, gutterAfterScroll);
			Assert.NotEqual(textAtStart, textAfterScroll);
		}

		[Fact]
		public void MultiLine_MouseHitTestingIncludesHorizontalOffset()
		{
			using var fixture = CreateFixture();
			MultiLineEditbox editor = AddEditor(fixture, new Vector2(80, 40), "01234567890123456789");
			editor.HorizontalScrollOffsetPixels = 32;

			editor.HandleMousePress(fixture.UI, new FishInputState(), FishMouseButton.Left,
				editor.GetAbsolutePosition() + new Vector2(1, 5));

			Assert.Equal(4, editor.CursorColumn);
		}

		[Fact]
		public void MultiLine_RuntimeScrollBars_AreNotSerialized()
		{
			using var fixture = CreateFixture();
			MultiLineEditbox editor = AddEditor(fixture, new Vector2(80, 24),
				"01234567890123456789\nsecond");

			string yaml = LayoutFormat.Serialize(fixture.UI);

			Assert.Contains("ShowHorizontalScrollBar", yaml);
			Assert.Contains("HorizontalScrollBarHeight", yaml);
			Assert.DoesNotContain("!ScrollBarH", yaml);
			Assert.DoesNotContain("!ScrollBarV", yaml);
		}

		[Fact]
		public void MultiLine_LegacySerializedScrollbar_IsDiscarded()
		{
			using var fixture = CreateFixture();
			const string yaml = @"- !MultiLineEditbox
  Text: |-
    Legacy line one
    Legacy line two
    Legacy line three
  Size: {X: 80, Y: 24}
  TextPadding: 0
  Children:
    - !ScrollBarV
      ThumbPosition: 0.5
";

			LayoutFormat.Deserialize(fixture.UI, yaml);
			fixture.Update();

			MultiLineEditbox editor = Assert.IsType<MultiLineEditbox>(Assert.Single(fixture.UI.GetAllControls()));
			Assert.True(editor.GetAllChildren().Count(child => child is ScrollBarV) <= 1);
			string serialized = LayoutFormat.Serialize(fixture.UI);
			Assert.DoesNotContain("!ScrollBarV", serialized);
			Assert.DoesNotContain("!ScrollBarH", serialized);
		}

		[Fact]
		public void Textbox_LongText_UsesOneContentScissorAndNoScrollbar()
		{
			using var fixture = CreateFixture();
			var textbox = new Textbox("01234567890123456789") { Size = new Vector2(80, 24) };
			fixture.UI.AddControl(textbox);
			fixture.UI.FocusControl(textbox);
			textbox.CursorPosition = textbox.Text.Length;

			fixture.Update();

			Assert.Null(textbox.FindChildByType<ScrollBarH>());
			int scissor = fixture.Graphics.DrawCalls.FindIndex(call => call.StartsWith("PushScissor"));
			int text = fixture.Graphics.DrawCalls.FindIndex(call => call.StartsWith("DrawText("));
			int caret = fixture.Graphics.DrawCalls.FindIndex(call => call.StartsWith("DrawLine("));
			int pop = fixture.Graphics.DrawCalls.FindIndex(call => call == "PopScissor");
			Assert.True(scissor >= 0 && text > scissor && caret > text && pop > caret);
		}

		[Fact]
		public void Textbox_ProgrammaticCursorAssignmentScrollsAndReturnsToStart()
		{
			using var fixture = CreateFixture();
			var textbox = new Textbox("01234567890123456789") { Size = new Vector2(80, 24) };
			fixture.UI.AddControl(textbox);
			textbox.CursorPosition = textbox.Text.Length;
			fixture.Update();

			Assert.Contains(fixture.Graphics.DrawCalls,
				call => call.StartsWith("DrawText(\"0123456789") && call.Contains("<-"));

			textbox.CursorPosition = 0;
			fixture.Graphics.Reset();
			fixture.Update();

			Assert.DoesNotContain(fixture.Graphics.DrawCalls,
				call => call.StartsWith("DrawText(\"0123456789") && call.Contains("<-"));
		}

		[Fact]
		public void ScrollbarArrowFocusesOwningEditor_AndTypingContinues()
		{
			using var fixture = CreateFixture();
			MultiLineEditbox editor = AddEditor(fixture, new Vector2(80, 40), "01234567890123456789");
			ScrollBarH scrollBar = Assert.IsType<ScrollBarH>(editor.FindChildByType<ScrollBarH>());
			fixture.UI.FocusControl(editor);
			Vector2 arrowCenter = scrollBar.GetAbsolutePosition() + new Vector2(5, 5);

			fixture.Input.SimulateMouseClick(FishMouseButton.Left, arrowCenter);
			fixture.Update();
			Assert.Same(editor, fixture.UI.InputActiveControl);
			fixture.Input.SimulateMouseUp(FishMouseButton.Left);
			fixture.Update();
			fixture.Input.SimulateCharTyped('x');
			fixture.Update();

			Assert.Contains('x', editor.Text);
		}

		[Fact]
		public void ScrollbarThumbDragKeepsFocusOnOwningEditor()
		{
			using var fixture = CreateFixture();
			MultiLineEditbox editor = AddEditor(fixture, new Vector2(80, 40), "01234567890123456789");
			ScrollBarH scrollBar = Assert.IsType<ScrollBarH>(editor.FindChildByType<ScrollBarH>());
			Button thumb = scrollBar.GetAllChildren().OfType<Button>().Single(button => button.Draggable);
			fixture.UI.FocusControl(editor);
			Vector2 thumbCenter = thumb.GetAbsolutePosition() + thumb.GetAbsoluteSize() / 2;

			fixture.Input.SimulateMouseClick(FishMouseButton.Left, thumbCenter);
			fixture.Update();
			fixture.Input.SimulateMouseMove(thumbCenter + new Vector2(8, 0));
			fixture.Update();

			Assert.Same(editor, fixture.UI.InputActiveControl);
			Assert.True(editor.HorizontalScrollOffsetPixels > 0);
		}

		[Theory]
		[InlineData(1.5f)]
		[InlineData(2f)]
		public void ScrollbarChildren_StayInLogicalCoordinates(float scale)
		{
			using var fixture = CreateFixture(scale);
			var horizontal = new ScrollBarH { Size = new Vector2(100, 15) };
			var vertical = new ScrollBarV { Position = new Vector2(120, 0), Size = new Vector2(15, 100) };
			fixture.UI.AddControl(horizontal);
			fixture.UI.AddControl(vertical);
			fixture.Update();

			Button right = horizontal.GetAllChildren().OfType<Button>().Single(button => !button.Draggable && button.Position.X > 0);
			Button bottom = vertical.GetAllChildren().OfType<Button>().Single(button => !button.Draggable && button.Position.Y > 0);
			Assert.Equal(85, right.Position.X);
			Assert.Equal(85, bottom.Position.Y);
		}

		[Fact]
		public void HorizontalScrollbar_ZeroRangeDragDoesNotRaiseChange()
		{
			using var fixture = CreateFixture();
			var scrollBar = new ScrollBarH { Size = new Vector2(100, 15), ThumbWidth = 1f };
			fixture.UI.AddControl(scrollBar);
			fixture.Update();
			Button thumb = scrollBar.GetAllChildren().OfType<Button>().Single(button => button.Draggable);
			int changes = 0;
			scrollBar.OnScrollChanged += (_, _, _) => changes++;

			thumb.HandleDrag(fixture.UI, Vector2.Zero, Vector2.One,
				new FishInputState { MouseDelta = new Vector2(10, 0) });

			Assert.Equal(0, changes);
		}
	}
}
