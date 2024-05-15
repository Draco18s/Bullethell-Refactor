using System;
using Assets.draco18s.bulletboss.entities;
using Assets.draco18s.util;
using UnityEngine;

namespace Assets.draco18s.bulletboss.pattern
{
	[CreateAssetMenu(menuName = "Pattern/Homing Change")]
	public class HomingTurnModuleType : ChangeModuleType
	{
		public HomingTurnModuleType()
		{
			changeType = ChangeType.Direction;
		}

		void OnValidate()
		{
			changeType = ChangeType.Direction;
		}

		public override PatternModule GetRuntimeObject()
		{
			return new HomingTurnModule(this);
		}

		public class HomingTurnModule : ChangeModule
		{
			protected HomingTurnModuleType homingTypeData;
			protected float maxTurnRate;
			public HomingTurnModule(HomingTurnModuleType modType) : base(modType)
			{
				homingTypeData = modType;
				maxTurnRate = newValue;
				newValue = 0;
			}

			public override PatternModule Clone()
			{
				HomingTurnModule mod = new HomingTurnModule(homingTypeData);
				mod.changeDuration = changeDuration;
				return mod;
			}

			public override bool DoShotStep(Bullet shot, float deltaTime, out bool shouldBulletBeRemoved)
			{
				float bestAngle = 180;
				foreach (Collider2D c in Physics2D.OverlapCircleAll(shot.transform.position, 10, shot.GetTargetLayerMask()))
				{
					Transform playerTransform = c.transform;

					Vector3 relativePos = playerTransform.position - shot.transform.position;
					Vector3 forward = shot.transform.right;
					var angle = Vector3.SignedAngle(relativePos.ReplaceZ(0), forward, shot.transform.forward);

					if (Math.Abs(angle) < Math.Abs(bestAngle))
					{
						bestAngle = angle;
					}
				}
				oldValue = shot.transform.localEulerAngles.z;
				oldValue = -(maxTurnRate / changeDuration * deltaTime - bestAngle);
				newValue = bestAngle;


				return base.DoShotStep(shot, deltaTime, out shouldBulletBeRemoved);
			}
		}
	}
}
