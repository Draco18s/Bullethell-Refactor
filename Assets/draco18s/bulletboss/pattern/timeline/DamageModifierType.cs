using Assets.draco18s.bulletboss.entities;
using UnityEngine;
using static Assets.draco18s.bulletboss.entities.Bullet;

namespace Assets.draco18s.bulletboss.pattern.timeline
{
	[CreateAssetMenu(menuName = "Alterations/Damage Modifier")]
	public class DamageModifierType : TimelineModifierType
	{
		private Sprite replacement => icon;
		[SerializeField] private int damageIncreaseAmount = 1;

		public override bool CanAddModule(Bullet shot, PatternModuleType module)
		{
			return true;
		}

		public override void ApplyModifier(Bullet shot)
		{
			shot.Damage += damageIncreaseAmount;
		}
	}
}
