using Assets.draco18s.util;
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.draco18s.bulletboss.cards;
using Assets.draco18s.bulletboss.entities;
using Assets.draco18s.bulletboss.ui;
using Assets.draco18s.serialization;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Assets.draco18s.bulletboss.pattern.timeline
{
	[Serializable]
	[JsonResolver(typeof(Resolver))]
	public class Timeline
	{
		[SerializeField] private bool runtimeEditable;
		[SerializeField] private TimelineModifierType[] modifiers;
		[SerializeField] private SerializableDictionary<int, PatternModuleType> patternObjects;

		private Dictionary<int, Card> activeRuntimePattern;
		private List<Card> activeRuntimeModifiers;
		//private Dictionary<int, PatternModule> functionalPattern => activeRuntimePattern.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.pattern);
		private Dictionary<Card, CardUI> uiLookup;
		private float currentTime;
		public delegate void OnTimelineChanged();
		[NonSerialized] public OnTimelineChanged onTimelineChanged = () => { };
		private bool loopsOnTimelineEnd = true;
		private float overrideDuration = 0;
		private PatternModuleType moduleTypeOfThis;
		[NonSerialized] private PatternModule moduleOfThis;

		public PatternModule runtimeModule => moduleOfThis;
		public bool isEditable => runtimeEditable;

		public void DeserializeForRuntime()
		{
			activeRuntimeModifiers ??= new List<Card>();
			activeRuntimePattern ??= new Dictionary<int, Card>();
			uiLookup ??= new Dictionary<Card, CardUI>();
			if (patternObjects == null || activeRuntimePattern.Count > 0) return;
			foreach (KeyValuePair<int, PatternModuleType> patternObject in patternObjects)
			{
				activeRuntimePattern.Add(patternObject.Key, new Card(patternObject.Value, true));
			}
			if (modifiers == null || activeRuntimeModifiers.Count > 0) return;
			foreach (TimelineModifierType modifierObject in modifiers)
			{
				activeRuntimeModifiers.Add(new Card(modifierObject, true));
			}
		}

		public void SetModuleType(PatternModule module)
		{
			moduleOfThis = module;
			moduleTypeOfThis = module.patternTypeData;
		}

		public void InitOrReset(bool allowedToLoop = true)
		{
			activeRuntimeModifiers ??= new List<Card>();
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
			currentTime = 0;
		}

		public void RemoveModule(CardUI card)
		{
			//uiLookup[patternRuntime[k]].transform.localPosition
			int k = (int)card.transform.localPosition.x;
			//Debug.Log($"Remove at {k}");
			activeRuntimePattern.Remove(k, out Card m);
			uiLookup.Remove(m);
			ValidateModules();
			currentTime = 0;
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
			int nextOpen = 0;
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
			
			KeyValuePair<int, Card>[] tooFar = activeRuntimePattern.Where(kvp => kvp.Key > secondWidth*10).ToArray();
			foreach (KeyValuePair<int, Card> pair in tooFar)
			{
				if (uiLookup.TryGetValue(pair.Value, out CardUI ui))
				{
					activeRuntimePattern.Remove(pair.Key);
					uiLookup.Remove(pair.Value);
					CardHand.instance.Insert(ui);
				}
				else
				{
					activeRuntimePattern.Remove(pair.Key);
					CardHand.instance.Insert(pair.Value);
				}
			}

			foreach (Card card in activeRuntimeModifiers)
			{
				CardUI uiCard = uiLookup[card];

				bool b = activeRuntimeModifiers.Count(m => m.timelineModifier.moduleType == TimelineModifierType.ModuleType.Sprite) > 1;
				if(b) uiCard.Disable("Cannot have two Sprite modifiers");
				else uiCard.Enable();
			}

			onTimelineChanged();
		}

		public bool RuntimeUpdate(Bullet bullet, float dt)
		{
			if(activeRuntimePattern == null) return true;
			float secondWidth = ((RectTransform)TimelineUI.instance.transform).rect.width / 10;
			int idx = Mathf.CeilToInt(currentTime * secondWidth);

			//if(activeRuntimePattern.Count > 1)
			//	Debug.Log("Acting on " + idx + string.Join(',', activeRuntimePattern.OrderBy(x => x.Key).Select(kv => kv.Value.pattern.patternTypeData.name + " @ " + kv.Key)));

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
			bool completed = currentTime >= GetDuration();
			if (completed && loopsOnTimelineEnd)
			{
				currentTime -= GetDuration();
				foreach (KeyValuePair<int, Card> module in activeRuntimePattern)
				{
					module.Value.pattern.ResetForNewLoopIteration();
				}
			}
			return completed;
		}

		public void SetOverrideDuration(float duration)
		{
			overrideDuration = duration;
		}

		public float GetDuration()
		{
			float secondWidth = ((RectTransform)TimelineUI.instance.transform).rect.width / 10;
			float max = overrideDuration;
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
			timeline.runtimeEditable = original.runtimeEditable;
			return timeline;
		}

		public static Timeline CloneForAsset(Timeline original)
		{
			Timeline timeline = new Timeline();

			original.DeserializeForRuntime();
			original.InitOrReset();
			timeline.InitOrReset();

			timeline.patternObjects = new SerializableDictionary<int, PatternModuleType>();
			foreach (KeyValuePair<int, Card> kvp in original.activeRuntimePattern)
			{
				timeline.patternObjects[kvp.Key] = kvp.Value.pattern.ExportAsScriptableObject();
			}
			timeline.runtimeEditable = false;
			return timeline;
		}

		public IReadOnlyList<PatternModuleType> GetPatternObjects()
		{
			return patternObjects.Values.ToList();
		}

		public bool CanAdd(PatternModule refPattern)
		{
			int max = moduleTypeOfThis?.GetMaxChildren() ?? -1;
			if (max < 0 || activeRuntimePattern.Count >= max) return false;
			return moduleTypeOfThis == null || moduleTypeOfThis.CanAddModule(refPattern.patternTypeData);
		}

		public void AddModifier(CardUI cardUI)
		{
			activeRuntimeModifiers.Add(cardUI.cardRef);
			uiLookup.Add(cardUI.cardRef, cardUI);
			ValidateModules();
		}

		public void RemoveModifier(CardUI cardUI)
		{
			activeRuntimeModifiers.Remove(cardUI.cardRef);
			uiLookup.Remove(cardUI.cardRef);
			ValidateModules();
		}

		public class Resolver : JsonConverter
		{
			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				if (value == null) return;

				Timeline v = (Timeline)value;
				JObject o = new JObject();

				if (v.activeRuntimePattern == null) return;
				if(v.activeRuntimePattern != null)
					o.Add(new JProperty("pattern", v.activeRuntimePattern.ToDictionary(kvp => kvp.Key, kvp => JsonConvert.SerializeObject(kvp.Value.pattern, ContractResolver.jsonSettings))));
				o.WriteTo(writer);
			}

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
			{
				JObject jObject = JObject.Load(reader);
				Timeline runObj = new Timeline();
				//runObj.activeRuntimePattern = jObject.GetValue("pattern").Value<Dictionary<int, PatternModule>>();
				return runObj;
			}

			public override bool CanConvert(Type objectType)
			{
				return typeof(Timeline).IsAssignableFrom(objectType);
			}
		}
	}
}