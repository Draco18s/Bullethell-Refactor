using Assets.draco18s.bulletboss.upgrades;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.draco18s.bulletboss.entities
{
	public class Player : MonoBehaviour, IHasSpeed
	{
		[SerializeField] private int baseHP;
		[SerializeField] private float _speed;

		public int currentHP { get; protected set; }
		public int maximumHP { get; protected set; }
		public int currentShield { get; protected set; }
		public int maximumShield { get; protected set; }
		public int armor { get; protected set; }
		public int collectedGems { get; protected set; } = 0;

		public float Speed => _speed;

		private int minDamage = 1;
		public bool isMoving = false;

		/*[UsedImplicitly]
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
				Destroy(other.gameObject);
				if (currentHP > 0) return;
				GameManager.instance.CheckGameOver(this);
				gameObject.SetActive(false);
			}
		}*/

		public void SetStats(PlayerProgress data, int mod)
		{
			currentHP = maximumHP = baseHP + (int)Mathf.Max(data.hullLevel+mod,0);
			currentShield = maximumShield = baseHP + (int)Mathf.Max(data.shieldLevel+mod,0);
			armor = (int)Mathf.Max(data.armorLevel + mod, 0);

			foreach (MysteryTechType d in data.advancedTech)
			{
				d.ApplyItem(this, data, mod);
			}
		}

		public void AddGun(PlayerProgress data, int mod)
		{
			GameObject mount = Instantiate(GameAssets.mountPointPrefab, transform);
			mount.transform.localPosition = Vector3.zero;
			MountPoint b = mount.GetComponent<MountPoint>();
			b.Damage = (int)Mathf.Max(data.damageLevel + mod/2f, 1);
		}

		public void SpecialSetup(float rot, float spd) { }
	}
}
