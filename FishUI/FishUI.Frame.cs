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
    public partial class FishUI
    {
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
            if (!float.IsFinite(Dt) || !float.IsFinite(Time))
                throw new ArgumentOutOfRangeException(nameof(Dt), "Frame values must be finite.");
            EnsureInitialized();
            InvalidateNonInteractiveState();
            int rootCount = Controls.Count;
            for (int i = 0; i < rootCount && i < Controls.Count; i++)
                Controls[i].EnsureInitializedSubtree(this);
            Animations.Update(Math.Max(0, Dt));
            for (int i = 0; i < rootCount && i < Controls.Count; i++)
                Controls[i].UpdateSubtree(this, Dt, Time);
            for (int i = 0; i < rootCount && i < Controls.Count; i++)
                Controls[i].PrepareLayoutSubtree(this);
            FreezeFrameHierarchy();
            _framePrepared = true;

            _keyboardInputConsumedThisFrame = false;
            Vector2 MousePos = Input.GetMousePosition();
            bool MouseLeft = Input.IsMouseDown(FishMouseButton.Left);
            bool MouseRight = Input.IsMouseDown(FishMouseButton.Right);
            float MouseWheel = Input.GetMouseWheelMove();
            Vector2 backendMousePosition = MousePos;
            bool backendMouseLeft = MouseLeft;
            bool backendMouseRight = MouseRight;
            float backendMouseWheel = MouseWheel;
            FishTouchPoint[] touchPoints = Input.GetTouchPoints() ?? Array.Empty<FishTouchPoint>();
            bool touchPressed = false;
            bool touchReleased = false;
            bool touchActive = false;
            bool releaseTouchAfterFrame = false;
            FishTouchPoint activeTouch = default;

            if (!VirtualMouse.Enabled)
            {
                if (!_activeTouchId.HasValue)
                {
                    for (int i = 0; i < touchPoints.Length; i++)
                    {
                        if (touchPoints[i].TouchType == FishTouchType.Release) continue;
                        _activeTouchId = touchPoints[i].Id;
                        activeTouch = touchPoints[i];
                        touchPressed = true;
                        touchActive = true;
                        break;
                    }
                }
                else
                {
                    for (int i = 0; i < touchPoints.Length; i++)
                    {
                        if (touchPoints[i].Id != _activeTouchId.Value) continue;
                        activeTouch = touchPoints[i];
                        touchReleased = activeTouch.TouchType == FishTouchType.Release;
                        touchActive = !touchReleased;
                        releaseTouchAfterFrame = touchReleased;
                        break;
                    }
                }
                if (touchActive || touchReleased)
                {
                    MousePos = activeTouch.Position;
                    _activeTouchPosition = MousePos;
                    MouseLeft = touchActive;
                    MouseRight = false;
                    MouseWheel = 0;
                }
                else if (_activeTouchId.HasValue)
                {
                    // Backends should emit releases, but synthesize one if an active ID disappears.
                    MousePos = _activeTouchPosition;
                    MouseLeft = false;
                    MouseRight = false;
                    MouseWheel = 0;
                    touchReleased = true;
                    releaseTouchAfterFrame = true;
                }
            }
            bool collectFrameState = Diagnostics.NeedsFrameState;
            FishUIPointerSnapshot backendPointer = collectFrameState ? new FishUIPointerSnapshot
            {
                Source = FishUIPointerSource.PhysicalMouse,
                PositionPixels = FishUIDebugPoint.From(backendMousePosition),
                LeftDown = backendMouseLeft,
                RightDown = backendMouseRight,
                WheelDelta = backendMouseWheel
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
            InState.TouchPoints = touchPoints;
            InState.PointerSource = VirtualMouse.Enabled ? FishUIPointerSource.VirtualMouse :
                (touchActive || touchReleased ? FishUIPointerSource.Touch : FishUIPointerSource.PhysicalMouse);

            if (VirtualMouse.Enabled)
            {
                InState.MouseLeftPressed = VirtualMouse.IsLeftPressed;
                InState.MouseLeftReleased = VirtualMouse.IsLeftReleased;
                InState.MouseRightPressed = VirtualMouse.IsRightPressed;
                InState.MouseRightReleased = VirtualMouse.IsRightReleased;
            }
            else if (touchActive || touchReleased)
            {
                InState.MouseLeftPressed = touchPressed;
                InState.MouseLeftReleased = touchReleased;
                InState.MouseRightPressed = false;
                InState.MouseRightReleased = false;
            }
            else
            {
                InState.MouseLeftPressed = Input.IsMousePressed(FishMouseButton.Left);
                InState.MouseLeftReleased = Input.IsMouseReleased(FishMouseButton.Left);
                InState.MouseRightPressed = Input.IsMousePressed(FishMouseButton.Right);
                InState.MouseRightReleased = Input.IsMouseReleased(FishMouseButton.Right);
            }

            InState.MouseDelta = touchPressed ? Vector2.Zero : MousePos - InLast.MousePos;
            InState.MouseWheelDelta = MouseWheel;

            // Modifier keys
            InState.ShiftDown = Input.IsKeyDown(FishKey.LeftShift) || Input.IsKeyDown(FishKey.RightShift);
            InState.CtrlDown = Input.IsKeyDown(FishKey.LeftControl) || Input.IsKeyDown(FishKey.RightControl);
            InState.AltDown = Input.IsKeyDown(FishKey.LeftAlt) || Input.IsKeyDown(FishKey.RightAlt);

            FishUIPointerSnapshot effectivePointer = collectFrameState ? new FishUIPointerSnapshot
            {
                Source = InState.PointerSource,
                PositionPixels = FishUIDebugPoint.From(MousePos),
                LeftDown = MouseLeft,
                RightDown = MouseRight,
                WheelDelta = MouseWheel
            } : null;
            Diagnostics.BeginFrame(Dt, Time, backendPointer, effectivePointer, collectFrameState ? new FishUIModifierSnapshot
            {
                Control = InState.CtrlDown,
                Shift = InState.ShiftDown,
                Alt = InState.AltDown
            } : null);

            Update(OrderedControls, InState, InLast, Dt, Time);
            for (int i = 0; i < OrderedControls.Length; i++)
                OrderedControls[i].PostInputUpdateSubtree(this, Dt, Time);
            for (int i = 0; i < OrderedControls.Length; i++)
                OrderedControls[i].PrepareLayoutSubtree(this);

            _frameOverlays.Clear();
            for (int i = 0; i < _openOverlays.Count; i++)
                _frameOverlays.Add(_openOverlays[i]);

            // Update tooltip
            UpdateTooltip(Dt, InState.MousePos);

            // Clear virtual mouse one-frame states
            VirtualMouse.EndFrame();
            if (releaseTouchAfterFrame)
                _activeTouchId = null;

            InLast = InState;
        }

        private void InvalidateNonInteractiveState()
        {
            if (InputActiveControl != null && !IsControlEffectivelyInteractive(InputActiveControl))
            {
                ReportDetachedInteractionTarget(InputActiveControl);
                ClearFocus();
            }
            if (ModalControl != null && !IsControlEffectivelyInteractive(ModalControl))
            {
                ReportDetachedInteractionTarget(ModalControl);
                ModalControl = null;
            }
            if (HoveredControl != null && !IsControlEffectivelyInteractive(HoveredControl))
            {
                ReportDetachedInteractionTarget(HoveredControl);
                HoveredControl.HandleMouseLeave(this, InLast);
                HoveredControl = null;
            }
            if (LeftClickedControl != null && !IsControlEffectivelyInteractive(LeftClickedControl))
            {
                ReportDetachedInteractionTarget(LeftClickedControl);
                LeftClickedControl = null;
            }
            if (RightClickedControl != null && !IsControlEffectivelyInteractive(RightClickedControl))
            {
                ReportDetachedInteractionTarget(RightClickedControl);
                RightClickedControl = null;
            }
            if (LeftClickedControl == null)
            {
                ActiveDragInteractionId = null;
                ActiveDiagnosticDragStarted = false;
            }
        }

        private void ReportDetachedInteractionTarget(Control control)
        {
            if (control?.AttachedFishUI != this)
                Diagnostics.ReportLiveWarning("DETACHED_INTERACTION_TARGET",
                    "FishUI cleared interaction state owned by a detached control.", control);
        }

        /// <summary>
        /// Draws all ordered controls, updating their visual state based on the elapsed and current time values. Should be called after TickUpdate
        /// </summary>
        /// <param name="Dt">The time elapsed since the last draw operation, in seconds. This value is typically used to update animations or
        /// time-dependent visual effects.</param>
        /// <param name="Time">The current time, in seconds, used to determine the state of controls during the draw operation.</param>
        public void TickDraw(float Dt, float Time)
        {
            if (!float.IsFinite(Dt) || !float.IsFinite(Time))
                throw new ArgumentOutOfRangeException(nameof(Dt), "Frame values must be finite.");
            EnsureInitialized();
            if (!_framePrepared)
                throw new InvalidOperationException("FishUI.TickUpdate() must prepare a frame before FishUI.TickDraw().");
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

    }
}
