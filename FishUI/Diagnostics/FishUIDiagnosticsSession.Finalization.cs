using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FishUI.Controls;

namespace FishUI
{
    public sealed partial class FishUIDiagnosticsSession
    {
        private void CaptureFramebuffer(IFishUIGfx graphics, CaptureBatch batch)
        {
            bool requested = batch.Superset.IncludeScreenshot || batch.Superset.IncludeAnnotatedOverlay;
            if (!requested)
            {
                batch.ScreenshotArtifact = Artifact(FishUIDiagnosticArtifactStatus.Excluded);
                batch.OverlayArtifact = Artifact(FishUIDiagnosticArtifactStatus.Excluded);
                return;
            }
            if (!PrivacyPolicy.EffectiveAllowFramebufferCapture)
            {
                batch.ScreenshotArtifact = Artifact(FishUIDiagnosticArtifactStatus.BlockedByPrivacy, message: "Framebuffer capture is disabled by the session privacy policy.");
                batch.OverlayArtifact = Artifact(FishUIDiagnosticArtifactStatus.BlockedByPrivacy, message: "Framebuffer capture is disabled by the session privacy policy.");
                return;
            }
            IFishUIFramebufferProvider provider = graphics as IFishUIFramebufferProvider;
            if (provider == null || (graphics is RecordingFishUIGfx recording && !recording.HasFramebufferProvider))
            {
                batch.ScreenshotArtifact = Artifact(FishUIDiagnosticArtifactStatus.Unsupported);
                batch.OverlayArtifact = Artifact(FishUIDiagnosticArtifactStatus.Unsupported);
                return;
            }
            batch.FramebufferAttempted = true;
            FishUIFramebuffer framebuffer = null;
            try
            {
                bool captured;
                try
                {
                    captured = provider.TryCaptureFramebuffer(out framebuffer);
                }
                catch (Exception ex)
                {
                    batch.ScreenshotArtifact = Artifact(FishUIDiagnosticArtifactStatus.Failed, "framebufferCapture", ex.Message);
                    batch.OverlayArtifact = Artifact(FishUIDiagnosticArtifactStatus.Unavailable, "framebufferCapture", ex.Message);
                    RecordArtifactFailure(batch, "framebufferCapture", ex);
                    return;
                }
                if (!captured || framebuffer == null)
                {
                    batch.ScreenshotArtifact = Artifact(FishUIDiagnosticArtifactStatus.Unavailable, "framebufferCapture");
                    batch.OverlayArtifact = Artifact(FishUIDiagnosticArtifactStatus.Unavailable, "framebufferCapture");
                    return;
                }

                byte[] rgba;
                try
                {
                    rgba = FishUIDebugImage.Normalize(framebuffer, MaximumFramebufferWidth, MaximumFramebufferHeight, MaximumFramebufferBytes);
                    batch.FramebufferWidthPixels = framebuffer.Width;
                    batch.FramebufferHeightPixels = framebuffer.Height;
                    if (batch.CoordinateWidthPixels > 0 && batch.CoordinateHeightPixels > 0)
                    {
                        batch.FramebufferScaleX = framebuffer.Width / (float)batch.CoordinateWidthPixels;
                        batch.FramebufferScaleY = framebuffer.Height / (float)batch.CoordinateHeightPixels;
                    }
                }
                catch (Exception ex)
                {
                    batch.ScreenshotArtifact = Artifact(FishUIDiagnosticArtifactStatus.Failed, "framebufferValidation", ex.Message);
                    batch.OverlayArtifact = Artifact(FishUIDiagnosticArtifactStatus.Unavailable, "framebufferValidation", ex.Message);
                    RecordArtifactFailure(batch, "framebufferValidation", ex);
                    return;
                }

                try
                {
                    batch.Screenshot = FishUIDebugImage.EncodePng(framebuffer.Width, framebuffer.Height, rgba);
                    batch.ScreenshotArtifact = Artifact(FishUIDiagnosticArtifactStatus.Available);
                }
                catch (Exception ex)
                {
                    batch.ScreenshotArtifact = Artifact(FishUIDiagnosticArtifactStatus.Failed, "screenshotEncoding", ex.Message);
                    RecordArtifactFailure(batch, "screenshotEncoding", ex);
                }

                if (!batch.Superset.IncludeAnnotatedOverlay)
                {
                    batch.OverlayArtifact = Artifact(FishUIDiagnosticArtifactStatus.Excluded);
                    return;
                }

                byte[] annotated;
                try
                {
                    annotated = (byte[])rgba.Clone();
                    FishUIDebugImage.DrawOverlay(annotated, framebuffer.Width, framebuffer.Height,
                        batch.CoordinateWidthPixels, batch.CoordinateHeightPixels, batch.Final.Values, batch.Warnings);
                }
                catch (Exception ex)
                {
                    batch.OverlayArtifact = Artifact(FishUIDiagnosticArtifactStatus.Failed, "overlayDrawing", ex.Message);
                    RecordArtifactFailure(batch, "overlayDrawing", ex);
                    return;
                }
                try
                {
                    batch.Overlay = FishUIDebugImage.EncodePng(framebuffer.Width, framebuffer.Height, annotated);
                    batch.OverlayArtifact = Artifact(FishUIDiagnosticArtifactStatus.Available);
                }
                catch (Exception ex)
                {
                    batch.OverlayArtifact = Artifact(FishUIDiagnosticArtifactStatus.Failed, "overlayEncoding", ex.Message);
                    RecordArtifactFailure(batch, "overlayEncoding", ex);
                }
            }
            finally
            {
                if (framebuffer != null)
                {
                    try { framebuffer.Dispose(); }
                    catch (Exception ex) { MarkDiagnosticFailure(batch, "framebufferDispose", ex); }
                }
            }
        }

        private void RecordArtifactFailure(CaptureBatch batch, string stage, Exception failure)
        {
            Record(FishUIDiagnosticEventCategory.Capture, FishUIDiagnosticEventType.CaptureFailure, null,
                $"capture={batch.CaptureId};stage={stage};exception={failure.GetType().Name}",
                bypassFilter: true, sensitiveDetail: failure.Message);
        }

        private void MarkDiagnosticFailure(CaptureBatch batch, string stage, Exception failure)
        {
            if (batch.DiagnosticFailure == null)
            {
                batch.DiagnosticFailure = failure;
                batch.DiagnosticFailureStage = stage;
            }
            batch.Warnings.Add(new FishUIDiagnosticWarning
            {
                Severity = FishUIDiagnosticSeverity.Error,
                Code = "CAPTURE_STAGE_FAILED",
                Message = stage,
                SensitiveDetail = CollectPersistentText(failure.Message),
                UiSessionId = UiSessionId
            });
            Record(FishUIDiagnosticEventCategory.Capture, FishUIDiagnosticEventType.CaptureFailure, null,
                $"capture={batch.CaptureId};stage={stage};exception={failure.GetType().Name}",
                sensitiveDetail: failure.Message);
        }

        private static void CollectRenderWarnings(CaptureBatch batch)
        {
            if (batch.RenderWarningsCollected || batch.RenderRecorder == null) return;
            batch.RenderRecorder.Complete();
            batch.Warnings.AddRange(batch.RenderRecorder.Warnings);
            batch.RenderWarningsCollected = true;
        }

        private ExportWork RegisterAutoExport(FishUIDebugSnapshot snapshot)
        {
            Func<FishUIDebugSnapshot, CancellationToken, Task> exporter = AutoExportAsync;
            if (exporter == null) return null;
            var start = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var published = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            Task task;
            lock (_gate)
            {
                if (_disposed || _lifetimeCtsDisposed) return null;
                task = RunAutoExportAsync(start.Task, published.Task, exporter, snapshot, _lifetimeCts.Token);
                _pendingExports.Add(task);
            }
            _ = task.ContinueWith(completed =>
            {
                lock (_gate)
                {
                    _pendingExports.Remove(completed);
                    TryDisposeLifetimeTokenSource();
                }
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            start.TrySetResult(true);
            return new ExportWork { CapturePublished = published };
        }

        private async Task RunAutoExportAsync(
            Task start,
            Task capturePublished,
            Func<FishUIDebugSnapshot, CancellationToken, Task> exporter,
            FishUIDebugSnapshot snapshot, CancellationToken token)
        {
            Exception failure = null;
            try
            {
                await start.ConfigureAwait(false);
                await exporter(snapshot, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                failure = ex;
            }
            await capturePublished.ConfigureAwait(false);
            if (failure == null)
            {
                RaiseExport(ExportCompleted, snapshot, null);
                return;
            }
            FishUIDebug.Log($"[Diagnostics] UI {snapshot.UiSessionId:N}, capture {snapshot.CaptureId}, request {snapshot.RequestId} export failed: {failure}");
            RaiseExport(ExportFailed, snapshot, failure);
        }

        public async Task WaitForPendingExportsAsync()
        {
            while (true)
            {
                Task[] pending;
                lock (_gate)
                {
                    if (_pendingExports.Count == 0)
                    {
                        TryDisposeLifetimeTokenSource();
                        return;
                    }
                    pending = _pendingExports.ToArray();
                }
                await Task.WhenAll(pending).ConfigureAwait(false);
            }
        }

        private void TryDisposeLifetimeTokenSource()
        {
            if (!_disposed || _pendingExports.Count != 0 || _lifetimeCtsDisposed) return;
            _lifetimeCtsDisposed = true;
            _lifetimeCts.Dispose();
        }

        private void RaiseCaptureCompleted(FishUIDebugSnapshot snapshot)
        {
            try { CaptureCompleted?.Invoke(this, new FishUICaptureCompletedEventArgs(snapshot)); }
            catch (Exception ex)
            {
                FishUIDebug.Log($"[Diagnostics] UI {snapshot.UiSessionId:N}, capture {snapshot.CaptureId}, request {snapshot.RequestId} CaptureCompleted handler failed: {ex}");
            }
        }

        private void RaiseExport(EventHandler<FishUIDebugExportEventArgs> handler, FishUIDebugSnapshot snapshot, Exception failure)
        {
            try { handler?.Invoke(this, new FishUIDebugExportEventArgs(snapshot, failure)); }
            catch (Exception ex)
            {
                FishUIDebug.Log($"[Diagnostics] UI {snapshot.UiSessionId:N}, capture {snapshot.CaptureId}, request {snapshot.RequestId} export event handler failed: {ex}");
            }
        }

    }
}
