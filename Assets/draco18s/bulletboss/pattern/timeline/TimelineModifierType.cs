using Assets.draco18s.bulletboss.entities;
using UnityEngine;

namespace Assets.draco18s.bulletboss.pattern.timeline
{
	public abstract class TimelineModifierType : ScriptableObject
	{
		public enum ModuleType
		{
			Sprite
		}

		public string description;
		public NamedRarity rarity;
		public Sprite icon;
		public ModuleType moduleType;

		public virtual bool CanAddModule(Bullet shot, PatternModuleType module)
		{
			return true;
		}

		public abstract void ApplyModifier(Bullet shot);
	}
}