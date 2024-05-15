using Assets.draco18s.bulletboss.entities;
using UnityEngine;
using static Assets.draco18s.bulletboss.entities.Bullet;

namespace Assets.draco18s.bulletboss.pattern.timeline
{
	[CreateAssetMenu(menuName = "Alterations/Sprite Replacement")]
	public class SpriteModifierType : TimelineModifierType
	{
		private Sprite replacement => icon;
		[SerializeField] private BulletShape allowedBulletShape;
		[SerializeField] private BulletSize allowedBulletSize;

		public override bool CanAddModule(Bullet shot, PatternModuleType module)
		{
			return shot.bulletSize == allowedBulletSize && shot.bulletShape == allowedBulletShape;
		}

		public override void ApplyModifier(Bullet shot)
		{
			shot.SetSprite(replacement);
		}
	}
}
