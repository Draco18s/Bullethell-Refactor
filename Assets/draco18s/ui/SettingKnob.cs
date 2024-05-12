using Assets.draco18s.bulletboss.ui;
using Assets.draco18s.util;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.draco18s.ui
{
	public class SettingKnob : DraggableElement, IPointerDownHandler
	{
		[SerializeField] private DraggableHandle handle;
		[SerializeField] private RectTransform changeBar;
		[SerializeField] private TextMeshProUGUI label;
		private FloatRange allowedRange;
		private Action<float> onUpdate;

		private Vector3 target;
		private Vector3 trueMouse;
		private bool rangeNonZero;

		public override void OnDrag(PointerEventData data)
		{
			float min = (gameObject.activeSelf && rangeNonZero) ? allowedRange.min : float.NegativeInfinity;
			float max = (gameObject.activeSelf && rangeNonZero) ? allowedRange.max : float.PositiveInfinity;
			float ang = changeBar.localEulerAngles.z;
			if (ang > 180) ang -= 360;

			float val;

			trueMouse += new Vector3(data.delta.x, data.delta.y);

			if (InputManager.PatternEditor.ShiftFidelity.IsPressed())
			{
				target += new Vector3(data.delta.x, data.delta.y) / 5;

				val = Mathf.Clamp((Mathf.Atan2(target.y, target.x) / Mathf.PI * 180) - 90, min, max);
				val = DMath.Step(val, 0.2f);
			}
			else
			{
				target += new Vector3(data.delta.x, data.delta.y);
				val = Mathf.Clamp((Mathf.Atan2(target.y, target.x) / Mathf.PI * 180) - 90, min, max);
				val = DMath.Step(val, 1f);
			}

			changeBar.localEulerAngles = new Vector3(0, 0, val);
			onUpdate(val);
			label.text = val.ToString("F1");
		}

		public override void SetValue(float val)
		{
			float min = (gameObject.activeSelf && rangeNonZero) ? allowedRange.min : float.NegativeInfinity;
			float max = (gameObject.activeSelf && rangeNonZero) ? allowedRange.max : float.PositiveInfinity;

			if (val > 180) val -= 360;
			val = Mathf.Clamp(val, min, max);
			changeBar.localEulerAngles = new Vector3(0, 0, val);
			trueMouse = target = Vector3.zero;
			label.text = val.ToString("F1");
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
			throw new Exception("Knob does not support a scalar");
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			rangeNonZero = !Mathf.Approximately(allowedRange.min, allowedRange.max);
			if (target.sqrMagnitude == 0)
			{
				trueMouse = target = new Vector3(0, 35, 0);
				return;
			}
			trueMouse = target = target.normalized * 35;
		}
	}
}
