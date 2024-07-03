using System;
using System.Collections.Generic;
using System.Linq;
using Assets.draco18s.bulletboss.cards;
using Assets.draco18s.bulletboss.pattern;
using Assets.draco18s.bulletboss.pattern.timeline;
using Assets.draco18s.serialization;
using Assets.draco18s.util;
using Newtonsoft.Json;
using Newtonsoft.Json.UnityConverters;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Assets.draco18s.bulletboss.ui
{
	public class TimelineUI : MonoBehaviour
	{
		[SerializeField] private Button saveAssetButton;
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
#if UNITY_EDITOR
			saveAssetButton.onClick.AddListener(SaveAsset);
#else
			saveAssetButton.gameObject.SetActive(false);
#endif
		}

		private void SaveAsset()
		{
#if !UNITY_EDITOR
			if (currentTimeline.runtimeModule != null)
			{
				PatternModuleType so = currentTimeline.runtimeModule.ExportAsScriptableObject();
				SaveRecurse(so, 0);
			}
		}
		private void SaveRecurse(PatternModuleType so, int depth) {
			string nm = so.name;
			if (string.IsNullOrEmpty(nm)) nm = so.GetType().Name;
			AssetDatabase.CreateAsset(so, $"Assets/ScriptableObjects/Export/{depth}_{nm}_{Mathf.FloorToInt(Random.value * 1000000).ToString().PadLeft(6, '0')}.asset");
			if (so is TimelinePatternModuleType timelineType)
			{
				foreach (PatternModuleType obj in timelineType.GetPattern().GetPatternObjects())
				{
					SaveRecurse(obj, depth+1);
				}
			}
#else
			JsonSerializerSettings settings = new JsonSerializerSettings();
			//settings.ContractResolver = new ContractResolver();
			settings.ContractResolver = new UnityTypeContractResolver();
			ContractResolver.jsonSettings = settings;
			Dictionary<int, PatternModule> points = new Dictionary<int, PatternModule>
			{
				{0, new ChangeModuleType.ChangeModule((ChangeModuleType)CardLibrary.instance.GetModuleByName("Common/Change Direction"))
				{
					newValue = 1
				}}
			};

			string json1 = JsonConvert.SerializeObject(points, Formatting.Indented, settings);
			Debug.Log(json1);
			
			string json2 = JsonConvert.SerializeObject(currentTimeline.GetModules().ToDictionary(kvp => kvp.Key, kvp => kvp.Value.pattern), Formatting.Indented, settings);
			Debug.Log(json2);
#endif
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
				throw new Exception("Card not allowed inside this timeline, should have already failed!");
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

		public bool CanAdd(CardUI cardUI)
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
			// TODO: scrolling region
			//float secondWidth = ((RectTransform)transform).rect.width / 10;
			//((RectTransform)cardContainer).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentTimeline.GetDuration() * secondWidth);
			parentPatternBtn.gameObject.SetActive(timelines.Count > 1);
		}

		public void Close()
		{
			canvas.enabled = false;
			timelines.Clear();
		}
	}
}
