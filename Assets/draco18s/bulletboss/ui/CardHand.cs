using System.Collections;
using System.Collections.Generic;
using Assets.draco18s.bulletboss;
using Assets.draco18s.bulletboss.cards;
using Assets.draco18s.bulletboss.entities;
using Assets.draco18s.bulletboss.pattern;
using Assets.draco18s.bulletboss.ui;
using UnityEngine;

namespace Assets.draco18s.bulletboss.ui
{
	public class CardHand : MonoBehaviour
	{
		public static CardHand instance;
		
		void Awake()
		{
			instance = this;
		}
	}
}