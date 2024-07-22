using Assets.draco18s.bulletboss.pattern;
using Assets.draco18s.bulletboss.pattern.timeline;
using UnityEngine;

namespace Assets.draco18s.bulletboss.cards
{
	public class Card
	{
		public PatternModule pattern { get; protected set; }
		public TimelineModifierType timelineModifier { get; protected set; }
		public bool isUnique { get; protected set; }
		public bool isEphemeral { get; protected set; }
		public bool isActive { get; protected set; } = true;
		public NamedRarity rarity => pattern?.patternTypeData.rarity ?? timelineModifier.rarity;
		public string name => pattern?.patternTypeData.name ?? timelineModifier.name;
		public string description => pattern?.patternTypeData.description ?? timelineModifier.description;
		public Sprite icon => pattern?.patternTypeData.icon ?? timelineModifier.icon;

		public Card(PatternModuleType moduleType, bool ephemeral=false)
		{
			pattern = moduleType.GetRuntimeObject();
			isUnique = moduleType.unique;
			isEphemeral = ephemeral;
		}

		public Card(PatternModule module, bool ephemeral = false)
		{
			pattern = module;
			isUnique = module.patternTypeData.unique;
			isEphemeral = ephemeral;
		}

		public Card(TimelineModifierType modifier, bool ephemeral = false)
		{
			timelineModifier = modifier;
			isUnique = modifier.isUnique;
			isEphemeral = ephemeral;
		}

		public void SetEphemeral()
		{
			isEphemeral = true;
		}

		public void SetDisabled(bool e)
		{
			isActive = e;
		}
	}
}
