﻿using System;
using Assets.draco18s.bulletboss.map;
using UnityEngine;

namespace Assets.draco18s.bulletboss.map
{
	[CreateAssetMenu(fileName = "Location", menuName = "Map/Location")]
	public class LocationType : ScriptableObject
	{
		[SerializeField] private MapNodeType _nodeType;
		[SerializeField] private string _description;
		[SerializeField] private Sprite _icon;
		[SerializeField] private BasicTechConfig _techConfig;

		public MapNodeType nodeType => _nodeType;
		public BasicTechConfig techConfig => _techConfig;
		public string description => _description;
		public Sprite icon => _icon;

		public void DoRewards(MapNode node)
		{
			switch (_nodeType)
			{
				case MapNodeType.Mystery:
				case MapNodeType.Treasure:
				case MapNodeType.RestSite:
				case MapNodeType.Store:
					GameManager.instance.DoEvent(node);
					break;
				case MapNodeType.NormalEncounter:
				case MapNodeType.FleetEncounter:
				case MapNodeType.Boss:
				default:
					GameManager.instance.StartNewCombat(node);
					break;
			}
		}
	}
}
