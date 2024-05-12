using Assets.draco18s.bulletboss.entities;
using Assets.draco18s.bulletboss.ui;
using Assets.draco18s.util;
using UnityEngine;
using Keyframe = Assets.draco18s.bulletboss.ui.Keyframe;

namespace Assets.draco18s.bulletboss.pattern
{
	[CreateAssetMenu(menuName = "New Pattern Loop")]
	public class LoopModuleType : TimelinePatternModuleType
	{
		[SerializeField] private int iterations;
		[SerializeField] private IntRange allowedValueRange;

		public override PatternModule GetRuntimeObject()
		{
			return new LoopModule(this);
		}

		public class LoopModule : TimelinePatternModule<LoopModuleType>
		{
			private int numLoops;
			private int loopCounter;
			public override bool hasEditableChild => !patternType.preconfiguredPattern || patternType.pattern.isEditable;
			public override float duration => childPattern.GetDuration() * numLoops;

			public LoopModule(LoopModuleType modType) : base(modType)
			{
				loopCounter = 0;
				numLoops = patternType.iterations;
				modType.pattern.DeserializeForRuntime();
			}

			public override PatternModule Clone()
			{
				LoopModule mod = new LoopModule(patternType);
				mod.numLoops = numLoops;
				return mod;
			}

			public override bool DoShotStep(Bullet shot, float deltaTime, out bool shouldBulletBeRemoved)
			{
				loopCounter++;
				shouldBulletBeRemoved = false;
				childPattern.RuntimeUpdate(shot, deltaTime);
				return loopCounter >= numLoops;
			}

			public override void ResetForNewLoopIteration()
			{
				loopCounter = 0;
				childPattern.ResetForNewLoopIteration();
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
