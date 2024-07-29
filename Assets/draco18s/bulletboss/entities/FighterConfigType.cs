using Assets.draco18s.bulletboss.pattern.timeline;
using UnityEngine;

namespace Assets.draco18s.bulletboss.entities
{
	[CreateAssetMenu(menuName = "Fighter Config")]
	public class FighterConfigType : ScriptableObject
	{
		[SerializeField] private Sprite _icon;
		[SerializeField] private int _maxHP;
		[SerializeField] private float _speed;
		[SerializeField] private int _gemReward;
		[SerializeField] private Timeline[] _defaultWeapons;
		public Sprite sprite => _icon;
		public int health => _maxHP;
		public float speed => _speed;
		public int gems => _gemReward;
		public Timeline[] weaponPatterns => _defaultWeapons;
	}
}
