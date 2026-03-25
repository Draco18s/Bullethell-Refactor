using System.Linq;
using Assets.draco18s.bulletboss.entities;
using UnityEngine;

namespace Assets.draco18s.bulletboss.pattern.timeline
{
	[CreateAssetMenu(menuName = "Alterations/Player Aim")]
	public class PlayerAimModifier : TimelineModifierType
	{
		public override void ApplyModifier_TimelineInit(Bullet shot)
		{
			PlayerAgent[] players = GameObject.FindObjectsByType<PlayerAgent>(FindObjectsSortMode.None);
			PlayerAgent player = players.FirstOrDefault(p => p.transform.parent.parent == shot.transform.parent.parent) ?? players.FirstOrDefault();
			shot.transform.LookAt(player.transform);

			shot.transform.localEulerAngles = new Vector3(0, 0, shot.transform.localEulerAngles.x);
		}
	}
}
