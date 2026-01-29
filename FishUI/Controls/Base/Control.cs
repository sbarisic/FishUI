using System;
using System.Collections.Generic;
using System.Numerics;
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
	/// Specifies which dimensions of a control should auto-size based on content.
	/// </summary>
	public enum AutoSizeMode
	{
		/// <summary>
		/// No auto-sizing. Size is manually set.
		/// </summary>
		None,

		/// <summary>
		/// Auto-size width based on content, height is manual.
		/// </summary>
		Width,

		/// <summary>
		/// Auto-size height based on content, width is manual.
		/// </summary>
		Height,

		/// <summary>
		/// Auto-size both width and height based on content.
		/// </summary>
		Both
	}

	/// <summary>
	/// Base class for all FishUI controls. Provides common functionality for positioning,
	/// sizing, rendering, input handling, and parent-child relationships.
	/// </summary>
	/// <remarks>
	/// This partial class is split across multiple files:
	/// - Control.cs (this file): Core properties, fields, and events
	/// - Control.Animation.cs: Animation methods (FadeIn, SlideIn, etc.)
	/// - Control.Anchoring.cs: Anchor system for responsive layouts
	/// - Control.AutoSize.cs: Auto-sizing based on content
	/// - Control.Children.cs: Child control management
	/// - Control.Drawing.cs: Rendering and drawing methods
	/// - Control.Input.cs: Input event handlers
	/// - Control.Position.cs: Position and size calculations
	/// </remarks>
	public abstract partial class Control
	{
		#region Core Fields

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
		/// Size of the control in logical pixels (unscaled).
		/// </summary>
		[YamlMember]
		public Vector2 Size;

		/// <summary>
		/// Gets the scaled size of this control based on the global UIScale factor.
		/// </summary>
		[YamlIgnore]
		public Vector2 ScaledSize => FishUI?.Settings?.Scale(Size) ?? Size;

		/// <summary>
		/// Scales a value by the current UIScale factor.
		/// </summary>
		/// <param name="value">The value to scale.</param>
		/// <returns>The scaled value.</returns>
		protected float Scale(float value) => FishUI?.Settings?.Scale(value) ?? value;

		/// <summary>
		/// Scales a Vector2 by the current UIScale factor.
		/// </summary>
		/// <param name="value">The vector to scale.</param>
		/// <returns>The scaled vector.</returns>
		protected Vector2 Scale(Vector2 value) => FishUI?.Settings?.Scale(value) ?? value;

		/// <summary>
		/// Gets the current UIScale factor.
		/// </summary>
		[YamlIgnore]
		protected float UIScale => FishUI?.Settings?.UIScale ?? 1.0f;

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
		/// Unique identifier for this control. Used for finding controls and layout serialization.
		/// </summary>
		[YamlMember]
		public string ID;

		/// <summary>
		/// Variable name used when exporting to C# designer code.
		/// If not set, falls back to ID, or auto-generates a name based on control type.
		/// Should be a valid C# identifier (e.g., "btnSubmit", "lblTitle").
		/// </summary>
		[YamlMember]
		public string DesignerName { get; set; }

		#endregion

		#region Visual Properties

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

		#endregion

		#region Color Overrides

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

		#endregion

		#region Input Properties

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
		/// True if the mouse is currently over this control.
		/// </summary>
		[YamlIgnore]
		public bool IsMouseInside;

		/// <summary>
		/// True if the mouse button is currently pressed on this control.
		/// </summary>
		[YamlIgnore]
		public bool IsMousePressed;

		#endregion

		#region Serializable Event Handlers

		/// <summary>
		/// Name of the registered event handler to invoke when this control is clicked.
		/// The handler must be registered with FishUI.EventHandlers before deserialization.
		/// </summary>
		[YamlMember]
		public string OnClickHandler { get; set; }

		/// <summary>
		/// Name of the registered event handler to invoke when this control's value changes.
		/// Applies to Slider, NumericUpDown, ProgressBar, etc.
		/// </summary>
		[YamlMember]
		public string OnValueChangedHandler { get; set; }

		/// <summary>
		/// Name of the registered event handler to invoke when this control's selection changes.
		/// Applies to ListBox, DropDown, TreeView, etc.
		/// </summary>
		[YamlMember]
		public string OnSelectionChangedHandler { get; set; }

		/// <summary>
		/// Name of the registered event handler to invoke when this control's text changes.
		/// Applies to Textbox, MultiLineEditbox, etc.
		/// </summary>
		[YamlMember]
		public string OnTextChangedHandler { get; set; }

		/// <summary>
		/// Name of the registered event handler to invoke when this control's checked state changes.
		/// Applies to CheckBox, RadioButton, ToggleSwitch, etc.
		/// </summary>
		[YamlMember]
		public string OnCheckedChangedHandler { get; set; }

		/// <summary>
		/// Invokes a named event handler from the registry.
		/// </summary>
		protected void InvokeHandler(string handlerName, EventHandlerArgs args)
		{
			if (string.IsNullOrEmpty(handlerName) || FishUI == null)
				return;

			FishUI.EventHandlers.Invoke(handlerName, this, args);
		}

		#endregion

		#region Events

		/// <summary>
		/// Event fired when this control is dragged with the mouse.
		/// </summary>
		public event OnControlDraggedFunc OnDragged;

		/// <summary>
		/// Event fired when this control is clicked.
		/// </summary>
		public event Action<Control, FishUIClickEventArgs> Clicked;

		/// <summary>
		/// Event fired when this control is double-clicked.
		/// </summary>
		public event Action<Control, FishUIClickEventArgs> DoubleClicked;

		/// <summary>
		/// Event fired when the mouse enters this control's bounds.
		/// </summary>
		public event Action<Control, FishUIMouseEventArgs> MouseEnter;

		/// <summary>
		/// Event fired when the mouse leaves this control's bounds.
		/// </summary>
		public event Action<Control, FishUIMouseEventArgs> MouseLeave;

		#endregion

		#region Utility Methods

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

		#endregion
	}
}
