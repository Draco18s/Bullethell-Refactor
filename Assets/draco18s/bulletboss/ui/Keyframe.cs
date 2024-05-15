using System;
using Assets.draco18s.bulletboss.pattern;
using Assets.draco18s.ui;
using Assets.draco18s.util;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.draco18s.bulletboss.ui
{
	public class Keyframe : MonoBehaviour
	{
		public enum EditTypes
		{
			None = 0,
			Angular = 1,
			Linear = 2,
		}

		[SerializeField] private Image icon;
		[SerializeField] private Image bg;
		[SerializeField] private Image frame;
		[SerializeField] private DraggableElement linearEditor;
		[SerializeField] private DraggableElement angularEditor;
		
		public void SetIcon(Sprite sprite, Color color)
		{
			icon.sprite = sprite;
			frame.color = color;
		}

		public void SetIconColor(Color color)
		{
			bg.color = color;
		}

		public void SetEditableType(EditTypes editType, FloatRange allowedRange, float curValue, bool showLabel, float scalar, Action<float> onUpdate)
		{
			bool rangeNonZero = !Mathf.Approximately(allowedRange.min, allowedRange.max);
			if (editType == EditTypes.Angular)
			{
				angularEditor.gameObject.SetActive(rangeNonZero);
				angularEditor.SetLimits(allowedRange, onUpdate);
				angularEditor.ShowLabel(showLabel);
				//angularEditor.SetScalar(scalar);
				angularEditor.SetValue(curValue);
			}
			if (editType == EditTypes.Linear)
			{
				linearEditor.gameObject.SetActive(rangeNonZero);
				linearEditor.SetLimits(allowedRange, onUpdate);
				linearEditor.ShowLabel(showLabel);
				linearEditor.SetScalar(scalar);
				linearEditor.SetValue(curValue);
			}
		}

		public void AddHover(ButtonExtensions.OnHoverDelegate action)
		{
			bg.AddHover(action, true);
		}
	}
}