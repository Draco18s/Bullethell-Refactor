using Assets.draco18s.bulletboss.upgrades;
using UnityEngine;

namespace Assets.draco18s.bulletboss.map
{
	[CreateAssetMenu(menuName = "Map/Mystery Config")]
	public class MysteryTechConfig : ScriptableObject
	{
		[SerializeField] private MysteryTechType[] _techOptions;
		public MysteryTechType[] techOptions => _techOptions;
	}
}
