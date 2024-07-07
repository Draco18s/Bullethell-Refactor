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
		public int currentShield { get; protected set; }
		public int maximumShield { get; protected set; }
		public int armor { get; protected set; }
		public int collectedGems { get; protected set; } = 0;

		private int minDamage = 1;
		
		void OnTriggerEnter2D(Collider2D other)
		{
			if (other.gameObject.layer == LayerMask.NameToLayer("Powerups"))
			{
				// todo: Gem value
				collectedGems += 1;
			}
			if (other.gameObject.layer == LayerMask.NameToLayer("EnemyBullets"))
			{
				var dmg = other.GetComponent<Bullet>().Damage;
				dmg = Mathf.Max(dmg - armor, minDamage);
				minDamage = 1 - minDamage;
				currentHP -= dmg;

				if (currentHP > 0) return;
				GameManager.instance.CheckGameOver(this);
				gameObject.SetActive(false);
			}
		}

		public void SetStats(PlayerProgress data, int mod)
		{
			currentHP = maximumHP = baseHP + (int)Mathf.Max(data.hullLevel+mod,0);
			currentShield = maximumShield = baseHP + (int)Mathf.Max(data.shieldLevel+mod,0);
			armor = (int)Mathf.Max(data.armorLevel + mod, 0);
			
			GameObject mount = Instantiate(mountPointPrefab, transform);
			mount.transform.localPosition = Vector3.zero;
			Bullet b = mount.GetComponent<Bullet>();
			b.Damage = (int)Mathf.Max(data.damageLevel + mod, 1);
		}
	}
}
