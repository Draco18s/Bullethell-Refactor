using System;
using Assets.draco18s.bulletboss.entities;
using Assets.draco18s.bulletboss.pattern.timeline;
using Assets.draco18s.bulletboss.ui;
using Assets.draco18s.serialization;
using Assets.draco18s.util;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine;
using static Assets.draco18s.bulletboss.pattern.ChangeModuleType;
using Keyframe = Assets.draco18s.bulletboss.ui.Keyframe;
using Unity.Sentis.Layers;

namespace Assets.draco18s.bulletboss.pattern
{
	[CreateAssetMenu(menuName = "Pattern/New Pattern Loop")]
	public class LoopModuleType : TimelinePatternModuleType
	{
		[SerializeField] private int iterations;
		[SerializeField] private IntRange allowedValueRange;

		public override PatternModule GetRuntimeObject()
		{
			return new LoopModule(this);
		}

		[JsonResolver(typeof(Converter))]
		public class LoopModule : TimelinePatternModule<LoopModuleType>
		{
			private int numLoops;
			private int loopCounter;
			public override bool hasEditableChild => !patternType.preconfiguredPattern || patternType.pattern.isEditable;
			public override float duration => childPattern.GetDuration() * numLoops;

			public LoopModule(LoopModuleType modType) : base(modType)
			{
				loopCounter = 0;
				numLoops = patternType.iterations;
				modType.pattern.DeserializeForRuntime();
			}

			public override PatternModule Clone()
			{
				LoopModule mod = new LoopModule(patternType);
				mod.numLoops = numLoops;
				mod.pattern = Timeline.CloneFrom(pattern);
				mod.pattern.InitOrReset();
				mod.pattern.SetModuleType(mod);
				return mod;
			}

			public override PatternModuleType ExportAsScriptableObject()
			{
				LoopModuleType result = CreateInstance<LoopModuleType>();
				result.name = patternType.name;
				result._preconfigured = result.preconfiguredPattern = true;
				result.pattern = Timeline.CloneForAsset(childPattern);
				result.iterations = numLoops;
				result.allowedValueRange = patternType.allowedValueRange;
				return result;
			}

			public override bool DoShotStep(Bullet shot, float deltaTime, out bool shouldBulletBeRemoved)
			{
				shouldBulletBeRemoved = false;
				if (numLoops > 0 && loopCounter >= numLoops) return true;
				if (childPattern.RuntimeUpdate(shot, deltaTime))
				{
					loopCounter++;
				}
				return numLoops > 0 && loopCounter >= numLoops;
			}

			public override void ResetForNewLoopIteration(Bullet shot)
			{
				loopCounter = 0;
				childPattern.ResetForNewLoopIteration(shot);
			}

			public override void ConfigureKeyframe(RectTransform keyframeBar, DraggableElement handle, Keyframe editableKeyframe)
			{
				childPattern.InitOrReset();
				handle.SetValue(duration);
				handle.Disable();
			}

			public class Converter : JsonConverter
			{
				public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
				{
					LoopModule v = (LoopModule)value;
					JObject o = new JObject();
					o.Add(new JProperty("mod_type", CardLibrary.instance.GetModuleName(v.patternTypeData)));
					o.Add(new JProperty("timeline", JToken.FromObject(v.pattern, serializer)));
					o.Add(new JProperty("numLoops", v.numLoops));
					o.WriteTo(writer);
				}

				public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
				{
					JObject jObject = JObject.Load(reader);
					string id = (string)jObject.GetValue("mod_type");
					PatternModuleType modType = CardLibrary.instance.GetModuleByName(id);
					if (modType == null) throw new JsonReaderException($"Unable to read {id}");
					LoopModule runObj = (LoopModule)modType.GetRuntimeObject();

					runObj.pattern = jObject.GetValue("timeline").Value<Timeline>();
					runObj.numLoops = (int)jObject.GetValue("numLoops");
					return runObj;
				}

				public override bool CanConvert(Type objectType)
				{
					return typeof(LoopModule).IsAssignableFrom(objectType);
				}

				public override bool CanRead => true;

				public override bool CanWrite => true;
			}
		}
	}
}
