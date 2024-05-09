using Assets.draco18s.bulletboss.ui;
using Assets.draco18s.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SocialPlatforms;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

namespace Assets.draco18s.ui
{
	public class SettingKnob : DraggableElement, IPointerDownHandler
	{
		[SerializeField] private DraggableHandle handle;
		[SerializeField] private RectTransform changeBar;
		private FloatRange allowedRange;
		private Action<float> onUpdate;

		private Vector3 target;
		
		public override void OnDrag(PointerEventData data)
		{
			float min = allowedRange.min;
			float max = allowedRange.max;
			float ang = changeBar.localEulerAngles.z;
			if (ang > 180) ang -= 360;
			
			target += new Vector3(data.delta.x, data.delta.y);
			float a = Mathf.Atan2(target.y, target.x) / Mathf.PI * 180;
			//SetValue(a-90);
			changeBar.localEulerAngles = new Vector3(0, 0, Mathf.Clamp(a - 90, min, max));
			onUpdate(Mathf.Clamp(a - 90, min, max));
		}

		public override void SetValue(float val)
		{
			float min = gameObject.activeSelf ? allowedRange.min : float.NegativeInfinity;
			float max = gameObject.activeSelf ? allowedRange.max : float.PositiveInfinity;

			if (val > 180) val -= 360;

			changeBar.localEulerAngles = new Vector3(0, 0, val);
			target = Vector3.zero;
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

		}

		public override void SetScalar(float scale)
		{
			throw new Exception("Knob does not support a scalar");
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (target.sqrMagnitude == 0)
			{
				target = new Vector3(0, 35, 0);
				return;
			}
			target = target.normalized * 35;
		}
	}
}
