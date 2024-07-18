using Assets.draco18s.bulletboss.entities;
using UnityEngine;
using static Assets.draco18s.bulletboss.entities.Bullet;

namespace Assets.draco18s.bulletboss.pattern.timeline
{
	[CreateAssetMenu(menuName = "Alterations/Sprite Color")]
	public class ColorModifierType : TimelineModifierType
	{
		[SerializeField] private Color _color;
		private Color _runtimeColor;

		public override void ApplyModifier_TimelinePreInit(Timeline timeline)
		{
			
		}

		public override void ApplyModifier_TimelineInit(Bullet shot)
		{
			shot.SetSpriteColor(_color);
		}

		public override void ApplyModifier_OnCollision(Bullet shot)
		{

		}
	}
}