using Assets.draco18s.bulletboss.pattern.timeline;
using System.Collections.Generic;
using Assets.draco18s.bulletboss.entities;
using Assets.draco18s.bulletboss.cards;
using Assets.draco18s.util;
using UnityEngine;

namespace Assets.draco18s.bulletboss.pattern
{
	[CreateAssetMenu(menuName = "New Grab Bag")]
	public class GrabBagModuleType : PatternGroupModuleType
	{
		public override PatternModule GetRuntimeObject()
		{
			return new GrabBagModule(this);
		}

		public class GrabBagModule : PatternGroup
		{
			protected GrabBagModuleType grabBagTypeData;
			protected PatternModule nextPattern;
			public GrabBagModule(GrabBagModuleType modType) : base(modType) { }

			public override PatternModule Clone()
			{
				GrabBagModule mod = new GrabBagModule(grabBagTypeData);
				mod.pattern = Timeline.CloneFrom(pattern);
				mod.pattern.InitOrReset();
				return mod;
			}

			public override bool DoShotStep(Bullet shot, float deltaTime, out bool shouldBulletBeRemoved)
			{
				shouldBulletBeRemoved = false;
				nextPattern.DoShotStep(shot, deltaTime, out bool b);
				shouldBulletBeRemoved |= b;
				return true;
			}

			public override void ResetForNewLoopIteration()
			{
				base.ResetForNewLoopIteration();
				KeyValuePair<int, Card> randKvp = childPattern.GetModules().GetRandom();
				nextPattern = randKvp.Value.pattern;
			}
		}
	}
}
