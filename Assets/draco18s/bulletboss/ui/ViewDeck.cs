using System.Collections;
using System.Collections.Generic;
using Assets.draco18s.bulletboss;
using Assets.draco18s.bulletboss.cards;
using Assets.draco18s.bulletboss.ui;
using Assets.draco18s.util;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.draco18s.ui
{
	public class ViewDeck : MonoBehaviour
	{
		[SerializeField] private Transform deckContainer;
		[SerializeField] private Transform discardContainer;

		private GameObject deckObj;
		private GameObject discardObj;
		private Canvas canvas;

		void Start()
		{
			canvas = GetComponent<Canvas>();
			canvas.enabled = false;
			deckObj = deckContainer.GetComponentInParent<ScrollRect>().gameObject;
			discardObj = discardContainer.GetComponentInParent<ScrollRect>().gameObject;
			discardObj.SetActive(false);
		}

		public void ViewActiveDeck()
		{
			Refresh();
			canvas.enabled = true;
			deckObj.SetActive(true);
			discardObj.SetActive(false);
		}

		public void ViewDiscardDeck()
		{
			Refresh();
			canvas.enabled = true;
			deckObj.SetActive(false);
			discardObj.SetActive(true);
		}

		private void Refresh()
		{
			if (canvas.enabled) return;

			deckContainer.Clear();
			foreach (Card card in CardLibrary.instance.activeDeck.GetDrawPile())
			{
				GameObject go = Instantiate(GameAssets.instance.viewyableUIObject, deckContainer);
				go.GetComponent<ViewCardUI>().SetData(card);
			}
			discardContainer.Clear();
			foreach (Card card in CardLibrary.instance.activeDeck.GetDiscardPile())
			{
				GameObject go = Instantiate(GameAssets.instance.viewyableUIObject, discardContainer);
				go.GetComponent<ViewCardUI>().SetData(card);
			}
		}
	}
}