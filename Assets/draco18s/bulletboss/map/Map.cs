using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Assets.draco18s.serialization;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System;

namespace Assets.draco18s.bulletboss.map
{
	[JsonResolver(typeof(Converter))]
	public class Map
	{
		public readonly List<MapNode> nodes;
		public readonly List<Vector2Int> path;
		public readonly string bossNodeName;
		public readonly MapConfig config;

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

		public void AddPath(MapNode node)
		{
			path.Add(node.point);
		}

		public class Converter : JsonConverter
		{
			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				if (value == null) return;

				Map v = (Map)value;
				JObject o = new JObject();
				o.Add(new JProperty("bossNodeName", v.bossNodeName));
				o.Add(new JProperty("path", new JArray(v.path.Select(n => JToken.FromObject(n)))));
				o.Add(new JProperty("nodes", new JArray(v.nodes.Select(n => JToken.FromObject(n)))));
				o.Add(new JProperty("config", v.config.name));
				
				o.WriteTo(writer);
			}

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
			{
				JObject jObject = JObject.Load(reader);
				string b = jObject.GetValue("bossNodeName").Value<string>();
				List<MapNode> l = jObject.GetValue("nodes").Value<JArray>().Select(t => t.Value<MapNode>()).ToList();
				IEnumerable<Vector2Int> p = jObject.GetValue("path").Value<JArray>().Select(t => t.Value<Vector2Int>());
				Map runObj = new Map(GameManager.instance.CurrentMapConfig, b,l);
				runObj.path.AddRange(p);

				return runObj;
			}

			public override bool CanConvert(Type objectType)
			{
				return typeof(Map).IsAssignableFrom(objectType);
			}
		}
	}
}
