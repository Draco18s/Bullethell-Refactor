using Assets.draco18s.util;
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.draco18s.bulletboss.cards;
using Assets.draco18s.bulletboss.entities;
using Assets.draco18s.bulletboss.ui;
using UnityEngine;
using System.Collections.ObjectModel;
using UnityEditor;

namespace Assets.draco18s.bulletboss.pattern.timeline
{
	[Serializable]
	public class Timeline
	{
		[SerializeField] private bool runtimeEditable;
		[SerializeField] private TimelineModuleType[] upgrades;
		[SerializeField] private SerializableDictionary<int, PatternModuleType> patternObjects;

		private Dictionary<int, Card> activeRuntimePattern;
		//private Dictionary<int, PatternModule> functionalPattern => activeRuntimePattern.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.pattern);
		private Dictionary<Card, CardUI> uiLookup;
		private float currentTime;
		public delegate void OnTimelineChanged();
		public OnTimelineChanged onTimelineChanged = () => { };
		private bool loopsOnTimelineEnd = true;

		public bool isEditable => runtimeEditable;

		public void DeserializeForRuntime()
		{
			activeRuntimePattern ??= new Dictionary<int, Card>();
			uiLookup ??= new Dictionary<Card, CardUI>();
			if (patternObjects == null || activeRuntimePattern.Count > 0) return;
			foreach (KeyValuePair<int, PatternModuleType> patternObject in patternObjects)
			{
				activeRuntimePattern.Add(patternObject.Key, new Card(patternObject.Value, true));
			}
		}

		public void InitOrReset(bool allowedToLoop = true)
		{
			activeRuntimePattern ??= new Dictionary<int, Card>();
			uiLookup ??= new Dictionary<Card, CardUI>();
			loopsOnTimelineEnd = allowedToLoop;
		}

		public void ResetForNewLoopIteration()
		{
			currentTime = 0;
			foreach (KeyValuePair<int, Card> module in activeRuntimePattern)
			{
				module.Value.pattern.ResetForNewLoopIteration();
			}
		}

		public void AddModule(CardUI card)
		{
			int k = (int)card.transform.localPosition.x;
			if (activeRuntimePattern.ContainsKey(k) && activeRuntimePattern[k] == card.cardRef) return;
			while (activeRuntimePattern.ContainsKey(k))
			{
				k++;
				card.transform.Translate(1, 0, 0, Space.Self);
			}
			activeRuntimePattern.Add(k, card.cardRef);
			uiLookup.Add(activeRuntimePattern[k], card);
			ValidateModules();
		}

		public void RemoveModule(CardUI card)
		{
			//uiLookup[patternRuntime[k]].transform.localPosition
			int k = (int)card.transform.localPosition.x;
			//Debug.Log($"Remove at {k}");
			activeRuntimePattern.Remove(k, out Card m);
			uiLookup.Remove(m);
			ValidateModules();
		}

		public void UpdateUIObj(Card module, CardUI cardUI)
		{
			uiLookup.Remove(module);
			uiLookup.Add(module, cardUI);
		}

		public IReadOnlyDictionary<int, Card> GetModules()
		{
			return activeRuntimePattern;
		}

		public void ValidateModules()
		{
			int nextOpen = -10000;
			float secondWidth = ((RectTransform)TimelineUI.instance.transform).rect.width / 10;
			foreach (int k in activeRuntimePattern.Keys.OrderBy(x => x))
			{
				if (k >= nextOpen)
				{
					nextOpen = k + Mathf.CeilToInt(activeRuntimePattern[k].pattern.duration * secondWidth);
				}
				else
				{
					activeRuntimePattern.Remove(k, out Card module);
					CardUI m = uiLookup[module];
					if (m == null) continue;
					Vector3 p = m.gameObject.transform.localPosition;
					m.gameObject.transform.localPosition = new Vector3(nextOpen, p.y, p.z);
					m.gameObject.transform.SetAsFirstSibling();
					activeRuntimePattern.Add((int)m.gameObject.transform.localPosition.x, module);
					nextOpen += Mathf.CeilToInt(module.pattern.duration * secondWidth);
				}
			}

			onTimelineChanged();
		}

		public bool RuntimeUpdate(Bullet bullet, float dt)
		{
			if(activeRuntimePattern == null) return true;
			float secondWidth = ((RectTransform)TimelineUI.instance.transform).rect.width / 10;
			int idx = Mathf.CeilToInt(currentTime * secondWidth);

			foreach (int k in activeRuntimePattern.Keys.OrderBy(x => x))
			{
				if (k > idx) break;
				if (k != idx && k + activeRuntimePattern[k].pattern.duration > idx) continue;
				bool b = activeRuntimePattern[k].pattern.DoShotStep(bullet, dt, out bool shouldRemove);
				if (shouldRemove) bullet.DestroySelf();
				if(b && activeRuntimePattern[k].pattern.duration < dt)
				{
					idx++;
					continue;
				}
				if(!b) break;
			}

			currentTime += dt;
			if (currentTime > GetDuration() && loopsOnTimelineEnd)
			{
				currentTime -= GetDuration();
				foreach (KeyValuePair<int, Card> module in activeRuntimePattern)
				{
					module.Value.pattern.ResetForNewLoopIteration();
				}
			}
			return false;
		}

		public float GetDuration()
		{
			float secondWidth = ((RectTransform)TimelineUI.instance.transform).rect.width / 10;
			float max = 0;
			if (activeRuntimePattern != null)
			{
				foreach (KeyValuePair<int, Card> kvp in activeRuntimePattern)
				{
					max = Mathf.Max(max, kvp.Key / secondWidth + kvp.Value.pattern.duration);
				}
			}

			return max;
		}

		public static Timeline CloneFrom(Timeline original)
		{
			Timeline timeline = new Timeline();

			original.DeserializeForRuntime();
			original.InitOrReset();
			timeline.InitOrReset();

			timeline.patternObjects = new SerializableDictionary<int, PatternModuleType>();
			timeline.activeRuntimePattern = new Dictionary<int, Card>();
			foreach (KeyValuePair<int, Card> kvp in original.activeRuntimePattern)
			{
				timeline.activeRuntimePattern[kvp.Key] = new Card(kvp.Value.pattern.Clone());
				timeline.patternObjects[kvp.Key] = kvp.Value.pattern.patternTypeData;
			}
			return timeline;
		}
	}
}