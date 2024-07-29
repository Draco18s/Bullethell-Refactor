﻿using Assets.draco18s.bulletboss.entities;
using UnityEngine;

namespace Assets.draco18s.bulletboss.pattern.timeline
{
	[CreateAssetMenu(menuName = "Alterations/Sprite Color")]
	public class ColorModifierType : TimelineModifierType
	{
		[SerializeField] private Color _color;
		private Color _runtimeColor;
		
		public override void ApplyModifier_TimelineInit(Bullet shot)
		{
			shot.SetSpriteColor(_color);
		}
	}
}