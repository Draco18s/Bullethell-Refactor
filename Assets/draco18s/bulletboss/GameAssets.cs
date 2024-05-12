using UnityEngine;

namespace Assets.draco18s.bulletboss
{
	public class GameAssets : MonoBehaviour
	{
		public static GameAssets instance;

		public GameObject cardUIObject;

		void Awake()
		{
			instance = this;
		}
	}
}
