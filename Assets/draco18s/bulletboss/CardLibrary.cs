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
			for (int i = 0; i < 5; i++)
			{
				Card card = collection.Draw();
				if(card == null) break;

				CardUI cardUI = Instantiate(GameAssets.instance.cardUIObject, CardHand.instance.transform).GetComponent<CardUI>();
				cardUI.SetData(card);
			}
			DeckUI.instance.SetDeck(collection);
		}
	}
}
