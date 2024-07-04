using Assets.draco18s.bulletboss.map;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Sentis.Layers;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.draco18s.bulletboss.ui
{
	public class MapNodeUI : Button
	{
		[SerializeField] private Image _completeSprite;
		[SerializeField] private Image _visibleIcon;

		public MapNode node { get; protected set; }

		public void Setup(MapNode node, bool visited, bool canAccess)
		{
			this.node = node;
			_visibleIcon.sprite = node.locType.icon;
			_completeSprite.enabled = visited;
			interactable = canAccess;
		}

		public void SetVisited()
		{
			_completeSprite.enabled = true;
		}

		public void SetAvailable(bool canVisit)
		{
			interactable = canVisit;
		}
	}
}