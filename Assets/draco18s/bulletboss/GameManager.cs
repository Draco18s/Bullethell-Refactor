using System.Collections;
using System.Collections.Generic;
using Assets.draco18s.bulletboss.entities;
using Assets.draco18s.bulletboss.entities.behavior;
using Assets.draco18s.bulletboss.map;
using Assets.draco18s.bulletboss.ui;
using Assets.draco18s.bulletboss.upgrades;
using Assets.draco18s.serialization;
using Assets.draco18s.util;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.UnityConverters;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.draco18s.bulletboss
{
	public class GameManager : MonoBehaviour
	{
		public enum GameState
		{
			Init, MainMenu, Map, Combat, Editing, Store, Forge, GameOver
		}

		public static GameManager instance;
		public Transform bulletParentContainer => bulletContainer;

		[SerializeField] private Button endTurnBtn;
		[SerializeField] private Button discardAndDrawBtn;
		[SerializeField] private Canvas interfaceCanvas;
		[SerializeField] private Canvas mapCanvas;
		[SerializeField] private Transform aiPlayerContainer;
		[SerializeField] private Transform bulletContainer;
		[SerializeField] private Texture2D bulletHeatmap;
		[SerializeField] private MapConfig conf;

		public int Depth { get; protected set; } = 0;
		public MapConfig CurrentMapConfig { get; protected set; }
		public Map CurrentMap { get; protected set; }

		public GameState gameState { get; protected set; } = GameState.Init;
		public Texture2D heatmap => bulletHeatmap;
		public bool doHeatmap => false;

		public PlayerProgress aiPlayerData { get; protected set; }
		private readonly List<GameObject> playerShips = new List<GameObject>();

		[UsedImplicitly]
		void Awake()
		{
			endTurnBtn.onClick.AddListener(EndTurn);
			discardAndDrawBtn.onClick.AddListener(DiscardAndDraw);
			StartCoroutine(WaitForReady());
			instance = this;

			CurrentMap = MapGenerator.GenerateMap(conf);
			mapCanvas.enabled = false;
			aiPlayerData = new PlayerProgress();
		}

		// todo: save game
		public void SaveGame()
		{
			JsonSerializerSettings settings = new JsonSerializerSettings();
			//settings.ContractResolver = new ContractResolver();
			settings.ContractResolver = new UnityTypeContractResolver();
			ContractResolver.jsonSettings = settings;

			// current map
			// deck/collection
			// aiplayer details
			// fighter config
		}

		public void LoadGame()
		{

		}

		private float timer = 1;

		[UsedImplicitly]
		private void Update()
		{
			if (doHeatmap) HeatMapUpdate();
			FighterConfigManager.instance.DoUpdate(Time.deltaTime);
		}

		private void HeatMapUpdate()
		{
			timer -= Time.deltaTime;
			if (timer < 0)
			{
				Color[] cols = heatmap.GetPixels();
				for (int i = 0; i < cols.Length; i++)
				{
					Color c = cols[i];
					Color.RGBToHSV(c, out float h, out float s, out float v);
					float a = c.a;
					if (h > 0.3335f)
					{
						h -= Mathf.Max(h * 0.02f, 0.02f);
					}
					else
					{
						s -= Mathf.Max(s * 0.0125f, 0.0125f);
						v -= Mathf.Max(v * 0.025f, 0.025f);
					}

					c = Color.HSVToRGB(Mathf.Clamp01(h), Mathf.Clamp01(s), Mathf.Clamp01(v));
					if (a < 0.025f)
					{
						c.a = 0;
					}
					else if (v < 0.25f)
					{
						c.a = a - 0.015f;
					}
					cols[i] = c;
				}

				heatmap.SetPixels(cols);
				heatmap.Apply();
				timer += 1;
			}
		}

		public IEnumerator WaitForReady()
		{
			yield return new WaitWhile(() => CardHand.instance == null);
			StartNewGame();
		}

		public void StartNewGame()
		{
			Color[] cols = heatmap.GetPixels();
			for (int i = 0; i < cols.Length; i++)
			{
				cols[i] = Color.clear;
			}
			heatmap.SetPixels(cols);
			heatmap.Apply();
			gameState = GameState.MainMenu;
			NavMapUI.instance.RenderMap(CurrentMap);
			// todo: temporary
			ShowMap();
		}

		private void ShowMap()
		{
			NavMapUI.instance.UpdateMap(CurrentMap);
			gameState = GameState.Map;
			mapCanvas.enabled = true;
		}

		public void DoEvent(MapNode node)
		{
			BasicTechType reward = null;
			switch (node.locType.nodeType)
			{
				case MapNodeType.Mystery:
				case MapNodeType.Treasure:
					reward = node.locType.techConfig.techOptions.GetRandom();
					break;
				case MapNodeType.Store:
					DisplayStore();
					reward = node.locType.techConfig.techOptions.GetRandom();
					break;
				case MapNodeType.RestSite:
					aiPlayerData.Rest();
					break;
			}
			aiPlayerData.AcquireItem(reward);
		}

		private void DisplayStore()
		{
			
		}

		public void StartNewCombat(MapNode node)
		{
			aiPlayerContainer.Clear();
			bulletContainer.Clear();
			if (node.locType.nodeType == MapNodeType.NormalEncounter || node.locType.nodeType == MapNodeType.Boss)
			{
				SetupPlayers(aiPlayerData, new[]{0});
			}
			if (node.locType.nodeType == MapNodeType.FleetEncounter)
			{
				SetupPlayers(aiPlayerData, new[] { 0, -2, -2 });
			}
			gameState = GameState.Combat;
			mapCanvas.enabled = false;
			NewTurn();
		}

		private void SetupPlayers(PlayerProgress aiData, int[] modifiers)
		{
			aiPlayerContainer.Clear();
			int i = -1;
			foreach (int mod in modifiers)
			{
				GameObject go = Instantiate(GameAssets.aiPlayerObject, aiPlayerContainer);
				go.transform.localPosition = Vector3.zero;
				playerShips.Add(go);
				if (mod != 0)
				{
					go.transform.localPosition += Vector3.left * i * 2;
					i *= -1;
				}

				go.GetComponent<Player>().SetStats(aiData, mod);
				go.GetComponent<PlayerAgent>().SetContainer(bulletContainer);
			}
		}

		public void NewTurn()
		{
			aiPlayerContainer.gameObject.SetActive(false);
			gameState = GameState.Editing;
			endTurnBtn.gameObject.SetActive(true);
			interfaceCanvas.enabled = true;
			CardHand.instance.Draw(GetDrawCount());
		}

		public void DiscardAndDraw()
		{
			CardHand.instance.Discard(-1, true);
			CardHand.instance.Draw(GetDrawCount());
		}

		public void EndTurn()
		{
			interfaceCanvas.enabled = false;
			TimelineUI.instance.Close();
			CardHand.instance.Discard(-1, true);
			endTurnBtn.gameObject.SetActive(false);
			aiPlayerContainer.gameObject.SetActive(true);
			StartCoroutine(WaitFive());
		}

		private IEnumerator WaitFive()
		{
			yield return new WaitForSecondsRealtime(25f);
			NewTurn();// temp
		}

		private int GetDrawCount()
		{
			// todo: upgradeable?
			return 5;
		}

		public void CheckGameOver(Player player)
		{
			playerShips.Remove(player.gameObject);
			aiPlayerData.AddFinalGems(player.collectedGems, true);
			if (playerShips.Count == 0)
			{
				aiPlayerContainer.gameObject.SetActive(false);
				interfaceCanvas.enabled = false;
				ShowMap();
			}
		}

		public void CheckGameOver(BossEntity boss)
		{
			if (boss.GetCurrentSegmentHP() <= 0)
			{
				NewTurn();
			} 
		}
	}
}