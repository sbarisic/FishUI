using System;
using System.Linq;
using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	public abstract partial class Control
	{
		[YamlIgnore]
		bool DrawHasInit = false;

		/// <summary>
		/// Called once at first draw for resource initialization.
		/// </summary>
		/// <param name="UI">The FishUI instance.</param>
		public virtual void Init(FishUI UI)
		{
		}

		/// <summary>
		/// Called after this control and its children have been deserialized from a layout file.
		/// Override this to reinitialize internal state that depends on child controls or load resources.
		/// </summary>
		/// <param name="UI">The FishUI instance for loading resources.</param>
		public virtual void OnDeserialized(FishUI UI)
		{
			// Call OnDeserialized on all children
			foreach (var child in Children)
			{
				child.OnDeserialized(UI);
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
		/// Draws the control editor interface for the specified user interface element
		/// </summary>
		/// <remarks>Override this method in a derived class to implement custom drawing logic for specific control
		/// editors. This method is intended for use within the UI rendering loop and may be called frequently.</remarks>
		/// <param name="UI">The FishUI instance</param>
		/// <param name="Dt">The time, in seconds, since the last frame</param>
		/// <param name="Time">The total elapsed time, in seconds, since the start of the application</param>
		public virtual void DrawControlEditor(FishUI UI, float Dt, float Time)
		{
			throw new NotImplementedException();
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

			if (IsMouseInside)
			{
				UI.Graphics.DrawRectangleOutline(GetAbsolutePosition(), GetAbsoluteSize(), new FishColor(0, 255, 255));
			}
			else
			{
				UI.Graphics.DrawRectangleOutline(GetAbsolutePosition(), GetAbsoluteSize(), new FishColor(100, 100, 100));
			}
		}

		/// <summary>
		/// Draws this control in editor mode (inert, no input processing).
		/// Override in derived classes to provide custom editor visualization.
		/// Default implementation calls DrawControl.
		/// Note: Position is already adjusted by the caller (EditorCanvas) before this is called.
		/// </summary>
		/// <param name="UI">The FishUI instance.</param>
		/// <param name="Dt">Delta time since last frame.</param>
		/// <param name="Time">Total elapsed time.</param>
		/// <param name="canvasOffset">Canvas offset (for information only, position already adjusted).</param>
		public virtual void DrawControlEditor(FishUI UI, float Dt, float Time, Vector2 canvasOffset)
		{
			// Default: just draw the control normally
			// Position is already adjusted by the caller (EditorCanvas)
			DrawControl(UI, Dt, Time);

			// Draw anchor visualization lines
			DrawAnchorVisualization(UI);
		}

		/// <summary>
		/// Draws children in editor mode (inert, no input processing).
		/// Override in derived classes to provide custom child rendering.
		/// </summary>
		/// <param name="UI">The FishUI instance.</param>
		/// <param name="Dt">Delta time since last frame.</param>
		/// <param name="Time">Total elapsed time.</param>
		public virtual void DrawChildrenEditor(FishUI UI, float Dt, float Time)
		{
			foreach (var child in Children.OrderBy(c => c.ZDepth))
			{
				child.DrawControlEditor(UI, Dt, Time, Vector2.Zero);
				child.DrawChildrenEditor(UI, Dt, Time);
			}
		}

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
	}
}
