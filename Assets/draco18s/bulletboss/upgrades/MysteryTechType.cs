using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.draco18s.bulletboss.entities;
using UnityEngine;

namespace Assets.draco18s.bulletboss.upgrades
{
	[CreateAssetMenu(menuName = "Upgrades/Mystery Tech")]
	public class MysteryTechType : ScriptableObject
	{
		[SerializeField] private string description;
		[SerializeField] private Sprite icon;
		[SerializeField] private NamedRarity rarity;

		public void ApplyItem(Player aiPlayer, MysteryTechType tech)
		{

		}
	}
}
