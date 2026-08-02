using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
                const string message = "Framebuffer capture is disabled by the session privacy policy.";
                batch.ScreenshotArtifact = Artifact(FishUIDiagnosticArtifactStatus.BlockedByPrivacy, message: message);
                batch.OverlayArtifact = Artifact(FishUIDiagnosticArtifactStatus.BlockedByPrivacy, message: message);
                return;
            }
            IFishUIFramebufferProvider provider = graphics as IFishUIFramebufferProvider;
            if (provider == null || graphics is RecordingFishUIGfx recording && !recording.HasFramebufferProvider)
            {
                batch.ScreenshotArtifact = Artifact(FishUIDiagnosticArtifactStatus.Unsupported);
                batch.OverlayArtifact = Artifact(FishUIDiagnosticArtifactStatus.Unsupported);
                return;
            }

            batch.FramebufferAttempted = true;
            try
            {
                if (!provider.TryCaptureFramebuffer(out FishUIFramebuffer framebuffer) || framebuffer == null)
                {
                    batch.ScreenshotArtifact = Artifact(FishUIDiagnosticArtifactStatus.Unavailable, "framebufferCapture");
                    batch.OverlayArtifact = Artifact(FishUIDiagnosticArtifactStatus.Unavailable, "framebufferCapture");
                    return;
                }
                batch.Framebuffer = framebuffer;
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
                batch.ScreenshotArtifact = Artifact(FishUIDiagnosticArtifactStatus.Failed, "framebufferCapture", ex.Message);
                batch.OverlayArtifact = Artifact(FishUIDiagnosticArtifactStatus.Unavailable, "framebufferCapture", ex.Message);
                RecordArtifactFailure(batch, "framebufferCapture", ex);
            }
        }

        private void QueueArtifactJob(CaptureBatch batch, List<ProjectedCapture> projected)
        {
            Task job;
            lock (_gate)
            {
                Task predecessor = _artifactWorkerTail;
                job = predecessor.ContinueWith(_ => ProcessArtifactJobAsync(batch, projected),
                    CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default).Unwrap();
                _artifactWorkerTail = job;
                _pendingExports.Add(job);
            }
            _ = job.ContinueWith(completed =>
            {
                lock (_gate)
                {
                    _pendingExports.Remove(completed);
                    TryDisposeLifetimeTokenSource();
                }
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        private async Task ProcessArtifactJobAsync(CaptureBatch batch, List<ProjectedCapture> projected)
        {
            try
            {
                GenerateArtifacts(batch);
                foreach (ProjectedCapture completion in projected)
                {
                    CaptureRequest request = completion.Request;
                    FishUIDebugSnapshot snapshot = completion.Snapshot;
                    ApplyArtifacts(batch, request, snapshot);

                    if (Interlocked.CompareExchange(ref request.CompletionState, 2, 0) != 0) continue;
                    LastCapture = snapshot;
                    request.Registration.Dispose();
                    RaiseCaptureCompleted(snapshot);
                    request.Completion.TrySetResult(snapshot);

                    Func<FishUIDebugSnapshot, CancellationToken, Task> exporter = AutoExportAsync;
                    if (exporter != null)
                    {
                        try
                        {
                            await exporter(snapshot, CancellationToken.None).ConfigureAwait(false);
                            RaiseExport(ExportCompleted, snapshot, null);
                        }
                        catch (Exception ex)
                        {
                            FishUIDebug.Log($"[Diagnostics] UI {snapshot.UiSessionId:N}, capture {snapshot.CaptureId}, request {snapshot.RequestId} export failed: {ex}");
                            RaiseExport(ExportFailed, snapshot, ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FishUIDebug.Log($"[Diagnostics] UI {UiSessionId:N}, capture {batch.CaptureId} artifact processing failed: {ex}");
                foreach (ProjectedCapture completion in projected)
                    if (Interlocked.CompareExchange(ref completion.Request.CompletionState, 2, 0) == 0)
                        completion.Request.Completion.TrySetException(ex);
            }
            finally
            {
                batch.Screenshot = null;
                batch.Overlay = null;
                DisposeFramebuffer(batch);
                ReleaseArtifactJobSlot();
            }
        }

        private void GenerateArtifacts(CaptureBatch batch)
        {
            if (batch.Framebuffer == null) return;
            byte[] rgba;
            try
            {
                rgba = FishUIDebugImage.Normalize(batch.Framebuffer, MaximumFramebufferWidth,
                    MaximumFramebufferHeight, MaximumFramebufferBytes);
            }
            catch (Exception ex)
            {
                batch.ScreenshotArtifact = Artifact(FishUIDiagnosticArtifactStatus.Failed, "framebufferValidation", ex.Message);
                batch.OverlayArtifact = Artifact(FishUIDiagnosticArtifactStatus.Unavailable, "framebufferValidation", ex.Message);
                return;
            }

            try
            {
                batch.Screenshot = FishUIDebugImage.EncodePng(batch.Framebuffer.Width, batch.Framebuffer.Height, rgba);
                batch.ScreenshotArtifact = Artifact(FishUIDiagnosticArtifactStatus.Available);
            }
            catch (Exception ex)
            {
                batch.ScreenshotArtifact = Artifact(FishUIDiagnosticArtifactStatus.Failed, "screenshotEncoding", ex.Message);
            }

            if (!batch.Superset.IncludeAnnotatedOverlay)
            {
                batch.OverlayArtifact = Artifact(FishUIDiagnosticArtifactStatus.Excluded);
                return;
            }
            try
            {
                FishUIDebugImage.DrawOverlay(rgba, batch.Framebuffer.Width, batch.Framebuffer.Height,
                    batch.CoordinateWidthPixels, batch.CoordinateHeightPixels, batch.Final.Values, batch.Warnings);
            }
            catch (Exception ex)
            {
                batch.OverlayArtifact = Artifact(FishUIDiagnosticArtifactStatus.Failed, "overlayDrawing", ex.Message);
                return;
            }
            try
            {
                batch.Overlay = FishUIDebugImage.EncodePng(batch.Framebuffer.Width, batch.Framebuffer.Height, rgba);
                batch.OverlayArtifact = Artifact(FishUIDiagnosticArtifactStatus.Available);
            }
            catch (Exception ex)
            {
                batch.OverlayArtifact = Artifact(FishUIDiagnosticArtifactStatus.Failed, "overlayEncoding", ex.Message);
            }
        }

        private void ApplyArtifacts(CaptureBatch batch, CaptureRequest request, FishUIDebugSnapshot snapshot)
        {
            FishUIDebugSnapshotOptions options = request.Options;
            snapshot.Artifacts["screenshot"] = ArtifactForRequest(batch.ScreenshotArtifact, options.IncludeScreenshot, options);
            snapshot.Artifacts["overlay"] = ArtifactForRequest(batch.OverlayArtifact, options.IncludeAnnotatedOverlay, options);
            if (options.IncludeScreenshot && batch.ScreenshotArtifact?.Status == FishUIDiagnosticArtifactStatus.Available)
                snapshot.ScreenshotPng = (byte[])batch.Screenshot.Clone();
            if (options.IncludeAnnotatedOverlay && batch.OverlayArtifact?.Status == FishUIDiagnosticArtifactStatus.Available)
                snapshot.OverlayPng = (byte[])batch.Overlay.Clone();
            if (options.IncludeScreenshot && batch.ScreenshotArtifact?.Status == FishUIDiagnosticArtifactStatus.Failed ||
                options.IncludeAnnotatedOverlay && batch.OverlayArtifact?.Status == FishUIDiagnosticArtifactStatus.Failed)
                snapshot.CaptureStatus = FishUIDebugCaptureStatus.Partial;
        }

        private void DisposeFramebuffer(CaptureBatch batch)
        {
            FishUIFramebuffer framebuffer = batch.Framebuffer;
            batch.Framebuffer = null;
            if (framebuffer == null) return;
            try { framebuffer.Dispose(); }
            catch (Exception ex) { FishUIDebug.Log($"[Diagnostics] Framebuffer release failed: {ex}"); }
        }

        private void ReleaseArtifactJobSlot()
        {
            lock (_gate) _artifactJobCount = Math.Max(0, _artifactJobCount - 1);
            _clearTemporaryHistoryWhenIdle = true;
            UpdateEventRecorderState();
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
            RecordArtifactFailure(batch, stage, failure);
        }

        private static void CollectRenderWarnings(CaptureBatch batch)
        {
            if (batch.RenderWarningsCollected || batch.RenderRecorder == null) return;
            batch.RenderRecorder.Complete();
            batch.Warnings.AddRange(batch.RenderRecorder.Warnings);
            batch.RenderWarningsCollected = true;
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
            catch (Exception ex) { FishUIDebug.Log($"[Diagnostics] CaptureCompleted handler failed: {ex}"); }
        }

        private void RaiseExport(EventHandler<FishUIDebugExportEventArgs> handler, FishUIDebugSnapshot snapshot, Exception failure)
        {
            try { handler?.Invoke(this, new FishUIDebugExportEventArgs(snapshot, failure)); }
            catch (Exception ex) { FishUIDebug.Log($"[Diagnostics] Export event handler failed: {ex}"); }
        }
    }
}
