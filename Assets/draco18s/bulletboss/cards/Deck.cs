using System;
using System.Collections.Generic;
using Assets.draco18s.util;

namespace Assets.draco18s.bulletboss.cards {
	public class Deck
	{
		private readonly List<Card> fullCollection;
		private readonly Queue<Card> activeDeck;
		private readonly List<Card> allCards;

		public event Action<int> OnSizeChange = delegate(int i) {  };

		public Deck()
		{
			allCards = new List<Card>();
			activeDeck = new Queue<Card>();
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
			if(activeDeck.Count > 0)
				return activeDeck.Dequeue();

			OnSizeChange(activeDeck.Count);
			return null;
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
	}
}