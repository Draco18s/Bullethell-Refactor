using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.draco18s.bulletboss.upgrades
{
	[CreateAssetMenu(menuName = "Upgrades/Basic Tech")]
	public class BasicTechType : ScriptableObject
	{
		public enum EffectedSystem
		{
			Hull,
			Armor,
			Shield,
			Damage,
			Weapon,
			Speed
		}

		[SerializeField] private EffectedSystem _upgradeType;
		[SerializeField] private float _upgradeAmount;

		public EffectedSystem upgradeType => _upgradeType;
		public float upgradeAmount => _upgradeAmount;
	}
}