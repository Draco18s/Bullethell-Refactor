using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.draco18s.bulletboss.entities;
using UnityEngine;

namespace Assets.draco18s.bulletboss.pattern.timeline
{
	[CreateAssetMenu(menuName = "Alterations/Mouse Aim")]
	public class MouseAimModifier : TimelineModifierType
	{
		public override void ApplyModifier_OnUpdate(Bullet shot)
		{
			Vector3 scPt = Camera.main.WorldToScreenPoint(shot.transform.position);
			Quaternion nRot = Quaternion.LookRotation((scPt - Input.mousePosition).normalized, Vector3.right);
			shot.transform.rotation = Quaternion.Euler(new Vector3(0, 0, (nRot.eulerAngles.y > 270 ? nRot.eulerAngles.x : 180 - nRot.eulerAngles.x)));
		}
	}
}
