using Assets.draco18s.util;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.draco18s.bulletboss
{
	public class GameAssets : MonoBehaviour
	{
		public static GameAssets instance;
		public GameObject playableUIObject => _playableCardUIPrefab;

		[SerializeField, FormerlySerializedAs("_cardUIObject")] private GameObject _playableCardUIPrefab;
		[SerializeField] private GameObject _viewCardUIPrefab;

		void Awake()
		{
			instance = this;
		}
	}
}
