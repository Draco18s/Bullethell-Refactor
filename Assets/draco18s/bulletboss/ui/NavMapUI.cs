using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.draco18s.bulletboss.ui.map;
using Assets.draco18s.ui;
using Assets.draco18s.util;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.draco18s.bulletboss.ui
{
	public class NavMapUI : MonoBehaviour
	{
		[SerializeField] private GameObject mapNodePrefab;
		[SerializeField] private ScrollRect scrollView;
		[SerializeField] private Transform nodeContainer;
		[SerializeField] private LocationType[] locations;

		void Start()
		{
			nodeContainer.Clear();
			scrollView.verticalNormalizedPosition = 0;
			//GenerateMap();
		}

		private void CreateNewNode()
		{
			LocationType loc = GetRandomLocation();
			GameObject go = Instantiate(mapNodePrefab, nodeContainer, false);
			((RectTransform)go.transform).anchoredPosition3D = new Vector3(0, GameManager.instance.Depth * 450 + 125, 0);
			Image img = go.GetComponent<Image>();
			img.sprite = loc.icon;
			img.AddHover(p =>
			{
				Tooltip.ShowTooltip(go.transform.position + new Vector3(50, 50), loc.description, 5);
			});
			go.GetComponent<Button>().onClick.AddListener(() =>
			{
				loc.DoRewards();
				GameManager.instance.StartNewGame();
			});
		}

		private LocationType GetRandomLocation()
		{
			return locations.GetRandom();
		}
	}
}
