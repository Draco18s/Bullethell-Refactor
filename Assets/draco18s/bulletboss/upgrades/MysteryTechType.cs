using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.draco18s.bulletboss.upgrades
{
	[CreateAssetMenu(menuName = "Tech Upgrade")]
	public class MysteryTechType : ScriptableObject
	{
		public enum TechUpgradeType
		{
			Hull,
			Armor,
			Shield,
			Damage,
			Weapon,
			Speed
		}

		[SerializeField, FormerlySerializedAs("upgradeType")] private TechUpgradeType _upgradeType;
		[SerializeField, FormerlySerializedAs("upgradeAmount")] private float _upgradeAmount;
		[SerializeField, FormerlySerializedAs("rarity")] private NamedRarity _rarity;

		public TechUpgradeType upgradeType => _upgradeType;
		public float upgradeAmount => _upgradeAmount;
	}
}