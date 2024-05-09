using System;
using Assets.draco18s.util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.draco18s.bulletboss.ui
{
	public class DraggableHandle : MonoBehaviour, IDragHandler, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField] private DraggableElement changeBar;
		[SerializeField] private Image handleHover;

		public void OnDrag(PointerEventData data)
		{
			changeBar.OnDrag(data);
		}

		public void Disable()
		{
			gameObject.SetActive(false);
		}

		public void Enable()
		{
			gameObject.SetActive(true);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			handleHover.enabled = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			handleHover.enabled = false;
		}
	}
}