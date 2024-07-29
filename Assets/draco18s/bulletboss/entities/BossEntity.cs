using JetBrains.Annotations;
using UnityEngine;

namespace Assets.draco18s.bulletboss.entities
{
	public class BossEntity : MonoBehaviour
	{
		[SerializeField] private int baseHP;
		private int currentSegment = 0;
		public int[] maximumHP { get; protected set; } = { 500, 500, 500 };
		public int[] currentHP { get; protected set; } = { 500, 500, 500 };

		[UsedImplicitly]
		void OnTriggerEnter2D(Collider2D other)
		{
			if (other.gameObject.layer == LayerMask.NameToLayer("PlayerBullets"))
			{
				currentHP[currentSegment] -= other.GetComponent<Bullet>().Damage;
				if (currentHP[currentSegment] <= 0)
				{
					currentSegment++;
					GameManager.instance.NewTurn();
					Debug.Log("Boss HP depleted");
				}
			}
		}

		public int GetCurrentSegmentHP()
		{
			return currentHP[currentSegment];
		}
	}
}
