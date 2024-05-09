using Assets.draco18s.bulletboss.pattern.timeline;
using Assets.draco18s.util;
using System;
using System.Collections.Generic;
using Assets.draco18s.bulletboss.cards;
using Assets.draco18s.bulletboss.entities;
using Assets.draco18s.bulletboss.ui;
using UnityEngine;
using Keyframe = Assets.draco18s.bulletboss.ui.Keyframe;

namespace Assets.draco18s.bulletboss.pattern
{
	[CreateAssetMenu(menuName = "New Pattern Group")]
	public class PatternGroupModule : TimelinePatternModuleType
	{
		[SerializeField] private IntRange allowedValueRange;

		public override PatternModule GetRuntimeObject()
		{
			return new PatternGroup(this);
		}

		public override bool CanAddModule(PatternModuleType module)
		{
			return module is SpawnModuleType;
		}

		public class PatternGroup : TimelinePatternModule<PatternGroupModule>
		{
			public override float duration => 1f;

			public PatternGroup(PatternGroupModule patternGroupModule) : base(patternGroupModule) { }

			public override bool DoShotStep(Bullet shot, float deltaTime, out bool shouldBulletBeRemoved)
			{
				shouldBulletBeRemoved = false;
				foreach (KeyValuePair<int, Card> grp in childPattern.GetModules())
				{
					if(!(grp.Value.pattern.patternTypeData is SpawnModuleType)) continue;
					grp.Value.pattern.DoShotStep(shot, deltaTime, out bool b);
					shouldBulletBeRemoved |= b;
				}
				return true;
			}

			public override PatternModule Clone()
			{
				PatternGroup mod = new PatternGroup(patternType);
				mod.pattern = Timeline.CloneFrom(pattern);
				mod.pattern.InitOrReset();
				return mod;
			}

			public override void ResetForNewLoopIteration()
			{
				foreach (KeyValuePair<int, Card> grp in childPattern.GetModules())
					grp.Value.pattern.ResetForNewLoopIteration();
			}

			public override void ConfigureKeyframe(RectTransform keyframeBar, DraggableElement handle, Keyframe editableKeyframe)
			{
				childPattern.InitOrReset();
				handle.SetValue(duration);
				handle.Disable();
			}
		}
	}
}
