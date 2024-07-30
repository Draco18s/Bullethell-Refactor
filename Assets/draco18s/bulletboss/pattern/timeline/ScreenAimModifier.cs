using Assets.draco18s.bulletboss.entities;
using UnityEngine;

namespace Assets.draco18s.bulletboss.pattern.timeline
{
	[CreateAssetMenu(menuName = "Alterations/Screen Aim")]
	public class ScreenAimModifier : TimelineModifierType
	{
		[SerializeField] private float _direction = -90;

		public override void ApplyModifier_TimelineInit(Bullet shot)
		{
			shot.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, _direction));
		}
	}
}
