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
		private long _nextSequence;
		private double _lastTime;

		public FishUIEventRecorderOptions Options { get; } = new FishUIEventRecorderOptions();
		public int Count { get { lock (_gate) return _count; } }
		public long DiscardedOldestCount { get { lock (_gate) return _discarded; } }
		public long LatestSequence { get { lock (_gate) return _nextSequence; } }

		internal FishUIDiagnosticEvent Add(FishUIDiagnosticEvent record)
		{
			if (!Options.Enabled)
				return null;
			lock (_gate)
			{
				EnsureBuffer();
				record.Sequence = ++_nextSequence;
				record.DeltaSincePreviousEventMs = _lastTime == 0 ? 0 : Math.Max(0, (record.TimeSeconds - _lastTime) * 1000.0);
				_lastTime = record.TimeSeconds;
				if (Options.EventFilter != null && !Options.EventFilter(record))
					return null;
				if (TryCoalesceMotion(record))
					return GetLast();

				if (_count == _buffer.Length)
				{
					_buffer[_start] = record;
					_start = (_start + 1) % _buffer.Length;
					_discarded++;
				}
				else
				{
					_buffer[(_start + _count) % _buffer.Length] = record;
					_count++;
				}
				return record;
			}
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
				_lastTime = 0;
			}
		}

		internal void ClearSensitiveHistory() => Reset();

		private void EnsureBuffer()
		{
			int capacity = Math.Max(1, Options.Capacity);
			if (_buffer != null && _buffer.Length == capacity)
				return;
			var previous = GetRecentEvents(capacity);
			_buffer = new FishUIDiagnosticEvent[capacity];
			_start = 0;
			_count = 0;
			foreach (var record in previous)
				_buffer[_count++] = record;
		}
	}
}
