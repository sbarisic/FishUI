using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	public delegate void OnScrollChangedFuncH(ScrollBarH Sender, float Scroll, int Direction);

	public class ScrollBarH : Control
	{
		[YamlMember]
		public float ThumbWidth = 0.5f; // 0..1

		[YamlMember]
		public float ThumbPosition = 0.1f; // 0..1

		[YamlMember]
		public float ScrollStep = 0.05f;

		public event OnScrollChangedFuncH OnScrollChanged;


		[YamlIgnore]
		Vector2 ButtonSize = new Vector2(15, 15);

		[YamlIgnore]
		Button BtnThumb = null;

		[YamlIgnore]
		Button BtnLeft = null;

		[YamlIgnore]
		Button BtnRight = null;

		public ScrollBarH()
		{
			Size = new Vector2(200, 15);
		}

		bool IsInsideThumb(FishUI UI, Vector2 Pos)
		{
			CalculateThumb(out Vector2 ThumbSize, out Vector2 ThumbPos);

			if (Utils.IsInside(ThumbPos, ThumbSize, Pos))
				return true;

			return false;
		}

		bool IsInsideBtnLeft(FishUI UI, Vector2 Pos)
		{
			Vector2 BtnPos = GetAbsolutePosition();

			if (Utils.IsInside(BtnPos, ButtonSize, Pos))
				return true;

			return false;
		}

		bool IsInsideBtnRight(FishUI UI, Vector2 Pos)
		{
			Vector2 BtnPos = GetAbsolutePosition() + new Vector2(GetAbsoluteSize().X - ButtonSize.X, 0);

			if (Utils.IsInside(BtnPos, ButtonSize, Pos))
				return true;

			return false;
		}

		void CalculateThumb(out Vector2 ThumbSize, out Vector2 ThumbPos)
		{
			Vector2 BtnSize = ButtonSize;
			Vector2 thumbScrollSize = GetAbsoluteSize() - new Vector2(BtnSize.X + BtnSize.X, 0);
			Vector2 thumbScrollPos = GetAbsolutePosition() + new Vector2(BtnSize.X, 0);

			float thumbW = thumbScrollSize.X * ThumbWidth;
			float thumbX = (thumbScrollSize.X - thumbW) * ThumbPosition;

			ThumbSize = new Vector2(thumbW, thumbScrollSize.Y);
			ThumbPos = thumbScrollPos + new Vector2(thumbX, 0);
		}




		void CreateChildControls(FishUI UI)
		{
			if (BtnRight != null)
				return;
			RemoveAllChildren();

			BtnLeft = new Button(UI.Settings.ImgSBHBtnLeftNormal, UI.Settings.ImgSBHBtnLeftDisabled, UI.Settings.ImgSBHBtnLeftPressed, UI.Settings.ImgSBHBtnLeftHover);
			BtnLeft.Position = new Vector2(0, 0);
			BtnLeft.Size = ButtonSize;
			BtnLeft.OnButtonPressed += (sender, btn, pos) =>
			{
				ScrollLeft();
			};
			AddChild(BtnLeft);

			BtnRight = new Button(UI.Settings.ImgSBHBtnRightNormal, UI.Settings.ImgSBHBtnRightDisabled, UI.Settings.ImgSBHBtnRightPressed, UI.Settings.ImgSBHBtnRightHover);
			BtnRight.Size = ButtonSize;
			BtnRight.Position = new Vector2(GetAbsoluteSize().X - ButtonSize.X, 0);
			BtnRight.OnButtonPressed += (sender, btn, pos) =>
			{
				ScrollRight();
			};
			AddChild(BtnRight);

			BtnThumb = new Button(UI.Settings.ImgSBHBarNormal, UI.Settings.ImgSBHBarDisabled, UI.Settings.ImgSBHBarPressed, UI.Settings.ImgSBHBarHover);
			BtnThumb.Draggable = true;
			BtnThumb.OnDragged += (_, Delta) =>
			{
				Vector2 BtnSize = ButtonSize;
				Vector2 thumbScrollSize = GetAbsoluteSize() - new Vector2(BtnSize.X + BtnSize.X, 0);

				float newThumbX = ThumbPosition * (thumbScrollSize.X - (thumbScrollSize.X * ThumbWidth)) + Delta.X;

				float OldThumbPosition = ThumbPosition;
				ThumbPosition = newThumbX / (thumbScrollSize.X - (thumbScrollSize.X * ThumbWidth));

				float Dt = ThumbPosition - OldThumbPosition;
				int Dir = Dt > 0 ? 1 : -1;

				if (ThumbPosition < 0)
					ThumbPosition = 0;
				if (ThumbPosition > 1)
					ThumbPosition = 1;

				OnScrollChanged?.Invoke(this, ThumbPosition, Dir);
			};
			AddChild(BtnThumb);

		}

		public void ScrollLeft()
		{
			ThumbPosition -= ScrollStep;

			if (ThumbPosition < 0)
				ThumbPosition = 0;

			OnScrollChanged?.Invoke(this, ThumbPosition, -1);
		}

		public void ScrollRight()
		{
			ThumbPosition += ScrollStep;

			if (ThumbPosition > 1)
				ThumbPosition = 1;

			OnScrollChanged?.Invoke(this, ThumbPosition, 1);
		}

		public override void HandleMouseWheel(FishUI UI, FishInputState InState, float WheelDelta)
		{
			if (WheelDelta > 0)
				ScrollLeft();
			else if (WheelDelta < 0)
				ScrollRight();
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			CreateChildControls(UI);

			Vector2 GlobalPos = GetAbsolutePosition();
			Vector2 size = GetAbsoluteSize();

			// Update button positions for size changes
			if (BtnLeft != null)
			{
				BtnLeft.Position = new Vector2(0, 0);
				BtnLeft.Size = ButtonSize;
			}

			if (BtnRight != null)
			{
				BtnRight.Position = new Vector2(size.X - ButtonSize.X, 0);
				BtnRight.Size = ButtonSize;
			}

			// Draw background
			UI.Graphics.DrawNPatch(UI.Settings.ImgSBHBarBackground, GlobalPos, size, Color);

			CalculateThumb(out Vector2 thumbSize, out Vector2 thumbPos);
			if (BtnThumb != null)
			{
				BtnThumb.Position = GetLocalRelative(thumbPos);
				BtnThumb.Size = thumbSize;
			}
		}
	}
}
