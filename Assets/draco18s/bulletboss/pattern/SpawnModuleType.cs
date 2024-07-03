using System;
using Assets.draco18s.bulletboss.entities;
using Assets.draco18s.bulletboss.pattern.timeline;
using Assets.draco18s.bulletboss.ui;
using Assets.draco18s.serialization;
using Assets.draco18s.util;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine;
using Keyframe = Assets.draco18s.bulletboss.ui.Keyframe;

namespace Assets.draco18s.bulletboss.pattern
{
	[CreateAssetMenu(menuName = "Pattern/New Spawn Pattern")]
	public class SpawnModuleType : TimelinePatternModuleType
	{
		[SerializeField] private float duration;
		[SerializeField] private GameObject prefab;
		[SerializeField] private bool killParent;
		[SerializeField] private bool followParent;
		[SerializeField] private float initialAngle;
		[SerializeField] private FloatRange angleLimit;

		private static int spawnCount = 0;

		public override PatternModule GetRuntimeObject()
		{
			return new SpawnModule(this);
		}

		[JsonResolver(typeof(Resolver))]
		public class SpawnModule : TimelinePatternModule<SpawnModuleType>
		{
			public override float duration => patternType.duration;
			private bool spawned = false;
			private float spawnAngle = 0;

			public SpawnModule(SpawnModuleType modType) : base(modType)
			{
				spawned = false;
				spawnAngle = patternType.initialAngle;
			}

			public override PatternModule Clone()
			{
				SpawnModule mod = new SpawnModule(patternType);
				mod.spawnAngle = spawnAngle;
				return mod;
			}
			
			public override PatternModuleType ExportAsScriptableObject()
			{
				SpawnModuleType result = CreateInstance<SpawnModuleType>();
				result.name = patternType.name;
				result.preconfigured = result.preconfiguredPattern = true;
				result.pattern = Timeline.CloneForAsset(pattern);
				result.initialAngle = spawnAngle;
				result.angleLimit = patternType.angleLimit;
				result.duration = patternType.duration;
				result.killParent = patternType.killParent;
				result.prefab = patternType.prefab;
				return result;
			}

			public override bool DoShotStep(Bullet shot, float deltaTime, out bool shouldBulletBeRemoved)
			{
				shouldBulletBeRemoved = patternType.killParent;
				SpawnNewBullet(shot, deltaTime, childPattern, patternType.followParent);
				return true;
			}

			private void SpawnNewBullet(Bullet parentShot, float deltaTime, Timeline timeline, bool followParent)
			{
				if (spawned) return;

				Quaternion q = Quaternion.Euler(0,0, spawnAngle);

				GameObject go = Instantiate(patternType.prefab, parentShot.transform.position, parentShot.transform.rotation * q, GameManager.instance.bulletParentContainer);
				Bullet shot = go.GetComponent<Bullet>();
				shot.SetPattern(timeline);
				if(followParent)
					shot.SetParent(parentShot);
				shot.gameObject.layer = (parentShot.gameObject.layer - (parentShot.gameObject.layer % 2)) + 1;
				spawned = true;
				spawnCount++;
				shot.gameObject.name += " - " + spawnCount;
			}

			public override void ResetForNewLoopIteration()
			{
				spawned = false;
			}

			public override void ConfigureKeyframe(RectTransform keyframeBar, DraggableElement handle, Keyframe editableKeyframe)
			{
				childPattern.InitOrReset();
				editableKeyframe.SetEditableType(Keyframe.EditTypes.Angular, patternType.angleLimit, spawnAngle, true, 1, UpdateSpawnAngle);
			}

			private void UpdateSpawnAngle(float newAngle)
			{
				spawnAngle = newAngle;
			}

			public class Resolver : JsonConverter
			{
				public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
				{
					SpawnModule v = (SpawnModule)value;
					JObject o = new JObject();
					o.Add(new JProperty("mod_type", v.patternTypeData.name));
					o.Add(new JProperty("timeline", JsonConvert.SerializeObject(v.pattern, ContractResolver.jsonSettings)));
					o.Add(new JProperty("spawnAngle", v.spawnAngle));
					o.WriteTo(writer);
				}

				public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
				{
					JObject jObject = JObject.Load(reader);
					string id = (string)jObject.GetValue("mod_type");
					PatternModuleType modType = CardLibrary.instance.GetModuleByName(id);
					if (modType == null) throw new JsonReaderException($"Unable to read {id}");
					SpawnModule runObj = (SpawnModule)modType.GetRuntimeObject();

					string timelineJson = (string)jObject.GetValue("timeline");
					runObj.pattern = JsonConvert.DeserializeObject<Timeline>(timelineJson ?? "");
					runObj.spawnAngle = (int)jObject.GetValue("spawnAngle");
					return runObj;
				}

				public override bool CanConvert(Type objectType)
				{
					return typeof(SpawnModule).IsAssignableFrom(objectType);
				}

				public override bool CanRead => true;

				public override bool CanWrite => true;
			}
		}
	}
}
