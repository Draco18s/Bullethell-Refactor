using System.Collections.Generic;
using Assets.draco18s.bulletboss.cards;
using Assets.draco18s.bulletboss.pattern;
using Assets.draco18s.bulletboss.pattern.timeline;
using Assets.draco18s.bulletboss.ui;
using UnityEngine;
using static Assets.draco18s.bulletboss.pattern.timeline.TimelineModifierType;

namespace Assets.draco18s.bulletboss
{
	public class CardLibrary : MonoBehaviour
	{
		public static CardLibrary instance;

		[SerializeField] private PatternModuleType[] modules;
		[SerializeField] private TimelineModifierType[] modifiers;

		public Deck collection { get; protected set; }

		void Awake()
		{
			instance = this;
			collection = new Deck();
		}

		void Start()
		{
			foreach (PatternModuleType module in modules)
			{
				if(module.rarity != NamedRarity.Starting) continue;
				collection.Add(new Card(module));
			}
			foreach (TimelineModifierType modifier in modifiers)
			{
				if (modifier.rarity != NamedRarity.Starting) continue;
				collection.Add(new Card(modifier));
			}
			collection.Reset();

			DeckUI.instance.SetDeck(collection);
		}

		public Card Draw()
		{
			return collection.Draw();
		}
	}
}
