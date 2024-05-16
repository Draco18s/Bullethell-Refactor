using Assets.draco18s.bulletboss.ui;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.draco18s.bulletboss
{
	public class GameManager : MonoBehaviour
	{
		[SerializeField] private Button endTurnBtn;

		void Awake()
		{
			endTurnBtn.onClick.AddListener(EndTurn);
		}

		public void EndTurn()
		{
			TimelineUI.instance.Close();
			CardHand.instance.Discard(-1);
			endTurnBtn.gameObject.SetActive(false);
		}

		public void NewTurn()
		{
			CardHand.instance.Draw(GetDrawCount());
		}

		private int GetDrawCount()
		{
			return 5;
		}
	}
}