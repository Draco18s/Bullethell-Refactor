using UnityEngine;

namespace Assets.draco18s.bulletboss.ui
{
	public class CardHand : MonoBehaviour
	{
		public static CardHand instance;
		
		void Awake()
		{
			instance = this;
		}
	}
}