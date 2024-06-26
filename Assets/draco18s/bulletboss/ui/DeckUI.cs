using Assets.draco18s.bulletboss.cards;
using Assets.draco18s.ui;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.draco18s.bulletboss.ui
{
	public class DeckUI  :MonoBehaviour
	{
		[SerializeField] private GameObject topCard;
		[SerializeField] private Transform cardStack;

		public static DeckUI instance;
		private Deck deck;

		void Awake()
		{
			instance = this;
		}

		public void SetDeck(Deck collection)
		{
			deck = collection;
			deck.OnSizeChange += UpdateDeckGraphics;
			GetComponent<Button>().AddHover(p =>
			{
				Tooltip.ShowTooltip(p, $"{deck.Count()} Cards Left");
			});
		}

		private void UpdateDeckGraphics(int numCards)
		{
			int display = (numCards / 4);
			for (int i = 0; i < cardStack.childCount; i++)
			{
				cardStack.GetChild(i).gameObject.SetActive(i >= cardStack.childCount - display);
			}
		}
	}
}
