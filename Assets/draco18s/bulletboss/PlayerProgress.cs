using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.draco18s.bulletboss
{
	[Serializable]
	public class PlayerProgress
	{
		public int shipAILevel { get; protected set; } = 0;
		public int collectedGems { get; protected set; } = 0;
		public int bonusTech { get; protected set; } = 0;
		public int weaponLevel { get; protected set; } = 0;

		private int expToLevel = 1000;

		public void AddFinalGems(int gems, bool wasKilled)
		{
			collectedGems += gems >> (wasKilled ? 1 : 0);
			while (collectedGems >= expToLevel)
			{
				collectedGems -= expToLevel;
				weaponLevel++;
			}
		}

		public void FoundTechItem()
		{
			bonusTech++;
		}

		public void OnCombatComplete(bool wasKilled)
		{
			if (wasKilled)
				shipAILevel++;
		}
	}
}
