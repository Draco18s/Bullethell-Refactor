using System.Collections.Generic;
using Assets.draco18s.util;

namespace Assets.draco18s.bulletboss.cards {
	public class Deck
	{
		private readonly List<Card> collection;
		private readonly Queue<Card> activeDeck;
		private readonly List<Card> allCards;

		public Deck()
		{
			allCards = new List<Card>();
			activeDeck = new Queue<Card>();
			collection = new List<Card>();
		}

		public void Reset()
		{
			activeDeck.Clear();
			activeDeck.AddRange(allCards);
			Shuffle();
		}

		public Card Draw()
		{
			if(activeDeck.Count > 0)
				return activeDeck.Dequeue();

			return null;
		}

		public void Shuffle()
		{
			activeDeck.Shuffle();
		}

		public void Add(Card card)
		{
			collection.Add(card);
			allCards.Add(card);
		}
	}
}