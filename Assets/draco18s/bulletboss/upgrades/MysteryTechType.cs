using Assets.draco18s.bulletboss.entities;
using Assets.draco18s.bulletboss.pattern.timeline;
using UnityEngine;

namespace Assets.draco18s.bulletboss.upgrades
{
	[CreateAssetMenu(menuName = "Upgrades/Mystery Tech")]
	public class MysteryTechType : ScriptableObject
	{
		[SerializeField] private string description;
		[SerializeField] private Sprite icon;
		[SerializeField] private NamedRarity rarity;
		[SerializeField] private TimelineModifierType gunModifier;

		public void ApplyItem(Player aiPlayer, PlayerProgress data, int modifier)
		{
			if (gunModifier != null)
			{
				foreach (Transform t in aiPlayer.transform)
				{
					MountPoint m = t.GetComponent<MountPoint>();
					if (m == null) continue;
					if(m.AddModifier(gunModifier)) break;
				}
			}
			switch (name)
			{
				case "Extra Cannon":
					aiPlayer.AddGun(data, modifier - 2);
					break;
			}
		}
	}
}
