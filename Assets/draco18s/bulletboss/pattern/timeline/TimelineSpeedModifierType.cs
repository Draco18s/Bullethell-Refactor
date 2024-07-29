using Assets.draco18s.bulletboss.entities;
using UnityEngine;

namespace Assets.draco18s.bulletboss.pattern.timeline
{
	[CreateAssetMenu(menuName = "Alterations/Timeline Speed")]
	public class TimelineSpeedModifierType : TimelineModifierType
	{
		[SerializeField] private float _modifier = 0.1f;
		public override void ApplyModifier_TimelinePreInit(Timeline timeline)
		{
			timeline.AddSpeedModifier(_modifier);
		}
	}
}
