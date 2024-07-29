using Assets.draco18s.bulletboss.upgrades;
using UnityEngine;

namespace Assets.draco18s.bulletboss.map
{
	[CreateAssetMenu(menuName = "Map/Mystery Tech Config")]
	public class MysteryTechConfig : ScriptableObject
	{
		[SerializeField] private MysteryTechType[] _techOptions;
		public MysteryTechType[] techOptions => _techOptions;
	}
}