using Assets.draco18s.bulletboss.pattern;
using Assets.draco18s.bulletboss.pattern.timeline;
using UnityEngine;
using static Assets.draco18s.bulletboss.pattern.timeline.TimelineModifierType;

namespace Assets.draco18s.bulletboss.cards
{
	public class Card
	{
		public PatternModule pattern { get; protected set; }
		public TimelineModifierType timelineModifier { get; protected set; }
		public bool isUnique { get; protected set; }
		public bool isEphemeral { get; protected set; }
		public int count { get; protected set; }
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
			count = isEphemeral ? 1 : moduleType.playableCopies;
		}

		public Card(PatternModule module, bool ephemeral = false)
		{
			pattern = module;
			isUnique = module.patternTypeData.unique;
			isEphemeral = ephemeral;
			count = isEphemeral ? 1 : module.patternTypeData.playableCopies;
		}

		public Card(TimelineModifierType modifier, bool ephemeral = false)
		{
			timelineModifier = modifier;
			isUnique = modifier.isUnique;
			isEphemeral = ephemeral;
			count = 1;
		}

		public void SetEphemeral()
		{
			isEphemeral = true;
			count = 1;
		}

		public void SetDisabled(bool e)
		{
			isActive = e;
		}

		public void Reduce(int amt)
		{
			count -= amt;
		}
	}
}
