using System;
using Assets.draco18s.bulletboss.pattern.timeline;
using System.Collections.Generic;
using Assets.draco18s.bulletboss.entities;
using Assets.draco18s.bulletboss.cards;
using Assets.draco18s.util;
using UnityEngine;

namespace Assets.draco18s.bulletboss.pattern
{
	[CreateAssetMenu(menuName = "Pattern/New Grab Bag")]
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
			public override float duration => nextPattern?.duration ?? 1f;

			public GrabBagModule(GrabBagModuleType modType) : base(modType)
			{
				grabBagTypeData = modType;
			}

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
				if (nextPattern == null)
					ResetForNewLoopIteration();
				if (nextPattern == null)
				{
					return true;
				}
				nextPattern.DoShotStep(shot, deltaTime, out bool b);
				shouldBulletBeRemoved |= b;
				return true;
			}

			public override void ResetForNewLoopIteration()
			{
				base.ResetForNewLoopIteration();
				KeyValuePair<int, Card> randKvp;
				do
				{
					randKvp = childPattern.GetModules().GetRandom();

					// if there's only two options (or fewer), heck trying to pick a non-duplicate
				} while (childPattern.GetModules().Count > 2 && randKvp.Value?.pattern == nextPattern);
				nextPattern = randKvp.Value?.pattern;
			}
		}
	}
}
