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
		[SerializeField] private float spawnInterval = 5;
		[SerializeField] private float spawnNum = 1;

		[SerializeField] private int minFighterLevel = 0;
		[SerializeField] private int maxFighterLevel = 0;
		private float spawnTimer = 0;
		private int spawnIndex = 0;

		private List<Transform> spawnPoints;

		private PlayerAgent _playerAgent;

		[UsedImplicitly]
		void Awake()
		{
			instance = this;
			spawnPoints = new List<Transform>();
			foreach (Transform t in _fighterSpawnPoints)
			{
				if(t.gameObject.activeSelf)
					spawnPoints.Add(t);
			}
		}

		public void DoUpdate(float dt)
		{
			if (_playerAgent == null)
			{
				_playerAgent = GameObject.FindFirstObjectByType<PlayerAgent>();
				return;
			}

			if (_playerAgent.IsTraining)
			{
				return;
			}

			if (_playerAgent.GetTotalSteps() < 3_000_000)
				return;
			spawnTimer -= dt;
			if (spawnTimer <= 0)
			{
				spawnPoints.Shuffle();
				for (int i = 0; i < spawnNum * (_playerAgent.StepCount > 9_000_000 ? 2 : 1) * (_playerAgent.StepCount > 12_000_000 ? 3 : 1); i++)
					SpawnFighter(spawnPoints.Skip((++spawnIndex) % spawnPoints.Count).First());
				spawnTimer += spawnInterval;
				if (_playerAgent.StepCount < 6_000_000)
					spawnTimer += spawnInterval;
				if (_playerAgent.StepCount > 9_000_000)
					spawnTimer /= 2;
				if (_playerAgent.StepCount > 12_000_000)
					spawnTimer /= 2;
			}
		}

		private void SpawnFighter(Transform trans)
		{
			int ty = Mathf.FloorToInt(Random.value * (maxFighterLevel - minFighterLevel) + minFighterLevel);
			GameObject go = Instantiate(_fighterPrefab, trans.position, trans.rotation, GameManager.instance.bulletParentContainer);
			go.GetComponent<Fighter>().SetData(_fighterTypes[ty]);
		}

		public Transform GetSpawnPoint()
		{
			return spawnPoints.Skip((++spawnIndex) % spawnPoints.Count).First();
		}
	}
}