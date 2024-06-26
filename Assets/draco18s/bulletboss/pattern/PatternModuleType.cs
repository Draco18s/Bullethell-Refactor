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