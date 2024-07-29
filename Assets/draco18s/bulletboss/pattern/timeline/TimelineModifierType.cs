using Assets.draco18s.bulletboss.entities;
using UnityEngine;

namespace Assets.draco18s.bulletboss.pattern.timeline
{
	public abstract class TimelineModifierType : ScriptableObject
	{
		public enum ModuleType
		{
			Sprite, Color, Damage, OnHit, OnSpawn, Aim
		}

		public string description;
		public NamedRarity rarity;
		public Sprite icon;
		public ModuleType moduleType;
		public bool isUnique;
		public bool applyRecursively;

		public virtual bool CanAddModule(Bullet shot, PatternModuleType module)
		{
			return true;
		}

		public virtual void ApplyModifier_TimelinePreInit(Timeline timeline) { }

		public virtual void ApplyModifier_TimelineInit(Bullet shot) { }
		public virtual void ApplyModifier_OnCollision(Bullet shot) { }
		public virtual void ApplyModifier_OnUpdate(Bullet shot) { }
	}
}