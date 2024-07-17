using System;
using Assets.draco18s.bulletboss.entities;
using Assets.draco18s.bulletboss.pattern.timeline;
using Assets.draco18s.serialization;
using Assets.draco18s.util;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.draco18s.bulletboss.pattern
{
	[CreateAssetMenu(menuName = "Pattern/Homing Change")]
	public class HomingTurnModuleType : ChangeModuleType
	{
		public HomingTurnModuleType()
		{
			changeType = ChangeType.Direction;
		}
#if UNITY_EDITOR
		[UsedImplicitly]
		void OnValidate()
		{
			changeType = ChangeType.Direction;
		}
#endif
		public override PatternModule GetRuntimeObject()
		{
			return new HomingTurnModule(this);
		}

		[JsonResolver(typeof(HomingResolver))]
		public class HomingTurnModule : ChangeModule
		{
			protected HomingTurnModuleType homingTypeData;
			protected float maxTurnRate;
			public HomingTurnModule(HomingTurnModuleType modType) : base(modType)
			{
				homingTypeData = modType;
				maxTurnRate = newValue;
				newValue = 0;
			}

			public override PatternModule Clone()
			{
				HomingTurnModule mod = new HomingTurnModule(homingTypeData);
				mod.changeDuration = changeDuration;
				return mod;
			}

			public override bool DoShotStep(Bullet shot, float deltaTime, out bool shouldBulletBeRemoved)
			{
				float bestAngle = 180;
				foreach (Collider2D c in Physics2D.OverlapCircleAll(shot.transform.position, 10, shot.GetTargetLayerMask()))
				{
					Transform playerTransform = c.transform;

					Vector3 relativePos = playerTransform.position - shot.transform.position;
					Vector3 forward = shot.transform.right;
					var angle = Vector3.SignedAngle(relativePos.ReplaceZ(0), forward, shot.transform.forward);

					if (Math.Abs(angle) < Math.Abs(bestAngle))
					{
						bestAngle = angle;
					}
				}
				oldValue = shot.transform.localEulerAngles.z;
				oldValue = -(maxTurnRate / changeDuration * deltaTime - bestAngle);
				newValue = bestAngle;
				
				return base.DoShotStep(shot, deltaTime, out shouldBulletBeRemoved);
			}

			public class HomingResolver : JsonConverter
			{
				public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
				{
					HomingTurnModule v = (HomingTurnModule)value;
					JObject o = new JObject();
					o.Add(new JProperty("mod_type", CardLibrary.instance.GetModuleName(v.patternTypeData)));
					o.Add(new JProperty("maxTurnRate", v.maxTurnRate));
					o.WriteTo(writer);
				}

				public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
				{
					JObject jObject = JObject.Load(reader);
					string id = (string)jObject.GetValue("mod_type");
					PatternModuleType modType = CardLibrary.instance.GetModuleByName(id);
					if (modType == null) throw new JsonReaderException($"Unable to read {id}");
					HomingTurnModule runObj = (HomingTurnModule)modType.GetRuntimeObject();
					runObj.maxTurnRate = (float)jObject.GetValue("maxTurnRate");
					runObj.newValue = 0;
					return runObj;
				}

				public override bool CanConvert(Type objectType)
				{
					return typeof(HomingTurnModule).IsAssignableFrom(objectType);
				}

				public override bool CanRead => true;

				public override bool CanWrite => true;
			}
		}
	}
}
