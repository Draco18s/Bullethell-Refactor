using System.Collections.Generic;
using Assets.draco18s.bulletboss.cards;
using Assets.draco18s.bulletboss.pattern;
using Assets.draco18s.bulletboss.pattern.timeline;
using Assets.draco18s.bulletboss.ui;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using static Assets.draco18s.bulletboss.pattern.timeline.TimelineModifierType;

namespace Assets.draco18s.bulletboss
{
	public class CardLibrary : MonoBehaviour
	{
		public static CardLibrary instance;
		
		[SerializeField] private PatternModuleType[] modules;
		[SerializeField] private TimelineModifierType[] modifiers;
		private Dictionary<string, PatternModuleType> moduleRegistry;
		private Dictionary<string, TimelineModifierType> modifierRegistry;

		public Deck collection { get; protected set; }

		void Awake()
		{
			instance = this;
			collection = new Deck();
			moduleRegistry = new Dictionary<string, PatternModuleType>();
			modifierRegistry = new Dictionary<string, TimelineModifierType>();
		}

		void Start()
		{
			foreach (PatternModuleType module in modules)
			{
				moduleRegistry.Add($"{module.rarity}/{module.name}", module);
				if (module.rarity > NamedRarity.Rare) continue;
				collection.Add(new Card(module));
			}
			foreach (TimelineModifierType modifier in modifiers)
			{
				if (modifier.rarity != NamedRarity.Starting) continue;
				collection.Add(new Card(modifier));
				modifierRegistry.Add(modifier.name, modifier);
			}
			collection.Reset();

			//DeckUI.instance.SetDeck(collection);
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
	}
}
