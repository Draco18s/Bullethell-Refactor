using Assets.draco18s.bulletboss.entities;
using UnityEngine;
using static Assets.draco18s.bulletboss.entities.Bullet;

namespace Assets.draco18s.bulletboss.pattern.timeline
{
	[CreateAssetMenu(menuName = "Alterations/Sprite Replacement")]
	public class SpriteModifierType : TimelineModifierType
	{
		[SerializeField] private GameObject prefab;

		public override void ApplyModifier_TimelinePreInit(Timeline timeline)
		{
			timeline.SetBulletPrefab(prefab);
		}
	}
}
