using FishUI;
using FishUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace FishUIEditor.Controls
{
	/// <summary>
	/// A design surface control for the layout editor.
	/// Renders child controls in "inert" mode - visual only, no input processing.
	/// Provides selection, dragging, and resize functionality for editor controls.
	/// </summary>
	public class EditorCanvas : Control
	{
		/// <summary>
		/// Background color of the canvas.
		/// </summary>
		public FishColor BackgroundColor { get; set; } = new FishColor(45, 45, 48, 255);

		/// <summary>
		/// Color of the grid lines.
		/// </summary>
		public FishColor GridColor { get; set; } = new FishColor(60, 60, 63, 255);

		/// <summary>
		/// Size of the grid squares in pixels.
		/// </summary>
		public int GridSize { get; set; } = 16;

		/// <summary>
		/// Whether to show the grid.
		/// </summary>
		public bool ShowGrid { get; set; } = true;

		/// <summary>
		/// The currently selected control in the editor.
		/// </summary>
		public Control SelectedControl { get; private set; }

		/// <summary>
		/// List of controls being edited (these are rendered inert).
		/// </summary>
		private List<Control> _editedControls = new List<Control>();

		/// <summary>
		/// Selection highlight color.
		/// </summary>
		public FishColor SelectionColor { get; set; } = new FishColor(0, 120, 215, 255);

		/// <summary>
		/// Resize handle size in pixels.
		/// </summary>
		public float HandleSize { get; set; } = 8f;

		/// <summary>
		/// Event fired when a control is selected.
		/// </summary>
		public event Action<Control> OnControlSelected;

		/// <summary>
		/// Event fired when a control's position or size changes.
		/// </summary>
		public event Action<Control> OnControlModified;

		/// <summary>
		/// Event fired when a control is reparented (moved to different container or root).
		/// </summary>
		public event Action<Control> OnControlReparented;

		private bool _isDragging = false;
		private bool _isResizing = false;
		private Vector2 _dragOffset;
		private ResizeHandle _activeHandle = ResizeHandle.None;
		private Control _dragStartParent = null; // Track original parent for reparenting

		private enum ResizeHandle
		{
			None,
			TopLeft, Top, TopRight,
			Left, Right,
			BottomLeft, Bottom, BottomRight
		}

		public EditorCanvas()
		{
			Size = new Vector2(800, 600);
		}

		/// <summary>
		/// Adds a control to the design surface for editing.
		/// </summary>
		public void AddEditedControl(Control control)
		{
			if (!_editedControls.Contains(control))
			{
				_editedControls.Add(control);
			}
		}

		/// <summary>
		/// Removes a control from the design surface.
		/// </summary>
		public void RemoveEditedControl(Control control)
		{
			_editedControls.Remove(control);
			if (SelectedControl == control)
				SelectedControl = null;
		}

		/// <summary>
		/// Clears all controls from the design surface.
		/// </summary>
		public void ClearEditedControls()
		{
			_editedControls.Clear();
			SelectedControl = null;
		}

		/// <summary>
		/// Gets all controls on the design surface.
		/// </summary>
		public IReadOnlyList<Control> GetEditedControls() => _editedControls;

		/// <summary>
		/// Selects a control.
		/// </summary>
		public void SelectControl(Control control)
		{
			SelectedControl = control;
			OnControlSelected?.Invoke(control);
		}

		public override void DrawControl(FishUI.FishUI UI, float Dt, float Time)
		{
			base.DrawControl(UI, Dt, Time);

			Vector2 pos = GetAbsolutePosition();
			Vector2 size = ScaledSize;

			// Draw background
			UI.Graphics.DrawRectangle(pos, size, BackgroundColor);

			// Draw grid
			if (ShowGrid)
			{
				DrawGrid(UI, pos, size);
			}

			// Draw border
			UI.Graphics.DrawRectangleOutline(pos, size, new FishColor(70, 70, 70, 255));

			// Begin scissor for canvas bounds
			UI.Graphics.BeginScissor(pos, size);

			// Draw all edited controls (inert rendering) - sorted by ZDepth (lowest first, so higher ZDepth draws on top)
			foreach (var control in _editedControls.OrderBy(c => c.ZDepth))
			{
				DrawInertControl(UI, control, pos, Dt, Time);
			}

			// Draw drop target highlight during drag operations
			if (HoveredDropTarget != null)
			{
				DrawDropTargetHighlight(UI, HoveredDropTarget, pos);
			}

			// Draw selection highlight
			if (SelectedControl != null)
			{
				DrawSelectionHighlight(UI, SelectedControl, pos);
			}

			UI.Graphics.EndScissor();
		}

		private void DrawDropTargetHighlight(FishUI.FishUI UI, Control target, Vector2 canvasPos)
		{
			Vector2 targetAbsPos = GetControlAbsolutePositionInCanvas(target, canvasPos);
			Vector2 targetSize = target.GetAnchorAdjustedSize();

			// Draw a thick outline around the drop target
			const float outlineThickness = 3f;

			// Draw multiple rectangles to create a thick outline effect
			for (int i = 0; i < (int)outlineThickness; i++)
			{
				Vector2 offset = new Vector2(i, i);
				UI.Graphics.DrawRectangleOutline(
					targetAbsPos - offset,
					targetSize + offset * 2,
					DropTargetHighlightColor);
			}

			// Draw label showing the container name
			string label = $"Drop into: {target.GetType().Name}";
			UI.Graphics.DrawTextColor(UI.Settings.FontDefault, label, targetAbsPos + new Vector2(4, -18), DropTargetHighlightColor);
		}

		private void DrawGrid(FishUI.FishUI UI, Vector2 pos, Vector2 size)
		{
			int gridSize = GridSize;

			// Vertical lines
			for (float x = 0; x < size.X; x += gridSize)
			{
				UI.Graphics.DrawLine(
					new Vector2(pos.X + x, pos.Y),
					new Vector2(pos.X + x, pos.Y + size.Y),
					1f, GridColor);
			}

			// Horizontal lines
			for (float y = 0; y < size.Y; y += gridSize)
			{
				UI.Graphics.DrawLine(
					new Vector2(pos.X, pos.Y + y),
					new Vector2(pos.X + size.X, pos.Y + y),
					1f, GridColor);
			}
		}

		/// <summary>
		/// Calculates the absolute position of a control within the canvas,
		/// taking into account parent control offsets and anchor adjustments for nested controls.
		/// </summary>
		private Vector2 GetControlAbsolutePositionInCanvas(Control control, Vector2 canvasPos)
		{
			// Start with this control's position including anchor adjustment
			Vector2 pos = control.GetAnchorAdjustedRelativePosition();

			// Walk up the parent chain to accumulate ALL parent offsets (also anchor-adjusted)
			Control parent = control.GetParent();
			while (parent != null && parent != this)
			{
				pos += parent.GetAnchorAdjustedRelativePosition();
				parent = parent.GetParent();
			}

			return canvasPos + pos;
		}

		/// <summary>
		/// Gets the accumulated parent offset for a control (excluding the control's own position).
		/// Includes anchor adjustments for all parent controls.
		/// </summary>
		private Vector2 GetParentOffset(Control control)
		{
			Vector2 offset = Vector2.Zero;

			Control parent = control.GetParent();
			while (parent != null && parent != this)
			{
				offset += parent.GetAnchorAdjustedRelativePosition();
				parent = parent.GetParent();
			}

			return offset;
		}







		/// <summary>
		/// Color used to outline controls that have Visible = false.
		/// </summary>
		public FishColor InvisibleControlColor { get; set; } = new FishColor(255, 105, 180, 200); // Hot pink

		/// <summary>
		/// Color used to highlight valid drop targets during drag operations.
		/// </summary>
		public FishColor DropTargetHighlightColor { get; set; } = new FishColor(0, 200, 100, 200); // Green

		/// <summary>
		/// The currently hovered drop target during a drag operation (null if not hovering over a valid target).
		/// </summary>
		public Control HoveredDropTarget { get; set; }

		private void DrawInertControl(FishUI.FishUI UI, Control control, Vector2 canvasPos, float Dt, float Time)
		{
			// Get control's relative position as Vector2
			Vector2 ctrlRelPos = new Vector2(control.Position.X, control.Position.Y);

			// Store original position and adjust for canvas offset
			FishUIPosition originalPos = control.Position;
			control.Position = canvasPos + ctrlRelPos;

			// Check if control is invisible - draw placeholder instead
			if (!control.Visible)
			{
				DrawInvisibleControlPlaceholder(UI, control);
			}
			else
			{
				// Draw the control normally (it won't process input since it's not added to FishUI)
				control.DrawControl(UI, Dt, Time);
			}

			// Explicitly draw children while parent position is still in screen coordinates
			// Don't modify child positions - let GetAbsolutePosition calculate correctly
			// (Parent.GetAbsolutePosition() + child.Position = parentScreenPos + childRelativePos)
			DrawChildrenRecursive(UI, control, Dt, Time);

			// Restore position after drawing children
			control.Position = originalPos;
		}

		private void DrawInvisibleControlPlaceholder(FishUI.FishUI UI, Control control)
		{
			Vector2 pos = control.GetAbsolutePosition();
			Vector2 size = control.Size;

			// Draw semi-transparent pink fill
			UI.Graphics.DrawRectangle(pos, size, new FishColor(255, 105, 180, 40));

			// Draw pink outline
			UI.Graphics.DrawRectangleOutline(pos, size, InvisibleControlColor);

			// Draw diagonal lines to indicate invisible
			UI.Graphics.DrawLine(pos, pos + size, 1f, InvisibleControlColor);
			UI.Graphics.DrawLine(new Vector2(pos.X + size.X, pos.Y), new Vector2(pos.X, pos.Y + size.Y), 1f, InvisibleControlColor);

			// Draw "HIDDEN" label
			string label = $"[Hidden] {control.GetType().Name}";
			UI.Graphics.DrawTextColor(UI.Settings.FontDefault, label, pos + new Vector2(4, 4), InvisibleControlColor);
		}

		private void DrawChildrenRecursive(FishUI.FishUI UI, Control parent, float Dt, float Time)
		{
			// Draw children sorted by ZDepth (lowest first, so higher ZDepth draws on top)
			foreach (var child in parent.Children.OrderBy(c => c.ZDepth))
			{
				// Check if child is invisible - draw placeholder instead
				if (!child.Visible)
				{
					DrawInvisibleControlPlaceholder(UI, child);
				}
				else
				{
					// Draw child using its relative position - GetAbsolutePosition will
					// correctly add the parent's current (screen) position
					child.DrawControl(UI, Dt, Time);
				}

				// Recursively draw this child's children
				DrawChildrenRecursive(UI, child, Dt, Time);
			}
		}



		private void DrawSelectionHighlight(FishUI.FishUI UI, Control control, Vector2 canvasPos)
		{
			// Calculate absolute position for the control (including parent offset if nested)
			Vector2 ctrlAbsPos = GetControlAbsolutePositionInCanvas(control, canvasPos);
			// Get anchor-adjusted size (accounts for stretching when anchored to opposite edges)
			Vector2 ctrlSize = control.GetAnchorAdjustedSize();

			// Draw selection rectangle
			UI.Graphics.DrawRectangleOutline(ctrlAbsPos, ctrlSize, SelectionColor);

			// Draw resize handles
			DrawHandle(UI, ctrlAbsPos, ctrlSize, ResizeHandle.TopLeft);
			DrawHandle(UI, ctrlAbsPos, ctrlSize, ResizeHandle.Top);
			DrawHandle(UI, ctrlAbsPos, ctrlSize, ResizeHandle.TopRight);
			DrawHandle(UI, ctrlAbsPos, ctrlSize, ResizeHandle.Left);
			DrawHandle(UI, ctrlAbsPos, ctrlSize, ResizeHandle.Right);
			DrawHandle(UI, ctrlAbsPos, ctrlSize, ResizeHandle.BottomLeft);
			DrawHandle(UI, ctrlAbsPos, ctrlSize, ResizeHandle.Bottom);
			DrawHandle(UI, ctrlAbsPos, ctrlSize, ResizeHandle.BottomRight);
		}

		private void DrawHandle(FishUI.FishUI UI, Vector2 pos, Vector2 size, ResizeHandle handle)
		{
			Vector2 handlePos = GetHandlePosition(pos, size, handle);
			float hs = HandleSize;

			UI.Graphics.DrawRectangle(
				new Vector2(handlePos.X - hs / 2, handlePos.Y - hs / 2),
				new Vector2(hs, hs),
				SelectionColor);
		}

		private Vector2 GetHandlePosition(Vector2 pos, Vector2 size, ResizeHandle handle)
		{
			return handle switch
			{
				ResizeHandle.TopLeft => pos,
				ResizeHandle.Top => new Vector2(pos.X + size.X / 2, pos.Y),
				ResizeHandle.TopRight => new Vector2(pos.X + size.X, pos.Y),
				ResizeHandle.Left => new Vector2(pos.X, pos.Y + size.Y / 2),
				ResizeHandle.Right => new Vector2(pos.X + size.X, pos.Y + size.Y / 2),
				ResizeHandle.BottomLeft => new Vector2(pos.X, pos.Y + size.Y),
				ResizeHandle.Bottom => new Vector2(pos.X + size.X / 2, pos.Y + size.Y),
				ResizeHandle.BottomRight => new Vector2(pos.X + size.X, pos.Y + size.Y),
				_ => pos
			};
		}

		private ResizeHandle GetHandleAtPosition(Vector2 mousePos, Vector2 ctrlPos, Vector2 ctrlSize)
		{
			float hs = HandleSize;

			ResizeHandle[] handles = {
				ResizeHandle.TopLeft, ResizeHandle.Top, ResizeHandle.TopRight,
				ResizeHandle.Left, ResizeHandle.Right,
				ResizeHandle.BottomLeft, ResizeHandle.Bottom, ResizeHandle.BottomRight
			};

			foreach (var handle in handles)
			{
				Vector2 handlePos = GetHandlePosition(ctrlPos, ctrlSize, handle);
				if (Math.Abs(mousePos.X - handlePos.X) <= hs &&
					Math.Abs(mousePos.Y - handlePos.Y) <= hs)
				{
					return handle;
				}
			}

			return ResizeHandle.None;
		}

		public override void HandleMousePress(FishUI.FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMousePress(UI, InState, Btn, Pos);

			if (Btn != FishMouseButton.Left)
				return;

			Vector2 canvasPos = GetAbsolutePosition();
			Vector2 localPos = Pos - canvasPos;

			// Check if clicking on a resize handle of selected control
			if (SelectedControl != null)
			{
				Vector2 ctrlAbsPos = GetControlAbsolutePositionInCanvas(SelectedControl, canvasPos);
				Vector2 ctrlSize = SelectedControl.GetAnchorAdjustedSize();

				_activeHandle = GetHandleAtPosition(Pos, ctrlAbsPos, ctrlSize);
				if (_activeHandle != ResizeHandle.None)
				{
					_isResizing = true;
					return;
				}
			}

			// Check if clicking on any control (including children)
			Control clickedControl = FindControlAtPosition(localPos);

			if (clickedControl != null)
			{
				SelectControl(clickedControl);
				_isDragging = true;
				_dragStartParent = clickedControl.GetParent(); // Track original parent for reparenting
															   // Calculate drag offset - for child controls, we need to account for parent offset
				Vector2 controlAbsPos = GetControlAbsolutePositionInCanvas(clickedControl, Vector2.Zero);
				_dragOffset = localPos - controlAbsPos;
			}
			else
			{
				SelectControl(null);
			}
		}

		public override void HandleMouseRelease(FishUI.FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMouseRelease(UI, InState, Btn, Pos);

			if (_isDragging && SelectedControl != null)
			{
				// Check for reparenting
				Vector2 canvasPos = GetAbsolutePosition();
				Vector2 localPos = Pos - canvasPos;

				// Find container at drop position (excluding the control being dragged and its children)
				Control newParent = FindContainerAtPositionExcluding(localPos, SelectedControl);

				// Get current parent
				Control currentParent = SelectedControl.GetParent();

				// Check if parent changed
				if (newParent != currentParent)
				{
					// Calculate the control's current absolute position in canvas space
					Vector2 controlAbsPos = GetControlAbsolutePositionInCanvas(SelectedControl, Vector2.Zero);

					// Remove from current parent
					if (currentParent != null)
					{
						currentParent.RemoveChild(SelectedControl);
					}
					else
					{
						// Remove from root edited controls
						_editedControls.Remove(SelectedControl);
					}

					// Add to new parent
					if (newParent != null)
					{
						// Calculate position relative to new parent
						Vector2 newParentAbsPos = GetControlAbsolutePositionInCanvas(newParent, Vector2.Zero);
						Vector2 newRelativePos = controlAbsPos - newParentAbsPos;

						// Snap to grid if enabled
						if (ShowGrid)
						{
							newRelativePos.X = (float)Math.Round(newRelativePos.X / GridSize) * GridSize;
							newRelativePos.Y = (float)Math.Round(newRelativePos.Y / GridSize) * GridSize;
						}

						SelectedControl.Position = newRelativePos;
						newParent.AddChild(SelectedControl);
					}
					else
					{
						// Move to root level - clear any remaining parent reference
						SelectedControl.ClearParent();

						Vector2 newPos = controlAbsPos;

						// Snap to grid if enabled
						if (ShowGrid)
						{
							newPos.X = (float)Math.Round(newPos.X / GridSize) * GridSize;
							newPos.Y = (float)Math.Round(newPos.Y / GridSize) * GridSize;
						}

						SelectedControl.Position = newPos;
						_editedControls.Add(SelectedControl);
					}

					OnControlReparented?.Invoke(SelectedControl);
				}
				else
				{
					OnControlModified?.Invoke(SelectedControl);
				}
			}
			else if (_isResizing && SelectedControl != null)
			{
				OnControlModified?.Invoke(SelectedControl);
			}

		_isDragging = false;
			_isResizing = false;
			_activeHandle = ResizeHandle.None;
			_dragStartParent = null;
			HoveredDropTarget = null;
		}

		public override void HandleDrag(FishUI.FishUI UI, Vector2 StartPos, Vector2 EndPos, FishInputState InState)
		{
			base.HandleDrag(UI, StartPos, EndPos, InState);

			if (SelectedControl == null)
				return;

			Vector2 canvasPos = GetAbsolutePosition();
			Vector2 localPos = EndPos - canvasPos;

			if (_isDragging)
			{
				// Track hovered drop target for visual feedback
				HoveredDropTarget = FindContainerAtPositionExcluding(localPos, SelectedControl);

				// Calculate new position - for child controls, subtract parent offset
				Vector2 parentOffset = GetParentOffset(SelectedControl);
				Vector2 newPos = localPos - _dragOffset - parentOffset;

				// Snap to grid if enabled
				if (ShowGrid)
				{
					newPos.X = (float)Math.Round(newPos.X / GridSize) * GridSize;
					newPos.Y = (float)Math.Round(newPos.Y / GridSize) * GridSize;
				}

				SelectedControl.Position = newPos;

				// Update anchor offsets to keep anchoring in sync with new position
				SelectedControl.UpdateOwnAnchorOffsets();
			}
			else if (_isResizing && _activeHandle != ResizeHandle.None)
			{
				// Resize control - work with Vector2 for easier math
				Vector2 ctrlPos = new Vector2(SelectedControl.Position.X, SelectedControl.Position.Y);
				Vector2 ctrlSize = SelectedControl.Size;

				switch (_activeHandle)
				{
					case ResizeHandle.Right:
						ctrlSize.X = localPos.X - ctrlPos.X;
						break;
					case ResizeHandle.Bottom:
						ctrlSize.Y = localPos.Y - ctrlPos.Y;
						break;
					case ResizeHandle.BottomRight:
						ctrlSize.X = localPos.X - ctrlPos.X;
						ctrlSize.Y = localPos.Y - ctrlPos.Y;
						break;
					case ResizeHandle.Left:
						float right = ctrlPos.X + ctrlSize.X;
						ctrlPos.X = localPos.X;
						ctrlSize.X = right - ctrlPos.X;
						break;
					case ResizeHandle.Top:
						float bottom = ctrlPos.Y + ctrlSize.Y;
						ctrlPos.Y = localPos.Y;
						ctrlSize.Y = bottom - ctrlPos.Y;
						break;
					case ResizeHandle.TopLeft:
						right = ctrlPos.X + ctrlSize.X;
						bottom = ctrlPos.Y + ctrlSize.Y;
						ctrlPos.X = localPos.X;
						ctrlPos.Y = localPos.Y;
						ctrlSize.X = right - ctrlPos.X;
						ctrlSize.Y = bottom - ctrlPos.Y;
						break;
					case ResizeHandle.TopRight:
						bottom = ctrlPos.Y + ctrlSize.Y;
						ctrlPos.Y = localPos.Y;
						ctrlSize.X = localPos.X - ctrlPos.X;
						ctrlSize.Y = bottom - ctrlPos.Y;
						break;
					case ResizeHandle.BottomLeft:
						right = ctrlPos.X + ctrlSize.X;
						ctrlPos.X = localPos.X;
						ctrlSize.X = right - ctrlPos.X;
						ctrlSize.Y = localPos.Y - ctrlPos.Y;
						break;
				}

				// Snap to grid
				if (ShowGrid)
				{
					ctrlPos.X = (float)Math.Round(ctrlPos.X / GridSize) * GridSize;
					ctrlPos.Y = (float)Math.Round(ctrlPos.Y / GridSize) * GridSize;
					ctrlSize.X = (float)Math.Round(ctrlSize.X / GridSize) * GridSize;
					ctrlSize.Y = (float)Math.Round(ctrlSize.Y / GridSize) * GridSize;
				}

				// Minimum size
				ctrlSize.X = Math.Max(GridSize, ctrlSize.X);
				ctrlSize.Y = Math.Max(GridSize, ctrlSize.Y);

				SelectedControl.Position = ctrlPos;
				SelectedControl.Size = ctrlSize;

				// Update anchor offsets for this control (so it stays in place relative to ITS parent)
				SelectedControl.UpdateOwnAnchorOffsets();

				// NOTE: Do NOT call RecalculateChildAnchors() here!
				// Children should move/resize according to their anchor settings when the parent is resized.
				// RecalculateChildAnchors() would reset their AnchorParentSize, making anchoring have no effect.
			}
		}

		/// <summary>
		/// Checks if a control type can contain child controls.
		/// </summary>
		public static bool IsContainerControl(Control control)
		{
			return control is Panel || control is GroupBox || control is Window;
		}

		/// <summary>
		/// Finds the topmost container control at the given local position (relative to canvas).
		/// Returns null if no container is found at that position.
		/// </summary>
		public Control FindContainerAtPosition(Vector2 localPos)
		{
			// Search in reverse order (topmost first)
			for (int i = _editedControls.Count - 1; i >= 0; i--)
			{
				var ctrl = _editedControls[i];
				if (!IsContainerControl(ctrl))
					continue;

				Vector2 ctrlPos = ctrl.GetAnchorAdjustedRelativePosition();
				Vector2 ctrlSize = ctrl.GetAnchorAdjustedSize();

				if (localPos.X >= ctrlPos.X && localPos.X <= ctrlPos.X + ctrlSize.X &&
					localPos.Y >= ctrlPos.Y && localPos.Y <= ctrlPos.Y + ctrlSize.Y)
				{
					return ctrl;
				}
			}
			return null;
		}

		/// <summary>
		/// Finds the topmost container control at the given local position, excluding a specific control and its children.
		/// Used for reparenting to avoid dropping a control onto itself.
		/// </summary>
		public Control FindContainerAtPositionExcluding(Vector2 localPos, Control excludeControl)
		{
			// Search all controls including nested ones
			Control bestContainer = null;

			foreach (var ctrl in _editedControls)
			{
				var found = FindContainerAtPositionRecursive(ctrl, localPos, Vector2.Zero, excludeControl);
				if (found != null)
					bestContainer = found;
			}

			return bestContainer;
		}

		private Control FindContainerAtPositionRecursive(Control control, Vector2 localPos, Vector2 parentOffset, Control excludeControl)
		{
			// Skip if this is the excluded control or a descendant of it
			if (control == excludeControl || IsDescendantOf(control, excludeControl))
				return null;

			Vector2 ctrlPos = parentOffset + control.GetAnchorAdjustedRelativePosition();
			Vector2 ctrlSize = control.GetAnchorAdjustedSize();

			Control result = null;

			// Check if position is within this control and it's a container
			if (IsContainerControl(control) &&
				localPos.X >= ctrlPos.X && localPos.X <= ctrlPos.X + ctrlSize.X &&
				localPos.Y >= ctrlPos.Y && localPos.Y <= ctrlPos.Y + ctrlSize.Y)
			{
				result = control;
			}

			// Check children (deeper containers take precedence)
			foreach (var child in control.Children)
			{
				var childResult = FindContainerAtPositionRecursive(child, localPos, ctrlPos, excludeControl);
				if (childResult != null)
					result = childResult;
			}

			return result;
		}

		/// <summary>
		/// Checks if a control is a descendant (child, grandchild, etc.) of another control.
		/// </summary>
		private bool IsDescendantOf(Control control, Control potentialAncestor)
		{
			if (potentialAncestor == null)
				return false;

			foreach (var child in potentialAncestor.Children)
			{
				if (child == control)
					return true;
				if (IsDescendantOf(control, child))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Gets all controls including children of containers (flattened list for iteration).
		/// </summary>
		public List<Control> GetAllControlsFlattened()
		{
			var result = new List<Control>();
			foreach (var ctrl in _editedControls)
			{
				result.Add(ctrl);
				AddChildrenRecursive(ctrl, result);
			}
			return result;
		}

		private void AddChildrenRecursive(Control parent, List<Control> result)
		{
			foreach (var child in parent.Children)
			{
				result.Add(child);
				AddChildrenRecursive(child, result);
			}
		}

		/// <summary>
		/// Finds the topmost control at the given local position, including children.
		/// Controls with higher ZDepth are considered "on top" and selected first.
		/// </summary>
		private Control FindControlAtPosition(Vector2 localPos)
		{
			// Sort by ZDepth descending (highest first) to find topmost control
			foreach (var ctrl in _editedControls.OrderByDescending(c => c.ZDepth))
			{
				var found = FindControlAtPositionRecursive(ctrl, localPos, Vector2.Zero);
				if (found != null)
					return found;
			}
			return null;
		}

		/// <summary>
		/// Recursively searches for a control at the given position.
		/// Children with higher ZDepth are checked first.
		/// Uses anchor-adjusted positions and sizes for accurate hit testing.
		/// </summary>
		private Control FindControlAtPositionRecursive(Control control, Vector2 localPos, Vector2 parentOffset)
		{
			// Use anchor-adjusted position and size for accurate hit testing
			Vector2 ctrlPos = parentOffset + control.GetAnchorAdjustedRelativePosition();
			Vector2 ctrlSize = control.GetAnchorAdjustedSize();

			// Check if position is within this control
			if (localPos.X >= ctrlPos.X && localPos.X <= ctrlPos.X + ctrlSize.X &&
				localPos.Y >= ctrlPos.Y && localPos.Y <= ctrlPos.Y + ctrlSize.Y)
			{
				// Check children first (sorted by ZDepth descending - highest on top)
				foreach (var child in control.Children.OrderByDescending(c => c.ZDepth))
				{
					var found = FindControlAtPositionRecursive(child, localPos, ctrlPos);
					if (found != null)
						return found;
				}

				// No child found, return this control
				return control;
			}

			return null;
		}
	}
}
