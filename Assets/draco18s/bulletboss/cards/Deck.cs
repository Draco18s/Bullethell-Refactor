using System;
using System.Collections;
using System.Collections.Generic;
using Assets.draco18s.util;
using JetBrains.Annotations;

namespace Assets.draco18s.bulletboss.cards {
	public class Deck
	{
		[Flags]
		public enum AddType
		{
			None = 0,
			BaseDeck = 1 << 0,
			DiscardPile = BaseDeck | 1 << 1,
			DrawPile = BaseDeck | 1 << 2,

			Shuffle_First = 1 << 3,
			Shuffle_After = 1 << 4,

			OnTop = 1 << 5,
			OnBottom = 1 << 6,
			Anywhere = OnTop | Shuffle_After,
		}
		private readonly Queue<Card> activeDeck;
		private readonly Queue<Card> activeDiscard;
		private readonly List<Card> unmodifiedDeck;

		public event Action<int> OnSizeChange = delegate(int i) {  };

		public Deck()
		{
			unmodifiedDeck = new List<Card>();
			activeDeck = new Queue<Card>();
			activeDiscard = new Queue<Card>();
		}

		public void Reset()
		{
			activeDeck.Clear();
			activeDeck.AddRange(unmodifiedDeck);
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

		public void Add(Card card, AddType method)
		{
			if(method.HasFlag(AddType.BaseDeck))
			{
				unmodifiedDeck.Add(card);
			}

			if (method.HasFlag(AddType.DiscardPile))
			{
				if (method.HasFlag(AddType.Shuffle_First))
					activeDiscard.Shuffle();

				if (method.HasFlag(AddType.OnTop))
					activeDiscard.Unqueue(card);
				else if (method.HasFlag(AddType.OnBottom))
					activeDiscard.Enqueue(card);

				if (method.HasFlag(AddType.Shuffle_After))
					activeDiscard.Shuffle();
			}
			else if (method.HasFlag(AddType.DrawPile))
			{
				if (method.HasFlag(AddType.Shuffle_First))
					activeDeck.Shuffle();

				if (method.HasFlag(AddType.OnTop))
					activeDeck.Unqueue(card);
				else if (method.HasFlag(AddType.OnBottom))
					activeDeck.Enqueue(card);

				if (method.HasFlag(AddType.Shuffle_After))
					activeDeck.Shuffle();
			}
		}

		public int Count()
		{
			return activeDeck.Count;
		}

		public void Discard(Card cardRef)
		{
			activeDiscard.Enqueue(cardRef);
		}
		
		public IEnumerable<Card> GetDrawPile()
		{
			List<Card> deck = new List<Card>(activeDeck);
			deck.Shuffle();
			return deck;
		}

		public IEnumerable<Card> GetDiscardPile()
		{
			List<Card> deck = new List<Card>(activeDiscard);
			deck.Shuffle();
			return deck;
		}
	}
}