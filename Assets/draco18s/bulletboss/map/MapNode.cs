using System;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Assets.draco18s.serialization;
using UnityEngine;
using Assets.draco18s.bulletboss.cards;
using Assets.draco18s.bulletboss.pattern.timeline;
using Assets.draco18s.bulletboss.pattern;
using Newtonsoft.Json.Linq;

namespace Assets.draco18s.bulletboss.map
{
	[JsonResolver(typeof(Converter))]
	public class MapNode
	{
		public readonly Vector2Int point;
		public readonly List<Vector2Int> incoming = new List<Vector2Int>();
		public readonly List<Vector2Int> outgoing = new List<Vector2Int>();
		[JsonConverter(typeof(StringEnumConverter))]
		public readonly LocationType locType;
		public Vector2 position;

		public MapNode(LocationType locType, Vector2Int point)
		{
			this.locType = locType;
			this.point = point;
		}

		public void AddIncoming(Vector2Int p)
		{
			if (incoming.Any(element => element.Equals(p)))
				return;

			incoming.Add(p);
		}

		public void AddOutgoing(Vector2Int p)
		{
			if (outgoing.Any(element => element.Equals(p)))
				return;

			outgoing.Add(p);
		}

		public void RemoveIncoming(Vector2Int p)
		{
			incoming.RemoveAll(element => element.Equals(p));
		}

		public void RemoveOutgoing(Vector2Int p)
		{
			outgoing.RemoveAll(element => element.Equals(p));
		}

		public bool HasNoConnections()
		{
			return incoming.Count == 0 && outgoing.Count == 0;
		}

		public class Converter : JsonConverter
		{
			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				if (value == null) return;

				MapNode v = (MapNode)value;
				JObject o = new JObject();

				o.Add(new JProperty("locType", v.locType.name));
				o.Add(new JProperty("point", JToken.FromObject(v.point)));
				o.Add(new JProperty("position", JToken.FromObject(v.position)));
				o.Add(new JProperty("incoming", new JArray(v.incoming.Select(n => JToken.FromObject(n)))));
				o.Add(new JProperty("outgoing", new JArray(v.outgoing.Select(n => JToken.FromObject(n)))));

				o.WriteTo(writer);
			}

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
			{
				JObject jObject = JObject.Load(reader);
				string loc = jObject.GetValue("locType").Value<string>();
				LocationType locType = GameManager.instance.CurrentMapConfig.nodeBlueprints.First(b => b.name == loc);
				Vector2Int p = jObject.GetValue("point").Value<Vector2Int>();//.Select(t => t.Value<MapNode>())
				IEnumerable<Vector2Int> inc = jObject.GetValue("incoming").Value<JArray>().Select(t => t.Value<Vector2Int>()).ToList();
				IEnumerable<Vector2Int> onc = jObject.GetValue("outgoing").Value<JArray>().Select(t => t.Value<Vector2Int>());

				MapNode runObj = new MapNode(locType, p);
				runObj.incoming.AddRange(inc);
				runObj.outgoing.AddRange(onc);
				runObj.position = jObject.GetValue("position").Value<Vector3>();

				return runObj;
			}

			public override bool CanConvert(Type objectType)
			{
				return typeof(MapNode).IsAssignableFrom(objectType);
			}
		}
	}
}
