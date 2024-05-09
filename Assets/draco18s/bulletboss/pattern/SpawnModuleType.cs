using Assets.draco18s.bulletboss.entities;
using Assets.draco18s.bulletboss.pattern.timeline;
using Assets.draco18s.bulletboss.ui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.draco18s.util;
using UnityEngine;
using Keyframe = Assets.draco18s.bulletboss.ui.Keyframe;

namespace Assets.draco18s.bulletboss.pattern
{
	[CreateAssetMenu(menuName = "New Spawn Pattern")]
	public class SpawnModuleType : TimelinePatternModuleType
	{
		[SerializeField] private GameObject prefab;
		[SerializeField] private bool killParent;
		[SerializeField] private bool followParent;
		[SerializeField] private float initialAngle;

		public override PatternModule GetRuntimeObject()
		{
			return new SpawnModule(this);
		}

		public class SpawnModule : TimelinePatternModule<SpawnModuleType>
		{
			public override float duration => 1f;
			private bool spawned = false;
			private float spawnAngle = 0;

			public SpawnModule(SpawnModuleType modType) : base(modType)
			{
				spawned = false;
				spawnAngle = patternType.initialAngle;
			}

			public override PatternModule Clone()
			{
				SpawnModule mod = new SpawnModule(patternType);
				mod.spawnAngle = spawnAngle;
				return mod;
			}

			public override bool DoShotStep(Bullet shot, float deltaTime, out bool shouldBulletBeRemoved)
			{
				shouldBulletBeRemoved = patternType.killParent;
				SpawnNewBullet(shot, deltaTime, childPattern, patternType.followParent);
				return true;
			}

			private void SpawnNewBullet(Bullet parentShot, float deltaTime, Timeline timeline, bool followParent)
			{
				if (spawned) return;

				Quaternion q = Quaternion.Euler(0,0, spawnAngle);

				GameObject go = Instantiate(patternType.prefab, parentShot.transform.position, parentShot.transform.rotation * q, parentShot.transform.parent);
				Bullet shot = go.GetComponent<Bullet>();
				shot.SetPattern(timeline);
				if(followParent)
					shot.SetParent(parentShot);
				spawned = true;
			}

			public override void ResetForNewLoopIteration()
			{
				spawned = false;
			}

			public override void ConfigureKeyframe(RectTransform keyframeBar, DraggableElement handle, Keyframe editableKeyframe)
			{
				childPattern.InitOrReset();
				editableKeyframe.SetEditableType(Keyframe.EditTypes.Angular, new FloatRange(-180,180), spawnAngle, true, 1, UpdateSpawnAngle);
			}

			private void UpdateSpawnAngle(float newAngle)
			{
				spawnAngle = newAngle;
			}
		}
	}
}
