using System.Collections;
using Assets.draco18s.bulletboss.ui;
using UnityEngine;
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

		public GameState gameState { get; protected set; } = GameState.Init;

		void Awake()
		{
			endTurnBtn.onClick.AddListener(EndTurn);
			StartCoroutine(WaitForReady());
			instance = this;
		}

		public IEnumerator WaitForReady()
		{
			yield return new WaitWhile(() => CardHand.instance == null);
			StartNewGame();
		}

		public void StartNewGame()
		{
			gameState = GameState.MainMenu;
			NewTurn();
		}

		public void EndTurn()
		{
			interfaceCanvas.enabled = false;
			TimelineUI.instance.Close();
			CardHand.instance.Discard(-1);
			endTurnBtn.gameObject.SetActive(false);
			aiPlayerObject.SetActive(true);
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