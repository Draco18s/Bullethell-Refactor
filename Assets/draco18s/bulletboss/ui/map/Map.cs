using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.draco18s.bulletboss.ui.map
{
	public class Map
	{
		public List<MapNode> nodes;
		public List<Vector2Int> path;
		public string bossNodeName;
		public MapConfig config;

		public Map(MapConfig conf, string bossNodeName, List<MapNode> nodes)
		{
			config = conf;
			this.bossNodeName = bossNodeName;
			this.nodes = nodes;
			path = new List<Vector2Int>();
		}

		public MapNode GetBossNode()
		{
			return nodes.FirstOrDefault(n => n.locType.nodeType == MapNodeType.Boss);
		}

		public float DistanceBetweenFirstAndLastLayers()
		{
			var bossNode = GetBossNode();
			var firstLayerNode = nodes.FirstOrDefault(n => n.point.y == 0);

			if (bossNode == null || firstLayerNode == null)
				return 0f;

			return bossNode.position.y - firstLayerNode.position.y;
		}

		public MapNode GetNode(Vector2Int point)
		{
			return nodes.FirstOrDefault(n => n.point.Equals(point));
		}

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore
			});
		}
	}
}
