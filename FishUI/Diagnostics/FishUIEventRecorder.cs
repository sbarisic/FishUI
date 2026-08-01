using System;
using System.Collections.Generic;

namespace FishUI
{
    public sealed class FishUIDebugPrivacyPolicy
    {
        private readonly Action _strengthened;
        private bool _requestedRedactText = true;
        private bool _effectiveRedactText = true;
        private bool _requestedRedactValues;
        private bool _effectiveRedactValues;
        private bool _requestedAllowFramebuffer;
        private bool _effectiveAllowFramebuffer;

        internal FishUIDebugPrivacyPolicy(Action strengthened) { _strengthened = strengthened; }

        public bool RedactText
        {
            get => _requestedRedactText;
            set
            {
                _requestedRedactText = value;
                if (value && !_effectiveRedactText) { _effectiveRedactText = true; _strengthened?.Invoke(); }
            }
        }

        public bool RedactValues
        {
            get => _requestedRedactValues;
            set
            {
                _requestedRedactValues = value;
                if (value && !_effectiveRedactValues) { _effectiveRedactValues = true; _strengthened?.Invoke(); }
            }
        }

        public bool AllowFramebufferCapture
        {
            get => _requestedAllowFramebuffer;
            set
            {
                _requestedAllowFramebuffer = value;
                if (!value && _effectiveAllowFramebuffer) { _effectiveAllowFramebuffer = false; _strengthened?.Invoke(); }
            }
        }

        public bool IncludeExceptionStackTrace { get; set; }
        internal bool EffectiveRedactText => _effectiveRedactText;
        internal bool EffectiveRedactValues => _effectiveRedactValues;
        internal bool EffectiveAllowFramebufferCapture => _effectiveAllowFramebuffer;

        internal void CommitAfterReset()
        {
            _effectiveRedactText = _requestedRedactText;
            _effectiveRedactValues = _requestedRedactValues;
            _effectiveAllowFramebuffer = _requestedAllowFramebuffer;
        }
    }

    public sealed class FishUIEventRecorder
    {
        private readonly object _gate = new object();
        private FishUIDiagnosticEvent[] _buffer;
        private int _start;
        private int _count;
        private long _discarded;
        private long _ageDiscarded;
        private long _capacityDiscarded;
        private double _capacityDiscardedThroughTimeSeconds = double.NegativeInfinity;
        private long _nextSequence;
        private double _lastTime;
        private TimeSpan _retentionDuration = TimeSpan.FromSeconds(10);

        public FishUIEventRecorderOptions Options { get; } = new FishUIEventRecorderOptions();
        public int Count { get { lock (_gate) return _count; } }
        public long DiscardedOldestCount { get { lock (_gate) return _discarded; } }
        internal long CapacityDiscardedTotal { get { lock (_gate) return _capacityDiscarded; } }
        internal double CapacityDiscardedThroughTimeSeconds { get { lock (_gate) return _capacityDiscardedThroughTimeSeconds; } }
        public long LatestSequence { get { lock (_gate) return _nextSequence; } }

        internal FishUIDiagnosticEvent Add(FishUIDiagnosticEvent record, bool bypassFilter = false)
        {
            if (!Options.Enabled)
                return null;
            lock (_gate)
            {
                EnsureBuffer();
                record.Sequence = ++_nextSequence;
                record.DeltaSincePreviousEventMs = _lastTime == 0 ? 0 : Math.Max(0, (record.TimeSeconds - _lastTime) * 1000.0);
                _lastTime = record.TimeSeconds;
                if (!bypassFilter && Options.EventFilter != null && !Options.EventFilter(record))
                    return null;
                EvictExpired(record.TimeSeconds);
                if (TryCoalesceMotion(record))
                    return GetLast();

                if (_count == _buffer.Length)
                {
                    RecordCapacityDiscard(_buffer[_start]);
                    _buffer[_start] = record;
                    _start = (_start + 1) % _buffer.Length;
                }
                else
                {
                    _buffer[(_start + _count) % _buffer.Length] = record;
                    _count++;
                }
                return record;
            }
        }

        internal void SetRetentionDuration(TimeSpan duration)
        {
            lock (_gate) _retentionDuration = duration < TimeSpan.Zero ? TimeSpan.Zero : duration;
        }

        internal void SetCapacity(int capacity)
        {
            capacity = Math.Max(1, capacity);
            lock (_gate)
            {
                Options.Capacity = capacity;
                if (_buffer == null || _buffer.Length == capacity) return;
                var replacement = new FishUIDiagnosticEvent[capacity];
                int retain = Math.Min(_count, capacity);
                int removed = _count - retain;
                for (int i = 0; i < removed; i++)
                    RecordCapacityDiscard(_buffer[(_start + i) % _buffer.Length]);
                for (int i = 0; i < retain; i++)
                    replacement[i] = _buffer[(_start + removed + i) % _buffer.Length];
                _buffer = replacement;
                _start = 0;
                _count = retain;
            }
        }

        private void EvictExpired(double nowSeconds)
        {
            double cutoff = nowSeconds - _retentionDuration.TotalSeconds;
            while (_count > 0)
            {
                FishUIDiagnosticEvent oldest = _buffer[_start];
                if (oldest.TimeSeconds >= cutoff) break;
                _buffer[_start] = null;
                _start = (_start + 1) % _buffer.Length;
                _count--;
                _discarded++;
                _ageDiscarded++;
            }
        }

        private void RecordCapacityDiscard(FishUIDiagnosticEvent record)
        {
            _discarded++;
            _capacityDiscarded++;
            if (record != null)
                _capacityDiscardedThroughTimeSeconds = Math.Max(_capacityDiscardedThroughTimeSeconds, record.TimeSeconds);
        }

        private bool TryCoalesceMotion(FishUIDiagnosticEvent record)
        {
            if (record.Type != FishUIDiagnosticEventType.MouseMoved || _count == 0) return false;
            FishUIDiagnosticEvent previous = GetLast();
            if (previous.Type != FishUIDiagnosticEventType.MouseMoved || previous.Frame != record.Frame ||
                previous.ControlId != record.ControlId || previous.InteractionId != record.InteractionId ||
                previous.Pointer?.Button != record.Pointer?.Button ||
                previous.Pointer?.EffectivePointer?.Source != record.Pointer?.EffectivePointer?.Source)
                return false;
            if (record.Pointer != null)
            {
                previous.Pointer.PositionPixels = record.Pointer.PositionPixels;
                previous.Pointer.EffectivePointer = record.Pointer.EffectivePointer;
                previous.Pointer.SampleCount += Math.Max(1, record.Pointer.SampleCount);
                if (previous.Pointer.DeltaPixels != null && record.Pointer.DeltaPixels != null)
                    previous.Pointer.DeltaPixels = new FishUIDebugPoint(
                        previous.Pointer.DeltaPixels.X + record.Pointer.DeltaPixels.X,
                        previous.Pointer.DeltaPixels.Y + record.Pointer.DeltaPixels.Y);
            }
            return true;
        }

        private FishUIDiagnosticEvent GetLast() => _buffer[(_start + _count - 1) % _buffer.Length];

        public IReadOnlyList<FishUIDiagnosticEvent> GetRecentEvents(int maximum = int.MaxValue)
        {
            lock (_gate)
            {
                int take = Math.Min(Math.Max(0, maximum), _count);
                var result = new List<FishUIDiagnosticEvent>(take);
                int first = _count - take;
                for (int i = first; i < _count; i++)
                    result.Add(_buffer[(_start + i) % _buffer.Length]);
                return result;
            }
        }

        public void Reset()
        {
            lock (_gate)
            {
                if (_buffer != null)
                    Array.Clear(_buffer, 0, _buffer.Length);
                _start = 0;
                _count = 0;
                _discarded = 0;
                _ageDiscarded = 0;
                _capacityDiscarded = 0;
                _capacityDiscardedThroughTimeSeconds = double.NegativeInfinity;
                _lastTime = 0;
            }
        }

        internal void ClearSensitiveHistory() => Reset();

        private void EnsureBuffer()
        {
            int capacity = Math.Max(1, Options.Capacity);
            if (_buffer == null) _buffer = new FishUIDiagnosticEvent[capacity];
        }
    }
}
