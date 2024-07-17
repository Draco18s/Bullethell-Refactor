using System;
using Assets.draco18s.bulletboss.entities;
using Assets.draco18s.bulletboss.ui;
using Assets.draco18s.serialization;
using Assets.draco18s.util;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Unity.Sentis.Layers;
using UnityEngine;
using Keyframe = Assets.draco18s.bulletboss.ui.Keyframe;
using Random = UnityEngine.Random;

namespace Assets.draco18s.bulletboss.pattern
{
	[CreateAssetMenu(menuName = "Pattern/New Bullet Change")]
	public class ChangeModuleType : PatternModuleType
	{
		public enum ChangeType
		{
			Speed,Direction,Size,Time
		}
		public virtual ModuleClassification moduleTypeClass => ModuleClassification.Transform;

		[SerializeField] protected ChangeType changeType;
		[SerializeField] private FloatRange allowedValueRange;

		[SerializeField] private float newValue;
		[SerializeField] private FloatRange randomRange;
		[SerializeField] private float changeDuration;
		[SerializeField] private FloatRange allowedDurationRange;

		public override PatternModule GetRuntimeObject()
		{
			return new ChangeModule(this);
		}
#if UNITY_EDITOR
		[UsedImplicitly]
		private void OnValidate()
		{
			if(preconfigured)
			{
				if (changeType == ChangeType.Time)
				{
					changeDuration = newValue;
				}
				allowedDurationRange = new FloatRange(changeDuration, changeDuration);
			}
		}
#endif
		[JsonResolver(typeof(Converter))]
		public class ChangeModule : PatternModule<ChangeModuleType>
		{
			protected float timeElapsed;
			protected float oldValue;
			public float newValue;
			public float targetValue;
			public float changeDuration;

			public override float duration => changeDuration;

			public ChangeModule(ChangeModuleType type) : base(type)
			{
				timeElapsed = 0;
				if (patternType.preconfigured)
				{
					targetValue = newValue = patternType.newValue;
					changeDuration = patternType.changeDuration;
				}
				else
				{
					changeDuration = patternType.allowedDurationRange.min;
				}

				if (patternType.changeType == ChangeType.Time)
				{
					changeDuration = patternType.newValue;
				}
			}

			public override PatternModule Clone()
			{
				ChangeModule mod = new ChangeModule(patternType);
				mod.newValue = newValue;
				mod.targetValue = targetValue;
				mod.changeDuration = changeDuration;
				return mod;
			}

			public override PatternModuleType ExportAsScriptableObject()
			{
				ChangeModuleType result = CreateInstance<ChangeModuleType>();
				result.name = patternType.name;
				result._preconfigured = true;
				result._rarity = patternTypeData.rarity;
				result._unique = true;
				result.newValue = newValue;
				result.changeDuration = changeDuration;
				result.changeType = patternType.changeType;
				result.allowedDurationRange = patternType.allowedDurationRange;
				result.allowedValueRange = patternType.allowedValueRange;
				return result;
			}

			public override bool DoShotStep(Bullet shot, float deltaTime, out bool shouldBulletBeRemoved)
			{
				timeElapsed += deltaTime;
				float t = Mathf.Clamp01(timeElapsed / changeDuration);
				float val = Mathf.Lerp(oldValue, targetValue, t);

				switch (patternType.changeType)
				{
					case ChangeType.Speed:
						if (oldValue < -1000)
						{
							oldValue = shot.speed;
							val = Mathf.Lerp(oldValue, targetValue, t);
						}
						shot.ChangeSpeed(val);
						break;
					case ChangeType.Direction:
						if (oldValue < -1000)
						{
							oldValue = shot.transform.localEulerAngles.z;
							targetValue = oldValue + newValue;
							val = Mathf.Lerp(oldValue, targetValue, t);
						}
						shot.ChangeRotation(val);
						break;
					case ChangeType.Size:
						if (oldValue < -1000)
						{
							oldValue = shot.transform.localScale.x;
							val = Mathf.Lerp(oldValue, targetValue, t);
						}
						shot.ChangeScale(val);
						break;
				}
				shouldBulletBeRemoved = patternType.killOnComplete && timeElapsed >= changeDuration;
				return timeElapsed >= changeDuration;
			}

			public override void ResetForNewLoopIteration(Bullet shot)
			{
				oldValue = float.NegativeInfinity;
				timeElapsed = 0;
				if(patternType.randomRange.min != 0 && patternType.randomRange.max != 0)
					newValue = patternType.newValue + Random.Range(patternType.randomRange.min, patternType.randomRange.max) + patternType.newValue;
				targetValue = newValue;
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
					case ChangeType.Time:
						editableKeyframe.SetEditableType(Keyframe.EditTypes.Linear, patternType.allowedValueRange, newValue, true, 100, UpdateValue);
						//Debug.Log($"Change time says duration is {changeDuration}");
						break;
				}
				if (patternType.preconfigured || patternType.allowedDurationRange.Range <= float.Epsilon)
					handle.Disable();
				if(patternType.preconfigured)
					editableKeyframe.SetEditableType(Keyframe.EditTypes.None, FloatRange.Zero, 0, false, 1, _ => { });
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

			public class Converter : JsonConverter
			{
				public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
				{
					ChangeModule v = (ChangeModule)value;
					JObject o = new JObject();
					o.Add(new JProperty("mod_type", CardLibrary.instance.GetModuleName(v.patternTypeData)));
					o.Add(new JProperty("newValue", v.newValue));
					o.Add(new JProperty("changeDuration", v.changeDuration));
					o.WriteTo(writer);
				}

				public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
				{
					JObject jObject = JObject.Load(reader);
					string id = (string)jObject.GetValue("mod_type");
					PatternModuleType modType = CardLibrary.instance.GetModuleByName(id);
					if (modType == null) throw new JsonReaderException($"Unable to read {id}");
					ChangeModule runObj = (ChangeModule)modType.GetRuntimeObject();

					runObj.newValue = (float)jObject.GetValue("newValue");
					runObj.changeDuration = (float)jObject.GetValue("changeDuration");
					return runObj;
				}

				public override bool CanConvert(Type objectType)
				{
					return typeof(ChangeModule).IsAssignableFrom(objectType);
				}

				public override bool CanRead => true;

				public override bool CanWrite => true;
			}
		}
	}
}
