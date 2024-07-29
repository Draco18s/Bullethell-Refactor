using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

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

		public void DoUpdate(float dt)
		{
			spawnTimer -= dt;
			if (spawnTimer <= 0)
			{
				int ty = Mathf.FloorToInt(Random.value*(maxFighterLevel-minFighterLevel) + minFighterLevel);
				int tr = Mathf.FloorToInt(Random.value* _fighterSpawnPoints.childCount);
				Transform trans = _fighterSpawnPoints.GetChild(tr);
				SpawnFighter(_fighterTypes[ty], trans);
				spawnTimer += spawnInterval;
			}
		}

		private void SpawnFighter(FighterConfigType fighterConfig, Transform location)
		{
			GameObject go = Instantiate(_fighterPrefab, location.position, location.rotation, GameManager.instance.bulletParentContainer);
			go.GetComponent<Fighter>().SetData(fighterConfig);
		}
	}
}
