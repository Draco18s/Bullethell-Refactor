using Assets.draco18s.util;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.draco18s.bulletboss.ui
{
	public abstract class DraggableElement : MonoBehaviour
	{
		public abstract void SetValue(float v);
		public abstract void OnDrag(PointerEventData data);
		public abstract void SetLimits(FloatRange range, Action<float> callback);
		public abstract void Disable();
		public abstract void Enable();
		public abstract void ShowLabel(bool show);
		public abstract void SetScalar(float scale);
	}
}
