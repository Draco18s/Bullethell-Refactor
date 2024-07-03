using Assets.draco18s.util;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.draco18s.bulletboss
{
	public class GameAssets : MonoBehaviour
	{
		public static GameAssets instance;
		public GameObject cardUIObject => _cardUIObject;

		[SerializeField, FormerlySerializedAs("cardUIObject")] private GameObject _cardUIObject;

		void Awake()
		{
			instance = this;
		}
	}
}
