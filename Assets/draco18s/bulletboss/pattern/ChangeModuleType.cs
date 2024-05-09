﻿using Assets.draco18s.bulletboss.entities;
using Assets.draco18s.bulletboss.ui;
using Assets.draco18s.util;
using UnityEngine;
using UnityEngine.Serialization;
using Keyframe = Assets.draco18s.bulletboss.ui.Keyframe;

namespace Assets.draco18s.bulletboss.pattern
{
	[CreateAssetMenu(menuName = "New Bullet Change")]
	public class ChangeModuleType : PatternModuleType
	{
		public enum ChangeType
		{
			Speed,Direction,Size,Time
		}

		[SerializeField] private ChangeType changeType;
		[SerializeField] private FloatRange allowedValueRange;

		[SerializeField] private float newValue;
		[SerializeField] private FloatRange randomRange;
		[SerializeField] private float changeDuration;
		[SerializeField] private FloatRange allowedDurationRange;

		public override PatternModule GetRuntimeObject()
		{
			return new ChangeModule(this);
		}

		public class ChangeModule : PatternModule<ChangeModuleType>
		{
			private float timeElapsed;
			private float oldValue;
			public float newValue;
			public FloatRange randomRange;
			public float changeDuration;

			public override float duration => changeDuration;

			public ChangeModule(ChangeModuleType type) : base(type)
			{
				timeElapsed = 0;
				if (patternType.preconfigured)
				{
					newValue = patternType.newValue;
					randomRange = patternType.randomRange;
					changeDuration = patternType.changeDuration;
				}
				else
				{
					changeDuration = patternType.allowedDurationRange.min;
				}
			}

			public override PatternModule Clone()
			{
				ChangeModule mod = new ChangeModule(patternType);
				mod.newValue = newValue;
				mod.changeDuration = changeDuration;
				//mod.randomRange = randomRange;
				return mod;
			}

			public override bool DoShotStep(Bullet shot, float deltaTime, out bool shouldBulletBeRemoved)
			{
				timeElapsed += deltaTime;
				float t = Mathf.Clamp01(timeElapsed / changeDuration);
				float val = Mathf.Lerp(oldValue, newValue, t);

				switch (patternType.changeType)
				{
					case ChangeType.Speed:
						if (oldValue < -1000)
						{
							oldValue = shot.speed;
							val = Mathf.Lerp(oldValue, newValue, t);
						}
						shot.ChangeSpeed(val);
						break;
					case ChangeType.Direction:
						if (oldValue < -1000)
						{
							oldValue = shot.transform.localEulerAngles.z;
							newValue = oldValue + newValue;
							val = Mathf.Lerp(oldValue, newValue, t);
						}
						shot.ChangeRotation(val);
						break;
					case ChangeType.Size:
						if (oldValue < -1000)
						{
							oldValue = shot.transform.localScale.x;
							val = Mathf.Lerp(oldValue, newValue, t);
						}
						shot.ChangeScale(val);
						break;
				}
				shouldBulletBeRemoved = patternType.killOnComplete && timeElapsed >= changeDuration;
				return timeElapsed >= changeDuration;
			}

			public override void ResetForNewLoopIteration()
			{
				oldValue = float.NegativeInfinity;
				timeElapsed = 0;
				if(patternType.randomRange.min != 0 && patternType.randomRange.max != 0)
					newValue = patternType.newValue + Random.Range(patternType.randomRange.min, patternType.randomRange.max) + patternType.newValue;
			}

			public override void ConfigureKeyframe(RectTransform keyframeBar, DraggableElement handle, Keyframe editableKeyframe)
			{
				handle.SetLimits(patternType.allowedDurationRange, UpdateDuration);
				handle.SetValue(changeDuration);
				switch (patternType.changeType)
				{
					case ChangeType.Direction:
						editableKeyframe.SetEditableType(Keyframe.EditTypes.Angular, patternType.allowedValueRange, newValue, true, 1, UpdateValue);
						break;
					case ChangeType.Speed:
					case ChangeType.Size:
						editableKeyframe.SetEditableType(Keyframe.EditTypes.Linear, patternType.allowedValueRange, newValue, true, 100, UpdateValue);
						break;
				}
				if (patternType.preconfigured || patternType.allowedDurationRange.Range <= float.Epsilon)
					handle.Disable();
			}

			private void UpdateDuration(float dv)
			{
				changeDuration = Mathf.Clamp(changeDuration + dv, patternType.allowedDurationRange.min, patternType.allowedDurationRange.max);
				TimelineUI.instance.currentTimeline.ValidateModules();
			}

			private void UpdateValue(float dv)
			{
				newValue = Mathf.Clamp(dv, patternType.allowedValueRange.min, patternType.allowedValueRange.max);
			}
		}
	}
}
