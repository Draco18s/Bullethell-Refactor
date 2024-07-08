using Assets.draco18s.bulletboss.upgrades;
using UnityEngine;

namespace Assets.draco18s.bulletboss.map
{
	[CreateAssetMenu(menuName = "Map/Basic Tech Config")]
	public class BasicTechConfig : ScriptableObject
	{
		[SerializeField] private BasicTechType[] _techOptions;
		public BasicTechType[] techOptions => _techOptions;
	}
}
