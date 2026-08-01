using System.Globalization;

namespace FishUI.Controls
{
	public abstract partial class Control
	{
		protected bool IsDiagnosticEventRecordingEnabled => FishUI?.Diagnostics?.IsEventRecordingEnabled == true;

		protected void RecordDiagnosticTransition(string name, string oldValue, string newValue)
		{
			FishUIDiagnosticsSession diagnostics = FishUI?.Diagnostics;
			if (diagnostics?.IsEventRecordingEnabled != true || string.Equals(oldValue, newValue, System.StringComparison.Ordinal)) return;
			diagnostics.Record(FishUIDiagnosticEventCategory.StateChange, FishUIDiagnosticEventType.StateChanged,
				this, null, state: new FishUIStateEventData { Name = name, OldValue = oldValue, NewValue = newValue });
		}

		protected void RecordDiagnosticTransition(string name, int oldValue, int newValue)
		{
			if (!IsDiagnosticEventRecordingEnabled || oldValue == newValue) return;
			RecordDiagnosticTransition(name, oldValue.ToString(CultureInfo.InvariantCulture), newValue.ToString(CultureInfo.InvariantCulture));
		}

		protected void RecordDiagnosticTransition(string name, long oldValue, long newValue)
		{
			if (!IsDiagnosticEventRecordingEnabled || oldValue == newValue) return;
			RecordDiagnosticTransition(name, oldValue.ToString(CultureInfo.InvariantCulture), newValue.ToString(CultureInfo.InvariantCulture));
		}

		protected void RecordDiagnosticTransition(string name, float oldValue, float newValue)
		{
			if (!IsDiagnosticEventRecordingEnabled || oldValue == newValue) return;
			RecordDiagnosticTransition(name, oldValue.ToString("R", CultureInfo.InvariantCulture), newValue.ToString("R", CultureInfo.InvariantCulture));
		}

		protected void RecordDiagnosticTransition(string name, bool oldValue, bool newValue)
		{
			if (!IsDiagnosticEventRecordingEnabled || oldValue == newValue) return;
			RecordDiagnosticTransition(name, oldValue ? bool.TrueString : bool.FalseString,
				newValue ? bool.TrueString : bool.FalseString);
		}
	}
}
