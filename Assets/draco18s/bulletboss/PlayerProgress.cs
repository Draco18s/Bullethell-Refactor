using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

		private int expToLevel = 1000;
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
			switch (tech.upgradeType)
			{
				case MysteryTechType.TechUpgradeType.Armor:
					armorLevel += tech.upgradeAmount;
					break;
				case MysteryTechType.TechUpgradeType.Hull:
					hullLevel += tech.upgradeAmount;
					break;
				case MysteryTechType.TechUpgradeType.Weapon:
					weaponLevel += tech.upgradeAmount;
					break;
				case MysteryTechType.TechUpgradeType.Speed:
					engineLevel += tech.upgradeAmount;
					break;
				case MysteryTechType.TechUpgradeType.Damage:
					damageLevel += tech.upgradeAmount;
					break;
				case MysteryTechType.TechUpgradeType.Shield:
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
