using System.Collections.Generic;
using System.Linq;
using Assets.draco18s.util;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.draco18s.bulletboss.entities.behavior
{
	public class FighterConfigManager : MonoBehaviour
	{
		public static FighterConfigManager instance;

		[SerializeField] private GameObject _fighterPrefab;
		[SerializeField] private Transform _fighterSpawnPoints;
		[SerializeField] private FighterConfigType[] _fighterTypes;

		private int minFighterLevel = 0;
		private int maxFighterLevel = 0;

		private float spawnInterval = 5;
		private float spawnTimer = 0;

		private List<Transform> spawnPoints;

		[UsedImplicitly]
		void Awake()
		{
			instance = this;
			spawnPoints = new List<Transform>();
			foreach (Transform t in _fighterSpawnPoints)
			{
				spawnPoints.Add(t);
			}
		}

		public void DoUpdate(float dt)
		{
			return;
			spawnTimer -= dt;
			if (spawnTimer <= 0)
			{
				spawnPoints.Shuffle();
				for (int i = 0; i < 5; i++)
					SpawnFighter(spawnPoints.Skip(i).First());
				spawnTimer += spawnInterval;
			}
		}

		private void SpawnFighter(Transform trans)
		{
			int ty = Mathf.FloorToInt(Random.value * (maxFighterLevel - minFighterLevel) + minFighterLevel);
			GameObject go = Instantiate(_fighterPrefab, trans.position, trans.rotation, GameManager.instance.bulletParentContainer);
			go.GetComponent<Fighter>().SetData(_fighterTypes[ty]);
		}
	}
}