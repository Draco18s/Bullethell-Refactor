using System.Collections;
using Assets.draco18s.bulletboss.ui;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;
using static Assets.draco18s.bulletboss.GameManager;

namespace Assets.draco18s.bulletboss
{
	public class GameManager : MonoBehaviour
	{
		public enum GameState
		{
			Init, MainMenu, Editing, Combat, GameOver
		}

		public static GameManager instance;
		public Transform bulletParentContainer => bulletContainer;

		[SerializeField] private Button endTurnBtn;
		[SerializeField] private Canvas interfaceCanvas;
		[SerializeField] private GameObject aiPlayerObject;
		[SerializeField] private Transform bulletContainer;
		[SerializeField] private Texture2D bulletHeatmap;

		public TextMeshProUGUI gemsTxt;
		public TextMeshProUGUI hitsTxt;
		public int gemsCount = 0;
		public int hitsCount = 0;

		public GameState gameState { get; protected set; } = GameState.Init;
		public Texture2D heatmap => bulletHeatmap;
		public bool doHeatmap => false;

		void Awake()
		{
			endTurnBtn.onClick.AddListener(EndTurn);
			StartCoroutine(WaitForReady());
			instance = this;
		}

		private float timer = 1;

		private void Update()
		{
			if (!doHeatmap) return;
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
					else if(v < 0.25f)
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
			NewTurn();
			//EndTurn();
		}

		public void EndTurn()
		{
			interfaceCanvas.enabled = false;
			TimelineUI.instance.Close();
			CardHand.instance.Discard(-1);
			endTurnBtn.gameObject.SetActive(false);
			aiPlayerObject.SetActive(true);
			//NewTurn();// temp
		}

		public void NewTurn()
		{
			gameState = GameState.Editing;
			interfaceCanvas.enabled = true;
			CardHand.instance.Draw(GetDrawCount());
			aiPlayerObject.SetActive(false);
		}

		private int GetDrawCount()
		{
			return 5;
		}
	}
}