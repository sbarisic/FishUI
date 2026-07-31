using FishUI;
using System.Numerics;
using UnityEngine;

using Vector2 = System.Numerics.Vector2;
using UVector2 = UnityEngine.Vector2;
using FishUI.Controls;

public class FishEvents : IFishUIEvents
{
	public void Broadcast(FishUI.FishUI FUI, Control Ctrl, string Name, object[] Args)
	{
		// Log the event for debugging purposes
		string argsStr = Args != null && Args.Length > 0 ? string.Join(", ", Args) : "none";
		Debug.Log($"[FishUI Event] {Name} from {Ctrl?.GetType().Name ?? "unknown"} with args: {argsStr}");
	}
}