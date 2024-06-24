using System;
using Assets.draco18s.bulletboss.entities;
using Assets.draco18s.bulletboss.pattern.timeline;
using Assets.draco18s.bulletboss.ui;
using Assets.draco18s.serialization;
using Assets.draco18s.util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Keyframe = Assets.draco18s.bulletboss.ui.Keyframe;

namespace Assets.draco18s.bulletboss.pattern
{
	public abstract class PatternModuleType : ScriptableObject
	{
		public NamedRarity rarity;
		public string description;
		public Sprite icon;
		public bool preconfigured;
		public bool unique = false;
		public bool killOnComplete = false;

		public abstract PatternModule GetRuntimeObject();
		public virtual bool CanAddModule(PatternModuleType module)
		{
			return true;
		}

		public virtual int GetMaxChildren()
		{
			return - 1;
		}
	}

	public abstract class TimelinePatternModuleType : PatternModuleType
	{
		[SerializeField] protected bool preconfiguredPattern;
		[SerializeField] protected Timeline pattern = new Timeline();
		[SerializeField] protected int maxObjects =-1;

		public bool GetIsPatternPreconfigured()
		{
			return preconfiguredPattern;
		}

		public Timeline GetPattern()
		{
			return pattern;
		}

		public override int GetMaxChildren()
		{
			return maxObjects;
		}
	}
	
	[Serializable]
	[JsonResolver(typeof(BaseResolver))]
	public abstract class PatternModule
	{
		[NonSerialized] public PatternModuleType patternTypeData;
		public virtual float duration => 0;
		public virtual bool hasEditableChild => false;
		public virtual Timeline childPattern => null;

		/// <summary>
		/// returns true if the action was completed
		/// </summary>
		public abstract bool DoShotStep(Bullet shot, float deltaTime, out bool shouldBulletBeRemoved);

		public abstract void ResetForNewLoopIteration();

		public abstract void ConfigureKeyframe(RectTransform keyframeBar, DraggableElement handle, Keyframe editableKeyframe);

		public abstract PatternModule Clone();

		public abstract PatternModuleType ExportAsScriptableObject();

		public class BaseResolver : JsonConverter
		{
			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				PatternModule v = (PatternModule)value;
				JObject o = new JObject();
				o.Add(new JProperty("mod_type", v.patternTypeData.name));
				o.WriteTo(writer);
			}

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
			{
				JObject jObject = JObject.Load(reader);
				string id = (string)jObject.GetValue("mod_type");
				PatternModuleType modType = CardLibrary.instance.GetModuleByName(id);
				if (modType == null) throw new JsonReaderException($"Unable to read {id}");
				PatternModule runObj = modType.GetRuntimeObject();
				
				return runObj;
			}

			public override bool CanConvert(Type objectType)
			{
				return typeof(PatternModule).IsAssignableFrom(objectType);
			}

			public override bool CanRead => true;

			public override bool CanWrite => true;
		}
	}

	public abstract class PatternModule<T> : PatternModule where T : PatternModuleType
	{
		public T patternType => (T)patternTypeData;

		protected PatternModule(T modType)
		{
			patternTypeData = modType;
		}
	}

	public abstract class TimelinePatternModule<U> : PatternModule<U> where U : TimelinePatternModuleType
	{
		[NonSerialized] protected Timeline pattern;
		public override bool hasEditableChild => !patternType.GetIsPatternPreconfigured() || pattern.isEditable;
		public override Timeline childPattern => pattern;

		protected TimelinePatternModule(U modType) : base(modType)
		{
			if (modType.GetIsPatternPreconfigured())
				pattern = Timeline.CloneFrom(patternType.GetPattern());
			else
				pattern = new Timeline(); 
			pattern.SetModuleType(this);
		}
	}
}