using Assets.draco18s.bulletboss.ui;
using Assets.draco18s.util;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.draco18s.ui
{
	public class SettingBar : DraggableElement, IPointerDownHandler
	{
		[SerializeField] private DraggableHandle handle;
		[SerializeField] private RectTransform changeBar;
		[SerializeField] private TextMeshProUGUI label;
		[SerializeField] private float scalar = 1;
		private FloatRange allowedRange;
		private Action<float> onUpdate;
		private Vector2 target;
		
		public override void OnDrag(PointerEventData data)
		{
			float min = allowedRange.min * scalar;
			float max = allowedRange.max * scalar;

			target += data.delta;

			changeBar.sizeDelta = new Vector2(Mathf.Clamp(target.x, min, max), changeBar.sizeDelta.y);
			onUpdate(data.delta.x * scalar);
			label.text = (changeBar.sizeDelta.x / scalar).ToString("F1");
		}

		public override void SetValue(float val)
		{
			bool rangeNonZero = !Mathf.Approximately(allowedRange.min, allowedRange.max);
			float min = (gameObject.activeSelf && rangeNonZero) ? allowedRange.min : float.NegativeInfinity;
			float max = (gameObject.activeSelf && rangeNonZero) ? allowedRange.max : float.PositiveInfinity;

			changeBar.sizeDelta = new Vector2(Mathf.Clamp(val, min, max) * scalar, changeBar.sizeDelta.y);
			target = new Vector2(changeBar.sizeDelta.x, 0);
			label.text = (changeBar.sizeDelta.x / scalar).ToString("F1");
		}

		public override void SetLimits(FloatRange range, Action<float> callback)
		{
			allowedRange = range;
			onUpdate = callback;
		}

		public override void Disable()
		{
			handle.Disable();
		}

		public override void Enable()
		{
			handle.Enable();
		}

		public override void ShowLabel(bool show)
		{
			label.gameObject.SetActive(show);
		}

		public override void SetScalar(float scale)
		{
			scalar = scale;
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			target = new Vector2(changeBar.sizeDelta.x, 0);
		}
	}
}
