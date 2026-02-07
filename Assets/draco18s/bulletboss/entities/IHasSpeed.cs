using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.draco18s.bulletboss.entities
{
	internal interface IHasSpeed
	{
		float Speed { get; }

		void SpecialSetup(float rot, float spd);
	}
}
