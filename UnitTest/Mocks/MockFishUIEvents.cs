using FishUI;
using FishUI.Controls;

namespace UnitTest.Mocks
{
	/// <summary>
	/// Mock events backend for unit testing FishUI event handling.
	/// </summary>
	public class MockFishUIEvents : IFishUIEvents
	{
		// Event tracking
		public List<(Control Control, string Name, object[] Args)> BroadcastCalls { get; } = new();
		public List<FishUIClickEventArgs> ClickEvents { get; } = new();
		public List<FishUIClickEventArgs> DoubleClickEvents { get; } = new();
		public List<FishUIMouseEventArgs> MouseEnterEvents { get; } = new();
		public List<FishUIMouseEventArgs> MouseLeaveEvents { get; } = new();
		public List<FishUIValueChangedEventArgs> ValueChangedEvents { get; } = new();
		public List<FishUISelectionChangedEventArgs> SelectionChangedEvents { get; } = new();
		public List<FishUITextChangedEventArgs> TextChangedEvents { get; } = new();
		public List<FishUICheckedChangedEventArgs> CheckedChangedEvents { get; } = new();
		public List<FishUILayoutLoadedEventArgs> LayoutLoadedEvents { get; } = new();

		public void Broadcast(FishUI.FishUI FUI, Control Ctrl, string Name, object[] Args)
		{
			BroadcastCalls.Add((Ctrl, Name, Args));
		}

		public void OnControlClicked(FishUIClickEventArgs e) => ClickEvents.Add(e);
		public void OnControlDoubleClicked(FishUIClickEventArgs e) => DoubleClickEvents.Add(e);
		public void OnControlMouseEnter(FishUIMouseEventArgs e) => MouseEnterEvents.Add(e);
		public void OnControlMouseLeave(FishUIMouseEventArgs e) => MouseLeaveEvents.Add(e);
		public void OnControlValueChanged(FishUIValueChangedEventArgs e) => ValueChangedEvents.Add(e);
		public void OnControlSelectionChanged(FishUISelectionChangedEventArgs e) => SelectionChangedEvents.Add(e);
		public void OnControlTextChanged(FishUITextChangedEventArgs e) => TextChangedEvents.Add(e);
		public void OnControlCheckedChanged(FishUICheckedChangedEventArgs e) => CheckedChangedEvents.Add(e);
		public void OnLayoutLoaded(FishUILayoutLoadedEventArgs e) => LayoutLoadedEvents.Add(e);

		public void Reset()
		{
			BroadcastCalls.Clear();
			ClickEvents.Clear();
			DoubleClickEvents.Clear();
			MouseEnterEvents.Clear();
			MouseLeaveEvents.Clear();
			ValueChangedEvents.Clear();
			SelectionChangedEvents.Clear();
			TextChangedEvents.Clear();
			CheckedChangedEvents.Clear();
			LayoutLoadedEvents.Clear();
		}
	}
}
