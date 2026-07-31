using FishUI.Controls;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Globalization;

namespace FishUI
{
	/// <summary>
	/// Main FishUI class that manages the UI system, controls, input handling, and rendering.
	/// </summary>
	public class FishUI : IDisposable
	{
		/// <summary>
		/// UI settings including theme, fonts, and control appearance.
		/// </summary>
		public FishUISettings Settings;

		/// <summary>
		/// Graphics backend interface for rendering.
		/// </summary>
		public IFishUIGfx Graphics;

		/// <summary>
		/// Input backend interface for mouse and keyboard input.
		/// </summary>
		public IFishUIInput Input;

		/// <summary>
		/// Events interface for broadcasting control events.
		/// </summary>
		public IFishUIEvents Events;

		/// <summary>
		/// File system interface for file operations (theme loading, layout serialization, etc.).
		/// Set to a custom IFishUIFileSystem implementation to use virtual file systems,
		/// embedded resources, or game engine asset systems.
		/// </summary>
		public IFishUIFileSystem FileSystem;

		List<Control> Controls;

		/// <summary>
		/// Width of the UI area in pixels.
		/// </summary>
		public int Width;

		/// <summary>
		/// Height of the UI area in pixels.
		/// </summary>
		public int Height;

		/// <summary>
		/// The control that currently has input focus.
		/// </summary>
		public Control InputActiveControl;

		//
		Control HoveredControl;
		Control LeftClickedControl;
		Control RightClickedControl;
		long? ActiveDragInteractionId;
		Vector2 ActiveDragStart;
		bool ActiveDiagnosticDragStarted;

		// Double-click detection
		float LastLeftClickTime = -1f;
		float LastRightClickTime = -1f;
		Vector2 LastLeftClickPos;
		Vector2 LastRightClickPos;
		Control LastLeftClickControl;
		Control LastRightClickControl;

		/// <summary>
		/// Maximum time between clicks for a double-click (in seconds).
		/// </summary>
		public float DoubleClickTime { get; set; } = 0.15f;

		/// <summary>
		/// Maximum distance between clicks for a double-click (in pixels).
		/// </summary>
		public float DoubleClickDistance { get; set; } = 5f;

		/// <summary>
		/// Manager for global keyboard hotkeys.
		/// </summary>
		public FishUIHotkeyManager Hotkeys { get; } = new FishUIHotkeyManager();

		/// <summary>
		/// Virtual mouse cursor for keyboard/gamepad input.
		/// </summary>
		public FishUIVirtualMouse VirtualMouse { get; } = new FishUIVirtualMouse();

		/// <summary>
		/// Registry for named event handlers used with layout serialization.
		/// Register handlers by name, then reference them in YAML layout files.
		/// </summary>
		public EventHandlerRegistry EventHandlers { get; } = new EventHandlerRegistry();

		/// <summary>
		/// The current modal control (if any). When set, only this control and its children receive input.
		/// </summary>
		public Control ModalControl { get; private set; }

		// Z-order management
		private int _nextZDepth = 0;

		// Tooltip management
		private Controls.Tooltip _activeTooltip;
		private Control _tooltipTargetControl;
		private float _tooltipHoverTime = 0f;

		/// <summary>
		/// Delay in seconds before showing tooltips.
		/// </summary>
		public float TooltipShowDelay { get; set; } = 0.5f;

		/// <summary>
		/// Animation manager for UI transitions and effects.
		/// </summary>
		public FishUIAnimationManager Animations { get; } = new FishUIAnimationManager();

		/// <summary>Diagnostic recording and snapshot capture for this UI instance.</summary>
		public FishUIDiagnosticsSession Diagnostics { get; }

		/// <summary>Convenience access to this UI instance's diagnostic event recorder.</summary>
		public FishUIEventRecorder DiagnosticsEvents => Diagnostics.Events;

		internal Control DiagnosticsHoveredControl => HoveredControl;
		internal Control DiagnosticsPressedControl => LeftClickedControl ?? RightClickedControl;

		Control[] OrderedControls;
		private readonly HashSet<KeyboardCaptureLease> _keyboardCaptureLeases = new HashSet<KeyboardCaptureLease>();
		private bool _keyboardInputConsumedThisFrame;
		private bool _disposed;

		public bool WantsKeyboardCapture => _keyboardCaptureLeases.Count != 0 || _keyboardInputConsumedThisFrame;

		/// <summary>
		/// Creates a new FishUI instance.
		/// </summary>
		/// <param name="Settings">UI settings for themes and appearance.</param>
		/// <param name="Graphics">Graphics backend implementation.</param>
		/// <param name="Input">Input backend implementation.</param>
		/// <param name="Events">Events handler for control events.</param>							
		/// <param name="FS">File system implementation. If null, uses DefaultFishUIFileSystem.</param>
		public FishUI(FishUISettings Settings, IFishUIGfx Graphics, IFishUIInput Input, IFishUIEvents Events, IFishUIFileSystem FS = null)
		{
			Controls = new List<Control>();

			if (FS == null)
				FS = new DefaultFishUIFileSystem();

			this.Settings = Settings;
			this.Graphics = Graphics;
			this.Input = Input;
			this.Events = Events;
			this.FileSystem = FS;
			Diagnostics = new FishUIDiagnosticsSession(this);

			// Create the global tooltip
			_activeTooltip = new Controls.Tooltip();
			_activeTooltip._FishUI = this;
			Diagnostics.AttachControl(_activeTooltip);
		}

		/// <summary>
		/// Initializes the UI system. Must be called before using the UI.
		/// </summary>
		public void Init()
		{
			Graphics.Init();
			Settings.Init(this);
		}

		/// <summary>
		/// Gets the next available Z-depth value for a control.
		/// </summary>
		internal int GetNextZDepth()
		{
			return _nextZDepth++;
		}

		/// <summary>
		/// Gets the highest Z-depth among all controls (excluding AlwaysOnTop controls).
		/// </summary>
		internal int GetHighestZDepth()
		{
			int highest = 0;
			foreach (var c in Controls)
			{
				if (!c.AlwaysOnTop && c.ZDepth > highest)
					highest = c.ZDepth;
			}
			return highest;
		}

		/// <summary>
		/// Gets the lowest Z-depth among all controls.
		/// </summary>
		internal int GetLowestZDepth()
		{
			int lowest = int.MaxValue;
			foreach (var c in Controls)
			{
				if (c.ZDepth < lowest)
					lowest = c.ZDepth;
			}
			return lowest == int.MaxValue ? 0 : lowest;
		}

		/// <summary>
		/// Sets the modal control. Only this control (and its children) will receive input.
		/// Pass null to clear modal mode.
		/// </summary>
		public void SetModalControl(Control control)
		{
			Control previous = ModalControl;
			ModalControl = control;
			if (control != null)
			{
				// Bring modal control to front
				control.BringToFront();
			}
			if (Diagnostics.IsEventRecordingEnabled)
				Diagnostics.Record(FishUIDiagnosticEventCategory.Modal, FishUIDiagnosticEventType.ModalChanged, control,
					$"from={previous?.DiagnosticRuntimeId.ToString() ?? "null"};to={control?.DiagnosticRuntimeId.ToString() ?? "null"}");
		}

		/// <summary>
		/// Adds a control to the UI.
		/// </summary>
		/// <param name="C">The control to add.</param>
		public void AddControl(Control C)
		{
			if (C == null) throw new ArgumentNullException(nameof(C));
			if (_disposed) throw new ObjectDisposedException(nameof(FishUI));
			if (C.AttachedFishUI == this && C.GetParent() == null && Controls.Contains(C)) return;

			FishUI oldUi = C.AttachedFishUI;
			Control oldParent = C.GetParent();
			int oldIndex = oldParent != null ? oldParent.Children.IndexOf(C) : oldUi?.IndexOfRoot(C) ?? -1;
			int oldZDepth = C.ZDepth;
			int previousNextZDepth = _nextZDepth;

			if (ReferenceEquals(oldUi, this))
			{
				if (oldParent != null) oldParent.Children.Remove(C); else Controls.Remove(C);
				C.SetParentInternal(null);
				C._FishUI = this;
				C.ZDepth = GetNextZDepth();
				Controls.Add(C);
				Diagnostics.NotifyHierarchyChanged();
				return;
			}

			if (oldUi != null) C.DetachSubtree(oldUi);
			if (oldParent != null) oldParent.Children.Remove(C); else oldUi?.RemoveRootReference(C);
			C.SetParentInternal(null);
			C._FishUI = this;
			C.ZDepth = GetNextZDepth();
			Controls.Add(C);
			try
			{
				Diagnostics.AttachControl(C);
				C.AttachSubtree(this);
				C.ResizeSubtree(this, Width, Height);
			}
			catch (Exception attachFailure)
			{
				Controls.Remove(C);
				C._FishUI = null;
				_nextZDepth = previousNextZDepth;
				try
				{
					C.ZDepth = oldZDepth;
					if (oldParent != null)
					{
						C.SetParentInternal(oldParent);
						oldParent.Children.Insert(Math.Max(0, Math.Min(oldIndex, oldParent.Children.Count)), C);
					}
					else if (oldUi != null)
					{
						C._FishUI = oldUi;
						oldUi.InsertRootReference(C, oldIndex);
					}
					if (oldUi != null) C.AttachSubtree(oldUi);
				}
				catch (Exception rollbackFailure) { throw new AggregateException(attachFailure, rollbackFailure); }
				throw;
			}
			Diagnostics.NotifyHierarchyChanged();
		}

		/// <summary>
		/// Removes a control from the UI.
		/// </summary>
		/// <param name="C">The control to remove.</param>
		/// <returns>True if the control was found and removed, false otherwise.</returns>
		public bool RemoveControl(Control C)
		{
			if (Controls.Remove(C))
			{
				if (C.AttachedFishUI == this) C.DetachSubtree(this);
				C._FishUI = null;
				Diagnostics.NotifyHierarchyChanged();
				if (ModalControl == C || ModalControl?.IsDescendantOf(C) == true)
					ModalControl = null;
				if (InputActiveControl == C || InputActiveControl?.IsDescendantOf(C) == true)
					ClearFocus();
				return true;
			}
			return false;
		}

		/// <summary>
		/// Removes all controls from the UI.
		/// </summary>
		public void RemoveAllControls()
		{
			Control[] controls = Controls.ToArray();
			for (int i = controls.Length - 1; i >= 0; i--) RemoveControl(controls[i]);
			ModalControl = null;
			Diagnostics.NotifyHierarchyChanged();
		}

		internal int IndexOfRoot(Control control) => Controls.IndexOf(control);
		internal void RemoveRootReference(Control control) => Controls.Remove(control);
		internal void InsertRootReference(Control control, int index) =>
			Controls.Insert(Math.Max(0, Math.Min(index, Controls.Count)), control);

		public IDisposable AcquireKeyboardCapture(object owner)
		{
			if (owner == null) throw new ArgumentNullException(nameof(owner));
			if (_disposed) throw new ObjectDisposedException(nameof(FishUI));
			KeyboardCaptureLease lease = new KeyboardCaptureLease(this, owner);
			_keyboardCaptureLeases.Add(lease);
			return lease;
		}

		private void ReleaseKeyboardCapture(KeyboardCaptureLease lease) => _keyboardCaptureLeases.Remove(lease);

		private sealed class KeyboardCaptureLease : IDisposable
		{
			private FishUI _ui;
			internal readonly object Owner;
			internal KeyboardCaptureLease(FishUI ui, object owner) { _ui = ui; Owner = owner; }
			public void Dispose()
			{
				FishUI ui = _ui;
				if (ui == null) return;
				_ui = null;
				ui.ReleaseKeyboardCapture(this);
			}
			internal void Invalidate() => _ui = null;
		}

		/// <summary>
		/// Gets controls ordered by Z-depth for rendering (lowest first, AlwaysOnTop last).
		/// </summary>
		public Control[] GetOrderedControls()
		{
			// Sort: normal controls by ZDepth, then AlwaysOnTop controls by ZDepth
			var normal = Controls.Where(c => !c.AlwaysOnTop).OrderBy(c => c.ZDepth);
			var alwaysOnTop = Controls.Where(c => c.AlwaysOnTop).OrderBy(c => c.ZDepth);
			return normal.Concat(alwaysOnTop).ToArray();
		}

		/// <summary>
		/// Gets all controls without ordering.
		/// </summary>
		/// <returns>Array of all controls.</returns>
		public Control[] GetAllControls()
		{
			return Controls.ToArray();
		}

		/// <summary>
		/// Checks if a control is allowed to receive input (respects modal blocking).
		/// </summary>
		private bool IsControlInputAllowed(Control control)
		{
			if (ModalControl == null)
				return true;

			// Check if control is the modal control or a descendant of it
			Control c = control;
			while (c != null)
			{
				if (c == ModalControl)
					return true;
				c = c.GetParent();
			}
			return false;
		}

		/// <summary>
		/// Gets the root-level control that contains the given control.
		/// </summary>
		private Control GetRootControl(Control control)
		{
			if (control == null)
				return null;

			Control root = control;
			while (root.GetParent() != null)
			{
				root = root.GetParent();
			}
			return root;
		}

		/// <summary>
		/// Brings the root-level parent of a control to the front.
		/// This is called automatically on mouse press.
		/// </summary>
		private void BringControlToFrontOnClick(Control control)
		{
			if (control == null)
				return;

			Control root = GetRootControl(control);
			if (root != null && Controls.Contains(root) && !root.AlwaysOnTop)
			{
				root.BringToFront();
			}
		}

		// Top-down control picking, for mouse events etc
		Control PickControl(Control[] Controls, Vector2 GlobalPos, Control Parent = null, FishUIHitTestTraceBuilder trace = null)
		{
			foreach (Control C in Controls)
			{
				FishUIHitTestCandidate candidate = trace == null ? null : CreateHitTestCandidate(C, GlobalPos);
				if (candidate != null) trace.Trace.Candidates.Add(candidate);
				if (!C.Visible)
				{
					if (candidate != null) candidate.RejectionReason = FishUIHitTestRejectionReason.Invisible;
					continue;
				}

				// Check if parent allows this child to receive input at this position
				if (Parent != null && !Parent.ShouldChildReceiveInput(C, GlobalPos))
				{
					if (candidate != null) candidate.RejectionReason = FishUIHitTestRejectionReason.RejectedByParent;
					continue;
				}

				// First, check children even if parent's IsPointInside is false
				// This handles cases where children extend beyond parent bounds (e.g., DropDown list)
				Control[] children = C.GetAllChildren().Reverse().ToArray();
				Control CPicked = PickControl(children, GlobalPos, C, trace);

				if (CPicked != null)
				{
					// Check modal blocking
					if (!IsControlInputAllowed(CPicked))
					{
						if (candidate != null) candidate.RejectionReason = FishUIHitTestRejectionReason.BlockedByModal;
						return null;
					}
					if (candidate != null) candidate.RejectionReason = FishUIHitTestRejectionReason.DescendantSelected;
					return CPicked;
				}

				// Then check if point is inside this control
				if (C.IsPointInside(GlobalPos))
				{
					// Check modal blocking
					if (!IsControlInputAllowed(C))
					{
						if (candidate != null) candidate.RejectionReason = FishUIHitTestRejectionReason.BlockedByModal;
						return null;
					}
					if (candidate != null) { candidate.Accepted = true; candidate.RejectionReason = FishUIHitTestRejectionReason.None; }
					return C;
				}
				if (candidate != null) candidate.RejectionReason = FishUIHitTestRejectionReason.OutsideBounds;
			}

			return null;
		}

		/// <summary>
		/// Gets the control at the specified screen position.
		/// </summary>
		/// <param name="GlobalPos">Position in screen coordinates.</param>
		/// <returns>The topmost control at the position, or null if none.</returns>
		public Control PickControl(Vector2 GlobalPos)
		{
			return PickControlCore(GlobalPos, null);
		}

		private Control PickControlCore(Vector2 GlobalPos, FishUIHitTestTraceBuilder trace)
		{
			Control openDropDown = PickOpenDropDown(GlobalPos);
			if (openDropDown != null)
			{
				if (trace != null)
				{
					var candidate = CreateHitTestCandidate(openDropDown, GlobalPos); candidate.Accepted = true;
					trace.Trace.Candidates.Add(candidate);
				}
				return openDropDown;
			}

			// Reverse the order so we check front controls (higher Z-depth) first
			return PickControl(GetOrderedControls().Reverse().ToArray(), GlobalPos, null, trace);
		}

		internal FishUIHitTestTrace ExplainHitTestInternal(Vector2 point)
		{
			var result = new FishUIHitTestTrace { TraceId = Diagnostics.NextTraceId(), UiSessionId = Diagnostics.UiSessionId, PointPixels = FishUIDebugPoint.From(point) };
			Control control = PickControlCore(point, new FishUIHitTestTraceBuilder(result));
			if (control != null)
			{
				Diagnostics.EnsureIdentity(control); result.ResultControlId = control.DiagnosticRuntimeId;
				result.ResultPath = result.Candidates.LastOrDefault(c => c.ControlId == control.DiagnosticRuntimeId)?.ControlPath;
			}
			return result;
		}

		private FishUIHitTestCandidate CreateHitTestCandidate(Control control, Vector2 point)
		{
			Diagnostics.EnsureIdentity(control);
			Vector2 pos = control.GetAbsolutePosition(); Vector2 size = control.GetAbsoluteSize();
			var bounds = new FishUIDebugRect(pos.X, pos.Y, size.X, size.Y);
			FishUIDebugRect clip = GetExpectedClip(control);
			return new FishUIHitTestCandidate
			{
				ControlId = control.DiagnosticRuntimeId, ControlPath = Diagnostics.PathFor(control),
				BoundsPixels = bounds, ExpectedClipPixels = clip, InsideBounds = control.IsPointInside(point),
				InsideExpectedClip = clip == null || clip.Contains(point)
			};
		}

		private FishUIDebugRect GetExpectedClip(Control control)
		{
			var chain = new Stack<Control>(); Control current = control.GetParent();
			while (current != null) { chain.Push(current); current = current.GetParent(); }
			FishUIDebugRect clip = new FishUIDebugRect(0, 0, Width > 0 ? Width : Graphics.GetWindowWidth(), Height > 0 ? Height : Graphics.GetWindowHeight());
			while (chain.Count > 0)
			{
				Control ancestor = chain.Pop(); if (ancestor.DisableChildScissor) continue;
				Vector2 pos = ancestor.GetAbsolutePosition(); Vector2 size = ancestor.GetAbsoluteSize();
				clip = FishUIDebugRect.Intersect(clip, new FishUIDebugRect(pos.X, pos.Y, size.X, size.Y));
			}
			return clip;
		}

		private Control PickOpenDropDown(Vector2 globalPosition)
		{
			foreach (DropDown dropDown in DropDown.OpenDropdowns.ToArray().Reverse())
			{
				if (!dropDown.BelongsTo(this))
					continue;

				if (!dropDown.IsHierarchyVisible())
				{
					dropDown.Close();
					continue;
				}

				if (IsControlInputAllowed(dropDown) && dropDown.IsPointInside(globalPosition))
					return dropDown;
			}

			return null;
		}

		Control FindControlByIDEx(Control[] Ctrls, string ID)
		{
			foreach (Control C in Ctrls)
			{
				if (C.ID == ID)
					return C;

				Control Ret = FindControlByIDEx(C.GetAllChildren(), ID);
				if (Ret != null)
					return Ret;
			}

			return null;
		}

		/// <summary>
		/// Finds a control by its ID.
		/// </summary>
		/// <param name="ID">The ID to search for.</param>
		/// <returns>The control with the matching ID, or null if not found.</returns>
		public Control FindControlByID(string ID)
		{
			return FindControlByIDEx(Controls.ToArray(), ID);
		}

		/// <summary>
		/// Finds a control by its ID and returns it as the specified type.
		/// </summary>
		/// <typeparam name="T">The type of control to return.</typeparam>
		/// <param name="ID">The ID to search for.</param>
		/// <returns>The control with the matching ID cast to type T, or null if not found or type mismatch.</returns>
		public T FindControlByID<T>(string ID) where T : Control
		{
			return FindControlByIDEx(Controls.ToArray(), ID) as T;
		}

		private Control FindControlByDiagnosticId(long id)
		{
			var stack = new Stack<Control>(Controls);
			var visited = new HashSet<Control>();
			while (stack.Count > 0)
			{
				Control control = stack.Pop();
				if (control == null || !visited.Add(control)) continue;
				Diagnostics.EnsureIdentity(control);
				if (control.DiagnosticRuntimeId == id) return control;
				if (control.Children != null) foreach (Control child in control.Children) stack.Push(child);
			}
			return null;
		}

		void UpdateSingleControl(Control Ctl, FishInputState InState, FishInputState InLast)
		{
			if (!Ctl.Visible)
				return;

			Ctl.IsMousePressed = LeftClickedControl == Ctl;
			Ctl.IsMouseInside = HoveredControl == Ctl;

			Control[] Children = Ctl.GetAllChildren();

			foreach (Control C in Children)
				UpdateSingleControl(C, InState, InLast);
		}

		// Check for mouse press
		// Mouse press gets triggered for the first control under the mouse
		void CheckMousePress(Control ControlUnderMouse, FishInputState InState, bool BtnPressed, FishMouseButton MBtn, ref Control ClickedControl)
		{
			if (BtnPressed)
			{
				bool recordDiagnostics = Diagnostics.IsEventRecordingEnabled;
				FishUIDiagnosticEvent diagnosticEvent = null;
				if (recordDiagnostics)
					diagnosticEvent = Diagnostics.Record(FishUIDiagnosticEventCategory.Pointer,
						FishUIDiagnosticEventType.MouseButtonPressed, ControlUnderMouse, MBtn.ToString(),
						new FishUIPointerEventData { Button = MBtn.ToString(), PositionPixels = FishUIDebugPoint.From(InState.MousePos) });
				if (ControlUnderMouse != null)
				{
					// Bring the root control to front (for windows, panels, etc.)
					BringControlToFrontOnClick(ControlUnderMouse);

					using (Diagnostics.EnterCause(diagnosticEvent?.Sequence))
					{
						ControlUnderMouse.HandleMousePress(this, InState, MBtn, InState.MousePos);
						ClickedControl = ControlUnderMouse;
						Control focusTarget = FindMouseFocusTarget(ControlUnderMouse);
						if (recordDiagnostics)
							Diagnostics.Record(FishUIDiagnosticEventCategory.Focus, FishUIDiagnosticEventType.FocusResolution,
								focusTarget, "MouseFocusTargetOrFocusableAncestor", focus: new FishUIFocusEventData
								{ PickedControlId = ControlUnderMouse.DiagnosticRuntimeId, ToControlId = focusTarget?.DiagnosticRuntimeId, Reason = "MouseFocusTargetOrFocusableAncestor" });
						FocusControl(focusTarget);
					}
					if (MBtn == FishMouseButton.Left)
					{
						if (recordDiagnostics)
						{
							ActiveDragInteractionId = Diagnostics.NextInteractionId();
							ActiveDragStart = InState.MousePos;
							ActiveDiagnosticDragStarted = false;
						}
					}
				}
			}
		}

		private Control FindMouseFocusTarget(Control control)
		{
			while (control != null)
			{
				Control proxy = control.MouseFocusTarget;
				if (proxy != null && proxy.AttachedFishUI == this && proxy.Focusable &&
					!proxy.Disabled && proxy.IsHierarchyVisible())
					return proxy;

				if (control.Focusable)
					return control;

				control = control.GetParent();
			}

			return null;
		}

		// Check for mouse release and clicks
		// Mouse release gets triggered for the first control under the mouse
		// Mouse click gets triggered only after release if the control under the mouse is the same as the one that was pressed
		void CheckMouseRelease(Control ControlUnderMouse, FishInputState InState, bool BtnReleased, FishMouseButton MBtn, ref Control ClickedControl, float Time)
		{
			if (BtnReleased)
			{
				bool recordDiagnostics = Diagnostics.IsEventRecordingEnabled;
				Control pressOwner = ClickedControl;
				FishUIDiagnosticEvent diagnosticEvent = null;
				if (recordDiagnostics)
					diagnosticEvent = Diagnostics.Record(FishUIDiagnosticEventCategory.Pointer,
						FishUIDiagnosticEventType.MouseButtonReleased, ControlUnderMouse, MBtn.ToString(),
						new FishUIPointerEventData { Button = MBtn.ToString(), PositionPixels = FishUIDebugPoint.From(InState.MousePos) },
						interactionId: MBtn == FishMouseButton.Left && ActiveDiagnosticDragStarted ? ActiveDragInteractionId : null);
				using (Diagnostics.EnterCause(diagnosticEvent?.Sequence))
				{
				if (ControlUnderMouse != null)
					ControlUnderMouse.HandleMouseRelease(this, InState, MBtn, InState.MousePos);

				if (ClickedControl != null && ControlUnderMouse == ClickedControl)
				{
					// Check for double-click
					bool isDoubleClick = false;

					if (MBtn == FishMouseButton.Left)
					{
						float timeSinceLastClick = Time - LastLeftClickTime;
						float distance = Vector2.Distance(InState.MousePos, LastLeftClickPos);

						if (timeSinceLastClick <= DoubleClickTime && distance <= DoubleClickDistance && LastLeftClickControl == ControlUnderMouse)
						{
							isDoubleClick = true;
							LastLeftClickTime = -1f; // Reset to prevent triple-click being detected as double
						}
						else
						{
							LastLeftClickTime = Time;
							LastLeftClickPos = InState.MousePos;
							LastLeftClickControl = ControlUnderMouse;
						}
					}
					else if (MBtn == FishMouseButton.Right)
					{
						float timeSinceLastClick = Time - LastRightClickTime;
						float distance = Vector2.Distance(InState.MousePos, LastRightClickPos);

						if (timeSinceLastClick <= DoubleClickTime && distance <= DoubleClickDistance && LastRightClickControl == ControlUnderMouse)
						{
							isDoubleClick = true;
							LastRightClickTime = -1f;
						}
						else
						{
							LastRightClickTime = Time;
							LastRightClickPos = InState.MousePos;
							LastRightClickControl = ControlUnderMouse;
						}
					}

					if (isDoubleClick)
					{
						ClickedControl.HandleMouseDoubleClick(this, InState, MBtn, InState.MousePos);
						if (recordDiagnostics)
							Diagnostics.Record(FishUIDiagnosticEventCategory.Pointer, FishUIDiagnosticEventType.MouseDoubleClicked, ClickedControl, MBtn.ToString());
					}
					else
					{
						ClickedControl.HandleMouseClick(this, InState, MBtn, InState.MousePos);
						if (recordDiagnostics)
							Diagnostics.Record(FishUIDiagnosticEventCategory.Pointer, FishUIDiagnosticEventType.MouseClicked, ClickedControl, MBtn.ToString());
					}
				}
				}
				if (recordDiagnostics && MBtn == FishMouseButton.Left && ActiveDragInteractionId.HasValue && ActiveDiagnosticDragStarted)
					Diagnostics.Record(FishUIDiagnosticEventCategory.Drag, FishUIDiagnosticEventType.DragEnded, pressOwner,
						"released", new FishUIPointerEventData { StartPositionPixels = FishUIDebugPoint.From(ActiveDragStart), PositionPixels = FishUIDebugPoint.From(InState.MousePos), TotalDeltaPixels = FishUIDebugPoint.From(InState.MousePos - ActiveDragStart) }, interactionId: ActiveDragInteractionId);

				ClickedControl = null;
				if (MBtn == FishMouseButton.Left)
				{
					ActiveDragInteractionId = null;
					ActiveDiagnosticDragStarted = false;
				}
			}
		}

		void CheckTextInput(FishInputState InState, bool suppressTextInput)
		{
			if (suppressTextInput)
			{
				int rejectedCharacter;
				while ((rejectedCharacter = Input.GetCharPressed()) != 0)
				{
					if (Diagnostics.IsEventRecordingEnabled)
						Diagnostics.Record(FishUIDiagnosticEventCategory.TextInput, FishUIDiagnosticEventType.CharacterRejected,
							InputActiveControl, "consumedByKeyOrHotkey", text: new FishUITextEventData
							{
								CharacterCount = 1, LineCount = rejectedCharacter == '\n' ? 1 : 0,
								Character = ((char)rejectedCharacter).ToString(), CodePoint = rejectedCharacter,
								UnicodeCategory = char.GetUnicodeCategory((char)rejectedCharacter).ToString()
							});
				}
				return;
			}

			if (InputActiveControl != null)
			{
				if (Input.IsKeyPressed(FishKey.Backspace))
					InputActiveControl.HandleTextInput(this, InState, '\b');

				if (Input.IsKeyPressed(FishKey.Enter) || Input.IsKeyPressed(FishKey.KpEnter))
					InputActiveControl.HandleTextInput(this, InState, '\n');

				int InChr = 0;

				while ((InChr = Input.GetCharPressed()) != 0)
				{
					Control textTarget = InputActiveControl;
					bool accepted = !(textTarget is IFishUITextInputFilter filter) ||
						filter.ShouldAcceptTextInput(this, InState, (char)InChr);
					if (!accepted)
					{
						if (Diagnostics.IsEventRecordingEnabled)
							Diagnostics.Record(FishUIDiagnosticEventCategory.TextInput, FishUIDiagnosticEventType.CharacterRejected,
								textTarget, "rejectedByControl", text: new FishUITextEventData
								{
									CharacterCount = 1, LineCount = InChr == '\n' ? 1 : 0,
									Character = ((char)InChr).ToString(), CodePoint = InChr,
									UnicodeCategory = char.GetUnicodeCategory((char)InChr).ToString()
								});
						continue;
					}

					FishUIDiagnosticEvent textEvent = null;
					if (Diagnostics.IsEventRecordingEnabled)
						textEvent = Diagnostics.Record(FishUIDiagnosticEventCategory.TextInput, FishUIDiagnosticEventType.CharacterAccepted,
							textTarget, null, text: new FishUITextEventData
							{
								CharacterCount = 1, LineCount = InChr == '\n' ? 1 : 0, Character = ((char)InChr).ToString(),
								CodePoint = InChr, UnicodeCategory = char.GetUnicodeCategory((char)InChr).ToString()
							});
					using (Diagnostics.EnterCause(textEvent?.Sequence))
						textTarget.HandleTextInput(this, InState, (char)InChr);
				}
			}
		}

		void Update(Control[] Controls, FishInputState InState, FishInputState InLast, float DeltaTime, float Time)
		{
			bool recordDiagnostics = Diagnostics.IsEventRecordingEnabled;
			bool significantHitTest = recordDiagnostics && Diagnostics.Events.Options.RecordHitTestTraces &&
				(InState.MouseLeftPressed || InState.MouseLeftReleased || InState.MouseRightPressed || InState.MouseRightReleased || InState.MouseWheelDelta != 0);
			FishUIHitTestTrace hitTrace = significantHitTest ? ExplainHitTestInternal(InState.MousePos) : null;
			if (hitTrace != null)
				RecordHitTestObservations(hitTrace);
			Control ControlUnderMouse = significantHitTest && hitTrace.ResultControlId.HasValue
				? FindControlByDiagnosticId(hitTrace.ResultControlId.Value) : PickControl(InState.MousePos);

			// Mouse drag
			if (LeftClickedControl != null && InState.MouseLeft && InState.MouseDelta != Vector2.Zero)
			{
				if (recordDiagnostics)
				{
					Vector2 totalDelta = InState.MousePos - ActiveDragStart;
					if (!ActiveDiagnosticDragStarted && ActiveDragInteractionId.HasValue &&
						totalDelta.LengthSquared() >= Diagnostics.DragStartThresholdPixels * Diagnostics.DragStartThresholdPixels)
					{
						ActiveDiagnosticDragStarted = true;
						Diagnostics.Record(FishUIDiagnosticEventCategory.Drag, FishUIDiagnosticEventType.DragStarted,
							LeftClickedControl, "thresholdExceeded", new FishUIPointerEventData
							{
								StartPositionPixels = FishUIDebugPoint.From(ActiveDragStart),
								PositionPixels = FishUIDebugPoint.From(InState.MousePos),
								TotalDeltaPixels = FishUIDebugPoint.From(totalDelta)
							}, interactionId: ActiveDragInteractionId);
					}
					if (ActiveDiagnosticDragStarted)
					Diagnostics.Record(FishUIDiagnosticEventCategory.Drag, FishUIDiagnosticEventType.DragUpdated, LeftClickedControl,
						"drag", new FishUIPointerEventData { StartPositionPixels = FishUIDebugPoint.From(ActiveDragStart), PreviousPositionPixels = FishUIDebugPoint.From(InLast.MousePos), PositionPixels = FishUIDebugPoint.From(InState.MousePos), DeltaPixels = FishUIDebugPoint.From(InState.MouseDelta), TotalDeltaPixels = FishUIDebugPoint.From(InState.MousePos - ActiveDragStart) }, interactionId: ActiveDragInteractionId);
				}
				LeftClickedControl.HandleDrag(this, InLast.MousePos, InState.MousePos, InState);
			}

			// Mouse move
			if (HoveredControl == ControlUnderMouse && ControlUnderMouse != null && InState.MouseDelta != Vector2.Zero)
			{
				if (recordDiagnostics)
					Diagnostics.Record(FishUIDiagnosticEventCategory.Pointer, FishUIDiagnosticEventType.MouseMoved, ControlUnderMouse,
						null, new FishUIPointerEventData { PositionPixels = FishUIDebugPoint.From(InState.MousePos), DeltaPixels = FishUIDebugPoint.From(InState.MouseDelta) });
				ControlUnderMouse.HandleMouseMove(this, InState, InState.MousePos);
			}

			// Mouse enter/leave handling
			if (HoveredControl != ControlUnderMouse)
			{
				if (HoveredControl != null)
				{
					if (recordDiagnostics)
						Diagnostics.Record(FishUIDiagnosticEventCategory.Pointer, FishUIDiagnosticEventType.MouseLeft, HoveredControl);
					HoveredControl.HandleMouseLeave(this, InState);
				}

				if (ControlUnderMouse != null)
				{
					if (recordDiagnostics)
						Diagnostics.Record(FishUIDiagnosticEventCategory.Pointer, FishUIDiagnosticEventType.MouseEntered, ControlUnderMouse);
					ControlUnderMouse.HandleMouseEnter(this, InState);
				}
				HoveredControl = ControlUnderMouse;
			}


			// Left mouse press/release handling
			CheckMousePress(ControlUnderMouse, InState, InState.MouseLeftPressed, FishMouseButton.Left, ref LeftClickedControl);
			CheckMouseRelease(ControlUnderMouse, InState, InState.MouseLeftReleased, FishMouseButton.Left, ref LeftClickedControl, Time);

			// Right mouse press/release handling
			CheckMousePress(ControlUnderMouse, InState, InState.MouseRightPressed, FishMouseButton.Right, ref RightClickedControl);
			CheckMouseRelease(ControlUnderMouse, InState, InState.MouseRightReleased, FishMouseButton.Right, ref RightClickedControl, Time);

			// Mouse wheel handling
			if (InState.MouseWheelDelta != 0 && ControlUnderMouse != null)
			{
				if (recordDiagnostics)
					Diagnostics.Record(FishUIDiagnosticEventCategory.Pointer, FishUIDiagnosticEventType.MouseWheel, ControlUnderMouse,
						InState.MouseWheelDelta.ToString(CultureInfo.InvariantCulture), new FishUIPointerEventData { PositionPixels = FishUIDebugPoint.From(InState.MousePos), HitTestTraceId = hitTrace?.TraceId });
				ControlUnderMouse.HandleMouseWheel(this, InState, InState.MouseWheelDelta);
			}

			// Key press handling
			FishKey Key = Input.GetKeyPressed();
			FishUIDiagnosticEvent keyEvent = null;
			if (recordDiagnostics && Key != FishKey.None)
			{
				FishUIRawKeyMetadata metadata = null;
				if (Input is IFishUIRawInputMetadataProvider metadataProvider) metadataProvider.TryGetKeyMetadata(Key, out metadata);
				keyEvent = Diagnostics.Record(FishUIDiagnosticEventCategory.Keyboard, FishUIDiagnosticEventType.KeyPressed, InputActiveControl,
					Key.ToString(), key: new FishUIKeyEventData { Key = Key.ToString(), BackendKeyCode = metadata?.BackendKeyCode,
						Repeat = metadata?.Repeat ?? false, Released = metadata?.Released ?? false,
						Modifiers = new FishUIModifierSnapshot { Control = InState.CtrlDown, Shift = InState.ShiftDown, Alt = InState.AltDown } });
			}

			// Process global hotkeys first
			bool hotkeyHandled = Hotkeys.ProcessKeyPress(Key, Input, out FishUIHotkey matchedHotkey);
			bool suppressTextInput = hotkeyHandled && matchedHotkey.ConsumesTextInput;
			if (hotkeyHandled)
			{
				_keyboardInputConsumedThisFrame = true;
				FishUIDiagnosticEvent hotkeyEvent = null;
				if (recordDiagnostics || Diagnostics.IsEventRecordingEnabled)
					hotkeyEvent = Diagnostics.Record(FishUIDiagnosticEventCategory.Keyboard, FishUIDiagnosticEventType.HotkeyHandled, InputActiveControl,
						matchedHotkey?.ID, key: new FishUIKeyEventData { Key = Key.ToString(), HotkeyId = matchedHotkey?.ID, Consumed = true },
						bypassFilter: Diagnostics.IsCaptureHotkey(matchedHotkey));
				Diagnostics.CompleteHotkeyTrigger(matchedHotkey, hotkeyEvent);
			}

			if (!hotkeyHandled)
			{
				bool previewHandled = Key != FishKey.None && InputActiveControl != null &&
					InputActiveControl.PreviewKeyPress(this, InState, Key);
				if (previewHandled)
				{
					_keyboardInputConsumedThisFrame = true;
					suppressTextInput = true;
				}

				// Tab key navigation
				if (!previewHandled && Key == FishKey.Tab)
				{
					bool shiftHeld = Input.IsKeyDown(FishKey.LeftShift) || Input.IsKeyDown(FishKey.RightShift);
					FocusNextControl(shiftHeld);
					if (recordDiagnostics)
						Diagnostics.Record(FishUIDiagnosticEventCategory.Keyboard, FishUIDiagnosticEventType.TabNavigation, InputActiveControl, shiftHeld ? "previous" : "next");
				}
				else if (!previewHandled && Key != FishKey.None && InputActiveControl != null)
				{
					using (Diagnostics.EnterCause(keyEvent?.Sequence))
					{
						InputActiveControl.HandleKeyPress(this, InState, Key);
						InputActiveControl.HandleKeyDown(this, InState, (int)Key);
					}
				}
			}

			foreach (Control Ctl in Controls)
				UpdateSingleControl(Ctl, InState, InLast);

			CheckTextInput(InState, suppressTextInput);
			for (int i = 0; i < Controls.Length; i++)
				Controls[i].UpdateSubtree(this, DeltaTime, Time);
			InLast = InState;
		}

		private void RecordHitTestObservations(FishUIHitTestTrace trace)
		{
			FishUIHitTestCandidate accepted = trace.Candidates.FirstOrDefault(candidate => candidate.Accepted);
			if (accepted != null && !accepted.InsideExpectedClip)
			{
				Diagnostics.ReportLiveWarning("HIT_TARGET_OUTSIDE_EXPECTED_CLIP",
					"The selected control is outside its expected drawing clip.", FindControlByDiagnosticId(accepted.ControlId),
					accepted.ControlId.ToString(CultureInfo.InvariantCulture));
				return;
			}
			if (trace.ResultControlId.HasValue) return;
			FishUIHitTestCandidate visuallyInside = trace.Candidates.FirstOrDefault(candidate =>
				candidate.InsideBounds && candidate.InsideExpectedClip && !candidate.Accepted);
			if (visuallyInside != null)
				Diagnostics.ReportLiveWarning("CLICK_VISUALLY_INSIDE_BUT_HIT_TEST_REJECTED",
					"The point is visually inside a control that hit testing rejected.", FindControlByDiagnosticId(visuallyInside.ControlId),
					visuallyInside.ControlId.ToString(CultureInfo.InvariantCulture) + ":" + visuallyInside.RejectionReason);
		}

		void Draw(Control[] Controls, float Dt, float Time)
		{
			// Update animations
			Animations.Update(Dt);
			IFishUIGfx originalGraphics = Graphics;
			Exception failure = null;
			string failureStage = "captureSetup";
			try
			{
				RecordingFishUIGfx recordingGraphics = Diagnostics.BeginDraw(originalGraphics);
				if (recordingGraphics != null) Graphics = recordingGraphics;
				failureStage = "beginDrawing";
				Graphics.BeginDrawing(Dt);
				failureStage = "draw";
				foreach (Control Ctl in Controls)
				{
					if (Ctl.Visible) Ctl.DrawControlAndChildren(this, Dt, Time);
				}

				foreach (DropDown dropdown in DropDown.OpenDropdowns.ToArray())
				{
					if (!dropdown.BelongsTo(this)) continue;
					if (!dropdown.IsHierarchyVisible()) { dropdown.Close(); continue; }
					using (Diagnostics.EnterRenderOwner("@overlay/dropdown/", dropdown.DiagnosticRuntimeId, dropdown))
						dropdown.DrawDropdownListOverlay(this);
				}

				if (_activeTooltip != null && _activeTooltip.IsShowing)
				{
					if (Settings.DebugLogTooltips)
						FishUIDebug.Log($"[Tooltip] Drawing tooltip in main Draw: '{_activeTooltip.Text}' IsShowing={_activeTooltip.IsShowing}");
					using (Diagnostics.EnterRenderOwner("@overlay/tooltip", _activeTooltip))
						_activeTooltip.DrawControl(this, Dt, Time);
				}

				using (Diagnostics.EnterRenderOwner("@overlay/virtualMouse"))
					VirtualMouse.Draw(Graphics);

				failureStage = "framebufferCapture";
				Diagnostics.BeforeEndDrawing(Graphics);
				failureStage = "endDrawing";
				Graphics.EndDrawing();
			}
			catch (Exception ex)
			{
				failure = ex;
			}
			finally
			{
				Graphics = originalGraphics;
				Diagnostics.EndDraw(failure, failureStage);
			}
			if (failure != null) ExceptionDispatchInfo.Capture(failure).Throw();
		}

		public void FocusControl(Control Ctrl)
		{
			Control previousFocus = InputActiveControl;
			if (previousFocus != null) Diagnostics.EnsureIdentity(previousFocus);
			if (Ctrl != null) Diagnostics.EnsureIdentity(Ctrl);

			if (previousFocus != null && previousFocus != Ctrl)
				previousFocus.HandleBlur();

			InputActiveControl = Ctrl;

			if (Ctrl != null)
				Ctrl.HandleFocus();

			if (Diagnostics.IsEventRecordingEnabled)
				Diagnostics.Record(FishUIDiagnosticEventCategory.Focus, FishUIDiagnosticEventType.FocusChanged, Ctrl,
					null, focus: new FishUIFocusEventData
					{
						FromControlId = previousFocus?.DiagnosticRuntimeId, ToControlId = Ctrl?.DiagnosticRuntimeId,
						Changed = !ReferenceEquals(previousFocus, Ctrl)
					});
		}

		/// <summary>
		/// Clears the current focus without focusing another control.
		/// </summary>
		public void ClearFocus()
		{
			Control previous = InputActiveControl;
			if (previous != null) Diagnostics.EnsureIdentity(previous);
			if (InputActiveControl != null)
			{
				InputActiveControl.HandleBlur();
				InputActiveControl = null;
			}
			if (Diagnostics.IsEventRecordingEnabled)
				Diagnostics.Record(FishUIDiagnosticEventCategory.Focus, FishUIDiagnosticEventType.FocusChanged, previous,
					"cleared", focus: new FishUIFocusEventData { FromControlId = previous?.DiagnosticRuntimeId, ToControlId = null, Changed = previous != null });
		}

		/// <summary>
		/// Gets all focusable controls in tab order.
		/// </summary>
		List<Control> GetFocusableControls()
		{
			List<Control> focusable = new List<Control>();
			CollectFocusableControls(Controls.ToArray(), focusable);
			return focusable.OrderBy(c => c.TabIndex).ToList();
		}

		void CollectFocusableControls(Control[] controls, List<Control> result)
		{
			foreach (Control c in controls)
			{
				if (c.Visible && !c.Disabled && c.Focusable)
					result.Add(c);

				CollectFocusableControls(c.GetAllChildren(), result);
			}
		}

		/// <summary>
		/// Focuses the next (or previous if reverse is true) focusable control.
		/// </summary>
		/// <param name="reverse">If true, focus the previous control (Shift+Tab behavior).</param>
		public void FocusNextControl(bool reverse = false)
		{
			List<Control> focusable = GetFocusableControls();

			if (focusable.Count == 0)
				return;

			int currentIndex = focusable.IndexOf(InputActiveControl);

			int nextIndex;
			if (currentIndex == -1)
			{
				// No control is focused, focus the first or last
				nextIndex = reverse ? focusable.Count - 1 : 0;
			}
			else
			{
				// Move to next or previous
				if (reverse)
					nextIndex = (currentIndex - 1 + focusable.Count) % focusable.Count;
				else
					nextIndex = (currentIndex + 1) % focusable.Count;
			}

			FocusControl(focusable[nextIndex]);
		}

		FishInputState InLast;

		/// <summary>
		/// Main update and render method. Call this every frame.
		/// </summary>
		/// <param name="Dt">Delta time since last frame in seconds.</param>
		/// <param name="Time">Total elapsed time in seconds.</param>
		public void Tick(float Dt, float Time)
		{
			TickUpdate(Dt, Time);
			TickDraw(Dt, Time);
		}

		/// <summary>
		/// Updates the input state for the current frame based on mouse, touch, and virtual mouse input, and processes
		/// control updates and tooltips accordingly.
		/// </summary>
		/// <remarks>This method processes both physical and virtual mouse input, updates the state of mouse buttons
		/// and modifier keys, and manages tooltip timing. It should be called once per frame to ensure consistent input
		/// handling and UI responsiveness.</remarks>
		/// <param name="Dt">The elapsed time, in seconds, since the last update. Used for time-based input processing and animations.</param>
		/// <param name="Time">The current time, in seconds, used for time-dependent calculations and animations.</param>
		public void TickUpdate(float Dt, float Time)
		{
			if (_disposed)
				throw new ObjectDisposedException(nameof(FishUI));

			_keyboardInputConsumedThisFrame = false;
			Vector2 MousePos = Input.GetMousePosition();
			bool MouseLeft = Input.IsMouseDown(FishMouseButton.Left);
			bool MouseRight = Input.IsMouseDown(FishMouseButton.Right);
			float MouseWheel = Input.GetMouseWheelMove();
			bool collectFrameState = Diagnostics.NeedsFrameState;
			FishUIPointerSnapshot backendPointer = collectFrameState ? new FishUIPointerSnapshot
			{
				Source = FishUIPointerSource.PhysicalMouse, PositionPixels = FishUIDebugPoint.From(MousePos),
				LeftDown = MouseLeft, RightDown = MouseRight, WheelDelta = MouseWheel
			} : null;

			// Update virtual mouse if enabled
			if (VirtualMouse.Enabled)
			{
				int screenWidth = Width > 0 ? Width : Graphics.GetWindowWidth();
				int screenHeight = Height > 0 ? Height : Graphics.GetWindowHeight();
				VirtualMouse.Update(Input, Dt, screenWidth, screenHeight);

				MousePos = VirtualMouse.Position;
				MouseLeft = VirtualMouse.IsLeftDown;
				MouseRight = VirtualMouse.IsRightDown;
			}

			FishInputState InState = new FishInputState();
			InState.MousePos = MousePos;
			InState.MouseLeft = MouseLeft;
			InState.MouseRight = MouseRight;
			InState.TouchPoints = Input.GetTouchPoints();

			if (VirtualMouse.Enabled)
			{
				InState.MouseLeftPressed = VirtualMouse.IsLeftPressed;
				InState.MouseLeftReleased = VirtualMouse.IsLeftReleased;
				InState.MouseRightPressed = VirtualMouse.IsRightPressed;
				InState.MouseRightReleased = VirtualMouse.IsRightReleased;
			}
			else
			{
				InState.MouseLeftPressed = Input.IsMousePressed(FishMouseButton.Left);
				InState.MouseLeftReleased = Input.IsMouseReleased(FishMouseButton.Left);
				InState.MouseRightPressed = Input.IsMousePressed(FishMouseButton.Right);
				InState.MouseRightReleased = Input.IsMouseReleased(FishMouseButton.Right);
			}

			InState.MouseDelta = MousePos - InLast.MousePos;
			InState.MouseWheelDelta = MouseWheel;

			// Modifier keys
			InState.ShiftDown = Input.IsKeyDown(FishKey.LeftShift) || Input.IsKeyDown(FishKey.RightShift);
			InState.CtrlDown = Input.IsKeyDown(FishKey.LeftControl) || Input.IsKeyDown(FishKey.RightControl);
			InState.AltDown = Input.IsKeyDown(FishKey.LeftAlt) || Input.IsKeyDown(FishKey.RightAlt);

			FishUIPointerSnapshot effectivePointer = collectFrameState ? new FishUIPointerSnapshot
			{
				Source = VirtualMouse.Enabled ? FishUIPointerSource.VirtualMouse : FishUIPointerSource.PhysicalMouse,
				PositionPixels = FishUIDebugPoint.From(MousePos), LeftDown = MouseLeft, RightDown = MouseRight,
				WheelDelta = MouseWheel
			} : null;
			Diagnostics.BeginFrame(Dt, Time, backendPointer, effectivePointer, collectFrameState ? new FishUIModifierSnapshot
			{
				Control = InState.CtrlDown, Shift = InState.ShiftDown, Alt = InState.AltDown
			} : null);

			OrderedControls = GetOrderedControls();

			Update(OrderedControls, InState, InLast, Dt, Time);

			// Update tooltip
			UpdateTooltip(Dt, InState.MousePos);

			// Clear virtual mouse one-frame states
			VirtualMouse.EndFrame();

			InLast = InState;
		}

		/// <summary>
		/// Draws all ordered controls, updating their visual state based on the elapsed and current time values. Should be called after TickUpdate
		/// </summary>
		/// <param name="Dt">The time elapsed since the last draw operation, in seconds. This value is typically used to update animations or
		/// time-dependent visual effects.</param>
		/// <param name="Time">The current time, in seconds, used to determine the state of controls during the draw operation.</param>
		public void TickDraw(float Dt, float Time)
		{
			Draw(OrderedControls, Dt, Time);
		}

		private void UpdateTooltip(float dt, Vector2 mousePos)
		{
			// Find the control under the mouse that has tooltip text
			Control controlWithTooltip = FindControlWithTooltip(HoveredControl);

			if (Settings.DebugLogTooltips)
			{
				if (HoveredControl != null && !string.IsNullOrEmpty(HoveredControl.TooltipText))
				{
					FishUIDebug.Log($"[Tooltip] Hovering control with tooltip: '{HoveredControl.TooltipText}'");
				}
			}

			if (controlWithTooltip != null && !string.IsNullOrEmpty(controlWithTooltip.TooltipText))
			{
				if (_tooltipTargetControl == controlWithTooltip)
				{
					// Still hovering the same control
					_tooltipHoverTime += dt;

					if (!_activeTooltip.IsShowing && _tooltipHoverTime >= TooltipShowDelay)
					{
						if (Settings.DebugLogTooltips)
						{
							FishUIDebug.Log($"[Tooltip] Showing tooltip: '{controlWithTooltip.TooltipText}' at {mousePos}");
						}
						_activeTooltip.Text = controlWithTooltip.TooltipText;
						_activeTooltip.Show(mousePos);
					}
				}
				else
				{
					// Started hovering a new control
					if (Settings.DebugLogTooltips)
					{
						FishUIDebug.Log($"[Tooltip] New control hovered, resetting timer");
					}
					_tooltipTargetControl = controlWithTooltip;
					_tooltipHoverTime = 0f;
					_activeTooltip.Hide();
				}
			}
			else
			{
				// Not hovering any control with tooltip
				if (_activeTooltip.IsShowing)
				{
					if (Settings.DebugLogTooltips)
					{
						FishUIDebug.Log($"[Tooltip] Hiding tooltip - no control with tooltip hovered");
					}
					_activeTooltip.Hide();
				}
				_tooltipTargetControl = null;
				_tooltipHoverTime = 0f;
			}

			// Update tooltip position if showing
			if (_activeTooltip.IsShowing)
			{
				_activeTooltip.UpdatePosition(this, mousePos);
			}
		}

		private Control FindControlWithTooltip(Control control)
		{
			if (control == null)
				return null;

			if (Settings.DebugLogTooltips)
			{
				FishUIDebug.Log($"[Tooltip] FindControlWithTooltip checking: {control.GetType().Name} ID={control.ID}, TooltipText='{control.TooltipText}'");
			}

			// Check this control first
			if (!string.IsNullOrEmpty(control.TooltipText))
			{
				if (Settings.DebugLogTooltips)
				{
					FishUIDebug.Log($"[Tooltip] Found control with tooltip: {control.GetType().Name}");
				}
				return control;
			}

			// Check parent chain
			Control parent = control.GetParent();
			while (parent != null)
			{
				if (!string.IsNullOrEmpty(parent.TooltipText))
					return parent;
				parent = parent.GetParent();
			}

			return null;
		}

		/// <summary>
		/// Called when the UI container is resized.
		/// Override to handle responsive layout updates.
		/// </summary>
		/// <param name="newWidth">New width of the UI container.</param>
		/// <param name="newHeight">New height of the UI container.</param>
		public void Resized(int newWidth, int newHeight)
		{
			Width = newWidth;
			Height = newHeight;
			Control[] controls = Controls.ToArray();
			for (int i = 0; i < controls.Length; i++) controls[i].ResizeSubtree(this, newWidth, newHeight);
		}

		/// <summary>
		/// Cancels pending diagnostic requests. Injected backend services remain owned by the caller.
		/// </summary>
		public void Dispose()
		{
			if (_disposed)
				return;
			_disposed = true;
			RemoveAllControls();
			KeyboardCaptureLease[] leases = _keyboardCaptureLeases.ToArray();
			_keyboardCaptureLeases.Clear();
			for (int i = 0; i < leases.Length; i++) leases[i].Invalidate();
			_keyboardInputConsumedThisFrame = false;
			Hotkeys.Clear();
			InputActiveControl = null;
			HoveredControl = null;
			LeftClickedControl = null;
			RightClickedControl = null;
			ModalControl = null;
			Diagnostics.Dispose();
		}
	}
}

