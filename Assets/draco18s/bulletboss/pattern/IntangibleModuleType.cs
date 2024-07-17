using System;
using Assets.draco18s.bulletboss.entities;
using Assets.draco18s.bulletboss.ui;
using Assets.draco18s.util;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Keyframe = Assets.draco18s.bulletboss.ui.Keyframe;

namespace Assets.draco18s.bulletboss.pattern
{
	[CreateAssetMenu(menuName = "Pattern/Intangible")]
	public class IntangibleModuleType : PatternModuleType
	{
		public virtual ModuleClassification moduleTypeClass => ModuleClassification.Effect;
		[SerializeField] private float duration;
		[SerializeField] private FloatRange allowedDurationRange;

		public override PatternModule GetRuntimeObject()
		{
			return new IntangibleModule(this);
		}
#if UNITY_EDITOR
		[UsedImplicitly]
		private void OnValidate()
		{
			if (preconfigured)
			{
				duration = Mathf.Max(duration, 0.025f);
				allowedDurationRange = new FloatRange(duration, duration);
			}
		}
#endif
		public class IntangibleModule : PatternModule<IntangibleModuleType>
		{
			protected float timeElapsed;
			public float changeDuration;
			public override float duration => changeDuration;

			public IntangibleModule(IntangibleModuleType type) : base(type)
			{
				timeElapsed = 0;
				if (patternType.preconfigured)
				{
					changeDuration = patternType.duration;
				}
				else
				{
					changeDuration = patternType.allowedDurationRange.min;
				}
			}

			public override bool DoShotStep(Bullet shot, float deltaTime, out bool shouldBulletBeRemoved)
			{
				shouldBulletBeRemoved = false;
				if (timeElapsed <= 0)
				{
					shot.SetCanCollide(true);
				}
				timeElapsed += deltaTime;
				shouldBulletBeRemoved = patternType.killOnComplete && timeElapsed >= changeDuration;
				if (timeElapsed >= changeDuration)
				{
					shot.SetCanCollide(false);
				}
				return timeElapsed >= changeDuration;
			}

			public override void ResetForNewLoopIteration(Bullet shot)
			{
				timeElapsed = 0;
			}

			public override void ConfigureKeyframe(RectTransform keyframeBar, DraggableElement handle, Keyframe editableKeyframe)
			{
				handle.SetLimits(patternType.allowedDurationRange, UpdateDuration);
			}

			private void UpdateDuration(float dv)
			{
				changeDuration = Mathf.Clamp(changeDuration + dv, patternType.allowedDurationRange.min, patternType.allowedDurationRange.max);
				TimelineUI.instance.currentTimeline.ValidateModules();
			}

			public override PatternModule Clone()
			{
				return new IntangibleModule(patternType);
			}

			public override PatternModuleType ExportAsScriptableObject()
			{
				IntangibleModuleType result = CreateInstance<IntangibleModuleType>();
				result.name = patternType.name;
				result._preconfigured = true;
				result._rarity = patternTypeData.rarity;
				result._unique = true;
				result.duration = changeDuration;
				result.allowedDurationRange = patternType.allowedDurationRange;
				return result;
			}

			public class Converter : JsonConverter
			{
				public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
				{
					IntangibleModule v = (IntangibleModule)value;
					JObject o = new JObject();
					o.Add(new JProperty("mod_type", CardLibrary.instance.GetModuleName(v.patternTypeData)));
					o.Add(new JProperty("changeDuration", v.changeDuration));
					o.WriteTo(writer);
				}

				public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
				{
					JObject jObject = JObject.Load(reader);
					string id = (string)jObject.GetValue("mod_type");
					PatternModuleType modType = CardLibrary.instance.GetModuleByName(id);
					if (modType == null) throw new JsonReaderException($"Unable to read {id}");
					IntangibleModule runObj = (IntangibleModule)modType.GetRuntimeObject();
					runObj.changeDuration = (float)jObject.GetValue("changeDuration");
					return runObj;
				}

				public override bool CanConvert(Type objectType)
				{
					return typeof(IntangibleModule).IsAssignableFrom(objectType);
				}

				public override bool CanRead => true;

				public override bool CanWrite => true;
			}
		}
	}
}