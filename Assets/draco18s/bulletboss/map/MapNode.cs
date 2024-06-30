using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.draco18s.bulletboss.map
{
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
	}
}
