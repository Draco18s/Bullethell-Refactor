using Assets.draco18s.bulletboss.cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.draco18s.bulletboss.ui
{
	public class ViewCardUI : MonoBehaviour
	{
		[SerializeField] private Image bgGlint;
		[SerializeField] private Image bg;
		[SerializeField] private Image bgShiny;
		[SerializeField] private Image sprite;
		[SerializeField] private Image spriteShiny;

		[SerializeField] private TextMeshProUGUI cardName;
		[SerializeField] private TextMeshProUGUI cardDesc;

		[SerializeField] private Transform hoverTiltCenter;
		[SerializeField] private Transform hoverPosCenter;

		[SerializeField] private RectTransform tagBar;
		[SerializeField] private Image tagBG;
		[SerializeField] private TextMeshProUGUI tagName;
		public Card cardRef { get; protected set; }
		private bool lastEphemeral = false;

		void Awake()
		{
			//selfCanvas = transform.GetComponent<Canvas>();
			//canvasParent = handTransform;
			//time = 1;
		}

		void Update() {
			if (cardRef.isEphemeral != lastEphemeral)
			{
				lastEphemeral = cardRef.isEphemeral;
				bg.enabled = !(cardRef.isUnique || cardRef.isEphemeral);
				bgShiny.enabled = cardRef.isUnique || cardRef.isEphemeral;
				bgShiny.GetComponent<Image>().material.SetFloat("_EphemeralStrength", cardRef.isEphemeral ? 1 : 0);
				Color tColor = cardRef.rarity.GetColor() * (cardRef.isEphemeral ? 0.35f : 1);
				tColor.a = 1;
				tagBG.color = tColor;
				tagName.text = cardRef.isEphemeral ? "Ephemeral" : cardRef.rarity.ToString();
				tagName.color = cardRef.isEphemeral ? Color.white : cardRef.rarity.GetTextColor();
				lastEphemeral = cardRef.isEphemeral;
			}
		}

		public void SetData(Card card)
		{
			cardRef = card;
			bgGlint.color = cardRef.rarity.GetColor();
			cardName.text = cardRef.name;
			cardDesc.text = cardRef.description;

			sprite.sprite = spriteShiny.sprite = card.icon;
			
			bg.enabled = !(card.isUnique || card.isEphemeral);
			bgShiny.enabled = card.isUnique || card.isEphemeral;
			sprite.enabled = !card.isUnique;
			spriteShiny.enabled = card.isUnique;
			bgShiny.GetComponent<Image>().material.SetFloat("_EphemeralStrength", card.isEphemeral ? 1 : 0);
			tagBG.color = cardRef.rarity.GetColor() * (cardRef.isEphemeral ? 0.25f : 1);
			tagName.text = cardRef.isEphemeral ? "Ephemeral" : cardRef.rarity.ToString();
			tagName.color = cardRef.isEphemeral ? Color.white : cardRef.rarity.GetTextColor();
			lastEphemeral = cardRef.isEphemeral;
		}
	}
}
