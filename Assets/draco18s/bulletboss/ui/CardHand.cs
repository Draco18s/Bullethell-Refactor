using Assets.draco18s.bulletboss.cards;
using Assets.draco18s.util;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Assets.draco18s.bulletboss.ui
{
	public class CardHand : MonoBehaviour
	{
		public static CardHand instance;
		
		void Awake()
		{
			instance = this;
			transform.Clear();
		}

		public void Discard(int num)
		{
			if (num < 0 || num > transform.childCount)
			{
				transform.Clear();
				return;
			}

			for (int i = 0; i < num; i++)
			{
				Destroy(transform.GetChild(Mathf.FloorToInt(Random.value * transform.childCount)).gameObject);
			}
		}

		public void Draw(int num)
		{
			for (int i = 0; i < num; i++)
			{
				Card card = CardLibrary.instance.Draw();
				if (card == null) break;

				CardUI cardUI = Instantiate(GameAssets.instance.cardUIObject, transform).GetComponent<CardUI>();
				cardUI.SetData(card);
			}
		}
	}
}