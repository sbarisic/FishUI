using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	public delegate void OnScrollChangedFunc(ScrollBarV Sender, float Scroll, int Direction);

	public class ScrollBarV : Control
	{
		[YamlMember]
		public float ThumbHeight = 0.5f; // 0..1

		[YamlMember]
		public float ThumbPosition = 0.1f; // 0..1

		[YamlMember]
		public float ScrollStep = 0.05f;

		public event OnScrollChangedFunc OnScrollChanged;


		[YamlIgnore]
		Vector2 ButtonSize = new Vector2(15, 15);

		[YamlIgnore]
		Button BtnThumb = null;

		[YamlIgnore]
		Button BtnUp = null;

		[YamlIgnore]
		Button BtnDown = null;

		public ScrollBarV()
		{
			Size = new Vector2(15, 200);
		}

		bool IsInsideThumb(FishUI UI, Vector2 Pos)
		{
			CalculateThumb(out Vector2 ThumbSize, out Vector2 ThumbPos);

			if (Utils.IsInside(ThumbPos, ThumbSize, Pos))
				return true;

			return false;
		}

		bool IsInsideBtnUp(FishUI UI, Vector2 Pos)
		{
			Vector2 BtnPos = GetAbsolutePosition();

			if (Utils.IsInside(BtnPos, ButtonSize, Pos))
				return true;

			return false;
		}

		bool IsInsideBtnDown(FishUI UI, Vector2 Pos)
		{
			Vector2 BtnPos = GetAbsolutePosition() + new Vector2(0, GetAbsoluteSize().Y - ButtonSize.Y);

			if (Utils.IsInside(BtnPos, ButtonSize, Pos))
				return true;

			return false;
		}

		void CalculateThumb(out Vector2 ThumbSize, out Vector2 ThumbPos)
		{
			Vector2 BtnSize = ButtonSize;
			Vector2 thumbScrollSize = GetAbsoluteSize() - new Vector2(0, BtnSize.Y + BtnSize.Y);
			Vector2 thumbScrollPos = GetAbsolutePosition() + new Vector2(0, BtnSize.Y);

			float thumbH = thumbScrollSize.Y * ThumbHeight;
			float thumbY = (thumbScrollSize.Y - thumbH) * ThumbPosition;

			ThumbSize = new Vector2(thumbScrollSize.X, thumbH);
			ThumbPos = thumbScrollPos + new Vector2(0, thumbY);
		}




		void CreateChildControls(FishUI UI)
		{
			if (BtnDown != null)
				return;
			RemoveAllChildren();

			BtnDown = new Button(UI.Settings.ImgSBVBtnUpNormal, UI.Settings.ImgSBVBtnUpDisabled, UI.Settings.ImgSBVBtnUpPressed, UI.Settings.ImgSBVBtnUpHover);
			BtnDown.Position = new Vector2(0, 0);
			BtnDown.Size = ButtonSize;
			BtnDown.OnButtonPressed += (sender, btn, pos) =>
			{
				ScrollUp();
			};
			AddChild(BtnDown);

			BtnUp = new Button(UI.Settings.ImgSBVBtnDownNormal, UI.Settings.ImgSBVBtnDownDisabled, UI.Settings.ImgSBVBtnDownPressed, UI.Settings.ImgSBVBtnDownHover);
			BtnUp.Size = ButtonSize;
			BtnUp.Position = new Vector2(0, GetAbsoluteSize().Y - ButtonSize.Y);
			BtnUp.OnButtonPressed += (sender, btn, pos) =>
			{
				ScrollDown();
			};
			AddChild(BtnUp);

			BtnThumb = new Button(UI.Settings.ImgSBVBarNormal, UI.Settings.ImgSBVBarDisabled, UI.Settings.ImgSBVBarPressed, UI.Settings.ImgSBVBarHover);
			BtnThumb.Draggable = true;
			BtnThumb.OnDragged += (_, Delta) =>
			{
				Vector2 BtnSize = ButtonSize;
				Vector2 thumbScrollSize = GetAbsoluteSize() - new Vector2(0, BtnSize.Y + BtnSize.Y);

				float thumbH = thumbScrollSize.Y * ThumbHeight;
				float availableRange = thumbScrollSize.Y - thumbH;

				// Avoid division by zero when thumb fills the entire track
				if (availableRange <= 0)
					return;

				float currentThumbY = ThumbPosition * availableRange;
				float newThumbY = currentThumbY + Delta.Y;

				float OldThumbPosition = ThumbPosition;
				ThumbPosition = newThumbY / availableRange;

				// Clamp before calculating direction
				if (ThumbPosition < 0)
					ThumbPosition = 0;
				if (ThumbPosition > 1)
					ThumbPosition = 1;

				float Dt = ThumbPosition - OldThumbPosition;
				int Dir = Dt > 0 ? 1 : (Dt < 0 ? -1 : 0);

				if (Dir != 0)
					OnScrollChanged?.Invoke(this, ThumbPosition, Dir);
			};
			AddChild(BtnThumb);

		}

		public void ScrollUp()
		{
			ThumbPosition -= ScrollStep;

			if (ThumbPosition < 0)
				ThumbPosition = 0;

			OnScrollChanged?.Invoke(this, ThumbPosition, -1);
		}

		public void ScrollDown()
		{
			ThumbPosition += ScrollStep;

			if (ThumbPosition > 1)
				ThumbPosition = 1;

			OnScrollChanged?.Invoke(this, ThumbPosition, 1);
		}

		public override void HandleMouseWheel(FishUI UI, FishInputState InState, float WheelDelta)
		{
			if (WheelDelta > 0)
				ScrollUp();
			else if (WheelDelta < 0)
				ScrollDown();
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			CreateChildControls(UI);

			Vector2 GlobalPos = GetAbsolutePosition();
			Vector2 size = GetAbsoluteSize();

			// Update button positions for size changes
			if (BtnDown != null)
			{
				BtnDown.Position = new Vector2(0, 0);
				BtnDown.Size = ButtonSize;
			}

			if (BtnUp != null)
			{
				BtnUp.Position = new Vector2(0, size.Y - ButtonSize.Y);
				BtnUp.Size = ButtonSize;
			}

			// Draw background
			UI.Graphics.DrawNPatch(UI.Settings.ImgSBVBarBackground, GlobalPos, size, Color);

			CalculateThumb(out Vector2 thumbSize, out Vector2 thumbPos);
			if (BtnThumb != null)
			{
				BtnThumb.Position = GetLocalRelative(thumbPos);
				BtnThumb.Size = thumbSize;
			}
		}
	}
}
