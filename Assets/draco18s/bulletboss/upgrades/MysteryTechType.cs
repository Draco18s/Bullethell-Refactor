using UnityEngine;

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

		[SerializeField] private TechUpgradeType upgradeType;
		[SerializeField] private float upgradeAmount;
		[SerializeField] private NamedRarity rarity;
	}
}