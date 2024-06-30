using Assets.draco18s.util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.draco18s.bulletboss.map
{
	public static class MapGenerator
	{
		private static readonly IEnumerable<MapNodeType> RandomNodes = Enum.GetValues(typeof(MapNodeType)).Cast<MapNodeType>();
		
		public static Map GenerateMap(MapConfig conf)
		{
			List<List<MapNode>> nodes = new List<List<MapNode>>();
			List<(float, float)> layerDistances = GenerateLayerDistances(conf);

			for (int i = 0; i < conf.layers.Count; i++)
				PlaceLayer(conf, layerDistances, i, nodes);

			RandomizeNodePositions(conf, layerDistances, nodes);

			SetUpConnections(conf, nodes);

			RemoveCrossConnections(conf, nodes);

			// select all the nodes with connections:
			List<MapNode> nodesList = nodes.SelectMany(n => n).Where(n => n.incoming.Count > 0 || n.outgoing.Count > 0).ToList();

			// pick a random name of the boss level for this map:
			var bossNodeName = conf.nodeBlueprints.Where(b => b.nodeType == MapNodeType.Boss).GetRandom().name;
			return new Map(conf, bossNodeName, nodesList);
		}

		private static List<(float, float)> GenerateLayerDistances(MapConfig config)
		{
			float lastDist = 0;
			List<(float, float)> layerDistances = new List<(float, float)>();
			foreach (MapLayer layer in config.layers)
			{
				float nextDist = layer.distanceFromPreviousLayer.GetValue();
				lastDist += nextDist;
				layerDistances.Add((nextDist, lastDist));
			}

			return layerDistances;
		}

		private static List<List<Vector2Int>> GeneratePaths(MapConfig config)
		{
			List<List<Vector2Int>> paths = new List<List<Vector2Int>>();
			Vector2Int finalNode = GetFinalNode(config);
			int numOfStartingNodes = config.numOfStartingNodes.GetValue();
			int numOfPreBossNodes = config.numOfPreBossNodes.GetValue();

			List<int> candidateXs = new List<int>();
			for (int i = 0; i < config.GridWidth; i++)
				candidateXs.Add(i);

			candidateXs.Shuffle();
			IEnumerable<int> startingXs = candidateXs.Take(numOfStartingNodes);
			List<Vector2Int> startingPoints = (from x in startingXs select new Vector2Int(x, 0)).ToList();

			candidateXs.Shuffle();
			IEnumerable<int> preBossXs = candidateXs.Take(numOfPreBossNodes);
			List<Vector2Int> preBossPoints = (from x in preBossXs select new Vector2Int(x, finalNode.y - 1)).ToList();

			int numOfPaths = Mathf.Max(numOfStartingNodes, numOfPreBossNodes) + Mathf.Max(0, config.extraPaths);
			for (int i = 0; i < numOfPaths; ++i)
			{
				Vector2Int startNode = startingPoints[i % numOfStartingNodes];
				Vector2Int endNode = preBossPoints[i % numOfPreBossNodes];
				List<Vector2Int> path = Path(startNode, endNode, config);
				path.Add(finalNode);
				paths.Add(path);
			}

			return paths;
		}

		private static Vector2Int GetFinalNode(MapConfig config)
		{
			int y = config.layers.Count - 1;
			return config.GridWidth % 2 == 1 || Random.Range(0, 2) == 0
				? new Vector2Int(config.GridWidth / 2, y)
				: new Vector2Int(config.GridWidth / 2 - 1, y);
		}

		private static void PlaceLayer(MapConfig config, List<(float dist, float sum)>  layerDistances, int layerIndex, List<List<MapNode>> nodes)
		{
			MapLayer layer = config.layers[layerIndex];
			List<MapNode> nodesOnThisLayer = new List<MapNode>();

			float offset = layer.nodesApartDistance * config.GridWidth / 2f;

			for (int i = 0; i < config.GridWidth; i++)
			{
				MapNodeType nodeType = Random.Range(0f, 1f) < layer.randomizeNodes ? RandomNodes.GetRandom() : layer.nodeType;
				LocationType blueprint = config.nodeBlueprints.Where(b => b.nodeType == nodeType).GetRandom();
				MapNode node = new MapNode(blueprint, new Vector2Int(i, layerIndex))
				{
					position = new Vector2(-offset + i * layer.nodesApartDistance, layerDistances[layerIndex].sum)
				};
				nodesOnThisLayer.Add(node);
			}

			nodes.Add(nodesOnThisLayer);
		}

		private static List<Vector2Int> Path(Vector2Int fromPoint, Vector2Int toPoint, MapConfig config)
		{
			int toRow = toPoint.y;
			int toCol = toPoint.x;

			int lastNodeCol = fromPoint.x;

			List<Vector2Int> path = new List<Vector2Int> { fromPoint };
			List<int> candidateCols = new List<int>();
			for (int row = 1; row < toRow; ++row)
			{
				candidateCols.Clear();

				int verticalDistance = toRow - row;
				int horizontalDistance;

				int forwardCol = lastNodeCol;
				horizontalDistance = Mathf.Abs(toCol - forwardCol);
				if (horizontalDistance <= verticalDistance)
					candidateCols.Add(lastNodeCol);

				int leftCol = lastNodeCol - 1;
				horizontalDistance = Mathf.Abs(toCol - leftCol);
				if (leftCol >= 0 && horizontalDistance <= verticalDistance)
					candidateCols.Add(leftCol);

				int rightCol = lastNodeCol + 1;
				horizontalDistance = Mathf.Abs(toCol - rightCol);
				if (rightCol < config.GridWidth && horizontalDistance <= verticalDistance)
					candidateCols.Add(rightCol);

				int randomCandidateIndex = Random.Range(0, candidateCols.Count);
				int candidateCol = candidateCols[randomCandidateIndex];
				Vector2Int nextPoint = new Vector2Int(candidateCol, row);

				path.Add(nextPoint);

				lastNodeCol = candidateCol;
			}

			path.Add(toPoint);

			return path;
		}

		private static void RandomizeNodePositions(MapConfig config, List<(float dist, float sum)> layerDistances, List<List<MapNode>> nodes)
		{
			for (int index = 0; index < nodes.Count; index++)
			{
				List<MapNode> list = nodes[index];
				MapLayer layer = config.layers[index];
				float distToNextLayer = index + 1 >= layerDistances.Count
					? 0f
					: layerDistances[index + 1].dist;
				float distToPreviousLayer = layerDistances[index].dist;

				foreach (MapNode node in list)
				{
					float xRnd = Random.Range(-1f, 1f);
					float yRnd = Random.Range(-1f, 1f);

					float x = xRnd * layer.nodesApartDistance / 2f;
					float y = yRnd < 0 ? distToPreviousLayer * yRnd / 2f : distToNextLayer * yRnd / 2f;

					node.position += new Vector2(x, y) * layer.randomizePosition;
				}
			}
		}

		private static void SetUpConnections(MapConfig config, List<List<MapNode>> nodes)
		{
			foreach (List<Vector2Int> path in GeneratePaths(config))
			{
				for (int i = 0; i < path.Count - 1; ++i)
				{
					MapNode node = nodes.GetNode(path[i]);
					MapNode nextNode = nodes.GetNode(path[i + 1]);
					node.AddOutgoing(nextNode.point);
					nextNode.AddIncoming(node.point);
				}
			}
		}

		private static void RemoveCrossConnections(MapConfig config, List<List<MapNode>> nodes)
		{
			for (int i = 0; i < config.GridWidth - 1; ++i)
			for (int j = 0; j < config.layers.Count - 1; ++j)
			{
				MapNode node = nodes.GetNode(new Vector2Int(i, j));
				if (node == null || node.HasNoConnections()) continue;
				MapNode right = nodes.GetNode(new Vector2Int(i + 1, j));
				if (right == null || right.HasNoConnections()) continue;
				MapNode top = nodes.GetNode(new Vector2Int(i, j + 1));
				if (top == null || top.HasNoConnections()) continue;
				MapNode topRight = nodes.GetNode(new Vector2Int(i + 1, j + 1));
				if (topRight == null || topRight.HasNoConnections()) continue;

				if (!node.outgoing.Any(element => element.Equals(topRight.point))) continue;
				if (!right.outgoing.Any(element => element.Equals(top.point))) continue;

				// we managed to find a cross node:
				// 1) add direct connections:
				node.AddOutgoing(top.point);
				top.AddIncoming(node.point);

				right.AddOutgoing(topRight.point);
				topRight.AddIncoming(right.point);

				float rnd = Random.Range(0f, 1f);
				if (rnd < 0.2f)
				{
					// remove both:
					// a) 
					node.RemoveOutgoing(topRight.point);
					topRight.RemoveIncoming(node.point);
					// b) 
					right.RemoveOutgoing(top.point);
					top.RemoveIncoming(right.point);
				}
				else if (rnd < 0.6f)
				{
					// remove just
					// a) 
					node.RemoveOutgoing(topRight.point);
					topRight.RemoveIncoming(node.point);
				}
				else
				{
					// remove just
					// b) 
					right.RemoveOutgoing(top.point);
					top.RemoveIncoming(right.point);
				}
			}
		}

		private static MapNode GetNode(this List<List<MapNode>> nodes, Vector2Int p)
		{
			if (p.y >= nodes.Count) return null;
			if (p.x >= nodes[p.y].Count) return null;

			return nodes[p.y][p.x];
		}
	}
}
