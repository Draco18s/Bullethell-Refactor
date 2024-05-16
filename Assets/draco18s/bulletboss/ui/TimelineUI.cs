using System.Collections.Generic;
using Assets.draco18s.bulletboss.cards;
using Assets.draco18s.bulletboss.pattern.timeline;
using Assets.draco18s.util;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.draco18s.bulletboss.ui
{
	public class TimelineUI : MonoBehaviour
	{
		[SerializeField] private Transform cardContainer;
		[SerializeField] private Transform modifierContainer;
		[SerializeField] private Button parentPatternBtn;
		public static TimelineUI instance;

		private Stack<Timeline> timelines;
		public Timeline currentTimeline => timelines.Peek();
		private Canvas canvas;

		void Awake()
		{
			instance = this;
			timelines = new Stack<Timeline>();
			canvas = GetComponent<Canvas>();
			canvas.enabled = false;
			parentPatternBtn.onClick.AddListener(SelectParentPattern);
		}

		public void Select(Timeline pattern)
		{
			timelines.Clear();
			timelines.Push(pattern);
			canvas.enabled = true;
			RefreshUI();
		}

		public void SelectChildPattern(Timeline pattern)
		{
			timelines.Push(pattern);
			RefreshUI();
		}

		public void SelectParentPattern()
		{
			timelines.Pop();
			RefreshUI();
		}

		public void RemoveModule(CardUI cardUI)
		{
			currentTimeline.RemoveModule(cardUI);
		}

		public void RemoveModifier(CardUI cardUI)
		{
			currentTimeline.RemoveModifier(cardUI);
		}

		public void AddModule(CardUI cardUI)
		{
			if (!CanAdd(cardUI))
			{

				return;
			}
			if (cardUI.cardRef.pattern == null)
			{
				cardUI.transform.SetParent(modifierContainer);
				currentTimeline.AddModifier(cardUI);
			}
			else
			{
				cardUI.transform.SetParent(cardContainer);
				cardUI.transform.localPosition = new Vector3(cardUI.transform.localPosition.x, 0);
				currentTimeline.AddModule(cardUI);
			}

			RefreshUI();
		}

		private bool CanAdd(CardUI cardUI)
		{
			if (cardUI.cardRef.pattern == null)
			{
				return modifierContainer.childCount < 5;
			}
			else
			{
				currentTimeline.CanAdd(cardUI.cardRef.pattern);
			}
			return true;
		}

		private void RefreshUI()
		{
			cardContainer.Clear();
			foreach (KeyValuePair<int, Card> m in currentTimeline.GetModules())
			{
				CardUI cardUI = Instantiate(GameAssets.instance.cardUIObject, cardContainer).GetComponent<CardUI>();
				Vector3 p = cardUI.gameObject.transform.localPosition;
				cardUI.SetData(m.Value);
				cardUI.gameObject.transform.localPosition = new Vector3(m.Key, p.y, p.z);
				currentTimeline.UpdateUIObj(m.Value, cardUI);
			}
			currentTimeline.ValidateModules();
			parentPatternBtn.gameObject.SetActive(timelines.Count > 1);
		}

		public void Close()
		{
			canvas.enabled = false;
			timelines.Clear();
		}
	}
}
