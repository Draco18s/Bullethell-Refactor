using System;
using System.Collections.Generic;
using Assets.draco18s.bulletboss.upgrades;

namespace Assets.draco18s.bulletboss
{
	[Serializable]
	public class PlayerProgress
	{
		public int collectedGems { get; protected set; } = 0;
		public float shipAILevel { get; protected set; } = 0;
		public float hullLevel { get; protected set; } = 0;
		public float armorLevel { get; protected set; } = 0;
		public float weaponLevel { get; protected set; } = 0;
		public float engineLevel { get; protected set; } = 0;
		public float damageLevel { get; protected set; } = 0;
		public float shieldLevel { get; protected set; } = 0;

		public IReadOnlyList<MysteryTechType> advancedTech => _advancedTech;
		private List<MysteryTechType> _advancedTech = new List<MysteryTechType>();

		private int expToLevel = 1_000;
		private bool lastCombatWasKilled;

		public void AddFinalGems(int gems, bool wasKilled)
		{
			collectedGems += gems >> (wasKilled ? 1 : 0);
			while (collectedGems >= expToLevel)
			{
				collectedGems -= expToLevel;
				weaponLevel++;
			}
		}

		public void OnCombatComplete(bool wasKilled)
		{
			lastCombatWasKilled = wasKilled;
			if (wasKilled)
				shipAILevel++;
		}

		public void AcquireItem(MysteryTechType tech)
		{
			_advancedTech.Add(tech);
		}

		public void AcquireItem(BasicTechType tech)
		{
			switch (tech.upgradeType)
			{
				case BasicTechType.EffectedSystem.Armor:
					armorLevel += tech.upgradeAmount;
					break;
				case BasicTechType.EffectedSystem.Hull:
					hullLevel += tech.upgradeAmount;
					break;
				case BasicTechType.EffectedSystem.Weapon:
					weaponLevel += tech.upgradeAmount;
					break;
				case BasicTechType.EffectedSystem.Speed:
					engineLevel += tech.upgradeAmount;
					break;
				case BasicTechType.EffectedSystem.Damage:
					damageLevel += tech.upgradeAmount;
					break;
				case BasicTechType.EffectedSystem.Shield:
					shieldLevel += tech.upgradeAmount;
					break;
			}
		}

		public void Rest()
		{
			if (lastCombatWasKilled)
				hullLevel++;
			else
				shipAILevel++;
		}
	}
}
