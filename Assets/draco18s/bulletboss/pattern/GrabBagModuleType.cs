using System;
using Assets.draco18s.bulletboss.pattern.timeline;
using System.Collections.Generic;
using Assets.draco18s.bulletboss.entities;
using Assets.draco18s.bulletboss.cards;
using Assets.draco18s.util;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using static Assets.draco18s.bulletboss.pattern.ChangeModuleType;

namespace Assets.draco18s.bulletboss.pattern
{
	[CreateAssetMenu(menuName = "Pattern/New Grab Bag")]
	public class GrabBagModuleType : PatternGroupModuleType
	{
		public override PatternModule GetRuntimeObject()
		{
			return new GrabBagModule(this);
		}

		//[JsonConverter(typeof(GroupResolver))]
		public class GrabBagModule : PatternGroup
		{
			protected GrabBagModuleType grabBagTypeData;
			protected PatternModule nextPattern;
			public override float duration => nextPattern?.duration ?? 1f;

			public GrabBagModule(GrabBagModuleType modType) : base(modType)
			{
				grabBagTypeData = modType;
			}

			public override PatternModule Clone()
			{
				GrabBagModule mod = new GrabBagModule(grabBagTypeData);
				mod.pattern = Timeline.CloneFrom(pattern);
				mod.pattern.InitOrReset();
				return mod;
			}

			public override bool DoShotStep(Bullet shot, float deltaTime, out bool shouldBulletBeRemoved)
			{
				shouldBulletBeRemoved = false;
				if (nextPattern == null)
					ResetForNewLoopIteration();
				if (nextPattern == null)
				{
					return true;
				}
				nextPattern.DoShotStep(shot, deltaTime, out bool b);
				shouldBulletBeRemoved |= b;
				return true;
			}

			public override void ResetForNewLoopIteration()
			{
				base.ResetForNewLoopIteration();
				KeyValuePair<int, Card> randKvp;
				do
				{
					randKvp = childPattern.GetModules().GetRandom();

					// if there's only two options (or fewer), heck trying to pick a non-duplicate
				} while (childPattern.GetModules().Count > 2 && randKvp.Value?.pattern == nextPattern);
				nextPattern = randKvp.Value?.pattern;
			}

			/*public class GroupResolver : JsonConverter
			{
				public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
				{
					GrabBagModule v = (GrabBagModule)value;
					JObject o = new JObject();
					o.Add(new JProperty("mod_type", v.patternTypeData.name));
					o.Add(new JProperty("timeline", JsonConvert.SerializeObject(v.pattern, ContractResolver.jsonSettings)));
					o.WriteTo(writer);
				}

				public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
				{
					JObject jObject = JObject.Load(reader);
					string id = (string)jObject.GetValue("mod_type");
					PatternModuleType modType = CardLibrary.instance.GetModuleByName(id);
					if (modType == null) throw new JsonReaderException($"Unable to read {id}");
					GrabBagModule runObj = (GrabBagModule)modType.GetRuntimeObject();
					runObj.pattern = jObject.GetValue("timeline").Value<Timeline>();
					return runObj;
				}

				public override bool CanConvert(Type objectType)
				{
					return typeof(GrabBagModule).IsAssignableFrom(objectType);
				}

				public override bool CanRead => true;

				public override bool CanWrite => true;
			}*/
		}
	}
}
