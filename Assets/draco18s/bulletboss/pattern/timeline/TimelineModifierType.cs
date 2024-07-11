using Assets.draco18s.bulletboss.entities;
using UnityEngine;

namespace Assets.draco18s.bulletboss.pattern.timeline
{
	public abstract class TimelineModifierType : ScriptableObject
	{
		public enum ModuleType
		{
			Sprite, Damage, OnHit, OnSpawn
		}

		public string description;
		public NamedRarity rarity;
		public Sprite icon;
		public ModuleType moduleType;
		public bool applyRecursively;

		public virtual bool CanAddModule(Bullet shot, PatternModuleType module)
		{
			return true;
		}

		public abstract void ApplyModifier_TimelineInit(Bullet shot);
		public abstract void ApplyModifier_OnCollision(Bullet shot);
	}
}