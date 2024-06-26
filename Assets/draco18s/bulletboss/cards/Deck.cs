using System;
using System.Collections.Generic;
using Assets.draco18s.util;

namespace Assets.draco18s.bulletboss.cards {
	public class Deck
	{
		private readonly List<Card> fullCollection;
		private readonly Queue<Card> activeDeck;
		private readonly Queue<Card> activeDiscard;
		private readonly List<Card> allCards;

		public event Action<int> OnSizeChange = delegate(int i) {  };

		public Deck()
		{
			allCards = new List<Card>();
			activeDeck = new Queue<Card>();
			activeDiscard = new Queue<Card>();
			fullCollection = new List<Card>();
		}

		public void Reset()
		{
			activeDeck.Clear();
			activeDeck.AddRange(allCards);
			Shuffle();
			OnSizeChange(activeDeck.Count);
		}

		public Card Draw()
		{
			if(activeDeck.Count <= 0 && activeDiscard.Count > 0)
			{
				activeDeck.AddRange(activeDiscard);
				activeDiscard.Clear();
				activeDeck.Shuffle();
			}

			Card ret = null;
			if (activeDeck.Count > 0)
				ret = activeDeck.Dequeue();

			OnSizeChange(activeDeck.Count);
			return ret;
		}

		public void Shuffle()
		{
			activeDeck.Shuffle();
		}

		public void Add(Card card)
		{
			fullCollection.Add(card);
			allCards.Add(card);
		}

		public int Count()
		{
			return activeDeck.Count;
		}

		public void Discard(Card cardRef)
		{
			activeDiscard.Enqueue(cardRef);
		}
	}
}