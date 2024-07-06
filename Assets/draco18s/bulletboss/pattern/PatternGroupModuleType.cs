using System;
using Assets.draco18s.bulletboss.pattern.timeline;
using Assets.draco18s.util;
using System.Collections.Generic;
using Assets.draco18s.bulletboss.cards;
using Assets.draco18s.bulletboss.entities;
using Assets.draco18s.bulletboss.ui;
using Assets.draco18s.serialization;
using UnityEngine;
using Keyframe = Assets.draco18s.bulletboss.ui.Keyframe;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Assets.draco18s.bulletboss.pattern
{
	[CreateAssetMenu(menuName = "Pattern/New Pattern Group")]
	public class PatternGroupModuleType : TimelinePatternModuleType
	{
		public override PatternModule GetRuntimeObject()
		{
			return new PatternGroup(this);
		}

		public override bool CanAddModule(PatternModuleType module)
		{
			return module is SpawnModuleType or PatternGroupModuleType;
		}

		[JsonResolver(typeof(Converter))]
		public class PatternGroup : TimelinePatternModule<PatternGroupModuleType>
		{
			public override float duration => Mathf.Max(childPattern.GetDuration(), 0.1f);

			public PatternGroup(PatternGroupModuleType patternGroupModuleType) : base(patternGroupModuleType) { }

			public override bool DoShotStep(Bullet shot, float deltaTime, out bool shouldBulletBeRemoved)
			{
				shouldBulletBeRemoved = false;
				foreach (KeyValuePair<int, Card> grp in childPattern.GetModules())
				{
					if(!(grp.Value.pattern.patternTypeData is SpawnModuleType)) continue;
					grp.Value.pattern.DoShotStep(shot, deltaTime, out bool b);
					shouldBulletBeRemoved |= b;
				}
				return true;
			}

			public override PatternModule Clone()
			{
				PatternGroup mod = new PatternGroup(patternType);
				mod.pattern = Timeline.CloneFrom(pattern);
				mod.pattern.InitOrReset();
				mod.pattern.SetModuleType(mod);
				return mod;
			}

			public override PatternModuleType ExportAsScriptableObject()
			{
				PatternGroupModuleType result = CreateInstance<PatternGroupModuleType>();
				result.name = patternType.name;
				result._preconfigured = result.preconfiguredPattern = true;
				result.pattern = Timeline.CloneForAsset(pattern);
				return result;
			}

			public override void ResetForNewLoopIteration()
			{
				foreach (KeyValuePair<int, Card> grp in childPattern.GetModules())
					grp.Value.pattern.ResetForNewLoopIteration();
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
					PatternGroup v = (PatternGroup)value;
					JObject o = new JObject();
					o.Add(new JProperty("mod_type", CardLibrary.instance.GetModuleName(v.patternTypeData)));
					o.Add(new JProperty("timeline", JToken.FromObject(v.pattern, serializer)));
					o.WriteTo(writer);
				}

				public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
				{
					JObject jObject = JObject.Load(reader);
					string id = (string)jObject.GetValue("mod_type");
					PatternModuleType modType = CardLibrary.instance.GetModuleByName(id);
					if (modType == null) throw new JsonReaderException($"Unable to read {id}");
					PatternGroup runObj = (PatternGroup)modType.GetRuntimeObject();

					runObj.pattern = jObject.GetValue("timeline").Value<Timeline>();
					return runObj;
				}

				public override bool CanConvert(Type objectType)
				{
					return typeof(PatternGroup).IsAssignableFrom(objectType);
				}

				public override bool CanRead => true;

				public override bool CanWrite => true;
			}
		}
	}
}
