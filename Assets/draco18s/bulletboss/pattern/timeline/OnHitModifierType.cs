using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.draco18s.bulletboss.entities;
using UnityEngine;

namespace Assets.draco18s.bulletboss.pattern.timeline
{
	[CreateAssetMenu(menuName = "Alterations/On Hit Effect")]
	public class OnHitModifierType : TimelineModifierType
	{
		private static int spawnCount = 0;
		
		[SerializeField] private GameObject prefab;
		[SerializeField] private Timeline pattern;
		[SerializeField] private float initialAngle;

		public override bool CanAddModule(Bullet shot, PatternModuleType module)
		{
			return module.GetType() == typeof(SpawnModuleType);
		}

		public override void ApplyModifier_TimelineInit(Bullet shot)
		{
			
		}

		public override void ApplyModifier_OnCollision(Bullet shot)
		{
			SpawnNewBullet(shot, pattern);
		}

		private void SpawnNewBullet(Bullet parentShot, Timeline timeline)
		{
			Quaternion q = Quaternion.Euler(0, 0, initialAngle);

			GameObject go = Instantiate(prefab, parentShot.transform.position, parentShot.transform.rotation * q, GameManager.instance.bulletParentContainer);
			Bullet shot = go.GetComponent<Bullet>();
			
			shot.SetPattern(timeline);
			shot.gameObject.layer = (parentShot.gameObject.layer - (parentShot.gameObject.layer % 2)) + 1;
			
			spawnCount++;
			shot.gameObject.name += " - " + spawnCount;
		}
	}
}
