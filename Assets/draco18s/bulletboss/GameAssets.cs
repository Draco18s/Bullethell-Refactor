using Assets.draco18s.util;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.draco18s.bulletboss
{
	public class GameAssets : MonoBehaviour
	{
		public static GameAssets instance;
		public static GameObject playableUIObject => instance._playableCardUIPrefab;
		public static GameObject viewyableUIObject => instance._viewCardUIPrefab;
		public static GameObject aiPlayerObject => instance._aiPlayerPrefab;
		public static GameObject defaultBulletPrefab => instance._defaultBulletPrefab;
		public static Material polyChromeMat => instance._polyChromeMat;
		public static Material ephemeralMat => instance._ephemeralMat;
		public static GameObject mountPointPrefab => instance._mountPointPrefab;

		[SerializeField] private GameObject _playableCardUIPrefab;
		[SerializeField] private GameObject _aiPlayerPrefab;
		[SerializeField] private GameObject _viewCardUIPrefab;
		[SerializeField] private GameObject _defaultBulletPrefab;
		[SerializeField] private Material _polyChromeMat;
		[SerializeField] private Material _ephemeralMat;
		[SerializeField] private GameObject _mountPointPrefab;

		void Awake()
		{
			instance = this;
		}
	}
}
