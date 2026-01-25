using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	/// <summary>
	/// Delegate for control drag events.
	/// </summary>
	/// <param name="Sender">The control being dragged.</param>
	/// <param name="MouseDelta">The mouse movement delta since the last frame.</param>
	public delegate void OnControlDraggedFunc(Control Sender, Vector2 MouseDelta);

	/// <summary>
	/// Base class for all FishUI controls. Provides common functionality for positioning,
	/// sizing, rendering, input handling, and parent-child relationships.
	/// </summary>
	public abstract class Control
	{
		[YamlIgnore]
		internal FishUI _FishUI;

		[YamlIgnore]
		protected FishUI FishUI
		{
			get
			{
				if (Parent != null)
					return Parent.FishUI;
				else
					return _FishUI;
			}
		}

		protected Control Parent;

		/// <summary>
		/// Child controls contained within this control.
		/// </summary>
		[YamlMember]
		public List<Control> Children = new List<Control>();

		/// <summary>
		/// Position of the control. Supports absolute, relative, and docked positioning modes.
		/// </summary>
		[YamlMember]
		public FishUIPosition Position;


		/// <summary>
		/// Size of the control in pixels.
		/// </summary>
		[YamlMember]
		public Vector2 Size;

		/// <summary>
		/// Margin (outer spacing) around this control. Affects position relative to siblings.
		/// </summary>
		[YamlMember]
		public FishUIMargin Margin;

		/// <summary>
		/// Padding (inner spacing) inside this control. Affects child control positions.
		/// </summary>
		[YamlMember]
		public FishUIMargin Padding;

		/// <summary>
		/// Anchor settings for responsive resizing. Determines how this control repositions/resizes
		/// when the parent resizes. Default is TopLeft (fixed position relative to parent origin).
		/// </summary>
		[YamlMember]
		public FishUIAnchor Anchor { get; set; } = FishUIAnchor.TopLeft;

		/// <summary>
		/// Stores the initial distance from the right edge of parent. Used for right anchor calculations.
		/// Set automatically when the control is added to a parent.
		/// </summary>
		[YamlIgnore]
		internal float AnchorRight { get; set; }

		/// <summary>
		/// Stores the initial distance from the bottom edge of parent. Used for bottom anchor calculations.
		/// Set automatically when the control is added to a parent.
		/// </summary>
		[YamlIgnore]
		internal float AnchorBottom { get; set; }

		/// <summary>
		/// Stores the initial parent size when anchor offsets were calculated.
		/// </summary>
		[YamlIgnore]
		internal Vector2 AnchorParentSize { get; set; }

		/// <summary>
		/// Unique identifier for this control. Used for finding controls and layout serialization.
		/// </summary>
		[YamlMember]
		public string ID;

		/// <summary>
		/// Z-depth for rendering order. Higher values are rendered on top.
		/// </summary>
		public virtual int ZDepth { get; set; }

		/// <summary>
		/// If true, this control is always rendered on top of non-AlwaysOnTop controls.
		/// </summary>
		public virtual bool AlwaysOnTop { get; set; } = false;

		/// <summary>
		/// If true, this control is disabled and cannot receive input.
		/// </summary>
		public virtual bool Disabled { get; set; } = false;

		/// <summary>
		/// If true, this control is visible and will be rendered.
		/// </summary>
		public virtual bool Visible { get; set; } = true;

		/// <summary>
		/// Tint color applied when rendering this control.
		/// </summary>
		public virtual FishColor Color { get; set; } = new FishColor(255, 255, 255, 255);

		/// <summary>
		/// Opacity of the control from 0.0 (fully transparent) to 1.0 (fully opaque).
		/// This is multiplied with the Color's alpha channel when rendering.
		/// </summary>
		[YamlMember]
		public virtual float Opacity { get; set; } = 1.0f;

		/// <summary>
		/// Per-control color overrides. Allows overriding specific theme colors for this control instance.
		/// Common keys: "Text", "Background", "Border", "Hover", "Pressed", "Disabled"
		/// Use GetColorOverride() to retrieve colors with fallback to theme defaults.
		/// </summary>
		[YamlMember]
		public Dictionary<string, FishColor> ColorOverrides { get; set; } = null;

		/// <summary>
		/// Gets a color override if set, otherwise returns the fallback color.
		/// </summary>
		/// <param name="key">Color key (e.g., "Text", "Background", "Border")</param>
		/// <param name="fallback">Fallback color if override is not set</param>
		/// <returns>The override color if set, otherwise the fallback</returns>
		public FishColor GetColorOverride(string key, FishColor fallback)
		{
			if (ColorOverrides != null && ColorOverrides.TryGetValue(key, out var color))
				return color;
			return fallback;
		}

		/// <summary>
		/// Sets a color override for this control.
		/// </summary>
		/// <param name="key">Color key (e.g., "Text", "Background", "Border")</param>
		/// <param name="color">The color value to set</param>
		public void SetColorOverride(string key, FishColor color)
		{
			ColorOverrides ??= new Dictionary<string, FishColor>();
			ColorOverrides[key] = color;
		}

		/// <summary>
		/// Clears a specific color override.
		/// </summary>
		/// <param name="key">Color key to clear</param>
		public void ClearColorOverride(string key)
		{
			ColorOverrides?.Remove(key);
		}

		/// <summary>
		/// Clears all color overrides for this control.
		/// </summary>
		public void ClearAllColorOverrides()
		{
			ColorOverrides?.Clear();
		}

		/// <summary>
		/// Gets the effective color for rendering, combining Color with Opacity.
		/// </summary>
		[YamlIgnore]
		public FishColor EffectiveColor
		{
			get
			{
				byte effectiveAlpha = (byte)(Color.A * Math.Clamp(Opacity, 0f, 1f));
				return new FishColor(Color.R, Color.G, Color.B, effectiveAlpha);
			}
		}

		/// <summary>
		/// If true, this control can be dragged with the mouse. Fires OnDragged event.
		/// </summary>
		public virtual bool Draggable { get; set; } = false;

		/// <summary>
		/// If true, this control can receive keyboard focus via Tab key navigation.
		/// </summary>
		public virtual bool Focusable { get; set; } = false;

		/// <summary>
		/// The tab order index for keyboard navigation. Lower values are focused first.
		/// Controls with the same TabIndex are focused in the order they were added.
		/// </summary>
		public virtual int TabIndex { get; set; } = 0;

		/// <summary>
		/// Returns true if this control currently has input focus.
		/// </summary>
		[YamlIgnore]
		public bool HasFocus => FishUI?.InputActiveControl == this;

		/// <summary>
		/// If true, children are drawn without scissor clipping to parent bounds.
		/// Useful for controls like CheckBox/RadioButton where labels extend beyond the control.
		/// </summary>
		public virtual bool DisableChildScissor { get; set; } = false;

		/// <summary>
		/// Tooltip text to display when hovering over this control.
		/// Set to null or empty to disable tooltip.
		/// </summary>
		[YamlMember]
		public virtual string TooltipText { get; set; }

		/// <summary>
		/// Event fired when this control is dragged with the mouse.
		/// </summary>
		public event OnControlDraggedFunc OnDragged;

		/// <summary>
		/// Gets the parent control.
		/// </summary>
		public Control GetParent()
		{
			return Parent;
		}

		/// <summary>
		/// Brings this control to the front of all sibling controls.
		/// For root-level controls, this brings it in front of all other root controls.
		/// </summary>
		public virtual void BringToFront()
		{
			if (FishUI != null)
			{
				ZDepth = FishUI.GetHighestZDepth() + 1;
			}
			else if (Parent != null)
			{
				// For child controls, bring to front among siblings
				int maxDepth = 0;
				foreach (var sibling in Parent.Children)
				{
					if (sibling != this && sibling.ZDepth > maxDepth)
						maxDepth = sibling.ZDepth;
				}
				ZDepth = maxDepth + 1;
			}
		}

		/// <summary>
		/// Sends this control to the back of all sibling controls.
		/// </summary>
		public virtual void SendToBack()
		{
			if (FishUI != null)
			{
				ZDepth = FishUI.GetLowestZDepth() - 1;
			}
			else if (Parent != null)
			{
				// For child controls, send to back among siblings
				int minDepth = int.MaxValue;
				foreach (var sibling in Parent.Children)
				{
					if (sibling != this && sibling.ZDepth < minDepth)
						minDepth = sibling.ZDepth;
				}
				ZDepth = minDepth == int.MaxValue ? 0 : minDepth - 1;
			}
		}

		/// <summary>
		/// Gets an additional position offset to apply to child controls.
		/// Override in container controls like ScrollablePane to implement scrolling.
		/// </summary>
		/// <param name="child">The child control requesting the offset.</param>
		/// <returns>The offset to apply to the child's position.</returns>
		public virtual Vector2 GetChildPositionOffset(Control child)
		{
			return Vector2.Zero;
		}

		/// <summary>
		/// Gets the absolute position of this control in screen coordinates.
		/// Accounts for parent Padding, this control's Margin, and Anchor settings.
		/// </summary>
		/// <returns>The absolute position in pixels.</returns>
		public Vector2 GetAbsolutePosition()
		{
			// Calculate parent padding offset
			Vector2 parentPaddingOffset = Vector2.Zero;
			Vector2 parentPos = Vector2.Zero;
			Vector2 parentSize = Vector2.Zero;

			if (Parent != null)
			{
				parentPaddingOffset = new Vector2(Parent.Padding.Left, Parent.Padding.Top);
				parentPos = Parent.GetAbsolutePosition();
				parentSize = Parent.GetAbsoluteSize();
			}
			else if (FishUI != null)
			{
				parentSize = new Vector2(FishUI.Width, FishUI.Height);
			}

			// This control's margin offset
			Vector2 marginOffset = new Vector2(Margin.Left, Margin.Top);

			// Calculate base position
			Vector2 basePos;
			if (Position.Mode == PositionMode.Absolute)
			{
				basePos = new Vector2(Position.X, Position.Y) + marginOffset;
			}
			else if (Position.Mode == PositionMode.Relative)
			{
				basePos = parentPos + parentPaddingOffset + new Vector2(Position.X, Position.Y) + marginOffset;
			}
			else if (Position.Mode == PositionMode.Docked)
			{
				Vector2 DockedPos = parentPos + parentPaddingOffset + new Vector2(Position.X, Position.Y) + marginOffset;

				if (Position.Dock.HasFlag(DockMode.Left))
				{
					DockedPos.X = parentPos.X + parentPaddingOffset.X + Position.Left + Margin.Left;
				}

				if (Position.Dock.HasFlag(DockMode.Top))
				{
					DockedPos.Y = parentPos.Y + parentPaddingOffset.Y + Position.Top + Margin.Top;
				}

				basePos = DockedPos;
			}
			else
			{
				// Fallback for unknown position modes - treat as absolute position
				basePos = new Vector2(Position.X, Position.Y) + marginOffset;
			}

			// Apply anchor adjustments if parent exists and anchored to right or bottom
			if (Parent != null && Anchor != FishUIAnchor.None && Anchor != FishUIAnchor.TopLeft)
			{
				Vector2 sizeDelta = parentSize - AnchorParentSize;

				// Right anchor: adjust X position based on parent width change
				if (Anchor.HasFlag(FishUIAnchor.Right) && !Anchor.HasFlag(FishUIAnchor.Left))
				{
					// Only right anchor - move with right edge
					basePos.X += sizeDelta.X;
				}

				// Bottom anchor: adjust Y position based on parent height change
				if (Anchor.HasFlag(FishUIAnchor.Bottom) && !Anchor.HasFlag(FishUIAnchor.Top))
				{
					// Only bottom anchor - move with bottom edge
					basePos.Y += sizeDelta.Y;
				}
			}

			// Apply parent's child position offset (used for scrolling containers)
			if (Parent != null)
			{
				basePos += Parent.GetChildPositionOffset(this);
			}

			return basePos;
		}

		/// <summary>
		/// Converts a global point to local coordinates relative to this control.
		/// </summary>
		/// <param name="GlobalPt">Point in screen coordinates.</param>
		/// <returns>Point in local coordinates relative to this control's position.</returns>
		public Vector2 GetLocalRelative(Vector2 GlobalPt)
		{
			return GlobalPt - GetAbsolutePosition();
		}

		/// <summary>
		/// Gets the absolute size of this control, accounting for docked positioning, margins, and anchor stretching.
		/// </summary>
		/// <returns>The actual size in pixels.</returns>
		public Vector2 GetAbsoluteSize()
		{
			Vector2 resultSize = Size;

			// Handle docked positioning
			if (Position.Mode == PositionMode.Docked)
			{
				Vector2 ParentPos;
				Vector2 ParentSize;
				FishUIMargin parentPadding = new FishUIMargin();

				if (Parent != null)
				{
					ParentPos = Parent.GetAbsolutePosition();
					ParentSize = Parent.GetAbsoluteSize();
					parentPadding = Parent.Padding;
				}
				else if (FishUI != null)
				{
					// No parent - dock to screen bounds using FishUI dimensions
					ParentPos = Vector2.Zero;
					ParentSize = new Vector2(FishUI.Width, FishUI.Height);
				}
				else
				{
					// No parent and no FishUI - return default size
					return Size;
				}

				Vector2 MyPos = GetAbsolutePosition();

				float SubX = MyPos.X - ParentPos.X;
				float SubY = MyPos.Y - ParentPos.Y;

				if (Position.Dock.HasFlag(DockMode.Right))
				{
					float FullChildWidth = ParentSize.X - SubX;
					// Account for parent right padding and this control's right margin
					resultSize.X = FullChildWidth - Position.Right - parentPadding.Right - Margin.Right;
				}

				if (Position.Dock.HasFlag(DockMode.Bottom))
				{
					float FullChildHeight = ParentSize.Y - SubY;
					// Account for parent bottom padding and this control's bottom margin
					resultSize.Y = FullChildHeight - Position.Bottom - parentPadding.Bottom - Margin.Bottom;
				}
			}

			// Handle anchor stretching (when anchored to both left+right or top+bottom)
			if (Parent != null && Anchor != FishUIAnchor.None && Anchor != FishUIAnchor.TopLeft)
			{
				Vector2 parentSize = Parent.GetAbsoluteSize();
				Vector2 sizeDelta = parentSize - AnchorParentSize;

				// Horizontal stretching: anchored to both left and right
				if (Anchor.HasFlag(FishUIAnchor.Left) && Anchor.HasFlag(FishUIAnchor.Right))
				{
					resultSize.X = Size.X + sizeDelta.X;
				}

				// Vertical stretching: anchored to both top and bottom
				if (Anchor.HasFlag(FishUIAnchor.Top) && Anchor.HasFlag(FishUIAnchor.Bottom))
				{
					resultSize.Y = Size.Y + sizeDelta.Y;
				}
			}

			return resultSize;
		}

		/// <summary>
		/// Checks if a point in screen coordinates is inside this control.
		/// </summary>
		/// <param name="GlobalPt">Point in screen coordinates.</param>
		/// <returns>True if the point is inside the control bounds.</returns>
		public virtual bool IsPointInside(Vector2 GlobalPt)
		{
			Vector2 AbsPos = GetAbsolutePosition();
			Vector2 AbsSize = GetAbsoluteSize();
			return Utils.IsInside(AbsPos, AbsSize, GlobalPt);
		}

		/// <summary>
		/// True if the mouse is currently over this control.
		/// </summary>
		[YamlIgnore]
		public bool IsMouseInside;

		/// <summary>
		/// True if the mouse button is currently pressed on this control.
		/// </summary>
		[YamlIgnore]
		public bool IsMousePressed;

		/// <summary>
		/// Removes this control from its parent.
		/// </summary>
		public void Unparent()
		{
			if (Parent != null)
			{
				Parent.RemoveChild(this);
			}
		}

		/// <summary>
		/// Adds a control as a child of this control.
		/// </summary>
		/// <param name="Child">The control to add as a child.</param>
		public void AddChild(Control Child)
		{
			Child.Parent = this;

			// Calculate anchor offsets based on current parent size
			UpdateChildAnchorOffsets(Child);

			// If Children contains Child, skip. It means this function was used to re-parent an existing child on deserialization
			if (Children.Contains(Child))
				return;

			// Assign ZDepth based on insertion order (higher = added later = on top)
			// Use the count of existing children as the ZDepth for proper ordering
			Child.ZDepth = Children.Count;

			Children.Add(Child);
		}

		/// <summary>
		/// Updates the anchor offset values for a child control based on current parent size.
		/// </summary>
		private void UpdateChildAnchorOffsets(Control Child)
		{
			Vector2 parentSize = GetAbsoluteSize();
			Child.AnchorParentSize = parentSize;

			// Calculate distances from right and bottom edges
			float childRight = Child.Position.X + Child.Size.X;
			float childBottom = Child.Position.Y + Child.Size.Y;

			Child.AnchorRight = parentSize.X - childRight;
			Child.AnchorBottom = parentSize.Y - childBottom;
		}

		/// <summary>
		/// Recalculates anchor offsets for all children. Call after parent is resized.
		/// </summary>
		public void RecalculateChildAnchors()
		{
			foreach (var child in Children)
			{
				UpdateChildAnchorOffsets(child);
			}
		}

		/// <summary>
		/// Determines if a child control should receive input at the specified point.
		/// Override in container controls like ScrollablePane to restrict input to visible area.
		/// </summary>
		/// <param name="child">The child control to check.</param>
		/// <param name="globalPoint">The point in screen coordinates.</param>
		/// <returns>True if the child should receive input at this point.</returns>
		public virtual bool ShouldChildReceiveInput(Control child, Vector2 globalPoint)
		{
			return true; // By default, all children can receive input
		}

		/// <summary>
		/// Finds the first child control of the specified type.
		/// </summary>
		/// <typeparam name="T">The type of control to find.</typeparam>
		/// <returns>The first matching child control, or null if not found.</returns>
		public T FindChildByType<T>() where T : Control
		{
			foreach (Control Ch in GetAllChildren())
			{
				if (Ch is T Ret)
					return Ret;
			}

			return null;
		}

		/// <summary>
		/// Gets all child controls.
		/// </summary>
		/// <param name="Order">If true, returns children ordered by ZDepth with AlwaysOnTop controls last.</param>
		/// <returns>Array of child controls.</returns>
		public Control[] GetAllChildren(bool Order = true)
		{
			if (Order)
			{
				// Sort: normal controls by ZDepth, then AlwaysOnTop controls by ZDepth
				// This matches GetOrderedControls behavior so AlwaysOnTop children are picked first when reversed
				var normal = Children.Where(c => !c.AlwaysOnTop).OrderBy(c => c.ZDepth);
				var alwaysOnTop = Children.Where(c => c.AlwaysOnTop).OrderBy(c => c.ZDepth);
				return normal.Concat(alwaysOnTop).ToArray();
			}
			else
				return Children.ToArray();
		}

		/// <summary>
		/// Removes a child control from this control.
		/// </summary>
		/// <param name="Child">The child control to remove.</param>
		public void RemoveChild(Control Child)
		{
			Child.Parent = null;
			Children.Remove(Child);
		}

		/// <summary>
		/// Removes all child controls from this control.
		/// </summary>
		public void RemoveAllChildren()
		{
			Control[] Ch = GetAllChildren(false);

			for (int i = 0; i < Ch.Length; i++)
				RemoveChild(Ch[i]);
		}

		/// <summary>
		/// Called once at first draw for resource initialization.
		/// </summary>
		/// <param name="UI">The FishUI instance.</param>
		public virtual void Init(FishUI UI)
		{
		}

		/// <summary>
		/// Called after this control and its children have been deserialized from a layout file.
		/// Override this to reinitialize internal state that depends on child controls.
		/// </summary>
		public virtual void OnDeserialized()
		{
			// Call OnDeserialized on all children
			foreach (var child in Children)
			{
				child.OnDeserialized();
			}
		}

		/// <summary>
		/// Draws all child controls.
		/// </summary>
		/// <param name="UI">The FishUI instance.</param>
		/// <param name="Dt">Delta time since last frame.</param>
		/// <param name="Time">Total elapsed time.</param>
		/// <param name="UseScissors">If true, clips children to this control's bounds.</param>
		public virtual void DrawChildren(FishUI UI, float Dt, float Time, bool UseScissors = true)
		{
			// Respect DisableChildScissor property - controls like CheckBox/RadioButton need labels to extend beyond bounds
			bool applyScissor = UseScissors && !DisableChildScissor;

			if (applyScissor)
				UI.Graphics.PushScissor(GetAbsolutePosition(), GetAbsoluteSize());

			Control[] Ch = GetAllChildren().Reverse().ToArray();
			foreach (var Child in Ch)
			{
				if (!Child.Visible)
					continue;

				Child.DrawControlAndChildren(UI, Dt, Time);
			}

			if (applyScissor)
				UI.Graphics.PopScissor();
		}

		/// <summary>
		/// Override this method to draw the control's visual appearance.
		/// </summary>
		/// <param name="UI">The FishUI instance.</param>
		/// <param name="Dt">Delta time since last frame.</param>
		/// <param name="Time">Total elapsed time.</param>
		public virtual void DrawControl(FishUI UI, float Dt, float Time)
		{
			UI.Graphics.DrawRectangle(GetAbsolutePosition(), GetAbsoluteSize(), Color);

			//UI.Graphics.DrawImage(UI.Skin, Position, Size, 0, 1, new FishColor(255, 255, 255, 255));

			if (IsMouseInside)
			{
				UI.Graphics.DrawRectangleOutline(GetAbsolutePosition(), GetAbsoluteSize(), new FishColor(0, 255, 255));
			}
			else
			{
				UI.Graphics.DrawRectangleOutline(GetAbsolutePosition(), GetAbsoluteSize(), new FishColor(100, 100, 100));
			}

			//DrawChildren(UI, Dt, Time);
		}

		[YamlIgnore]
		bool DrawHasInit = false;

		/// <summary>
		/// Draws this control and all its children.
		/// </summary>
		/// <param name="UI">The FishUI instance.</param>
		/// <param name="Dt">Delta time since last frame.</param>
		/// <param name="Time">Total elapsed time.</param>
		public void DrawControlAndChildren(FishUI UI, float Dt, float Time)
		{
			if (!DrawHasInit)
			{
				DrawHasInit = true;
				Init(UI);
			}

			DrawControl(UI, Dt, Time);

			// Draw debug outline if enabled
			if (FishUIDebug.DrawControlOutlines)
				UI.Graphics.DrawRectangleOutline(GetAbsolutePosition(), GetAbsoluteSize(), FishUIDebug.OutlineColor);

			// Draw focus indicator if this control has focus
			if (FishUIDebug.DrawFocusIndicators && HasFocus && Focusable)
				UI.Graphics.DrawRectangleOutline(GetAbsolutePosition() - new Vector2(2, 2), GetAbsoluteSize() + new Vector2(4, 4), FishUIDebug.FocusIndicatorColor);

			DrawChildren(UI, Dt, Time);
		}

		/// <summary>
		/// Called when the control is being dragged with the mouse.
		/// </summary>
		public virtual void HandleDrag(FishUI UI, Vector2 StartPos, Vector2 EndPos, FishInputState InState)
		{
			if (Draggable)
			{
				OnDragged?.Invoke(this, InState.MouseDelta);
				Position += InState.MouseDelta;
			}

			//if (DebugPrint)
			//	Console.WriteLine($"{GetType().Name}({ID ?? "null"}) - Drag Control");
		}

		/// <summary>
		/// Called when the mouse enters this control's bounds.
		/// </summary>
		public virtual void HandleMouseEnter(FishUI UI, FishInputState InState)
		{
			FishUIDebug.LogControlEvent(GetType().Name, ID, "Mouse Enter");
		}

		/// <summary>
		/// Called when the mouse moves within this control's bounds.
		/// </summary>
		public virtual void HandleMouseMove(FishUI UI, FishInputState InState, Vector2 Pos)
		{
		}

		/// <summary>
		/// Called when the mouse leaves this control's bounds.
		/// </summary>
		public virtual void HandleMouseLeave(FishUI UI, FishInputState InState)
		{
			FishUIDebug.LogControlEvent(GetType().Name, ID, "Mouse Leave");
		}


		/// <summary>
		/// Called when a mouse button is pressed on this control.
		/// </summary>
		public virtual void HandleMousePress(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			FishUIDebug.LogControlEvent(GetType().Name, ID, "Mouse Press", Btn.ToString());
		}

		/// <summary>
		/// Called when a mouse button is released on this control.
		/// </summary>
		public virtual void HandleMouseRelease(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			FishUIDebug.LogControlEvent(GetType().Name, ID, "Mouse Release", Btn.ToString());
		}

		/// <summary>
		/// Called when the mouse is clicked on this control.
		/// </summary>
		public virtual void HandleMouseClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			FishUIDebug.LogControlEvent(GetType().Name, ID, "Mouse Click", Btn.ToString());

			UI.Events.Broadcast(UI, this, "mouse_click", null);
		}

		/// <summary>
		/// Called when the mouse is double-clicked on this control.
		/// </summary>
		public virtual void HandleMouseDoubleClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			FishUIDebug.LogControlEvent(GetType().Name, ID, "Mouse Double Click", Btn.ToString());

			UI.Events.Broadcast(UI, this, "mouse_double_click", null);
		}

		/// <summary>
		/// Called when this control receives input focus.
		/// </summary>
		public virtual void HandleFocus()
		{
			FishUIDebug.LogControlEvent(GetType().Name, ID, "Focus");
		}

		/// <summary>
		/// Called when this control loses input focus.
		/// </summary>
		public virtual void HandleBlur()
		{
			FishUIDebug.LogControlEvent(GetType().Name, ID, "Blur");
		}

		/// <summary>
		/// Called when text is typed while this control has focus.
		/// </summary>
		public virtual void HandleTextInput(FishUI UI, FishInputState InState, char Chr)
		{

		}

		/// <summary>
		/// Called when a key is pressed while this control has focus.
		/// </summary>
		public virtual void HandleKeyPress(FishUI UI, FishInputState InState, FishKey Key)
		{
		}

		/// <summary>
		/// Called when a key is held down while this control has focus.
		/// </summary>
		public virtual void HandleKeyDown(FishUI UI, FishInputState InState, int KeyCode)
		{
		}

		/// <summary>
		/// Called when a key is released while this control has focus.
		/// </summary>
		public virtual void HandleKeyRelease(FishUI UI, FishInputState InState, FishKey Key)
		{
		}

		/// <summary>
		/// Called when the mouse wheel is scrolled over this control.
		/// By default, propagates to parent control.
		/// </summary>
		public virtual void HandleMouseWheel(FishUI UI, FishInputState InState, float WheelDelta)
		{
			// By default, propagate mouse wheel events to parent (bubble up)
			if (Parent != null)
				Parent.HandleMouseWheel(UI, InState, WheelDelta);
		}
	}
}
