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
    public enum FishUILifecycleState
    {
        Created,
        Initialized,
        Disposed
    }

    /// <summary>
    /// Main FishUI class that manages the UI system, controls, input handling, and rendering.
    /// </summary>
    public partial class FishUI : IDisposable
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
        public Control InputActiveControl { get; private set; }

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

        Control[] OrderedControls = Array.Empty<Control>();
        private int _orderedControlSignature;
        private bool _framePrepared;
        private readonly HashSet<KeyboardCaptureLease> _keyboardCaptureLeases = new HashSet<KeyboardCaptureLease>();
        private readonly List<Control> _openOverlays = new List<Control>();
        private readonly List<Control> _frameOverlays = new List<Control>();
        private bool _keyboardInputConsumedThisFrame;
        private bool _disposed;
        private FishUILifecycleState _lifecycleState;
        internal bool IsDisposingOrDisposed => _disposed;
        private int? _activeTouchId;
        private Vector2 _activeTouchPosition;
        private readonly HashSet<FishKey> _activeKeys = new HashSet<FishKey>();
        private readonly List<FishKey> _releasedKeys = new List<FishKey>();

        public FishUILifecycleState LifecycleState => _lifecycleState;
        public int MaximumKeyEventsPerFrame { get; set; } = 256;
        public int MaximumCharacterEventsPerFrame { get; set; } = 1024;

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
            if (Settings == null) throw new ArgumentNullException(nameof(Settings));
            if (Graphics == null) throw new ArgumentNullException(nameof(Graphics));
            if (Input == null) throw new ArgumentNullException(nameof(Input));
            if (Events == null) throw new ArgumentNullException(nameof(Events));
            Controls = new List<Control>();

            if (FS == null)
                FS = new DefaultFishUIFileSystem();

            this.Settings = Settings;
            this.Graphics = Graphics;
            this.Input = Input;
            this.Events = Events;
            this.FileSystem = FS;
            Diagnostics = new FishUIDiagnosticsSession(this);
            _lifecycleState = FishUILifecycleState.Created;

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
            ThrowIfDisposed();
            if (_lifecycleState == FishUILifecycleState.Initialized)
                return;
            Graphics.Init();
            Settings.Init(this);
            _lifecycleState = FishUILifecycleState.Initialized;
        }

        private void ThrowIfDisposed()
        {
            if (_lifecycleState == FishUILifecycleState.Disposed || _disposed)
                throw new ObjectDisposedException(nameof(FishUI));
        }

        private void EnsureInitialized()
        {
            ThrowIfDisposed();
            if (_lifecycleState != FishUILifecycleState.Initialized)
                throw new InvalidOperationException("FishUI.Init() must complete before frame processing.");
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
        public void SetModalControl(Control? control)
        {
            EnsureInitialized();
            if (control != null && (control.AttachedFishUI != this || !control.IsHierarchyVisible() || !control.IsHierarchyEnabled()))
                throw new InvalidOperationException("The modal control must be attached to this UI and effectively interactive.");
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

            if (oldUi != null)
            {
                oldUi.PrepareSubtreeDetach(C);
                C.DetachSubtree(oldUi);
            }
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
            if (C != null && Controls.Contains(C))
            {
                PrepareSubtreeDetach(C);
                Controls.Remove(C);
                if (C.AttachedFishUI == this) C.DetachSubtree(this);
                C._FishUI = null;
                Diagnostics.NotifyHierarchyChanged();
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
            Control[] result = Controls.ToArray();
            for (int i = 1; i < result.Length; i++)
            {
                Control value = result[i];
                int j = i - 1;
                while (j >= 0 && PaintsAfter(result[j], value))
                {
                    result[j + 1] = result[j];
                    j--;
                }
                result[j + 1] = value;
            }
            return result;
        }

        private void FreezeFrameHierarchy()
        {
            int signature = Controls.Count;
            for (int i = 0; i < Controls.Count; i++)
            {
                Control control = Controls[i];
                signature = HashCode.Combine(signature, control == null ? 0 : RuntimeHelpers.GetHashCode(control),
                    control?.ZDepth ?? 0, control?.AlwaysOnTop ?? false);
            }

            if (signature != _orderedControlSignature || OrderedControls.Length != Controls.Count)
            {
                OrderedControls = Controls.ToArray();
                for (int i = 1; i < OrderedControls.Length; i++)
                {
                    Control value = OrderedControls[i];
                    int j = i - 1;
                    while (j >= 0 && PaintsAfter(OrderedControls[j], value))
                    {
                        OrderedControls[j + 1] = OrderedControls[j];
                        j--;
                    }
                    OrderedControls[j + 1] = value;
                }
                _orderedControlSignature = signature;
            }

            for (int i = 0; i < OrderedControls.Length; i++)
                OrderedControls[i]?.FreezeFrameHierarchy();
        }

        private static bool PaintsAfter(Control left, Control right)
        {
            if (left.AlwaysOnTop != right.AlwaysOnTop) return left.AlwaysOnTop;
            return left.ZDepth > right.ZDepth;
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

        internal bool IsControlEffectivelyInteractive(Control control)
        {
            return control != null && control.AttachedFishUI == this && control.IsHierarchyVisible() &&
                control.IsHierarchyEnabled() && IsControlInputAllowed(control);
        }

        private static bool IsWithinSubtree(Control control, Control root)
        {
            return control != null && (ReferenceEquals(control, root) || control.IsDescendantOf(root));
        }

        internal void PrepareSubtreeDetach(Control root)
        {
            if (root == null)
                return;
            for (int i = _openOverlays.Count - 1; i >= 0; i--)
            {
                Control overlay = _openOverlays[i];
                if (!IsWithinSubtree(overlay, root)) continue;
                if (overlay is DropDown dropDown) dropDown.Close();
                else if (overlay is DatePicker datePicker) datePicker.Close();
                else _openOverlays.RemoveAt(i);
            }
            if (IsWithinSubtree(InputActiveControl, root))
                ClearFocus();
            if (IsWithinSubtree(HoveredControl, root))
            {
                HoveredControl.HandleMouseLeave(this, InLast);
                HoveredControl = null;
            }
            if (IsWithinSubtree(LeftClickedControl, root))
            {
                LeftClickedControl = null;
                ActiveDragInteractionId = null;
                ActiveDiagnosticDragStarted = false;
            }
            if (IsWithinSubtree(RightClickedControl, root))
                RightClickedControl = null;
            if (IsWithinSubtree(ModalControl, root))
                ModalControl = null;
            if (IsWithinSubtree(LastLeftClickControl, root))
                LastLeftClickControl = null;
            if (IsWithinSubtree(LastRightClickControl, root))
                LastRightClickControl = null;
            if (IsWithinSubtree(_tooltipTargetControl, root))
            {
                _tooltipTargetControl = null;
                _tooltipHoverTime = 0f;
                _activeTooltip.Hide();
            }

            StopSubtreeAnimations(root);
            KeyboardCaptureLease[] leases = _keyboardCaptureLeases.ToArray();
            for (int i = 0; i < leases.Length; i++)
            {
                if (leases[i].Owner is Control owner && IsWithinSubtree(owner, root))
                    leases[i].Dispose();
            }
        }

        private void StopSubtreeAnimations(Control root)
        {
            Animations.StopAnimationsFor(root);
            Control[] children = root.GetAllChildren(false);
            for (int i = 0; i < children.Length; i++)
                StopSubtreeAnimations(children[i]);
        }

        internal void RegisterOverlay(Control control)
        {
            if (control == null || control.AttachedFishUI != this)
                return;
            _openOverlays.Remove(control);
            _openOverlays.Add(control);
        }

        internal void UnregisterOverlay(Control control) => _openOverlays.Remove(control);

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
            for (int controlIndex = Controls.Length - 1; controlIndex >= 0; controlIndex--)
            {
                Control C = Controls[controlIndex];
                if (C == null)
                    continue;
                FishUIHitTestCandidate candidate = trace == null ? null : CreateHitTestCandidate(C, GlobalPos);
                if (candidate != null) trace.Trace.Candidates.Add(candidate);
                if (C.AttachedFishUI != this)
                {
                    if (candidate != null) candidate.RejectionReason = FishUIHitTestRejectionReason.Detached;
                    continue;
                }
                if (!C.Visible)
                {
                    if (candidate != null) candidate.RejectionReason = FishUIHitTestRejectionReason.Invisible;
                    continue;
                }
                if (!C.IsHierarchyEnabled())
                {
                    if (candidate != null) candidate.RejectionReason = FishUIHitTestRejectionReason.Disabled;
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
                Control[] children = C.FrameChildrenPaintOrder;
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
            if (!_framePrepared)
                FreezeFrameHierarchy();
            return PickControlCore(GlobalPos, null);
        }

        private Control PickControlCore(Vector2 GlobalPos, FishUIHitTestTraceBuilder trace)
        {
            Control openOverlay = PickOpenOverlay(GlobalPos);
            if (openOverlay != null)
            {
                if (trace != null)
                {
                    var candidate = CreateHitTestCandidate(openOverlay, GlobalPos); candidate.Accepted = true;
                    trace.Trace.Candidates.Add(candidate);
                }
                return openOverlay;
            }

            return PickControl(OrderedControls, GlobalPos, null, trace);
        }

        internal FishUIHitTestTrace ExplainHitTestInternal(Vector2 point)
        {
            if (!_framePrepared)
                FreezeFrameHierarchy();
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
                ControlId = control.DiagnosticRuntimeId,
                ControlPath = Diagnostics.PathFor(control),
                BoundsPixels = bounds,
                ExpectedClipPixels = clip,
                InsideBounds = control.IsPointInside(point),
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

        private Control PickOpenOverlay(Vector2 globalPosition)
        {
            for (int i = _openOverlays.Count - 1; i >= 0; i--)
            {
                Control overlay = _openOverlays[i];
                if (!IsControlEffectivelyInteractive(overlay))
                {
                    Diagnostics.ReportLiveWarning("OVERLAY_LEAK",
                        "An overlay registration outlived its interactive owner.", overlay);
                    if (overlay is DropDown dropDown) dropDown.Close();
                    else if (overlay is DatePicker datePicker) datePicker.Close();
                    else _openOverlays.RemoveAt(i);
                    continue;
                }
                if (overlay.IsPointInside(globalPosition))
                    return overlay;
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
            _openOverlays.Clear();
            Diagnostics.Dispose();
            _lifecycleState = FishUILifecycleState.Disposed;
        }
    }
}

