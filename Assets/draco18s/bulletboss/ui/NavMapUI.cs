using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.draco18s.bulletboss.map;
using Assets.draco18s.ui;
using Assets.draco18s.util;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.draco18s.bulletboss.ui
{
	public class NavMapUI : MonoBehaviour
	{
		public static NavMapUI instance;
		[SerializeField] private GameObject mapNodePrefab;
		[SerializeField] private ScrollRect scrollView;
		[SerializeField] private Transform nodeContainer;

		void Start()
		{
			instance = this;
			nodeContainer.Clear();
			scrollView.verticalNormalizedPosition = 0;
			//StartCoroutine(Wait());
		}

		void Update()
		{
			Debug.Log(scrollView.verticalNormalizedPosition);
		}

		public void UpdateMap(Map map)
		{
			MapNodeUI[] nodes = FindObjectsOfType<MapNodeUI>();
			if (map.path.Count == 0) return;
			foreach (MapNodeUI node in nodes)
			{
				node.SetAvailable(node.node.incoming.Contains(map.path[^1]));
			}
			scrollView.verticalNormalizedPosition = 0;
		}

		public void RenderMap(Map map)
		{
			List<MapNodeUI> nodes = new List<MapNodeUI>();
			foreach (MapNode n in map.nodes)
			{
				bool acc = map.path.Count == 0 ? n.incoming.Count == 0 : map.nodes.First(d => d.point == map.path[^1]).outgoing.Contains(n.point);
				nodes.Add(CreateNewNode(n, map.path.Contains(n.point), acc));
				
			}
			foreach (MapNodeUI node in nodes)
			{
				foreach (Vector2Int connection in node.node.outgoing)
					AddLineConnection(node, GetNode(nodes, connection));
			}
			scrollView.verticalNormalizedPosition = 0;
		}

		private MapNodeUI GetNode(List<MapNodeUI> nodes, Vector2Int p)
		{
			return nodes.First(n => n.node.point == p);
		}

		private MapNodeUI CreateNewNode(MapNode node, bool visited, bool canAccess)
		{
			LocationType loc = node.locType;

			GameObject go = Instantiate(mapNodePrefab, nodeContainer, false);
			((RectTransform)go.transform).anchoredPosition3D = node.position;
			MapNodeUI ui = go.GetComponent<MapNodeUI>();
			ui.Setup(node, visited, canAccess);
			ui.AddHover(p =>
			{
				Tooltip.ShowTooltip(go.transform.position + new Vector3(50, 50), $"{loc.name}\n{loc.description}", 5);
			});
			go.GetComponent<Button>().onClick.AddListener(() =>
			{
				// todo
				loc.DoRewards(node);
				GameManager.instance.CurrentMap.AddPath(node);
				ui.SetVisited();
			});
			return ui;
		}

		protected void AddLineConnection(MapNodeUI from, MapNodeUI to)
		{
			GameObject go = new GameObject("Line", typeof(UILineRenderer));
			go.transform.SetParent(nodeContainer);
			go.transform.SetAsFirstSibling();
			RectTransform rt = ((RectTransform)go.transform);
			rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0);
			rt.anchoredPosition3D = Vector3.zero;
			UILineRenderer lineRenderer = go.GetComponent<UILineRenderer>();
			lineRenderer.transform.SetAsFirstSibling();
			RectTransform fromRT = (RectTransform)from.transform;
			RectTransform toRT = (RectTransform)to.transform;

			lineRenderer.Points = new[] { fromRT.anchoredPosition + new Vector2(50, 50), toRT.anchoredPosition + new Vector2(50, 50) };
			lineRenderer.color = Color.gray;
		}
	}
}
