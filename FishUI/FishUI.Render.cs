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
        void Draw(Control[] Controls, float Dt, float Time)
        {
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

                for (int i = 0; i < _frameOverlays.Count; i++)
                {
                    Control overlay = _frameOverlays[i];
                    if (!IsControlEffectivelyInteractive(overlay))
                    {
                        Diagnostics.ReportLiveWarning("OVERLAY_LEAK",
                            "An overlay registration outlived its interactive owner.", overlay);
                        if (overlay is DropDown hiddenDropDown) hiddenDropDown.Close();
                        else if (overlay is DatePicker hiddenDatePicker) hiddenDatePicker.Close();
                        continue;
                    }
                    if (overlay is DropDown dropdown)
                    {
                        using (Diagnostics.EnterRenderOwner("@overlay/dropdown/", dropdown.DiagnosticRuntimeId, dropdown))
                            dropdown.DrawDropdownListOverlay(this);
                    }
                    else if (overlay is DatePicker datePicker)
                    {
                        using (Diagnostics.EnterRenderOwner("@overlay/datePicker/", datePicker.DiagnosticRuntimeId, datePicker))
                            datePicker.DrawCalendarPopupOverlay(this);
                    }
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
                Diagnostics.AfterAllDrawingBeforeGraphicsEnd(Graphics);
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

    }
}
