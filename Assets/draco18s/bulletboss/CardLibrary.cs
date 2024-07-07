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

		public Deck collection { get; protected set; }

		void Awake()
		{
			instance = this;
			collection = new Deck();
			moduleRegistry = new Dictionary<string, PatternModuleType>();
			modifierRegistry = new Dictionary<string, TimelineModifierType>();
			cardPools = new Dictionary<NamedRarity, List<Card>>();
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
				collection.Add(new Card(module));
			}
			foreach (TimelineModifierType modifier in modifiers)
			{
				if (modifier.rarity != NamedRarity.Starting) continue;
				cardPools[modifier.rarity].Add(new Card(modifier));

				collection.Add(new Card(modifier));
				modifierRegistry.Add(modifier.name, modifier);
			}
			collection.Reset();
		}

		public Card Draw()
		{
			return collection.Draw();
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
			collection.Discard(cardRef);
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
