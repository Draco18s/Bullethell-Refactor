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

		void Awake()
		{
			instance = this;

			CardLibrary.instance.activeDeck.OnSizeChange += UpdateDeckGraphics;
			GetComponent<Button>().AddHover(p =>
			{
				Tooltip.ShowTooltip(p, $"{CardLibrary.instance.activeDeck.Count()} Cards Left\nClick to view deck/discard piles", 4);
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
