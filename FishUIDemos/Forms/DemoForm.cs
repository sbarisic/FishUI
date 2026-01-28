using System;
using System.Numerics;
using FishUI;
using FishUI.Controls;

namespace FishUIDemos.Forms
{
	/// <summary>
	/// User-defined logic for DemoForm. Event handlers and custom code go here.
	/// This partial class extends the designer-generated DemoForm.Designer.cs
	/// </summary>
	public partial class DemoForm
	{
		/// <summary>
		/// Wires up event handlers after controls are loaded.
		/// Call this method after LoadControls.
		/// </summary>
		public void SetupEventHandlers()
		{
			// Wire up button click events
			btnGreet.OnButtonPressed += OnGreetClicked;
			btnClose.OnButtonPressed += OnCloseClicked;
		}

		private void OnGreetClicked(Button sender, FishMouseButton btn, Vector2 pos)
		{
			string name = string.IsNullOrWhiteSpace(txtName.Text) ? "World" : txtName.Text;
			lblGreeting.Text = $"Hello, {name}! Welcome to FishUI!";
		}

		private void OnCloseClicked(Button sender, FishMouseButton btn, Vector2 pos)
		{
			mainWindow.Close();
		}
	}
}
