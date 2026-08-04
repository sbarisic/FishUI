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
        void UpdateSingleControl(Control Ctl, FishInputState InState, FishInputState InLast)
        {
            if (!Ctl.Visible)
                return;

            Ctl.IsMousePressed = LeftClickedControl == Ctl;
            Ctl.IsMouseInside = HoveredControl == Ctl;

            Control[] Children = Ctl.FrameChildren;

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
                if (proxy != null && proxy.Focusable && IsControlEffectivelyInteractive(proxy))
                    return proxy;

                if (control.Focusable && IsControlEffectivelyInteractive(control))
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
            if (InputActiveControl != null && !IsControlEffectivelyInteractive(InputActiveControl))
                ClearFocus();
            int maximumCharacters = Math.Max(1, MaximumCharacterEventsPerFrame);
            if (suppressTextInput)
            {
                int rejectedCharacter;
                int rejectedCount = 0;
                while (rejectedCount < maximumCharacters && (rejectedCharacter = Input.GetCharPressed()) != 0)
                {
                    rejectedCount++;
                    if (Diagnostics.IsEventRecordingEnabled)
                        Diagnostics.Record(FishUIDiagnosticEventCategory.TextInput, FishUIDiagnosticEventType.CharacterRejected,
                            InputActiveControl, "consumedByKeyOrHotkey", text: new FishUITextEventData
                            {
                                CharacterCount = 1,
                                LineCount = rejectedCharacter == '\n' ? 1 : 0,
                                Character = ((char)rejectedCharacter).ToString(),
                                CodePoint = rejectedCharacter,
                                UnicodeCategory = char.GetUnicodeCategory((char)rejectedCharacter).ToString()
                            });
                }
                if (rejectedCount == maximumCharacters)
                    Diagnostics.ReportLiveWarning("INPUT_CHARACTER_QUEUE_LIMIT_REACHED",
                        "The per-frame character event limit was reached.", InputActiveControl,
                        maximumCharacters.ToString(CultureInfo.InvariantCulture));
                return;
            }

            if (InputActiveControl != null)
            {
                if (Input.IsKeyPressed(FishKey.Backspace))
                    InputActiveControl.HandleTextInput(this, InState, '\b');

                if (Input.IsKeyPressed(FishKey.Enter) || Input.IsKeyPressed(FishKey.KpEnter))
                    InputActiveControl.HandleTextInput(this, InState, '\n');

                int InChr = 0;

                int characterCount = 0;
                while (characterCount < maximumCharacters && (InChr = Input.GetCharPressed()) != 0)
                {
                    characterCount++;
                    Control textTarget = InputActiveControl;
                    bool accepted = !(textTarget is IFishUITextInputFilter filter) ||
                        filter.ShouldAcceptTextInput(this, InState, (char)InChr);
                    if (!accepted)
                    {
                        if (Diagnostics.IsEventRecordingEnabled)
                            Diagnostics.Record(FishUIDiagnosticEventCategory.TextInput, FishUIDiagnosticEventType.CharacterRejected,
                                textTarget, "rejectedByControl", text: new FishUITextEventData
                                {
                                    CharacterCount = 1,
                                    LineCount = InChr == '\n' ? 1 : 0,
                                    Character = ((char)InChr).ToString(),
                                    CodePoint = InChr,
                                    UnicodeCategory = char.GetUnicodeCategory((char)InChr).ToString()
                                });
                        continue;
                    }

                    FishUIDiagnosticEvent textEvent = null;
                    if (Diagnostics.IsEventRecordingEnabled)
                        textEvent = Diagnostics.Record(FishUIDiagnosticEventCategory.TextInput, FishUIDiagnosticEventType.CharacterAccepted,
                            textTarget, null, text: new FishUITextEventData
                            {
                                CharacterCount = 1,
                                LineCount = InChr == '\n' ? 1 : 0,
                                Character = ((char)InChr).ToString(),
                                CodePoint = InChr,
                                UnicodeCategory = char.GetUnicodeCategory((char)InChr).ToString()
                            });
                    using (Diagnostics.EnterCause(textEvent?.Sequence))
                        textTarget.HandleTextInput(this, InState, (char)InChr);
                }
                if (characterCount == maximumCharacters)
                    Diagnostics.ReportLiveWarning("INPUT_CHARACTER_QUEUE_LIMIT_REACHED",
                        "The per-frame character event limit was reached.", InputActiveControl,
                        maximumCharacters.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                int discarded = 0;
                while (discarded < maximumCharacters && Input.GetCharPressed() != 0)
                    discarded++;
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

            bool suppressTextInput = false;
            int maximumKeys = Math.Max(1, MaximumKeyEventsPerFrame);
            int consumedKeys = 0;
            for (; consumedKeys < maximumKeys; consumedKeys++)
            {
                FishKey key = Input.GetKeyPressed();
                if (key == FishKey.None)
                    break;
                _activeKeys.Add(key);
                FishUIDiagnosticEvent keyEvent = null;
                if (recordDiagnostics)
                {
                    FishUIRawKeyMetadata metadata = null;
                    if (Input is IFishUIRawInputMetadataProvider metadataProvider) metadataProvider.TryGetKeyMetadata(key, out metadata);
                    keyEvent = Diagnostics.Record(FishUIDiagnosticEventCategory.Keyboard, FishUIDiagnosticEventType.KeyPressed, InputActiveControl,
                        key.ToString(), key: new FishUIKeyEventData
                        {
                            Key = key.ToString(),
                            BackendKeyCode = metadata?.BackendKeyCode,
                            Repeat = metadata?.Repeat ?? false,
                            Released = metadata?.Released ?? false,
                            Modifiers = new FishUIModifierSnapshot { Control = InState.CtrlDown, Shift = InState.ShiftDown, Alt = InState.AltDown }
                        });
                }

                bool hotkeyHandled = Hotkeys.ProcessKeyPress(key, Input, out FishUIHotkey matchedHotkey);
                if (hotkeyHandled)
                {
                    _keyboardInputConsumedThisFrame = true;
                    suppressTextInput |= matchedHotkey.ConsumesTextInput;
                    FishUIDiagnosticEvent hotkeyEvent = null;
                    if (recordDiagnostics || Diagnostics.IsEventRecordingEnabled)
                        hotkeyEvent = Diagnostics.Record(FishUIDiagnosticEventCategory.Keyboard, FishUIDiagnosticEventType.HotkeyHandled, InputActiveControl,
                            matchedHotkey?.ID, key: new FishUIKeyEventData { Key = key.ToString(), HotkeyId = matchedHotkey?.ID, Consumed = true },
                            bypassFilter: Diagnostics.IsCaptureHotkey(matchedHotkey));
                    Diagnostics.CompleteHotkeyTrigger(matchedHotkey, hotkeyEvent);
                    continue;
                }

                bool previewHandled = InputActiveControl != null && IsControlEffectivelyInteractive(InputActiveControl) &&
                    InputActiveControl.PreviewKeyPress(this, InState, key);
                if (previewHandled)
                {
                    _keyboardInputConsumedThisFrame = true;
                    suppressTextInput = true;
                }
                if (!previewHandled && key == FishKey.Tab)
                {
                    bool shiftHeld = Input.IsKeyDown(FishKey.LeftShift) || Input.IsKeyDown(FishKey.RightShift);
                    FocusNextControl(shiftHeld);
                    if (recordDiagnostics)
                        Diagnostics.Record(FishUIDiagnosticEventCategory.Keyboard, FishUIDiagnosticEventType.TabNavigation, InputActiveControl, shiftHeld ? "previous" : "next");
                }
                else if (!previewHandled && InputActiveControl != null && IsControlEffectivelyInteractive(InputActiveControl))
                {
                    using (Diagnostics.EnterCause(keyEvent?.Sequence))
                        InputActiveControl.HandleKeyPressed(this, InState, key);
                }
            }
            if (consumedKeys == maximumKeys)
                Diagnostics.ReportLiveWarning("INPUT_KEY_QUEUE_LIMIT_REACHED",
                    "The per-frame key event limit was reached.", InputActiveControl, maximumKeys.ToString(CultureInfo.InvariantCulture));

            _releasedKeys.Clear();
            foreach (FishKey activeKey in _activeKeys)
            {
                if (Input.IsKeyReleased(activeKey) || Input.IsKeyUp(activeKey))
                {
                    if (InputActiveControl != null && IsControlEffectivelyInteractive(InputActiveControl))
                        InputActiveControl.HandleKeyReleased(this, InState, activeKey);
                    _releasedKeys.Add(activeKey);
                }
                else if (InputActiveControl != null && IsControlEffectivelyInteractive(InputActiveControl))
                {
                    InputActiveControl.HandleKeyHeld(this, InState, activeKey);
                }
            }
            for (int i = 0; i < _releasedKeys.Count; i++)
                _activeKeys.Remove(_releasedKeys[i]);

            for (int i = 0; i < Controls.Length; i++)
                UpdateSingleControl(Controls[i], InState, InLast);

            CheckTextInput(InState, suppressTextInput);
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

    }
}
