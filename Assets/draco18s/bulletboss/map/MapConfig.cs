using Assets.draco18s.util;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.draco18s.bulletboss.map
{
	[CreateAssetMenu(fileName = "MapConfig", menuName = "Map/Config")]
	public class MapConfig : ScriptableObject
	{
		public List<LocationType> nodeBlueprints;
		public int GridWidth => Mathf.Max(numOfPreBossNodes.max, numOfStartingNodes.max);
		
		public IntRange numOfPreBossNodes;
		public IntRange numOfStartingNodes;

		[Tooltip("Increase this number to generate more paths")]
		public int extraPaths;
		public List<MapLayer> layers;
	}
}
