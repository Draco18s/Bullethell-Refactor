﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.draco18s.bulletboss.ui.map;
using UnityEngine;

namespace Assets.draco18s.bulletboss
{
	[CreateAssetMenu(fileName = "Location", menuName = "Map/Location")]
	public class LocationType : ScriptableObject
	{
		[SerializeField] private MapNodeType _nodeType;
		[SerializeField] private string _description;
		[SerializeField] private Sprite _icon;

		public MapNodeType nodeType => _nodeType;
		public string description => _description;
		public Sprite icon => _icon;

		public void DoRewards()
		{

		}
	}
}
