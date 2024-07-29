using System.Collections.Generic;
using Assets.draco18s.bulletboss.cards;
using Assets.draco18s.util;
using System.Collections.ObjectModel;
using Assets.draco18s.bulletboss.pattern;
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

		public void Discard(int num, bool dupStarting=false)
		{
			if (num < 0 || num > transform.childCount)
			{
				num = transform.childCount;
			}
			for (int i = 0; i < num; i++)
			{
				Transform t = transform.GetChild(i);
				CardLibrary.instance.Discard(t.GetComponent<CardUI>().cardRef);
				Destroy(t.gameObject);
			}

			if (dupStarting)
			{
				foreach (CardUI cardUI in playedCards)
				{
					if(cardUI.cardRef.pattern?.patternTypeData != null)
						CardLibrary.instance.Discard(new Card(cardUI.cardRef.pattern.patternTypeData));
					else if(cardUI.cardRef.timelineModifier != null)
						CardLibrary.instance.Discard(new Card(cardUI.cardRef.timelineModifier));
				}
			}
		}

		public void Draw(int num)
		{
			for (int i = 0; i < num; i++)
			{
				Card card = CardLibrary.instance.Draw();
				if (card == null) break;

				CardUI cardUI = Instantiate(GameAssets.playableUIObject, transform).GetComponent<CardUI>();
				cardUI.SetData(card);
			}
		}

		public void Insert(CardUI cardUI)
		{
			cardUI.transform.SetParent(transform);
			if (playedCards.Contains(cardUI))
			{
				playedCards.Remove(cardUI);
			}
			else if (cardUI.cardRef.rarity == NamedRarity.Starting)
			{
				cardUI.cardRef.SetEphemeral();
			}
		}

		public void Insert(Card card)
		{
			CardUI cardUI = Instantiate(GameAssets.playableUIObject, transform).GetComponent<CardUI>();
			cardUI.SetData(card);
		}

		private List<CardUI> playedCards = new List<CardUI>();

		public void Remove(CardUI cardUI)
		{
			if (!cardUI.cardRef.isUnique)
			{
				playedCards.Add(cardUI);
			}
		}
	}
}