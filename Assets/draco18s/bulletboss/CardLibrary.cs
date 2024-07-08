using System;
using System.Collections.Generic;
using Assets.draco18s.bulletboss.cards;
using Assets.draco18s.bulletboss.pattern;
using Assets.draco18s.bulletboss.pattern.timeline;
using UnityEngine;

namespace Assets.draco18s.bulletboss
{
	public class CardLibrary : MonoBehaviour
	{
		public static CardLibrary instance;
		
		[SerializeField] private PatternModuleType[] modules;
		[SerializeField] private TimelineModifierType[] modifiers;
		private Dictionary<string, PatternModuleType> moduleRegistry;
		private Dictionary<string, TimelineModifierType> modifierRegistry;
		private Dictionary<NamedRarity, List<Card>> cardPools;
		private List<Card> unlockedCards;

		public Deck activeDeck { get; protected set; }

		void Awake()
		{
			instance = this;
			activeDeck = new Deck();
			moduleRegistry = new Dictionary<string, PatternModuleType>();
			modifierRegistry = new Dictionary<string, TimelineModifierType>();
			cardPools = new Dictionary<NamedRarity, List<Card>>();
			unlockedCards = new List<Card>();
			foreach (NamedRarity r in Enum.GetValues(typeof(NamedRarity)))
			{
				cardPools.Add(r, new List<Card>());
			}
		}

		void Start()
		{
			foreach (PatternModuleType module in modules)
			{
				moduleRegistry.Add(GetModuleName(module), module);
				cardPools[module.rarity].Add(new Card(module));

				if (module.rarity > NamedRarity.Uncommon) continue;
				activeDeck.Add(new Card(module), Deck.AddType.BaseDeck);
				unlockedCards.Add(new Card(module));
			}
			foreach (TimelineModifierType modifier in modifiers)
			{
				if (modifier.rarity != NamedRarity.Starting) continue;
				cardPools[modifier.rarity].Add(new Card(modifier));

				activeDeck.Add(new Card(modifier), Deck.AddType.BaseDeck);
				modifierRegistry.Add(modifier.name, modifier);
				unlockedCards.Add(new Card(modifier));
			}
			activeDeck.Reset();
		}

		public Card Draw()
		{
			return activeDeck.Draw();
		}

		public PatternModuleType GetModuleByName(string id)
		{
			return moduleRegistry[id];
		}

		public TimelineModifierType GetModifierByName(string id)
		{
			return modifierRegistry[id];
		}

		public void Discard(Card cardRef)
		{
			if(cardRef.isEphemeral) return;
			activeDeck.Discard(cardRef);
		}

		public string GetModuleName(PatternModuleType module)
		{
			return $"{module.rarity}/{module.name}";
		}

		public string GetModuleName(TimelineModifierType module)
		{
			return $"{module.rarity}/{module.name}";
		}
	}
}
