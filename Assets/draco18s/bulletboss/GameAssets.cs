﻿using Assets.draco18s.util;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.draco18s.bulletboss
{
	public class GameAssets : MonoBehaviour
	{
		public static GameAssets instance;
		public GameObject playableUIObject => _playableCardUIPrefab;
		public GameObject viewyableUIObject => _viewCardUIPrefab;
		public GameObject aiPlayerObject => _aiPlayerPrefab;

		[SerializeField] private GameObject _playableCardUIPrefab;
		[SerializeField] private GameObject _aiPlayerPrefab;
		[SerializeField] private GameObject _viewCardUIPrefab;

		void Awake()
		{
			instance = this;
		}
	}
}
