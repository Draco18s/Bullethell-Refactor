using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.draco18s.bulletboss.pattern.timeline;
using UnityEngine;

namespace Assets.draco18s.bulletboss.entities
{
	public class Player : MonoBehaviour
	{
		[SerializeField] private int baseHP;
		[SerializeField] private GameObject mountPointPrefab;

		public int currentHP { get; protected set; }
		public int maximumHP { get; protected set; }
		public int collectedGems { get; protected set; } = 0;
		
		void OnTriggerEnter2D(Collider2D other)
		{
			if (other.gameObject.layer == LayerMask.NameToLayer("Powerups"))
			{
				
			}
			if (other.gameObject.layer == LayerMask.NameToLayer("EnemyBullets"))
			{
				currentHP--;
				if (currentHP <= 0)
				{
					GameManager.instance.CheckGameOver(this);
					this.gameObject.SetActive(false);
				}
			}
		}
	}
}
