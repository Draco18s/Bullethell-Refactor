using Assets.draco18s.bulletboss.pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.draco18s.bulletboss.cards
{
	public class Card
	{
		public PatternModule pattern { get; protected set; }
		public bool isUnique { get; protected set; }
		public bool isEphemeral { get; protected set; }

		public Card(PatternModuleType moduleType, bool ephemeral=false)
		{
			pattern = moduleType.GetRuntimeObject();
			isUnique = moduleType.unique;
			isEphemeral = ephemeral;
		}

		public Card(PatternModule module, bool ephemeral = false)
		{
			pattern = module;
			isUnique = false;
			isEphemeral = ephemeral;
		}
	}
}
